using System;
using SAPbouiCOM;
using ContractManagementAddon.Forms;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Core
{
    /// <summary>
    /// Manages SAP Business One events for the add-on
    /// </summary>
    public class EventManager
    {
        private ContractManagementApplication _app;
        private SAPbouiCOM.Application _uiApp;

        public EventManager(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _uiApp = _app.UIApp;
        }

        /// <summary>
        /// Register event handlers
        /// </summary>
        public void RegisterEvents()
        {
            try
            {
                Logger.Info("Registering event handlers...");

                // Menu event handler
                _uiApp.MenuEvent += new _IApplicationEvents_MenuEventEventHandler(OnMenuEvent);

                // Item event handler (for form events)
                _uiApp.ItemEvent += new _IApplicationEvents_ItemEventEventHandler(OnItemEvent);

                // Application event handler (for shutdown, etc.)
                _uiApp.AppEvent += new _IApplicationEvents_AppEventEventHandler(OnAppEvent);

                Logger.Info("Event handlers registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to register event handlers: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Unregister event handlers
        /// </summary>
        public void UnregisterEvents()
        {
            try
            {
                Logger.Info("Unregistering event handlers...");

                if (_uiApp != null)
                {
                    _uiApp.MenuEvent -= new _IApplicationEvents_MenuEventEventHandler(OnMenuEvent);
                    _uiApp.ItemEvent -= new _IApplicationEvents_ItemEventEventHandler(OnItemEvent);
                    _uiApp.AppEvent -= new _IApplicationEvents_AppEventEventHandler(OnAppEvent);
                }

                Logger.Info("Event handlers unregistered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error unregistering event handlers: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Handle menu click events
        /// </summary>
        private void OnMenuEvent(ref MenuEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            try
            {
                if (pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case MenuManager.MenuIds.Contracts:
                            Logger.Info("Opening Contracts form using Framework pattern...");
                            // Framework-based form - instantiation automatically loads .b1f and calls OnInitializeComponent()
                            ContractForm contractForm = new ContractForm();
                            break;

                        case MenuManager.MenuIds.IPC:
                            Logger.Info("Opening IPC form...");
                            // TODO: Convert to Framework pattern
                            IPCForm ipcForm = new IPCForm(_app);
                            ipcForm.Show();
                            break;

                        case MenuManager.MenuIds.ChangeOrder:
                            Logger.Info("Opening Change Orders form...");
                            // TODO: Convert to Framework pattern
                            ChangeOrderForm coForm = new ChangeOrderForm(_app);
                            coForm.Show();
                            break;

                        case MenuManager.MenuIds.Dashboard:
                            Logger.Info("Opening Dashboard...");
                            // TODO: Convert to Framework pattern
                            DashboardForm dashForm = new DashboardForm(_app);
                            dashForm.Show();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling menu event: " + ex.Message, ex);
                _uiApp.StatusBar.SetText("Error: " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                bubbleEvent = false;
            }
        }

        /// <summary>
        /// Handle form item events
        /// </summary>
        private void OnItemEvent(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            try
            {
                // Handle global events for our custom forms
                if (pVal.FormTypeEx == "ContractManagementAddon.Forms.ContractForm" ||
                    pVal.FormTypeEx == "FRM_IPC" ||
                    pVal.FormTypeEx == "FRM_CO" ||
                    pVal.FormTypeEx == "FRM_DASHBOARD")
                {
                    // Forms will handle their own events
                    // This is just for logging or global handling
                    if (pVal.EventType == BoEventTypes.et_FORM_CLOSE && !pVal.BeforeAction)
                    {
                        Logger.Info($"Form {pVal.FormTypeEx} closed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling item event: " + ex.Message, ex);
                bubbleEvent = false;
            }
        }

        /// <summary>
        /// Handle application events (shutdown, etc.)
        /// </summary>
        private void OnAppEvent(BoAppEventTypes eventType)
        {
            try
            {
                switch (eventType)
                {
                    case BoAppEventTypes.aet_ShutDown:
                        Logger.Info("SAP Business One is shutting down");
                        _app.Shutdown();
                        System.Windows.Forms.Application.Exit();
                        break;

                    case BoAppEventTypes.aet_CompanyChanged:
                        Logger.Info("Company changed - reloading add-on");
                        // Reload connection and UDOs for new company
                        break;

                    case BoAppEventTypes.aet_LanguageChanged:
                        Logger.Info("Language changed");
                        // Reload UI text if supporting multiple languages
                        break;

                    case BoAppEventTypes.aet_ServerTerminition:
                        Logger.Warning("Server connection terminated");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling app event: " + ex.Message, ex);
            }
        }
    }
}
