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

                // Try to create Application object with error handling
                try
                {
                    if (args.Length < 1)
                    {
                        // Try alternative connection methods for development
                        System.Windows.Forms.MessageBox.Show(
                            "Attempting to connect to running SAP Business One instance...\n\n" +
                            "If this fails, please:\n" +
                            "1. Ensure SAP B1 is running and logged in\n" +
                            "2. Register this add-on in SAP B1\n" +
                            "3. Restart from SAP B1",
                            "Contract Management Add-On - Development Mode",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);

                        // Try to connect with empty string (works if SAP B1 is running)
                        oApp = new Application("");
                    }
                    else
                    {
                        //If you want to use an add-on identifier for the development license, you can specify an add-on identifier string as the second parameter.
                        //oApp = new Application(args[0], "XXXXX");
                        oApp = new Application(args[0]);
                    }
                }
                catch (Exception connEx)
                {
                    // Connection failed - provide detailed instructions
                    string helpMessage = "CONNECTION FAILED\n\n" +
                        "Cannot connect to SAP Business One.\n\n" +
                        "IMPORTANT: This add-on CANNOT run standalone!\n\n" +
                        "To run this add-on properly:\n\n" +
                        "OPTION 1 - Register the Add-On (Recommended):\n" +
                        "1. Build this project in Visual Studio\n" +
                        "2. Copy ContractManagementAddon.exe to a permanent folder\n" +
                        "3. Open SAP Business One\n" +
                        "4. Go to: Administration → Add-Ons → Add-On Administration\n" +
                        "5. Click 'Register Add-On Manually'\n" +
                        "6. Browse to SAP\\addon.xml in your project folder\n" +
                        "7. Complete registration\n" +
                        "8. Restart SAP B1\n\n" +
                        "OPTION 2 - Attach Debugger:\n" +
                        "1. Start SAP B1 and log in\n" +
                        "2. Register add-on (one-time)\n" +
                        "3. In Visual Studio: Debug → Attach to Process\n" +
                        "4. Select ContractManagementAddon.exe\n\n" +
                        "Error details: " + connEx.Message;

                    System.Windows.Forms.MessageBox.Show(
                        helpMessage,
                        "Cannot Start Add-On",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);

                    return; // Exit the application
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
