namespace JPStockPacking.Models
{
    public class ReqUserModel
    {
        public int? UserID { get; set; }
        public string? Username { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserModel
    {
        public int UserID { get; set; } = 0;
        public int EmployeeID { get; set; } = 0;
        public int DepartmentID { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
