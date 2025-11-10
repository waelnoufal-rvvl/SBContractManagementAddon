using System;
using System.Collections.Generic;
using System.Linq;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Currency Service - Manages currencies and exchange rates
    /// Provides multi-currency support for global operations
    /// </summary>
    public class CurrencyService
    {
        private readonly Company _company;
        private Dictionary<string, Currency> _currencyCache;
        private Dictionary<CurrencyPair, ExchangeRate> _rateCache;
        private Currency _baseCurrency;

        public CurrencyService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _currencyCache = new Dictionary<string, Currency>();
            _rateCache = new Dictionary<CurrencyPair, ExchangeRate>();

            LoadCurrencies();
            LoadBaseCurrency();

            Logger.Info("CurrencyService initialized");
        }

        #region Currency Management

        /// <summary>
        /// Get all active currencies
        /// </summary>
        public List<Currency> GetAllCurrencies()
        {
            try
            {
                if (_currencyCache.Count == 0)
                {
                    LoadCurrencies();
                }

                return _currencyCache.Values.Where(c => c.IsActive).OrderBy(c => c.Code).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting all currencies", ex);
                return new List<Currency>();
            }
        }

        /// <summary>
        /// Get currency by code
        /// </summary>
        public Currency GetCurrency(string currencyCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currencyCode))
                    return null;

                if (_currencyCache.ContainsKey(currencyCode.ToUpper()))
                {
                    return _currencyCache[currencyCode.ToUpper()];
                }

                // If not in cache, try to load from database
                LoadCurrency(currencyCode);

                return _currencyCache.ContainsKey(currencyCode.ToUpper())
                    ? _currencyCache[currencyCode.ToUpper()]
                    : null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting currency {currencyCode}", ex);
                return null;
            }
        }

        /// <summary>
        /// Get base currency (company's reporting currency)
        /// </summary>
        public Currency GetBaseCurrency()
        {
            if (_baseCurrency == null)
            {
                LoadBaseCurrency();
            }

            return _baseCurrency;
        }

        /// <summary>
        /// Create or update currency
        /// </summary>
        public string SaveCurrency(Currency currency)
        {
            try
            {
                // Validate
                var errors = currency.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                // If setting as base currency, unset any existing base currency
                if (currency.IsBaseCurrency)
                {
                    UnsetBaseCurrency();
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_CURRENCY");

                // Check if currency exists
                bool exists = CurrencyExists(currency.Code);

                if (exists)
                {
                    // Update existing
                    GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                    generalParams.SetProperty("Code", currency.Code);
                    GeneralData generalData = generalService.GetByParams(generalParams);

                    generalData.SetProperty("U_Name", currency.Name);
                    generalData.SetProperty("U_Symbol", currency.Symbol);
                    generalData.SetProperty("U_DecimalPlaces", currency.DecimalPlaces);
                    generalData.SetProperty("U_IsActive", currency.IsActive ? "Y" : "N");
                    generalData.SetProperty("U_IsBaseCurrency", currency.IsBaseCurrency ? "Y" : "N");
                    generalData.SetProperty("U_ModifyDate", DateTime.Now);
                    generalData.SetProperty("U_ModifyUser", _company.UserName);

                    generalService.Update(generalData);
                    Logger.Info($"Currency updated: {currency.Code}");
                }
                else
                {
                    // Create new
                    GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                    generalData.SetProperty("Code", currency.Code);
                    generalData.SetProperty("Name", currency.Name);
                    generalData.SetProperty("U_Name", currency.Name);
                    generalData.SetProperty("U_Symbol", currency.Symbol);
                    generalData.SetProperty("U_DecimalPlaces", currency.DecimalPlaces);
                    generalData.SetProperty("U_IsActive", currency.IsActive ? "Y" : "N");
                    generalData.SetProperty("U_IsBaseCurrency", currency.IsBaseCurrency ? "Y" : "N");
                    generalData.SetProperty("U_CreateDate", DateTime.Now);
                    generalData.SetProperty("U_CreateUser", _company.UserName);

                    generalService.Add(generalData);
                    Logger.Info($"Currency created: {currency.Code}");
                }

                // Refresh cache
                LoadCurrency(currency.Code);
                if (currency.IsBaseCurrency)
                {
                    _baseCurrency = currency;
                }

                return currency.Code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving currency {currency.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if currency exists
        /// </summary>
        private bool CurrencyExists(string currencyCode)
        {
            try
            {
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"SELECT COUNT(*) as CNT FROM ""@CM_CURRENCY"" T0 WHERE T0.""Code"" = '{DatabaseHelper.EscapeSqlString(currencyCode)}'";
                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    int count = SafeConversion.SafeToInt(recordset.Fields.Item("CNT").Value);
                    return count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking currency existence: {currencyCode}", ex);
                return false;
            }
        }

        #endregion

        #region Exchange Rate Management

        /// <summary>
        /// Get exchange rate for a specific date
        /// If no rate exists for the exact date, returns the most recent rate before that date
        /// </summary>
        public ExchangeRate GetExchangeRate(string fromCurrency, string toCurrency, DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                    return null;

                // If same currency, rate is 1.0
                if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                {
                    return new ExchangeRate
                    {
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        Rate = 1.0,
                        RateDate = date ?? DateTime.Today,
                        RateSource = "System"
                    };
                }

                DateTime rateDate = date ?? DateTime.Today;

                // Check cache first
                var pair = new CurrencyPair(fromCurrency.ToUpper(), toCurrency.ToUpper());
                if (_rateCache.ContainsKey(pair))
                {
                    var cachedRate = _rateCache[pair];
                    if (cachedRate.RateDate == rateDate)
                    {
                        return cachedRate;
                    }
                }

                // Query database
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT TOP 1 *
                    FROM ""@CM_EXCHANGE_RATE""
                    WHERE ""U_FromCurrency"" = '{DatabaseHelper.EscapeSqlString(fromCurrency)}'
                    AND ""U_ToCurrency"" = '{DatabaseHelper.EscapeSqlString(toCurrency)}'
                    AND ""U_RateDate"" <= '{rateDate:yyyyMMdd}'
                    AND ""U_IsActive"" = 'Y'
                    ORDER BY ""U_RateDate"" DESC";

                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    var rate = new ExchangeRate
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        FromCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_FromCurrency").Value),
                        ToCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_ToCurrency").Value),
                        RateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_RateDate").Value),
                        Rate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Rate").Value),
                        RateSource = SafeConversion.SafeToString(recordset.Fields.Item("U_RateSource").Value),
                        IsActive = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsActive").Value)
                    };

                    // Update cache
                    _rateCache[pair] = rate;

                    return rate;
                }

                // Try inverse rate (ToCurrency -> FromCurrency)
                var inverseRate = GetInverseExchangeRate(fromCurrency, toCurrency, rateDate);
                if (inverseRate != null)
                {
                    return inverseRate;
                }

                Logger.Warning($"No exchange rate found for {fromCurrency}/{toCurrency} on {rateDate:yyyy-MM-dd}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting exchange rate {fromCurrency}/{toCurrency}", ex);
                return null;
            }
        }

        /// <summary>
        /// Get inverse exchange rate (swap currencies)
        /// </summary>
        private ExchangeRate GetInverseExchangeRate(string fromCurrency, string toCurrency, DateTime rateDate)
        {
            try
            {
                // Look for rate in opposite direction
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);

                string query = $@"
                    SELECT TOP 1 *
                    FROM ""@CM_EXCHANGE_RATE""
                    WHERE ""U_FromCurrency"" = '{DatabaseHelper.EscapeSqlString(toCurrency)}'
                    AND ""U_ToCurrency"" = '{DatabaseHelper.EscapeSqlString(fromCurrency)}'
                    AND ""U_RateDate"" <= '{rateDate:yyyyMMdd}'
                    AND ""U_IsActive"" = 'Y'
                    ORDER BY ""U_RateDate"" DESC";

                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    double inverseRateValue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Rate").Value);

                    return new ExchangeRate
                    {
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        RateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_RateDate").Value),
                        Rate = 1.0 / inverseRateValue, // Inverse the rate
                        RateSource = SafeConversion.SafeToString(recordset.Fields.Item("U_RateSource").Value) + " (Inverse)",
                        IsActive = true
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting inverse exchange rate", ex);
                return null;
            }
        }

        /// <summary>
        /// Save exchange rate
        /// </summary>
        public string SaveExchangeRate(ExchangeRate exchangeRate)
        {
            try
            {
                // Validate
                var errors = exchangeRate.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_EXCHANGE_RATE");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Generate code if not provided
                if (string.IsNullOrWhiteSpace(exchangeRate.Code))
                {
                    exchangeRate.Code = Guid.NewGuid().ToString();
                }

                generalData.SetProperty("Code", exchangeRate.Code);
                generalData.SetProperty("Name", $"{exchangeRate.FromCurrency}/{exchangeRate.ToCurrency}");
                generalData.SetProperty("U_FromCurrency", exchangeRate.FromCurrency);
                generalData.SetProperty("U_ToCurrency", exchangeRate.ToCurrency);
                generalData.SetProperty("U_RateDate", exchangeRate.RateDate);
                generalData.SetProperty("U_Rate", exchangeRate.Rate);
                generalData.SetProperty("U_RateSource", exchangeRate.RateSource);
                generalData.SetProperty("U_IsActive", exchangeRate.IsActive ? "Y" : "N");
                generalData.SetProperty("U_CreateDate", DateTime.Now);
                generalData.SetProperty("U_CreateUser", _company.UserName);

                generalService.Add(generalData);

                // Clear cache to force reload
                var pair = new CurrencyPair(exchangeRate.FromCurrency, exchangeRate.ToCurrency);
                if (_rateCache.ContainsKey(pair))
                {
                    _rateCache.Remove(pair);
                }

                Logger.Info($"Exchange rate saved: {exchangeRate.FromCurrency}/{exchangeRate.ToCurrency} = {exchangeRate.Rate} on {exchangeRate.RateDate:yyyy-MM-dd}");
                return exchangeRate.Code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving exchange rate", ex);
                throw;
            }
        }

        #endregion

        #region Currency Conversion

        /// <summary>
        /// Convert amount from one currency to another
        /// </summary>
        public double ConvertAmount(double amount, string fromCurrency, string toCurrency, DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
                {
                    Logger.Warning("Currency code is empty, cannot convert");
                    return amount;
                }

                // Same currency, no conversion needed
                if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                {
                    return amount;
                }

                // Get exchange rate
                ExchangeRate rate = GetExchangeRate(fromCurrency, toCurrency, date);
                if (rate == null)
                {
                    Logger.Warning($"No exchange rate found for {fromCurrency}/{toCurrency}, cannot convert");
                    return amount; // Return original amount if no rate found
                }

                // Convert
                double convertedAmount = amount * rate.Rate;

                // Round to target currency decimal places
                Currency targetCurrency = GetCurrency(toCurrency);
                if (targetCurrency != null)
                {
                    convertedAmount = targetCurrency.RoundAmount(convertedAmount);
                }

                Logger.Debug($"Converted {amount} {fromCurrency} to {convertedAmount} {toCurrency} using rate {rate.Rate}");
                return convertedAmount;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error converting amount from {fromCurrency} to {toCurrency}", ex);
                return amount; // Return original amount on error
            }
        }

        /// <summary>
        /// Convert amount to base currency
        /// </summary>
        public double ConvertToBaseCurrency(double amount, string fromCurrency, DateTime? date = null)
        {
            var baseCurrency = GetBaseCurrency();
            if (baseCurrency == null)
            {
                Logger.Warning("No base currency defined");
                return amount;
            }

            return ConvertAmount(amount, fromCurrency, baseCurrency.Code, date);
        }

        /// <summary>
        /// Calculate FX gain/loss between two amounts in different periods
        /// </summary>
        public double CalculateFXGainLoss(double amount, string currency, DateTime originalDate, DateTime newDate)
        {
            try
            {
                var baseCurrency = GetBaseCurrency();
                if (baseCurrency == null || currency == baseCurrency.Code)
                {
                    return 0; // No FX gain/loss if base currency
                }

                // Convert using original date rate
                double originalBaseCurrencyAmount = ConvertToBaseCurrency(amount, currency, originalDate);

                // Convert using new date rate
                double newBaseCurrencyAmount = ConvertToBaseCurrency(amount, currency, newDate);

                // FX Gain/Loss = New Amount - Original Amount
                double fxGainLoss = newBaseCurrencyAmount - originalBaseCurrencyAmount;

                Logger.Debug($"FX Gain/Loss: {fxGainLoss} (Original: {originalBaseCurrencyAmount}, New: {newBaseCurrencyAmount})");
                return fxGainLoss;
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating FX gain/loss", ex);
                return 0;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Load all currencies from database into cache
        /// </summary>
        private void LoadCurrencies()
        {
            try
            {
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = @"SELECT * FROM ""@CM_CURRENCY"" ORDER BY ""Code""";
                recordset.DoQuery(query);

                _currencyCache.Clear();

                while (!recordset.EoF)
                {
                    var currency = new Currency
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        Name = SafeConversion.SafeToString(recordset.Fields.Item("U_Name").Value),
                        Symbol = SafeConversion.SafeToString(recordset.Fields.Item("U_Symbol").Value),
                        DecimalPlaces = SafeConversion.SafeToInt(recordset.Fields.Item("U_DecimalPlaces").Value, 2),
                        IsActive = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsActive").Value, true),
                        IsBaseCurrency = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsBaseCurrency").Value, false)
                    };

                    _currencyCache[currency.Code] = currency;
                    recordset.MoveNext();
                }

                Logger.Info($"Loaded {_currencyCache.Count} currencies into cache");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading currencies", ex);
            }
        }

        /// <summary>
        /// Load specific currency from database
        /// </summary>
        private void LoadCurrency(string currencyCode)
        {
            try
            {
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $@"SELECT * FROM ""@CM_CURRENCY"" T0 WHERE T0.""Code"" = '{DatabaseHelper.EscapeSqlString(currencyCode)}'";
                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    var currency = new Currency
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        Name = SafeConversion.SafeToString(recordset.Fields.Item("U_Name").Value),
                        Symbol = SafeConversion.SafeToString(recordset.Fields.Item("U_Symbol").Value),
                        DecimalPlaces = SafeConversion.SafeToInt(recordset.Fields.Item("U_DecimalPlaces").Value, 2),
                        IsActive = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsActive").Value, true),
                        IsBaseCurrency = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsBaseCurrency").Value, false)
                    };

                    _currencyCache[currency.Code] = currency;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading currency {currencyCode}", ex);
            }
        }

        /// <summary>
        /// Load base currency
        /// </summary>
        private void LoadBaseCurrency()
        {
            try
            {
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                // HANA requires table alias (T0) for user table queries
                string query = @"SELECT * FROM ""@CM_CURRENCY"" T0 WHERE T0.""U_IsBaseCurrency"" = 'Y'";
                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    _baseCurrency = new Currency
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        Name = SafeConversion.SafeToString(recordset.Fields.Item("U_Name").Value),
                        Symbol = SafeConversion.SafeToString(recordset.Fields.Item("U_Symbol").Value),
                        DecimalPlaces = SafeConversion.SafeToInt(recordset.Fields.Item("U_DecimalPlaces").Value, 2),
                        IsActive = true,
                        IsBaseCurrency = true
                    };

                    Logger.Info($"Base currency loaded: {_baseCurrency.Code}");
                }
                else
                {
                    Logger.Warning("No base currency defined");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading base currency", ex);
            }
        }

        /// <summary>
        /// Unset existing base currency before setting a new one
        /// </summary>
        private void UnsetBaseCurrency()
        {
            try
            {
                Recordset recordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = @"UPDATE ""@CM_CURRENCY"" SET ""U_IsBaseCurrency"" = 'N' WHERE ""U_IsBaseCurrency"" = 'Y'";
                recordset.DoQuery(query);

                _baseCurrency = null;
                Logger.Info("Base currency unset");
            }
            catch (Exception ex)
            {
                Logger.Error("Error unsetting base currency", ex);
            }
        }

        /// <summary>
        /// Clear all caches
        /// </summary>
        public void ClearCache()
        {
            _currencyCache.Clear();
            _rateCache.Clear();
            _baseCurrency = null;
            Logger.Info("Currency cache cleared");
        }

        #endregion
    }
}
