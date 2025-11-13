using Dapper;
using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.JPDbContext.Entities;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Reflection;
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
            var apiSettings = _configuration.GetSection("SendToStoreSettings");
            var Percentage = apiSettings["Percentage"];

            var sumStore = _sPDbContext.Store
                .Where(s => s.IsActive)
                .GroupBy(s => s.LotNo)
                .Select(g => new
                {
                    LotNo = g.Key,
                    TtWg = g.Sum(x => (double?)x.TtWg),
                    TtQty = g.Sum(x => (decimal?)x.TtQty),
                    g.FirstOrDefault()!.IsSended,
                    g.FirstOrDefault()!.IsStored
                });

            var sumMelt = _sPDbContext.Melt
                .Where(s => s.IsActive)
                .GroupBy(s => s.LotNo)
                .Select(g => new
                {
                    LotNo = g.Key,
                    g.FirstOrDefault()!.BreakDescriptionId,
                    TtWg = g.Sum(x => (double?)x.TtWg),
                    TtQty = g.Sum(x => (decimal?)x.TtQty),
                    g.FirstOrDefault()!.IsSended,
                    g.FirstOrDefault()!.IsMelted
                });

            var sumExport = _sPDbContext.Export
                .Where(s => s.IsActive)
                .GroupBy(s => s.LotNo)
                .Select(g => new
                {
                    LotNo = g.Key,
                    TtWg = g.Sum(x => (double?)x.TtWg),
                    TtQty = g.Sum(x => (decimal?)x.TtQty),
                    g.FirstOrDefault()!.IsSended
                });

            var baseData = await (
                from ord in _sPDbContext.Order
                join lot in _sPDbContext.Lot on ord.OrderNo equals lot.OrderNo
                join sqd in _sPDbContext.SendQtyToPackDetail on lot.LotNo equals sqd.LotNo into sqdj
                from sqd in sqdj.DefaultIfEmpty()
                join rec in (
                    from rec in _sPDbContext.Received
                    group rec by rec.LotNo into g
                    select new
                    {
                        LotNo = g.Key,
                        TtQty = g.Sum(x => (decimal?)x.TtQty),
                        TtWg = g.Sum(x => (double?)x.TtWg)

                    }
                ) on lot.LotNo equals rec.LotNo into revj
                from rec in revj.DefaultIfEmpty()
                where ord.OrderNo == orderNo
                select new OrderToStoreModel
                {
                    OrderNo = ord.OrderNo,
                    CustCode = ord.CustCode ?? string.Empty,
                    LotNo = lot.LotNo,
                    ListNo = lot.ListNo,
                    Article = lot.Article ?? string.Empty,
                    TtQty = lot.TtQty ?? 0,
                    TtWg = rec != null ? (rec.TtWg ?? 0) : 0,
                    Si = lot.Si ?? 0,
                    SendPack_Qty = rec != null ? (rec.TtQty ?? 0) : 0,
                    SendToPack_Qty = sqd != null ? sqd.TtQty : 0,
                    Packed_Qty = lot != null ? (lot.ReturnedQty ?? 0) : 0,
                    Percentage = !string.IsNullOrEmpty(Percentage) ? decimal.Parse(Percentage) : 0,

                    Store_Qty = sumStore.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtQty).FirstOrDefault() ?? 0,
                    Store_Wg = sumStore.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtWg).FirstOrDefault() ?? 0,
                    IsStoreSended = sumStore.Where(x => x.LotNo == lot.LotNo).Select(x => x.IsSended).FirstOrDefault(),
                    IsStored = sumStore.Where(x => x.LotNo == lot.LotNo).Select(x => x.IsStored).FirstOrDefault(),

                    Melt_Qty = sumMelt.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtQty).FirstOrDefault() ?? 0,
                    Melt_Wg = sumMelt.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtWg).FirstOrDefault() ?? 0,
                    BreakDescriptionId = sumMelt.Where(x => x.LotNo == lot.LotNo).Select(x => x.BreakDescriptionId).FirstOrDefault(),
                    IsMeltSended = sumMelt.Where(x => x.LotNo == lot.LotNo).Select(x => x.IsSended).FirstOrDefault(),
                    IsMelted = sumMelt.Where(x => x.LotNo == lot.LotNo).Select(x => x.IsMelted).FirstOrDefault(),

                    Export_Qty = sumExport.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtQty).FirstOrDefault() ?? 0,
                    Export_Wg = sumExport.Where(x => x.LotNo == lot.LotNo).Select(x => x.TtWg).FirstOrDefault() ?? 0,
                    IsExportSended = sumExport.Where(x => x.LotNo == lot.LotNo).Select(x => x.IsSended).FirstOrDefault(),
                }
            ).ToListAsync();

            return baseData;
        }

        public async Task<BaseResponseModel> SendStockAsync(SendStockInput input)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                // ดึง Received ทั้งหมดที่ LotNo ตรง
                var receivedList = await _sPDbContext.Received
                    .Where(x => x.LotNo == input.LotNo)
                    .OrderByDescending(o => o.CreateDate) // ล่าสุดก่อน
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

                // หาตัวล่าสุดที่ Doc ยังว่างใน Store / Melt / Export
                var received = receivedList.FirstOrDefault(r =>
                    !_sPDbContext.Store.Any(s => s.BillNumber == r.BillNumber && s.Doc != string.Empty) &&
                    !_sPDbContext.Melt.Any(m => m.BillNumber == r.BillNumber && m.Doc != string.Empty) &&
                    !_sPDbContext.Export.Any(e => e.LotNo == r.LotNo && e.Doc != string.Empty)
                );

                if (received == null)
                {
                    await transaction.RollbackAsync();
                    return new BaseResponseModel
                    {
                        IsSuccess = false,
                        Message = "ไม่พบ BillNumber ที่ยังสามารถส่งได้ (Doc ไม่ว่างทั้งหมด)"
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
                        UpdateDate = DateTime.Now
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
                        UpdateDate = DateTime.Now
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

                // ===== EXPORT =====
                var export = await _sPDbContext.Export
                    .FirstOrDefaultAsync(x => x.LotNo == input.LotNo);

                if (export == null && input.KxQty > 0)
                {
                    _sPDbContext.Export.Add(new Export
                    {
                        LotNo = input.LotNo,
                        Doc = string.Empty,
                        BillNumber = billNumber,
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
                    await _sPDbContext.Export
                        .Where(x => x.LotNo == input.LotNo)
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
            // ตรวจสอบ Doc ที่มีอยู่แล้ว
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
                    // INSERT
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
                        // DELETE
                        _jPDbContext.JobBillSendStock.Remove(record);
                    }
                    else
                    {
                        if (qty != record.Ttqty)
                        {
                            // UPDATE
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
                where EF.Functions.Like(h.SendType.Substring(1, 1), "S")
                    && (h.Doc == "" || h.Doc == null)
                    && h.MdateSend.Year == DateTime.Now.Year
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

                where
                    h.SendType.Substring(1, 1) == "M" &&
                    (h.Doc == null || h.Doc == "") &&
                    h.MdateSend.Year == DateTime.Now.Year

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
                    SendDate = a.SendDate ?? DateTime.MinValue,
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

            return filteredResult;
        }

        private async Task<List<TempPack>> GetExportAsync(string[] lotNos)
        {
            List<TempPack> tempPacks = await (from expt in _sPDbContext.Export
                                   join lot in _sPDbContext.Lot on expt.LotNo equals lot.LotNo into gjLot
                                   from lot in gjLot.DefaultIfEmpty()
                                   join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo into gjOrd
                                   from ord in gjOrd.DefaultIfEmpty()
                                   where lotNos.Contains(expt.LotNo)
                                      && (expt.Doc == "" || expt.Doc == null)
                                      && expt.CreateDate!.Value.Year == DateTime.Now.Year
                                   select new TempPack
                                   {
                                       LotNo = expt.LotNo,
                                       OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                       CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                       Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                       BillNumber = expt.BillNumber,
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
                var receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ1);
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
                    // ตรวจว่ารายการนี้เคยรับแล้วหรือไม่ (BillNumber + NumSend)
                    bool exists = await _jPDbContext.Sj1dreceive.AnyAsync(x => x.Numsend == pack.NumSend && x.Billnumber == pack.BillNumber);

                    if (exists) throw new InvalidOperationException($"รายการ {pack.BillNumber} เคยรับเข้าแล้ว");

                    var (Sizes, Quantities) = MapSizes(pack);

                    // ดึง BoxNo จาก OrdLotno → OrdHorder ถ้าไม่มีใช้ "A1"
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

                    // อัปเดต JobBill_SendStock → Doc = ReceiveNo
                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == pack.BillNumber &&
                        a.Numsend == pack.NumSend &&
                        a.SendType.Substring(1, 1) == "S" &&
                        string.IsNullOrEmpty(a.Doc));

                    if (sendStock != null) sendStock.Doc = receiveNo;

                    // อัปเดต JobBill → SendStockDoc
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
                var receiveNo = await GenerateReceiveNoAsync(ReceiveType.SJ2);
                var now = DateTime.Now;

                // ใช้ connection เดียวกับ EF Core
                var connection = _jPDbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                // ใช้ Dapper บน connection นี้ พร้อมกับ EF transaction
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
                    transaction: transaction.GetDbTransaction() // ผูก transaction เดียวกับ EF
                );

                // ส่วนอื่นยังคงใช้ EF Core ได้ตามปกติ
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

                    // ใช้ connection และ transaction เดิมจาก EF
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
                        Article = pack.Article,
                        Barcode = pack.Barcode,
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
                        Q12 = (decimal)Quantities[11]
                    }, transaction: transaction.GetDbTransaction()); //อยู่ใน transaction เดียวกับ EF


                    // อัปเดต JobBill_SendStock
                    var sendMelt = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == pack.BillNumber &&
                        a.Numsend == pack.NumSend &&
                        a.SendType.Substring(1, 1) == "M" &&
                        a.ReturnFound == false &&
                        string.IsNullOrEmpty(a.Doc));

                    if (sendMelt != null)
                        sendMelt.Doc = receiveNo;

                    // อัปเดต JobBill
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

        public async Task<BaseResponseModel> ConfirmToSendExportAsync(string[] lotNos, string userId)
        {
            List<TempPack> tempPacks = await GetExportAsync(lotNos);

            if (tempPacks == null || tempPacks.Count == 0) return new BaseResponseModel { IsSuccess = false };

            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveNo = await GenerateSPReceiveNoAsync();

                foreach (var pack in tempPacks)
                {
                    var export = await _sPDbContext.Export.FirstOrDefaultAsync(x => x.LotNo == pack.LotNo && (x.Doc == null || x.Doc == ""));
                    if (export != null)
                    {
                        export.Doc = receiveNo;
                        export.IsSended = true;
                        export.UpdateDate = DateTime.Now;

                        _sPDbContext.Export.UpdateRange(export);
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

        public async Task<List<TempPack>> GetAllDocToPrint(string[] lotNos, string userid)
        {
            var users = await _pISService.GetAllUser();
            var Reporter = users != null && userid != null ? users.Where(x => x.UserID == Convert.ToInt32(userid)).ToList() : [];

            List<TempPack> result = await (
                from h in _jPDbContext.JobBillSendStock
                join a in _jPDbContext.JobBill on h.Billnumber equals a.Billnumber into gjA
                from a in gjA.DefaultIfEmpty()
                join d in _jPDbContext.JobDetail on a.JobBarcode equals d.JobBarcode
                join i in _jPDbContext.JobHead on new { d.DocNo, d.EmpCode } equals new { i.DocNo, i.EmpCode }
                join b in _jPDbContext.OrdLotno on new { d.ListNo, d.OrderNo } equals new { b.ListNo, b.OrderNo }
                join c in _jPDbContext.CpriceSale on a.Barcode equals c.Barcode
                join f in _jPDbContext.OrdLotno on a.Lotno equals f.LotNo
                join g in _jPDbContext.JobBillsize on a.Billnumber equals g.Billnumber into gjG
                from g in gjG.Where(x => a.SizeNoOrd == true).DefaultIfEmpty()
                where (h.SendType == "KM" || h.SendType == "KS")
                    && !string.IsNullOrEmpty(h.Doc)
                    && h.MdateSend.Year == DateTime.Now.Year
                orderby a.Lotno
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
                    Doc = h.Doc ?? string.Empty,
                    ListNo = a.ListNo,
                    BillDate = a.BillDate,
                    CheckBill = a.CheckBill,
                    Username = Reporter != null && Reporter.Count != 0 ? $"{Reporter.FirstOrDefault()!.FirstName} {Reporter.FirstOrDefault()!.LastName}".Trim() : string.Empty,
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

            var meltInfo = (
                from r in filteredResult
                join s in _sPDbContext.Melt on new { r.LotNo, r.BillNumber } equals new { s.LotNo, s.BillNumber }
                join b in _sPDbContext.BreakDescription on s.BreakDescriptionId equals b.BreakDescriptionId
                select new
                {
                    s.LotNo,
                    s.BillNumber,
                    BreakDes = b.Name
                }
            ).ToList();

            foreach (var item in filteredResult)
            {
                var matched = meltInfo.FirstOrDefault(m => m.LotNo == item.LotNo && m.BillNumber == item.BillNumber);
                if (matched != null)
                {
                    item.BreakDescription = matched.BreakDes;
                }
            }

            List<TempPack> tempPacks = await (from expt in _sPDbContext.Export
                                              join lot in _sPDbContext.Lot on expt.LotNo equals lot.LotNo into gjLot
                                              from lot in gjLot.DefaultIfEmpty()
                                              join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo into gjOrd
                                              from ord in gjOrd.DefaultIfEmpty()
                                              where lotNos.Contains(expt.LotNo)
                                                 && !string.IsNullOrEmpty(expt.Doc)
                                                 && expt.CreateDate!.Value.Year == DateTime.Now.Year
                                              select new TempPack
                                              {
                                                  LotNo = expt.LotNo,
                                                  ListNo = lot.ListNo,
                                                  OrderNo = ord != null ? ord.OrderNo : string.Empty,
                                                  CustCode = ord != null ? ord.CustCode ?? string.Empty : string.Empty,
                                                  Article = lot != null ? lot.Article ?? string.Empty : string.Empty,
                                                  BillNumber = expt.BillNumber,
                                                  Doc = expt.Doc ?? string.Empty,
                                                  MdateSend = expt.CreateDate!.Value,
                                                  Unit = lot != null ? lot.Unit ?? string.Empty : string.Empty,
                                                  FinishingEN = lot != null ? lot.EdesFn ?? string.Empty : string.Empty,
                                                  FinishingTH = lot != null ? lot.TdesFn ?? string.Empty : string.Empty,
                                                  Username = Reporter != null && Reporter.Count != 0 ? $"{Reporter.FirstOrDefault()!.FirstName} {Reporter.FirstOrDefault()!.LastName}".Trim() : string.Empty,
                                                  IsOverQouta = expt.IsOverQuota,
                                                  SendType = "KX",
                                                  OkTtl = expt.TtQty,
                                                  OkWg = (decimal)expt.TtWg!,
                                              }).ToListAsync();

            filteredResult.AddRange(tempPacks);

            return filteredResult;
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

        private async Task<string> GenerateReceiveNoAsync(ReceiveType type)
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


        public class SendStockInput
        {
            public SendStockInput()
            {
                LotNo = string.Empty;
                BillNumber = 0;
                SizeNoOrd = false;
                ItemSend = "01";
                KsQty = 0;
                KsWg = 0;
                KmQty = 0;
                KmWg = 0;
                KmDes = 0;
                KxQty = 0;
                KxWg = 0;
                ReturnFound = false;
                UserId = string.Empty;
            }

            public string LotNo { get; set; } = string.Empty;
            public int BillNumber { get; set; } = 0;
            public bool SizeNoOrd { get; set; } = false;
            public string ItemSend { get; set; } = string.Empty;

            public decimal KsQty { get; set; } = 0;
            public decimal KsWg { get; set; } = 0;

            public decimal KmQty { get; set; } = 0;
            public decimal KmWg { get; set; } = 0;
            public int KmDes { get; set; } = 0;

            public decimal KxQty { get; set; } = 0;
            public decimal KxWg { get; set; } = 0;
            public int Approver { get; set; } = 0;

            public bool ReturnFound { get; set; } = false;
            public string UserId { get; set; } = string.Empty;
        }

        public class OrderToStoreModel
        {
            public string OrderNo { get; set; } = string.Empty;
            public string CustCode { get; set; } = string.Empty;
            public string Article { get; set; } = string.Empty;
            public string LotNo { get; set; } = string.Empty;
            public string ListNo { get; set; } = string.Empty;
            public decimal TtQty { get; set; } = 0;
            public double TtWg { get; set; } = 0;
            public decimal Si { get; set; } = 0;
            public decimal SendPack_Qty { get; set; } = 0;
            public decimal SendToPack_Qty { get; set; } = 0;
            public decimal Packed_Qty { get; set; } = 0;

            public decimal Store_Qty { get; set; } = 0;
            public double Store_Wg { get; set; } = 0;
            public bool IsStoreSended { get; set; } = false;
            public bool IsStored { get; set; } = false;

            public decimal Melt_Qty { get; set; } = 0;
            public double Melt_Wg { get; set; } = 0;
            public int BreakDescriptionId { get; set; } = 0;
            public bool IsMeltSended { get; set; } = false;
            public bool IsMelted { get; set; } = false;


            public decimal Export_Qty { get; set; } = 0;
            public double Export_Wg { get; set; } = 0;
            public bool IsExportSended { get; set; } = false;

            public decimal Percentage { get; set; } = 0;
        }

        public sealed class SaveResult
        {
            public string ReceiveNo { get; init; } = "";
            public int InsertedDetails { get; init; }
            public int UpdatedSendStockRows { get; init; }
            public int UpdatedJobBills { get; init; }
        }

    }
}
