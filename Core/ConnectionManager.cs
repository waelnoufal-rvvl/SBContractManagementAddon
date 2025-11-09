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

                        // Check process architecture (SAP B1 requires 32-bit)
                        bool is64Bit = Environment.Is64BitProcess;
                        Logger.Info($"Add-on process: {(is64Bit ? "64-bit" : "32-bit")}");
                        Logger.Info($"Operating System: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");

                        if (is64Bit)
                        {
                            Logger.Error("CRITICAL: Add-on is running as 64-bit but SAP B1 requires 32-bit!");
                            Logger.Error("FIX: In Visual Studio → Solution Configuration → Platform → Select 'x86'");
                            throw new Exception(
                                "Architecture Mismatch!\n\n" +
                                "SAP Business One is 32-bit but this add-on is running as 64-bit.\n\n" +
                                "TO FIX in Visual Studio 2019:\n" +
                                "1. Click the dropdown next to 'Debug' (shows 'AnyCPU' or 'x64')\n" +
                                "2. Click 'Configuration Manager...'\n" +
                                "3. Under 'Active solution platform', select 'x86'\n" +
                                "4. If 'x86' doesn't exist:\n" +
                                "   - Select '<New...>'\n" +
                                "   - Type or select: x86\n" +
                                "   - Copy settings from: AnyCPU\n" +
                                "   - Click OK\n" +
                                "5. Close Configuration Manager\n" +
                                "6. Press F5 to run again\n\n" +
                                "The project is already configured for x86, but Visual Studio\n" +
                                "must be set to use the x86 platform configuration."
                            );
                        }

                        // Check if SAP B1 process is running
                        var sapProcesses = System.Diagnostics.Process.GetProcessesByName("SAP");
                        Logger.Info($"SAP Business One processes found: {sapProcesses.Length}");
                        if (sapProcesses.Length == 0)
                        {
                            Logger.Error("SAP Business One (SAP.exe) is NOT running!");
                            throw new Exception(
                                "SAP Business One is not running!\n\n" +
                                "Please start SAP Business One client:\n" +
                                "1. Launch SAP Business One from Start Menu or Desktop\n" +
                                "2. Log in with your credentials\n" +
                                "3. Select your company database\n" +
                                "4. Wait for the main window to fully load\n" +
                                "5. Then press F5 in Visual Studio to run the add-on"
                            );
                        }
                        else
                        {
                            Logger.Info($"SAP B1 process detected: {sapProcesses[0].ProcessName} (PID: {sapProcesses[0].Id})");
                        }

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
                            Logger.Warning("SAP.exe is running but COM object is not available - SAP B1 may not be fully logged in yet");
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

                        // All methods failed - provide detailed diagnostics
                        string errorMsg =
                            "Cannot connect to SAP Business One.\n\n" +
                            "TROUBLESHOOTING CHECKLIST:\n" +
                            "☐ SAP Business One is running and fully logged in\n" +
                            "☐ You can see the main SAP B1 window with menus\n" +
                            "☐ Visual Studio platform is set to 'x86' (not AnyCPU or x64)\n" +
                            "   → Check dropdown next to 'Debug' button in VS toolbar\n\n" +
                            "DEVELOPMENT MODE SETUP:\n" +
                            "1. Start SAP Business One and log in completely\n" +
                            "2. In Visual Studio: Configuration Manager → Platform → x86\n" +
                            "3. Press F5 to run the add-on\n\n" +
                            "PRODUCTION MODE SETUP:\n" +
                            "1. Build in Release mode (x86 platform)\n" +
                            "2. Register: SAP B1 → Administration → Add-ons → Add-on Administration\n" +
                            "3. SAP B1 will auto-launch the add-on on login\n\n" +
                            "Check the log file for detailed diagnostics:\n" +
                            $"C:\\Logs\\ContractManagement\\ContractManagement_{DateTime.Now:yyyyMMdd}.log";

                        Logger.Error(errorMsg);
                        throw new Exception(errorMsg);
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
