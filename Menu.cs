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
            oCreationPackage.String = "ContractManagementAddon";
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

                // Create sub menu
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Dashboard";
                oCreationPackage.String = "Contract Dashboard";
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
                if (pVal.BeforeAction && pVal.MenuUID == "ContractManagementAddon.Dashboard")
                {
                    // Create a simple wrapper to get Company from SAP Application
                    var company = (SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany();

                    // Create a minimal app wrapper
                    var appWrapper = new Core.ContractManagementApplicationWrapper(
                        Application.SBO_Application,
                        company);

                    DashboardForm dashboardForm = new DashboardForm(appWrapper);
                    dashboardForm.Show();
                }
            }
            catch (Exception ex)
            {
                Application.SBO_Application.MessageBox(ex.ToString(), 1, "Ok", "", "");
            }
        }

    }
}
