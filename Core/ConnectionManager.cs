using System;
using System.Configuration;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using SAPbouiCOM;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Core
{
    /// <summary>
    /// Manages connection to SAP Business One UI and DI APIs
    /// Uses the simplified connection pattern from SAP Framework
    /// </summary>
    public class ConnectionManager
    {
        private SAPbouiCOM.Application _uiApp;
        private SAPbobsCOM.Company _company;
        private OdbcConnection _odbcConnection;

        /// <summary>
        /// Get UI Application (gets from SAP Framework)
        /// </summary>
        public SAPbouiCOM.Application GetUIApplication()
        {
            if (_uiApp == null)
            {
                try
                {
                    Logger.Info("Getting SAP Business One UI Application from Framework...");

                    // ✅ Use SAP Framework's Application object (simpler and more reliable)
                    _uiApp = (SAPbouiCOM.Application)SAPbouiCOM.Framework.Application.SBO_Application;

                    if (_uiApp == null)
                    {
                        throw new Exception("Failed to get UI Application from SAP Framework. Ensure SAP B1 is running and addon is launched by SAP B1.");
                    }

                    Logger.Info("✅ Successfully got UI Application from Framework");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get UI Application: " + ex.Message, ex);
                    throw;
                }
            }

            return _uiApp;
        }

        /// <summary>
        /// Get DI Company (gets from SAP Framework using GetDICompany)
        /// </summary>
        public SAPbobsCOM.Company GetCompany()
        {
            if (_company == null || !_company.Connected)
            {
                try
                {
                    Logger.Info("Getting SAP Business One DI Company from Framework...");

                    // ✅ Use SAP Framework's GetDICompany method (simpler and more reliable)
                    _company = (SAPbobsCOM.Company)SAPbouiCOM.Framework.Application.SBO_Application.Company.GetDICompany();

                    if (_company == null)
                    {
                        throw new Exception("Failed to get DI Company from SAP Framework");
                    }

                    if (!_company.Connected)
                    {
                        throw new Exception("DI Company is not connected to SAP B1");
                    }

                    Logger.Info("✅ Successfully got DI Company from Framework");
                    Logger.Info($"   Company: {_company.CompanyName}");
                    Logger.Info($"   Database: {_company.CompanyDB}");
                    Logger.Info($"   Server: {_company.Server}");
                    Logger.Info($"   SAP Version: {_company.Version}");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get DI Company: " + ex.Message, ex);
                    throw;
                }
            }

            return _company;
        }


        /// <summary>
        /// Test if company is connected
        /// </summary>
        public bool IsConnected()
        {
            return _company != null && _company.Connected;
        }

        /// <summary>
        /// Get ODBC Connection to SAP HANA database using HDBODBC driver
        /// </summary>
        public OdbcConnection GetOdbcConnection()
        {
            if (_odbcConnection == null || _odbcConnection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    Logger.Info("Establishing ODBC connection to SAP HANA using HDBODBC driver...");

                    string odbcConnectionString = BuildOdbcConnectionString();

                    if (string.IsNullOrEmpty(odbcConnectionString))
                    {
                        throw new Exception("ODBC connection string not configured");
                    }

                    _odbcConnection = new OdbcConnection(odbcConnectionString);
                    _odbcConnection.Open();

                    Logger.Info("Successfully connected to SAP HANA via ODBC");
                    Logger.Info($"Driver: HDBODBC, Server: {_odbcConnection.DataSource}");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to establish ODBC connection: " + ex.Message, ex);
                    throw;
                }
            }

            return _odbcConnection;
        }

        /// <summary>
        /// Build ODBC connection string for SAP HANA using HDBODBC driver
        /// </summary>
        private string BuildOdbcConnectionString()
        {
            try
            {
                // First try to get from connectionStrings section
                string connStr = ConfigurationManager.ConnectionStrings["HANAODBCConnection"]?.ConnectionString;

                if (!string.IsNullOrEmpty(connStr) && connStr.Contains("UID=;"))
                {
                    // Connection string exists but needs credentials - try to get from appSettings
                    string server = ConfigurationManager.AppSettings["HANAServer"] ?? "localhost";
                    string port = ConfigurationManager.AppSettings["HANAPort"] ?? "30015";
                    string database = ConfigurationManager.AppSettings["HANADatabase"] ?? "";
                    string username = ConfigurationManager.AppSettings["HANAUsername"] ?? "";
                    string password = ConfigurationManager.AppSettings["HANAPassword"] ?? "";

                    // Try to get database info from SAP Company if connected
                    if (_company != null && _company.Connected)
                    {
                        database = string.IsNullOrEmpty(database) ? _company.CompanyDB : database;
                        username = string.IsNullOrEmpty(username) ? _company.UserName : username;
                    }

                    // Build connection string with HDBODBC driver
                    connStr = $"Driver={{HDBODBC}};ServerNode={server}:{port};Database={database};UID={username};PWD={password}";

                    Logger.Info($"Built ODBC connection string: Driver={{HDBODBC}};ServerNode={server}:{port};Database={database};UID={username};PWD=***");
                }
                else if (!string.IsNullOrEmpty(connStr))
                {
                    // Use the configured connection string as-is
                    Logger.Info("Using ODBC connection string from configuration");
                }
                else
                {
                    // No connection string configured - try to build from appSettings
                    string server = ConfigurationManager.AppSettings["HANAServer"] ?? "localhost";
                    string port = ConfigurationManager.AppSettings["HANAPort"] ?? "30015";
                    string database = ConfigurationManager.AppSettings["HANADatabase"] ?? "";
                    string username = ConfigurationManager.AppSettings["HANAUsername"] ?? "";
                    string password = ConfigurationManager.AppSettings["HANAPassword"] ?? "";

                    // Try to get database info from SAP Company if connected
                    if (_company != null && _company.Connected)
                    {
                        database = string.IsNullOrEmpty(database) ? _company.CompanyDB : database;
                        username = string.IsNullOrEmpty(username) ? _company.UserName : username;
                    }

                    connStr = $"Driver={{HDBODBC}};ServerNode={server}:{port};Database={database};UID={username};PWD={password}";

                    Logger.Info($"Built ODBC connection string from appSettings: Driver={{HDBODBC}};ServerNode={server}:{port};Database={database};UID={username};PWD=***");
                }

                return connStr;
            }
            catch (Exception ex)
            {
                Logger.Error("Error building ODBC connection string: " + ex.Message, ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Test if ODBC connection is active
        /// </summary>
        public bool IsOdbcConnected()
        {
            return _odbcConnection != null && _odbcConnection.State == System.Data.ConnectionState.Open;
        }

        /// <summary>
        /// Disconnect from company and close ODBC connection
        /// Note: Company object is managed by SAP, so we just release the COM reference
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Close ODBC connection
                if (_odbcConnection != null)
                {
                    if (_odbcConnection.State == System.Data.ConnectionState.Open)
                    {
                        Logger.Info("Closing ODBC connection...");
                        _odbcConnection.Close();
                    }
                    _odbcConnection.Dispose();
                    _odbcConnection = null;
                    Logger.Info("✅ ODBC connection closed");
                }

                // Release DI Company COM object
                // Note: We don't call Disconnect() as SAP Framework manages the connection
                if (_company != null)
                {
                    try
                    {
                        Logger.Info("Releasing DI Company COM object...");
                        Marshal.ReleaseComObject(_company);
                        _company = null;
                        Logger.Info("✅ DI Company object released");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error releasing company object: " + ex.Message, ex);
                    }
                }

                // Release UI Application COM object
                if (_uiApp != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(_uiApp);
                        _uiApp = null;
                        Logger.Info("✅ UI Application object released");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error releasing UI application object: " + ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during disconnect: " + ex.Message, ex);
            }
        }
    }
}
