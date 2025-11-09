using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Seeds initial currency data (ISO 4217 currencies)
    /// Populates common currencies used in global business
    /// </summary>
    public static class CurrencySeedData
    {
        /// <summary>
        /// Get list of common currencies to seed
        /// Top 50 most used currencies in international trade
        /// </summary>
        public static List<Currency> GetCommonCurrencies()
        {
            return new List<Currency>
            {
                // Major Currencies
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "GBP", Name = "British Pound Sterling", Symbol = "£", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, IsActive = true },
                new Currency { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "NZD", Name = "New Zealand Dollar", Symbol = "NZ$", DecimalPlaces = 2, IsActive = true },

                // European Currencies (Non-Euro)
                new Currency { Code = "SEK", Name = "Swedish Krona", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "NOK", Name = "Norwegian Krone", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "DKK", Name = "Danish Krone", Symbol = "kr", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "PLN", Name = "Polish Zloty", Symbol = "zł", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "CZK", Name = "Czech Koruna", Symbol = "Kč", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "HUF", Name = "Hungarian Forint", Symbol = "Ft", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "RON", Name = "Romanian Leu", Symbol = "lei", DecimalPlaces = 2, IsActive = true },

                // Asia-Pacific
                new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "HKD", Name = "Hong Kong Dollar", Symbol = "HK$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "SGD", Name = "Singapore Dollar", Symbol = "S$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "KRW", Name = "South Korean Won", Symbol = "₩", DecimalPlaces = 0, IsActive = true },
                new Currency { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "THB", Name = "Thai Baht", Symbol = "฿", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "MYR", Name = "Malaysian Ringgit", Symbol = "RM", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "IDR", Name = "Indonesian Rupiah", Symbol = "Rp", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "PHP", Name = "Philippine Peso", Symbol = "₱", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "VND", Name = "Vietnamese Dong", Symbol = "₫", DecimalPlaces = 0, IsActive = true },
                new Currency { Code = "TWD", Name = "Taiwan Dollar", Symbol = "NT$", DecimalPlaces = 2, IsActive = true },

                // Middle East & Africa
                new Currency { Code = "AED", Name = "UAE Dirham", Symbol = "د.إ", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "SAR", Name = "Saudi Riyal", Symbol = "﷼", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "QAR", Name = "Qatari Riyal", Symbol = "﷼", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "KWD", Name = "Kuwaiti Dinar", Symbol = "د.ك", DecimalPlaces = 3, IsActive = true },
                new Currency { Code = "BHD", Name = "Bahraini Dinar", Symbol = "د.ب", DecimalPlaces = 3, IsActive = true },
                new Currency { Code = "OMR", Name = "Omani Rial", Symbol = "﷼", DecimalPlaces = 3, IsActive = true },
                new Currency { Code = "ILS", Name = "Israeli New Shekel", Symbol = "₪", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "EGP", Name = "Egyptian Pound", Symbol = "E£", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "ZAR", Name = "South African Rand", Symbol = "R", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "NGN", Name = "Nigerian Naira", Symbol = "₦", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "KES", Name = "Kenyan Shilling", Symbol = "KSh", DecimalPlaces = 2, IsActive = true },

                // Americas
                new Currency { Code = "MXN", Name = "Mexican Peso", Symbol = "Mex$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "BRL", Name = "Brazilian Real", Symbol = "R$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "ARS", Name = "Argentine Peso", Symbol = "AR$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "CLP", Name = "Chilean Peso", Symbol = "CL$", DecimalPlaces = 0, IsActive = true },
                new Currency { Code = "COP", Name = "Colombian Peso", Symbol = "COL$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "PEN", Name = "Peruvian Sol", Symbol = "S/", DecimalPlaces = 2, IsActive = true },

                // Others
                new Currency { Code = "RUB", Name = "Russian Ruble", Symbol = "₽", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "TRY", Name = "Turkish Lira", Symbol = "₺", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "PKR", Name = "Pakistani Rupee", Symbol = "₨", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "BDT", Name = "Bangladeshi Taka", Symbol = "৳", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "LKR", Name = "Sri Lankan Rupee", Symbol = "Rs", DecimalPlaces = 2, IsActive = true }
            };
        }

        /// <summary>
        /// Get sample exchange rates for common currency pairs
        /// Note: These are sample rates - should be updated with real-time rates in production
        /// </summary>
        public static List<ExchangeRate> GetSampleExchangeRates()
        {
            DateTime today = DateTime.Today;

            return new List<ExchangeRate>
            {
                // USD to major currencies
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "EUR", Rate = 0.92, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "GBP", Rate = 0.79, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "JPY", Rate = 149.50, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "CHF", Rate = 0.88, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "CAD", Rate = 1.36, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "AUD", Rate = 1.52, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "CNY", Rate = 7.24, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "INR", Rate = 83.20, RateDate = today, RateSource = "Manual" },

                // EUR to major currencies
                new ExchangeRate { FromCurrency = "EUR", ToCurrency = "GBP", Rate = 0.86, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "EUR", ToCurrency = "USD", Rate = 1.09, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "EUR", ToCurrency = "JPY", Rate = 162.50, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "EUR", ToCurrency = "CHF", Rate = 0.96, RateDate = today, RateSource = "Manual" },

                // GBP to major currencies
                new ExchangeRate { FromCurrency = "GBP", ToCurrency = "USD", Rate = 1.27, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "GBP", ToCurrency = "EUR", Rate = 1.16, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "GBP", ToCurrency = "JPY", Rate = 189.50, RateDate = today, RateSource = "Manual" },

                // Middle East
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "AED", Rate = 3.67, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "SAR", Rate = 3.75, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "QAR", Rate = 3.64, RateDate = today, RateSource = "Manual" },

                // Asia-Pacific
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "SGD", Rate = 1.34, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "HKD", Rate = 7.82, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "KRW", Rate = 1305.00, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "THB", Rate = 35.20, RateDate = today, RateSource = "Manual" },
                new ExchangeRate { FromCurrency = "USD", ToCurrency = "MYR", Rate = 4.68, RateDate = today, RateSource = "Manual" }
            };
        }

        /// <summary>
        /// Seed currencies into the system
        /// </summary>
        public static void SeedCurrencies(Company company, string baseCurrencyCode = "USD")
        {
            try
            {
                Logger.Info("Starting currency seed process...");

                CurrencyService currencyService = new CurrencyService(company);
                List<Currency> currencies = GetCommonCurrencies();

                int successCount = 0;
                int errorCount = 0;

                foreach (var currency in currencies)
                {
                    try
                    {
                        // Set base currency flag
                        if (currency.Code == baseCurrencyCode)
                        {
                            currency.IsBaseCurrency = true;
                        }

                        currency.CreateUser = company.UserName;
                        currency.CreateDate = DateTime.Now;

                        currencyService.SaveCurrency(currency);
                        successCount++;

                        Logger.Info($"Seeded currency: {currency.Code} - {currency.Name}");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Logger.Warning($"Failed to seed currency {currency.Code}: {ex.Message}");
                        // Continue with next currency
                    }
                }

                Logger.Info($"Currency seed completed: {successCount} succeeded, {errorCount} failed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error seeding currencies", ex);
                throw;
            }
        }

        /// <summary>
        /// Seed exchange rates into the system
        /// </summary>
        public static void SeedExchangeRates(Company company)
        {
            try
            {
                Logger.Info("Starting exchange rate seed process...");

                CurrencyService currencyService = new CurrencyService(company);
                List<ExchangeRate> rates = GetSampleExchangeRates();

                int successCount = 0;
                int errorCount = 0;

                foreach (var rate in rates)
                {
                    try
                    {
                        rate.CreateUser = company.UserName;
                        rate.CreateDate = DateTime.Now;
                        rate.IsActive = true;

                        currencyService.SaveExchangeRate(rate);
                        successCount++;

                        Logger.Info($"Seeded exchange rate: {rate.FromCurrency}/{rate.ToCurrency} = {rate.Rate}");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Logger.Warning($"Failed to seed exchange rate {rate.FromCurrency}/{rate.ToCurrency}: {ex.Message}");
                        // Continue with next rate
                    }
                }

                Logger.Info($"Exchange rate seed completed: {successCount} succeeded, {errorCount} failed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error seeding exchange rates", ex);
                throw;
            }
        }

        /// <summary>
        /// Seed all currency data (currencies + exchange rates)
        /// </summary>
        public static void SeedAllCurrencyData(Company company, string baseCurrencyCode = "USD")
        {
            try
            {
                Logger.Info("=== Starting complete currency data seed ===");

                // Seed currencies first
                SeedCurrencies(company, baseCurrencyCode);

                // Then seed exchange rates
                SeedExchangeRates(company);

                Logger.Info("=== Currency data seed completed successfully ===");
            }
            catch (Exception ex)
            {
                Logger.Error("Error in complete currency data seed", ex);
                throw;
            }
        }
    }
}
