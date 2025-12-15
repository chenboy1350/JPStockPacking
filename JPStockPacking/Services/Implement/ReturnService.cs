using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class ReturnService(SPDbContext sPDbContext) : IReturnService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<BaseResponseModel> ReturnReceivedAsync(string lotNo, int[] assignmentIDs, decimal returnQty)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                List<Assignment> Assignments = await _sPDbContext.Assignment.Where(a => assignmentIDs.Contains(a.AssignmentId) && !a.IsReturned && a.IsActive).ToListAsync();
                if (Assignments.Count == 0) return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = "ไม่มีรายการที่คืน"
                };

                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(o => o.LotNo == lotNo && !o.IsSuccess && o.IsActive) ?? throw new InvalidOperationException($"Lot '{lotNo}' not found , inactive or succeed.");

                var returned = new Returned
                {
                    ReturnTtQty = returnQty,
                    IsSuccess = true,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                _sPDbContext.Returned.Add(returned);
                await _sPDbContext.SaveChangesAsync();

                var details = new List<ReturnedDetail>();

                if (assignmentIDs.Length > 0)
                {
                    var receivedList = await (
                        from a in _sPDbContext.AssignmentReceived
                        join b in _sPDbContext.Assignment on a.AssignmentId equals b.AssignmentId
                        join r in _sPDbContext.Received on a.ReceivedId equals r.ReceivedId
                        where Assignments.Select(x => x.AssignmentId).Contains(a.AssignmentId) && a.IsActive && r.LotNo == lotNo
                        select new { a.AssignmentId, Received = r }
                    ).ToListAsync();

                    foreach (var item in receivedList)
                    {
                        Assignments.Find(a => a.AssignmentId == item.AssignmentId)!.IsReturned = true;

                        item.Received.IsReturned = true;
                        item.Received.UpdateDate = DateTime.Now;

                        details.Add(new ReturnedDetail
                        {
                            ReturnId = returned.ReturnId,
                            AssignmentId = item.AssignmentId,
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        });
                    }

                    _sPDbContext.ReturnedDetail.AddRange(details);
                }

                lot.ReturnedQty = (lot.ReturnedQty ?? 0) + returnQty;
                lot.Unallocated = (lot.Unallocated ?? 0) + returnQty;
                lot.UpdateDate = DateTime.Now;

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = "คืนสินค้าสำเร็จ"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = "คืนสินค้าไม่สำเร็จ"
                };
            }
        }

        public async Task<List<TableModel>> GetTableToReturnAsync(string LotNo)
        {
            var result = await
                (from asmt in _sPDbContext.AssignmentTable
                 join asmr in _sPDbContext.AssignmentReceived on asmt.AssignmentReceivedId equals asmr.AssignmentReceivedId into asmJoin
                 from asmr in asmJoin.DefaultIfEmpty()
                 join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId into revJoin
                 from rev in revJoin.DefaultIfEmpty()
                 join wt in _sPDbContext.WorkTable on asmt.WorkTableId equals wt.Id into wtJoin
                 from wt in wtJoin.DefaultIfEmpty()
                 where asmr.IsActive == true && rev.LotNo == LotNo && !rev.IsReturned
                 select new TableModel { Id = wt.Id, Name = wt.Name! }
                ).Distinct().ToListAsync();

            return result;
        }

        public async Task<List<ReceivedListModel>> GetRecievedToReturnAsync(string LotNo, int TableID)
        {
            var result = await (from wt in _sPDbContext.WorkTable
                                join asmt in _sPDbContext.AssignmentTable on wt.Id equals asmt.WorkTableId into asmtJoin
                                from asmt in asmtJoin.DefaultIfEmpty()
                                join asmr in _sPDbContext.AssignmentReceived on asmt.AssignmentReceivedId equals asmr.AssignmentReceivedId into asmJoin
                                from asmr in asmJoin.DefaultIfEmpty()
                                join rev in _sPDbContext.Received on asmr.ReceivedId equals rev.ReceivedId into revJoin
                                from rev in revJoin.DefaultIfEmpty()
                                join lot in _sPDbContext.Lot on rev.LotNo equals lot.LotNo into bJoin
                                from lot in bJoin.DefaultIfEmpty()
                                where asmt != null && asmt.IsActive == true && wt.Id == TableID && rev != null && rev.LotNo == LotNo && !rev.IsReturned
                                select new ReceivedListModel
                                {
                                    ReceivedID = rev.ReceivedId,
                                    ReceiveNo = rev.ReceiveNo,
                                    LotNo = rev.LotNo,
                                    ListNo = lot.ListNo!,
                                    OrderNo = lot.OrderNo,
                                    Article = lot.Article!,
                                    Barcode = rev.Barcode!,
                                    CustPCode = lot.CustPcode ?? string.Empty,
                                    TtQty = rev.TtQty.GetValueOrDefault(),
                                    TtWg = rev.TtWg.GetValueOrDefault(),
                                    AssignmentID = asmr.AssignmentId
                                }).ToListAsync();

            return result;
        }
    }
}
