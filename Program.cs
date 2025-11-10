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
            try
            {
                // Check if launched by SAP Business One
                if (args.Length < 1)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "This add-on must be launched by SAP Business One.\n\n" +
                        "To install and register this add-on:\n" +
                        "1. Copy the compiled DLL and addon.srf to your addon folder\n" +
                        "2. Register in SAP B1: Administration → Add-Ons → Add-On Administration\n" +
                        "3. Launch SAP B1 and the addon will start automatically\n\n" +
                        "For development/testing, ensure SAP B1 is running and logged in first.\n" +
                        "The addon will attempt to connect to the running SAP B1 instance...",
                        "Contract Management Add-On",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information
                    );

                    System.Console.WriteLine("No connection string provided. Attempting to connect to running SAP B1 instance...");
                }

                // Use the new ContractManagementApplication class
                _app = new ContractManagementApplication();
                _app.Run();

                // Register app event handler
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);
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
                        "  1. Build your project\n" +
                        "  2. Copy DLL and addon.srf to SAP addon folder\n" +
                        "  3. Open SAP B1 → Administration → Add-Ons → Add-On Administration\n" +
                        "  4. Click 'Register Add-On' and select addon.srf\n" +
                        "  5. Restart SAP B1\n\n" +
                        "OPTION 2 - Debug Mode:\n" +
                        "  1. Start SAP Business One and log in first\n" +
                        "  2. Then run/debug your addon\n" +
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
