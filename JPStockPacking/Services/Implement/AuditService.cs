using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace JPStockPacking.Services.Implement
{
    public class AuditService(JPDbContext jPDbContext, SPDbContext sPDbContext) : IAuditService
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
                    JPInvoiceList = JPInvoiceList.Where(x => x.hinv.InvNo.ToLower() == invoiceNo.ToLower());
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
                            if (spexlot == null || spexlot.Count == 0)
                            {
                                continue;
                            }

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
                                MakeUnit = jpItem.MakeUnit!.Trim() ?? string.Empty,

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
                                    MakeUnit = jpItem.MakeUnit!.Trim() ?? string.Empty,

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

        public async Task<List<UnallocatedQuantityModel>> GetUnallocatedQuentityToStore(ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            DateTime FromDate = comparedInvoiceFilterModel.FromDate ?? DateTime.Now;
            DateTime ToDate = comparedInvoiceFilterModel.ToDate ?? DateTime.Now;

            if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
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

            var JPInvoiceList = from hinv in _jPDbContext.ExHminv
                                join dinv in _jPDbContext.ExDminv on hinv.MinvNo equals dinv.MinvNo into dinvGroup
                                from dinv in dinvGroup
                                join ord in _jPDbContext.OrdHorder on dinv.OrderNo equals ord.OrderNo into ordGroup
                                from ord in ordGroup.DefaultIfEmpty()
                                where hinv.InvSend
                                select new
                                {
                                    hinv,
                                    dinv,
                                };

            JPInvoiceList = JPInvoiceList.Where(x => x.hinv.Mdate >= FromDate && x.hinv.Mdate <= ToDate);

            if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
            {
                JPInvoiceList = JPInvoiceList.Where(x => x.dinv.OrderNo == comparedInvoiceFilterModel.OrderNo);
            }

            var JPExDminvLots = await JPInvoiceList.Where(x => !string.IsNullOrEmpty(x.dinv.LotNo)).Select(x => x.dinv.LotNo).Distinct().ToListAsync();

            List<UnallocatedQuantityModel> unallocatedQuantityModels = [];

            foreach (var lotNo in JPExDminvLots)
            {
                var cutoffDate = DateTime.Now.AddDays(-30);

                var lot = await _sPDbContext.Lot.Where(x => x.LotNo == lotNo).FirstOrDefaultAsync();
                if (lot != null)
                {
                    var spexlot = await _sPDbContext.Export.Where(x => x.LotNo == lot.LotNo).ToListAsync();
                    if (spexlot != null && spexlot.Count > 0)
                    {
                        var sumExTtQty = spexlot.Sum(x => x.TtQty);
                        var unallocatedQty = lot.ReturnedQty - sumExTtQty;
                        if (sumExTtQty > lot.ReturnedQty)
                        {

                        }
                    }
                }
            }

            return unallocatedQuantityModels;
        }
    }

    public class UnallocatedQuantityModel
    {
        public string LotNo { get; set; } = string.Empty;
        public decimal ExportedQty { get; set; } = 0;
        public decimal StoredQty { get; set; } = 0;
        public decimal MeltedQty { get; set; } = 0;
        public decimal UnallocatedQty { get; set; }
    }
}

