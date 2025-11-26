using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Implement
{
    public class OrderManagementService(JPDbContext jPDbContext, SPDbContext sPDbContext, IPISService pISService, IReceiveManagementService receiveManagementService) : IOrderManagementService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IPISService _pISService = pISService;
        private readonly IReceiveManagementService _receiveManagementService = receiveManagementService;

        public async Task<PagedScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string lotNo, string custCode, DateTime fromDate, DateTime toDate, int page, int pageSize)
        {
            var today = DateTime.Now.Date;

            // 1) Base query
            var ordersQuery = _sPDbContext.Order.Where(o => o.IsActive && !o.IsSuccess);

            if (!string.IsNullOrEmpty(orderNo))
            ordersQuery = ordersQuery.Where(o => o.OrderNo.Contains(orderNo));

            if (!string.IsNullOrEmpty(lotNo))
            {
                ordersQuery = from ord in ordersQuery
                              join lt in _sPDbContext.Lot on ord.OrderNo equals lt.OrderNo
                              where lt.IsActive && !lt.IsSuccess && lt.LotNo.Contains(lotNo)
                              select ord;
            }

            if (!string.IsNullOrEmpty(custCode))
                ordersQuery = ordersQuery.Where(o => o.CustCode!.Contains(custCode));

            if (fromDate != DateTime.MinValue)
                ordersQuery = ordersQuery.Where(o => o.PackStartDate >= fromDate);

            if (toDate != DateTime.MinValue)
                ordersQuery = ordersQuery.Where(o => o.PackStartDate <= toDate);

            // 2) Count
            int totalItems = await ordersQuery.CountAsync();

            // 3) Apply paging
            var orders = await ordersQuery
                .OrderByDescending(o => o.FactoryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4) Preload child data only for paged orders
            var orderNos = orders.Select(o => o.OrderNo).ToList();

            var orderNotifies = await _sPDbContext.OrderNotify
                .Where(x => x.IsActive && orderNos.Contains(x.OrderNo))
                .ToDictionaryAsync(x => x.OrderNo, x => x);

            var lotsQuery = _sPDbContext.Lot
                .Where(l => l.IsActive && !l.IsSuccess && orderNos.Contains(l.OrderNo));

            //if (!string.IsNullOrEmpty(lotNo))
            //{
            //    lotsQuery = lotsQuery.Where(l => l.LotNo.Contains(lotNo));
            //    orders = orders.Where(o => lotsQuery.Any(l => l.OrderNo == o.OrderNo)).ToList();
            //}

            var lots = await lotsQuery
                .GroupBy(l => l.OrderNo)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var lotNos = lots.SelectMany(g => g.Value.Select(l => l.LotNo)).ToList();

            var assignedTables = await (
                from ass in _sPDbContext.Assignment
                join asmr in _sPDbContext.AssignmentReceived on ass.AssignmentId equals asmr.AssignmentId
                join asnt in _sPDbContext.AssignmentTable on asmr.AssignmentReceivedId equals asnt.AssignmentReceivedId
                join wt in _sPDbContext.WorkTable on asnt.WorkTableId equals wt.Id
                join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId
                where lotNos.Contains(rev.LotNo) && asmr.IsActive && ass.IsActive && asnt.IsActive && wt.IsActive
                select new { rev.LotNo, ass.AssignmentId, wt.Name }
            ).ToListAsync();

            var assignedTableDict = assignedTables
                .GroupBy(a => a.LotNo)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new AssignedWorkTableModel
                    {
                        AssignmentId = x.AssignmentId,
                        TableName = x.Name!
                    }).Distinct().ToList()
                );

            var lotNotifies = await _sPDbContext.LotNotify
                .Where(l => lotNos.Contains(l.LotNo) && l.IsActive)
                .ToDictionaryAsync(l => l.LotNo, l => l);

            var repairs = await (
                from bek in _sPDbContext.Break
                join rev in _sPDbContext.Received on bek.ReceivedId equals rev.ReceivedId
                join lot in _sPDbContext.Lot on rev.LotNo equals lot.LotNo
                where lotNos.Contains(lot.LotNo)
                select lot.LotNo
            ).Distinct().ToListAsync();

            var losts = await (
                from los in _sPDbContext.Lost
                join lot in _sPDbContext.Lot on los.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                where lotNos.Contains(lot.LotNo)
                select lot.LotNo
            ).Distinct().ToListAsync();

            var notAllAssignedLots = await _sPDbContext.Received
                .Where(r => lotNos.Contains(r.LotNo) && !r.IsAssigned)
                .Select(r => r.LotNo)
                .Distinct()
                .ToListAsync();


            // 5) Compose result
            var result = new List<CustomOrder>();

            foreach (var order in orders)
            {
                orderNotifies.TryGetValue(order.OrderNo, out var notify);
                lots.TryGetValue(order.OrderNo, out var relatedLots);
                relatedLots ??= [];

                var operateDays = relatedLots.Sum(l => l.OperateDays ?? 0);

                var packDate = order.OrderDate?.Date ?? DateTime.MinValue;
                var exportDate = order.SeldDate1?.Date ?? DateTime.MinValue;

                var packDaysRemain = (packDate - today).Days;
                var exportDaysRemain = (exportDate - today).Days;

                var customLots = relatedLots.Select(l =>
                {
                    assignedTableDict.TryGetValue(l.LotNo, out var atables);
                    lotNotifies.TryGetValue(l.LotNo, out var lotNotify);

                    bool isLotUpdate = lotNotify?.IsUpdate ?? false;
                    bool isAllReturned = l.TtQty > 0 && l.ReturnedQty >= l.TtQty;
                    bool isAllReceived = l.TtQty > 0 && l.ReceivedQty >= l.TtQty;
                    bool isPacking = l.TtQty > 0 && l.AssignedQty > 0;
                    bool isAllAssigned = !notAllAssignedLots.Contains(l.LotNo);
                    bool hasRepair = repairs.Contains(l.LotNo);
                    bool hasLost = losts.Contains(l.LotNo);

                    return new CustomLot
                    {
                        LotNo = l.LotNo,
                        OrderNo = l.OrderNo,
                        ListNo = l.ListNo!,
                        CustPcode = l.CustPcode!,
                        TtQty = l.TtQty ?? 0,
                        Article = l.Article!,
                        Barcode = l.Barcode!,
                        TdesArt = l.TdesArt!,
                        MarkCenter = l.MarkCenter!,
                        SaleRem = l.SaleRem!,
                        IsSuccess = l.IsSuccess,
                        IsActive = l.IsActive,
                        IsUpdate = isLotUpdate,
                        UpdateDate = l.UpdateDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                        AssignTo = atables ?? [],
                        IsPacking = isPacking,
                        IsAllAssigned = isAllAssigned,
                        ReceivedQty = l.ReceivedQty ?? 0,
                        ReturnedQty = l.ReturnedQty ?? 0,
                        IsAllReceived = isAllReceived,
                        HasRepair = hasRepair,
                        HasLost = hasLost,
                        IsAllReturned = isAllReturned
                    };
                }).ToList();

                result.Add(new CustomOrder
                {
                    OrderNo = order.OrderNo,
                    CustCode = order.CustCode ?? string.Empty,
                    FactoryDate = order.FactoryDate ?? DateTime.MinValue,
                    FactoryDateTH = order.FactoryDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                    OrderDate = order.OrderDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                    SeldDate1 = order.SeldDate1?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                    OrdDate = order.OrdDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                    TotalLot = relatedLots.Count,
                    SumTtQty = (int)relatedLots.Sum(l => l.TtQty ?? 0),
                    CompleteLot = relatedLots.Count(l => l.IsSuccess),
                    OperateDays = operateDays,
                    StartDate = order.PackStartDate ?? DateTime.MinValue,
                    StartDateTH = order.PackStartDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? string.Empty,
                    PackDaysRemain = packDaysRemain,
                    ExportDaysRemain = exportDaysRemain,
                    IsUpdate = notify?.IsUpdate ?? false,
                    IsNew = notify?.IsNew ?? false,
                    IsActive = order.IsActive,
                    IsSuccess = order.IsSuccess,
                    IsReceivedLate = packDaysRemain <= 1 && !order.IsSuccess,
                    IsPackingLate = exportDaysRemain <= 1 && !order.IsSuccess,
                    CustomLot = customLots
                });
            }

            // 6) Group result
            var schedule = new ScheduleListModel();

            if (groupMode == GroupMode.Day)
            {
                schedule.Days = result
                    .GroupBy(o => o.FactoryDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new Day
                    {
                        Title = g.Key == today ? "Today" : g.Key.ToString("ddd dd MMM yyyy"),
                        Orders = g.ToList()
                    }).ToList();
            }
            else
            {
                schedule.Weeks = result
                    .GroupBy(o => o.FactoryDate.StartOfWeek())
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var start = g.Key;
                        var end = start.AddDays(6);
                        var title = (start <= today && today <= end)
                            ? "This Week"
                            : $"{start:dd MMM} – {end:dd MMM yyyy}";

                        var days = g
                            .GroupBy(x => x.FactoryDate.Date)
                            .OrderBy(d => d.Key)
                            .Select(d => new Day
                            {
                                Title = d.Key == today ? "Today" : d.Key.ToString("ddd dd MMM"),
                                Orders = d.ToList()
                            }).ToList();

                        return new Week
                        {
                            Title = title,
                            Orders = days
                        };
                    }).ToList();
            }

            // 7) Return paged result
            return new PagedScheduleListModel
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Data = schedule
            };
        }

        public async Task<ScheduleListModel> GetOrderAndLotByRangeAsync2(GroupMode groupMode, string orderNo, string custCode, DateTime fromDate, DateTime toDate)
        {
            var today = DateTime.Now.Date;

            // 1) Base query: filter ตั้งแต่แรก
            var ordersQuery = _sPDbContext.Order.Where(o => o.IsActive && !o.IsSuccess);

            if (!string.IsNullOrEmpty(orderNo))
                ordersQuery = ordersQuery.Where(o => o.OrderNo.Contains(orderNo));

            if (!string.IsNullOrEmpty(custCode))
                ordersQuery = ordersQuery.Where(o => o.CustCode!.Contains(custCode));

            if (fromDate != DateTime.MinValue)
                ordersQuery = ordersQuery.Where(o => o.PackStartDate >= fromDate);

            if (toDate != DateTime.MinValue)
                ordersQuery = ordersQuery.Where(o => o.PackStartDate <= toDate);

            var orders = await ordersQuery.ToListAsync();

            // 2) Preload dictionaries
            var orderNos = orders.Select(o => o.OrderNo).ToList();

            var orderNotifies = await _sPDbContext.OrderNotify
                .Where(x => x.IsActive && orderNos.Contains(x.OrderNo))
                .ToDictionaryAsync(x => x.OrderNo, x => x);

            var lots = await _sPDbContext.Lot
                .Where(l => l.IsActive && !l.IsSuccess && orderNos.Contains(l.OrderNo))
                .GroupBy(l => l.OrderNo)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var lotNos = lots.SelectMany(g => g.Value.Select(l => l.LotNo)).ToList();

            // Assigned table preload
            var assignedTables = await (
                from ass in _sPDbContext.Assignment
                join asmr in _sPDbContext.AssignmentReceived on ass.AssignmentId equals asmr.AssignmentId
                join asnt in _sPDbContext.AssignmentTable on asmr.AssignmentReceivedId equals asnt.AssignmentReceivedId
                join wt in _sPDbContext.WorkTable on asnt.WorkTableId equals wt.Id
                join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId
                where lotNos.Contains(rev.LotNo) && asmr.IsActive && ass.IsActive && asnt.IsActive && wt.IsActive
                select new { rev.LotNo, ass.AssignmentId, wt.Name }
            ).ToListAsync();

            var assignedTableDict = assignedTables
                .GroupBy(a => a.LotNo)
                .ToDictionary(g => g.Key,
                    g => g.Select(x => new AssignedWorkTableModel
                    {
                        AssignmentId = x.AssignmentId,
                        TableName = x.Name!
                    }).Distinct().ToList());

            // LotNotify preload
            var lotNotifies = await _sPDbContext.LotNotify
                .Where(l => lotNos.Contains(l.LotNo) && l.IsActive)
                .ToDictionaryAsync(l => l.LotNo, l => l);

            var repairs = await (
                from bek in _sPDbContext.Break
                join rev in _sPDbContext.Received on bek.ReceivedId equals rev.ReceivedId
                join lot in _sPDbContext.Lot on rev.LotNo equals lot.LotNo
                where lotNos.Contains(lot.LotNo)
                select lot.LotNo
            ).Distinct().ToListAsync();

            var losts = await (
                from los in _sPDbContext.Lost
                join lot in _sPDbContext.Lot on los.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                where lotNos.Contains(lot.LotNo)
                select lot.LotNo
            ).Distinct().ToListAsync();

            var notAllAssignedLots = await _sPDbContext.Received
                .Where(r => lotNos.Contains(r.LotNo) && !r.IsAssigned)
                .Select(r => r.LotNo)
                .Distinct()
                .ToListAsync();

            // 3) Compose result
            var result = new List<CustomOrder>();

            foreach (var order in orders)
            {
                orderNotifies.TryGetValue(order.OrderNo, out var notify);
                lots.TryGetValue(order.OrderNo, out var relatedLots);
                relatedLots ??= [];

                var operateDays = relatedLots.Sum(l => l.OperateDays ?? 0);

                var packDate = order.OrderDate?.Date ?? DateTime.MinValue;
                var exportDate = order.SeldDate1?.Date ?? DateTime.MinValue;

                var packDaysRemain = (packDate - today).Days;
                var exportDaysRemain = (exportDate - today).Days;

                var customLots = relatedLots.Select(l =>
                {
                    assignedTableDict.TryGetValue(l.LotNo, out var atables);
                    lotNotifies.TryGetValue(l.LotNo, out var lotNotify);

                    bool isLotUpdate = lotNotify?.IsUpdate ?? false;
                    bool isAllReturned = l.TtQty > 0 && l.ReturnedQty >= l.TtQty;
                    bool isAllReceived = l.TtQty > 0 && l.ReceivedQty >= l.TtQty;
                    bool isPacking = l.TtQty > 0 && l.AssignedQty > 0;
                    bool isAllAssigned = !notAllAssignedLots.Contains(l.LotNo);
                    bool hasRepair = repairs.Contains(l.LotNo);
                    bool hasLost = losts.Contains(l.LotNo);

                    return new CustomLot
                    {
                        LotNo = l.LotNo,
                        OrderNo = l.OrderNo,
                        ListNo = l.ListNo!,
                        CustPcode = l.CustPcode!,
                        TtQty = l.TtQty ?? 0,
                        Article = l.Article!,
                        Barcode = l.Barcode!,
                        TdesArt = l.TdesArt!,
                        MarkCenter = l.MarkCenter!,
                        SaleRem = l.SaleRem!,
                        IsSuccess = l.IsSuccess,
                        IsActive = l.IsActive,
                        IsUpdate = isLotUpdate,
                        UpdateDate = l.UpdateDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        AssignTo = atables ?? [],
                        IsPacking = isPacking,
                        IsAllAssigned = isAllAssigned,
                        ReceivedQty = l.ReceivedQty ?? 0,
                        ReturnedQty = l.ReturnedQty ?? 0,
                        IsAllReceived = isAllReceived,
                        HasRepair = hasRepair,
                        HasLost = hasLost,
                        IsAllReturned = isAllReturned
                    };
                }).ToList();

                result.Add(new CustomOrder
                {
                    OrderNo = order.OrderNo,
                    CustCode = order.CustCode ?? "",
                    FactoryDate = order.FactoryDate ?? DateTime.MinValue,
                    FactoryDateTH = order.FactoryDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    OrderDate = order.OrderDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    SeldDate1 = order.SeldDate1?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    OrdDate = order.OrdDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    TotalLot = relatedLots.Count,
                    SumTtQty = (int)relatedLots.Sum(l => l.TtQty ?? 0),
                    CompleteLot = relatedLots.Count(l => l.IsSuccess),
                    OperateDays = operateDays,
                    StartDate = order.PackStartDate ?? DateTime.MinValue,
                    StartDateTH = order.PackStartDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    PackDaysRemain = packDaysRemain,
                    ExportDaysRemain = exportDaysRemain,
                    IsUpdate = notify?.IsUpdate ?? false,
                    IsNew = notify?.IsNew ?? false,
                    IsActive = order.IsActive,
                    IsSuccess = order.IsSuccess,
                    IsReceivedLate = packDaysRemain <= 1 && !order.IsSuccess,
                    IsPackingLate = exportDaysRemain <= 1 && !order.IsSuccess,
                    CustomLot = customLots
                });
            }

            // 4) Grouping
            var schedule = new ScheduleListModel();
            if (groupMode == GroupMode.Day)
            {
                schedule.Days = [.. result
                    .GroupBy(o => o.FactoryDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new Day
                    {
                        Title = g.Key == today ? "Today" : g.Key.ToString("ddd dd MMM yyyy"),
                        Orders = g.ToList()
                    })];
            }
            else
            {
                schedule.Weeks = [.. result
                    .GroupBy(o => o.FactoryDate.StartOfWeek())
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var start = g.Key;
                        var end = start.AddDays(6);
                        var title = (start <= today && today <= end)
                            ? "This Week"
                            : $"{start:dd MMM} – {end:dd MMM yyyy}";

                        var days = g
                            .GroupBy(x => x.FactoryDate.Date)
                            .OrderBy(d => d.Key)
                            .Select(d => new Day
                            {
                                Title = d.Key == today ? "Today" : d.Key.ToString("ddd dd MMM"),
                                Orders = d.ToList()
                            }).ToList();

                        return new Week
                        {
                            Title = title,
                            Orders = days
                        };
                    })];
            }

            return schedule;
        }

        public async Task<CustomLot?> GetCustomLotAsync(string lotNo)
        {
            var lot = await _sPDbContext.Lot
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LotNo == lotNo && l.IsActive);

            if (lot == null)
                return null;

            // Assigned tables
            var assignedTable = await (
                from rec in _sPDbContext.Received
                join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                join at in _sPDbContext.AssignmentTable on ass.AssignmentReceivedId equals at.AssignmentReceivedId
                join wt in _sPDbContext.WorkTable on at.WorkTableId equals wt.Id
                where rec.LotNo == lot.LotNo && rec.IsActive && ass.IsActive && at.IsActive && wt.IsActive
                select new AssignedWorkTableModel
                {
                    AssignmentId = ass.AssignmentId,
                    TableName = wt.Name!
                }
            ).Distinct().ToListAsync();

            // LotNotify
            var isLotUpdate = await _sPDbContext.LotNotify
                .AsNoTracking()
                .Where(w => w.LotNo == lot.LotNo && w.IsActive)
                .Select(x => x.IsUpdate)
                .FirstOrDefaultAsync();

            // Check not all assigned
            var hasNotAllAssigned = await _sPDbContext.Received
                .AsNoTracking()
                .Where(r => r.LotNo == lot.LotNo && !r.IsAssigned)
                .AnyAsync();
            var isAllAssigned = !hasNotAllAssigned;

            // HasRepair
            //var hasRepair = await (
            //    from rec in _sPDbContext.Received
            //    join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
            //    join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
            //    join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
            //    where rec.LotNo == lot.LotNo && rec.IsSendRepair && ret.HasRepair
            //    select 1
            //).AnyAsync();

            //// HasLost
            //var hasLost = await (
            //    from rec in _sPDbContext.Received
            //    join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
            //    join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
            //    join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
            //    where rec.LotNo == lot.LotNo && ret.HasLost
            //    select 1
            //).AnyAsync();

            var notAllAssignedLots = await _sPDbContext.Received
                .Where(r => lotNo.Contains(r.LotNo) && !r.IsAssigned)
                .Select(r => r.LotNo)
                .Distinct()
                .ToListAsync();

            // คำนวณสถานะ
            var ttQty = lot.TtQty.GetValueOrDefault();
            var returnedQty = lot.ReturnedQty.GetValueOrDefault();
            var receivedQty = lot.ReceivedQty.GetValueOrDefault();
            var assignedQty = lot.AssignedQty.GetValueOrDefault();

            bool isAllReturned = ttQty > 0 && returnedQty >= ttQty;
            bool isAllReceived = ttQty > 0 && receivedQty >= ttQty;
            bool isPacking = ttQty > 0 && assignedQty > 0;

            return new CustomLot
            {
                LotNo = lot.LotNo,
                OrderNo = lot.OrderNo,
                ListNo = lot.ListNo!,
                CustPcode = lot.CustPcode!,
                TtQty = ttQty,
                Article = lot.Article!,
                Barcode = lot.Barcode!,
                TdesArt = lot.TdesArt!,
                MarkCenter = lot.MarkCenter!,
                SaleRem = lot.SaleRem!,
                IsSuccess = lot.IsSuccess,
                IsActive = lot.IsActive,
                IsUpdate = isLotUpdate,
                UpdateDate = lot.UpdateDate?.ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                AssignTo = assignedTable,

                IsPacking = isPacking,
                IsAllAssigned = isAllAssigned,

                ReceivedQty = receivedQty,
                ReturnedQty = returnedQty,

                IsAllReceived = isAllReceived,
                HasRepair = false,
                HasLost = false,
                IsAllReturned = isAllReturned,
            };
        }

        public async Task UpdateAllReceivedItemsAsync(string receiveNo)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var allReceived = await (
                    from a in _jPDbContext.Spdreceive
                    join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                    join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                    where a.ReceiveNo == receiveNo
                    select new
                    {
                        a.ReceiveNo,
                        a.Lotno,
                        a.Ttqty,
                        a.Ttwg,
                        a.Barcode,
                        a.Billnumber,
                        a.Mdate,
                        c.ListNo,
                        d.OrderNo
                    }
                ).ToListAsync();

                if (allReceived.Count == 0) throw new InvalidOperationException("ไม่พบใบรับ"); ;

                var validOrderNos = await _sPDbContext.Order
                    .Select(x => x.OrderNo)
                    .ToHashSetAsync();

                var filtered = allReceived
                    .Where(x => validOrderNos.Contains(x.OrderNo))
                    .ToList();

                if (filtered.Count == 0) throw new InvalidOperationException("ไม่มีออเดอร์ที่สามารถนำเข้าได้");

                var existingKeys = await _sPDbContext.Received
                    .Where(x => x.ReceiveNo == receiveNo)
                    .Select(x => new { x.ReceiveNo, x.LotNo, x.Barcode })
                    .ToHashSetAsync();

                var toInsert = filtered
                    .Where(x => !existingKeys.Contains(new { x.ReceiveNo, LotNo = x.Lotno, x.Barcode }))
                    .Select(x => new Received
                    {
                        ReceiveNo = x.ReceiveNo,
                        LotNo = x.Lotno,
                        TtQty = x.Ttqty,
                        TtWg = (double)x.Ttwg,
                        Barcode = x.Barcode,
                        BillNumber = x.Billnumber,
                        Mdate = x.Mdate,
                        IsReceived = true,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    })
                    .ToList();

                if (toInsert.Count == 0)
                    throw new InvalidOperationException("ไม่มีรายการที่สามารถนำเข้าได้");

                await _sPDbContext.Received.AddRangeAsync(toInsert);

                var lotSums = toInsert
                    .GroupBy(x => x.LotNo)
                    .Select(g => new
                    {
                        LotNo = g.Key,
                        SumQty = g.Sum(r => r.TtQty ?? 0m)
                    })
                    .ToList();

                var lotNos = lotSums.Select(s => s.LotNo).ToList();
                var lots = await _sPDbContext.Lot
                    .Where(l => lotNos.Contains(l.LotNo))
                    .ToListAsync();

                var now = DateTime.Now;
                foreach (var s in lotSums)
                {
                    var lot = lots.FirstOrDefault(l => l.LotNo == s.LotNo);
                    if (lot == null) continue;

                    lot.ReceivedQty = (lot.ReceivedQty ?? 0m) + s.SumQty;
                    lot.UpdateDate = now;
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReturnReceivedAsync(string lotNo, int[] assignmentIDs, decimal returnQty)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(o => o.LotNo == lotNo && o.IsActive) ?? throw new InvalidOperationException($"Lot '{lotNo}' not found or inactive.");

                var returned = new Returned
                {
                    ReturnTtQty = returnQty,
                    IsSuccess = true,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _sPDbContext.Returned.Add(returned);
                await _sPDbContext.SaveChangesAsync();

                var details = new List<ReturnedDetail>();

                if (assignmentIDs.Length > 0)
                {
                    var receivedList = await (
                        from a in _sPDbContext.AssignmentReceived
                        join r in _sPDbContext.Received on a.ReceivedId equals r.ReceivedId
                        where assignmentIDs.Contains(a.AssignmentId) && a.IsActive && r.LotNo == lotNo
                        select new { a.AssignmentId, Received = r }
                    ).ToListAsync();

                    foreach (var item in receivedList)
                    {
                        item.Received.IsReturned = true;
                        item.Received.UpdateDate = DateTime.Now;

                        details.Add(new ReturnedDetail
                        {
                            ReturnId = returned.ReturnId,
                            AssignmentId = item.AssignmentId,
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        });
                    }

                    _sPDbContext.ReturnedDetail.AddRange(details);
                }

                lot.ReturnedQty = (lot.ReturnedQty ?? 0) + returnQty;
                lot.UpdateDate = DateTime.Now;

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo)
        {
            var result = await (from a in _sPDbContext.Received
                                join b in _sPDbContext.Lot on a.LotNo equals b.LotNo
                                where a.LotNo == lotNo && !a.IsReceived
                                select new ReceivedListModel
                                {
                                    ReceivedID = a.ReceivedId,
                                    ReceiveNo = a.ReceiveNo,
                                    LotNo = a.LotNo,
                                    ListNo = b.ListNo!,
                                    OrderNo = b.OrderNo,
                                    Article = b.Article!,
                                    Barcode = a.Barcode!,
                                    CustPCode = b.CustPcode ?? string.Empty,
                                    TtQty = a.TtQty.GetValueOrDefault(),
                                    TtWg = a.TtWg.GetValueOrDefault(),

                                }).ToListAsync();

            return result;
        }

        public async Task<List<WorkTable>> GetTableAsync()
        {
            var tables = await _sPDbContext.WorkTable
                .Where(x => x.IsActive)
                .Select(x => new WorkTable
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .ToListAsync();

            return [.. tables];
        }

        public async Task<List<TableModel>> GetTableToReturnAsync(string LotNo)
        {
            var result = await
                (from asmt in _sPDbContext.AssignmentTable
                 join asmr in _sPDbContext.AssignmentReceived on asmt.AssignmentReceivedId equals asmr.AssignmentReceivedId into asmJoin
                 from asmr in asmJoin.DefaultIfEmpty()
                 join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId into revJoin
                 from rev in revJoin.DefaultIfEmpty()
                 join wt in _sPDbContext.WorkTable on asmt.WorkTableId equals wt.Id into wtJoin
                 from wt in wtJoin.DefaultIfEmpty()
                 where asmr.IsActive == true && rev.LotNo == LotNo && !rev.IsReturned
                 select new TableModel { Id = wt.Id, Name = wt.Name! }
                ).Distinct().ToListAsync();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetRecievedToReturnAsync(string LotNo, int TableID)
        {
            var result = await (from wt in _sPDbContext.WorkTable
                                join asmt in _sPDbContext.AssignmentTable on wt.Id equals asmt.WorkTableId into asmtJoin
                                from asmt in asmtJoin.DefaultIfEmpty()
                                join asmr in _sPDbContext.AssignmentReceived on asmt.AssignmentReceivedId equals asmr.AssignmentReceivedId into asmJoin
                                from asmr in asmJoin.DefaultIfEmpty()
                                join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId into revJoin
                                from rev in revJoin.DefaultIfEmpty()
                                join lot in _sPDbContext.Lot on rev.LotNo equals lot.LotNo into bJoin
                                from lot in bJoin.DefaultIfEmpty()
                                where asmt != null && asmt.IsActive == true && wt.Id == TableID && rev != null && rev.LotNo == LotNo && !rev.IsReturned
                                select new ReceivedListModel
                                {
                                    ReceivedID = rev.ReceivedId,
                                    ReceiveNo = rev.ReceiveNo,
                                    LotNo = rev.LotNo,
                                    ListNo = lot.ListNo!,
                                    OrderNo = lot.OrderNo,
                                    Article = lot.Article!,
                                    Barcode = rev.Barcode!,
                                    CustPCode = lot.CustPcode ?? string.Empty,
                                    TtQty = rev.TtQty.GetValueOrDefault(),
                                    TtWg = rev.TtWg.GetValueOrDefault(),
                                    AssignmentID = asmr.AssignmentId
                                }).ToListAsync();

            return result;
        }

        public async Task<UserModel> ValidateApporverAsync(string username, string password)
        {
            var user = await _pISService.ValidateApproverAsync(username, password);

            var HasRole = await _sPDbContext.MappingPermission.AnyAsync(x => x.UserId == user.UserID && x.IsActive && x.PermissionId == 3);

            if (user != null && HasRole)
            {
                return user;
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        public async Task ImportOrderAsync()
        {
            List<OrderNotify> orderNotifies = [];
            List<LotNotify> lotNotifies = [];

            var newLots = new List<Lot>();

            var newOrders = await GetJPOrderAsync();

            var newOrderNos = newOrders.Select(s => s.OrderNo).Distinct().ToList();

            var existingOrderNos = await _sPDbContext.Order.Where(o => o.IsActive && o.FactoryDate!.Value.Year == DateTime.Now.Year).Select(o => o.OrderNo).ToListAsync();

            var notExistOrderNos = newOrderNos.Except(existingOrderNos).ToList();


            if (newOrders.Count != 0)
            {
                newLots = await _receiveManagementService.GetJPLotAsync(newOrders);

                if (newLots.Count == 0) return;

                foreach (var order in newOrders)
                {
                    orderNotifies.Add(new OrderNotify
                    {
                        OrderNo = order.OrderNo,
                        IsNew = true,
                        IsUpdate = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    });
                }

                foreach (var lot in newLots)
                {
                    lotNotifies.Add(new LotNotify
                    {
                        LotNo = lot.LotNo,
                        IsUpdate = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    });
                }
            }
            else return;

            using var transaction = _sPDbContext.Database.BeginTransaction();
            try
            {
                _sPDbContext.Order.AddRange(newOrders);
                _sPDbContext.Lot.AddRange(newLots);
                _sPDbContext.OrderNotify.AddRange(orderNotifies);
                _sPDbContext.LotNotify.AddRange(lotNotifies);

                await _sPDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<Order>> GetJPOrderAsync()
        {
            var Orders = await (from a in _jPDbContext.OrdHorder
                                join b in _jPDbContext.OrdOrder on a.OrderNo equals b.Ordno into bGroup
                                from b in bGroup.DefaultIfEmpty()

                                where a.FactoryDate!.Value.Year == DateTime.Now.Year
                                      && a.Factory == true
                                      && !a.OrderNo.StartsWith("S")
                                      && (a.CustCode != "STOCK" && a.CustCode != "SAMPLE")
                                      && _jPDbContext.JobOrder.Any(j => j.OrderNo == a.OrderNo && j.Owner != "SAMPLE")

                                orderby a.FactoryDate descending

                                select new Order
                                {
                                    OrderNo = a.OrderNo,
                                    CustCode = a.CustCode,
                                    FactoryDate = a.FactoryDate,
                                    OrderDate = b.OrderDate,
                                    SeldDate1 = b.SeldDate1,
                                    OrdDate = a.OrdDate,

                                    IsSuccess = false,
                                    IsActive = true,
                                    CreateDate = DateTime.Now,
                                    UpdateDate = DateTime.Now
                                }).ToListAsync();

            var existingOrders = from a in Orders
                                 join b in _sPDbContext.Order on a.OrderNo equals b.OrderNo into abGroup
                                 from b in abGroup.DefaultIfEmpty()

                                 where b == null

                                 select a;

            return [.. existingOrders];
        }
    }
}
