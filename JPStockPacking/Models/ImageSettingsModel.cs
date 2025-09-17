namespace JPStockPacking.Models
{
    public class ImageSettingsModel
    {
        public string ReportHeaderImgPath { get; set; } = string.Empty;
        public string ImgPath { get; set; } = string.Empty;
        public string Separator { get; set; } = "\\";
    }

    public class AppSettingModel
    {
        public bool UseMockUp { get; set; } = false;
    }
}
