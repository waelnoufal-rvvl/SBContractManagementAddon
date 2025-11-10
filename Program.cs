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
                SAPbouiCOM.SboGuiApi sboGuiApi = null;
                Application oApp = null;

                // Method 1: Try connecting using SboGuiApi directly (for development)
                if (args.Length < 1)
                {
                    try
                    {
                        // Use SboGuiApi to connect to running SAP B1 instance
                        sboGuiApi = new SAPbouiCOM.SboGuiApi();

                        // Try to connect without connection string (attaches to running instance)
                        sboGuiApi.Connect("");

                        // Get the application
                        app = sboGuiApi.GetApplication();

                        System.Windows.Forms.MessageBox.Show(
                            "Successfully connected to running SAP B1 instance!",
                            "Contract Management Add-On",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Could not connect to SAP B1. Please ensure:\n\n" +
                            "1. SAP Business One is running\n" +
                            "2. You are logged into a company\n\n" +
                            "Then run this add-on again.",
                            "Connection Required",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                        return;
                    }

                    // Create Application wrapper
                    oApp = new Application();
                }
                else
                {
                    // Method 2: Normal connection with connection string (from SAP B1)
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
                string errorMsg = "Failed to start Contract Management Add-On:\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Windows.Forms.MessageBox.Show(errorMsg, "Startup Error",
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
