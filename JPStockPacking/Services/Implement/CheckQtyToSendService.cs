using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class CheckQtyToSendService(JPDbContext jPDbContext, SPDbContext sPDbContext, IPISService pISService, IConfiguration configuration) : ICheckQtyToSendService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IPISService _pISService = pISService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<SendToPackModel> GetOrderToSendQtyAsync(string orderNo)
        {
            var apiSettings = _configuration.GetSection("SendQtySettings");
            var Persentage = apiSettings["Persentage"];

            var OrdLotno = _jPDbContext.OrdLotno
                .Where(x => x.OrderNo == orderNo)
                .Select(x => x.LotNo)
                .Distinct();

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
                }
            ).ToListAsync();

            if (baseData is not { Count: > 0 }) return new SendToPackModel();

            var headerId = await _sPDbContext.SendQtyToPack
                .Where(x => x.OrderNo == orderNo && x.IsActive)
                .Select(x => x.SendQtyToPackId)
                .FirstOrDefaultAsync();

            Dictionary<string, (decimal TtQty, int? Approver)> lotDetailDict = new(StringComparer.OrdinalIgnoreCase);

            if (headerId > 0)
            {
                lotDetailDict = await _sPDbContext.SendQtyToPackDetail
                    .Where(d => d.SendQtyToPackId == headerId && d.IsActive)
                    .GroupBy(d => d.LotNo)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => (
                            g.Last().TtQty,
                            g.Last().Approver
                        ),
                        StringComparer.OrdinalIgnoreCase
                    );
            }

            var lotNos = baseData.Select(x => x.LotNo).Where(x => x != null).Distinct().ToList();
            var sizeMap = await GetSizeByLotBulkAsync(orderNo, lotNos);

            var lots = new List<SendToPackLots>();

            foreach (var x in baseData)
            {
                lotDetailDict.TryGetValue(x.LotNo ?? "", out var lotDetail);
                sizeMap.TryGetValue(x.LotNo ?? "", out var sizes);

                if (lotDetail.Approver != 0 && lotDetail.Approver != null)
                {
                    List<UserModel> user = await _pISService.GetUser(new ReqUserModel { UserID = lotDetail.Approver });

                    var lot = new SendToPackLots
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
                        TtQtyToPack = lotDetail.TtQty,
                        IsDefined = lotDetail.TtQty > 0,
                        ApproverID = user.FirstOrDefault()!.UserID.ToString(),
                        Approver = $"{user.FirstOrDefault()!.FirstName} {user.FirstOrDefault()!.LastName}".Trim(),
                        Persentage = int.TryParse(Persentage, out var p) ? p : 0,
                        Size = sizes ?? []
                    };

                    lots.Add(lot);
                }
                else
                {
                    var lot = new SendToPackLots
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
                        TtQtyToPack = lotDetail.TtQty,
                        IsDefined = lotDetail.TtQty > 0,
                        ApproverID = string.Empty,
                        Approver = string.Empty,
                        Persentage = int.TryParse(Persentage, out var p) ? p : 0,
                        Size = sizes ?? []
                    };

                    lots.Add(lot);
                }


            }

            return new SendToPackModel
            {
                OrderNo = orderNo,
                CustCode = baseData[0].CustCode ?? string.Empty,
                Grade = baseData[0].Grade ?? string.Empty,
                SCountry = baseData[0].Scountry ?? string.Empty,
                Special = baseData[0].Special ?? string.Empty,
                IsOrderDefined = lots.Any(l => l.IsDefined),
                Persentage = int.TryParse(Persentage, out var pv) ? pv : 0,
                Lots = lots
            };
        }

        public async Task<SendToPackModel> GetOrderToSendQtyWithPriceAsync(string orderNo)
        {
            var apiSettings = _configuration.GetSection("SendQtySettings");
            var Persentage = apiSettings["Persentage"];

            var OrdLotno = _jPDbContext.OrdLotno
                .Where(x => x.OrderNo == orderNo)
                .Select(x => x.LotNo)
                .Distinct();

            var baseData = await (
                from lot in OrdLotno
                from b in _jPDbContext.OrdLotno.Where(x => x.OrderNo == orderNo && x.LotNo == lot).Take(1)
                join a in _jPDbContext.OrdHorder on b.OrderNo equals a.OrderNo
                join c in _jPDbContext.CpriceSale on b.Barcode equals c.Barcode into gj2
                from c in gj2.DefaultIfEmpty()
                join e in _jPDbContext.JobCost on new { b.LotNo, b.OrderNo } equals new { LotNo = e.Lotno, OrderNo = e.Orderno } into gj4
                from e in gj4.DefaultIfEmpty()
                join f in _jPDbContext.CfnCode on c.FnCode equals f.FnCode into gj5
                from f in gj5.DefaultIfEmpty()
                join g in _jPDbContext.Cprofile on c.Article equals g.Article into gj6
                from g in gj6.DefaultIfEmpty()
                join h in _jPDbContext.OrdDorder on new { b.OrderNo, b.LotNo } equals new { h.OrderNo, h.LotNo } into gj7
                from h in gj7.DefaultIfEmpty()
                join d in (
                    from spd in _jPDbContext.Spdreceive
                    join sph in _jPDbContext.Sphreceive on spd.ReceiveNo equals sph.ReceiveNo into sphj
                    from sph in sphj.DefaultIfEmpty()
                    group spd by spd.Lotno into gg
                    select new { Lotno = gg.Key, TTQty = gg.Sum(x => (decimal?)x.Ttqty) }
                ) on b.LotNo equals d.Lotno into gj3
                from d in gj3.DefaultIfEmpty()

                where a.Factory == true
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
                    SendPack_Qty = d != null ? (d.TTQty ?? 0) : 0,
                    Price = string.IsNullOrEmpty(b.SetNo1) ? (b.Price ?? 0) : (h.Price ?? 0),
                    PriceDec = a.DecimalPrice != 0 ? a.DecimalPrice : 2
                }
            ).ToListAsync();

            if (baseData is not { Count: > 0 }) return new SendToPackModel();

            var headerId = await _sPDbContext.SendQtyToPack
                .Where(x => x.OrderNo == orderNo && x.IsActive)
                .Select(x => x.SendQtyToPackId)
                .FirstOrDefaultAsync();

            Dictionary<string, (decimal TtQty, int? Approver)> lotDetailDict = new(StringComparer.OrdinalIgnoreCase);

            if (headerId > 0)
            {
                lotDetailDict = await _sPDbContext.SendQtyToPackDetail
                    .Where(d => d.SendQtyToPackId == headerId && d.IsActive)
                    .GroupBy(d => d.LotNo)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => (
                            g.Last().TtQty,
                            g.Last().Approver
                        ),
                        StringComparer.OrdinalIgnoreCase
                    );
            }

            var lotNos = baseData.Select(x => x.LotNo).Where(x => x != null).Distinct().ToList();
            var sizeMap = await GetSizeByLotBulkAsync(orderNo, lotNos);

            var lots = new List<SendToPackLots>();

            foreach (var x in baseData)
            {
                lotDetailDict.TryGetValue(x.LotNo ?? "", out var lotDetail);
                sizeMap.TryGetValue(x.LotNo ?? "", out var sizes);

                if (lotDetail.Approver != 0 && lotDetail.Approver != null)
                {
                    List<UserModel> user = await _pISService.GetUser(new ReqUserModel { UserID = lotDetail.Approver });

                    var lot = new SendToPackLots
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
                        EnPrice = x.Price.ToString().EncodeToText(),
                        EnTtPrice = ((decimal)x.Price * x.TtQty ?? 0).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText(),
                        DePrice = x.Price,
                        DeTtPrice = x.Price * (double)x.TtQty!,

                        EnSendQtyPrice = ((decimal)x.Price * lotDetail.TtQty).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText(),
                        DeSendQtyPrice = x.Price * (double)lotDetail.TtQty,

                        TtQty = x.TtQty ?? 0,
                        QtySi = x.QtySi,
                        SendTtQty = x.SendPack_Qty,
                        TtQtyToPack = lotDetail.TtQty,
                        IsDefined = lotDetail.TtQty > 0,
                        Approver = $"{user.FirstOrDefault()!.FirstName} {user.FirstOrDefault()!.LastName}".Trim(),
                        Persentage = int.TryParse(Persentage, out var p) ? p : 0,
                        Size = sizes ?? []
                    };

                    lots.Add(lot);
                }
                else
                {
                    var lot = new SendToPackLots
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
                        EnPrice = x.Price.ToString().EncodeToText(),
                        EnTtPrice = ((decimal)x.Price * x.TtQty ?? 0).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText(),
                        DePrice = x.Price,
                        DeTtPrice = x.Price * (double)x.TtQty!,

                        EnSendQtyPrice = ((decimal)x.Price * lotDetail.TtQty).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText(),
                        DeSendQtyPrice = x.Price * (double)lotDetail.TtQty,

                        TtQty = x.TtQty ?? 0,
                        QtySi = x.QtySi,
                        SendTtQty = x.SendPack_Qty,
                        TtQtyToPack = lotDetail.TtQty,
                        IsDefined = lotDetail.TtQty > 0,
                        Approver = string.Empty,
                        Persentage = int.TryParse(Persentage, out var p) ? p : 0,
                        Size = sizes ?? []
                    };

                    lots.Add(lot);
                }


            }

            return new SendToPackModel
            {
                OrderNo = orderNo,
                CustCode = baseData[0].CustCode ?? string.Empty,
                Grade = baseData[0].Grade ?? string.Empty,
                SCountry = baseData[0].Scountry ?? string.Empty,
                Special = baseData[0].Special ?? string.Empty,
                IsOrderDefined = lots.Any(l => l.IsDefined),
                SumTtQty = (double)lots.Sum(l => l.TtQty),
                SumTtPrice = lots.Sum(l => l.DeTtPrice).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText().ToUpper(),
                Persentage = int.TryParse(Persentage, out var pv) ? pv : 0,
                SumSendTtQty = (double)lots.Sum(l => l.SendTtQty),
                SumSendTtPrice = lots.Sum(l => l.DeSendQtyPrice).ToString(string.Format("N{0}", baseData[0].PriceDec)).EncodeToText().ToUpper(),
                Lots = lots
            };
        }

        private async Task<Dictionary<string, List<Size>>> GetSizeByLotBulkAsync(string orderNo, List<string> lotNos)
        {
            var apiSettings = _configuration.GetSection("SendQtySettings");
            var Persentage = apiSettings["Persentage"];

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
                        if (approver != 0)
                        {
                            List<UserModel> user = await _pISService.GetUser(new ReqUserModel { UserID = approver });

                            sizes.Add(new Size
                            {
                                S = s ?? "",
                                CS = cs ?? "",
                                Q = q,
                                TtQtyToPack = qtyToPack,
                                IsDefined = qtyToPack > 0,
                                IsUnderQuota = isUnderQuota,
                                ApproverID = user.FirstOrDefault()!.UserID.ToString(),
                                Approver = user.FirstOrDefault()!.Username,
                            });
                        }
                        else
                        {
                            sizes.Add(new Size
                            {
                                S = s ?? "",
                                CS = cs ?? "",
                                Q = q,
                                TtQtyToPack = qtyToPack,
                                IsDefined = qtyToPack > 0,
                                IsUnderQuota = isUnderQuota,
                                ApproverID = string.Empty,
                                Approver = string.Empty
                            });
                        }
                    }
                }

                result[lot.LotNo!] = sizes;
            }

            return result;
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

                            var oldSizes = await _sPDbContext.SendQtyToPackDetailSize.Where(s => s.SendQtyToPackDetailId == existing.SendQtyToPackDetailId && s.IsActive).ToListAsync();

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

                            if (lot.LotApprover != 0)
                            {
                                newDetail.Approver = lot.LotApprover;
                                newDetail.IsUnderQuota = true;
                            }

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

                                if (size.SizeApprover != 0)
                                {
                                    newSizeDetail.Approver = size.SizeApprover;
                                    newSizeDetail.IsUnderQuota = true;
                                }

                                _sPDbContext.SendQtyToPackDetailSize.Add(newSizeDetail);
                                await _sPDbContext.SaveChangesAsync();
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

                        if (lot.LotApprover != 0)
                        {
                            newDetail.Approver = lot.LotApprover;
                            newDetail.IsUnderQuota = true;
                        }

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

                            if (size.SizeApprover != 0)
                            {
                                newSizeDetail.Approver = size.SizeApprover;
                                newSizeDetail.IsUnderQuota = true;
                            }

                            _sPDbContext.SendQtyToPackDetailSize.Add(newSizeDetail);
                            await _sPDbContext.SaveChangesAsync();
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

    }
}
