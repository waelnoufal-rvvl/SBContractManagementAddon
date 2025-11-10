using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;

namespace ContractManagementAddon
{
    class Program
    {
        private static SAPbobsCOM.Company company;
        private static SAPbouiCOM.Application app;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Check if SAP B1 is running by trying to get the connection string
                if (args.Length < 1)
                {
                    // No connection string provided - show helpful message
                    string message = "This add-on must be started from SAP Business One.\n\n" +
                                   "To run this add-on:\n" +
                                   "1. Start SAP Business One and log in\n" +
                                   "2. Register this add-on using Add-On Administration\n" +
                                   "3. The add-on will start automatically\n\n" +
                                   "For development/testing:\n" +
                                   "- Ensure SAP B1 is running\n" +
                                   "- The add-on will attempt to connect to the running instance";

                    System.Windows.Forms.MessageBox.Show(message, "Contract Management Add-On",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);
                }

                Application oApp = null;
                if (args.Length < 1)
                {
                    oApp = new Application();
                }
                else
                {
                    //If you want to use an add-on identifier for the development license, you can specify an add-on identifier string as the second parameter.
                    //oApp = new Application(args[0], "XXXXX");
                    oApp = new Application(args[0]);
                }

                // Connect to SAP B1 and get company object
                ConnectToSAP();

                // Setup menu
                Menu MyMenu = new Menu();
                MyMenu.AddMenuItems();
                oApp.RegisterMenuEventHandler(MyMenu.SBO_Application_MenuEvent);
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);

                oApp.Run();
            }
            catch (Exception ex)
            {
                string errorMsg = "Failed to start Contract Management Add-On:\n\n" + ex.Message;
                if (ex.Message.Contains("Could not find SBO"))
                {
                    errorMsg += "\n\nPlease ensure:\n" +
                               "1. SAP Business One is running\n" +
                               "2. You are logged into a company\n" +
                               "3. The add-on is registered in Add-On Administration";
                }
                System.Windows.Forms.MessageBox.Show(errorMsg, "Connection Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Connect to SAP Business One and retrieve company object
        /// </summary>
        private static void ConnectToSAP()
        {
            try
            {
                // Get the DI Company object from the running SAP B1 instance
                company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();

                // Get the UI Application object
                app = (SAPbouiCOM.Application)Application.SBO_Application;

                if (company != null && company.Connected)
                {
                    string message = $"Connected to SAP B1 - Company: {company.CompanyName}, DB: {company.CompanyDB}";
                    Application.SBO_Application.SetStatusBarMessage(message, SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
                else
                {
                    throw new Exception("Failed to connect to SAP Business One");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection error: {ex.Message}");
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
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
