using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class AssignmentService(SPDbContext sPDbContext, IPISService pISService) : IAssignmentService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IPISService _pISService = pISService;

        public async Task<List<ReceivedListModel>> GetReceivedToAssignAsync(string lotNo)
        {
            var result = await (from a in _sPDbContext.Received
                                join b in _sPDbContext.Lot on a.LotNo equals b.LotNo
                                where a.LotNo == lotNo && a.IsReceived && !a.IsReturned
                                select new ReceivedListModel
                                {
                                    ReceivedID = a.ReceivedId,
                                    ReceiveNo = a.ReceiveNo,
                                    LotNo = a.LotNo,
                                    ListNo = b.ListNo!,
                                    OrderNo = b.OrderNo,
                                    Article = b.Article!,
                                    Barcode = a.Barcode!,
                                    CustPCode = b.CustPcode ?? string.Empty,
                                    TtQty = a.TtQty.GetValueOrDefault(),
                                    TtWg = a.TtWg.GetValueOrDefault(),
                                }).ToListAsync();

            return result;
        }

        public async Task<List<TableMemberModel>> GetTableMemberAsync(int tableID)
        {
            var employees = await _pISService.GetEmployeeAsync();

            if (employees == null || employees.Count == 0)
            {
                throw new Exception("No employees found.");
            }

            var tableMembers = await _sPDbContext.WorkTableMember
                .Where(x => x.WorkTableId == tableID && x.IsActive)
                .ToListAsync();

            var result = from member in tableMembers
                         join employee in employees on member.EmpId equals employee.Id
                         select new TableMemberModel
                         {
                             Id = employee.Id,
                             FirstName = employee.FirstName,
                             LastName = employee.LastName,
                             NickName = employee.NickName
                         };

            return [.. result];
        }


        public async Task AssignReceivedAsync(string lotNo, int[] receivedIDs, string tableId, string[] memberIds, bool hasPartTime, int WorkerNumber)
        {
            using var transaction = await _sPDbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var revID in receivedIDs)
                {
                    var receiveds = await _sPDbContext.Received
                        .FirstOrDefaultAsync(x => x.ReceivedId == revID && x.LotNo == lotNo);

                    if (receiveds == null) continue;

                    var existingAssignments = await _sPDbContext.AssignmentReceived
                        .Where(a => a.ReceivedId == receiveds.ReceivedId && a.IsActive)
                        .ToListAsync();

                    foreach (var assign in existingAssignments)
                    {
                        assign.IsActive = false;
                        assign.UpdateDate = DateTime.Now;

                        var assignTables = await _sPDbContext.AssignmentTable
                            .Where(at => at.AssignmentReceivedId == assign.AssignmentReceivedId && at.IsActive)
                            .ToListAsync();
                        foreach (var t in assignTables)
                        {
                            t.IsActive = false;
                            t.UpdateDate = DateTime.Now;
                        }

                        var assignMembers = await _sPDbContext.AssignmentMember
                            .Where(am => am.AssignmentReceivedId == assign.AssignmentId && am.IsActive)
                            .ToListAsync();
                        foreach (var m in assignMembers)
                        {
                            m.IsActive = false;
                            m.UpdateDate = DateTime.Now;
                        }
                    }

                    var newAssignment = new Assignment
                    {
                        NumberWorkers = WorkerNumber + memberIds.Length,
                        HasPartTime = hasPartTime,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.Assignment.Add(newAssignment);
                    await _sPDbContext.SaveChangesAsync();

                    var assignReceived = new AssignmentReceived
                    {
                        AssignmentId = newAssignment.AssignmentId,
                        ReceivedId = receiveds.ReceivedId,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.AssignmentReceived.Add(assignReceived);
                    await _sPDbContext.SaveChangesAsync();

                    var assignTable = new AssignmentTable
                    {
                        AssignmentReceivedId = assignReceived.AssignmentReceivedId,
                        WorkTableId = Convert.ToInt32(tableId),
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _sPDbContext.AssignmentTable.Add(assignTable);

                    foreach (var memberId in memberIds)
                    {
                        var assignMember = new AssignmentMember
                        {
                            AssignmentReceivedId = assignReceived.AssignmentReceivedId,
                            WorkTableMemberId = Convert.ToInt32(memberId),
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now
                        };
                        _sPDbContext.AssignmentMember.Add(assignMember);
                    }

                    receiveds.IsAssigned = true;
                    receiveds.UpdateDate = DateTime.Now;
                    await _sPDbContext.SaveChangesAsync();
                }

                var lot = await _sPDbContext.Lot.FirstOrDefaultAsync(x => x.LotNo == lotNo);
                if (lot != null)
                {
                    var assignedQty = await _sPDbContext.Received
                        .Where(x => x.LotNo == lotNo && x.IsAssigned)
                        .SumAsync(x => x.TtQty ?? 0);

                    lot.AssignedQty = assignedQty;
                    lot.UpdateDate = DateTime.Now;
                }

                await _sPDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
