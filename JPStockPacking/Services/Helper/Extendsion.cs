using System.Text;

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

        public static string EncodeToText(this string input)
        {
            var map = new Dictionary<char, char>
            {
                { '1', 'g' },
                { '2', 'o' },
                { '3', 'l' },
                { '4', 'd' },
                { '5', 'm' },
                { '6', 'a' },
                { '7', 's' },
                { '8', 't' },
                { '9', 'e' },
                { '0', 'r' }
            };

            var result = new StringBuilder();

            foreach (var c in input)
            {
                if (c == '.')
                {
                    result.Append('.');
                }
                else if (map.TryGetValue(c, out var encodedChar))
                {
                    result.Append(encodedChar);
                }
                else
                {
                    continue;
                }
            }

            return result.ToString();
        }

        public static double? DecodeToNumber(this string input)
        {
            var reverseMap = new Dictionary<char, char>
            {
                { 'g', '1' },
                { 'o', '2' },
                { 'l', '3' },
                { 'd', '4' },
                { 'm', '5' },
                { 'a', '6' },
                { 's', '7' },
                { 't', '8' },
                { 'e', '9' },
                { 'r', '0' }
            };

            var decodedString = new StringBuilder();

            foreach (var c in input)
            {
                if (c == '.')
                {
                    decodedString.Append('.');
                }
                else if (reverseMap.TryGetValue(c, out var decodedChar))
                {
                    decodedString.Append(decodedChar);
                }
                else
                {
                    return null;
                }
            }

            if (double.TryParse(decodedString.ToString(), out double result))
            {
                return result;
            }

            return null;
        }

        public static decimal Trim2(this decimal value) => Math.Truncate(value * 100) / 100;

        public static double Trim2(this double value) => Math.Truncate(value * 100) / 100;

        public static double ToDouble(this decimal value) => Convert.ToDouble(value);
    }
}
