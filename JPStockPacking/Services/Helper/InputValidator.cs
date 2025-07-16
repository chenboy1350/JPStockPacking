using System.Text.RegularExpressions;

namespace JPStockPacking.Services.Helper
{
    public partial class InputValidator
    {
        private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "select", "insert", "update", "delete", "drop", "alter", "create", "exec", "execute",
            "declare", "cast", "cursor", "fetch", "kill", "sys", "sysobjects", "syscolumns", "table"
        };

        private static readonly string[] DangerousSymbols =
        {
            "--", ";--", ";", "/*", "*/", "@@", "@", "'", "\""
        };

        private static readonly string[] DangerousScripts =
        {
            "<script", "</script>", "javascript:", "onerror", "onload", "<iframe", "</iframe>", "<img", "<svg"
        };

        public bool IsValidInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (var symbol in DangerousSymbols.Concat(DangerousScripts))
            {
                if (input.Contains(symbol, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            var words = WordRegex().Matches(input)
                                   .Select(m => m.Value);

            foreach (var word in words)
            {
                if (SqlKeywords.Contains(word))
                    return false;
            }

            return true;
        }

        [GeneratedRegex(@"\b\w+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        public static partial Regex WordRegex();
    }
}
