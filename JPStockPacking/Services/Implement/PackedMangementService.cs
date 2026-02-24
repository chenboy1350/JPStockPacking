using Dapper;
using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using static JPStockPacking.Services.Helper.Enum;

namespace JPStockPacking.Services.Implement
{
    public class PackedMangementService(JPDbContext jPDbContext, SPDbContext sPDbContext, IPISService pISService, IConfiguration configuration) : IPackedMangementService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IPISService _pISService = pISService;
        private readonly IConfiguration _configuration = configuration;
        private static readonly string[] sendType = ["KS", "KM"];

        public async Task<List<OrderToStoreModel>> GetOrderToStoreAsync(string orderNo)
        {
            await UpdateArticleAsync(orderNo);

            var percentage = decimal.TryParse(_configuration["SendToStoreSettings:Percentage"], out var p) ? p : 0;

            var dictStore = await (
                from s in _sPDbContext.Store
                join l in _sPDbContext.Lot on s.LotNo equals l.LotNo
                where s.IsActive && l.OrderNo == orderNo
                group s by s.LotNo into g
                select new
                {
                    LotNo = g.Key,
                    StoreFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    StoreFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    StoreDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    StoreDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasStoreDraft = g.Any(x => string.IsNullOrEmpty(x.Doc)),
                    HasStoreSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).ToDictionaryAsync(x => x.LotNo);

            var dictMelt = await (
                from s in _sPDbContext.Melt
                join l in _sPDbContext.Lot on s.LotNo equals l.LotNo
                where s.IsActive && l.OrderNo == orderNo
                group s by s.LotNo into g
                select new
                {
                    LotNo = g.Key,
                    g.First().BreakDescriptionId,
                    MeltFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    MeltFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    MeltDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    MeltDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasMeltDraft = g.Any(x => string.IsNullOrEmpty(x.Doc)),
                    HasMeltSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).ToDictionaryAsync(x => x.LotNo);

            var dictLost = await (
                from s in _sPDbContext.SendLostDetail
                join l in _sPDbContext.Lot on s.LotNo equals l.LotNo
                where s.IsActive && l.OrderNo == orderNo
                group s by s.LotNo into g
                select new
                {
                    LotNo = g.Key,
                    LostFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    LostFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    LostDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    LostDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasLostDraft = g.Any(x => string.IsNullOrEmpty(x.Doc)),
                    HasLostSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).ToDictionaryAsync(x => x.LotNo);

            var dictExport = await (
                from s in _sPDbContext.ExportDetail
                join l in _sPDbContext.Lot on s.LotNo equals l.LotNo
                where s.IsActive && l.OrderNo == orderNo
                group s by s.LotNo into g
                select new
                {
                    LotNo = g.Key,
                    ExportFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ExportFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    ExportDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ExportDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasExportDraft = g.Any(x => string.IsNullOrEmpty(x.Doc)),
                    HasExportSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).ToDictionaryAsync(x => x.LotNo);

            var dictShowroom = await (
                from s in _sPDbContext.SendShowroomDetail
                join l in _sPDbContext.Lot on s.LotNo equals l.LotNo
                where s.IsActive && l.OrderNo == orderNo
                group s by s.LotNo into g
                select new
                {
                    LotNo = g.Key,
                    ShowroomFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ShowroomFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    ShowroomDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ShowroomDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasShowroomSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).ToDictionaryAsync(x => x.LotNo);

            var baseData = await (
                from ord in _sPDbContext.Order
                join lot in _sPDbContext.Lot on ord.OrderNo equals lot.OrderNo
                join sqd in _sPDbContext.SendQtyToPackDetail on lot.LotNo equals sqd.LotNo into sqdj
                from sqd in sqdj.DefaultIfEmpty()
                join rec in (
                    from r in _sPDbContext.Received
                    group r by r.LotNo into g
                    select new
                    {
                        LotNo = g.Key,
                        TtQty = g.Sum(x => (decimal?)x.TtQty),
                        TtWg = g.Sum(x => (double?)x.TtWg)
                    }
                ) on lot.LotNo equals rec.LotNo into revj
                from rec in revj.DefaultIfEmpty()
                where ord.OrderNo == orderNo
                && (sqd == null || sqd.IsActive)
                select new { ord, lot, sqd, rec }
            ).ToListAsync();

            var result = baseData.Select(x =>
            {
                dictStore.TryGetValue(x.lot.LotNo, out var st);
                dictMelt.TryGetValue(x.lot.LotNo, out var ml);
                dictLost.TryGetValue(x.lot.LotNo, out var ls);
                dictExport.TryGetValue(x.lot.LotNo, out var ex);
                dictShowroom.TryGetValue(x.lot.LotNo, out var sr);

                return new OrderToStoreModel
                {
                    OrderNo = x.ord.OrderNo,
                    CustCode = x.ord.CustCode ?? string.Empty,
                    LotNo = x.lot.LotNo,
                    ListNo = x.lot.ListNo,
                    Article = x.lot.Article ?? string.Empty,
                    TtQty = x.lot.TtQty ?? 0,
                    TtWg = x.rec?.TtWg ?? 0,
                    Si = x.lot.Si ?? 0,
                    SendPack_Qty = x.rec?.TtQty ?? 0,
                    SendToPack_Qty = x.sqd?.TtQty ?? 0,
                    Packed_Qty = x.lot.ReturnedQty ?? 0,
                    Percentage = percentage,

                    Store_Qty = st?.StoreDraftQty ?? 0,
                    Store_Wg = st?.StoreDraftWg ?? 0,
                    IsStoreSended = st?.HasStoreSent ?? false,
                    Store_FixedQty = st?.StoreFixedQty ?? 0,
                    Store_FixedWg = st?.StoreFixedWg ?? 0,

                    Melt_Qty = ml?.MeltDraftQty ?? 0,
                    Melt_Wg = ml?.MeltDraftWg ?? 0,
                    IsMeltSended = ml?.HasMeltSent ?? false,
                    Melt_FixedQty = ml?.MeltFixedQty ?? 0,
                    Melt_FixedWg = ml?.MeltFixedWg ?? 0,
                    BreakDescriptionId = ml?.BreakDescriptionId ?? 0,

                    Lost_Qty = ls?.LostDraftQty ?? 0,
                    Lost_Wg = ls?.LostDraftWg ?? 0,
                    IsLostSended = ls?.HasLostSent ?? false,
                    Lost_FixedQty = ls?.LostFixedQty ?? 0,
                    Lost_FixedWg = ls?.LostFixedWg ?? 0,

                    Export_Qty = ex?.ExportDraftQty ?? 0,
                    Export_Wg = ex?.ExportDraftWg ?? 0,
                    IsExportSended = ex?.HasExportSent ?? false,
                    Export_FixedQty = ex?.ExportFixedQty ?? 0,
                    Export_FixedWg = ex?.ExportFixedWg ?? 0,

                    Showroom_Qty = sr?.ShowroomDraftQty ?? 0,
                    Showroom_Wg = sr?.ShowroomDraftWg ?? 0,
                    IsShowroomSended = sr?.HasShowroomSent ?? false,
                    Showroom_FixedQty = sr?.ShowroomFixedQty ?? 0,
                    Showroom_FixedWg = sr?.ShowroomFixedWg ?? 0,
                };
            }).ToList();

            return result;
        }


        public async Task<OrderToStoreModel?> GetOrderToStoreByLotAsync(string lotNo)
        {
            var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == lotNo);
            if (lot == null) return null;

            var percentage = decimal.TryParse(_configuration["SendToStoreSettings:Percentage"], out var p) ? p : 0;

            var storeData = await (
                from s in _sPDbContext.Store
                where s.IsActive && s.LotNo == lotNo
                group s by s.LotNo into g
                select new
                {
                    StoreFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    StoreFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    StoreDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    StoreDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasStoreSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).FirstOrDefaultAsync();

            var meltData = await (
                from s in _sPDbContext.Melt
                where s.IsActive && s.LotNo == lotNo
                group s by s.LotNo into g
                select new
                {
                    g.First().BreakDescriptionId,
                    MeltFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    MeltFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    MeltDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    MeltDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasMeltSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).FirstOrDefaultAsync();

            var lostData = await (
                from s in _sPDbContext.SendLostDetail
                where s.IsActive && s.LotNo == lotNo
                group s by s.LotNo into g
                select new
                {
                    LostFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    LostFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    LostDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    LostDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasLostSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).FirstOrDefaultAsync();

            var exportData = await (
                from s in _sPDbContext.ExportDetail
                where s.IsActive && s.LotNo == lotNo
                group s by s.LotNo into g
                select new
                {
                    ExportFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ExportFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    ExportDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ExportDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasExportSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).FirstOrDefaultAsync();

            var showroomData = await (
                from s in _sPDbContext.SendShowroomDetail
                where s.IsActive && s.LotNo == lotNo
                group s by s.LotNo into g
                select new
                {
                    ShowroomFixedQty = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ShowroomFixedWg = g.Where(x => !string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    ShowroomDraftQty = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (decimal?)x.TtQty) ?? 0,
                    ShowroomDraftWg = g.Where(x => string.IsNullOrEmpty(x.Doc)).Sum(x => (double?)x.TtWg) ?? 0,
                    HasShowroomSent = g.Any(x => !string.IsNullOrEmpty(x.Doc))
                }
            ).FirstOrDefaultAsync();

            var baseRow = await (
                from ord in _sPDbContext.Order
                join l in _sPDbContext.Lot on ord.OrderNo equals l.OrderNo
                join sqd in _sPDbContext.SendQtyToPackDetail on l.LotNo equals sqd.LotNo into sqdj
                from sqd in sqdj.DefaultIfEmpty()
                join rec in (
                    from r in _sPDbContext.Received
                    where r.LotNo == lotNo
                    group r by r.LotNo into g
                    select new
                    {
                        LotNo = g.Key,
                        TtQty = g.Sum(x => (decimal?)x.TtQty),
                        TtWg = g.Sum(x => (double?)x.TtWg)
                    }
                ) on l.LotNo equals rec.LotNo into revj
                from rec in revj.DefaultIfEmpty()
                where l.LotNo == lotNo
                && (sqd == null || sqd.IsActive)
                select new { ord, lot = l, sqd, rec }
            ).FirstOrDefaultAsync();

            if (baseRow == null) return null;

            return new OrderToStoreModel
            {
                OrderNo = baseRow.ord.OrderNo,
                CustCode = baseRow.ord.CustCode ?? string.Empty,
                LotNo = baseRow.lot.LotNo,
                ListNo = baseRow.lot.ListNo,
                Article = baseRow.lot.Article ?? string.Empty,
                TtQty = baseRow.lot.TtQty ?? 0,
                TtWg = baseRow.rec?.TtWg ?? 0,
                Si = baseRow.lot.Si ?? 0,
                SendPack_Qty = baseRow.rec?.TtQty ?? 0,
                SendToPack_Qty = baseRow.sqd?.TtQty ?? 0,
                Packed_Qty = baseRow.lot.ReturnedQty ?? 0,
                Percentage = percentage,

                Store_Qty = storeData?.StoreDraftQty ?? 0,
                Store_Wg = storeData?.StoreDraftWg ?? 0,
                IsStoreSended = storeData?.HasStoreSent ?? false,
                Store_FixedQty = storeData?.StoreFixedQty ?? 0,
                Store_FixedWg = storeData?.StoreFixedWg ?? 0,

                Melt_Qty = meltData?.MeltDraftQty ?? 0,
                Melt_Wg = meltData?.MeltDraftWg ?? 0,
                IsMeltSended = meltData?.HasMeltSent ?? false,
                Melt_FixedQty = meltData?.MeltFixedQty ?? 0,
                Melt_FixedWg = meltData?.MeltFixedWg ?? 0,
                BreakDescriptionId = meltData?.BreakDescriptionId ?? 0,

                Lost_Qty = lostData?.LostDraftQty ?? 0,
                Lost_Wg = lostData?.LostDraftWg ?? 0,
                IsLostSended = lostData?.HasLostSent ?? false,
                Lost_FixedQty = lostData?.LostFixedQty ?? 0,
                Lost_FixedWg = lostData?.LostFixedWg ?? 0,

                Export_Qty = exportData?.ExportDraftQty ?? 0,
                Export_Wg = exportData?.ExportDraftWg ?? 0,
                IsExportSended = exportData?.HasExportSent ?? false,
                Export_FixedQty = exportData?.ExportFixedQty ?? 0,
                Export_FixedWg = exportData?.ExportFixedWg ?? 0,

                Showroom_Qty = showroomData?.ShowroomDraftQty ?? 0,
                Showroom_Wg = showroomData?.ShowroomDraftWg ?? 0,
                IsShowroomSended = showroomData?.HasShowroomSent ?? false,
                Showroom_FixedQty = showroomData?.ShowroomFixedQty ?? 0,
                Showroom_FixedWg = showroomData?.ShowroomFixedWg ?? 0,
            };
        }

        public async Task<BaseResponseModel> SendStockAsync(SendStockInput input)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == input.LotNo);
                if (lot == null)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponseModel
                    {
                        IsSuccess = false,
                        Message = "ไม่พบ LotNo ที่ระบุ"
                    };
                }

                var receivedList = await _sPDbContext.Received
                    .Where(x => x.LotNo == input.LotNo)
                    .OrderByDescending(o => o.CreateDate)
                    .ToListAsync();

                if (receivedList == null || receivedList.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponseModel
                    {
                        IsSuccess = false,
                        Message = "ไม่พบข้อมูลใบรับ"
                    };
                }

                var received = receivedList.FirstOrDefault(r =>
                    !_sPDbContext.Store.Any(s => s.BillNumber == r.BillNumber && s.Doc != string.Empty) &&
                    !_sPDbContext.Melt.Any(m => m.BillNumber == r.BillNumber && m.Doc != string.Empty)
                );

                if (received == null)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponseModel
                    {
                        IsSuccess = false,
                        Message = "ไม่พบ JobBarcode ที่ยังสามารถส่งได้"
                    };
                }

                var billNumber = received.BillNumber;

                // ===== STORE =====
                var store = await _sPDbContext.Store
                    .FirstOrDefaultAsync(x => x.LotNo == input.LotNo && x.BillNumber == billNumber);

                if (store == null && input.KsQty > 0)
                {
                    _sPDbContext.Store.Add(new Store
                    {
                        LotNo = input.LotNo,
                        BillNumber = billNumber,
                        Doc = string.Empty,
                        TtQty = input.KsQty,
                        TtWg = (double)input.KsWg,
                        IsSended = false,
                        IsStored = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        CreateBy = int.TryParse(input.UserId, out int userId) ? userId : null,
                        UpdateDate = DateTime.Now,
                        UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null
                    });
                }
                else if (input.KsQty == 0)
                {
                    await _sPDbContext.Store
                        .Where(x => x.LotNo == input.LotNo && x.BillNumber == billNumber)
                        .ExecuteDeleteAsync();
                }
                else if (store != null && store.TtQty != input.KsQty)
                {
                    store.TtQty = input.KsQty;
                    store.TtWg = (double)input.KsWg;
                    store.UpdateDate = DateTime.Now;
                }

                // ===== MELT =====
                var melt = await _sPDbContext.Melt
                    .FirstOrDefaultAsync(x => x.LotNo == input.LotNo && x.BillNumber == billNumber);

                if (melt == null && input.KmQty > 0)
                {
                    _sPDbContext.Melt.Add(new Melt
                    {
                        LotNo = input.LotNo,
                        BillNumber = billNumber,
                        Doc = string.Empty,
                        TtQty = input.KmQty,
                        TtWg = (double)input.KmWg,
                        BreakDescriptionId = input.KmDes,
                        IsSended = false,
                        IsMelted = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        CreateBy = int.TryParse(input.UserId, out int userId) ? userId : null,
                        UpdateDate = DateTime.Now,
                        UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null
                    });
                }
                else if (input.KmQty == 0)
                {
                    await _sPDbContext.Melt
                        .Where(x => x.LotNo == input.LotNo && x.BillNumber == billNumber)
                        .ExecuteDeleteAsync();
                }
                else if (melt != null && melt.TtQty != input.KmQty)
                {
                    melt.TtQty = input.KmQty;
                    melt.TtWg = (double)input.KmWg;
                    melt.BreakDescriptionId = input.KmDes;
                    melt.UpdateDate = DateTime.Now;
                }

                // ===== LOST =====
                var sendLost = await _sPDbContext.SendLostDetail.FirstOrDefaultAsync(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc));

                if (sendLost == null && input.KlQty > 0)
                {
                    _sPDbContext.SendLostDetail.Add(new SendLostDetail
                    {
                        LotNo = input.LotNo,
                        Doc = string.Empty,
                        TtQty = input.KlQty,
                        TtWg = (double)input.KlWg,
                        IsSended = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateBy = int.TryParse(input.UserId, out int userId) ? userId : null,
                        UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null
                    });
                }
                else if (input.KlQty == 0)
                {
                    await _sPDbContext.SendLostDetail
                        .Where(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc))
                        .ExecuteDeleteAsync();
                }
                else if (sendLost != null && sendLost.TtQty != input.KlQty)
                {
                    sendLost.TtQty = input.KlQty;
                    sendLost.TtWg = (double)input.KlWg;
                    sendLost.UpdateDate = DateTime.Now;
                    sendLost.UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null;
                }

                // ===== EXPORT =====
                var export = await _sPDbContext.ExportDetail.FirstOrDefaultAsync(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc));

                if (export == null && input.KxQty > 0)
                {
                    _sPDbContext.ExportDetail.Add(new ExportDetail
                    {
                        LotNo = input.LotNo,
                        Doc = string.Empty,
                        TtQty = input.KxQty,
                        TtWg = (double)input.KxWg,
                        Approver = input.Approver,
                        IsOverQuota = input.Approver != 0,
                        IsSended = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateBy = int.TryParse(input.UserId, out int userId) ? userId : null,
                        UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null
                    });
                }
                else if (input.KxQty == 0)
                {
                    await _sPDbContext.ExportDetail
                        .Where(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc))
                        .ExecuteDeleteAsync();
                }
                else if (export != null && export.TtQty != input.KxQty)
                {
                    export.TtQty = input.KxQty;
                    export.TtWg = (double)input.KxWg;
                    export.Approver = input.Approver;
                    export.IsOverQuota = input.Approver != 0;
                    export.UpdateDate = DateTime.Now;
                    export.UpdateBy = int.TryParse(input.UserId, out int updateBy) ? updateBy : null;
                }

                // ===== SHOWROOM (KR) =====
                var showroom = await _sPDbContext.SendShowroomDetail.FirstOrDefaultAsync(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc));

                if (showroom == null && input.KrQty > 0)
                {
                    _sPDbContext.SendShowroomDetail.Add(new SendShowroomDetail
                    {
                        LotNo = input.LotNo,
                        Doc = string.Empty,
                        TtQty = input.KrQty,
                        TtWg = (double)input.KrWg,
                        IsSended = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateBy = int.TryParse(input.UserId, out int srUserId) ? srUserId : null,
                        UpdateBy = int.TryParse(input.UserId, out int srUpdateBy) ? srUpdateBy : null
                    });
                }
                else if (input.KrQty == 0)
                {
                    await _sPDbContext.SendShowroomDetail
                        .Where(x => x.LotNo == input.LotNo && string.IsNullOrEmpty(x.Doc))
                        .ExecuteDeleteAsync();
                }
                else if (showroom != null && showroom.TtQty != input.KrQty)
                {
                    showroom.TtQty = input.KrQty;
                    showroom.TtWg = (double)input.KrWg;
                    showroom.UpdateDate = DateTime.Now;
                    showroom.UpdateBy = int.TryParse(input.UserId, out int srUpdateBy) ? srUpdateBy : null;
                }

                lot.Unallocated = input.Unallocated;
                lot.UpdateDate = DateTime.Now;

                if (input.Unallocated <= 0)
                {
                    lot.IsSuccess = true;
                    lot.UpdateDate = DateTime.Now;

                    await UpdateOrderSuccessAsync(lot.OrderNo);
                }

                await _sPDbContext.SaveChangesAsync();

                input.BillNumber = billNumber;
                BaseResponseModel res = await ProcessSendStockAsync(input);

                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"ส่งสต็อกสำเร็จ {res.Message}"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        private async Task<BaseResponseModel> ProcessSendStockAsync(SendStockInput input)
        {
            var existingDocs = await _jPDbContext.JobBillSendStock
                .Where(s => s.Billnumber == input.BillNumber &&
                            s.SizeNoOrd == input.SizeNoOrd &&
                            s.ItemSend == input.ItemSend && sendType.Contains(s.SendType))
                .Select(s => s.Doc)
                .ToListAsync();

            if (existingDocs.Any(doc => !string.IsNullOrEmpty(doc))) return new BaseResponseModel { IsSuccess = false, Message = "JobBarcode นี้มีการยืนยันส่งแล้ว ไม่สามารถแก้ไขได้" };

            await UpsertStockAsync(input, "KS", input.KsQty, input.KsWg);
            await UpsertStockAsync(input, "KM", input.KmQty, input.KmWg);

            return new BaseResponseModel
            {
                IsSuccess = true,
                Message = "ประมวลผลส่งสต็อกสำเร็จ"
            };
        }

        private async Task UpsertStockAsync(SendStockInput input, string sendType, decimal qty, decimal wg)
        {
            using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var record = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(s =>
                    s.Billnumber == input.BillNumber &&
                    s.SizeNoOrd == input.SizeNoOrd &&
                    s.ItemSend == input.ItemSend &&
                    s.SendType == sendType);

                if (record == null && qty > 0)
                {
                    _jPDbContext.JobBillSendStock.Add(new JobBillSendStock
                    {
                        Billnumber = input.BillNumber,
                        SendType = sendType,
                        Ttqty = qty,
                        Ttwg = wg,
                        SizeNoOrd = input.SizeNoOrd,
                        ItemSend = input.ItemSend,
                        ReturnFound = input.ReturnFound,
                        Userid = input.UserId,
                        Sendpack = true,
                        Sdate = DateTime.Now
                    });
                }
                else if (record != null)
                {
                    if (qty == 0)
                    {
                        _jPDbContext.JobBillSendStock.Remove(record);
                    }
                    else
                    {
                        if (qty != record.Ttqty)
                        {
                            record.Ttqty = qty;
                            record.Ttwg = wg;
                            record.ReturnFound = input.ReturnFound;
                            record.Userid = input.UserId;
                        }
                    }
                }

                await _jPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
            }
        }

        private async Task<List<TempPack>> GetStoreAsync(string[] lotNos)
        {
            List<TempPack> result = await (
                from h in _jPDbContext.JobBillSendStock
                join a in _jPDbContext.JobBill on h.Billnumber equals a.Billnumber into gjA
                from a in gjA.DefaultIfEmpty()
                join d in _jPDbContext.JobDetail on a.JobBarcode equals d.JobBarcode
                join i in _jPDbContext.JobHead
                    on new { d.DocNo, d.EmpCode } equals new { i.DocNo, i.EmpCode }
                join b in _jPDbContext.OrdLotno
                    on new { d.ListNo, d.OrderNo } equals new { b.ListNo, b.OrderNo }
                join c in _jPDbContext.CpriceSale on a.Barcode equals c.Barcode
                join f in _jPDbContext.OrdLotno on a.Lotno equals f.LotNo
                join g in _jPDbContext.JobBillsize
                    on a.Billnumber equals g.Billnumber into gjG
                from g in gjG.Where(x => a.SizeNoOrd == true).DefaultIfEmpty()
                where EF.Functions.Like(h.SendType.Substring(1, 1), "S") && (h.Doc == "" || h.Doc == null)
                orderby h.MdateSend, a.Lotno, a.Num
                select new TempPack
                {
                    Name = (from t in _jPDbContext.TempProfile where t.EmpCode == a.EmpCode select (t.TitleName ?? "") + " " + (t.Name ?? "")).FirstOrDefault() ?? string.Empty,
                    EmpCode = a.EmpCode,
                    LotNo = a.Lotno,
                    JobBarcode = a.JobBarcode,
                    Article = a.Article,
                    Barcode = a.Barcode,
                    ArtCode = a.ArtCode,
                    FnCode = a.FnCode,
                    DocNo = a.DocNo,
                    ListNo = a.ListNo,
                    BillDate = a.BillDate,
                    CheckBill = a.CheckBill,
                    Username = a.UserName,
                    Mdate = a.MDate,
                    SendPack = a.SendPack,
                    PackDoc = a.PackDoc,
                    Num = a.Num,
                    SendDate = a.SendDate.GetValueOrDefault(),
                    SizeNoOrd = a.SizeNoOrd,
                    Wg_Over = a.WgOver,
                    BillNumber = a.Billnumber,

                    SendType = h.SendType ?? string.Empty,
                    OkTtl = h.Ttqty,
                    OkWg = h.Ttwg,

                    OkQ1 = h.Q1,
                    OkQ2 = h.Q2,
                    OkQ3 = h.Q3,
                    OkQ4 = h.Q4,
                    OkQ5 = h.Q5,
                    OkQ6 = h.Q6,
                    OkQ7 = h.Q7,
                    OkQ8 = h.Q8,
                    OkQ9 = h.Q9,
                    OkQ10 = h.Q10,
                    OkQ11 = h.Q11,
                    OkQ12 = h.Q12,

                    SUser = h.Userid ?? string.Empty,
                    SizeZone = f.SizeZone ?? string.Empty,
                    ChkSize = f.ChkSize ?? false,
                    S1 = g.S1,
                    S2 = g.S2,
                    S3 = g.S3,
                    S4 = g.S4,
                    S5 = g.S5,
                    S6 = g.S6,
                    S7 = g.S7,
                    S8 = g.S8,
                    S9 = g.S9,
                    S10 = g.S10,
                    S11 = g.S11,
                    S12 = g.S12,

                    Unit = b.Unit,
                    OrderNo = b.OrderNo,
                    ListGem = c.ListGem,
                    FinishingTH = c.TdesFn,
                    WgActual = c.WgActual,
                    CustCode = d.CustCode,

                    NumSend = h.Numsend,
                    MdateSend = h.MdateSend,
                    COther1 = i.COther1,
                    CreatedBy = a.UserName
                }).ToListAsync();

            List<TempPack> filteredResult = [.. result.Where(r => lotNos.Contains(r.LotNo))];

            foreach (var item in filteredResult)
            {
                if (item.ChkSize && string.IsNullOrEmpty(item.S1) && string.IsNullOrEmpty(item.S2)
                    && string.IsNullOrEmpty(item.S3) && string.IsNullOrEmpty(item.S4)
                    && string.IsNullOrEmpty(item.S5) && string.IsNullOrEmpty(item.S6)
                    && string.IsNullOrEmpty(item.S7) && string.IsNullOrEmpty(item.S8)
                    && string.IsNullOrEmpty(item.S9) && string.IsNullOrEmpty(item.S10)
                    && string.IsNullOrEmpty(item.S11) && string.IsNullOrEmpty(item.S12))
                {
                    item.S1 = "52";
                    item.OkQ1 = item.OkTtl;
                }
            }

            return filteredResult;
        }

        private async Task<List<TempPack>> GetMeltAsync(string[] lotNos)
        {
            List<TempPack> result = await (
                from h in _jPDbContext.JobBillSendStock
                join a in _jPDbContext.JobBill on h.Billnumber equals a.Billnumber into gjA
                from a in gjA.DefaultIfEmpty()

                join d in _jPDbContext.JobDetail on a.JobBarcode equals d.JobBarcode
                join i in _jPDbContext.JobHead
                    on new { d.DocNo, d.EmpCode } equals new { i.DocNo, i.EmpCode }
                join b in _jPDbContext.OrdLotno
                    on new { d.ListNo, d.OrderNo } equals new { b.ListNo, b.OrderNo }
                join c in _jPDbContext.CpriceSale on a.Barcode equals c.Barcode
                join f in _jPDbContext.OrdLotno on a.Lotno equals f.LotNo

                join g in _jPDbContext.JobBillsize
                    on a.Billnumber equals g.Billnumber into gjG
                from g in gjG
                    .Where(x => a.SizeNoOrd == true)
                    .DefaultIfEmpty()

                where h.SendType.Substring(1, 1) == "M" && (h.Doc == null || h.Doc == "")

                orderby h.MdateSend, a.Lotno, a.Num

                select new TempPack
                {
                    Name = (from t in _jPDbContext.TempProfile where t.EmpCode == a.EmpCode select (t.TitleName ?? "") + " " + (t.Name ?? "")).FirstOrDefault() ?? string.Empty,
                    EmpCode = a.EmpCode,
                    LotNo = a.Lotno ?? string.Empty,
                    JobBarcode = a.JobBarcode ?? string.Empty,
                    Article = a.Article ?? string.Empty,
                    Barcode = a.Barcode ?? string.Empty,
                    ArtCode = a.ArtCode ?? string.Empty,
                    FnCode = a.FnCode ?? string.Empty,
                    DocNo = a.DocNo ?? string.Empty,
                    ListNo = a.ListNo,
                    BillDate = a.BillDate,
                    CheckBill = a.CheckBill,
                    Username = a.UserName ?? string.Empty,
                    Mdate = a.MDate,
                    SendPack = a.SendPack,
                    PackDoc = a.PackDoc ?? string.Empty,
                    Num = a.Num,
                    SendDate = a.SendDate.GetValueOrDefault(),
                    SizeNoOrd = a.SizeNoOrd,
                    Wg_Over = a.WgOver,
                    BillNumber = a.Billnumber,

                    SendType = h.SendType ?? string.Empty,
                    OkTtl = h.Ttqty,
                    OkWg = h.Ttwg,

                    OkQ1 = h.Q1,
                    OkQ2 = h.Q2,
                    OkQ3 = h.Q3,
                    OkQ4 = h.Q4,
                    OkQ5 = h.Q5,
                    OkQ6 = h.Q6,
                    OkQ7 = h.Q7,
                    OkQ8 = h.Q8,
                    OkQ9 = h.Q9,
                    OkQ10 = h.Q10,
                    OkQ11 = h.Q11,
                    OkQ12 = h.Q12,

                    SUser = h.Userid ?? string.Empty,
                    SizeZone = f.SizeZone ?? string.Empty,
                    ChkSize = f.ChkSize ?? false,
                    S1 = g.S1,
                    S2 = g.S2,
                    S3 = g.S3,
                    S4 = g.S4,
                    S5 = g.S5,
                    S6 = g.S6,
                    S7 = g.S7,
                    S8 = g.S8,
                    S9 = g.S9,
                    S10 = g.S10,
                    S11 = g.S11,
                    S12 = g.S12,

                    Unit = b.Unit ?? string.Empty,
                    OrderNo = b.OrderNo ?? string.Empty,
                    ListGem = c.ListGem ?? string.Empty,
                    FinishingTH = c.TdesFn ?? string.Empty,
                    WgActual = c.WgActual,
                    CustCode = d.CustCode ?? string.Empty,

                    NumSend = h.Numsend,
                    MdateSend = h.MdateSend,
                    COther1 = i.COther1,
                    CreatedBy = a.UserName ?? string.Empty
                }
            ).ToListAsync();

            List<TempPack> filteredResult = [.. result.Where(r => lotNos.Contains(r.LotNo))];

            foreach (var item in filteredResult)
            {
                if (item.ChkSize && string.IsNullOrEmpty(item.S1) && string.IsNullOrEmpty(item.S2)
                    && string.IsNullOrEmpty(item.S3) && string.IsNullOrEmpty(item.S4)
                    && string.IsNullOrEmpty(item.S5) && string.IsNullOrEmpty(item.S6)
                    && string.IsNullOrEmpty(item.S7) && string.IsNullOrEmpty(item.S8)
                    && string.IsNullOrEmpty(item.S9) && string.IsNullOrEmpty(item.S10)
                    && string.IsNullOrEmpty(item.S11) && string.IsNullOrEmpty(item.S12))
                {
                    item.S1 = "52";
                    item.OkQ1 = item.OkTtl;
                }
            }

            return filteredResult;
        }

        private async Task<List<TempPack>> GetSendLostAsync(string[] lotNos)
        {
            List<TempPack> tempPacks = await (from sl in _sPDbContext.SendLostDetail
                                              join lot in _sPDbContext.Lot on sl.LotNo equals lot.LotNo into gjLot
                                              from lot in gjLot.DefaultIfEmpty()
                                              join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo into gjOrd
                                              from ord in gjOrd.DefaultIfEmpty()
                                              where lotNos.Contains(sl.LotNo) && (sl.Doc == "" || sl.Doc == null)
                                              select new TempPack
                                              {
                                                  LotNo = sl.LotNo,
                                                  OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                                  CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                                  Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                                  SendDate = sl.CreateDate.GetValueOrDefault(),
                                                  Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                                                  FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                                                  FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                                                  SendType = "KL",
                                                  OkTtl = sl.TtQty,
                                                  OkWg = (decimal)sl.TtWg!,
                                              }).ToListAsync();
            return tempPacks;
        }

        private async Task<List<TempPack>> GetExportAsync(string[] lotNos)
        {
            List<TempPack> tempPacks = await (from expt in _sPDbContext.ExportDetail
                                              join lot in _sPDbContext.Lot on expt.LotNo equals lot.LotNo into gjLot
                                              from lot in gjLot.DefaultIfEmpty()
                                              join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo into gjOrd
                                              from ord in gjOrd.DefaultIfEmpty()
                                              where lotNos.Contains(expt.LotNo) && (expt.Doc == "" || expt.Doc == null)
                                              select new TempPack
                                              {
                                                  LotNo = expt.LotNo,
                                                  OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                                  CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                                  Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                                  SendDate = expt.CreateDate!.Value,
                                                  Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                                                  FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                                                  FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                                                  SendType = "KX",
                                                  OkTtl = expt.TtQty,
                                                  OkWg = (decimal)expt.TtWg!,
                                              }).ToListAsync();
            return tempPacks;
        }

        public async Task<BaseResponseModel> ConfirmToSendStoreAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetStoreAsync(lotNos);

            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };

            await using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ1, false);
                var existsHeader = await _jPDbContext.Sj1hreceive.AnyAsync(x => x.ReceiveNo == receiveNo);

                while (existsHeader)
                {
                    receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ1, true);
                    existsHeader = await _jPDbContext.Sj1hreceive.AnyAsync(x => x.ReceiveNo == receiveNo);
                }

                var header = new Sj1hreceive
                {
                    ReceiveNo = receiveNo,
                    Username = userId,
                    Mdate = DateTime.Now,
                    Upday = DateTime.Now,
                    Department = "BL"
                };

                _jPDbContext.Sj1hreceive.Add(header);

                foreach (var pack in tempPacks)
                {
                    bool exists = await _jPDbContext.Sj1dreceive.AnyAsync(x => x.Numsend == pack.NumSend && x.Billnumber == pack.BillNumber);

                    if (exists) throw new InvalidOperationException($"รายการ {pack.BillNumber} เคยรับเข้าแล้ว");

                    var (Sizes, Quantities) = MapSizes(pack);

                    var defaultBox = "A1";
                    var boxQuery = await (from b in _jPDbContext.OrdLotno
                                          join c in _jPDbContext.OrdHorder on b.OrderNo equals c.OrderNo
                                          where b.LotNo == pack.LotNo
                                          select c.CustCode).FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(boxQuery)) defaultBox = boxQuery;

                    var detail = new Sj1dreceive
                    {
                        ReceiveNo = receiveNo,
                        Article = pack.Article,
                        Barcode = pack.Barcode,
                        Lotno = pack.LotNo,
                        Price = 0,
                        Ttwg = pack.OkWg,
                        Ttqty = pack.OkTtl,
                        OldStock = 0,
                        NewStock = 0,
                        Mdate = DateTime.Now,
                        Username = userId,
                        Chksize = pack.ChkSize,
                        Upday = DateTime.Now,
                        Billnumber = pack.BillNumber,
                        SizeNoOrd = pack.SizeNoOrd,
                        Boxno = defaultBox,
                        Numsend = pack.NumSend,

                        S1 = Sizes[0],
                        S2 = Sizes[1],
                        S3 = Sizes[2],
                        S4 = Sizes[3],
                        S5 = Sizes[4],
                        S6 = Sizes[5],
                        S7 = Sizes[6],
                        S8 = Sizes[7],
                        S9 = Sizes[8],
                        S10 = Sizes[9],
                        S11 = Sizes[10],
                        S12 = Sizes[11],

                        Q1 = (decimal)Quantities[0],
                        Q2 = (decimal)Quantities[1],
                        Q3 = (decimal)Quantities[2],
                        Q4 = (decimal)Quantities[3],
                        Q5 = (decimal)Quantities[4],
                        Q6 = (decimal)Quantities[5],
                        Q7 = (decimal)Quantities[6],
                        Q8 = (decimal)Quantities[7],
                        Q9 = (decimal)Quantities[8],
                        Q10 = (decimal)Quantities[9],
                        Q11 = (decimal)Quantities[10],
                        Q12 = (decimal)Quantities[11],
                    };

                    _jPDbContext.Sj1dreceive.Add(detail);

                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == pack.BillNumber &&
                        a.Numsend == pack.NumSend &&
                        a.SendType.Substring(1, 1) == "S" &&
                        string.IsNullOrEmpty(a.Doc));

                    if (sendStock != null) sendStock.Doc = receiveNo;

                    var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b => b.Billnumber == pack.BillNumber);
                    if (jobBill != null) jobBill.SendStockDoc = receiveNo;

                    await _jPDbContext.SaveChangesAsync();
                }

                await UpdateStoreAsSentAsync(tempPacks, receiveNo);

                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"รับเข้าเก็บสำเร็จ เลขที่รับเข้า: {receiveNo}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdateStoreAsSentAsync(List<TempPack> tempPacks, string receiveNo)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var pack in tempPacks)
                {
                    var Store = _sPDbContext.Store.FirstOrDefault(x => x.LotNo == pack.LotNo && x.BillNumber == pack.BillNumber);
                    if (Store != null)
                    {
                        Store.IsSended = true;
                        Store.Doc = receiveNo;
                        Store.UpdateDate = DateTime.Now;
                        Store.UpdateBy = int.TryParse(pack.Username, out int userId) ? userId : null;
                        _sPDbContext.Store.UpdateRange(Store);
                    }
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

        public async Task<BaseResponseModel> ConfirmToSendMeltAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetMeltAsync(lotNos);
            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };

            await using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ2, false);
                var existsHeader = await _jPDbContext.Sj2hreceive.AnyAsync(x => x.ReceiveNo == receiveNo);

                while (existsHeader)
                {
                    receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ2, true);
                    existsHeader = await _jPDbContext.Sj2hreceive.AnyAsync(x => x.ReceiveNo == receiveNo);
                }

                var now = DateTime.Now;

                var connection = _jPDbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await connection.ExecuteAsync(@"
                    INSERT INTO Sj2hreceive
                    (ReceiveNo, Username, Mdate, Upday, Department, return_found)
                    VALUES (@ReceiveNo, @Username, @Mdate, @Upday, @Department, @ReturnFound)",
                    new
                    {
                        ReceiveNo = receiveNo,
                        Username = userId,
                        Mdate = now,
                        Upday = now,
                        Department = "BL",
                        ReturnFound = false
                    },
                    transaction: transaction.GetDbTransaction()
                );

                foreach (var pack in tempPacks)
                {
                    bool exists = await _jPDbContext.Sj2dreceive
                        .AnyAsync(x => x.Numsend == pack.NumSend && x.Billnumber == pack.BillNumber);

                    if (exists)
                        throw new InvalidOperationException($"รายการ {pack.BillNumber} เคยรับเข้าแล้ว");

                    var (Sizes, Quantities) = MapSizes(pack);

                    var defaultBox = "A1";
                    var boxQuery = await (from b in _jPDbContext.OrdLotno
                                          join c in _jPDbContext.OrdHorder on b.OrderNo equals c.OrderNo
                                          where b.LotNo == pack.LotNo
                                          select c.CustCode).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(boxQuery))
                        defaultBox = boxQuery;

                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    var sqlInsertDetail = @"
                        INSERT INTO Sj2dreceive
                        (ReceiveNo, Article, Barcode, Lotno, Price, Ttwg, Ttqty, OldStock, NewStock, Mdate, Username, 
                         Chksize, Upday, Billnumber, SizeNoOrd, Boxno, Numsend,
                         S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12,
                         Q1, Q2, Q3, Q4, Q5, Q6, Q7, Q8, Q9, Q10, Q11, Q12)
                        VALUES
                        (@ReceiveNo, @Article, @Barcode, @Lotno, @Price, @Ttwg, @Ttqty, @OldStock, @NewStock, @Mdate, @Username,
                         @Chksize, @Upday, @Billnumber, @SizeNoOrd, @Boxno, @Numsend,
                         @S1, @S2, @S3, @S4, @S5, @S6, @S7, @S8, @S9, @S10, @S11, @S12,
                         @Q1, @Q2, @Q3, @Q4, @Q5, @Q6, @Q7, @Q8, @Q9, @Q10, @Q11, @Q12);
                    ";

                    await connection.ExecuteAsync(sqlInsertDetail, new
                    {
                        ReceiveNo = receiveNo,
                        pack.Article,
                        pack.Barcode,
                        Lotno = pack.LotNo,
                        Price = 0,
                        Ttwg = pack.OkWg,
                        Ttqty = pack.OkTtl,
                        OldStock = 0,
                        NewStock = 0,
                        Mdate = now,
                        Username = userId,
                        Chksize = pack.ChkSize,
                        Upday = now,
                        Billnumber = pack.BillNumber,
                        pack.SizeNoOrd,
                        Boxno = defaultBox,
                        Numsend = pack.NumSend,

                        S1 = Sizes[0],
                        S2 = Sizes[1],
                        S3 = Sizes[2],
                        S4 = Sizes[3],
                        S5 = Sizes[4],
                        S6 = Sizes[5],
                        S7 = Sizes[6],
                        S8 = Sizes[7],
                        S9 = Sizes[8],
                        S10 = Sizes[9],
                        S11 = Sizes[10],
                        S12 = Sizes[11],

                        Q1 = (decimal)Quantities[0],
                        Q2 = (decimal)Quantities[1],
                        Q3 = (decimal)Quantities[2],
                        Q4 = (decimal)Quantities[3],
                        Q5 = (decimal)Quantities[4],
                        Q6 = (decimal)Quantities[5],
                        Q7 = (decimal)Quantities[6],
                        Q8 = (decimal)Quantities[7],
                        Q9 = (decimal)Quantities[8],
                        Q10 = (decimal)Quantities[9],
                        Q11 = (decimal)Quantities[10],
                        Q12 = (decimal)Quantities[11]
                    }, transaction: transaction.GetDbTransaction());

                    var sendMelt = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == pack.BillNumber &&
                        a.Numsend == pack.NumSend &&
                        a.SendType.Substring(1, 1) == "M" &&
                        a.ReturnFound == false &&
                        string.IsNullOrEmpty(a.Doc));

                    if (sendMelt != null)
                        sendMelt.Doc = receiveNo;

                    var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b => b.Billnumber == pack.BillNumber);
                    if (jobBill != null)
                        jobBill.SendMeltDoc = receiveNo;

                    await _jPDbContext.SaveChangesAsync();
                }

                await UpdateMeltAsSentAsync(tempPacks, receiveNo);

                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"รับเข้าหลอมสำเร็จ เลขที่รับเข้า: {receiveNo}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdateMeltAsSentAsync(List<TempPack> tempPacks, string receiveNo)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var pack in tempPacks)
                {
                    var Melt = _sPDbContext.Melt.FirstOrDefault(x => x.LotNo == pack.LotNo && x.BillNumber == pack.BillNumber);
                    if (Melt != null)
                    {
                        Melt.IsSended = true;
                        Melt.Doc = receiveNo;
                        Melt.UpdateDate = DateTime.Now;
                        Melt.UpdateBy = int.TryParse(pack.Username, out int userId) ? userId : null;
                        _sPDbContext.Melt.UpdateRange(Melt);
                    }
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

        public async Task<BaseResponseModel> ConfirmToSendLostAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetSendLostAsync(lotNos);
            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateSPReceiveNoAsync();

                SendLost sendLost = new()
                {
                    Doc = receiveNo,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    CreateBy = int.TryParse(userId, out int userIdInt) ? userIdInt : null,
                    UpdateDate = DateTime.Now,
                    UpdateBy = int.TryParse(userId, out int userIdInt2) ? userIdInt2 : null,

                };
                _sPDbContext.SendLost.Add(sendLost);

                foreach (var pack in tempPacks)
                {
                    var sendLostDetail = await _sPDbContext.SendLostDetail.FirstOrDefaultAsync(x => x.LotNo == pack.LotNo && (x.Doc == null || x.Doc == ""));
                    if (sendLostDetail != null)
                    {
                        sendLostDetail.Doc = receiveNo;
                        sendLostDetail.IsSended = true;
                        sendLostDetail.UpdateDate = DateTime.Now;
                        _sPDbContext.SendLostDetail.UpdateRange(sendLostDetail);
                    }
                }
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"ยืนยันการส่งของหายสำเร็จ เลขที่อ้างอิง: {receiveNo}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BaseResponseModel> ConfirmToSendExportAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetExportAsync(lotNos);

            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };

            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateSPReceiveNoAsync();

                Export export = new()
                {
                    Doc = receiveNo,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    CreateBy = int.TryParse(userId, out int userIdInt) ? userIdInt : null,
                    UpdateDate = DateTime.Now,
                    UpdateBy = int.TryParse(userId, out int userIdInt2) ? userIdInt2 : null,
                };

                _sPDbContext.Export.Add(export);

                foreach (var pack in tempPacks)
                {
                    var exports = await _sPDbContext.ExportDetail.Where(x => x.LotNo == pack.LotNo).ToListAsync();

                    var exportDetail = exports.FirstOrDefault(x => x.LotNo == pack.LotNo && (x.Doc == null || x.Doc == ""));
                    if (exportDetail != null)
                    {
                        exportDetail.Doc = receiveNo;
                        exportDetail.IsSended = true;
                        exportDetail.UpdateDate = DateTime.Now;

                        _sPDbContext.ExportDetail.UpdateRange(exportDetail);
                    }

                    var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == pack.LotNo);
                    var exportedQty = exports.Where(x => x.LotNo == pack.LotNo && x.IsSended == true && x.Doc != null && x.Doc != "").Sum(x => x.TtQty);

                    if (lot != null)
                    {
                        if(exportedQty >= lot.TtQty)
                        {
                            lot.IsSuccess = true;
                            lot.UpdateDate = DateTime.Now;
                        }
                    }
                }
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"ยืนยันการส่งออกสำเร็จ เลขที่อ้างอิง: {receiveNo}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<TempPack>> GetShowroomAsync(string[] lotNos)
        {
            List<TempPack> tempPacks = await (from sr in _sPDbContext.SendShowroomDetail
                                              join lot in _sPDbContext.Lot on sr.LotNo equals lot.LotNo into gjLot
                                              from lot in gjLot.DefaultIfEmpty()
                                              join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo into gjOrd
                                              from ord in gjOrd.DefaultIfEmpty()
                                              where lotNos.Contains(sr.LotNo) && (sr.Doc == "" || sr.Doc == null)
                                              select new TempPack
                                              {
                                                  LotNo = sr.LotNo,
                                                  OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                                  CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                                  Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                                  SendDate = sr.CreateDate!.Value,
                                                  Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                                                  FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                                                  FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                                                  SendType = "KR",
                                                  OkTtl = sr.TtQty,
                                                  OkWg = (decimal)sr.TtWg,
                                              }).ToListAsync();
            return tempPacks;
        }

        public async Task<BaseResponseModel> ConfirmToSendShowroomAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetShowroomAsync(lotNos);

            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };

            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateSPReceiveNoAsync();

                SendShowroom sendShowroom = new()
                {
                    Doc = receiveNo,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    CreateBy = int.TryParse(userId, out int userIdInt) ? userIdInt : null,
                    UpdateDate = DateTime.Now,
                    UpdateBy = int.TryParse(userId, out int userIdInt2) ? userIdInt2 : null,
                };

                _sPDbContext.SendShowroom.Add(sendShowroom);

                foreach (var pack in tempPacks)
                {
                    var showroomDetails = await _sPDbContext.SendShowroomDetail.Where(x => x.LotNo == pack.LotNo).ToListAsync();

                    var detail = showroomDetails.FirstOrDefault(x => x.LotNo == pack.LotNo && (x.Doc == null || x.Doc == ""));
                    if (detail != null)
                    {
                        detail.Doc = receiveNo;
                        detail.IsSended = true;
                        detail.UpdateDate = DateTime.Now;

                        _sPDbContext.SendShowroomDetail.UpdateRange(detail);
                    }

                    var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == pack.LotNo);
                    var showroomQty = showroomDetails.Where(x => x.LotNo == pack.LotNo && x.IsSended == true && x.Doc != null && x.Doc != "").Sum(x => x.TtQty);

                    if (lot != null && showroomQty >= lot.TtQty)
                    {
                        lot.IsSuccess = true;
                        lot.UpdateDate = DateTime.Now;
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = $"ยืนยันส่ง Showroom สำเร็จ เลขที่อ้างอิง: {receiveNo}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ดึงชื่อผู้รายงานจาก userid
        private async Task<string> GetReporterNameAsync(string? userid)
        {
            if (string.IsNullOrEmpty(userid)) return string.Empty;
            var users = await _pISService.GetAllUser();
            if (users == null) return string.Empty;
            var reporter = users.FirstOrDefault(x => x.UserID == Convert.ToInt32(userid));
            return reporter != null ? $"{reporter.FirstName} {reporter.LastName}".Trim() : string.Empty;
        }

        // LINQ หลักสำหรับ KS/KM จาก JobBillSendStock (ใช้ร่วมกันระหว่าง Store และ Melt)
        private async Task<List<TempPack>> QueryJobBillSendStockForPrintAsync(string[] lotNos, string reporterName, string sendType)
        {
            try
            {
                List<TempPack> result = await (
                    from h in _jPDbContext.JobBillSendStock.AsNoTracking()
                    join a in _jPDbContext.JobBill.AsNoTracking() on h.Billnumber equals a.Billnumber into gjA
                    from a in gjA.DefaultIfEmpty()
                    join d in _jPDbContext.JobDetail.AsNoTracking() on a.JobBarcode equals d.JobBarcode
                    join i in _jPDbContext.JobHead.AsNoTracking() on new { d.DocNo, d.EmpCode } equals new { i.DocNo, i.EmpCode }
                    join b in _jPDbContext.OrdLotno.AsNoTracking() on new { d.ListNo, d.OrderNo } equals new { b.ListNo, b.OrderNo }
                    join c in _jPDbContext.CpriceSale.AsNoTracking() on a.Barcode equals c.Barcode
                    join f in _jPDbContext.OrdLotno.AsNoTracking() on a.Lotno equals f.LotNo
                    join g in _jPDbContext.JobBillsize.AsNoTracking() on a.Billnumber equals g.Billnumber into gjG
                    from g in gjG.Where(x => a.SizeNoOrd == true).DefaultIfEmpty()
                    where h.SendType == sendType && !string.IsNullOrEmpty(h.Doc)
                    orderby a.Lotno
                    select new TempPack
                    {
                        Name = (from t in _jPDbContext.TempProfile.AsNoTracking() where t.EmpCode == a.EmpCode select (t.TitleName ?? "") + " " + (t.Name ?? "")).FirstOrDefault() ?? string.Empty,
                        EmpCode = a.EmpCode,
                        LotNo = a.Lotno,
                        JobBarcode = a.JobBarcode,
                        Article = a.Article,
                        Barcode = a.Barcode,
                        ArtCode = a.ArtCode,
                        FnCode = a.FnCode,
                        DocNo = a.DocNo,
                        Doc = h.Doc ?? string.Empty,
                        ListNo = a.ListNo,
                        BillDate = a.BillDate,
                        CheckBill = a.CheckBill,
                        Username = reporterName,
                        Mdate = a.MDate,
                        SendPack = a.SendPack,
                        PackDoc = a.PackDoc,
                        Num = a.Num,
                        SendDate = a.SendDate.GetValueOrDefault(),
                        SizeNoOrd = a.SizeNoOrd,
                        Wg_Over = a.WgOver,
                        BillNumber = a.Billnumber,
                        SendType = h.SendType ?? string.Empty,
                        OkTtl = h.Ttqty,
                        OkWg = h.Ttwg,
                        OkQ1 = h.Q1,
                        OkQ2 = h.Q2,
                        OkQ3 = h.Q3,
                        OkQ4 = h.Q4,
                        OkQ5 = h.Q5,
                        OkQ6 = h.Q6,
                        OkQ7 = h.Q7,
                        OkQ8 = h.Q8,
                        OkQ9 = h.Q9,
                        OkQ10 = h.Q10,
                        OkQ11 = h.Q11,
                        OkQ12 = h.Q12,
                        SUser = h.Userid ?? string.Empty,
                        SizeZone = f.SizeZone ?? string.Empty,
                        ChkSize = f.ChkSize ?? false,
                        S1 = g.S1,
                        S2 = g.S2,
                        S3 = g.S3,
                        S4 = g.S4,
                        S5 = g.S5,
                        S6 = g.S6,
                        S7 = g.S7,
                        S8 = g.S8,
                        S9 = g.S9,
                        S10 = g.S10,
                        S11 = g.S11,
                        S12 = g.S12,
                        Unit = b.Unit,
                        OrderNo = b.OrderNo,
                        ListGem = c.ListGem,
                        FinishingTH = c.TdesFn,
                        WgActual = c.WgActual,
                        CustCode = d.CustCode,
                        NumSend = h.Numsend,
                        MdateSend = h.MdateSend,
                        COther1 = i.COther1,
                        CreatedBy = a.UserName
                    }).ToListAsync();

                return [.. result.Where(r => lotNos.Contains(r.LotNo))];

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ส่งคลัง (KS) - ดึงรายการที่ยืนยันแล้ว พร้อม Unallocated จาก Store table
        private async Task<List<TempPack>> GetConfirmedStoreForPrintAsync(string[] lotNos, string reporterName)
        {
            var filteredResult = await QueryJobBillSendStockForPrintAsync(lotNos, reporterName, "KS");

            var storeInfo = (
                from r in filteredResult
                join s in _sPDbContext.Store.AsNoTracking() on new { r.LotNo, r.BillNumber } equals new { s.LotNo, s.BillNumber }
                select new { s.LotNo, s.BillNumber }
            ).ToList();

            var lotNosInResult = filteredResult.Select(r => r.LotNo).Distinct().ToList();
            var lots = await _sPDbContext.Lot.AsNoTracking()
                .Where(l => lotNosInResult.Contains(l.LotNo))
                .ToDictionaryAsync(l => l.LotNo);

            foreach (var item in filteredResult)
            {
                if (lots.TryGetValue(item.LotNo, out var lot))
                {
                    var storeMatched = storeInfo.FirstOrDefault(s => s.LotNo == item.LotNo && s.BillNumber == item.BillNumber);
                    if (storeMatched != null)
                        item.Unallocated = lot.Unallocated ?? 0;
                }
            }

            return filteredResult;
        }

        // หลอม (KM) - ดึงรายการที่ยืนยันแล้ว พร้อม BreakDescription และ Unallocated จาก Melt table
        private async Task<List<TempPack>> GetConfirmedMeltForPrintAsync(string[] lotNos, string reporterName)
        {
            var filteredResult = await QueryJobBillSendStockForPrintAsync(lotNos, reporterName, "KM");

            var meltInfo = (
                from r in filteredResult
                join s in _sPDbContext.Melt.AsNoTracking() on new { r.LotNo, r.BillNumber } equals new { s.LotNo, s.BillNumber }
                join b in _sPDbContext.BreakDescription.AsNoTracking() on s.BreakDescriptionId equals b.BreakDescriptionId
                select new { s.LotNo, s.BillNumber, BreakDes = b.Name }
            ).ToList();

            var lotNosInResult = filteredResult.Select(r => r.LotNo).Distinct().ToList();
            var lots = await _sPDbContext.Lot.AsNoTracking()
                .Where(l => lotNosInResult.Contains(l.LotNo))
                .ToDictionaryAsync(l => l.LotNo);

            foreach (var item in filteredResult)
            {
                if (lots.TryGetValue(item.LotNo, out var lot))
                {
                    var meltMatched = meltInfo.FirstOrDefault(m => m.LotNo == item.LotNo && m.BillNumber == item.BillNumber);
                    if (meltMatched != null)
                    {
                        item.BreakDescription = meltMatched.BreakDes;
                        item.Unallocated = lot.Unallocated ?? 0;
                    }
                }
            }

            return filteredResult;
        }

        // ส่งออก (KX) - ดึงรายการจาก ExportDetail ที่ยืนยันแล้ว
        private async Task<List<TempPack>> GetConfirmedExportForPrintAsync(string[] lotNos, string reporterName)
        {
            return await (from expt in _sPDbContext.ExportDetail.AsNoTracking()
                          join lot in _sPDbContext.Lot.AsNoTracking() on expt.LotNo equals lot.LotNo into gjLot
                          from lot in gjLot.DefaultIfEmpty()
                          join ord in _sPDbContext.Order.AsNoTracking() on lot.OrderNo equals ord.OrderNo into gjOrd
                          from ord in gjOrd.DefaultIfEmpty()
                          where lotNos.Contains(expt.LotNo) && !string.IsNullOrEmpty(expt.Doc) && expt.IsSended == true && expt.IsActive
                          select new TempPack
                          {
                              LotNo = expt.LotNo,
                              ListNo = lot.ListNo,
                              OrderNo = ord != null ? ord.OrderNo : string.Empty,
                              CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                              Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                              Doc = expt.Doc ?? string.Empty,
                              MdateSend = expt.CreateDate!.Value,
                              Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                              FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                              FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                              Username = reporterName,
                              IsOverQouta = expt.IsOverQuota,
                              SendType = "KX",
                              OkTtl = expt.TtQty,
                              OkWg = (decimal)expt.TtWg!,
                          }).ToListAsync();
        }

        // หาย (KL) - ดึงรายการจาก SendLostDetail ที่ยืนยันแล้ว พร้อม TableName จาก Assignment
        private async Task<List<TempPack>> GetConfirmedLostForPrintAsync(string[] lotNos, string reporterName)
        {
            var assignedTables = await (
                from ass in _sPDbContext.Assignment.AsNoTracking()
                join asmr in _sPDbContext.AssignmentReceived.AsNoTracking() on ass.AssignmentId equals asmr.AssignmentId
                join asnt in _sPDbContext.AssignmentTable.AsNoTracking() on asmr.AssignmentReceivedId equals asnt.AssignmentReceivedId
                join wt in _sPDbContext.WorkTable.AsNoTracking() on asnt.WorkTableId equals wt.Id
                join rev in _sPDbContext.Received.AsNoTracking() on asmr.ReceivedId equals rev.ReceivedId
                where lotNos.Contains(rev.LotNo) && asmr.IsActive && ass.IsActive && asnt.IsActive && wt.IsActive
                select new { rev.LotNo, ass.AssignmentId, wt.Name }
            ).ToListAsync();

            var assignedTableDict = assignedTables
                .GroupBy(a => a.LotNo)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(x => new AssignedWorkTableModel { AssignmentId = x.AssignmentId, TableName = x.Name! })
                        .DistinctBy(m => m.TableName)
                        .ToList()
                );

            List<TempPack> lostTempPacks = await (from sl in _sPDbContext.SendLostDetail.AsNoTracking()
                                                   join lot in _sPDbContext.Lot.AsNoTracking() on sl.LotNo equals lot.LotNo into gjLot
                                                   from lot in gjLot.DefaultIfEmpty()
                                                   join ord in _sPDbContext.Order.AsNoTracking() on lot.OrderNo equals ord.OrderNo into gjOrd
                                                   from ord in gjOrd.DefaultIfEmpty()
                                                   where lotNos.Contains(sl.LotNo) && !string.IsNullOrEmpty(sl.Doc) && sl.IsSended == true && sl.IsActive
                                                   select new TempPack
                                                   {
                                                       LotNo = sl.LotNo,
                                                       ListNo = lot.ListNo,
                                                       OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                                       CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                                       Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                                       Doc = sl.Doc ?? string.Empty,
                                                       MdateSend = sl.CreateDate!.Value,
                                                       Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                                                       FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                                                       FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                                                       Username = reporterName,
                                                       TableName = string.Empty,
                                                       SendType = "KL",
                                                       OkTtl = sl.TtQty,
                                                       OkWg = (decimal)sl.TtWg!,
                                                   }).ToListAsync();

            foreach (var item in lostTempPacks)
            {
                if (assignedTableDict.TryGetValue(item.LotNo, out var tables))
                    item.TableName = string.Join(", ", tables.Select(t => t.TableName));
            }

            return lostTempPacks;
        }

        private async Task<List<TempPack>> GetConfirmedShowroomForPrintAsync(string[] lotNos, string reporterName)
        {
            return await (from sr in _sPDbContext.SendShowroomDetail.AsNoTracking()
                          join lot in _sPDbContext.Lot.AsNoTracking() on sr.LotNo equals lot.LotNo into gjLot
                          from lot in gjLot.DefaultIfEmpty()
                          join ord in _sPDbContext.Order.AsNoTracking() on lot.OrderNo equals ord.OrderNo into gjOrd
                          from ord in gjOrd.DefaultIfEmpty()
                          where lotNos.Contains(sr.LotNo) && !string.IsNullOrEmpty(sr.Doc) && sr.IsSended == true && sr.IsActive
                          select new TempPack
                          {
                              LotNo = sr.LotNo,
                              ListNo = lot.ListNo,
                              OrderNo = ord != null ? ord.OrderNo : string.Empty,
                              CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                              Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                              Doc = sr.Doc ?? string.Empty,
                              MdateSend = sr.CreateDate!.Value,
                              Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                              FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                              FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                              Username = reporterName,
                              SendType = "KR",
                              OkTtl = sr.TtQty,
                              OkWg = (decimal)sr.TtWg,
                          }).ToListAsync();
        }

        public async Task<List<TempPack>> GetDocToPrintByType(string[] lotNos, string userid, string sendType)
        {
            string reporterName = await GetReporterNameAsync(userid);
            return sendType switch
            {
                "KS" => await GetConfirmedStoreForPrintAsync(lotNos, reporterName),
                "KM" => await GetConfirmedMeltForPrintAsync(lotNos, reporterName),
                "KX" => await GetConfirmedExportForPrintAsync(lotNos, reporterName),
                "KL" => await GetConfirmedLostForPrintAsync(lotNos, reporterName),
                "KR" => await GetConfirmedShowroomForPrintAsync(lotNos, reporterName),
                _ => [
                    .. await GetConfirmedStoreForPrintAsync(lotNos, reporterName),
                    .. await GetConfirmedMeltForPrintAsync(lotNos, reporterName),
                    .. await GetConfirmedExportForPrintAsync(lotNos, reporterName),
                    .. await GetConfirmedLostForPrintAsync(lotNos, reporterName),
                    .. await GetConfirmedShowroomForPrintAsync(lotNos, reporterName),
                ]
            };
        }

        public async Task<List<TempPack>> GetAllDocToPrint(string[] lotNos, string userid)
        {
            try
            {
                return await GetDocToPrintByType(lotNos, userid, "All");
            }
            catch (Exception ex)
            {
                // log ไว้ดู เช่น _logger.LogError(ex, "GetAllDocToPrint failed | lotNos: {LotNos}, userid: {UserId}", string.Join(",", lotNos), userid);
                throw;
            }
        }

        public async Task UpdateOrderSuccessAsync(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return;
            var lots = await _sPDbContext.Lot
                .Where(x => x.OrderNo == orderNo)
                .ToListAsync();
            if (lots.Count == 0)
                return;
            bool allSuccess = lots.All(x => x.IsSuccess);
            var order = await _sPDbContext.Order.FirstOrDefaultAsync(x => x.OrderNo == orderNo);
            if (order != null)
            {
                order.IsSuccess = allSuccess;
                order.UpdateDate = DateTime.Now;
                await _sPDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateArticleAsync(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return;

            var baseData = await (
                from a in _jPDbContext.OrdHorder.AsNoTracking()
                join b in _jPDbContext.OrdLotno.AsNoTracking()
                    on a.OrderNo equals b.OrderNo
                join c in _jPDbContext.CpriceSale.AsNoTracking()
                    on b.Barcode equals c.Barcode into bc
                from c in bc.DefaultIfEmpty()
                where a.OrderNo == orderNo
                      && a.Factory
                      && !string.IsNullOrEmpty(b.LotNo)
                orderby b.ListNo
                select new
                {
                    b.LotNo,
                    Article = c.Article ?? string.Empty,
                }
            ).ToListAsync();

            if (baseData.Count == 0)
                return;

            var spLots = await _sPDbContext.Lot
                .Where(x => x.OrderNo == orderNo)
                .ToListAsync();

            var spLotDict = spLots.ToDictionary(x => x.LotNo);

            foreach (var item in baseData)
            {
                if (!spLotDict.TryGetValue(item.LotNo, out var spLot))
                    continue;

                if (spLot.Article != item.Article)
                {
                    spLot.Article = item.Article;
                }
            }

            await _sPDbContext.SaveChangesAsync();
        }

        private static (string[] Sizes, double[] Quantities) MapSizes(TempPack pack)
        {
            var sizes = new[]
            {
                pack.S1, pack.S2, pack.S3, pack.S4, pack.S5, pack.S6,
                pack.S7, pack.S8, pack.S9, pack.S10, pack.S11, pack.S12
            }.Select(x => x?.Trim() ?? "").ToArray();

            var qty = new[]
            {
                (double)pack.OkQ1, (double)pack.OkQ2, (double)pack.OkQ3, (double)pack.OkQ4, (double)pack.OkQ5, (double)pack.OkQ6,
                (double)pack.OkQ7, (double)pack.OkQ8, (double)pack.OkQ9, (double)pack.OkQ10, (double)pack.OkQ11, (double)pack.OkQ12
            }.Select(x => x).ToArray();

            var (shiftedSizes, shiftedQty) = ShiftSizeValues(sizes, qty);

            return (shiftedSizes, shiftedQty);
        }

        private static (string[] sizes, double[] qty) ShiftSizeValues(string[] sizes, double[] qty)
        {
            var resultSizes = new string[12];
            var resultQty = new double[12];

            int pos = 0;
            for (int i = 0; i < 12; i++)
            {
                if (qty[i] > 0)
                {
                    resultSizes[pos] = sizes[i];
                    resultQty[pos] = qty[i];
                    pos++;
                }
            }

            for (int i = pos; i < 12; i++)
            {
                resultSizes[i] = "";
                resultQty[i] = 0;
            }

            return (resultSizes, resultQty);
        }

        private async Task<string> GenerateReceiveNoAsync(ReceiveType type, bool IsDuplicate)
        {
            string middleCode = "RS";
            string year = DateTime.Now.ToString("yy");
            string month = DateTime.Now.ToString("MM");

            int count = type switch
            {
                ReceiveType.SJ1 => await _jPDbContext.Sj1hreceive
                    .Where(r =>
                        EF.Functions.DataLength(r.ReceiveNo) == 10 &&
                        r.ReceiveNo.Substring(0, 2) == year &&
                        r.ReceiveNo.Substring(2, 2) == middleCode &&
                        r.ReceiveNo.Substring(4, 2) == month
                    )
                    .CountAsync(),
                ReceiveType.SJ2 => await _jPDbContext.Sj2hreceive
                    .Where(r =>
                        EF.Functions.DataLength(r.ReceiveNo) == 10 &&
                        r.ReceiveNo.Substring(0, 2) == year &&
                        r.ReceiveNo.Substring(2, 2) == middleCode &&
                        r.ReceiveNo.Substring(4, 2) == month
                    )
                    .CountAsync(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (IsDuplicate)
            {
                count += 1;
            }

            int seq = count + 1;

            return $"{year}{middleCode}{month}{seq:D4}";
        }

        private async Task<string> GenerateSPReceiveNoAsync()
        {
            string prefix = "SP";
            string year = DateTime.Now.ToString("yy");
            string month = DateTime.Now.ToString("MM");
            string basePrefix = $"{year}{prefix}{month}";

            string? lastDoc = await _sPDbContext.Export
                .Where(r => r.Doc != null && r.Doc.StartsWith(basePrefix))
                .OrderByDescending(r => r.Doc)
                .Select(r => r.Doc)
                .FirstOrDefaultAsync();

            int nextSeq = 1;

            if (!string.IsNullOrWhiteSpace(lastDoc) && lastDoc.Length >= 10)
            {
                string seqPart = lastDoc.Substring(6, 4);
                if (int.TryParse(seqPart, out int lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            string newDoc = $"{basePrefix}{nextSeq:D4}";

            return newDoc;
        }
    }
}
