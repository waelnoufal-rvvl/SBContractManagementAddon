using SAPbouiCOM.Framework;
using System;
using System.IO;
using ContractManagementAddon.Core;
using ContractManagementAddon.DataAccess;

namespace ContractManagementAddon
{
    class Program
    {
        #region Properties

        public static SAPbobsCOM.Company company;
        public static SAPbouiCOM.Application app;
        private static MenuManager menuManager;

        private static string logFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"ContractManagementAddon_{DateTime.Now:yyyyMMdd}.log"
        );
        #endregion

        #region Main Entry Point

        [STAThread]
        static void Main(string[] args)
        {
            Application oApp = null;

            try
            {
                LogMessage("========================================");
                LogMessage("Contract Management Add-On Starting...");
                LogMessage($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                LogMessage("========================================");

                // ✅ 1. Initialize SAP Application
                if (!InitializeApplication(args, out oApp))
                {
                    LogError("Failed to initialize SAP Application");
                    return;
                }

                // ✅ 2. Get Company and Application objects
                if (!ConnectToSAP())
                {
                    LogError("Failed to connect to SAP");
                    return;
                }

                // ✅ 3. Initialize UDO (User Defined Objects)
                InitializeUDOs();

                // ✅ 4. Setup menu using MenuManager
                if (!SetupMenu())
                {
                    LogError("Failed to setup menu");
                    ShowWarning("Menu setup failed. Check log file for details.");
                }

                // ✅ 5. Register event handlers
                RegisterEventHandlers(oApp);

                LogMessage("✅ Contract Management Add-On started successfully!");
                ShowSuccess("Contract Management Add-On loaded successfully!");

                // ✅ 6. Run application
                oApp.Run();
            }
            catch (Exception ex)
            {
                LogException("Critical error in Main", ex);
                ShowError($"Critical Error:\n{ex.Message}\n\nCheck log file:\n{logFilePath}");
            }
            finally
            {
                CleanupResources();
                LogMessage("Application terminated.");
            }
        }
        #endregion

        #region Initialization Methods

        private static bool InitializeApplication(string[] args, out Application oApp)
        {
            try
            {
                LogMessage("Initializing SAP Application Framework...");

                if (args.Length < 1)
                {
                    LogMessage("No connection string provided - attempting to connect to running SAP B1 instance");
                    oApp = new Application();
                }
                else
                {
                    LogMessage($"Using connection string from SAP B1");
                    // If you want to use an add-on identifier for the development license:
                    // oApp = new Application(args[0], "YOUR_ADDON_IDENTIFIER");
                    oApp = new Application(args[0]);
                }

                LogMessage("✅ SAP Application Framework initialized");
                return true;
            }
            catch (Exception ex)
            {
                oApp = null;
                LogException("Failed to initialize Application", ex);
                ShowError($"Failed to initialize SAP Application Framework.\n\n" +
                         $"Please ensure:\n" +
                         $"1. SAP Business One is running\n" +
                         $"2. You are logged into a company\n" +
                         $"3. Or register this add-on in Add-On Administration\n\n" +
                         $"Error: {ex.Message}");
                return false;
            }
        }

        private static bool ConnectToSAP()
        {
            try
            {
                LogMessage("Connecting to SAP Business One...");

                // Get DI Company object
                company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();

                if (company == null)
                {
                    LogError("Failed to get DI Company object");
                    return false;
                }

                if (!company.Connected)
                {
                    LogError("DI Company is not connected");
                    return false;
                }

                // Get UI Application object
                app = (SAPbouiCOM.Application)Application.SBO_Application;

                if (app == null)
                {
                    LogError("Failed to get UI Application object");
                    return false;
                }

                LogMessage($"✅ Connected to SAP - Company: {company.CompanyName}");
                LogMessage($"   Database: {company.CompanyDB}");
                LogMessage($"   Server: {company.Server}");
                LogMessage($"   SAP Version: {company.Version}");

                return true;
            }
            catch (Exception ex)
            {
                LogException("Failed to connect to SAP", ex);
                return false;
            }
        }

        private static void InitializeUDOs()
        {
            try
            {
                LogMessage("Initializing User Defined Objects...");

                UDOManager udoManager = new UDOManager(company);
                udoManager.CreateUDOs();

                LogMessage("✅ UDOs initialized successfully");
            }
            catch (Exception ex)
            {
                LogException("Failed to initialize UDOs", ex);
                ShowWarning("UDO initialization failed. Some features may not work. Check log file.");
            }
        }

        private static bool SetupMenu()
        {
            try
            {
                LogMessage("Setting up menu using MenuManager...");

                menuManager = new MenuManager(app);
                menuManager.AddMenuItems();

                LogMessage("✅ Menu setup completed");
                return true;
            }
            catch (Exception ex)
            {
                LogException("Failed to setup menu", ex);
                return false;
            }
        }

        private static void RegisterEventHandlers(Application oApp)
        {
            try
            {
                LogMessage("Registering event handlers...");

                // Application events
                Application.SBO_Application.AppEvent +=
                    new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);

                // Menu events
                Application.SBO_Application.MenuEvent +=
                    new SAPbouiCOM._IApplicationEvents_MenuEventEventHandler(SBO_Application_MenuEvent);

                LogMessage("✅ Event handlers registered");
            }
            catch (Exception ex)
            {
                LogException("Failed to register event handlers", ex);
            }
        }

        #endregion

        #region Application Events

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            try
            {
                switch (EventType)
                {
                    case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                        LogMessage("Shutdown event received");
                        CleanupResources();
                        System.Windows.Forms.Application.Exit();
                        break;

                    case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                        LogMessage("Company changed event received");
                        // Reinitialize company object
                        company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();
                        LogMessage($"New company: {company?.CompanyName}");
                        break;

                    case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                        LogMessage("Font changed event received");
                        break;

                    case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                        LogMessage("Language changed event received");
                        break;

                    case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                        LogMessage("Server termination event received");
                        CleanupResources();
                        System.Windows.Forms.Application.Exit();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException($"Error handling AppEvent: {EventType}", ex);
            }
        }

        static void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                if (pVal.BeforeAction)
                {
                    LogMessage($"Menu clicked: {pVal.MenuUID}");

                    switch (pVal.MenuUID)
                    {
                        case "CMADDON_CONTRACTS":
                            app.SetStatusBarMessage("Contracts form - Coming soon!", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            break;

                        case "CMADDON_IPC":
                            app.SetStatusBarMessage("IPC form - Coming soon!", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            break;

                        case "CMADDON_CO":
                            app.SetStatusBarMessage("Change Orders form - Coming soon!", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            break;

                        case "CMADDON_DASH":
                            app.SetStatusBarMessage("Dashboard form - Coming soon!", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException("Error in MenuEvent", ex);
                app.MessageBox(ex.Message, 1, "OK", "", "");
            }
        }

        #endregion

        #region Cleanup

        private static void CleanupResources()
        {
            try
            {
                LogMessage("Cleaning up resources...");

                if (company != null && company.Connected)
                {
                    try
                    {
                        // Note: Usually you don't disconnect the company object
                        // as it's managed by SAP, but we release the COM object
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(company);
                        company = null;
                        LogMessage("✅ Company object released");
                    }
                    catch (Exception ex)
                    {
                        LogException("Error releasing company object", ex);
                    }
                }

                LogMessage("✅ Cleanup completed");
            }
            catch (Exception ex)
            {
                LogException("Error in CleanupResources", ex);
            }
        }

        #endregion

        #region Logging Methods

        private static void LogMessage(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

                // Also write to debug output
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch
            {
                // Ignore logging errors to prevent infinite loops
            }
        }

        private static void LogError(string message)
        {
            LogMessage($"❌ ERROR: {message}");
        }

        private static void LogException(string context, Exception ex)
        {
            LogMessage("========================================");
            LogMessage($"❌ EXCEPTION: {context}");
            LogMessage($"Message: {ex.Message}");
            LogMessage($"Type: {ex.GetType().FullName}");
            LogMessage($"Stack Trace:\n{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                LogMessage($"Inner Exception: {ex.InnerException.Message}");
                LogMessage($"Inner Stack Trace:\n{ex.InnerException.StackTrace}");
            }

            LogMessage("========================================");
        }

        #endregion

        #region UI Helper Methods

        private static void ShowError(string message)
        {
            try
            {
                if (app != null)
                {
                    app.MessageBox(message, 1, "OK", "", "");
                    app.SetStatusBarMessage(message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                        message,
                        "Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(message, "Error");
            }
        }

        private static void ShowWarning(string message)
        {
            try
            {
                if (app != null)
                {
                    app.SetStatusBarMessage(message, SAPbouiCOM.BoMessageTime.bmt_Medium, false);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                        message,
                        "Warning",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Warning
                    );
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(message, "Warning");
            }
        }

        private static void ShowSuccess(string message)
        {
            try
            {
                if (app != null)
                {
                    app.SetStatusBarMessage(message, SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
            }
            catch
            {
                // Ignore
            }
        }

        #endregion
    }
}
