using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
    /// <summary>
    /// Repository for Currency and Exchange Rate data access
    /// Provides CRUD operations for currency master data
    /// </summary>
    public class CurrencyRepository
    {
        private readonly Company _company;

        public CurrencyRepository(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        #region Currency Operations

        /// <summary>
        /// Get all currencies
        /// </summary>
        public List<Currency> GetAllCurrencies()
        {
            List<Currency> currencies = new List<Currency>();
            Recordset recordset = null;

            try
            {
                string query = @"SELECT * FROM ""@CM_CURRENCY"" ORDER BY ""Code""";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    currencies.Add(MapCurrency(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {currencies.Count} currencies");
                return currencies;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting all currencies", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get currency by code
        /// </summary>
        public Currency GetByCode(string code)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_CURRENCY"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(code)}'";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"Currency not found: {code}");
                    return null;
                }

                Currency currency = MapCurrency(recordset);
                Logger.Info($"Retrieved currency: {code}");
                return currency;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting currency {code}", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get base currency
        /// </summary>
        public Currency GetBaseCurrency()
        {
            Recordset recordset = null;

            try
            {
                string query = @"SELECT * FROM ""@CM_CURRENCY"" WHERE ""U_IsBaseCurrency"" = 'Y'";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning("No base currency defined");
                    return null;
                }

                return MapCurrency(recordset);
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting base currency", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Map recordset to Currency object
        /// </summary>
        private Currency MapCurrency(Recordset recordset)
        {
            return new Currency
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                Name = SafeConversion.SafeToString(recordset.Fields.Item("U_Name").Value),
                Symbol = SafeConversion.SafeToString(recordset.Fields.Item("U_Symbol").Value),
                DecimalPlaces = SafeConversion.SafeToInt(recordset.Fields.Item("U_DecimalPlaces").Value, 2),
                IsActive = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsActive").Value, true),
                IsBaseCurrency = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsBaseCurrency").Value, false),
                CreateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CreateDate").Value),
                CreateUser = SafeConversion.SafeToString(recordset.Fields.Item("U_CreateUser").Value),
                ModifyDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_ModifyDate").Value),
                ModifyUser = SafeConversion.SafeToString(recordset.Fields.Item("U_ModifyUser").Value)
            };
        }

        #endregion

        #region Exchange Rate Operations

        /// <summary>
        /// Get all exchange rates
        /// </summary>
        public List<ExchangeRate> GetAllExchangeRates()
        {
            List<ExchangeRate> rates = new List<ExchangeRate>();
            Recordset recordset = null;

            try
            {
                string query = @"SELECT * FROM ""@CM_EXCHANGE_RATE"" ORDER BY ""U_RateDate"" DESC, ""U_FromCurrency"", ""U_ToCurrency""";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    rates.Add(MapExchangeRate(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {rates.Count} exchange rates");
                return rates;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting all exchange rates", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get exchange rate by currency pair and date
        /// </summary>
        public ExchangeRate GetExchangeRate(string fromCurrency, string toCurrency, DateTime date)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"
                    SELECT TOP 1 *
                    FROM ""@CM_EXCHANGE_RATE""
                    WHERE ""U_FromCurrency"" = '{DatabaseHelper.EscapeSqlString(fromCurrency)}'
                    AND ""U_ToCurrency"" = '{DatabaseHelper.EscapeSqlString(toCurrency)}'
                    AND ""U_RateDate"" <= '{date:yyyyMMdd}'
                    AND ""U_IsActive"" = 'Y'
                    ORDER BY ""U_RateDate"" DESC";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"Exchange rate not found: {fromCurrency}/{toCurrency} on {date:yyyy-MM-dd}");
                    return null;
                }

                return MapExchangeRate(recordset);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting exchange rate {fromCurrency}/{toCurrency}", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get exchange rates for a currency pair (all dates)
        /// </summary>
        public List<ExchangeRate> GetExchangeRateHistory(string fromCurrency, string toCurrency)
        {
            List<ExchangeRate> rates = new List<ExchangeRate>();
            Recordset recordset = null;

            try
            {
                string query = $@"
                    SELECT *
                    FROM ""@CM_EXCHANGE_RATE""
                    WHERE ""U_FromCurrency"" = '{DatabaseHelper.EscapeSqlString(fromCurrency)}'
                    AND ""U_ToCurrency"" = '{DatabaseHelper.EscapeSqlString(toCurrency)}'
                    ORDER BY ""U_RateDate"" DESC";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    rates.Add(MapExchangeRate(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {rates.Count} exchange rates for {fromCurrency}/{toCurrency}");
                return rates;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting exchange rate history", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Map recordset to ExchangeRate object
        /// </summary>
        private ExchangeRate MapExchangeRate(Recordset recordset)
        {
            return new ExchangeRate
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                FromCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_FromCurrency").Value),
                ToCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_ToCurrency").Value),
                RateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_RateDate").Value),
                Rate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Rate").Value),
                RateSource = SafeConversion.SafeToString(recordset.Fields.Item("U_RateSource").Value),
                IsActive = SafeConversion.SafeToBool(recordset.Fields.Item("U_IsActive").Value, true),
                CreateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CreateDate").Value),
                CreateUser = SafeConversion.SafeToString(recordset.Fields.Item("U_CreateUser").Value)
            };
        }

        #endregion

        #region Conversion Log Operations

        /// <summary>
        /// Get conversion log for a document
        /// </summary>
        public List<CurrencyConversionLog> GetConversionLog(string documentType, string documentCode)
        {
            List<CurrencyConversionLog> logs = new List<CurrencyConversionLog>();
            Recordset recordset = null;

            try
            {
                string query = $@"
                    SELECT *
                    FROM ""@CM_CURRENCY_LOG""
                    WHERE ""U_DocumentType"" = '{DatabaseHelper.EscapeSqlString(documentType)}'
                    AND ""U_DocumentCode"" = '{DatabaseHelper.EscapeSqlString(documentCode)}'
                    ORDER BY ""U_ConversionDate"" DESC";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    logs.Add(MapConversionLog(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {logs.Count} conversion logs for {documentType} {documentCode}");
                return logs;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting conversion log", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Map recordset to CurrencyConversionLog object
        /// </summary>
        private CurrencyConversionLog MapConversionLog(Recordset recordset)
        {
            return new CurrencyConversionLog
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocumentType = SafeConversion.SafeToString(recordset.Fields.Item("U_DocumentType").Value),
                DocumentCode = SafeConversion.SafeToString(recordset.Fields.Item("U_DocumentCode").Value),
                OriginalCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_OriginalCurrency").Value),
                OriginalAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_OriginalAmount").Value),
                TargetCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_TargetCurrency").Value),
                ConvertedAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ConvertedAmount").Value),
                ExchangeRate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ExchangeRate").Value),
                ConversionDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_ConversionDate").Value),
                ConversionUser = SafeConversion.SafeToString(recordset.Fields.Item("U_ConversionUser").Value),
                ConversionReason = SafeConversion.SafeToString(recordset.Fields.Item("U_ConversionReason").Value)
            };
        }

        #endregion
    }
}
