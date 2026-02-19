using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class ProductTypeService(SPDbContext sPDbContext) : IProductTypeService
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;

        public async Task<List<ProductType>> GetAllAsync()
        {
            return await _sPDbContext.ProductType.OrderBy(x => x.ProductTypeId).ToListAsync();
        }

        public async Task<ProductType?> GetByIdAsync(int id)
        {
            return await _sPDbContext.ProductType.FindAsync(id);
        }

        public async Task<BaseResponseModel> AddAsync(ProductType model)
        {
            try
            {
                model.IsActive = true;
                model.CreateDate = DateTime.Now;
                model.UpdateDate = DateTime.Now;

                _sPDbContext.ProductType.Add(model);
                await _sPDbContext.SaveChangesAsync();

                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "เพิ่มประเภทสินค้าเรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseModel> UpdateAsync(ProductType model)
        {
            try
            {
                var existing = await _sPDbContext.ProductType.FindAsync(model.ProductTypeId);
                if (existing == null)
                    return new BaseResponseModel
                    {
                        Code = 404,
                        IsSuccess = false,
                        Message = "ไม่พบข้อมูลประเภทสินค้า"
                    };

                existing.Name = model.Name;
                existing.BaseTime = model.BaseTime;
                existing.UpdateDate = DateTime.Now;

                await _sPDbContext.SaveChangesAsync();

                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "แก้ไขประเภทสินค้าเรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponseModel> ToggleStatusAsync(int id)
        {
            try
            {
                var existing = await _sPDbContext.ProductType.FindAsync(id);
                if (existing == null)
                    return new BaseResponseModel
                    {
                        Code = 404,
                        IsSuccess = false,
                        Message = "ไม่พบข้อมูลประเภทสินค้า"
                    };

                existing.IsActive = !existing.IsActive;
                existing.UpdateDate = DateTime.Now;

                await _sPDbContext.SaveChangesAsync();

                return new BaseResponseModel
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = $"{(existing.IsActive ? "เปิด" : "ปิด")}ใช้งานประเภทสินค้าเรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseModel
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = $"เกิดข้อผิดพลาด: {ex.Message}"
                };
            }
        }
    }
}
