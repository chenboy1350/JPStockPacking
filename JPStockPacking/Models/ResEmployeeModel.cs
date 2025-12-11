namespace JPStockPacking.Models
{
    public class ResEmployeeModel
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public int DepartmentID { get; set; } = 0;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
