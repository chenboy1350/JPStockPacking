using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Implement
{
    public class OrderManagementService(
        JPDbContext jPDbContext,
        SPDbContext sPDbContext,
        IProductionPlanningService productionPlanningService,
        IPISService pISService
        ) : IOrderManagementService
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
                    .Where(x => x.IsActive && !x.IsSuccess)
                    .ToListAsync();

                var orderNotifies = await _sPDbContext.OrderNotify
                    .Where(x => x.IsActive)
                    .ToDictionaryAsync(x => x.OrderNo, x => x);

                var lots = await _sPDbContext.Lot
                    .Where(x => x.IsActive)
                    .GroupBy(l => l.OrderNo)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var updatedLotNos = await _sPDbContext.LotNotify
                    .Where(x => x.IsActive && x.IsUpdate)
                    .Select(x => x.LotNo)
                    .ToHashSetAsync();

                var result = orders.Select(order =>
                {
                    orderNotifies.TryGetValue(order.OrderNo, out var notify);
                    var relatedLots = lots.TryGetValue(order.OrderNo, out var lotGroup) ? lotGroup : [];

                    var operateDays = Convert.ToInt32(Math.Ceiling(relatedLots.Select(l => l.OperateDays ?? 0).Sum()));

                    var packDate = order.OrderDate.GetValueOrDefault().Date;
                    var exportDate = order.SeldDate1.GetValueOrDefault().Date;

                    var packDaysRemain = (packDate - today).Days;
                    var exportDaysRemain = (exportDate - today).Days;

                    return new CustomOrder
                    {
                        OrderNo = order.OrderNo,
                        CustCode = order.CustCode ?? "",
                        FactoryDate = order.FactoryDate.GetValueOrDefault(),
                        OrderDate = order.OrderDate.GetValueOrDefault(),
                        SeldDate1 = order.SeldDate1.GetValueOrDefault(),
                        OrdDate = order.OrdDate.GetValueOrDefault(),
                        TotalLot = relatedLots.Count,
                        SumTtQty = (int)relatedLots.Sum(l => l.TtQty.GetValueOrDefault()),
                        CompleteLot = relatedLots.Count(l => l.IsSuccess),
                        OperateDays = operateDays,
                        StartDate = order.PackStartDate.GetValueOrDefault(),
                        PackDaysRemain = packDaysRemain,
                        ExportDaysRemain = exportDaysRemain,
                        IsUpdate = notify != null && notify.IsUpdate,
                        IsNew = notify != null && notify.IsNew,
                        IsActive = order.IsActive,
                        IsSuccess = order.IsSuccess,
                        IsReceivedLate = packDaysRemain <= 1 && !order.IsSuccess,
                        IsPackingLate = exportDaysRemain <= 1 && !order.IsSuccess,
                        CustomLot = [.. relatedLots.Select(l => new CustomLot
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
                            ReceivedQty = l.ReceivedQty.GetValueOrDefault(),
                            IsSuccess = l.IsSuccess,
                            IsActive = l.IsActive,
                            IsUpdate = updatedLotNos.Contains(l.LotNo),
                            UpdateDate = l.UpdateDate.GetValueOrDefault(),

                            IsAllReceived = l.ReceivedQty.GetValueOrDefault() >= l.TtQty.GetValueOrDefault(),
                        })]
                    };
                })
                .OrderBy(o => o.StartDate)
                .ToList();

                if (!string.IsNullOrEmpty(orderNo))
                {
                    result = [.. result.Where(o => o.OrderNo.Contains(orderNo, StringComparison.OrdinalIgnoreCase))];
                }

                if (!string.IsNullOrEmpty(custCode))
                {
                    result = [.. result.Where(o => o.CustCode.Contains(custCode, StringComparison.OrdinalIgnoreCase))];
                }

                if (fromDate.Date != DateTime.MinValue && toDate.Date != DateTime.MinValue)
                {
                    result = [.. result.Where(o => o.StartDate.Date >= fromDate.Date && o.StartDate.Date <= toDate.Date)];
                }

                if (fromDate.Date != DateTime.MinValue)
                {
                    result = [.. result.Where(o => o.StartDate.Date >= fromDate.Date)];
                }

                if (toDate.Date != DateTime.MinValue)
                {
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
                    schedule.Weeks = result
                        .GroupBy(o => o.StartDate.StartOfWeek())
                        .OrderBy(g => g.Key)
                        .Select(g =>
                        {
                            var start = g.Key;
                            var end = start.AddDays(6);
                            var title = (start <= today && today <= end) ? "This Week" : $"{start:dd MMM} – {end:dd MMM yyyy}";

                            var days = g.GroupBy(x => x.StartDate.Date)
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
                        })
                        .ToList();
                }

                return schedule;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CustomLot?> GetCustomLotAsync(string lotNo)
        {
            var result = await (
                from lot in _sPDbContext.Lot
                join notify in _sPDbContext.LotNotify
                    on lot.LotNo equals notify.LotNo into lotNotifyGroup
                from notify in lotNotifyGroup.DefaultIfEmpty()
                where lot.LotNo == lotNo && lot.IsActive
                select new CustomLot
                {
                    LotNo = lot.LotNo,
                    OrderNo = lot.OrderNo,
                    ListNo = lot.ListNo ?? string.Empty,
                    CustPcode = lot.CustPcode ?? string.Empty,
                    TtQty = lot.TtQty ?? 0,
                    Article = lot.Article ?? string.Empty,
                    Barcode = lot.Barcode ?? string.Empty,
                    TdesArt = lot.TdesArt ?? string.Empty,
                    MarkCenter = lot.MarkCenter ?? string.Empty,
                    SaleRem = lot.SaleRem ?? string.Empty,
                    ReceivedQty = lot.ReceivedQty ?? 0,
                    IsSuccess = lot.IsSuccess,
                    IsActive = lot.IsActive,
                    IsUpdate = notify != null && notify.IsUpdate,
                    UpdateDate = lot.UpdateDate ?? DateTime.MinValue
                }
            ).FirstOrDefaultAsync();

            return result;
        }

        public async Task ImportOrderAsync()
        {
            List<OrderNotify> orderNotifies = [];
            List<LotNotify> lotNotifies = [];

            var newLots = new List<Lot>();

            var newOrders = await GetJPOrderAsync();

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

        public async Task GetUpdateLotAsync()
        {
            var newReceiveds = await GetJPReceivedAsync();

            if (newReceiveds.Count == 0) return;

            var lotNos = newReceiveds.Select(x => x.LotNo).Distinct().ToList();

            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                _sPDbContext.Received.AddRange(newReceiveds);

                var existingLotNotifies = await _sPDbContext.LotNotify
                    .Where(x => lotNos.Contains(x.LotNo))
                    .ToListAsync();

                var existingOrderNotifies = await (from a in _sPDbContext.OrderNotify
                                                   join b in _sPDbContext.Lot
                                                   on a.OrderNo equals b.OrderNo
                                                   where lotNos.Contains(b.LotNo)
                                                   select a).ToListAsync();

                foreach (var lotNotify in existingLotNotifies)
                {
                    lotNotify.IsUpdate = true;
                    lotNotify.UpdateDate = DateTime.Now;
                }

                foreach (var orderNotify in existingOrderNotifies)
                {
                    orderNotify.IsUpdate = true;
                    orderNotify.UpdateDate = DateTime.Now;
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

        public async Task UpdateReceivedItemsAsync(string lotNo, string[] receivedNo)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                decimal totalReceivedQty = 0;

                var Lot = await _sPDbContext.Lot.FirstOrDefaultAsync(o => o.LotNo == lotNo);
                if (Lot != null)
                {
                    foreach (var item in receivedNo)
                    {
                        var receive = await _sPDbContext.Received.FirstOrDefaultAsync(o => o.ReceiveNo == item && o.LotNo == lotNo && !o.IsReceived);

                        totalReceivedQty += receive?.TtQty ?? 0;

                        if (receive != null)
                        {
                            receive.IsReceived = true;
                            receive.UpdateDate = DateTime.Now;

                            await _sPDbContext.SaveChangesAsync();
                        }
                    }

                    if (totalReceivedQty <= 0) return;

                    Lot.ReceivedQty = totalReceivedQty;
                    Lot.UpdateDate = DateTime.Now;

                    var lotNotify = await _sPDbContext.LotNotify.FirstOrDefaultAsync(x => x.LotNo == lotNo);
                    if (lotNotify != null)
                    {
                        lotNotify.IsUpdate = false;
                        lotNotify.UpdateDate = DateTime.Now;
                    }

                    var lstLotNotic = await (from a in _sPDbContext.Order
                                             join b in _sPDbContext.Lot on a.OrderNo equals b.OrderNo into abGroup
                                             from b in abGroup.DefaultIfEmpty()
                                             join c in _sPDbContext.LotNotify on b.LotNo equals c.LotNo into bcGroup
                                             from c in bcGroup.DefaultIfEmpty()
                                             where a.OrderNo == Lot.OrderNo
                                             select c).ToListAsync();

                    var hasIsUpdate = lstLotNotic.Any(x => x.IsUpdate == true);
                    if (!hasIsUpdate)
                    {
                        var orderNotify = await _sPDbContext.OrderNotify.FirstOrDefaultAsync(x => x.OrderNo == Lot.OrderNo);
                        if (orderNotify != null)
                        {
                            orderNotify.IsUpdate = false;
                            orderNotify.UpdateDate = DateTime.Now;
                        }
                    }
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

        public async Task AssignReceivedAsync(string lotNo, string[] receivedNo, string tableId, string[] memberIds)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var revNo in receivedNo)
                {
                    var received = await _sPDbContext.Received
                        .FirstOrDefaultAsync(x => x.ReceiveNo == revNo && x.LotNo == lotNo);

                    if (received == null)
                        continue;

                    // 1. ปิดข้อมูล Assignment เก่าก่อน (ถ้ามี)
                    var existingAssignments = await _sPDbContext.Assignment
                        .Where(a => a.ReceivedNo == revNo && a.IsActive)
                        .ToListAsync();

                    foreach (var assign in existingAssignments)
                    {
                        assign.IsActive = false;
                        assign.UpdateDate = DateTime.Now;

                        var assignTables = await _sPDbContext.AssignmentTable
                            .Where(at => at.AssignmentId == assign.AssignmentId && at.IsActive)
                            .ToListAsync();
                        foreach (var t in assignTables)
                        {
                            t.IsActive = false;
                            t.UpdateDate = DateTime.Now;
                        }

                        var assignMembers = await _sPDbContext.AssignmentMember
                            .Where(am => am.AssignmentId == assign.AssignmentId && am.IsActive)
                            .ToListAsync();
                        foreach (var m in assignMembers)
                        {
                            m.IsActive = false;
                            m.UpdateDate = DateTime.Now;
                        }
                    }

                    // 2. สร้าง Assignment ใหม่
                    var newAssignment = new Assignment
                    {
                        ReceivedNo = revNo,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.Assignment.Add(newAssignment);
                    await _sPDbContext.SaveChangesAsync(); // ต้องเซฟก่อนเพื่อให้ AssignmentId มา

                    // 3. เพิ่ม AssignmentTable ใหม่
                    var assignTable = new AssignmentTable
                    {
                        AssignmentId = newAssignment.AssignmentId,
                        WorkTableId = Convert.ToInt32(tableId),
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.AssignmentTable.Add(assignTable);

                    // 4. เพิ่ม AssignmentMember ใหม่
                    foreach (var memberId in memberIds)
                    {
                        var assignMember = new AssignmentMember
                        {
                            AssignmentId = newAssignment.AssignmentId,
                            WorkTableMemberId = Convert.ToInt32(memberId),
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };
                        _sPDbContext.AssignmentMember.Add(assignMember);
                    }

                    // 5. อัปเดตสถานะของ Received
                    received.IsAssigned = true;
                    received.UpdateDate = DateTime.Now;
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


        public async Task<List<Order>> GetJPOrderAsync()
        {
            var Orders = await (from a in _jPDbContext.OrdHorder
                                join b in _jPDbContext.OrdOrder on a.OrderNo equals b.Ordno into bGroup
                                from b in bGroup.DefaultIfEmpty()

                                where a.FactoryDate!.Value.Month == DateTime.Now.Month
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

                              where a.FactoryDate!.Value.Month == DateTime.Now.Month
                                      && a.FactoryDate!.Value.Year == DateTime.Now.Year
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

        public async Task<List<Received>> GetJPReceivedAsync()
        {
            var allReceived = await (
                from a in _jPDbContext.Spdreceive
                join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                where d.FactoryDate.HasValue && d.FactoryDate.Value.Year == DateTime.Now.Year
                      && a.Mdate >= DateTime.Now.AddMonths(-3)
                select new Received
                {
                    ReceiveNo = a.ReceiveNo,
                    LotNo = a.Lotno,
                    TtQty = a.Ttqty,
                    TtWg = (double)a.Ttwg,
                    IsReceived = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                }
            ).ToListAsync();

            var incompleteLotNos = await _sPDbContext.Lot
                .Where(x => !x.IsSuccess && x.ReceivedQty < x.TtQty)
                .Select(x => x.LotNo)
                .ToHashSetAsync();

            var existingReceiveNos = await _sPDbContext.Received
                .Where(x => incompleteLotNos.Contains(x.LotNo))
                .Select(x => x.ReceiveNo)
                .ToHashSetAsync();

            var receivedToInsert = allReceived
                .Where(x => !existingReceiveNos.Contains(x.ReceiveNo) && incompleteLotNos.Contains(x.LotNo))
                .ToList();

            return [.. receivedToInsert];
        }

        public async Task<List<ReceivedListModel>> GetReceivedAsync(string lotNo)
        {
            var result = await (from a in _sPDbContext.Received
                                join b in _sPDbContext.Lot on a.LotNo equals b.LotNo
                                where a.LotNo == lotNo && !a.IsReceived
                                select new ReceivedListModel
                                {
                                    ReceiveNo = a.ReceiveNo,
                                    LotNo = a.LotNo,
                                    ListNo = b.ListNo!,
                                    OrderNo = b.OrderNo,
                                    Article = b.Article!,
                                    Barcode = b.Barcode!,
                                    CustPCode = b.CustPcode ?? string.Empty,
                                    TtQty = a.TtQty.GetValueOrDefault(),
                                    Ttwg = a.TtWg.GetValueOrDefault(),
                                }).ToListAsync();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo)
        {
            var result = await (from a in _sPDbContext.Received
                                join b in _sPDbContext.Lot on a.LotNo equals b.LotNo
                                where a.LotNo == lotNo && a.IsReceived
                                select new ReceivedListModel
                                {
                                    ReceiveNo = a.ReceiveNo,
                                    LotNo = a.LotNo,
                                    ListNo = b.ListNo!,
                                    OrderNo = b.OrderNo,
                                    Article = b.Article!,
                                    Barcode = b.Barcode!,
                                    CustPCode = b.CustPcode ?? string.Empty,
                                    TtQty = a.TtQty.GetValueOrDefault(),
                                    Ttwg = a.TtWg.GetValueOrDefault(),
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
