﻿using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace JPStockPacking.Services.Implement
{
    public class BreakService(JPDbContext jPDbContext, SPDbContext sPDbContext) : IBreakService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<LostAndRepairModel>> GetBreakAsync(BreakAndLostFilterModel breakAndLostFilterModel)
        {
            var result = await (
                from bek in _sPDbContext.Break
                join rev in _sPDbContext.Received on bek.ReceivedId equals rev.ReceivedId
                join lot in _sPDbContext.Lot on rev.LotNo equals lot.LotNo
                join ord in _sPDbContext.Order on lot.OrderNo equals ord.OrderNo
                join desc in _sPDbContext.BreakDescription on bek.BreakDescriptionId equals desc.BreakDescriptionId into descJoin
                from desc in descJoin.DefaultIfEmpty()
                select new LostAndRepairModel
                {
                    BreakID = bek.BreakId,
                    ReceiveNo = rev.ReceiveNo,
                    LotNo = rev.LotNo,
                    OrderQty = lot.TtQty,
                    TtQty = rev.TtQty ?? 0,
                    TtWg = Math.Round((double)(rev.TtWg ?? 0), 2),
                    Barcode = rev.Barcode,
                    Article = lot.Article ?? string.Empty,
                    OrderNo = lot.OrderNo,
                    CustCode = ord.CustCode ?? string.Empty,
                    ListNo = lot.ListNo,
                    BreakQty = bek.BreakQty,
                    PreviousQty = bek.PreviousQty,
                    IsReported = bek.IsReported,
                    BreakDescription = desc.Name ?? string.Empty,
                    SeldDate1 = ord.SeldDate1.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
                    CreateDate = bek.CreateDate.GetValueOrDefault().ToString("dd MMMM yyyy", new CultureInfo("th-TH")) ?? "",
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

            if (breakAndLostFilterModel.BreakIDs != null && breakAndLostFilterModel.BreakIDs.Length > 0)
            {
                result = result.Where(r => breakAndLostFilterModel.BreakIDs.Contains(r.BreakID)).ToList();
            }

            return [.. result.OrderByDescending(x => x.CreateDate)];
        }

        public async Task AddBreakAsync(string lotNo, double breakQty, int breakDes)
        {
            if (string.IsNullOrWhiteSpace(lotNo))
                throw new ArgumentException("LotNo ไม่สามารถว่างได้", nameof(lotNo));

            if (breakQty <= 0)
                throw new ArgumentException("BreakQty ต้องมากกว่า 0", nameof(breakQty));

            var lotExists = await _sPDbContext.Lot.FirstOrDefaultAsync(l => l.LotNo == lotNo && l.IsActive);

            var revs = await _sPDbContext.Received
                .Where(l => l.LotNo == lotNo && l.IsActive)
                .OrderByDescending(r => r.TtQty)
                .ToListAsync();

            if (revs.Count == 0)
                throw new KeyNotFoundException($"ไม่พบ LotNo: {lotNo}");

            //  เช็ครวมยอด ttqty ก่อนเข้าลูป
            int totalQty = revs.Sum(r => (int)(r.TtQty ?? 0));
            if (breakQty > totalQty)
                throw new InvalidOperationException($"จำนวน Break ({breakQty}) มากกว่ายอดรวมทั้งหมด ({totalQty}) ของ Lot {lotNo}");

            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                double remainingQty = breakQty;

                foreach (var rev in revs)
                {
                    if (remainingQty <= 0)
                        break;

                    double ttQty = (double)rev.TtQty!;
                    if (ttQty <= 0)
                        continue;

                    double newQty = ttQty - remainingQty;
                    double oldQty = (double)rev.TtQty!;
                    double oldWg = rev.TtWg ?? 0;
                    double newWg = oldQty > 0 ? (oldWg / oldQty) * newQty : 0;

                    if (newQty >= 0)
                    {
                        // Break ได้พอดี หรือไม่เกิน
                        _sPDbContext.Break.Add(new Break
                        {
                            ReceivedId = rev.ReceivedId,
                            BreakQty = (decimal)remainingQty,
                            PreviousQty = (decimal)oldQty,
                            BreakDescriptionId = breakDes,
                            IsReported = false,
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        });

                        lotExists!.ReceivedQty = (lotExists.ReceivedQty ?? 0) - (decimal)remainingQty;
                        lotExists.AssignedQty = (lotExists.AssignedQty ?? 0) - (decimal)remainingQty;
                        lotExists.ReturnedQty = Math.Max((lotExists.ReturnedQty ?? 0) - (decimal)remainingQty, 0);

                        rev.TtQty = (decimal)newQty;
                        rev.TtWg = Math.Round(newWg, 2);
                        rev.UpdateDate = DateTime.Now;
                        remainingQty = 0;

                        await UpdateJobBillSendStockAndSpdreceive(rev.BillNumber, (decimal)newQty, (decimal)Math.Round(newWg, 2));
                    }
                    else
                    {
                        // break เกิน rev นี้ ต้องไปต่อ rev ถัดไป
                        double breakUsed = ttQty;
                        _sPDbContext.Break.Add(new Break
                        {
                            ReceivedId = rev.ReceivedId,
                            BreakQty = (decimal)breakUsed,
                            PreviousQty = (decimal)oldQty,
                            BreakDescriptionId = breakDes,
                            IsReported = false,
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        });

                        lotExists!.ReceivedQty = (lotExists.ReceivedQty ?? 0) - (decimal)breakUsed;
                        lotExists.AssignedQty = (lotExists.AssignedQty ?? 0) - (decimal)breakUsed;
                        lotExists.ReturnedQty = Math.Max((lotExists.ReturnedQty ?? 0) - (decimal)breakUsed, 0);

                        rev.TtQty = 0;
                        rev.TtWg = 0;
                        rev.UpdateDate = DateTime.Now;

                        remainingQty = Math.Abs(newQty); // จำนวนที่เหลือต้องไปหักต่อ

                        await UpdateJobBillSendStockAndSpdreceive(rev.BillNumber, 0, 0);
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

        public async Task UpdateJobBillSendStockAndSpdreceive(int Billnumber, decimal TtQty, decimal TtWg)
        {
            using var transaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var spdreceive = await _jPDbContext.Spdreceive.FirstOrDefaultAsync(o => o.Billnumber == Billnumber);
                if (spdreceive != null)
                {
                    spdreceive.Ttqty = TtQty;
                    spdreceive.Ttwg = TtWg;
                }

                var jobBillSendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(o => o.Billnumber == Billnumber);
                if (jobBillSendStock != null)
                {
                    jobBillSendStock.Ttqty = TtQty;
                    jobBillSendStock.Ttwg = TtWg;
                }

                //await _jPDbContext.SaveChangesAsync();
                //await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<BreakDescription>> GetBreakDescriptionsAsync()
        {
            var des = await _sPDbContext.BreakDescription
                .Where(x => x.IsActive)
                .Select(x => new BreakDescription
                {
                    BreakDescriptionId = x.BreakDescriptionId,
                    Name = x.Name,
                })
                .ToListAsync();

            return [.. des];
        }

        public async Task<List<BreakDescription>> AddNewBreakDescription(string breakDescription)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                BreakDescription newDescription = new()
                {
                    Name = breakDescription,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                _sPDbContext.BreakDescription.Add(newDescription);
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetBreakDescriptionsAsync();
        }


        public async Task PintedBreakReport(int[]? BreakIDs)
        {
            var breaks = await _sPDbContext.Break.Where(b => BreakIDs!.Contains(b.BreakId) && b.IsActive).ToListAsync();

            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var bek in breaks)
                {
                    if (bek.IsReported) continue;

                    bek.IsReported = true;
                    bek.UpdateDate = DateTime.Now;
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
