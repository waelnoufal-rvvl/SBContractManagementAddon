using System;
using System.Data;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Database helper methods for SAP Business One
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Execute scalar query and return result
        /// </summary>
        public static object ExecuteScalar(Company company, string query)
        {
            Recordset recordset = null;

            try
            {
                Logger.Debug($"Executing scalar query: {query}");

                recordset = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordset.DoQuery(query);

                if (!recordset.EoF)
                {
                    return recordset.Fields.Item(0).Value;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing scalar query: {query}", ex);
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
        /// Execute query and return recordset
        /// </summary>
        public static Recordset ExecuteQuery(Company company, string query)
        {
            try
            {
                Logger.Debug($"Executing query: {query}");

                Recordset recordset = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordset.DoQuery(query);

                return recordset;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing query: {query}", ex);
                throw;
            }
        }

        /// <summary>
        /// Execute non-query (INSERT, UPDATE, DELETE)
        /// </summary>
        public static void ExecuteNonQuery(Company company, string query)
        {
            Recordset recordset = null;

            try
            {
                Logger.Debug($"Executing non-query: {query}");

                recordset = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);
                recordset.DoQuery(query);

                Logger.Debug("Non-query executed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing non-query: {query}", ex);
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
        /// Check if table exists
        /// </summary>
        public static bool TableExists(Company company, string tableName)
        {
            try
            {
                string query = company.DbServerType == BoDataServerTypes.dst_HANADB
                    ? $"SELECT COUNT(*) FROM SYS.TABLES WHERE TABLE_NAME = '{tableName}'"
                    : $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

                object result = ExecuteScalar(company, query);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking if table exists: {tableName}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if user table exists
        /// </summary>
        public static bool UserTableExists(Company company, string tableName)
        {
            try
            {
                // Remove @ prefix if present
                string cleanTableName = tableName.TrimStart('@');

                // Check if the user table exists in OUTB (User Tables)
                string query = $"SELECT COUNT(*) FROM OUTB WHERE TableName = '{cleanTableName}'";
                object result = ExecuteScalar(company, query);

                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking if user table exists: {tableName}", ex);
                return false;
            }
        }

        /// <summary>
        /// Check if user field exists
        /// </summary>
        public static bool UserFieldExists(Company company, string tableName, string fieldName)
        {
            try
            {
                // Remove @ and U_ prefixes if present
                string cleanTableName = tableName.TrimStart('@');
                string cleanFieldName = fieldName.Replace("U_", "");

                // Check in CUFD (Custom User Field Definitions) using TableName column
                string query = $"SELECT COUNT(*) FROM CUFD WHERE \"TableID\" = '@{cleanTableName}' AND \"AliasID\" = '{cleanFieldName}'";
                object result = ExecuteScalar(company, query);

                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking if user field exists: {tableName}.{fieldName}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get next available document number
        /// </summary>
        public static string GetNextDocNum(Company company, string series = "")
        {
            try
            {
                string query = string.IsNullOrEmpty(series)
                    ? "SELECT MAX(CAST(DocNum AS INT)) + 1 FROM [@CONTRACT_HDR]"
                    : $"SELECT MAX(CAST(DocNum AS INT)) + 1 FROM [@CONTRACT_HDR] WHERE Series = '{series}'";

                object result = ExecuteScalar(company, query);

                if (result == null || result == DBNull.Value)
                {
                    return "1";
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting next document number", ex);
                return "1";
            }
        }

        /// <summary>
        /// Escape SQL string value
        /// </summary>
        public static string EscapeSqlString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("'", "''");
        }

        /// <summary>
        /// Format SQL date
        /// </summary>
        public static string FormatSqlDate(DateTime date, Company company)
        {
            if (company.DbServerType == BoDataServerTypes.dst_HANADB)
            {
                return $"'{date:yyyy-MM-dd}'";
            }
            else
            {
                return $"'{date:yyyyMMdd}'";
            }
        }

        /// <summary>
        /// Get database type name
        /// </summary>
        public static string GetDatabaseTypeName(Company company)
        {
            switch (company.DbServerType)
            {
                case BoDataServerTypes.dst_HANADB:
                    return "SAP HANA";
                case BoDataServerTypes.dst_MSSQL2005:
                case BoDataServerTypes.dst_MSSQL2008:
                case BoDataServerTypes.dst_MSSQL2012:
                case BoDataServerTypes.dst_MSSQL2014:
                case BoDataServerTypes.dst_MSSQL2016:
                case BoDataServerTypes.dst_MSSQL2017:
                case BoDataServerTypes.dst_MSSQL2019:
                    return "SQL Server";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Execute query with parameters (simple parameter replacement)
        /// </summary>
        public static Recordset ExecuteQueryWithParams(Company company, string query, params object[] parameters)
        {
            try
            {
                // Simple parameter replacement - in production use proper parameterized queries
                string finalQuery = query;
                for (int i = 0; i < parameters.Length; i++)
                {
                    string paramValue = parameters[i] == null ? "NULL" : $"'{EscapeSqlString(parameters[i].ToString())}'";
                    finalQuery = finalQuery.Replace($"@p{i}", paramValue);
                }

                return ExecuteQuery(company, finalQuery);
            }
            catch (Exception ex)
            {
                Logger.Error("Error executing parameterized query", ex);
                throw;
            }
        }

        /// <summary>
        /// Begin transaction
        /// </summary>
        public static void BeginTransaction(Company company)
        {
            try
            {
                if (company.InTransaction)
                {
                    Logger.Warning("Transaction already in progress");
                    return;
                }

                company.StartTransaction();
                Logger.Debug("Transaction started");
            }
            catch (Exception ex)
            {
                Logger.Error("Error starting transaction", ex);
                throw;
            }
        }

        /// <summary>
        /// Commit transaction
        /// </summary>
        public static void CommitTransaction(Company company)
        {
            try
            {
                if (!company.InTransaction)
                {
                    Logger.Warning("No transaction in progress");
                    return;
                }

                // EndTransaction returns void in SAP B1 DI API - check error after calling
                company.EndTransaction(BoWfTransOpt.wf_Commit);
                int errCode;
                string errMsg;
                company.GetLastError(out errCode, out errMsg);
                if (errCode != 0)
                {
                    throw new Exception($"Failed to commit transaction: {errMsg}");
                }

                Logger.Debug("Transaction committed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error committing transaction", ex);
                throw;
            }
        }

        /// <summary>
        /// Rollback transaction
        /// </summary>
        public static void RollbackTransaction(Company company)
        {
            try
            {
                if (!company.InTransaction)
                {
                    Logger.Warning("No transaction in progress");
                    return;
                }

                // EndTransaction returns void in SAP B1 DI API - check error after calling
                company.EndTransaction(BoWfTransOpt.wf_RollBack);
                int errCode;
                string errMsg;
                company.GetLastError(out errCode, out errMsg);
                if (errCode != 0)
                {
                    Logger.Error($"Failed to rollback transaction: {errMsg}");
                }

                Logger.Debug("Transaction rolled back");
            }
            catch (Exception ex)
            {
                Logger.Error("Error rolling back transaction", ex);
            }
        }
    }
}
