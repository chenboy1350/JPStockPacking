using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
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
                                     where hinv.InvSend
                                     select new
                                     {
                                         hinv,
                                         dinv,
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

                var JPExDminv = await JPInvoiceList.Where(w => !string.IsNullOrEmpty(w.dinv.OrderNo)).Select(x => new ExDminv
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
                    OrderCust = x.hinv.CusCode ?? string.Empty,
                }).ToListAsync();

                List<ComparedInvoiceModel> JPComparedInvoiceList = [];

                foreach (var jpItem in JPExDminv)
                {
                    if (!string.IsNullOrWhiteSpace(jpItem.SetNo) && string.IsNullOrWhiteSpace(jpItem.Article))
                    {
                        var lot = await _jPDbContext.OrdLotno.Where(x => x.SetNo1 == jpItem.SetNo.Trim() && x.OrderNo == jpItem.OrderNo).ToListAsync();
                        if (lot != null && lot.Count > 0)
                        {
                            var spexlot = await _sPDbContext.ExportDetail.Where(x => lot.Select(l => l.LotNo).Contains(x.LotNo) && x.IsActive).ToListAsync();
                            if (spexlot == null || spexlot.Count == 0)
                            {
                                continue;
                            }

                            string ListNo = lot.FirstOrDefault()!.GroupNo ?? string.Empty;

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
                                ListNo = ListNo,

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
                            var spexlot = await _sPDbContext.ExportDetail.Where(x => x.LotNo == jpItem.LotNo && x.IsActive).ToListAsync();

                            if (spexlot != null && spexlot.Count > 0)
                            {
                                string ListNo = lot.ListNo ?? string.Empty;

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
                                    ListNo = ListNo,

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

        public async Task<List<ComparedInvoiceModel>> GetConfirmedInvoice(string InvoiceNo)
        {
            var invoiceNo = InvoiceNo.Insert(2, "I").ToUpper();
            var confirmedInvoices = await _sPDbContext.ComparedInvoice
                .Where(x => x.InvoiceNo.ToLower() == invoiceNo)
                .Select(x => new ComparedInvoiceModel
                {
                    JPInvoiceNo = x.InvoiceNo,
                    JPOrderNo = x.OrderNo,
                    JPArticle = x.Article,
                    JPTtQty = (double)x.JpttQty,
                    JPPrice = (double)x.Jpprice,
                    JPTotalPrice = (double)x.JptotalPrice,
                    JPTotalSetTtQty = (double)x.JptotalSetTtQty,
                    SPTtQty = (double)x.SpttQty,
                    SPPrice = (double)x.Spprice,
                    SPTotalPrice = (double)x.SptotalPrice,
                    SPTotalSetTtQty = (double)x.SptotalSetTtQty,
                    IsMatched = x.IsMatched,
                    CustCode = x.CustCode,
                    MakeUnit = x.MakeUnit,
                    ListNo = x.ListNo
                })
                .ToListAsync();

            return confirmedInvoices;
        }

        public async Task<BaseResponseModel> GetIsMarked(string InvoiceNo)
        {
            if (InvoiceNo.Length < 2)
            {
                return new BaseResponseModel { IsSuccess = false };
            }

            var invoiceNo = InvoiceNo.Insert(2, "I").ToUpper();
            var isMarked = _sPDbContext.ComparedInvoice.Any(x => x.InvoiceNo == invoiceNo);
            if (isMarked)
            {
                return new BaseResponseModel { IsSuccess = true};
            }
            else
            {
                return new BaseResponseModel { IsSuccess = false };
            }
        }

        public async Task<BaseResponseModel> MarkInvoiceAsRead(string InvoiceNo, int userId)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var invoiceNo = InvoiceNo.Insert(2, "I").ToUpper();
                List<ComparedInvoiceModel> comparedInvoiceModels = await GetFilteredInvoice(new ComparedInvoiceFilterModel { InvoiceNo = InvoiceNo });
                var comparedInvoices = _sPDbContext.ComparedInvoice.Where(x => x.InvoiceNo == invoiceNo).ToList();

                BaseResponseModel responseModel = new();

                if (comparedInvoices.Count == 0 && comparedInvoiceModels.Count > 0)
                {
                    var result = comparedInvoiceModels.Select(x => new ComparedInvoice
                    {
                        InvoiceNo = x.JPInvoiceNo,
                        OrderNo = x.JPOrderNo,
                        Article = x.JPArticle,
                        JpttQty = x.JPTtQty,
                        Jpprice = x.JPPrice,
                        JptotalPrice = x.JPTotalPrice,
                        JptotalSetTtQty = x.JPTotalSetTtQty,
                        SpttQty = x.SPTtQty,
                        Spprice = x.SPPrice,
                        SptotalPrice = x.SPTotalPrice,
                        SptotalSetTtQty = x.SPTotalSetTtQty,
                        IsMatched = x.IsMatched,
                        CustCode = x.CustCode,
                        MakeUnit = x.MakeUnit,
                        ListNo = x.ListNo,
                        IsActive = true,
                        CreateDate = DateTime.UtcNow,
                        CreateBy = userId,
                        UpdateDate = DateTime.UtcNow,
                        UpdateBy = userId
                    }).ToList();

                    await _sPDbContext.ComparedInvoice.AddRangeAsync(result);
                    await _sPDbContext.SaveChangesAsync();

                    responseModel = new BaseResponseModel { IsSuccess = true, Message = "Invoice marked as read successfully." };
                }
                else if (comparedInvoices.Count == 0 && comparedInvoiceModels.Count == 0)
                {
                    responseModel = new BaseResponseModel { IsSuccess = false, Message = "No invoice data found to mark as read." };
                }
                else
                {
                    responseModel = new BaseResponseModel { IsSuccess = true, Message = "Invoice already marked as read." };
                }

                await transaction.CommitAsync();
                return responseModel;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel { IsSuccess = false, Message = "Failed to mark invoice as read." };
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

            var cutoffDate = DateTime.Now.AddDays(-30);

            foreach (var lotNo in JPExDminvLots)
            {
                var lot = await (from l in _sPDbContext.Lot
                                 join o in _sPDbContext.Order on l.OrderNo equals o.OrderNo
                                 where l.LotNo == lotNo
                                 select new { l, o }).FirstOrDefaultAsync();

                if (lot == null) continue;

                // กรองตาม IsSample
                if (comparedInvoiceFilterModel.IsSample != lot.o.IsSample) continue;
                
                var lotData = lot.l;

                // แสดงเฉพาะ lot ที่ยังมี Unallocated คงเหลือ
                if (lotData.Unallocated > 0)
                {
                    // ดึงข้อมูล Export ตามเงื่อนไขวันที่
                    var exportList = await _sPDbContext.ExportDetail
                        .Where(x => x.LotNo == lotNo && x.IsActive &&
                                   (comparedInvoiceFilterModel.IsOver30Days ? x.CreateDate < cutoffDate : x.CreateDate >= cutoffDate))
                        .ToListAsync();

                    if (exportList.Count > 0)
                    {
                        // ดึงข้อมูล Store
                        var storeList = await _sPDbContext.Store
                            .Where(x => x.LotNo == lotNo && x.IsActive)
                            .ToListAsync();

                        // ดึงข้อมูล Melt
                        var meltList = await _sPDbContext.Melt
                            .Where(x => x.LotNo == lotNo && x.IsActive)
                            .ToListAsync();

                        // คำนวณจำนวนแต่ละประเภท
                        decimal exportedQty = exportList.Sum(x => x.TtQty);
                        decimal storedQty = storeList.Sum(x => x.TtQty);
                        decimal meltedQty = meltList.Sum(x => x.TtQty);

                        // เพิ่มข้อมูลลง list เฉพาะที่มี Export เกิดขึ้นในช่วง 30 วัน
                        if (exportList.Count > 0)
                        {
                            unallocatedQuantityModels.Add(new UnallocatedQuantityModel
                            {
                                LotNo = lotNo ?? string.Empty,
                                OrderNo = lotData.OrderNo ?? string.Empty,
                                ListNo = lotData.ListNo ?? string.Empty,
                                Article = lotData.Article ?? string.Empty,
                                ExportedQty = exportedQty,
                                StoredQty = storedQty,
                                MeltedQty = meltedQty,
                                UnallocatedQty = lotData.Unallocated ?? 0
                            });
                        }
                    }
                }

            }

            return unallocatedQuantityModels;
        }

        public async Task<List<SendLostCheckModel>> GetSendLostCheckList(ComparedInvoiceFilterModel comparedInvoiceFilterModel)
        {
            DateTime FromDate = comparedInvoiceFilterModel.FromDate ?? DateTime.Now;
            DateTime ToDate = comparedInvoiceFilterModel.ToDate ?? DateTime.Now;

            if (comparedInvoiceFilterModel.FromDate != null)
            {
                FromDate = comparedInvoiceFilterModel.FromDate.Value;
            }

            if (comparedInvoiceFilterModel.ToDate != null)
            {
                ToDate = comparedInvoiceFilterModel.ToDate.Value;
            }
            
            // Adjust time to cover the whole day
            FromDate = FromDate.Date;
            ToDate = ToDate.Date.AddDays(1).AddSeconds(-1);

            var query = from sld in _sPDbContext.SendLostDetail
                        join l in _sPDbContext.Lot on sld.LotNo equals l.LotNo
                        join o in _sPDbContext.Order on l.OrderNo equals o.OrderNo
                        where sld.IsActive
                        select new { sld, l, o };

            if (!string.IsNullOrEmpty(comparedInvoiceFilterModel.OrderNo))
            {
                query = query.Where(x => x.o.OrderNo == comparedInvoiceFilterModel.OrderNo);
            }
            
            var result = await query.Select(x => new SendLostCheckModel
            {
                Customer = x.o.CustCode ?? string.Empty,
                OrderNo = x.o.OrderNo,
                LotNo = x.l.LotNo,
                ListNo = x.l.ListNo,
                Article = x.l.Article ?? string.Empty,
                Qty = x.sld.TtQty,
                Wg = x.sld.TtWg
            }).ToListAsync();

            return result;
        }
    }

    public class UnallocatedQuantityModel
    {
        public string OrderNo { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string ListNo { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public decimal ExportedQty { get; set; } = 0;
        public decimal StoredQty { get; set; } = 0;
        public decimal MeltedQty { get; set; } = 0;
        public decimal UnallocatedQty { get; set; } = 0;
    }
}

