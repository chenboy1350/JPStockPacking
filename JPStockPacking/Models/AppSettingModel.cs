namespace JPStockPacking.Models
{
    public class AppSettingModel
    {
        public string AppVersion { get; set; } = string.Empty;
        public string DatabaseVersion { get; set; } = string.Empty;
    }

    public class SendQtyModel
    {
        public int Persentage { get; set; }
    }
}
