using SAPbouiCOM.Framework;
using System;
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
                // Use SAP Framework Application for connection handling
                Application oApp = null;
                if (args.Length < 1)
                {
                    oApp = new Application();
                }
                else
                {
                    // Connection string passed by SAP B1
                    oApp = new Application(args[0]);
                }

                // Initialize our application after SAP connection is established
                _app = new ContractManagementApplication();
                _app.Run();

                // Register shutdown handler
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);

                // Run the SAP application
                oApp.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Failed to start Contract Management Add-On:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Contract Management Add-On Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    if (_app != null)
                    {
                        _app.Shutdown();
                    }
                    System.Windows.Forms.Application.Exit();
                    break;
            }
        }
    }
}
