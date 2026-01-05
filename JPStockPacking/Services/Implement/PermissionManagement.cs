using JPStockPacking.Data.SPDbContext;
using JPStockPacking.Data.SPDbContext.Entities;
using JPStockPacking.Models;
using JPStockPacking.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JPStockPacking.Services.Implement
{
    public class PermissionManagement(SPDbContext sPDbContext, IPISService pISService) : IPermissionManagement
    {
        private readonly SPDbContext _sPDbContext = sPDbContext;
        private readonly IPISService _pISService = pISService;

        public async Task<List<Permission>> GetPermissionAsync()
        {
            var permissions = await _sPDbContext.Permission.ToListAsync();
            return permissions;
        }

        public async Task<List<UserModel>> GetUserAsync()
        {
            var users = await _pISService.GetUser(new ReqUserModel());

            var userPermissions = from user in users
                                  join up in _sPDbContext.MappingPermission on user.UserID equals up.UserId into userPerms
                                  from up in userPerms.DefaultIfEmpty()
                                  group user by new
                                  {
                                      user.UserID,
                                      user.EmployeeID,
                                      user.DepartmentID,
                                      user.Username,
                                      user.FirstName,
                                      user.LastName,
                                      user.NickName,
                                      user.DepartmentName,
                                      user.IsActive,
                                      user.CreateDate,
                                      user.UpdateDate
                                  } into userGroup
                                  select new
                                  {
                                      User = userGroup,
                                  };

            var result = userPermissions.Select(ug => new UserModel
            {
                UserID = ug.User.Key.UserID,
                EmployeeID = ug.User.Key.EmployeeID,
                DepartmentID = ug.User.Key.DepartmentID,
                Username = ug.User.Key.Username,
                FirstName = ug.User.Key.FirstName,
                LastName = ug.User.Key.LastName,
                NickName = ug.User.Key.NickName,
                DepartmentName = ug.User.Key.DepartmentName,
                IsActive = ug.User.Key.IsActive,
                CreateDate = ug.User.Key.CreateDate,
                UpdateDate = ug.User.Key.UpdateDate
            }).ToList();

            return result;
        }

        public async Task<List<MappingPermission>> GetMappingPermissionAsync(int UserID)
        {
            var mappingPermissions = await _sPDbContext.MappingPermission.Where(x => x.UserId == UserID).ToListAsync();
            return mappingPermissions;
        }

        public async Task<BaseResponseModel> UpdatePermissionAsync(UpdatePermissionModel model)
        {
            if (model == null)
                return new BaseResponseModel
                {
                    Code = 400,
                    IsSuccess = false,
                    Message = "Invalid model."
                };

            var userId = model.UserId;
            var selected = model.PermissionIds ?? new List<int>();

            var existing = await _sPDbContext.MappingPermission
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var pid in selected)
            {
                var item = existing.FirstOrDefault(x => x.PermissionId == pid);

                if (item != null)
                {
                    item.IsActive = true;
                    item.UpdateDate = DateTime.Now;
                }
                else
                {
                    _sPDbContext.MappingPermission.Add(new MappingPermission
                    {
                        UserId = userId,
                        PermissionId = pid,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    });
                }
            }

            foreach (var item in existing)
            {
                if (!selected.Contains(item.PermissionId))
                {
                    item.IsActive = false;
                    item.UpdateDate = DateTime.Now;
                }
            }

            await _sPDbContext.SaveChangesAsync();

            return new BaseResponseModel
            {
                Code = 200,
                IsSuccess = true,
                Message = "Permissions updated successfully."
            };
        }
    }
}

public class UpdatePermissionModel
{
    public int UserId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
