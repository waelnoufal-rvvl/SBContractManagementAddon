using System;
using System.Collections.Generic;

namespace ContractManagementAddon.Models
{
    /// <summary>
    /// Currency Master - ISO 4217 Currency Codes
    /// Supports 150+ currencies for global operations
    /// </summary>
    public class Currency
    {
        public string Code { get; set; }              // ISO 4217 code (USD, EUR, GBP, etc.)
        public string Name { get; set; }              // Currency name
        public string Symbol { get; set; }            // Currency symbol ($, €, £, etc.)
        public int DecimalPlaces { get; set; }        // Usually 2, but can be 0 (JPY) or 3 (KWD)
        public bool IsActive { get; set; }            // Active/Inactive flag
        public bool IsBaseCurrency { get; set; }      // Only one currency can be base
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string ModifyUser { get; set; }

        public Currency()
        {
            DecimalPlaces = 2; // Default
            IsActive = true;
            IsBaseCurrency = false;
            CreateDate = DateTime.Now;
        }

        /// <summary>
        /// Validate currency data
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Code))
                errors.Add("Currency code is required");
            else if (Code.Length != 3)
                errors.Add("Currency code must be 3 characters (ISO 4217 standard)");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Currency name is required");

            if (string.IsNullOrWhiteSpace(Symbol))
                errors.Add("Currency symbol is required");

            if (DecimalPlaces < 0 || DecimalPlaces > 4)
                errors.Add("Decimal places must be between 0 and 4");

            return errors;
        }

        /// <summary>
        /// Format amount according to currency rules
        /// </summary>
        public string FormatAmount(double amount)
        {
            string format = $"N{DecimalPlaces}";
            return $"{Symbol}{amount.ToString(format)}";
        }

        /// <summary>
        /// Round amount to currency decimal places
        /// </summary>
        public double RoundAmount(double amount)
        {
            return Math.Round(amount, DecimalPlaces);
        }
    }

    /// <summary>
    /// Exchange Rate between two currencies
    /// Supports historical rates for accurate conversion
    /// </summary>
    public class ExchangeRate
    {
        public string Code { get; set; }              // Unique ID
        public string FromCurrency { get; set; }      // Source currency code
        public string ToCurrency { get; set; }        // Target currency code
        public DateTime RateDate { get; set; }        // Date of the exchange rate
        public double Rate { get; set; }              // Exchange rate value
        public string RateSource { get; set; }        // Manual / Automatic / API
        public bool IsActive { get; set; }            // Active flag
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string ModifyUser { get; set; }

        public ExchangeRate()
        {
            RateDate = DateTime.Today;
            RateSource = "Manual";
            IsActive = true;
            CreateDate = DateTime.Now;
        }

        /// <summary>
        /// Validate exchange rate data
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FromCurrency))
                errors.Add("From currency is required");

            if (string.IsNullOrWhiteSpace(ToCurrency))
                errors.Add("To currency is required");

            if (FromCurrency == ToCurrency)
                errors.Add("From and To currencies cannot be the same");

            if (Rate <= 0)
                errors.Add("Exchange rate must be greater than zero");

            if (RateDate > DateTime.Today)
                errors.Add("Exchange rate date cannot be in the future");

            return errors;
        }

        /// <summary>
        /// Calculate inverse rate (for bidirectional conversion)
        /// </summary>
        public double GetInverseRate()
        {
            return 1.0 / Rate;
        }

        /// <summary>
        /// Convert amount using this exchange rate
        /// </summary>
        public double ConvertAmount(double amount)
        {
            return amount * Rate;
        }

        /// <summary>
        /// Check if rate is current (today's date)
        /// </summary>
        public bool IsCurrent()
        {
            return RateDate.Date == DateTime.Today;
        }

        /// <summary>
        /// Check if rate is within tolerance of another rate
        /// </summary>
        public bool IsWithinTolerance(double otherRate, double tolerancePercent)
        {
            double difference = Math.Abs(Rate - otherRate);
            double allowedDifference = Rate * (tolerancePercent / 100.0);
            return difference <= allowedDifference;
        }
    }

    /// <summary>
    /// Currency Conversion Log for audit trail
    /// Tracks all currency conversions performed
    /// </summary>
    public class CurrencyConversionLog
    {
        public string Code { get; set; }              // Unique ID
        public string DocumentType { get; set; }      // Contract / IPC / Invoice
        public string DocumentCode { get; set; }      // Document reference
        public string OriginalCurrency { get; set; }  // Original currency
        public double OriginalAmount { get; set; }    // Original amount
        public string TargetCurrency { get; set; }    // Converted to currency
        public double ConvertedAmount { get; set; }   // Converted amount
        public double ExchangeRate { get; set; }      // Rate used
        public DateTime ConversionDate { get; set; }  // When conversion happened
        public string ConversionUser { get; set; }    // Who performed conversion
        public string ConversionReason { get; set; }  // Why conversion was done
        public DateTime CreateDate { get; set; }

        public CurrencyConversionLog()
        {
            ConversionDate = DateTime.Now;
            CreateDate = DateTime.Now;
        }

        /// <summary>
        /// Validate conversion log
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(DocumentType))
                errors.Add("Document type is required");

            if (string.IsNullOrWhiteSpace(DocumentCode))
                errors.Add("Document code is required");

            if (string.IsNullOrWhiteSpace(OriginalCurrency))
                errors.Add("Original currency is required");

            if (string.IsNullOrWhiteSpace(TargetCurrency))
                errors.Add("Target currency is required");

            if (ExchangeRate <= 0)
                errors.Add("Exchange rate must be greater than zero");

            return errors;
        }

        /// <summary>
        /// Verify conversion calculation is correct
        /// </summary>
        public bool VerifyConversion(double tolerance = 0.01)
        {
            double expectedAmount = OriginalAmount * ExchangeRate;
            double difference = Math.Abs(ConvertedAmount - expectedAmount);
            return difference <= tolerance;
        }
    }

    /// <summary>
    /// Currency pair for quick lookup
    /// </summary>
    public class CurrencyPair
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }

        public CurrencyPair(string fromCurrency, string toCurrency)
        {
            FromCurrency = fromCurrency;
            ToCurrency = toCurrency;
        }

        public override string ToString()
        {
            return $"{FromCurrency}/{ToCurrency}";
        }

        public override bool Equals(object obj)
        {
            if (obj is CurrencyPair other)
            {
                return FromCurrency == other.FromCurrency && ToCurrency == other.ToCurrency;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
