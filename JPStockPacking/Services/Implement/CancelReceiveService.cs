using Dapper;
using JPStockPacking.Data.JPDbContext;
using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog.Parsing;
using System.Globalization;

namespace JPStockPacking.Services.Implement
{
    public class CancelReceiveService(SPDbContext sPDbContext, JPDbContext jPDbContext) : ICancelReceiveService
    {
        private readonly JPDbContext _jPDbContext = jPDbContext;
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<ReceivedListModel>> GetTopSJ1JPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo)
        {
            var query =
                from a in _jPDbContext.Sj1hreceive
                select new
                {
                    a.ReceiveNo,
                    a.Mdate,
                    a.Mupdate,
                    a.Cancel
                };

            if (!string.IsNullOrWhiteSpace(receiveNo))
            {
                query = query.Where(b => b.ReceiveNo.Contains(receiveNo));
            }

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                query = query.Where(b => _jPDbContext.Sj1dreceive
                    .Join(_jPDbContext.OrdLotno, sr => sr.Lotno, ol => ol.LotNo, (sr, ol) => new { sr, ol })
                    .Any(joined => joined.ol.OrderNo.Contains(orderNo) && joined.sr.ReceiveNo == b.ReceiveNo));

            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                query = query.Where(b => _jPDbContext.Sj1dreceive.Any(sr => sr.Lotno.Contains(lotNo) && sr.ReceiveNo == b.ReceiveNo));
            }

            var receives = await query.OrderByDescending(o => o.Mdate).Take(100).ToListAsync();

            var result = receives.Select(r => new ReceivedListModel
            {
                ReceiveNo = r.ReceiveNo,
                Mdate = r.Mdate.ToString("dd MMMM yyyy", new CultureInfo("th-TH")),
                IsReceived = r.Mupdate,
                IsCancel = r.Cancel,
            }).ToList();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetSJ1JPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo)
        {
            var allReceived = await (
                from a in _jPDbContext.Sj1dreceive
                join h in _jPDbContext.Sj1hreceive on a.ReceiveNo equals h.ReceiveNo
                join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                where a.ReceiveNo == receiveNo && !string.IsNullOrEmpty(a.Lotno)
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
                    c.ListNo,
                    d.CustCode,
                    IsReceived = h.Mupdate
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

            var result = allReceived.Select(x => new ReceivedListModel
            {
                ReceivedID = x.Id,
                ReceiveNo = x.ReceiveNo,
                CustCode = x.CustCode,
                LotNo = x.Lotno,
                TtQty = x.Ttqty,
                TtWg = (double)x.Ttwg,
                Barcode = x.Barcode,
                Article = x.Article,
                OrderNo = x.OrderNo,
                ListNo = x.ListNo,
                IsReceived = x.IsReceived
            }).ToList();

            result = [.. result.OrderBy(x => x.LotNo).ThenBy(x => x.OrderNo).ThenBy(x => x.ListNo)];

            return result;
        }

        public async Task<BaseResponseModel> CancelSJ1ByReceiveNoAsync(string receiveNo, int userId)
        {
            var hasDetails = await _sPDbContext.Store.AnyAsync(d => d.Doc == receiveNo);
            if (!hasDetails)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูล: {receiveNo} ในระบบ"
                };
            }

            // 1. หา details ที่จะยกเลิก (ก่อนเริ่ม transaction)
            var details = await _jPDbContext.Sj1dreceive
                .Where(d => d.ReceiveNo == receiveNo)
                .ToListAsync();

            if (details == null || details.Count == 0)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูลเลขที่รับเข้า: {receiveNo}"
                };
            }

            await using var jpTransaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in details)
                {
                    // เคลียร์ JobBillSendStock.Doc
                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == detail.Billnumber &&
                        a.Numsend == detail.Numsend &&
                        a.Doc == receiveNo);

                    if (sendStock != null)
                    {

                        _jPDbContext.Remove(sendStock);
                        await _jPDbContext.SaveChangesAsync();
                    }

                    // เคลียร์ JobBill.SendStockDoc  
                    var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b =>
                        b.Billnumber == detail.Billnumber &&
                        b.SendStockDoc == receiveNo);

                    if (jobBill != null)
                    {
                        jobBill.SendStockDoc = string.Empty;
                        await _jPDbContext.SaveChangesAsync();

                    }
                }

                // ลบ Sj1dreceive
                _jPDbContext.Sj1dreceive.RemoveRange(details);

                // ลบ Sj1hreceive
                var header = await _jPDbContext.Sj1hreceive.FirstOrDefaultAsync(h => h.ReceiveNo == receiveNo);
                if (header != null)
                {
                    header.Cancel = true;
                }

                await _jPDbContext.SaveChangesAsync();
                await jpTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await jpTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาดที่ JP: {ex.Message}"
                };
            }

            await using var spTransaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in details)
                {
                    var store = await _sPDbContext.Store.FirstOrDefaultAsync(x =>
                        x.LotNo == detail.Lotno &&
                        x.BillNumber == detail.Billnumber &&
                        x.Doc == receiveNo);

                    if (store != null)
                    {
                        _sPDbContext.Remove(store);
                        await _sPDbContext.SaveChangesAsync();
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await spTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await spTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"ยกเลิก JP สำเร็จ แต่ SP ล้มเหลว: {ex.Message}"
                };
            }

            return new BaseResponseModel
            {
                Code = 200,
                IsSuccess = true,
                Message = $"ยกเลิกใบส่งเก็บ {receiveNo} สำเร็จ"
            };
        }

        public async Task<BaseResponseModel> CancelSJ1ByLotNoAsync(string receiveNo, string[] lotNos, int userId)
        {
            var hasDetails = await _sPDbContext.Store.AnyAsync(d => d.Doc == receiveNo);
            if (!hasDetails)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูล: {receiveNo} ในระบบ"
                };
            }

            // 1. หา details ที่จะยกเลิก (ก่อนเริ่ม transaction)
            var allDetails = await _jPDbContext.Sj1dreceive
                .Where(d => d.ReceiveNo == receiveNo)
                .ToListAsync();

            var details = allDetails.Where(d => lotNos.Contains(d.Lotno)).ToList();

            if (details == null || details.Count == 0)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูลที่ต้องการยกเลิก"
                };
            }

            await using var jpTransaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in details)
                {
                    // เคลียร์ JobBillSendStock.Doc
                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == detail.Billnumber &&
                        a.Numsend == detail.Numsend &&
                        a.Doc == receiveNo);

                    if (sendStock != null)
                    {
                        
                        _jPDbContext.Remove(sendStock);
                        await _jPDbContext.SaveChangesAsync();

                    }

                    // เคลียร์ JobBill.SendStockDoc (เฉพาะถ้าไม่มี detail อื่นเหลือ)
                    var remainingDetails = allDetails.Count(d => d.Billnumber == detail.Billnumber && !lotNos.Contains(d.Lotno));

                    if (remainingDetails == 0)
                    {
                        var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b =>
                            b.Billnumber == detail.Billnumber &&
                            b.SendStockDoc == receiveNo);

                        if (jobBill != null)
                        {
                            jobBill.SendStockDoc = string.Empty;
                            await _jPDbContext.SaveChangesAsync();

                        }
                    }
                }

                // ลบ Sj1dreceive เฉพาะรายการที่เลือก
                _jPDbContext.Sj1dreceive.RemoveRange(details);

                // ถ้าไม่มี detail เหลือ ให้ลบ header ด้วย
                var remainingCount = allDetails.Count(d => !lotNos.Contains(d.Lotno));

                if (remainingCount == 0)
                {
                    var header = await _jPDbContext.Sj1hreceive.FirstOrDefaultAsync(h => h.ReceiveNo == receiveNo);
                    if (header != null)
                    {
                        header.Cancel = true;
                    }
                }

                await _jPDbContext.SaveChangesAsync();
                await jpTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await jpTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาดที่ JP: {ex.Message}"
                };
            }

            await using var spTransaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in details)
                {
                    var store = await _sPDbContext.Store.FirstOrDefaultAsync(x =>
                        x.LotNo == detail.Lotno &&
                        x.BillNumber == detail.Billnumber &&
                        x.Doc == receiveNo);

                    if (store != null)
                    {
                        _sPDbContext.Remove(store);
                        await _sPDbContext.SaveChangesAsync();
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await spTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await spTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"ยกเลิก JP สำเร็จ แต่ SP ล้มเหลว: {ex.Message}"
                };
            }

            return new BaseResponseModel
            {
                Code = 200,
                IsSuccess = true,
                Message = $"ยกเลิก {details.Count} รายการสำเร็จ"
            };
        }

        public async Task<List<ReceivedListModel>> GetTopSJ2JPReceivedAsync(string? receiveNo, string? orderNo, string? lotNo)
        {
            var query =
                from a in _jPDbContext.Sj2hreceive
                select new
                {
                    a.ReceiveNo,
                    a.Mdate,
                    a.Mupdate,
                    a.Cancel
                };

            if (!string.IsNullOrWhiteSpace(receiveNo))
            {
                query = query.Where(b => b.ReceiveNo.Contains(receiveNo));
            }

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                query = query.Where(b => _jPDbContext.Sj2dreceive
                    .Join(_jPDbContext.OrdLotno, sr => sr.Lotno, ol => ol.LotNo, (sr, ol) => new { sr, ol })
                    .Any(joined => joined.ol.OrderNo.Contains(orderNo) && joined.sr.ReceiveNo == b.ReceiveNo));
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                query = query.Where(b => _jPDbContext.Sj2dreceive.Any(sr => sr.Lotno.Contains(lotNo) && sr.ReceiveNo == b.ReceiveNo));
            }

            var receives = await query.OrderByDescending(o => o.Mdate).Take(100).ToListAsync();

            var result = receives.Select(r => new ReceivedListModel
            {
                ReceiveNo = r.ReceiveNo,
                Mdate = r.Mdate.ToString("dd MMMM yyyy", new CultureInfo("th-TH")),
                IsReceived = r.Mupdate,
                IsCancel = r.Cancel,
            }).ToList();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetSJ2JPReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo)
        {
            var allReceived = await (
                from a in _jPDbContext.Sj2dreceive
                join h in _jPDbContext.Sj2hreceive on a.ReceiveNo equals h.ReceiveNo
                join c in _jPDbContext.OrdLotno on a.Lotno equals c.LotNo
                join d in _jPDbContext.OrdHorder on c.OrderNo equals d.OrderNo
                where a.ReceiveNo == receiveNo && !string.IsNullOrEmpty(a.Lotno)
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
                    c.ListNo,
                    d.CustCode,
                    IsReceived = h.Mupdate
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

            var result = allReceived.Select(x => new ReceivedListModel
            {
                ReceivedID = x.Id,
                ReceiveNo = x.ReceiveNo,
                CustCode = x.CustCode,
                LotNo = x.Lotno,
                TtQty = x.Ttqty,
                TtWg = (double)x.Ttwg,
                Barcode = x.Barcode,
                Article = x.Article,
                OrderNo = x.OrderNo,
                ListNo = x.ListNo,
                IsReceived = x.IsReceived
            }).ToList();

            result = [.. result.OrderBy(x => x.LotNo).ThenBy(x => x.OrderNo).ThenBy(x => x.ListNo)];

            return result;
        }

        public async Task<BaseResponseModel> CancelSJ2ByReceiveNoAsync(string receiveNo, int userId)
        {
            var hasDetails = await _sPDbContext.Melt.AnyAsync(d => d.Doc == receiveNo);
            if (!hasDetails)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูล: {receiveNo} ในระบบ"
                };
            }

            // Query Sj2dreceive ด้วย Dapper (ไม่มี primary key)
            var jpConnection = _jPDbContext.Database.GetDbConnection();
            if (jpConnection.State != System.Data.ConnectionState.Open)
                await jpConnection.OpenAsync();

            var allDetails = (await jpConnection.QueryAsync<dynamic>(
                "SELECT Id, ReceiveNo, Billnumber, Numsend, Lotno FROM Sj2dreceive WHERE ReceiveNo = @ReceiveNo",
                new { ReceiveNo = receiveNo }
            )).ToList();

            if (allDetails == null || allDetails.Count == 0)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูลเลขที่รับเข้า: {receiveNo}"
                };
            }

            await using var jpTransaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var dbTransaction = jpTransaction.GetDbTransaction();

                foreach (var detail in allDetails)
                {
                    string lotno = (string)detail.Lotno;
                    int billnumber = (int)detail.Billnumber;
                    int numsend = (int)detail.Numsend;

                    // เคลียร์ JobBillSendStock.Doc (ใช้ EF Core)
                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == billnumber &&
                        a.Numsend == numsend &&
                        a.Doc == receiveNo);

                    if (sendStock != null)
                    {
                        _jPDbContext.Remove(sendStock);
                        await _jPDbContext.SaveChangesAsync();
                    }

                    // เคลียร์ JobBill.SendMeltDoc (ใช้ EF Core)
                    var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b =>
                        b.Billnumber == billnumber &&
                        b.SendMeltDoc == receiveNo);

                    if (jobBill != null)
                    {
                        jobBill.SendMeltDoc = string.Empty;
                        await _jPDbContext.SaveChangesAsync();
                    }
                }

                // ลบ Sj2dreceive ด้วย Dapper (ไม่มี primary key)
                await jpConnection.ExecuteAsync(
                    "DELETE FROM Sj2dreceive WHERE ReceiveNo = @ReceiveNo",
                    new { ReceiveNo = receiveNo },
                    transaction: dbTransaction
                );

                // อัพเดท Cancel ใน Sj2hreceive ด้วย Dapper (ไม่มี primary key)
                await jpConnection.ExecuteAsync(
                    "UPDATE Sj2hreceive SET Cancel = 1 WHERE ReceiveNo = @ReceiveNo",
                    new { ReceiveNo = receiveNo },
                    transaction: dbTransaction
                );

                await jpTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await jpTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาดที่ JP: {ex.Message}"
                };
            }

            await using var spTransaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in allDetails)
                {
                    string lotno = (string)detail.Lotno;
                    int billnumber = (int)detail.Billnumber;

                    var melt = await _sPDbContext.Melt.FirstOrDefaultAsync(x =>
                        x.LotNo == lotno &&
                        x.BillNumber == billnumber &&
                        x.Doc == receiveNo);

                    if (melt != null)
                    {
                        _sPDbContext.Remove(melt);
                        await _sPDbContext.SaveChangesAsync();
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await spTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await spTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"ยกเลิก JP สำเร็จ แต่ SP ล้มเหลว: {ex.Message}"
                };
            }

            return new BaseResponseModel
            {
                Code = 200,
                IsSuccess = true,
                Message = $"ยกเลิกใบส่งหลอม {receiveNo} สำเร็จ"
            };
        }

        public async Task<BaseResponseModel> CancelSJ2ByLotNoAsync(string receiveNo, string[] lotNos, int userId)
        {
            var hasDetails = await _sPDbContext.Melt.AnyAsync(d => d.Doc == receiveNo);
            if (!hasDetails)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูล: {receiveNo} ในระบบ"
                };
            }

            // Query Sj2dreceive ด้วย Dapper (ไม่มี primary key)
            var jpConnection = _jPDbContext.Database.GetDbConnection();
            if (jpConnection.State != System.Data.ConnectionState.Open)
                await jpConnection.OpenAsync();

            var allDetails = (await jpConnection.QueryAsync<dynamic>(
                "SELECT Id, ReceiveNo, Billnumber, Numsend, Lotno FROM Sj2dreceive WHERE ReceiveNo = @ReceiveNo",
                new { ReceiveNo = receiveNo }
            )).ToList();

            var details = allDetails.Where(d => lotNos.Contains((string)d.Lotno)).ToList();

            if (details == null || details.Count == 0)
            {
                return new BaseResponseModel
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = $"ไม่พบข้อมูลที่ต้องการยกเลิก"
                };
            }

            await using var jpTransaction = await _jPDbContext.Database.BeginTransactionAsync();
            try
            {
                var dbTransaction = jpTransaction.GetDbTransaction();

                foreach (var detail in details)
                {
                    string lotno = (string)detail.Lotno;
                    int billnumber = (int)detail.Billnumber;
                    int numsend = (int)detail.Numsend;

                    // เคลียร์ JobBillSendStock.Doc (ใช้ EF Core)
                    var sendStock = await _jPDbContext.JobBillSendStock.FirstOrDefaultAsync(a =>
                        a.Billnumber == billnumber &&
                        a.Numsend == numsend &&
                        a.Doc == receiveNo);

                    if (sendStock != null)
                    {
                        _jPDbContext.Remove(sendStock);
                        await _jPDbContext.SaveChangesAsync();
                    }

                    // เคลียร์ JobBill.SendMeltDoc (เฉพาะถ้าไม่มี detail อื่นเหลือ) (ใช้ EF Core)
                    var remainingDetails = allDetails.Count(d => (int)d.Billnumber == billnumber && !lotNos.Contains((string)d.Lotno));

                    if (remainingDetails == 0)
                    {
                        var jobBill = await _jPDbContext.JobBill.FirstOrDefaultAsync(b =>
                            b.Billnumber == billnumber &&
                            b.SendMeltDoc == receiveNo);

                        if (jobBill != null)
                        {
                            jobBill.SendMeltDoc = string.Empty;
                            await _jPDbContext.SaveChangesAsync();
                        }
                    }
                }

                // ลบ Sj2dreceive เฉพาะรายการที่เลือก ด้วย Dapper (ไม่มี primary key)
                foreach (var detail in details)
                {
                    int id = (int)detail.Id;
                    await jpConnection.ExecuteAsync(
                        "DELETE FROM Sj2dreceive WHERE Id = @Id",
                        new { Id = id },
                        transaction: dbTransaction
                    );
                }

                // ถ้าไม่มี detail เหลือ ให้ set Cancel ใน header
                var remainingCount = allDetails.Count(d => !lotNos.Contains((string)d.Lotno));

                if (remainingCount == 0)
                {
                    await jpConnection.ExecuteAsync(
                        "UPDATE Sj2hreceive SET Cancel = 1 WHERE ReceiveNo = @ReceiveNo",
                        new { ReceiveNo = receiveNo },
                        transaction: dbTransaction
                    );
                }

                await jpTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await jpTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาดที่ JP: {ex.Message}"
                };
            }

            await using var spTransaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in details)
                {
                    string lotno = (string)detail.Lotno;
                    int billnumber = (int)detail.Billnumber;

                    var melt = await _sPDbContext.Melt.FirstOrDefaultAsync(x =>
                        x.LotNo == lotno &&
                        x.BillNumber == billnumber &&
                        x.Doc == receiveNo);

                    if (melt != null)
                    {
                        _sPDbContext.Remove(melt);
                        await _sPDbContext.SaveChangesAsync();
                    }
                }

                await _sPDbContext.SaveChangesAsync();
                await spTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await spTransaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"ยกเลิก JP สำเร็จ แต่ SP ล้มเหลว: {ex.Message}"
                };
            }

            return new BaseResponseModel
            {
                Code = 200,
                IsSuccess = true,
                Message = $"ยกเลิก {details.Count} รายการสำเร็จ"
            };
        }

        public async Task<List<ReceivedListModel>> GetTopSendLostReceivedAsync(string? receiveNo, string? orderNo, string? lotNo)
        {
            var query =
                from a in _sPDbContext.SendLost
                where a.IsActive
                select new
                {
                    a.Doc,
                    a.CreateDate
                };

            if (!string.IsNullOrWhiteSpace(receiveNo))
            {
                query = query.Where(b => b.Doc.Contains(receiveNo));
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                query = query.Where(b => _sPDbContext.SendLostDetail.Any(sr => sr.LotNo.Contains(lotNo) && sr.Doc == b.Doc));
            }

            var receives = await query.OrderByDescending(o => o.CreateDate).Take(100).ToListAsync();

            var result = receives.Select(r => new ReceivedListModel
            {
                ReceiveNo = r.Doc,
                Mdate = r.CreateDate.HasValue
                    ? r.CreateDate.Value.ToString("dd MMMM yyyy", new CultureInfo("th-TH"))
                    : string.Empty,
                IsReceived = false,
            }).ToList();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetSendLostReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo)
        {
            var allReceived = await (
                from a in _sPDbContext.SendLostDetail
                join c in _sPDbContext.Lot on a.LotNo equals c.LotNo
                join d in _sPDbContext.Order on c.OrderNo equals d.OrderNo
                where a.Doc == receiveNo && !string.IsNullOrEmpty(a.LotNo) && a.IsActive
                select new
                {
                    a.SendLostId,
                    a.Doc,
                    a.LotNo,
                    a.TtQty,
                    a.TtWg,
                    c.Barcode,
                    c.Article,
                    c.OrderNo,
                    c.ListNo,
                    d.CustCode
                }
            ).ToListAsync();

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                allReceived = [.. allReceived.Where(x => x.OrderNo.Contains(orderNo))];
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                allReceived = [.. allReceived.Where(x => x.LotNo.Contains(lotNo))];
            }

            var result = allReceived.Select(x => new ReceivedListModel
            {
                ReceivedID = x.SendLostId,
                ReceiveNo = x.Doc,
                CustCode = x.CustCode,
                LotNo = x.LotNo,
                TtQty = x.TtQty,
                TtWg = (double)x.TtWg,
                Barcode = x.Barcode,
                Article = x.Article,
                OrderNo = x.OrderNo,
                ListNo = x.ListNo,
                IsReceived = false
            }).ToList();

            result = [.. result.OrderBy(x => x.LotNo).ThenBy(x => x.OrderNo).ThenBy(x => x.ListNo)];

            return result;
        }

        public async Task<BaseResponseModel> CancelSendLostByReceiveNoAsync(string receiveNo, int userId)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveHeader = _sPDbContext.SendLost.FirstOrDefault(sl => sl.Doc == receiveNo && sl.IsActive);
                if (receiveHeader != null)
                {
                    var receives = _sPDbContext.SendLostDetail.Where(sld => sld.Doc == receiveNo && sld.IsActive).ToList();

                    foreach (var detail in receives)
                    {
                        detail.IsActive = false;
                        detail.UpdateBy = userId;
                        detail.UpdateDate = DateTime.Now;

                        _sPDbContext.SendLostDetail.Update(detail);
                    }

                    receiveHeader.IsActive = false;
                    receiveHeader.UpdateBy = userId;
                    receiveHeader.UpdateDate = DateTime.Now;
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Cancel Send Lost successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Error occurred while canceling Send Lost: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseModel> CancelSendLostByLotNoAsync(string receiveNo, string[] lotNos, int userId)
        {
            var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveHeader = _sPDbContext.SendLost.FirstOrDefault(sl => sl.Doc == receiveNo && sl.IsActive);
                if (receiveHeader != null)
                {
                    var receives = _sPDbContext.SendLostDetail
                        .Where(sld => sld.Doc == receiveNo && lotNos.Contains(sld.LotNo) && sld.IsActive)
                        .ToList();
                    foreach (var detail in receives)
                    {
                        detail.IsActive = false;
                        detail.UpdateBy = userId;
                        detail.UpdateDate = DateTime.Now;
                        _sPDbContext.SendLostDetail.Update(detail);
                    }
                }
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Cancel Send Lost by Lot No successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Error occurred while canceling Send Lost by Lot No: {ex.Message}"
                };
            }
        }

        public async Task<List<ReceivedListModel>> GetTopExportReceivedAsync(string? receiveNo, string? orderNo, string? lotNo)
        {
            var query =
                from a in _sPDbContext.Export
                where a.IsActive
                select new
                {
                    a.Doc,
                    a.CreateDate
                };

            if (!string.IsNullOrWhiteSpace(receiveNo))
            {
                query = query.Where(b => b.Doc.Contains(receiveNo));
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                query = query.Where(b => _sPDbContext.ExportDetail.Any(ed => ed.LotNo.Contains(lotNo) && ed.Doc == b.Doc));
            }

            var receives = await query.OrderByDescending(o => o.CreateDate).Take(100).ToListAsync();

            var result = receives.Select(r => new ReceivedListModel
            {
                ReceiveNo = r.Doc,
                Mdate = r.CreateDate.HasValue
                    ? r.CreateDate.Value.ToString("dd MMMM yyyy", new CultureInfo("th-TH"))
                    : string.Empty,
                IsReceived = false,
            }).ToList();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetExportReceivedByReceiveNoAsync(string receiveNo, string? orderNo, string? lotNo)
        {
            var allReceived = await (
                from a in _sPDbContext.ExportDetail
                join c in _sPDbContext.Lot on a.LotNo equals c.LotNo
                join d in _sPDbContext.Order on c.OrderNo equals d.OrderNo
                where a.Doc == receiveNo && !string.IsNullOrEmpty(a.LotNo) && a.IsActive
                select new
                {
                    a.ExportId,
                    a.Doc,
                    a.LotNo,
                    a.TtQty,
                    a.TtWg,
                    c.Barcode,
                    c.Article,
                    c.OrderNo,
                    c.ListNo,
                    d.CustCode
                }
            ).ToListAsync();

            if (!string.IsNullOrWhiteSpace(orderNo))
            {
                allReceived = [.. allReceived.Where(x => x.OrderNo.Contains(orderNo))];
            }

            if (!string.IsNullOrWhiteSpace(lotNo))
            {
                allReceived = [.. allReceived.Where(x => x.LotNo.Contains(lotNo))];
            }

            var result = allReceived.Select(x => new ReceivedListModel
            {
                ReceivedID = x.ExportId,
                ReceiveNo = x.Doc,
                CustCode = x.CustCode,
                LotNo = x.LotNo,
                TtQty = x.TtQty,
                TtWg = x.TtWg,
                Barcode = x.Barcode,
                Article = x.Article,
                OrderNo = x.OrderNo,
                ListNo = x.ListNo,
                IsReceived = false
            }).ToList();

            result = [.. result.OrderBy(x => x.LotNo).ThenBy(x => x.OrderNo).ThenBy(x => x.ListNo)];

            return result;
        }

        public async Task<BaseResponseModel> CancelExportByReceiveNoAsync(string receiveNo, int userId)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveHeader = _sPDbContext.Export.FirstOrDefault(e => e.Doc == receiveNo && e.IsActive);
                if (receiveHeader != null)
                {
                    var details = _sPDbContext.ExportDetail.Where(ed => ed.Doc == receiveNo && ed.IsActive).ToList();

                    foreach (var detail in details)
                    {
                        detail.IsActive = false;
                        detail.UpdateBy = userId;
                        detail.UpdateDate = DateTime.Now;

                        _sPDbContext.ExportDetail.Update(detail);
                    }

                    receiveHeader.IsActive = false;
                    receiveHeader.UpdateDate = DateTime.Now;
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Cancel Export successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Error occurred while canceling Export: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseModel> CancelExportByLotNoAsync(string receiveNo, string[] lotNos, int userId)
        {
            var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                var receiveHeader = _sPDbContext.Export.FirstOrDefault(e => e.Doc == receiveNo && e.IsActive);
                if (receiveHeader != null)
                {
                    var details = _sPDbContext.ExportDetail
                        .Where(ed => ed.Doc == receiveNo && lotNos.Contains(ed.LotNo) && ed.IsActive)
                        .ToList();
                    foreach (var detail in details)
                    {
                        detail.IsActive = false;
                        detail.UpdateBy = userId;
                        detail.UpdateDate = DateTime.Now;
                        _sPDbContext.ExportDetail.Update(detail);
                    }
                }
                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Cancel Export by Lot No successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"Error occurred while canceling Export by Lot No: {ex.Message}"
                };
            }
        }
    }
}
