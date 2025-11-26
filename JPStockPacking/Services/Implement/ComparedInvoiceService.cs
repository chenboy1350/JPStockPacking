using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace JPStockPacking.Services.Implement
{
    public class ComparedInvoiceService(JPDbContext jPDbContext, SPDbContext sPDbContext) : IComparedInvoiceService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<ComparedInvoiceModel>> GetFilteredInvoice(ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            try
            {
                DateTime FromDate = comparedInvoiceFilterModel.FromDate ?? DateTime.Now;
                DateTime ToDate = comparedInvoiceFilterModel.ToDate ?? DateTime.Now;

                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo) || !string.IsNullOrEmpty(comparedInvoiceFilterModel.InvoiceNo))
                {
                    FromDate = SqlDateTime.MinValue.Value;
                    ToDate = SqlDateTime.MaxValue.Value;
                }

                if (comparedInvoiceFilterModel.FromDate != null)
                {
                    FromDate = comparedInvoiceFilterModel.FromDate.Value;
                }

                if (comparedInvoiceFilterModel.ToDate != null)
                {
                    ToDate = comparedInvoiceFilterModel.ToDate.Value;
                }

                var JPInvoiceList = (from hinv in _jPDbContext.ExHminv
                                     join dinv in _jPDbContext.ExDminv on hinv.MinvNo equals dinv.MinvNo into dinvGroup
                                     from dinv in dinvGroup
                                     join ord in _jPDbContext.OrdHorder on dinv.OrderNo equals ord.OrderNo into ordGroup
                                     from ord in ordGroup.DefaultIfEmpty()
                                     where hinv.InvSend
                                     select new
                                     {
                                         hinv,
                                         dinv,
                                         ord.CustCode
                                     });

                JPInvoiceList = JPInvoiceList.Where(x => x.hinv.Mdate >= FromDate && x.hinv.Mdate <= ToDate);

                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.InvoiceNo))
                {
                    var invoiceNo = comparedInvoiceFilterModel.InvoiceNo.Insert(2, "I").ToUpper();
                    JPInvoiceList = JPInvoiceList.Where(x => x.hinv.InvNo == invoiceNo);
                }

                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
                {
                    JPInvoiceList = JPInvoiceList.Where(x => x.dinv.OrderNo == comparedInvoiceFilterModel.OrderNo);
                }

                var JPExDminv = await JPInvoiceList.Select(x => new ExDminv
                {
                    MinvNo = x.hinv.MinvNo,
                    InvNo = x.hinv.InvNo,
                    LotNo = x.dinv.LotNo,
                    Article = x.dinv.Article,
                    SetNo = x.dinv.SetNo,
                    MakeUnit = x.dinv.MakeUnit,
                    OrderNo = x.dinv.OrderNo,
                    TtQty = x.dinv.TtQty,
                    PperUnit = x.dinv.PperUnit,
                    TtPrice = x.dinv.TtPrice,
                    OrderCust = x.CustCode
                }).ToListAsync();

                List<ComparedInvoiceModel> JPComparedInvoiceList = [];

                foreach (var jpItem in JPExDminv.Where(w => !string.IsNullOrEmpty(w.OrderNo)))
                {
                    if (!string.IsNullOrEmpty(jpItem.SetNo) && string.IsNullOrEmpty(jpItem.Article))
                    {
                        var lot = await _jPDbContext.OrdLotno.Where(x => x.SetNo1 == jpItem.SetNo.Trim() && x.OrderNo == jpItem.OrderNo).ToListAsync();
                        if (lot != null && lot.Count > 0)
                        {
                            var spexlot = await _sPDbContext.Export.Where(x => lot.Select(l => l.LotNo).Contains(x.LotNo)).ToListAsync();

                            decimal JPQty = jpItem.TtQty ?? 0;
                            decimal JPPrice = jpItem.PperUnit;
                            decimal JPTotalPrice = jpItem.TtPrice ?? 0;
                            decimal JPTotalSetTtQty = lot.Where(w => !string.IsNullOrEmpty(w.LotNo)).Sum(x => x.TtQty ?? 0);

                            decimal SPQty = spexlot.FirstOrDefault()!.TtQty;
                            decimal SPPrice = lot.FirstOrDefault()!.CsetPrice;
                            decimal SPTotalPrice = SPPrice * SPQty;
                            decimal SPTotalSetTtQty = spexlot.Sum(x => x.TtQty);

                            bool isMatched = false;

                            if (JPQty == SPQty && JPPrice == SPPrice && (double)JPTotalPrice == (double)SPTotalPrice && JPTotalSetTtQty == SPTotalSetTtQty)
                            {
                                isMatched = true;
                            }

                            JPComparedInvoiceList.Add(new ComparedInvoiceModel
                            {
                                CustCode = jpItem.OrderCust ?? string.Empty,
                                MakeUnit = jpItem.MakeUnit ?? string.Empty,

                                JPInvoiceNo = jpItem.InvNo,
                                JPOrderNo = jpItem.OrderNo,
                                JPArticle = jpItem.SetNo,
                                JPTtQty = (double)JPQty,
                                JPPrice = (double)JPPrice,
                                JPTotalPrice = (double)JPTotalPrice,
                                JPTotalSetTtQty = (double)JPTotalSetTtQty,

                                SPInvoiceNo = jpItem.InvNo,
                                SPOrderNo = jpItem.OrderNo,
                                SPArticle = jpItem.SetNo,
                                SPTtQty = (double)SPQty,
                                SPPrice = (double)SPPrice,
                                SPTotalPrice = (double)SPTotalPrice,
                                SPTotalSetTtQty = (double)SPTotalSetTtQty,

                                IsMatched = isMatched
                            });

                        }
                    }
                    else
                    {
                        var lot = await _jPDbContext.OrdLotno.Where(x => x.LotNo == jpItem.LotNo).FirstOrDefaultAsync();

                        if (lot != null)
                        {
                            var spexlot = await _sPDbContext.Export.Where(x => x.LotNo == jpItem.LotNo).ToListAsync();

                            if (spexlot != null && spexlot.Count > 0)
                            {
                                decimal JPQty = jpItem.TtQty ?? 0;
                                decimal JPPrice = jpItem.PperUnit;
                                decimal JPTotalPrice = jpItem.TtPrice ?? 0;

                                decimal SPQty = spexlot.Where(x => x.TtQty == JPQty).Select(x => x.TtQty).FirstOrDefault();
                                decimal SPPrice = (decimal)lot.Price!;
                                decimal SPTotalPrice = SPPrice * SPQty;

                                bool isMatched = false;

                                if (JPQty == SPQty && JPPrice == SPPrice && (double)JPTotalPrice == (double)SPTotalPrice)
                                {
                                    isMatched = true;
                                }

                                JPComparedInvoiceList.Add(new ComparedInvoiceModel
                                {
                                    CustCode = jpItem.OrderCust ?? string.Empty,
                                    MakeUnit = jpItem.MakeUnit ?? string.Empty,

                                    JPInvoiceNo = jpItem.InvNo,
                                    JPOrderNo = jpItem.OrderNo,
                                    JPArticle = jpItem.Article,
                                    JPTtQty = (double)JPQty,
                                    JPPrice = (double)JPPrice,
                                    JPTotalPrice = (double)JPTotalPrice,

                                    SPInvoiceNo = jpItem.InvNo,
                                    SPOrderNo = lot != null ? lot.OrderNo : "",
                                    SPArticle = jpItem.Article,
                                    SPTtQty = (double)SPQty,
                                    SPPrice = (double)SPPrice,
                                    SPTotalPrice = (double)SPTotalPrice,

                                    IsMatched = isMatched
                                });
                            }
                        }
                    }
                }

                return JPComparedInvoiceList;
            }
            catch (Exception)
            {
                return [];
            }

        }

        private async Task<List<ComparedInvoiceModel>> GetJPFilteredInvoice(ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            try
            {
                DateTime FromDate = comparedInvoiceFilterModel.FromDate ?? DateTime.MinValue;
                DateTime ToDate = comparedInvoiceFilterModel.ToDate ?? DateTime.MaxValue;

                var JPInvoiceList = (from hinv in _jPDbContext.ExHminv
                                     join dinv in _jPDbContext.ExDminv on hinv.MinvNo equals dinv.MinvNo into dinvGroup
                                     from dinv in dinvGroup
                                     join ord in _jPDbContext.OrdHorder on dinv.OrderNo equals ord.OrderNo into ordGroup
                                     from ord in ordGroup.DefaultIfEmpty()
                                     where hinv.InvSend
                                     select new
                                     {
                                         hinv,
                                         dinv,
                                         ord.CustCode
                                     }
                          );

                if (comparedInvoiceFilterModel.FromDate != null && comparedInvoiceFilterModel.ToDate != null)
                {
                    JPInvoiceList = JPInvoiceList.Where(x => x.hinv.Mdate >= FromDate && x.hinv.Mdate <= ToDate);
                }

                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.InvoiceNo))
                {
                    var invoiceNo = comparedInvoiceFilterModel.InvoiceNo.Insert(2, "I").ToUpper();
                    JPInvoiceList = JPInvoiceList.Where(x => x.hinv.InvNo == invoiceNo);
                }

                if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
                {
                    JPInvoiceList = JPInvoiceList.Where(x => x.dinv.OrderNo == comparedInvoiceFilterModel.OrderNo);
                }

                var JPExDminv = await JPInvoiceList.Select(x => new ExDminv
                {
                    MinvNo = x.hinv.MinvNo,
                    InvNo = x.hinv.InvNo,
                    LotNo = x.dinv.LotNo,
                    Article = x.dinv.Article,
                    SetNo = x.dinv.SetNo,
                    MakeUnit = x.dinv.MakeUnit,
                    OrderNo = x.dinv.OrderNo,
                    TtQty = x.dinv.TtQty,
                    PperUnit = x.dinv.PperUnit,
                    TtPrice = x.dinv.TtPrice,
                    OrderCust = x.CustCode
                }).ToListAsync();

                List<ComparedInvoiceModel> JPComparedInvoiceList = [];

                foreach (var jpItem in JPExDminv.Where(w => !string.IsNullOrEmpty(w.OrderNo)))
                {
                    if (!string.IsNullOrEmpty(jpItem.SetNo) && string.IsNullOrEmpty(jpItem.Article))
                    {
                        var lot = await _jPDbContext.OrdLotno.Where(x => x.SetNo1 == jpItem.SetNo.Trim() && x.OrderNo == jpItem.OrderNo).ToListAsync();
                        if (lot != null && lot.Count > 0)
                        {
                            decimal totalsentopackQty = 0;
                            foreach (var item in lot.Where(w => !string.IsNullOrEmpty(w.LotNo)))
                            {
                                totalsentopackQty = _jPDbContext.Spdreceive.Where(x => x.Lotno == item.LotNo).Sum(s => s.Ttqty);

                                List<int> billnos = [.. _jPDbContext.Spdreceive.Where(x => x.Lotno == item.LotNo).Select(s => s.Billnumber)];

                                foreach (var billno in billnos)
                                {
                                    var qtyKs = await (
                                        from js in _jPDbContext.JobBillSendStock
                                        join sjd1 in _jPDbContext.Sj1dreceive on js.Doc equals sjd1.ReceiveNo
                                        join sjh1 in _jPDbContext.Sj1hreceive on sjd1.ReceiveNo equals sjh1.ReceiveNo
                                        where js.Billnumber == billno
                                              && sjd1.Lotno == item.LotNo
                                              && js.SendType == "KS"
                                              && sjh1.Mupdate
                                        select js.Ttqty
                                    ).SumAsync();

                                    totalsentopackQty -= qtyKs;

                                    var qtyKm = await (
                                        from js in _jPDbContext.JobBillSendStock
                                        join sjd1 in _jPDbContext.Sj1dreceive on js.Doc equals sjd1.ReceiveNo
                                        join sjh1 in _jPDbContext.Sj1hreceive on sjd1.ReceiveNo equals sjh1.ReceiveNo
                                        where js.Billnumber == billno
                                              && sjd1.Lotno == item.LotNo
                                              && js.SendType == "KM"
                                              && sjh1.Mupdate
                                        select js.Ttqty
                                    ).SumAsync();

                                    totalsentopackQty -= qtyKm;
                                }
                            }

                            var spexlot = await _sPDbContext.Export.Where(x => lot.Select(l => l.LotNo).Contains(x.LotNo)).ToListAsync();

                            decimal JPQty = jpItem.TtQty ?? 0;
                            decimal JPPrice = jpItem.PperUnit;
                            decimal JPTotalPrice = jpItem.TtPrice ?? 0;
                            decimal JPTotalSetTtQty = lot.Sum(x => x.TtQty ?? 0);

                            //decimal SPQty = spexlot.FirstOrDefault()!.TtQty;
                            decimal SPQty = totalsentopackQty;
                            decimal SPPrice = lot.FirstOrDefault()!.CsetPrice;
                            decimal SPTotalPrice = SPPrice * SPQty;
                            //decimal SPTotalSetTtQty = spexlot.Sum(x => x.TtQty);
                            decimal SPTotalSetTtQty = lot.Sum(x => x.SetQty);

                            bool isMatched = false;

                            if (JPQty == SPQty && JPPrice == SPPrice && (double)JPTotalPrice == (double)SPTotalPrice && JPTotalSetTtQty == SPTotalSetTtQty)
                            {
                                isMatched = true;
                            }

                            JPComparedInvoiceList.Add(new ComparedInvoiceModel
                            {
                                CustCode = jpItem.OrderCust ?? string.Empty,
                                MakeUnit = jpItem.MakeUnit ?? string.Empty,

                                JPInvoiceNo = jpItem.InvNo,
                                JPOrderNo = jpItem.OrderNo,
                                JPArticle = jpItem.SetNo,
                                JPTtQty = (double)JPQty,
                                JPPrice = (double)JPPrice,
                                JPTotalPrice = (double)JPTotalPrice,
                                JPTotalSetTtQty = (double)JPTotalSetTtQty,

                                SPInvoiceNo = jpItem.InvNo,
                                SPOrderNo = jpItem.OrderNo,
                                SPArticle = jpItem.SetNo,
                                SPTtQty = (double)SPQty,
                                SPPrice = (double)SPPrice,
                                SPTotalPrice = (double)SPTotalPrice,
                                SPTotalSetTtQty = (double)SPTotalSetTtQty,

                                IsMatched = isMatched
                            });

                        }
                    }
                    else
                    {
                        var lot = await _jPDbContext.OrdLotno.Where(x => x.LotNo == jpItem.LotNo).FirstOrDefaultAsync();

                        if (lot != null)
                        {
                            decimal totalsentopackQty = _jPDbContext.Spdreceive.Where(x => x.Lotno == lot.LotNo).Sum(s => s.Ttqty);

                            List<int> billnos = [.. _jPDbContext.Spdreceive.Where(x => x.Lotno == lot.LotNo).Select(s => s.Billnumber)];

                            decimal StockQty = 0;
                            foreach (var billno in billnos)
                            {
                                var qtyKs = await (
                                    from js in _jPDbContext.JobBillSendStock
                                    join sjd1 in _jPDbContext.Sj1dreceive on js.Doc equals sjd1.ReceiveNo
                                    join sjh1 in _jPDbContext.Sj1hreceive on sjd1.ReceiveNo equals sjh1.ReceiveNo
                                    where js.Billnumber == billno
                                          && sjd1.Lotno == lot.LotNo
                                          && js.SendType == "KS"
                                          && sjh1.Mupdate
                                    select js.Ttqty
                                ).SumAsync();

                                StockQty += qtyKs;

                                var qtyKm = await (
                                    from js in _jPDbContext.JobBillSendStock
                                    join sjd1 in _jPDbContext.Sj1dreceive on js.Doc equals sjd1.ReceiveNo
                                    join sjh1 in _jPDbContext.Sj1hreceive on sjd1.ReceiveNo equals sjh1.ReceiveNo
                                    where js.Billnumber == billno
                                          && sjd1.Lotno == lot.LotNo
                                          && js.SendType == "KM"
                                          && sjh1.Mupdate
                                    select js.Ttqty
                                ).SumAsync();

                                StockQty += qtyKm;
                            }

                            var spexlot = await _sPDbContext.Export.Where(x => x.LotNo == jpItem.LotNo).FirstOrDefaultAsync();

                            decimal JPQty = jpItem.TtQty ?? 0;
                            decimal JPPrice = jpItem.PperUnit;
                            decimal JPTotalPrice = jpItem.TtPrice ?? 0;

                            //decimal SPQty = spexlot.TtQty ?? 0;
                            decimal SPQty = totalsentopackQty - StockQty;
                            decimal SPPrice = (decimal)lot.Price!;
                            decimal SPTotalPrice = SPPrice * SPQty;

                            bool isMatched = false;

                            if (JPQty == SPQty && JPPrice == SPPrice && (double)JPTotalPrice == (double)SPTotalPrice)
                            {
                                isMatched = true;
                            }

                            JPComparedInvoiceList.Add(new ComparedInvoiceModel
                            {
                                CustCode = jpItem.OrderCust ?? string.Empty,
                                MakeUnit = jpItem.MakeUnit ?? string.Empty,

                                JPInvoiceNo = jpItem.InvNo,
                                JPOrderNo = jpItem.OrderNo,
                                JPArticle = jpItem.Article,
                                JPTtQty = (double)JPQty,
                                JPPrice = (double)JPPrice,
                                JPTotalPrice = (double)JPTotalPrice,

                                SPInvoiceNo = jpItem.InvNo,
                                SPOrderNo = lot != null ? lot.OrderNo : "",
                                SPArticle = jpItem.Article,
                                SPTtQty = (double)SPQty,
                                SPPrice = (double)SPPrice,
                                SPTotalPrice = (double)SPTotalPrice,

                                IsMatched = isMatched
                            });
                        }
                    }
                }

                return JPComparedInvoiceList;
            }
            catch (Exception)
            {
                return [];
            }

        }

    }
}
