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
    /// </summary>
    public class ConnectionManager
    {
        private SAPbouiCOM.Application _uiApp;
        private SAPbobsCOM.Company _company;
        private string _connectionString;
        private OdbcConnection _odbcConnection;

        /// <summary>
        /// Get UI Application (connects if not already connected)
        /// </summary>
        public SAPbouiCOM.Application GetUIApplication()
        {
            if (_uiApp == null)
            {
                try
                {
                    Logger.Info("Connecting to SAP Business One UI API...");

                    SAPbouiCOM.SboGuiApi sboGuiApi = new SAPbouiCOM.SboGuiApi();

                    // PRODUCTION MODE: Get connection string from SAP B1 (when registered)
                    // SAP B1 passes connection string as first command-line argument when launching add-on
                    _connectionString = Environment.GetCommandLineArgs().Length > 1
                        ? Environment.GetCommandLineArgs()[1]
                        : null;

                    if (!string.IsNullOrEmpty(_connectionString))
                    {
                        // Production: Use connection string from SAP B1
                        Logger.Info("Connecting using connection string from SAP B1 (registered add-on mode)");
                        sboGuiApi.Connect(_connectionString);
                        _uiApp = sboGuiApi.GetApplication();
                        Logger.Info("Successfully connected to SAP B1 (registered mode)");
                    }
                    else
                    {
                        // DEVELOPMENT MODE: Connect to already-running SAP B1 instance
                        Logger.Info("No connection string (development mode) - connecting to running SAP B1...");

                        // Method 1: Try GetActiveObject (most reliable for development)
                        try
                        {
                            _uiApp = (SAPbouiCOM.Application)Marshal.GetActiveObject("SAPbouiCOM.Application");
                            Logger.Info("✓ Connected to running SAP B1 instance (GetActiveObject)");
                            return _uiApp;
                        }
                        catch (Exception ex1)
                        {
                            Logger.Info($"GetActiveObject failed: {ex1.Message}");
                        }

                        // Method 2: Try Connect with empty string
                        try
                        {
                            Logger.Info("Trying SboGuiApi.Connect with empty string...");
                            sboGuiApi.Connect("");
                            _uiApp = sboGuiApi.GetApplication();
                            Logger.Info("✓ Connected with empty connection string");
                            return _uiApp;
                        }
                        catch (Exception ex2)
                        {
                            Logger.Info($"Empty connection string failed: {ex2.Message}");
                        }

                        // Method 3: Try Connect with null
                        try
                        {
                            Logger.Info("Trying SboGuiApi.Connect with null...");
                            sboGuiApi.Connect(null);
                            _uiApp = sboGuiApi.GetApplication();
                            Logger.Info("✓ Connected with null connection string");
                            return _uiApp;
                        }
                        catch (Exception ex3)
                        {
                            Logger.Info($"Null connection string failed: {ex3.Message}");
                        }

                        // All methods failed
                        throw new Exception(
                            "Cannot connect to SAP Business One.\n\n" +
                            "DEVELOPMENT MODE:\n" +
                            "1. Make sure SAP Business One is running and you are logged in\n" +
                            "2. Then run this add-on from Visual Studio (F5)\n\n" +
                            "PRODUCTION MODE:\n" +
                            "1. Build the project in Release mode\n" +
                            "2. Register the add-on using SAP B1 → Administration → Add-ons → Add-on Administration\n" +
                            "3. SAP B1 will launch the add-on automatically when you log in"
                        );
                    }

                    Logger.Info("Successfully connected to UI API");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to connect to UI API: " + ex.Message, ex);
                    throw;
                }
            }

            return _uiApp;
        }

        /// <summary>
        /// Get DI Company (connects if not already connected)
        /// </summary>
        public SAPbobsCOM.Company GetCompany()
        {
            if (_company == null || !_company.Connected)
            {
                try
                {
                    Logger.Info("Connecting to SAP Business One DI API...");

                    _company = new SAPbobsCOM.Company();

                    // Get connection context from UI Application
                    if (_uiApp != null)
                    {
                        string contextCookie = _company.GetContextCookie();
                        string sConnectionContext = _uiApp.Company.GetConnectionContext(contextCookie);

                        if (_company.SetSboLoginContext(sConnectionContext) != 0)
                        {
                            throw new Exception("Failed to set login context: " + _company.GetLastErrorDescription());
                        }

                        if (_company.Connect() != 0)
                        {
                            int errCode = _company.GetLastErrorCode();
                            string errMsg = _company.GetLastErrorDescription();
                            throw new Exception($"Failed to connect to company (Code: {errCode}): {errMsg}");
                        }

                        Logger.Info("Successfully connected to DI API");
                        Logger.Info($"Company: {_company.CompanyName}, DB: {_company.CompanyDB}");
                    }
                    else
                    {
                        throw new Exception("UI Application not initialized");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to connect to DI API: " + ex.Message, ex);
                    throw;
                }
            }

            return _company;
        }

        /// <summary>
        /// Get connection string from registry or environment
        /// </summary>
        private string GetConnectionString()
        {
            try
            {
                // Try to get from environment variable
                string connStr = Environment.GetEnvironmentVariable("B1_CONNECTION_STRING");

                if (!string.IsNullOrEmpty(connStr))
                {
                    return connStr;
                }

                // Try to construct from available information
                Logger.Warning("Connection string not found, attempting alternate connection method");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting connection string: " + ex.Message, ex);
                return string.Empty;
            }
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
                    Logger.Info("ODBC connection closed");
                }

                // Disconnect from SAP DI API
                if (_company != null && _company.Connected)
                {
                    Logger.Info("Disconnecting from company...");
                    _company.Disconnect();
                    Marshal.ReleaseComObject(_company);
                    _company = null;
                    Logger.Info("Disconnected successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during disconnect: " + ex.Message, ex);
            }
        }
    }
}
