using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class FormulaManagementService(SPDbContext sPDbContext) : IFormulaManagementService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<FormulaModel>> GetFormula(Formula formula)
        {
            var query =
                from fm in _sPDbContext.Formula
                join cg in _sPDbContext.CustomerGroup on fm.CustomerGroupId equals cg.CustomerGroupId into cgJoin
                from cg in cgJoin.DefaultIfEmpty()
                join pt in _sPDbContext.ProductType on fm.ProductTypeId equals pt.ProductTypeId into ptJoin
                from pt in ptJoin.DefaultIfEmpty()
                join pm in _sPDbContext.PackMethod on fm.PackMethodId equals pm.PackMethodId into pmJoin
                from pm in pmJoin.DefaultIfEmpty()
                select new { fm, cg, pt, pm };

            if (formula.FormulaId > 0)
            {
                query = query.Where(x => x.fm.FormulaId == formula.FormulaId);
            }

            if (!string.IsNullOrEmpty(formula.Name))
            {
                query = query.Where(x => x.fm.Name!.Contains(formula.Name));
            }

            var result = await query
                .Select(x => new FormulaModel
                {
                    FormulaID = x.fm.FormulaId,
                    Name = x.fm.Name ?? string.Empty,
                    CustomerGroupID = x.cg.CustomerGroupId,
                    CustomerGroup = x.cg.Name ?? string.Empty,
                    ProductTypeID = x.pt.ProductTypeId,
                    ProductType = x.pt.Name ?? string.Empty,
                    PackMethodID = x.pm.PackMethodId,
                    PackMethod = x.pm.Name ?? string.Empty,
                    Items = x.fm.Items,
                    P1 = x.fm.P1,
                    P2 = x.fm.P2,
                    Avg = x.fm.Avg,
                    ItemPerSec = x.fm.ItemPerSec,
                    IsActive = x.fm.IsActive
                })
                .ToListAsync();

            return result;
        }


        public async Task<BaseResponseModel> AddNewFormula(Formula formula)
        {
            await using var transaction = await _sPDbContext.Database.BeginTransactionAsync();
            try
            {
                decimal p1 = Convert.ToDecimal(formula.P1);
                decimal p2 = Convert.ToDecimal(formula.P2);
                int items = formula.Items;

                p1 = SanitizeValue(p1);
                p2 = SanitizeValue(p2);
                items = items <= 0 ? 1 : items;

                decimal avg = (p1 + p2) / 2m;
                if (avg < 0) avg = 0;

                decimal itemPerSec = avg / items;

                var newFormula = new Formula
                {
                    Name = formula.Name?.Trim() ?? string.Empty,
                    CustomerGroupId = formula.CustomerGroupId,
                    ProductTypeId = formula.ProductTypeId,
                    PackMethodId = formula.PackMethodId,
                    P1 = (double)p1,
                    P2 = (double)p2,
                    Avg = (double)avg,
                    Items = items,
                    ItemPerSec = (double)itemPerSec,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _sPDbContext.Formula.AddAsync(newFormula);
                await _sPDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new BaseResponseModel
                {
                    IsSuccess = true,
                    Message = "New formula added successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = $"Error adding new formula: {ex.Message}"
                };
            }
        }

        private decimal SanitizeValue(decimal value)
        {
            if (value < 0) return 0;
            if (value > decimal.MaxValue / 2) return decimal.MaxValue / 2;
            return value;
        }


        public async Task<BaseResponseModel> UpdateFormula(Formula formula)
        {
            var existing = await _sPDbContext.Formula.FindAsync(formula.FormulaId);
            if (existing == null)
            {
                return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = "Formula not found."
                };
            }

            decimal p1 = Convert.ToDecimal(formula.P1);
            decimal p2 = Convert.ToDecimal(formula.P2);

            p1 = SanitizeValue(p1);
            p2 = SanitizeValue(p2);
            int items = formula.Items <= 0 ? 1 : formula.Items;

            decimal avg = (p1 + p2) / 2m;
            if (avg < 0) avg = 0;

            decimal itemPerSec = avg / items;

            existing.Name = formula.Name?.Trim() ?? string.Empty;
            existing.CustomerGroupId = formula.CustomerGroupId;
            existing.ProductTypeId = formula.ProductTypeId;
            existing.PackMethodId = formula.PackMethodId;

            existing.P1 = (double)p1;
            existing.P2 = (double)p2;
            existing.Items = items;

            existing.Avg = (double)avg;
            existing.ItemPerSec = (double)itemPerSec;

            existing.UpdateDate = DateTime.UtcNow;

            await _sPDbContext.SaveChangesAsync();

            return new BaseResponseModel
            {
                IsSuccess = true,
                Message = "Formula updated successfully."
            };
        }


        public async Task<BaseResponseModel> ToggleFormulaStatus(int formulaId)
        {
            var existing = await _sPDbContext.Formula.FindAsync(formulaId);

            if (existing == null)
            {
                return new BaseResponseModel
                {
                    IsSuccess = false,
                    Message = "Formula not found."
                };
            }

            bool newStatus = !existing.IsActive;
            existing.IsActive = newStatus;
            existing.UpdateDate = DateTime.UtcNow;

            await _sPDbContext.SaveChangesAsync();

            return new BaseResponseModel
            {
                IsSuccess = true,
                Message = newStatus
                            ? "Formula activated successfully."
                            : "Formula disabled successfully."
            };
        }

    }
}
