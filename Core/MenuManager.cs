using System;
using SAPbouiCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Core
{
    /// <summary>
    /// Manages SAP Business One menu items for the add-on
    /// </summary>
    public class MenuManager
    {
        private SAPbouiCOM.Application _uiApp;
        private const string MENU_ID_MAIN = "CMADDON_MAIN";
        private const string MENU_ID_CONTRACTS = "CMADDON_CONTRACTS";
        private const string MENU_ID_IPC = "CMADDON_IPC";
        private const string MENU_ID_CHANGEORDER = "CMADDON_CO";
        private const string MENU_ID_DASHBOARD = "CMADDON_DASH";

        public MenuManager(SAPbouiCOM.Application uiApp)
        {
            _uiApp = uiApp ?? throw new ArgumentNullException(nameof(uiApp));
        }

        /// <summary>
        /// Add menu items to SAP Business One
        /// </summary>
        public void AddMenuItems()
        {
            try
            {
                Logger.Info("Adding menu items...");

                Menus menus = _uiApp.Menus;
                MenuCreationParams menuCreationParams;

                // Check if main menu already exists
                if (MenuExists(MENU_ID_MAIN))
                {
                    Logger.Info("Menu already exists, removing old menu...");
                    menus.RemoveEx(MENU_ID_MAIN);
                }

                // Add main menu under Modules (position after Sales)
                MenuItem modulesMenu = menus.Item("43520"); // Modules menu
                menuCreationParams = (MenuCreationParams)_uiApp.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                menuCreationParams.Type = BoMenuType.mt_POPUP;
                menuCreationParams.UniqueID = MENU_ID_MAIN;
                menuCreationParams.String = "★ Contract Mgmt"; // Added star icon to verify version
                menuCreationParams.Position = 15;

                modulesMenu.SubMenus.AddEx(menuCreationParams);
                Logger.Info("Main menu added successfully");

                // Add sub-menus
                MenuItem mainMenu = menus.Item(MENU_ID_MAIN);

                // Contracts menu item
                menuCreationParams = (MenuCreationParams)_uiApp.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                menuCreationParams.Type = BoMenuType.mt_STRING;
                menuCreationParams.UniqueID = MENU_ID_CONTRACTS;
                menuCreationParams.String = "Contracts";
                menuCreationParams.Position = 1;
                mainMenu.SubMenus.AddEx(menuCreationParams);

                // IPC menu item
                menuCreationParams = (MenuCreationParams)_uiApp.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                menuCreationParams.Type = BoMenuType.mt_STRING;
                menuCreationParams.UniqueID = MENU_ID_IPC;
                menuCreationParams.String = "Interim Payment Certificates";
                menuCreationParams.Position = 2;
                mainMenu.SubMenus.AddEx(menuCreationParams);

                // Change Orders menu item
                menuCreationParams = (MenuCreationParams)_uiApp.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                menuCreationParams.Type = BoMenuType.mt_STRING;
                menuCreationParams.UniqueID = MENU_ID_CHANGEORDER;
                menuCreationParams.String = "Change Orders";
                menuCreationParams.Position = 3;
                mainMenu.SubMenus.AddEx(menuCreationParams);

                // Dashboard menu item
                menuCreationParams = (MenuCreationParams)_uiApp.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                menuCreationParams.Type = BoMenuType.mt_STRING;
                menuCreationParams.UniqueID = MENU_ID_DASHBOARD;
                menuCreationParams.String = "Dashboard";
                menuCreationParams.Position = 4;
                mainMenu.SubMenus.AddEx(menuCreationParams);

                Logger.Info("All menu items added successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add menu items: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Check if menu exists
        /// </summary>
        private bool MenuExists(string menuId)
        {
            try
            {
                _uiApp.Menus.Item(menuId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove menu items
        /// </summary>
        public void RemoveMenuItems()
        {
            try
            {
                Logger.Info("Removing menu items...");

                if (MenuExists(MENU_ID_MAIN))
                {
                    _uiApp.Menus.RemoveEx(MENU_ID_MAIN);
                    Logger.Info("Menu items removed successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to remove menu items: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get menu ID constants
        /// </summary>
        public static class MenuIds
        {
            public const string Main = MENU_ID_MAIN;
            public const string Contracts = MENU_ID_CONTRACTS;
            public const string IPC = MENU_ID_IPC;
            public const string ChangeOrder = MENU_ID_CHANGEORDER;
            public const string Dashboard = MENU_ID_DASHBOARD;
        }
    }
}
