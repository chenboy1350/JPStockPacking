namespace JPStockPacking.Services.Helper
{
    public static class Extendsion
    {
        public static DateTime StartOfWeek(this DateTime dt)
        {
            int diff = (7 + (int)dt.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return dt.AddDays(-diff).Date;
        }
    }
}
