using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using ContractManagementAddon.Core;
using ContractManagementAddon.Forms;

namespace ContractManagementAddon
{
    class Menu
    {
        public void AddMenuItems()
        {
            SAPbouiCOM.Menus oMenus = null;
            SAPbouiCOM.MenuItem oMenuItem = null;

            oMenus = Application.SBO_Application.Menus;

            SAPbouiCOM.MenuCreationParams oCreationPackage = null;
            oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
            oMenuItem = Application.SBO_Application.Menus.Item("43520"); // moudles'

            oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
            oCreationPackage.UniqueID = "ContractManagementAddon";
            oCreationPackage.String = "Contract Management";
            oCreationPackage.Enabled = true;
            oCreationPackage.Position = -1;

            oMenus = oMenuItem.SubMenus;

            try
            {
                //  If the menu already exists this code will fail
                oMenus.AddEx(oCreationPackage);
            }
            catch
            {
                // Menu already exists, ignore
            }

            try
            {
                // Get the menu collection of the newly added pop-up item
                oMenuItem = Application.SBO_Application.Menus.Item("ContractManagementAddon");
                oMenus = oMenuItem.SubMenus;

                // Create Contracts menu item
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Contracts";
                oCreationPackage.String = "Contracts";
                oCreationPackage.Position = 1;
                oMenus.AddEx(oCreationPackage);

                // Create IPC menu item
                oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.IPC";
                oCreationPackage.String = "Interim Payment Certificates";
                oCreationPackage.Position = 2;
                oMenus.AddEx(oCreationPackage);

                // Create Change Orders menu item
                oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.ChangeOrders";
                oCreationPackage.String = "Change Orders";
                oCreationPackage.Position = 3;
                oMenus.AddEx(oCreationPackage);

                // Create Dashboard menu item
                oCreationPackage = ((SAPbouiCOM.MenuCreationParams)(Application.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)));
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Dashboard";
                oCreationPackage.String = "Dashboard";
                oCreationPackage.Position = 4;
                oMenus.AddEx(oCreationPackage);
            }
            catch
            {
                // Menu already exists
                Application.SBO_Application.SetStatusBarMessage("Menu Already Exists", SAPbouiCOM.BoMessageTime.bmt_Short, true);
            }
        }

        public void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                if (pVal.BeforeAction)
                {
                    // Create a simple wrapper to get Company from SAP Application
                    var company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();

                    // Create a minimal app wrapper
                    var appWrapper = new Core.ContractManagementApplicationWrapper(
                        Application.SBO_Application,
                        company);

                    switch (pVal.MenuUID)
                    {
                        case "ContractManagementAddon.Contracts":
                            ContractForm contractForm = new ContractForm(appWrapper);
                            contractForm.Show();
                            break;

                        case "ContractManagementAddon.IPC":
                            IPCForm ipcForm = new IPCForm(appWrapper);
                            ipcForm.Show();
                            break;

                        case "ContractManagementAddon.ChangeOrders":
                            ChangeOrderForm coForm = new ChangeOrderForm(appWrapper);
                            coForm.Show();
                            break;

                        case "ContractManagementAddon.Dashboard":
                            DashboardForm dashboardForm = new DashboardForm(appWrapper);
                            dashboardForm.Show();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Application.SBO_Application.MessageBox(ex.ToString(), 1, "Ok", "", "");
            }
        }

    }
}
