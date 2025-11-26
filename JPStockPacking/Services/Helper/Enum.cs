namespace JPStockPacking.Services.Helper
{
    public class Enum
    {
        public enum GroupMode
        {
            Day = 0,
            Week= 1
        }

        public enum PrintTo
        {
            Packing = 1,
            QA = 2,
            QC = 3,
            Export = 4
        }

        public enum SendType
        {
            KS = 1,
            KM = 2
        }

        public enum ReceiveType
        {
            SJ1 = 2,
            SJ2 = 3
        }

        public enum InvoiceType
        {
            All = 1,
            IncorectOnly = 2,
            CorrectOnly = 3
        }
    }
}
