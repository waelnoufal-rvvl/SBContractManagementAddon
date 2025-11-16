using System;
using ContractManagementAddon.Core;

namespace ContractManagementAddon
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Use the new architecture with ContractManagementApplication
                ContractManagementApplication app = new ContractManagementApplication();
                app.Run();

                // Keep the application running
                System.Windows.Forms.Application.Run();
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
    }
}
