using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using ContractManagementAddon.Forms;
using ContractManagementAddon.Core;

namespace ContractManagementAddon
{
    class Menu
    {
        private ContractManagementApplication _app;

        public Menu(ContractManagementApplication app)
        {
            _app = app;
        }

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
                // Menu already exists, continue
            }

            try
            {
                // Get the menu collection of the newly added pop-up item
                oMenuItem = Application.SBO_Application.Menus.Item("ContractManagementAddon");
                oMenus = oMenuItem.SubMenus;

                // Create Dashboard menu
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "ContractManagementAddon.Dashboard";
                oCreationPackage.String = "Dashboard";
                oMenus.AddEx(oCreationPackage);

                // Create Contract menu
                oCreationPackage.UniqueID = "ContractManagementAddon.Contract";
                oCreationPackage.String = "Contracts";
                oMenus.AddEx(oCreationPackage);

                // Create IPC menu
                oCreationPackage.UniqueID = "ContractManagementAddon.IPC";
                oCreationPackage.String = "IPCs";
                oMenus.AddEx(oCreationPackage);

                // Create Change Order menu
                oCreationPackage.UniqueID = "ContractManagementAddon.ChangeOrder";
                oCreationPackage.String = "Change Orders";
                oMenus.AddEx(oCreationPackage);
            }
            catch
            { //  Menu already exists
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
                    switch (pVal.MenuUID)
                    {
                        case "ContractManagementAddon.Dashboard":
                            DashboardForm dashboardForm = new DashboardForm(_app);
                            dashboardForm.Show();
                            break;

                        case "ContractManagementAddon.Contract":
                            // Framework-based form - instantiation automatically loads .b1f and calls OnInitializeComponent()
                            Contract contractForm = new Contract();
                            break;

                        case "ContractManagementAddon.IPC":
                            IPCForm ipcForm = new IPCForm(_app);
                            ipcForm.Show();
                            break;

                        case "ContractManagementAddon.ChangeOrder":
                            ChangeOrderForm coForm = new ChangeOrderForm(_app);
                            coForm.Show();
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
