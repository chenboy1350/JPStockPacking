using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static JPStockPacking.Services.Helper.Enum;
using static JPStockPacking.Services.Implement.AuthService;

namespace JPStockPacking.Services.Implement
{
    public class OrderManagementService(
        JPDbContext jPDbContext,
        SPDbContext sPDbContext,
        IProductionPlanningService productionPlanningService,
        IPISService pISService) : IOrderManagementService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IProductionPlanningService _productionPlanningService = productionPlanningService;
        private readonly IPISService _pISService = pISService;

        public async Task<ScheduleListModel> GetOrderAndLotByRangeAsync(GroupMode groupMode, string orderNo, string custCode, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var today = DateTime.Now.Date;

                var orders = await _sPDbContext.Order
                    .Where(x => x.IsActive)
                    .ToListAsync();

                var orderNotifies = await _sPDbContext.OrderNotify
                    .Where(x => x.IsActive)
                    .ToDictionaryAsync(x => x.OrderNo, x => x);

                var lots = await _sPDbContext.Lot
                    .Where(x => x.IsActive)
                    .GroupBy(l => l.OrderNo)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var result = new List<CustomOrder>();

                foreach (var order in orders)
                {
                    orderNotifies.TryGetValue(order.OrderNo, out var notify);
                    var relatedLots = lots.TryGetValue(order.OrderNo, out var lotGroup) ? lotGroup : new List<Lot>();

                    var operateDays = Convert.ToInt32(Math.Ceiling(
                        relatedLots.Select(l => l.OperateDays ?? 0).Sum()
                    ));

                    var packDate = order.OrderDate.GetValueOrDefault().Date;
                    var exportDate = order.SeldDate1.GetValueOrDefault().Date;

                    var packDaysRemain = (packDate - today).Days;
                    var exportDaysRemain = (exportDate - today).Days;

                    var customLots = new List<CustomLot>();

                    foreach (var l in relatedLots)
                    {
                        var assignedTable = await (
                            from rec in _sPDbContext.Received
                            join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                            join at in _sPDbContext.AssignmentTable on ass.AssignmentReceivedId equals at.AssignmentReceivedId
                            join wt in _sPDbContext.WorkTable on at.WorkTableId equals wt.Id
                            where rec.LotNo == l.LotNo && rec.IsActive && ass.IsActive && at.IsActive && wt.IsActive
                            select new AssignedWorkTableModel
                            {
                                AssignmentId = ass.AssignmentId,
                                TableName = wt.Name!
                            }).Distinct().ToListAsync();

                        bool IsLotUpdate = await _sPDbContext.LotNotify.Where(w => w.LotNo == l.LotNo).AnyAsync(a => a.IsActive && a.IsUpdate);
                        bool IsAllReturned = l.TtQty.GetValueOrDefault() > 0 && l.ReturnedQty.GetValueOrDefault() >= l.TtQty.GetValueOrDefault();
                        bool IsAllReceived = l.TtQty.GetValueOrDefault() > 0 && l.ReceivedQty.GetValueOrDefault() >= l.TtQty.GetValueOrDefault();
                        bool IsPacking = l.TtQty.GetValueOrDefault() > 0 && l.AssignedQty.GetValueOrDefault() > 0;
                        bool IsPacked = l.ReturnedQty >= l.TtQty.GetValueOrDefault();
                        bool IsAllAssigned = !await _sPDbContext.Received.Where(w => w.LotNo == l.LotNo).AnyAsync(a => !a.IsAssigned);

                        bool HasRepair = await (
                            from rec in _sPDbContext.Received
                            join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                            join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
                            join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
                            where rec.LotNo == l.LotNo
                                  && rec.IsSendRepair == true
                                  && ret.HasRepair == true
                            select ret
                        ).AnyAsync();

                        bool HasLost = await (
                            from rec in _sPDbContext.Received
                            join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                            join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
                            join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
                            where rec.LotNo == l.LotNo
                                  && ret.HasLost == true
                            select ret
                        ).AnyAsync();

                        customLots.Add(new CustomLot
                        {
                            LotNo = l.LotNo,
                            OrderNo = l.OrderNo,
                            ListNo = l.ListNo!,
                            CustPcode = l.CustPcode!,
                            TtQty = l.TtQty.GetValueOrDefault(),
                            Article = l.Article!,
                            Barcode = l.Barcode!,
                            TdesArt = l.TdesArt!,
                            MarkCenter = l.MarkCenter!,
                            SaleRem = l.SaleRem!,
                            IsSuccess = l.IsSuccess,
                            IsActive = l.IsActive,
                            IsUpdate = IsLotUpdate,
                            UpdateDate = l.UpdateDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                            AssignTo = assignedTable,

                            IsPacking = IsPacking,
                            IsPacked = IsPacked,
                            IsAllAssigned = IsAllAssigned,

                            ReceivedQty = l.ReceivedQty.GetValueOrDefault(),
                            ReturnedQty = l.ReturnedQty.GetValueOrDefault(),

                            IsAllReceived = IsAllReceived,

                            HasRepair = HasRepair,
                            HasLost = HasLost,
                            IsAllReturned = IsAllReturned
                        });
                    }

                    result.Add(new CustomOrder
                    {
                        OrderNo = order.OrderNo,
                        CustCode = order.CustCode ?? "",
                        FactoryDate = order.FactoryDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        OrderDate = order.OrderDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        SeldDate1 = order.SeldDate1.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        OrdDate = order.OrdDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        TotalLot = relatedLots.Count,
                        SumTtQty = (int)relatedLots.Sum(l => l.TtQty.GetValueOrDefault()),
                        CompleteLot = relatedLots.Count(l => l.IsSuccess),
                        OperateDays = operateDays,
                        StartDate = order.PackStartDate.GetValueOrDefault(),
                        StartDateTH = order.PackStartDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                        PackDaysRemain = packDaysRemain,
                        ExportDaysRemain = exportDaysRemain,
                        IsUpdate = notify != null && notify.IsUpdate,
                        IsNew = notify != null && notify.IsNew,
                        IsActive = order.IsActive,
                        IsSuccess = order.IsSuccess,
                        IsReceivedLate = packDaysRemain <= 1 && !order.IsSuccess,
                        IsPackingLate = exportDaysRemain <= 1 && !order.IsSuccess,
                        CustomLot = customLots
                    });
                }

                if (!string.IsNullOrEmpty(orderNo))
                    result = [.. result.Where(o => o.OrderNo.Contains(orderNo, StringComparison.OrdinalIgnoreCase))];

                if (!string.IsNullOrEmpty(custCode))
                    result = [.. result.Where(o => o.CustCode.Contains(custCode, StringComparison.OrdinalIgnoreCase))];

                if (fromDate.Date != DateTime.MinValue && toDate.Date != DateTime.MinValue)
                    result = [.. result.Where(o => o.StartDate.Date >= fromDate.Date && o.StartDate.Date <= toDate.Date)];
                else
                {
                    if (fromDate.Date != DateTime.MinValue)
                        result = [.. result.Where(o => o.StartDate.Date >= fromDate.Date)];

                    if (toDate.Date != DateTime.MinValue)
                        result = [.. result.Where(o => o.StartDate.Date <= toDate.Date)];
                }

                var schedule = new ScheduleListModel();

                if (groupMode == GroupMode.Day)
                {
                    schedule.Days = [.. result
                        .GroupBy(o => o.StartDate.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new Day
                        {
                            Title = g.Key == today ? "Today" : g.Key.ToString("ddd dd MMM yyyy"),
                            Orders = [.. g]
                        })];
                }
                else
                {
                    schedule.Weeks = [.. result
                        .GroupBy(o => o.StartDate.StartOfWeek())
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var start = g.Key;
                            var end = start.AddDays(6);
                            var title = (start <= today && today <= end)
                                ? "This Week"
                                : $"{start:dd MMM} – {end:dd MMM yyyy}";

                            var days = g
                                .GroupBy(x => x.StartDate.Date)
                                .OrderBy(d => d.Key)
                                .Select(d => new Day
                                {
                                    Title = d.Key == today ? "Today" : d.Key.ToString("ddd dd MMM"),
                                    Orders = [.. d]
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
            catch
            {
                throw;
            }
        }

        public async Task<CustomLot?> GetCustomLotAsync(string lotNo)
        {
            var lot = await _sPDbContext.Lot
                .FirstOrDefaultAsync(l => l.LotNo == lotNo && l.IsActive);

            if (lot == null)
                return null;

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
                }).Distinct().ToListAsync();

            bool IsLotUpdate = await _sPDbContext.LotNotify
                .Where(w => w.LotNo == lot.LotNo)
                .AnyAsync(a => a.IsActive && a.IsUpdate);

            bool IsAllReturned = lot.TtQty.GetValueOrDefault() > 0 &&
                                 lot.ReturnedQty.GetValueOrDefault() >= lot.TtQty.GetValueOrDefault();

            bool IsAllReceived = lot.TtQty.GetValueOrDefault() > 0 &&
                                 lot.ReceivedQty.GetValueOrDefault() >= lot.TtQty.GetValueOrDefault();

            bool IsPacking = lot.TtQty.GetValueOrDefault() > 0 &&
                             lot.AssignedQty.GetValueOrDefault() > 0;

            bool IsPacked = lot.ReturnedQty >= lot.TtQty.GetValueOrDefault();

            bool IsAllAssigned = !await _sPDbContext.Received
                .Where(w => w.LotNo == lot.LotNo)
                .AnyAsync(a => !a.IsAssigned);

            bool HasRepair = await (
                from rec in _sPDbContext.Received
                join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
                join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
                where rec.LotNo == lot.LotNo &&
                      rec.IsSendRepair == true &&
                      ret.HasRepair == true
                select ret
            ).AnyAsync();

            bool HasLost = await (
                from rec in _sPDbContext.Received
                join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
                join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
                where rec.LotNo == lot.LotNo &&
                      ret.HasLost == true
                select ret
            ).AnyAsync();

            return new CustomLot
            {
                LotNo = lot.LotNo,
                OrderNo = lot.OrderNo,
                ListNo = lot.ListNo!,
                CustPcode = lot.CustPcode!,
                TtQty = lot.TtQty.GetValueOrDefault(),
                Article = lot.Article!,
                Barcode = lot.Barcode!,
                TdesArt = lot.TdesArt!,
                MarkCenter = lot.MarkCenter!,
                SaleRem = lot.SaleRem!,
                IsSuccess = lot.IsSuccess,
                IsActive = lot.IsActive,
                IsUpdate = IsLotUpdate,
                UpdateDate = lot.UpdateDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                AssignTo = assignedTable,

                IsPacking = IsPacking,
                IsPacked = IsPacked,
                IsAllAssigned = IsAllAssigned,

                ReceivedQty = lot.ReceivedQty.GetValueOrDefault(),
                ReturnedQty = lot.ReturnedQty.GetValueOrDefault(),

                IsAllReceived = IsAllReceived,
                HasRepair = HasRepair,
                HasLost = HasLost,
                IsAllReturned = IsAllReturned
            };
        }

        public async Task ImportOrderAsync(string orderNo)
        {
            List<OrderNotify> orderNotifies = [];
            List<LotNotify> lotNotifies = [];

            var newLots = new List<Lot>();

            var existingOrder = await _sPDbContext.Order.AnyAsync(o => o.OrderNo == orderNo && o.IsActive);
            if (existingOrder)
            {
                throw new InvalidOperationException("Order นี้มีอยู่ในระบบแล้ว");
            }

            var newOrders = await GetJPOrderAsync(orderNo);

            if (newOrders.Count != 0)
            {
                newLots = await GetJPLotAsync(newOrders);

                if (newLots.Count == 0)
                {
                    return;
                }

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
            else
            {
                throw new InvalidOperationException("ไม่พบ Order ที่ต้องการนำเข้า");
            }

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

            await RecalculateScheduleAsync();
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

        public async Task AssignReceivedAsync(string lotNo, int[] receivedIDs, string tableId, string[] memberIds, bool hasPartTime, int WorkerNumber)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var revID in receivedIDs)
                {
                    var receiveds = await _sPDbContext.Received
                        .FirstOrDefaultAsync(x => x.ReceivedId == revID && x.LotNo == lotNo);

                    if (receiveds == null) continue;

                    var existingAssignments = await _sPDbContext.AssignmentReceived
                        .Where(a => a.ReceivedId == receiveds.ReceivedId && a.IsActive)
                        .ToListAsync();

                    foreach (var assign in existingAssignments)
                    {
                        assign.IsActive = false;
                        assign.UpdateDate = DateTime.Now;

                        var assignTables = await _sPDbContext.AssignmentTable
                            .Where(at => at.AssignmentReceivedId == assign.AssignmentReceivedId && at.IsActive)
                            .ToListAsync();
                        foreach (var t in assignTables)
                        {
                            t.IsActive = false;
                            t.UpdateDate = DateTime.Now;
                        }

                        var assignMembers = await _sPDbContext.AssignmentMember
                            .Where(am => am.AssignmentReceivedId == assign.AssignmentId && am.IsActive)
                            .ToListAsync();
                        foreach (var m in assignMembers)
                        {
                            m.IsActive = false;
                            m.UpdateDate = DateTime.Now;
                        }
                    }

                    var newAssignment = new Assignment
                    {
                        NumberWorkers = WorkerNumber + memberIds.Length,
                        HasPartTime = hasPartTime,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.Assignment.Add(newAssignment);
                    await _sPDbContext.SaveChangesAsync();

                    var assignReceived = new AssignmentReceived
                    {
                        AssignmentId = newAssignment.AssignmentId,
                        ReceivedId = receiveds.ReceivedId,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.AssignmentReceived.Add(assignReceived);
                    await _sPDbContext.SaveChangesAsync();

                    var assignTable = new AssignmentTable
                    {
                        AssignmentReceivedId = assignReceived.AssignmentReceivedId,
                        WorkTableId = Convert.ToInt32(tableId),
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.AssignmentTable.Add(assignTable);

                    foreach (var memberId in memberIds)
                    {
                        var assignMember = new AssignmentMember
                        {
                            AssignmentReceivedId = assignReceived.AssignmentReceivedId,
                            WorkTableMemberId = Convert.ToInt32(memberId),
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };
                        _sPDbContext.AssignmentMember.Add(assignMember);
                    }

                    receiveds.IsAssigned = true;
                    receiveds.UpdateDate = DateTime.Now;
                    await _sPDbContext.SaveChangesAsync();
                }

                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == lotNo);
                if (lot != null)
                {
                    var assignedQty = await _sPDbContext.Received
                        .Where(x => x.LotNo == lotNo && x.IsAssigned)
                        .SumAsync(x => x.TtQty ?? 0);

                    lot.AssignedQty = assignedQty;
                    lot.UpdateDate = DateTime.Now;
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReturnReceivedAsync(string lotNo, int[] assignmentIDs, decimal lostQty, decimal breakQty, decimal returnQty)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(o => o.LotNo == lotNo && o.IsActive) ?? throw new InvalidOperationException($"Lot '{lotNo}' not found or inactive.");

                var returned = new Returned
                {
                    ReturnTtQty = returnQty,
                    LostQty = lostQty,
                    BreakQty = breakQty,
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
                        item.Received.IsSendRepair = false;
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

                if ((lot.TtQty ?? 0) > 0 && lot.ReturnedQty >= lot.TtQty)
                {
                    lot.IsSuccess = true;
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

        public async Task LostAndRepairAsync(string lotNo, int[] assignmentIDs, decimal lostQty, decimal breakQty, decimal returnQty, int breakDescriptionID)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(o => o.LotNo == lotNo && o.IsActive) ?? throw new InvalidOperationException($"Lot '{lotNo}' not found or inactive.");

                Returned returned = new()
                {
                    ReturnTtQty = returnQty,
                    LostQty = lostQty,
                    BreakQty = breakQty,
                    HasRepair = breakQty != 0,
                    HasLost = lostQty != 0,
                    BreakDescriptionId = breakDescriptionID,
                    IsSuccess = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _sPDbContext.Returned.Add(returned);
                await _sPDbContext.SaveChangesAsync();

                List<ReturnedDetail> details = [];

                if (assignmentIDs.Length > 0)
                {
                    var receivedList = await (
                        from a in _sPDbContext.AssignmentReceived
                        join r in _sPDbContext.Received on a.ReceivedId equals r.ReceivedId
                        where assignmentIDs.Contains(a.AssignmentId) && a.IsActive && r.LotNo == lotNo
                        select new { a.AssignmentId, Received = r }
                    ).OrderByDescending(x => x.Received.TtQty).ToListAsync();

                    foreach (var item in receivedList)
                    {
                        decimal ttQty = item.Received.TtQty ?? 0;
                        decimal ttWg = (decimal)(item.Received.TtWg ?? 0);

                        if (breakQty > 0 && ttQty > 0)
                        {
                            decimal repairQty = ttQty - breakQty;
                            if (repairQty < 0) repairQty = 0;

                            decimal repairWg = (ttQty > 0) ? repairQty * (ttWg / ttQty) : 0;

                            await UpdateJobBillSendStockAndSpdreceive(item.Received.BillNumber, repairQty, Math.Round(repairWg, 2));

                            item.Received.TtQty = repairQty;
                            item.Received.IsSendRepair = true;
                            item.Received.RepairTtQty = breakQty;

                            lot.ReceivedQty -= breakQty;
                            lot.AssignedQty -= breakQty;

                            breakQty -= ttQty;
                        }

                        if (lostQty > 0 && ttQty > 0)
                        {
                            lot.ReceivedQty -= lostQty;
                            lot.AssignedQty -= lostQty;
                            item.Received.IsSendRepair = false;
                        }

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

                if ((lot.TtQty ?? 0) > 0 && lot.ReturnedQty >= lot.TtQty)
                {
                    lot.IsSuccess = true;
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

        public async Task UpdateJobBillSendStockAndSpdreceive(int Billnumber, decimal TtQty, decimal TtWg)
        {
            using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var spdreceive = await _jPDbContext.Spdreceive.FirstOrDefaultAsync(o => o.Billnumber == Billnumber);
                if (spdreceive != null)
                {
                    spdreceive.Ttqty = TtQty;
                    spdreceive.Ttwg = TtWg;
                }

                var jobBillSendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(o => o.Billnumber == Billnumber);
                if (jobBillSendStock != null)
                {
                    jobBillSendStock.Ttqty = TtQty;
                    jobBillSendStock.Ttwg = TtWg;
                }

                //await _jPDbContext.SaveChangesAsync();
                //await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DefineToPackAsync(string orderNo, List<LotToPackDTO> lots)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();

            try
            {
                var header = await _sPDbContext.SendQtyToPack
                    .FirstOrDefaultAsync(x => x.OrderNo == orderNo && x.IsActive);

                if (header == null)
                {
                    header = new SendQtyToPack
                    {
                        OrderNo = orderNo,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.SendQtyToPack.Add(header);
                    await _sPDbContext.SaveChangesAsync();
                }

                var oldDetails = await _sPDbContext.SendQtyToPackDetail
                    .Where(x => x.SendQtyToPackId == header.SendQtyToPackId && x.IsActive)
                    .ToListAsync();

                var now = DateTime.Now;
                var hasChanges = false;

                foreach (var lot in lots)
                {
                    var existing = oldDetails.FirstOrDefault(d => d.LotNo == lot.LotNo);

                    if (existing != null)
                    {
                        if (existing.TtQty == lot.Qty)
                        {
                            continue;
                        }
                        else
                        {
                            existing.IsActive = false;
                            existing.UpdateDate = now;

                            var oldSizes = await _sPDbContext.SendQtyToPackDetailSize
                                .Where(s => s.SendQtyToPackDetailId == existing.SendQtyToPackDetailId && s.IsActive)
                                .ToListAsync();

                            foreach (var s in oldSizes)
                            {
                                s.IsActive = false;
                                s.UpdateDate = now;
                            }

                            var newDetail = new SendQtyToPackDetail
                            {
                                SendQtyToPackId = header.SendQtyToPackId,
                                LotNo = lot.LotNo,
                                TtQty = lot.Qty,
                                IsActive = true,
                                CreateDate = now,
                                UpdateDate = now
                            };
                            _sPDbContext.SendQtyToPackDetail.Add(newDetail);

                            foreach (var size in lot.Sizes)
                            {
                                var newSizeDetail = new SendQtyToPackDetailSize
                                {
                                    SendQtyToPackDetailId = newDetail.SendQtyToPackDetailId,
                                    SizeIndex = size.SizeIndex,
                                    TtQty = size.TtQty,
                                    IsActive = true,
                                    CreateDate = now,
                                    UpdateDate = now
                                };

                                _sPDbContext.SendQtyToPackDetailSize.Add(newSizeDetail);
                            }

                            hasChanges = true;
                        }
                    }
                    else
                    {
                        var newDetail = new SendQtyToPackDetail
                        {
                            SendQtyToPackId = header.SendQtyToPackId,
                            LotNo = lot.LotNo,
                            TtQty = lot.Qty,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        };
                        _sPDbContext.SendQtyToPackDetail.Add(newDetail);
                        await _sPDbContext.SaveChangesAsync();

                        foreach (var size in lot.Sizes)
                        {
                            var newSizeDetail = new SendQtyToPackDetailSize
                            {
                                SendQtyToPackDetailId = newDetail.SendQtyToPackDetailId,
                                SizeIndex = size.SizeIndex,
                                TtQty = size.TtQty,
                                IsActive = true,
                                CreateDate = now,
                                UpdateDate = now
                            };

                            _sPDbContext.SendQtyToPackDetailSize.Add(newSizeDetail);
                        }

                        hasChanges = true;
                    }
                }

                if (!hasChanges)
                {
                    throw new InvalidOperationException("ไม่มีข้อมูลที่เปลี่ยนแปลง");
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

        public async Task<List<Order>> GetJPOrderAsync(string orderNo)
        {
            var Orders = await (from a in _jPDbContext.OrdHorder
                                join b in _jPDbContext.OrdOrder on a.OrderNo equals b.Ordno into bGroup
                                from b in bGroup.DefaultIfEmpty()

                                where /*a.FactoryDate!.Value.Month == DateTime.Now.AddMonths(-1).Month*/
                                      a.OrderNo == orderNo
                                      && a.FactoryDate!.Value.Year == DateTime.Now.Year
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

        public async Task<List<Lot>> GetJPLotAsync(List<Order> newOrders)
        {
            var Lots = await (from a in _jPDbContext.OrdHorder
                              join b in _jPDbContext.OrdLotno on a.OrderNo equals b.OrderNo into abGroup
                              from b in abGroup.DefaultIfEmpty()
                              join e in _jPDbContext.CpriceSale on b.Barcode equals e.Barcode into beGroup
                              from e in beGroup.DefaultIfEmpty()
                              join f in _jPDbContext.Cprofile on e.Article equals f.Article into efGroup
                              from f in efGroup.DefaultIfEmpty()
                              join g in _jPDbContext.OrdDorder on new { b.OrderNo, b.Barcode, b.CustPcode } equals new { g.OrderNo, g.Barcode, g.CustPcode } into bgGroup
                              from g in bgGroup.DefaultIfEmpty()

                              where /*a.FactoryDate!.Value.Month == DateTime.Now.AddMonths(-1).Month*/
                                       a.FactoryDate!.Value.Year == DateTime.Now.Year
                                      && a.Factory == true
                                      && !string.IsNullOrEmpty(b.LotNo)
                                      && !a.OrderNo.StartsWith("S")
                                      && (a.CustCode != "STOCK" && a.CustCode != "SAMPLE")
                                      && _jPDbContext.JobOrder.Any(j => j.OrderNo == a.OrderNo && j.Owner != "SAMPLE")

                              orderby a.FactoryDate descending

                              select new Lot
                              {
                                  LotNo = b.LotNo,
                                  OrderNo = a.OrderNo,
                                  ListNo = b.ListNo,
                                  CustPcode = b.CustPcode,
                                  TtQty = b.TtQty,
                                  TtWg = (double?)b.TtWg,
                                  Article = e.Article,
                                  Barcode = b.Barcode,
                                  TdesArt = f.TdesArt,
                                  MarkCenter = f.MarkCenter,
                                  SaleRem = g.SaleRem,
                                  ReceivedQty = 0,
                                  ReturnedQty = 0,
                                  OperateDays = _productionPlanningService.CalLotOperateDay(Convert.ToInt32(b.TtQty ?? 0)),
                                  IsSuccess = false,
                                  IsActive = true,
                                  CreateDate = DateTime.Now,
                                  UpdateDate = DateTime.Now
                              }).ToListAsync();

            var existingLots = from a in Lots
                               join b in newOrders on a.OrderNo equals b.OrderNo into abGroup
                               from b in abGroup.DefaultIfEmpty()
                               where b != null
                               select a;

            return [.. existingLots];
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

        public async Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo)
        {
            var result = await (from a in _sPDbContext.Received
                                join b in _sPDbContext.Lot on a.LotNo equals b.LotNo
                                where a.LotNo == lotNo && a.IsReceived && !a.IsReturned
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

        public async Task<List<BreakDescription>> GetBreakDescriptionsAsync()
        {
            var des = await _sPDbContext.BreakDescription
                .Where(x => x.IsActive)
                .Select(x => new BreakDescription
                {
                    BreakDescriptionId = x.BreakDescriptionId,
                    Name = x.Name,
                })
                .ToListAsync();

            return [.. des];
        }

        public async Task<List<TableMemberModel>> GetTableMemberAsync(int tableID)
        {
            var employees = await _pISService.GetEmployeeAsync();

            if (employees == null || employees.Count == 0)
            {
                throw new Exception("No employees found.");
            }

            var tableMembers = await _sPDbContext.WorkTableMember
                .Where(x => x.WorkTableId == tableID && x.IsActive)
                .ToListAsync();

            var result = from member in tableMembers
                         join employee in employees on member.EmpId equals employee.Id
                         select new TableMemberModel
                         {
                             Id = employee.Id,
                             FirstName = employee.FirstName,
                             LastName = employee.LastName,
                             NickName = employee.NickName
                         };

            return [.. result];
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

        public async Task<SendToPackModel> GetOrderToSendQtyAsync(string orderNo)
        {
            var baseData = await (
                from a in _jPDbContext.OrdHorder
                join b in _jPDbContext.OrdLotno on a.OrderNo equals b.OrderNo into gj
                from b in gj.DefaultIfEmpty()
                join c in _jPDbContext.CpriceSale on b.Barcode equals c.Barcode into gj2
                from c in gj2.DefaultIfEmpty()
                join e in _jPDbContext.JobCost on new { b.LotNo, b.OrderNo } equals new { LotNo = e.Lotno, OrderNo = e.Orderno } into gj4
                from e in gj4.DefaultIfEmpty()
                join f in _jPDbContext.CfnCode on c.FnCode equals f.FnCode into gj5
                from f in gj5.DefaultIfEmpty()
                join g in _jPDbContext.Cprofile on c.Article equals g.Article into gj6
                from g in gj6.DefaultIfEmpty()
                join d in (
                    from spd in _jPDbContext.Spdreceive
                    join sph in _jPDbContext.Sphreceive on spd.ReceiveNo equals sph.ReceiveNo into sphj
                    from sph in sphj.DefaultIfEmpty()
                    group spd by spd.Lotno into g
                    select new
                    {
                        Lotno = g.Key,
                        TTQty = g.Sum(x => (decimal?)x.Ttqty)
                    }
                ) on b.LotNo equals d.Lotno into gj3
                from d in gj3.DefaultIfEmpty()
                where a.OrderNo == orderNo && a.Factory == true
                orderby b.LotNo
                select new
                {
                    a.OrderNo,
                    a.CustCode,
                    a.Grade,
                    a.Scountry,
                    a.Special,
                    b.LotNo,
                    b.ListNo,
                    b.Barcode,
                    Article = c.Article ?? string.Empty,
                    g.Tunit,
                    g.TdesArt,
                    f.EdesFn,
                    f.TdesFn,
                    c.Picture,
                    b.TtQty,
                    QtySi = e != null ? e.QtySi : 0,
                    SendPack_Qty = d != null ? (d.TTQty ?? 0) : 0
                }).ToListAsync();

            if (baseData is not { Count: > 0 })
                throw new InvalidOperationException($"Order '{orderNo}' not found or has no lots.");

            var headerId = await _sPDbContext.SendQtyToPack
                .Where(x => x.OrderNo == orderNo && x.IsActive)
                .Select(x => x.SendQtyToPackId)
                .FirstOrDefaultAsync();

            Dictionary<string, decimal> detailDict = new(StringComparer.OrdinalIgnoreCase);

            if (headerId > 0)
            {
                detailDict = await _sPDbContext.SendQtyToPackDetail
                    .Where(d => d.SendQtyToPackId == headerId && d.IsActive)
                    .GroupBy(d => d.LotNo)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Last().TtQty,
                        StringComparer.OrdinalIgnoreCase
                    );
            }

            var lotNos = baseData.Select(x => x.LotNo).Where(x => x != null).Distinct().ToList();
            var sizeMap = await GetSizeByLotBulkAsync(orderNo, lotNos);

            var lots = baseData.Select(x =>
            {
                detailDict.TryGetValue(x.LotNo ?? "", out var planQty);
                sizeMap.TryGetValue(x.LotNo ?? "", out var sizes);

                return new SendToPackLots
                {
                    LotNo = x.LotNo ?? string.Empty,
                    ListNo = x.ListNo ?? string.Empty,
                    Barcode = x.Barcode ?? string.Empty,
                    Article = x.Article,
                    Tunit = x.Tunit ?? string.Empty,
                    EdesFn = x.EdesFn ?? string.Empty,
                    TdesFn = x.TdesFn ?? string.Empty,
                    TdesArt = x.TdesArt ?? string.Empty,
                    Picture = (x.Picture ?? "").Split("\\", StringSplitOptions.None).LastOrDefault() ?? string.Empty,
                    ImagePath = x.Picture ?? string.Empty,
                    TtQty = x.TtQty ?? 0,
                    QtySi = x.QtySi,
                    SendTtQty = x.SendPack_Qty,
                    TtQtyToPack = planQty,
                    IsDefined = planQty > 0,
                    Size = sizes ?? []
                };
            }).ToList();

            return new SendToPackModel
            {
                OrderNo = orderNo,
                CustCode = baseData[0].CustCode ?? string.Empty,
                Grade = baseData[0].Grade ?? string.Empty,
                SCountry = baseData[0].Scountry ?? string.Empty,
                Special = baseData[0].Special ?? string.Empty,
                Lots = lots
            };
        }

        private async Task<Dictionary<string, List<Size>>> GetSizeByLotBulkAsync(string orderNo, List<string> lotNos)
        {
            if (lotNos == null || lotNos.Count == 0) return [];

            var ordLots = await _jPDbContext.OrdLotno.Where(l => l.OrderNo == orderNo).ToListAsync();

            var filteredLots = ordLots
                .Where(l => l.OrderNo == orderNo && lotNos.Contains(l.LotNo!))
                .ToList();

            var savedSizes = await (
                from header in _sPDbContext.SendQtyToPack
                join detail in _sPDbContext.SendQtyToPackDetail on header.SendQtyToPackId equals detail.SendQtyToPackId
                join size in _sPDbContext.SendQtyToPackDetailSize on detail.SendQtyToPackDetailId equals size.SendQtyToPackDetailId
                where header.OrderNo == orderNo && header.IsActive && detail.IsActive && size.IsActive
                select new
                {
                    detail.LotNo,
                    SizeIndex = EF.Property<int>(size, "SizeIndex"),
                    Qty = size.TtQty ?? 0,
                    size.IsUnderQuota,
                    Approver = size.Approver ?? 0
                }
            ).ToListAsync();

            var savedSizeDict = savedSizes
                .GroupBy(x => x.LotNo)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.SizeIndex, s => s)
                );

            var result = new Dictionary<string, List<Size>>(StringComparer.OrdinalIgnoreCase);

            foreach (var lot in filteredLots)
            {
                var sizes = new List<Size>();
                var type = lot.GetType();

                var hasAny = Enumerable.Range(1, 12).Any(i =>
                    !string.IsNullOrWhiteSpace((string?)type.GetProperty($"S{i}")?.GetValue(lot)) ||
                    !string.IsNullOrWhiteSpace((string?)type.GetProperty($"Cs{i}")?.GetValue(lot)) ||
                    Convert.ToDecimal(type.GetProperty($"Q{i}")?.GetValue(lot) ?? 0) > 0
                );

                if (!hasAny)
                {
                    result[lot.LotNo!] = [];
                    continue;
                }

                for (int i = 1; i <= 12; i++)
                {
                    string? s = (string?)type.GetProperty($"S{i}")?.GetValue(lot);
                    string? cs = (string?)type.GetProperty($"Cs{i}")?.GetValue(lot);
                    decimal q = Convert.ToDecimal(type.GetProperty($"Q{i}")?.GetValue(lot) ?? 0);

                    decimal qtyToPack = 0;
                    bool isUnderQuota = false;
                    int approver = 0;

                    if (savedSizeDict.TryGetValue(lot.LotNo!, out var lotSizes) && lotSizes.TryGetValue(i, out var saved))
                    {
                        qtyToPack = saved.Qty;
                        isUnderQuota = saved.IsUnderQuota;
                        approver = saved.Approver;
                    }

                    if (!string.IsNullOrWhiteSpace(s) || !string.IsNullOrWhiteSpace(cs) || q > 0)
                    {
                        sizes.Add(new Size
                        {
                            S = s ?? "",
                            CS = cs ?? "",
                            Q = q,
                            TtQtyToPack = qtyToPack,
                            IsDefined = qtyToPack > 0,
                            IsUnderQuota = isUnderQuota,
                            Approver = approver
                        });
                    }
                }

                result[lot.LotNo!] = sizes;
            }

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

        public async Task<List<BreakDescription>> AddNewBreakDescription(string breakDescription)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                BreakDescription newDescription = new()
                {
                    Name = breakDescription,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                _sPDbContext.BreakDescription.Add(newDescription);
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetBreakDescriptionsAsync();
        }

        public async Task<LostAndRepairModel> GetLostAsync(string LotNo)
        {
            var result = await (
                from ret in _sPDbContext.Returned
                join retd in _sPDbContext.ReturnedDetail on ret.ReturnId equals retd.ReturnId
                join ass in _sPDbContext.AssignmentReceived on retd.AssignmentId equals ass.AssignmentId
                join rec in _sPDbContext.Received on ass.ReceivedId equals rec.ReceivedId
                join lot in _sPDbContext.Lot on rec.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                where rec.LotNo == LotNo && ret.HasLost == true
                select new LostAndRepairModel
                {
                    LotNo = rec.LotNo,
                    Barcode = rec.Barcode,
                    Article = lot.Article ?? string.Empty,
                    OrderNo = lot.OrderNo,
                    ListNo = lot.ListNo,
                    SeldDate1 = ord.SeldDate1.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    CustCode = ord.CustCode ?? string.Empty,
                    LostQty = ret.LostQty,
                    TtQty = lot.TtQty ?? 0,
                    TtWg = (double)(lot.TtWg ?? 0),
                }
            ).FirstOrDefaultAsync();

            return result!;
        }

        public async Task<List<LostAndRepairModel>> GetRepairAsync(string LotNo)
        {
            var result = await (
                from rec in _sPDbContext.Received
                join ass in _sPDbContext.AssignmentReceived on rec.ReceivedId equals ass.ReceivedId
                join retd in _sPDbContext.ReturnedDetail on ass.AssignmentId equals retd.AssignmentId
                join ret in _sPDbContext.Returned on retd.ReturnId equals ret.ReturnId
                join lot in _sPDbContext.Lot on rec.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                join bek in _sPDbContext.BreakDescription on ret.BreakDescriptionId equals bek.BreakDescriptionId
                where rec.LotNo == LotNo
                      && rec.IsSendRepair == true
                      && ret.HasRepair == true
                select new LostAndRepairModel
                {
                    ReceiveNo = rec.ReceiveNo,
                    LotNo = rec.LotNo,
                    TtQty = rec.TtQty ?? 0,
                    TtWg = (double)(rec.TtWg ?? 0),
                    Barcode = rec.Barcode,
                    Article = lot.Article ?? string.Empty,
                    OrderNo = lot.OrderNo,
                    CustCode = ord.CustCode ?? string.Empty,
                    ListNo = lot.ListNo,
                    BreakQty = ret.BreakQty,
                    BreakDescription = bek.Name ?? string.Empty
                }
            ).ToListAsync();

            return result;
        }

        public class TableModel
        {
            public int Id { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
        }

        public class LostAndRepairModel : ReceivedListModel
        {

            public decimal? BreakQty { get; set; }
            public decimal? LostQty { get; set; }
            public string BreakDescription { get; set; } = string.Empty;
            public string CustCode { get; set; } = string.Empty;
            public string SeldDate1 { get; set; } = string.Empty;
        }

        public async Task RecalculateScheduleAsync()
        {
            const int tables = 6;
            const double hoursPerTablePerDay = 8.5;
            const double dailyCapacity = tables * hoursPerTablePerDay; // 6 * 8.5 = 51.0 hours/day

            var lots = await _sPDbContext.Lot
                .Where(l => l.IsActive && !l.IsSuccess)
                .OrderBy(l => l.OrderNo)
                .ToListAsync();

            var usedPerDay = new Dictionary<DateTime, double>();

            // Group by Order
            var lotGroups = lots.GroupBy(l => l.OrderNo);

            foreach (var group in lotGroups)
            {
                var relatedLots = group.ToList();
                var totalHours = relatedLots.Sum(l => (l.OperateDays ?? 0) * hoursPerTablePerDay); // แปลงวันเป็นชั่วโมง


                var deadline = _sPDbContext.Order
                    .Where(o => o.OrderNo == group.Key)
                    .Select(o => o.OrderDate)
                    .FirstOrDefault() ?? DateTime.Today;

                var scheduledStart = _productionPlanningService.FindAvailableStartDate(
                    totalHours,
                    deadline,
                    usedPerDay,
                    dailyCapacity
                );

                // Adjust if scheduledStart falls on weekend
                while (scheduledStart.DayOfWeek == DayOfWeek.Saturday || scheduledStart.DayOfWeek == DayOfWeek.Sunday)
                {
                    scheduledStart = scheduledStart.AddDays(-1);
                }

                // Update lots with same start date (shared)
                //foreach (var lot in relatedLots)
                //{
                //    lot.PackStartDate = scheduledStart;
                //    lot.PackEndDate = deadline;
                //}

                var order = await _sPDbContext.Order.FirstOrDefaultAsync(o => o.OrderNo == group.Key);
                if (order != null)
                {
                    order.PackStartDate = scheduledStart;
                    order.PackEndDate = deadline;
                }

                // Update usedPerDay tracking in hourly chunks
                double remainingHours = totalHours;
                var day = scheduledStart;
                while (remainingHours > 0)
                {
                    if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (!usedPerDay.ContainsKey(day))
                            usedPerDay[day] = 0;

                        var available = dailyCapacity - usedPerDay[day];
                        var hoursToUse = Math.Min(available, remainingHours);
                        usedPerDay[day] += hoursToUse;
                        remainingHours -= hoursToUse;
                    }
                    day = day.AddDays(1);
                }
            }

            await _sPDbContext.SaveChangesAsync();
        }
    }
}
