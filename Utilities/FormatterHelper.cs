using System;
using System.Globalization;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Formatting helper methods for display
    /// </summary>
    public static class FormatterHelper
    {
        /// <summary>
        /// Format currency with symbol
        /// </summary>
        public static string FormatCurrency(double amount, string currencyCode = "USD")
        {
            try
            {
                CultureInfo culture = GetCultureForCurrency(currencyCode);
                return amount.ToString("C", culture);
            }
            catch
            {
                return $"{currencyCode} {amount:N2}";
            }
        }

        /// <summary>
        /// Format currency without symbol
        /// </summary>
        public static string FormatAmount(double amount, int decimals = 2)
        {
            string format = "N" + decimals;
            return amount.ToString(format);
        }

        /// <summary>
        /// Format percentage
        /// </summary>
        public static string FormatPercentage(double percentage, int decimals = 2)
        {
            return percentage.ToString($"F{decimals}") + "%";
        }

        /// <summary>
        /// Format date in standard format
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Format date with time
        /// </summary>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Format date in short format
        /// </summary>
        public static string FormatDateShort(DateTime date)
        {
            return date.ToString("MM/dd/yyyy");
        }

        /// <summary>
        /// Format date in long format
        /// </summary>
        public static string FormatDateLong(DateTime date)
        {
            return date.ToString("MMMM dd, yyyy");
        }

        /// <summary>
        /// Format contract code with prefix
        /// </summary>
        public static string FormatContractCode(string code, string prefix = "CON")
        {
            if (string.IsNullOrWhiteSpace(code))
                return string.Empty;

            return code.StartsWith(prefix) ? code : $"{prefix}-{code}";
        }

        /// <summary>
        /// Format IPC number
        /// </summary>
        public static string FormatIPCNumber(string contractCode, int ipcNumber)
        {
            return $"{contractCode}-IPC{ipcNumber:D3}";
        }

        /// <summary>
        /// Format change order number
        /// </summary>
        public static string FormatChangeOrderNumber(string contractCode, int coNumber)
        {
            return $"{contractCode}-CO{coNumber:D3}";
        }

        /// <summary>
        /// Format file size
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Format status with color code
        /// </summary>
        public static string FormatStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Unknown";

            // Capitalize first letter
            return char.ToUpper(status[0]) + status.Substring(1).ToLower();
        }

        /// <summary>
        /// Format phone number
        /// </summary>
        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Remove all non-numeric characters
            string digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");

            // Format based on length
            if (digitsOnly.Length == 10)
            {
                return $"({digitsOnly.Substring(0, 3)}) {digitsOnly.Substring(3, 3)}-{digitsOnly.Substring(6)}";
            }
            else if (digitsOnly.Length == 11 && digitsOnly[0] == '1')
            {
                return $"+1 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7)}";
            }

            return phoneNumber; // Return as-is if format not recognized
        }

        /// <summary>
        /// Format duration in days
        /// </summary>
        public static string FormatDuration(int days)
        {
            if (days == 0)
                return "0 days";
            else if (days == 1)
                return "1 day";
            else if (days < 30)
                return $"{days} days";
            else if (days < 365)
            {
                int months = days / 30;
                int remainingDays = days % 30;
                return remainingDays > 0 ? $"{months} months, {remainingDays} days" : $"{months} months";
            }
            else
            {
                int years = days / 365;
                int remainingDays = days % 365;
                return remainingDays > 0 ? $"{years} years, {remainingDays} days" : $"{years} years";
            }
        }

        /// <summary>
        /// Parse currency string to double
        /// </summary>
        public static double ParseCurrency(string currencyString)
        {
            if (string.IsNullOrWhiteSpace(currencyString))
                return 0;

            // Remove currency symbols and whitespace
            string cleaned = System.Text.RegularExpressions.Regex.Replace(currencyString, @"[^\d\.\-,]", "");

            if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Get culture info for currency code
        /// </summary>
        private static CultureInfo GetCultureForCurrency(string currencyCode)
        {
            switch (currencyCode.ToUpper())
            {
                case "USD":
                    return new CultureInfo("en-US");
                case "EUR":
                    return new CultureInfo("de-DE");
                case "GBP":
                    return new CultureInfo("en-GB");
                case "JPY":
                    return new CultureInfo("ja-JP");
                case "SAR":
                    return new CultureInfo("ar-SA");
                case "AED":
                    return new CultureInfo("ar-AE");
                default:
                    return CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// Truncate text with ellipsis
        /// </summary>
        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
