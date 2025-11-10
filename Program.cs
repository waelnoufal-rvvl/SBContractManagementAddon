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
                        "For development/testing, ensure SAP B1 is running and logged in.",
                        "Contract Management Add-On",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information
                    );

                    // Try to connect to running SAP B1 instance anyway (for development)
                    System.Console.WriteLine("Attempting to connect to running SAP B1 instance...");
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

                if (ex.Message.Contains("connection string") || ex.Message.Contains("parameter is incorrect"))
                {
                    errorMessage += "\n\n" +
                        "Connection Error - Possible Solutions:\n" +
                        "1. Ensure SAP Business One is running and you are logged in\n" +
                        "2. Register this add-on in SAP B1 Add-On Administration\n" +
                        "3. Restart SAP Business One after registration\n" +
                        "4. Check that the addon.srf file is in the correct location";
                }

                System.Windows.Forms.MessageBox.Show(
                    errorMessage,
                    "Add-On Initialization Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                System.Console.WriteLine(errorMessage);
                System.Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
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
