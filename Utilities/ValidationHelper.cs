using System;
using System.Text.RegularExpressions;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Validation helper methods
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate if string is not null or empty
        /// </summary>
        public static bool IsValidString(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Validate if value is a positive number
        /// </summary>
        public static bool IsPositiveNumber(double value)
        {
            return value > 0;
        }

        /// <summary>
        /// Validate if value is non-negative
        /// </summary>
        public static bool IsNonNegative(double value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate date range
        /// </summary>
        public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return endDate >= startDate;
        }

        /// <summary>
        /// Validate percentage (0-100)
        /// </summary>
        public static bool IsValidPercentage(double percentage)
        {
            return percentage >= 0 && percentage <= 100;
        }

        /// <summary>
        /// Validate contract code format (alphanumeric with dashes)
        /// </summary>
        public static bool IsValidContractCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var pattern = @"^[A-Za-z0-9\-_]+$";
            return Regex.IsMatch(code, pattern);
        }

        /// <summary>
        /// Validate currency code (3 letters)
        /// </summary>
        public static bool IsValidCurrencyCode(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return false;

            return currencyCode.Length == 3 && Regex.IsMatch(currencyCode, @"^[A-Z]{3}$");
        }

        /// <summary>
        /// Validate amount range
        /// </summary>
        public static bool IsValidAmountRange(double minAmount, double maxAmount)
        {
            return minAmount >= 0 && maxAmount >= minAmount;
        }

        /// <summary>
        /// Validate phone number
        /// </summary>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Allow digits, spaces, dashes, parentheses, and plus sign
            var pattern = @"^[\d\s\-\(\)\+]+$";
            return Regex.IsMatch(phoneNumber, pattern) && phoneNumber.Length >= 7;
        }

        /// <summary>
        /// Validate that value is within percentage of target
        /// </summary>
        public static bool IsWithinPercentage(double value, double target, double percentageTolerance)
        {
            if (target == 0)
                return value == 0;

            double difference = Math.Abs(value - target);
            double percentDiff = (difference / Math.Abs(target)) * 100;

            return percentDiff <= percentageTolerance;
        }

        /// <summary>
        /// Validate contract status
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            string[] validStatuses = { "Draft", "Active", "OnHold", "Completed", "Cancelled" };
            return Array.Exists(validStatuses, s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Validate IPC status
        /// </summary>
        public static bool IsValidIPCStatus(string status)
        {
            string[] validStatuses = { "Draft", "Submitted", "Approved", "Rejected", "Paid" };
            return Array.Exists(validStatuses, s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get validation error message
        /// </summary>
        public static string GetValidationMessage(string fieldName, string validationType)
        {
            switch (validationType.ToLower())
            {
                case "required":
                    return $"{fieldName} is required";
                case "positive":
                    return $"{fieldName} must be a positive number";
                case "nonnegative":
                    return $"{fieldName} must be zero or positive";
                case "email":
                    return $"{fieldName} must be a valid email address";
                case "daterange":
                    return "End date must be after start date";
                case "percentage":
                    return $"{fieldName} must be between 0 and 100";
                default:
                    return $"{fieldName} is invalid";
            }
        }
    }
}
