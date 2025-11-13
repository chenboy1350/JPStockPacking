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

    public class UpdateAppSettingsModel
    {
        public int ChxQtyPersentage { get; set; } = 0;
        public int MinWgPersentage { get; set; } = 0;
    }
}
