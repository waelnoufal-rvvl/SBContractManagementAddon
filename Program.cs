using System;
using System.Windows.Forms;
using ContractManagementAddon.Core;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon
{
    /// <summary>
    /// Main entry point for the Contract Management Add-on
    /// </summary>
    class Program
    {
        private static ContractManagementApplication _application;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Logger.Info("=================================================================");
                Logger.Info("Starting Contract Management Add-On for SAP Business One");
                Logger.Info("=================================================================");

                // Initialize and run the application
                _application = new ContractManagementApplication();
                _application.Run();

                // Keep the application running
                Logger.Info("Application is running. Press Ctrl+C or close SAP B1 to exit.");
                Application.Run();
            }
            catch (Exception ex)
            {
                string errorMsg = $"Fatal error starting add-on: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                Logger.Error(errorMsg, ex);
                MessageBox.Show(errorMsg, "Contract Management Add-on Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cleanup on exit
                if (_application != null)
                {
                    try
                    {
                        _application.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error during cleanup: " + ex.Message, ex);
                    }
                }

                Logger.Info("=================================================================");
                Logger.Info("Contract Management Add-On terminated");
                Logger.Info("=================================================================");
            }
        }
    }
}
