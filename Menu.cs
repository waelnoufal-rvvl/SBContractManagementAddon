using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
                //  If the manu already exists this code will fail
                oMenus.AddEx(oCreationPackage);
            }
            catch (Exception)
            {
                // Menu item already exists, ignore
            }

            try
            {
                // Get the menu collection of the newly added pop-up item
                oMenuItem = Application.SBO_Application.Menus.Item("ContractManagementAddon");
                oMenus = oMenuItem.SubMenus;

                // Create s sub menu
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Form1";
                oCreationPackage.String = "Form1";
                oMenus.AddEx(oCreationPackage);
            }
            catch (Exception)
            {
                // Menu already exists, ignore
            }
        }

        public void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                if (pVal.BeforeAction && pVal.MenuUID == "ContractManagementAddon.Form1")
                {
                    // TODO: Instantiate ContractForm with ContractManagementApplication instance
                    // For now, this requires the form to be opened through the application context
                    // ContractForm contractForm = new ContractForm(_app);
                    // contractForm.Show();
                    Application.SBO_Application.SetStatusBarMessage("Contract Management form - to be implemented", SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
            }
            catch (Exception ex)
            {
                Application.SBO_Application.MessageBox(ex.ToString(), 1, "Ok", "", "");
            }
        }

    }
}
