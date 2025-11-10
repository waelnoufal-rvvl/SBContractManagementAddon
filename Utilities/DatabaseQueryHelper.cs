using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Database-agnostic query helper for SQL Server and SAP HANA compatibility
    /// Handles differences between database platforms
    /// </summary>
    public class DatabaseQueryHelper
    {
        private readonly Company _company;
        private readonly BoDataServerTypes _dbType;
        private readonly bool _isHANA;

        public DatabaseQueryHelper(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _dbType = company.DbServerType;
            _isHANA = IsHANADatabase();

            Logger.Debug($"Database type detected: {(_isHANA ? "HANA" : "SQL Server")} - {_dbType}");
        }

        /// <summary>
        /// Check if current database is HANA
        /// </summary>
        public bool IsHANADatabase()
        {
            return _dbType == BoDataServerTypes.dst_HANADB;
        }

        /// <summary>
        /// Check if current database is SQL Server
        /// </summary>
        public bool IsSQLServer()
        {
            // Check for all SQL Server versions
            // Note: dst_MSSQL2022 may not exist in older SDK versions, so we cast the enum value
            return _dbType == BoDataServerTypes.dst_MSSQL2012 ||
                   _dbType == BoDataServerTypes.dst_MSSQL2014 ||
                   _dbType == BoDataServerTypes.dst_MSSQL2016 ||
                   _dbType == BoDataServerTypes.dst_MSSQL2017 ||
                   _dbType == BoDataServerTypes.dst_MSSQL2019 ||
                   ((int)_dbType == 9); // MSSQL2022 value (may not exist in enum)
        }

        #region SQL Function Compatibility

        /// <summary>
        /// Get ISNULL/IFNULL/COALESCE function based on database type
        /// SQL Server: ISNULL(value, default)
        /// HANA: IFNULL(value, default) or COALESCE(value, default)
        /// Recommendation: Use COALESCE (works on both)
        /// </summary>
        public string GetNullFunction(string expression, string defaultValue)
        {
            // COALESCE is SQL standard and works on both platforms
            return $"COALESCE({expression}, {defaultValue})";
        }

        /// <summary>
        /// Get MAX with NULL handling
        /// Compatible with both SQL Server and HANA
        /// </summary>
        public string GetMaxWithDefault(string columnName, string defaultValue)
        {
            return $"COALESCE(MAX({columnName}), {defaultValue})";
        }

        /// <summary>
        /// Get current date function
        /// SQL Server: GETDATE()
        /// HANA: CURRENT_DATE or CURRENT_TIMESTAMP
        /// </summary>
        public string GetCurrentDateFunction()
        {
            if (_isHANA)
            {
                return "CURRENT_DATE";
            }
            else
            {
                return "GETDATE()";
            }
        }

        /// <summary>
        /// Get current timestamp function
        /// SQL Server: GETDATE()
        /// HANA: CURRENT_TIMESTAMP
        /// </summary>
        public string GetCurrentTimestampFunction()
        {
            if (_isHANA)
            {
                return "CURRENT_TIMESTAMP";
            }
            else
            {
                return "GETDATE()";
            }
        }

        /// <summary>
        /// Format date for SQL query
        /// Returns date in 'YYYYMMDD' format compatible with both databases
        /// </summary>
        public string FormatDateForSQL(DateTime date)
        {
            // 'YYYYMMDD' format works on both SQL Server and HANA
            return $"'{date:yyyyMMdd}'";
        }

        /// <summary>
        /// Format datetime for SQL query
        /// SQL Server: 'YYYY-MM-DD HH:MM:SS'
        /// HANA: 'YYYY-MM-DD HH:MM:SS'
        /// </summary>
        public string FormatDateTimeForSQL(DateTime dateTime)
        {
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
        }

        /// <summary>
        /// Get string length function
        /// SQL Server: LEN(string)
        /// HANA: LENGTH(string)
        /// </summary>
        public string GetLengthFunction(string expression)
        {
            if (_isHANA)
            {
                return $"LENGTH({expression})";
            }
            else
            {
                return $"LEN({expression})";
            }
        }

        /// <summary>
        /// Get substring function
        /// SQL Server: SUBSTRING(string, start, length)
        /// HANA: SUBSTRING(string, start, length)
        /// Note: Both use the same syntax, but start position may differ (1-based vs 0-based)
        /// </summary>
        public string GetSubstringFunction(string expression, int start, int length)
        {
            // Both use SUBSTRING, 1-based indexing
            return $"SUBSTRING({expression}, {start}, {length})";
        }

        /// <summary>
        /// Get string concatenation
        /// SQL Server: string1 + string2 or CONCAT(string1, string2)
        /// HANA: string1 || string2 or CONCAT(string1, string2)
        /// Recommendation: Use CONCAT for compatibility
        /// </summary>
        public string ConcatenateStrings(params string[] strings)
        {
            if (strings == null || strings.Length == 0)
                return "''";

            if (strings.Length == 1)
                return strings[0];

            // CONCAT is available in both, but with different max parameters
            // Use || for HANA, + for SQL Server for multiple concatenations
            if (_isHANA)
            {
                return string.Join(" || ", strings);
            }
            else
            {
                return string.Join(" + ", strings);
            }
        }

        /// <summary>
        /// Get TOP/LIMIT clause for limiting results
        /// SQL Server: SELECT TOP n
        /// HANA: SELECT ... LIMIT n
        /// </summary>
        public string GetTopClause(int count)
        {
            if (_isHANA)
            {
                return ""; // LIMIT goes at the end
            }
            else
            {
                return $"TOP {count}";
            }
        }

        /// <summary>
        /// Get LIMIT clause (for end of query)
        /// SQL Server: Not used (uses TOP)
        /// HANA: LIMIT n
        /// </summary>
        public string GetLimitClause(int count)
        {
            if (_isHANA)
            {
                return $"LIMIT {count}";
            }
            else
            {
                return ""; // SQL Server uses TOP
            }
        }

        #endregion

        #region Table and Schema Queries

        /// <summary>
        /// Get query to check if user table exists
        /// SQL Server: INFORMATION_SCHEMA.TABLES
        /// HANA: SYS.TABLES or TABLES
        /// </summary>
        public string GetTableExistsQuery(string tableName)
        {
            string escapedName = EscapeString(tableName);

            if (_isHANA)
            {
                return $@"
                    SELECT COUNT(*) as CNT
                    FROM SYS.TABLES
                    WHERE SCHEMA_NAME = '{_company.CompanyDB}'
                    AND TABLE_NAME = '{escapedName}'";
            }
            else
            {
                return $@"
                    SELECT COUNT(*) as CNT
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_CATALOG = '{_company.CompanyDB}'
                    AND TABLE_NAME = '{escapedName}'";
            }
        }

        /// <summary>
        /// Get query to check if column exists in table
        /// SQL Server: INFORMATION_SCHEMA.COLUMNS
        /// HANA: SYS.TABLE_COLUMNS
        /// </summary>
        public string GetColumnExistsQuery(string tableName, string columnName)
        {
            string escapedTable = EscapeString(tableName);
            string escapedColumn = EscapeString(columnName);

            if (_isHANA)
            {
                return $@"
                    SELECT COUNT(*) as CNT
                    FROM SYS.TABLE_COLUMNS
                    WHERE SCHEMA_NAME = '{_company.CompanyDB}'
                    AND TABLE_NAME = '{escapedTable}'
                    AND COLUMN_NAME = '{escapedColumn}'";
            }
            else
            {
                return $@"
                    SELECT COUNT(*) as CNT
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_CATALOG = '{_company.CompanyDB}'
                    AND TABLE_NAME = '{escapedTable}'
                    AND COLUMN_NAME = '{escapedColumn}'";
            }
        }

        #endregion

        #region String Handling and Security

        /// <summary>
        /// Escape single quotes in string for SQL
        /// Doubles single quotes: O'Brien -> O''Brien
        /// </summary>
        public string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("'", "''");
        }

        /// <summary>
        /// Escape and quote string for SQL
        /// </summary>
        public string QuoteString(string value)
        {
            return $"'{EscapeString(value)}'";
        }

        /// <summary>
        /// Validate and sanitize numeric input
        /// </summary>
        public string SanitizeNumeric(object value)
        {
            if (value == null)
                return "0";

            if (double.TryParse(value.ToString(), out double result))
            {
                return result.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            throw new ArgumentException($"Invalid numeric value: {value}");
        }

        /// <summary>
        /// Validate and sanitize integer input
        /// </summary>
        public string SanitizeInteger(object value)
        {
            if (value == null)
                return "0";

            if (int.TryParse(value.ToString(), out int result))
            {
                return result.ToString();
            }

            throw new ArgumentException($"Invalid integer value: {value}");
        }

        /// <summary>
        /// Validate and sanitize double input
        /// </summary>
        public string SanitizeDouble(object value)
        {
            if (value == null)
                return "0.0";

            if (double.TryParse(value.ToString(), out double result))
            {
                return result.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            throw new ArgumentException($"Invalid double value: {value}");
        }

        /// <summary>
        /// Sanitize integer input and return as int (overload for convenience)
        /// </summary>
        public int SanitizeInteger(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            throw new ArgumentException($"Invalid integer value: {value}");
        }

        #endregion

        #region Query Builders

        /// <summary>
        /// Build INSERT query with proper escaping
        /// </summary>
        public string BuildInsertQuery(string tableName, Dictionary<string, object> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("Values dictionary cannot be empty");

            // System tables (not starting with @) don't use quoted column names in HANA
            bool isUserTable = tableName.StartsWith("@");

            List<string> columns = new List<string>();
            List<string> valueStrings = new List<string>();

            foreach (var kvp in values)
            {
                // Quote column names only for user tables
                columns.Add(isUserTable ? $"\"{kvp.Key}\"" : kvp.Key);

                if (kvp.Value == null)
                {
                    valueStrings.Add("NULL");
                }
                else if (kvp.Value is string strValue)
                {
                    valueStrings.Add(QuoteString(strValue));
                }
                else if (kvp.Value is DateTime dtValue)
                {
                    valueStrings.Add(FormatDateForSQL(dtValue));
                }
                else if (kvp.Value is bool boolValue)
                {
                    valueStrings.Add(boolValue ? "'Y'" : "'N'");
                }
                else if (IsNumericType(kvp.Value))
                {
                    valueStrings.Add(SanitizeNumeric(kvp.Value));
                }
                else
                {
                    valueStrings.Add(QuoteString(kvp.Value.ToString()));
                }
            }

            // Quote table name only for user tables
            string formattedTableName = isUserTable ? $"\"{tableName}\"" : tableName;

            return $@"
                INSERT INTO {formattedTableName}
                ({string.Join(", ", columns)})
                VALUES
                ({string.Join(", ", valueStrings)})";
        }

        /// <summary>
        /// Build UPDATE query with proper escaping
        /// </summary>
        public string BuildUpdateQuery(string tableName, Dictionary<string, object> values, string whereClause)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("Values dictionary cannot be empty");

            List<string> setStatements = new List<string>();

            foreach (var kvp in values)
            {
                string valueStr;

                if (kvp.Value == null)
                {
                    valueStr = "NULL";
                }
                else if (kvp.Value is string strValue)
                {
                    valueStr = QuoteString(strValue);
                }
                else if (kvp.Value is DateTime dtValue)
                {
                    valueStr = FormatDateForSQL(dtValue);
                }
                else if (kvp.Value is bool boolValue)
                {
                    valueStr = boolValue ? "'Y'" : "'N'";
                }
                else if (IsNumericType(kvp.Value))
                {
                    valueStr = SanitizeNumeric(kvp.Value);
                }
                else
                {
                    valueStr = QuoteString(kvp.Value.ToString());
                }

                setStatements.Add($"\"{kvp.Key}\" = {valueStr}");
            }

            return $@"
                UPDATE ""{tableName}""
                SET {string.Join(", ", setStatements)}
                WHERE {whereClause}";
        }

        /// <summary>
        /// Build parameterized SELECT query
        /// </summary>
        public string BuildSelectQuery(string tableName, string[] columns, string whereClause = null, string orderBy = null, int? topLimit = null)
        {
            string columnList = columns != null && columns.Length > 0
                ? string.Join(", ", Array.ConvertAll(columns, c => $"\"{c}\""))
                : "*";

            string topClause = topLimit.HasValue ? GetTopClause(topLimit.Value) : "";
            string limitClause = topLimit.HasValue ? GetLimitClause(topLimit.Value) : "";

            string query = $"SELECT {topClause} {columnList} FROM \"{tableName}\"";

            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                query += $" WHERE {whereClause}";
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query += $" ORDER BY {orderBy}";
            }

            if (!string.IsNullOrWhiteSpace(limitClause))
            {
                query += $" {limitClause}";
            }

            return query;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if value is numeric type
        /// </summary>
        private bool IsNumericType(object value)
        {
            return value is int || value is long || value is float || value is double || value is decimal;
        }

        /// <summary>
        /// Execute query and return recordset
        /// </summary>
        public Recordset ExecuteQuery(string query)
        {
            try
            {
                Recordset oRecordset = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecordset.DoQuery(query);
                return oRecordset;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing query: {ex.Message}\nQuery: {query}", ex);
                throw;
            }
        }

        /// <summary>
        /// Execute query and return single value
        /// </summary>
        public T ExecuteScalar<T>(string query, string columnName)
        {
            Recordset oRecordset = ExecuteQuery(query);

            if (!oRecordset.EoF)
            {
                object value = oRecordset.Fields.Item(columnName).Value;

                if (value == null || value == DBNull.Value)
                    return default(T);

                return (T)Convert.ChangeType(value, typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// Execute query and return integer count
        /// </summary>
        public int ExecuteCount(string query)
        {
            return ExecuteScalar<int>(query, "CNT");
        }

        #endregion
    }
}
