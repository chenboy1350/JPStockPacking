namespace JPStockPacking.Models
{
    public class EmployeeWithDepartmentModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public int DepartmentID { get; set; } = 0;
    }
}
