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
                System.Windows.Forms.MessageBox.Show(ex.Message);
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
