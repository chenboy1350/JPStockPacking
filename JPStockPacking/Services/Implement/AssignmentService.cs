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
                         join employee in employees on member.EmpId equals employee.EmployeeID
                         select new TableMemberModel
                         {
                             Id = employee.EmployeeID,
                             FirstName = employee.FirstName,
                             LastName = employee.LastName,
                             NickName = employee.NickName
                         };

            return [.. result];
        }

        public async Task SyncAssignmentsForTableAsync(string lotNo, int tableId, int[] receivedIds, string[] memberIds, bool hasPartTime, int workerNumber)
        {
            if (tableId <= 0) return;

            var now = DateTime.UtcNow;

            using var tx = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                // =========================================================
                // 1) received ที่ currently อยู่โต๊ะนี้
                // =========================================================
                var currentAtTable = await
                (
                    from table in _sPDbContext.AssignmentTable
                    join assignmentReceived in _sPDbContext.AssignmentReceived
                        on table.AssignmentReceivedId equals assignmentReceived.AssignmentReceivedId
                    join received in _sPDbContext.Received
                        on assignmentReceived.ReceivedId equals received.ReceivedId
                    where table.WorkTableId == tableId
                          && table.IsActive
                          && assignmentReceived.IsActive
                          && received.LotNo == lotNo
                    select received.ReceivedId
                )
                .Distinct()
                .ToListAsync();

                // =========================================================
                // 2) diff
                // =========================================================
                var toAssign = receivedIds.Except(currentAtTable).ToList();
                var toUnassign = currentAtTable.Except(receivedIds).ToList();

                // =========================================================
                // 3) UNASSIGN
                // =========================================================
                if (toUnassign.Any())
                {
                    var assignmentReceivedList = await
                    (
                        from assignmentReceived in _sPDbContext.AssignmentReceived
                        where assignmentReceived.IsActive
                              && toUnassign.Contains(assignmentReceived.ReceivedId)
                        select assignmentReceived
                    )
                    .ToListAsync();

                    var assignmentReceivedIds = assignmentReceivedList
                        .Select(x => x.AssignmentReceivedId)
                        .ToList();

                    var assignmentIds = assignmentReceivedList
                        .Select(x => x.AssignmentId)
                        .ToList();

                    var tablesToDisable = await
                    (
                        from table in _sPDbContext.AssignmentTable
                        where table.IsActive
                              && assignmentReceivedIds.Contains(table.AssignmentReceivedId)
                        select table
                    )
                    .ToListAsync();

                    foreach (var table in tablesToDisable)
                    {
                        table.IsActive = false;
                        table.UpdateDate = now;
                    }

                    var membersToDisable = await
                    (
                        from member in _sPDbContext.AssignmentMember
                        where member.IsActive
                              && assignmentReceivedIds.Contains(member.AssignmentReceivedId)
                        select member
                    )
                    .ToListAsync();

                    foreach (var member in membersToDisable)
                    {
                        member.IsActive = false;
                        member.UpdateDate = now;
                    }

                    foreach (var assignmentReceived in assignmentReceivedList)
                    {
                        assignmentReceived.IsActive = false;
                        assignmentReceived.UpdateDate = now;
                    }

                    var assignmentsToDisable = await
                    (
                        from assignment in _sPDbContext.Assignment
                        where assignment.IsActive
                              && assignmentIds.Contains(assignment.AssignmentId)
                        select assignment
                    )
                    .ToListAsync();

                    foreach (var assignment in assignmentsToDisable)
                    {
                        assignment.IsActive = false;
                        assignment.UpdateDate = now;
                    }

                    var receivedToUnassign = await
                    (
                        from received in _sPDbContext.Received
                        where toUnassign.Contains(received.ReceivedId)
                        select received
                    )
                    .ToListAsync();

                    foreach (var received in receivedToUnassign)
                    {
                        received.IsAssigned = false;
                        received.UpdateDate = now;
                    }
                }

                // =========================================================
                // 4) ASSIGN / UPDATE
                // =========================================================
                foreach (var receivedId in toAssign)
                {
                    var assignmentData = await
                    (
                        from tbar in _sPDbContext.AssignmentReceived
                        join tbas in _sPDbContext.Assignment
                            on tbar.AssignmentId equals tbas.AssignmentId
                        where tbar.ReceivedId == receivedId
                              && tbar.IsActive
                              && tbas.IsActive
                        select new
                        {
                            AssignmentReceived = tbar,
                            Assignment = tbas
                        }
                    )
                    .FirstOrDefaultAsync();

                    AssignmentReceived assignmentReceived;
                    Assignment assignment;

                    if (assignmentData != null)
                    {
                        // -------- UPDATE EXISTING --------
                        assignmentReceived = assignmentData.AssignmentReceived;
                        assignment = assignmentData.Assignment;

                        assignment.NumberWorkers = workerNumber + memberIds.Length;
                        assignment.HasPartTime = hasPartTime;
                        assignment.UpdateDate = now;

                        var oldTables = await
                        (
                            from table in _sPDbContext.AssignmentTable
                            where table.AssignmentReceivedId == assignmentReceived.AssignmentReceivedId
                                  && table.IsActive
                            select table
                        )
                        .ToListAsync();

                        foreach (var table in oldTables)
                        {
                            table.IsActive = false;
                            table.UpdateDate = now;
                        }

                        _sPDbContext.AssignmentTable.Add(new AssignmentTable
                        {
                            AssignmentReceivedId = assignmentReceived.AssignmentReceivedId,
                            WorkTableId = tableId,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        });

                        var oldMembers = await
                        (
                            from member in _sPDbContext.AssignmentMember
                            where member.AssignmentReceivedId == assignmentReceived.AssignmentReceivedId
                                  && member.IsActive
                            select member
                        )
                        .ToListAsync();

                        foreach (var member in oldMembers)
                        {
                            member.IsActive = false;
                            member.UpdateDate = now;
                        }

                        foreach (var memberId in memberIds)
                        {
                            _sPDbContext.AssignmentMember.Add(new AssignmentMember
                            {
                                AssignmentReceivedId = assignmentReceived.AssignmentReceivedId,
                                WorkTableMemberId = Convert.ToInt32(memberId),
                                IsActive = true,
                                CreateDate = now,
                                UpdateDate = now
                            });
                        }
                    }
                    else
                    {
                        // -------- CREATE NEW --------
                        assignment = new Assignment
                        {
                            NumberWorkers = workerNumber + memberIds.Length,
                            HasPartTime = hasPartTime,
                            IsReturned = false,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        };

                        _sPDbContext.Assignment.Add(assignment);
                        await _sPDbContext.SaveChangesAsync();

                        assignmentReceived = new AssignmentReceived
                        {
                            AssignmentId = assignment.AssignmentId,
                            ReceivedId = receivedId,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        };

                        _sPDbContext.AssignmentReceived.Add(assignmentReceived);
                        await _sPDbContext.SaveChangesAsync();

                        _sPDbContext.AssignmentTable.Add(new AssignmentTable
                        {
                            AssignmentReceivedId = assignmentReceived.AssignmentReceivedId,
                            WorkTableId = tableId,
                            IsActive = true,
                            CreateDate = now,
                            UpdateDate = now
                        });

                        foreach (var memberId in memberIds)
                        {
                            _sPDbContext.AssignmentMember.Add(new AssignmentMember
                            {
                                AssignmentReceivedId = assignmentReceived.AssignmentReceivedId,
                                WorkTableMemberId = Convert.ToInt32(memberId),
                                IsActive = true,
                                CreateDate = now,
                                UpdateDate = now
                            });
                        }
                    }

                    var receivedToAssign = await
                    (
                        from received in _sPDbContext.Received
                        where received.ReceivedId == receivedId
                        select received
                    )
                    .FirstAsync();

                    receivedToAssign.IsAssigned = true;
                    receivedToAssign.UpdateDate = now;
                }

                // =========================================================
                // 5) Update Lot summary
                // =========================================================
                var lot = await
                (
                    from l in _sPDbContext.Lot
                    where l.LotNo == lotNo
                    select l
                )
                .FirstOrDefaultAsync();

                if (lot != null)
                {
                    lot.AssignedQty = await
                    (
                        from ar in _sPDbContext.AssignmentReceived
                        join r in _sPDbContext.Received
                            on ar.ReceivedId equals r.ReceivedId
                        where ar.IsActive
                              && r.LotNo == lotNo
                        select r.TtQty ?? 0
                    )
                    .SumAsync();

                    lot.UpdateDate = now;
                }

                await _sPDbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



    }
}
