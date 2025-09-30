namespace JPStockPacking.Models
{
    public class ResEmployeeModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
    }
}
