using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;

namespace JPStockPacking.Services.Implement
{
    public class OrderManagementService(JPDbContext jPDbContext, SPDbContext sPDbContext) : IOrderManagementService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public List<CustomOrder> GetOrderAndLot()
        {
            try
            {
                var orders = _sPDbContext.Order
                    .Where(x => x.IsActive && !x.IsSuccess)
                    .ToList();

                var orderNotifies = _sPDbContext.OrderNotify
                    .Where(x => x.IsActive)
                    .ToDictionary(x => x.OrderNo, x => x);

                var lots = _sPDbContext.Lot
                    .Where(x => x.IsActive)
                    .ToList()
                    .GroupBy(l => l.OrderNo)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var updatedLotNos = _sPDbContext.LotNotify
                    .Where(x => x.IsActive && x.IsUpdate)
                    .Select(x => x.LotNo)
                    .ToHashSet();

                var result = orders.Select(order =>
                {
                    orderNotifies.TryGetValue(order.OrderNo, out var notify);
                    var relatedLots = lots.TryGetValue(order.OrderNo, out var lotGroup) ? lotGroup : [];

                    return new CustomOrder
                    {
                        OrderNo = order.OrderNo,
                        CustCode = order.CustCode ?? "",
                        FactoryDate = order.FactoryDate.GetValueOrDefault(),
                        OrderDate = order.OrderDate.GetValueOrDefault(),
                        SeldDate1 = order.SeldDate1.GetValueOrDefault(),
                        OrdDate = order.OrdDate.GetValueOrDefault(),
                        TotalLot = relatedLots.Count,
                        CompleteLot = relatedLots.Count(l => l.IsPacked),
                        StartDate = relatedLots.Count != 0 ? relatedLots.Min(l => l.CreateDate.GetValueOrDefault()) : DateTime.Now,
                        OperateDays = relatedLots.Count != 0 ? (DateTime.Now - relatedLots.Min(l => l.CreateDate.GetValueOrDefault())).Days : 0,
                        IsNew = notify?.IsNew ?? false,
                        IsUpdate = notify?.IsUpdate ?? false,
                        IsActive = order.IsActive,
                        IsSuccess = order.IsSuccess,
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
                            AssignTo = l.AssignTo.GetValueOrDefault(),
                            IsPacked = l.IsPacked,
                            IsActive = l.IsActive,
                            IsUpdate = updatedLotNos.Contains(l.LotNo),
                            UpdateDate = l.UpdateDate.GetValueOrDefault(),
                        })]
                    };
                }).ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public void ImportOrder()
        {
            var newOrders = GetJPOrder();

            if (newOrders.Count != 0)
            {
                var newLots = GetJPLot(newOrders);

                if (newLots.Count == 0)
                {
                    return;
                }

                //var newReceiveds = GetJPReceived();


                List<OrderNotify> orderNotifies = [];
                List<LotNotify> lotNotifies = [];

                foreach (var order in newOrders)
                {
                    orderNotifies.Add(new OrderNotify
                    {
                        OrderNo = order.OrderNo,
                        IsNew = true,
                        IsUpdate = false,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    });
                }

                //foreach (var rec in newReceiveds)
                //{
                //    lotNotifies.Add(new LotNotify
                //    {
                //        LotNo = rec.LotNo,
                //        IsUpdate = true,
                //        IsActive = true,
                //        CreateDate = DateTime.Now,
                //        UpdateDate = DateTime.Now
                //    });
                //}

                using var transaction = _sPDbContext.Database.BeginTransaction();
                try
                {
                    _sPDbContext.Order.AddRange(newOrders);
                    _sPDbContext.SaveChanges();

                    _sPDbContext.Lot.AddRange(newLots);
                    _sPDbContext.SaveChanges();

                    _sPDbContext.OrderNotify.AddRange(orderNotifies);
                    _sPDbContext.SaveChanges();

                    //_sPDbContext.LotNotify.AddRange(lotNotifies);
                    //_sPDbContext.SaveChanges();

                    //_sPDbContext.Received.AddRange(newReceiveds);
                    //_sPDbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateLot()
        {
            //var jpLot = GetJPLot();

            //if (jpLot != null)
            //{

            //}
        }

        public List<Order> GetJPOrder()
        {
            var Orders = from a in _jPDbContext.OrdHorder
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
                         };

            var existingOrders = from a in Orders.ToList()
                                 join b in _sPDbContext.Order on a.OrderNo equals b.OrderNo into abGroup
                                 from b in abGroup.DefaultIfEmpty()

                                 where b == null

                                 select a;

            return [.. existingOrders];
        }

        public List<Lot> GetJPLot(List<Order> newOrders)
        {
            var Lots = from a in _jPDbContext.OrdHorder
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
                           AssignTo = null,
                           IsPacked = false,
                           IsActive = true,
                           CreateDate = DateTime.Now,
                           UpdateDate = DateTime.Now
                       };

            var existingLots = from a in Lots.ToList()
                               join b in newOrders on a.OrderNo equals b.OrderNo into abGroup
                               from b in abGroup.DefaultIfEmpty()
                               where b != null
                               select a;

            return [.. existingLots];
        }

        public List<Received> GetJPReceived()
        {
            var receivedToInsert = (
                from a in _jPDbContext.Spdreceive
                join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                join e in _sPDbContext.Order on c.OrderNo equals e.OrderNo
                where e.IsActive
                      && d.FactoryDate.HasValue && d.FactoryDate.Value.Year == DateTime.Now.Year
                      && a.Mdate >= DateTime.Now.AddMonths(-3)
                      && !_sPDbContext.Received
                            .Any(r => r.ReceiveNo == a.ReceiveNo && r.LotNo == a.Lotno)
                select new Received
                {
                    ReceiveNo = a.ReceiveNo,
                    LotNo = a.Lotno,
                    Ttqty = a.Ttqty,
                    Ttwg = a.Ttwg,

                    IsReceived = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                }
            ).ToList();


            return [.. receivedToInsert];
        }
    }
}
