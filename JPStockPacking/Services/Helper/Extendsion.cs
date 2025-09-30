namespace JPStockPacking.Services.Helper
{
    public static class Extendsion
    {
        public static DateTime StartOfWeek(this DateTime dt)
        {
            int diff = (7 + (int)dt.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return dt.AddDays(-diff).Date;
        }

        public static string? GetContentType(this string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => null
            };
        }
    }
}
