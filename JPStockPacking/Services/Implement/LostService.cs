using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace JPStockPacking.Services.Implement
{
    public class LostService(SPDbContext sPDbContext) : ILostService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<LostAndRepairModel>> GetLostAsync(BreakAndLostFilterModel breakAndLostFilterModel)
        {
            var result = await (
                from los in _sPDbContext.Lost
                join lot in _sPDbContext.Lot on los.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                select new LostAndRepairModel
                {
                    LostID = los.LostId,
                    LotNo = los.LotNo,
                    Barcode = lot.Barcode ?? string.Empty,
                    Article = lot.Article ?? string.Empty,
                    OrderNo = lot.OrderNo,
                    ListNo = lot.ListNo,
                    SeldDate1 = ord.SeldDate1.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    CustCode = ord.CustCode ?? string.Empty,
                    LostQty = los.LostQty,
                    TtQty = lot.TtQty ?? 0,
                    TtWg = (double)(lot.TtWg ?? 0),
                    IsReported = los.IsReported,
                    CreateDateTH = los.CreateDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    CreateDate = los.CreateDate.GetValueOrDefault()
                }
            ).ToListAsync();

            if (result == null || result.Count == 0)
            {
                return [];
            }

            if (!string.IsNullOrWhiteSpace(breakAndLostFilterModel.LotNo))
            {
                result = result.Where(r => r.LotNo == breakAndLostFilterModel.LotNo).ToList();
            }

            if (breakAndLostFilterModel.LostIDs != null && breakAndLostFilterModel.LostIDs.Length > 0)
            {
                result = result.Where(r => breakAndLostFilterModel.LostIDs.Contains(r.LostID)).ToList();
            }

            return [.. result.OrderByDescending(x => x.CreateDate)];
        }

        public async Task AddLostAsync(string lotNo, double lostQty)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var sumLost = await _sPDbContext.Lost.Where(l => l.LotNo == lotNo && l.IsActive).SumAsync(l => (double?)l.LostQty) ?? 0;

                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(l => l.LotNo == lotNo && l.IsActive) ?? throw new KeyNotFoundException($"ไม่พบ LotNo: {lotNo}");

                if (sumLost > (double?)lot.TtQty)
                {
                    throw new Exception("จำนวน Lost มากกว่า จำนวนทั้งหมด");
                }

                lot.AssignedQty = (lot.AssignedQty ?? 0) - (decimal)lostQty;
                lot.ReceivedQty = (lot.ReceivedQty ?? 0) - (decimal)lostQty;
                lot.ReturnedQty = Math.Max((lot.ReturnedQty ?? 0) - (decimal)lostQty, 0);

                Lost lost = new()
                {
                    LotNo = lotNo,
                    LostQty = (decimal)lostQty,
                    IsReported = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                _sPDbContext.Lost.Add(lost);

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task PintedLostReport(int[]? LostIDs)
        {
            var losts = await _sPDbContext.Lost.Where(b => LostIDs!.Contains(b.LostId) && b.IsActive).ToListAsync();

            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var los in losts)
                {
                    if (los.IsReported) continue;

                    los.IsReported = true;
                    los.UpdateDate = DateTime.Now;
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
