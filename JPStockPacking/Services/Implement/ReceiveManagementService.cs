using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace JPStockPacking.Services.Implement
{
    public class ReceiveManagementService(JPDbContext jPDbContext, SPDbContext sPDbContext, IProductionPlanningService productionPlanningService) : IReceiveManagementService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IProductionPlanningService _productionPlanningService = productionPlanningService;

        public async Task UpdateLotItemsAsync(string receiveNo, string[] orderNos, int[] receiveIds)
        {
            foreach (var orderNo in orderNos)
            {
                var existingOrder = await _sPDbContext.Order.FirstOrDefaultAsync(o => o.OrderNo == orderNo && o.IsActive);
                if (existingOrder == null)
                {
                    await ImportOrderAsync(orderNo);
                }
            }

            foreach (var receiveId in receiveIds)
            {
                await UpdateAllReceivedItemsAsync(receiveId);
            }

            await UpdateReceiveHeaderStatusAsync(receiveNo);
        }

        private async Task ImportOrderAsync(string orderNo)
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
                return;
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

        private async Task UpdateAllReceivedItemsAsync(int receiveId)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receive = await (
                    from a in _jPDbContext.Spdreceive
                    join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                    join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                    where a.Id == receiveId
                    select new
                    {
                        a.Id,
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
                ).FirstOrDefaultAsync() ?? throw new InvalidOperationException("ไม่พบใบรับ");

                var validOrder = await _sPDbContext.Order.AnyAsync(o => o.OrderNo == receive.OrderNo);
                if (!validOrder) return;

                var existingIds = await _sPDbContext.Received
                    .Where(x => x.ReceiveId == receive.Id)
                    .Select(x => x.ReceiveId)
                    .ToHashSetAsync();

                var candidate = new Received
                {
                    ReceiveId = receive.Id,
                    ReceiveNo = receive.ReceiveNo,
                    LotNo = receive.Lotno,
                    TtQty = receive.Ttqty,
                    TtWg = (double)receive.Ttwg,
                    Barcode = receive.Barcode,
                    BillNumber = receive.Billnumber,
                    Mdate = receive.Mdate,
                    IsReceived = true,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                var toInsert = new List<Received>();
                if (!existingIds.Contains(candidate.ReceiveId))
                {
                    toInsert.Add(candidate);
                }

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

        public async Task UpdateReceiveHeaderStatusAsync(string receiveNo)
        {
            using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var header = await _jPDbContext.Sphreceive
                    .Include(h => h.Spdreceive)
                    .FirstOrDefaultAsync(h => h.ReceiveNo == receiveNo) ?? throw new InvalidOperationException($"ไม่พบใบรับ {receiveNo}");

                var detailIds = header.Spdreceive.Select(d => d.Id).ToList();
                if (detailIds.Count == 0)
                    throw new InvalidOperationException($"ใบรับ {receiveNo} ไม่มีรายการ detail");

                var receivedIds = await _sPDbContext.Received
                    .Where(r => r.ReceiveNo == receiveNo && r.IsReceived)
                    .Select(r => r.ReceiveId)
                    .ToListAsync();

                bool isComplete = detailIds.All(id => receivedIds.Contains(id));

                if (isComplete && !header.Mupdate)
                {
                    header.Mupdate = true;
                    await _jPDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<Order>> GetJPOrderAsync(string orderNo)
        {
            var Orders = await (from a in _jPDbContext.OrdHorder
                                join b in _jPDbContext.OrdOrder on a.OrderNo equals b.Ordno into bGroup
                                from b in bGroup.DefaultIfEmpty()

                                where a.OrderNo == orderNo
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
                              join h in _jPDbContext.JobCost on new { b.LotNo, b.OrderNo } equals new { LotNo = h.Lotno, OrderNo = h.Orderno } into bhGroup
                              from h in bhGroup.DefaultIfEmpty()
                              join i in _jPDbContext.CfnCode on e.FnCode equals i.FnCode into eiGroup
                              from i in eiGroup.DefaultIfEmpty()

                              where a.FactoryDate!.Value.Year == DateTime.Now.Year
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
                                  Unit = b.Unit.Trim(),
                                  Si = h != null ? h.QtySi : 0,
                                  Article = e.Article,
                                  Barcode = b.Barcode,
                                  TdesArt = f.TdesArt,
                                  EdesArt = f.EdesArt,
                                  TdesFn = i.TdesFn,
                                  EdesFn = i.EdesFn,
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

        public async Task<List<ReceivedListModel>> GetTopJPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo)
        {
            var query = from a in _jPDbContext.Sphreceive
                select new
                {
                    a.ReceiveNo,
                    a.Mdate,
                    a.Mupdate,
                    TotalDetail = _jPDbContext.Spdreceive.Count()
                };

            if (!string.IsNullOrWhiteSpace(receiveNo))
            {
                query = query.Where(b => b.ReceiveNo.Contains(receiveNo));
            }

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                query = query.Where(b => _jPDbContext.Spdreceive
                    .Join(_jPDbContext.OrdLotno, sr => sr.Lotno, ol => ol.LotNo, (sr, ol) => new { sr, ol })
                    .Any(joined => joined.ol.OrderNo.Contains(orderNo) && joined.sr.ReceiveNo == b.ReceiveNo));

            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                query = query.Where(b => _jPDbContext.Spdreceive.Any(sr => sr.Lotno.Contains(lotNo) && sr.ReceiveNo == b.ReceiveNo));
            }

            var receives = await query.OrderByDescending(o => o.Mdate).Take(100).ToListAsync();

            var receiveNos = receives.Select(r => r.ReceiveNo).ToList();

            var receivedLookup = await _sPDbContext.Received
                .Where(r => receiveNos.Contains(r.ReceiveNo) && r.IsReceived)
                .GroupBy(r => r.ReceiveNo)
                .Select(g => new { ReceiveNo = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ReceiveNo, x => x.Count);

            var result = receives.Select(r => new ReceivedListModel
            {
                ReceiveNo = r.ReceiveNo,
                Mdate = r.Mdate.ToString("dd MMMM yyyy", new CultureInfo("th-TH")),
                IsReceived = r.Mupdate,
                HasRevButNotAll = receivedLookup.TryGetValue(r.ReceiveNo, out var receivedCount) && receivedCount > 0 && receivedCount < r.TotalDetail
            }).ToList();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetJPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo)
        {
            var allReceived = await (
                from a in _jPDbContext.Spdreceive
                join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                where a.ReceiveNo == receiveNo
                select new
                {
                    a.Id,
                    a.ReceiveNo,
                    a.Lotno,
                    a.Ttqty,
                    a.Ttwg,
                    a.Barcode,
                    a.Article,
                    d.OrderNo,
                    c.ListNo
                }
            ).ToListAsync();

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                allReceived = [.. allReceived.Where(x => x.OrderNo.Contains(orderNo))];
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                allReceived = [.. allReceived.Where(x => x.Lotno.Contains(lotNo))];
            }

            var existingIds = await _sPDbContext.Received
                .Where(x => x.ReceiveNo == receiveNo && x.IsReceived)
                .Select(x => x.ReceiveId)
                .ToHashSetAsync();

            var result = allReceived.Select(x => new ReceivedListModel
            {
                ReceivedID = x.Id,
                ReceiveNo = x.ReceiveNo,
                LotNo = x.Lotno,
                TtQty = x.Ttqty,
                TtWg = (double)x.Ttwg,
                Barcode = x.Barcode,
                Article = x.Article,
                OrderNo = x.OrderNo,
                ListNo = x.ListNo,
                IsReceived = existingIds.Contains(x.Id)
            }).ToList();

            return result;
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
