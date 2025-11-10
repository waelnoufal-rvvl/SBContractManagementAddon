using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using ContractManagementAddon.Core;

namespace ContractManagementAddon
{
    class Program
    {
        private static ContractManagementApplication _app;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application oApp = null;

            try
            {
                // ✅ 1. Initialize SAP Application Framework
                if (args.Length < 1)
                {
                    // No connection string - running standalone for development
                    oApp = new Application();

                    System.Windows.Forms.MessageBox.Show(
                        "⚠️ Running in development mode (no connection string)\n\n" +
                        "For production deployment:\n" +
                        "1. Copy the compiled EXE/DLL and ContractManagement.ard to your addon folder\n" +
                        "2. Register in SAP B1: Administration → Add-Ons → Add-On Administration\n" +
                        "3. Select the .ard file (not .srf for modern SAP B1 versions)\n" +
                        "4. Complete registration and restart SAP B1\n\n" +
                        "Attempting to connect to running SAP B1 instance...",
                        "Contract Management Add-On",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information
                    );
                }
                else
                {
                    // Connection string provided by SAP B1
                    // If you want to use an add-on identifier for development license:
                    // oApp = new Application(args[0], "YOUR_ADDON_IDENTIFIER");
                    oApp = new Application(args[0]);
                }

                // ✅ 2. Initialize and run the Contract Management Application
                _app = new ContractManagementApplication();
                _app.Run();

                // ✅ 3. Register event handlers
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);

                // ✅ 4. Run the SAP Framework application
                oApp.Run();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to start Contract Management Add-On:\n\n{ex.Message}";

                if (ex.Message.Contains("connection string") || ex.Message.Contains("parameter is incorrect") || ex.Message.Contains("connect"))
                {
                    errorMessage += "\n\n" +
                        "❌ CONNECTION FAILED\n\n" +
                        "This addon cannot run standalone. You must:\n\n" +
                        "OPTION 1 - Register with SAP B1 (Recommended):\n" +
                        "  1. Build your project in Release mode\n" +
                        "  2. Copy files to SAP addon folder:\n" +
                        "     - ContractManagementAddon.exe\n" +
                        "     - ContractManagement.ard (registration file)\n" +
                        "     - All DLL dependencies\n" +
                        "  3. Open SAP B1 → Administration → Add-Ons → Add-On Administration\n" +
                        "  4. Click 'Register Add-On' and select ContractManagement.ard\n" +
                        "     Note: Use .ard file for SAP B1 10.0+, .srf for older versions\n" +
                        "  5. Complete registration wizard\n" +
                        "  6. Restart SAP B1 - addon will auto-start\n\n" +
                        "OPTION 2 - Debug Mode (Development):\n" +
                        "  1. Start SAP Business One and log in first\n" +
                        "  2. Then run/debug your addon from Visual Studio\n" +
                        "  3. The addon will connect to the running SAP B1 instance\n\n" +
                        "Current Error: Cannot find running SAP B1 instance or connection failed.";
                }

                System.Windows.Forms.MessageBox.Show(
                    errorMessage,
                    "Add-On Initialization Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                System.Console.WriteLine(errorMessage);
                System.Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");

                // Don't close immediately - let user read the error
                System.Console.WriteLine("\nPress Enter to exit...");
                System.Console.ReadLine();
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
                    if (_app != null)
                    {
                        _app.Shutdown();
                    }
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }
    }
}
