using System;
using System.Configuration;
using SAPbouiCOM;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;
using ContractManagementAddon.DataAccess;
using ContractManagementAddon.Services;

namespace ContractManagementAddon.Core
{
    /// <summary>
    /// Interface for accessing SAP B1 connection
    /// </summary>
    public interface IContractManagementApp
    {
        SAPbouiCOM.Application UIApp { get; }
        SAPbobsCOM.Company Company { get; }
    }

    /// <summary>
    /// Lightweight wrapper for forms that already have a SAP connection
    /// </summary>
    public class ContractManagementApplicationWrapper : IContractManagementApp
    {
        private SAPbouiCOM.Application _uiApp;
        private SAPbobsCOM.Company _company;

        public SAPbouiCOM.Application UIApp => _uiApp;
        public SAPbobsCOM.Company Company => _company;

        public ContractManagementApplicationWrapper(SAPbouiCOM.Application uiApp, SAPbobsCOM.Company company)
        {
            _uiApp = uiApp ?? throw new ArgumentNullException(nameof(uiApp));
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }
    }

    /// <summary>
    /// Main application class that manages add-on lifecycle
    /// </summary>
    public class ContractManagementApplication : IContractManagementApp
    {
        private SAPbouiCOM.Application _uiApp;
        private SAPbobsCOM.Company _company;
        private ConnectionManager _connectionManager;
        private MenuManager _menuManager;
        private EventManager _eventManager;
        private UDOManager _udoManager;

        // Phase 1 Services
        private AuthorizationManager _authManager;
        private ApprovalWorkflowManager _approvalManager;
        private EmailNotificationService _emailService;
        private AttachmentService _attachmentService;
        private CurrencyService _currencyService;
        private RevenueRecognitionService _revenueService;
        private ContractService _contractService;
        private IPCService _ipcService;
        //private ChangeOrderService _changeOrderService;

        public SAPbouiCOM.Application UIApp => _uiApp;
        public SAPbobsCOM.Company Company => _company;

        // Public accessors for Phase 1 services
        public AuthorizationManager AuthManager => _authManager;
        public ApprovalWorkflowManager ApprovalManager => _approvalManager;
        public EmailNotificationService EmailService => _emailService;
        public AttachmentService AttachmentService => _attachmentService;
        public CurrencyService CurrencyService => _currencyService;
        public RevenueRecognitionService RevenueService => _revenueService;
        public ContractService ContractService => _contractService;
        public IPCService IPCService => _ipcService;
        //public ChangeOrderService ChangeOrderService => _changeOrderService;

        /// <summary>
        /// Initialize and run the add-on
        /// </summary>
        public void Run()
        {
            try
            {
                Logger.Info("Initializing Contract Management Add-On...");

                // Connect to SAP Business One
                _connectionManager = new ConnectionManager();
                _uiApp = _connectionManager.GetUIApplication();
                _company = _connectionManager.GetCompany();

                if (_company == null || !_company.Connected)
                {
                    throw new Exception("Failed to connect to SAP Business One");
                }

                Logger.Info($"Connected to company: {_company.CompanyName}");
                Logger.Info($"Database: {_company.DbServerType} - {_company.CompanyDB}");

                // Initialize UDO (User Defined Objects)
                _udoManager = new UDOManager(_company);
                _udoManager.CreateUDOs();

                // Initialize Phase 1 Services
                Logger.Info("Initializing Phase 1 services...");

                _authManager = new AuthorizationManager(_company);
                _currencyService = new CurrencyService(_company);
                _revenueService = new RevenueRecognitionService(_company);
                _approvalManager = new ApprovalWorkflowManager(_company);
                _emailService = new EmailNotificationService(_company);
                _attachmentService = new AttachmentService(_company);

                // Initialize business services
                _contractService = new ContractService(_company);
                _ipcService = new IPCService(_company);
                //_changeOrderService = new ChangeOrderService(_company);

                // Create approval templates (first-time setup)
                try
                {
                    _approvalManager.CreateApprovalTemplates();
                    Logger.Info("Approval templates initialized");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Approval templates already exist or error: {ex.Message}");
                }

                // Note: To seed currency data on first installation, uncomment the line below:
                // CurrencySeedData.SeedAllCurrencyData(_company, "USD");

                Logger.Info("Phase 1 services initialized successfully");

                // Setup menus
                _menuManager = new MenuManager(_uiApp);
                _menuManager.AddMenuItems();

                // Setup event handlers
                _eventManager = new EventManager(this);
                _eventManager.RegisterEvents();

                Logger.Info("Contract Management Add-On started successfully");
                _uiApp.StatusBar.SetText("Contract Management Add-On v2.0.0 (Phase 1) loaded successfully",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize add-on: " + ex.Message, ex);
                if (_uiApp != null)
                {
                    _uiApp.StatusBar.SetText("Failed to load Contract Management Add-On: " + ex.Message,
                        BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
                }
                throw;
            }
        }

        /// <summary>
        /// Cleanup resources on shutdown
        /// </summary>
        public void Shutdown()
        {
            try
            {
                Logger.Info("Shutting down Contract Management Add-On...");

                if (_eventManager != null)
                {
                    _eventManager.UnregisterEvents();
                }

                if (_company != null && _company.Connected)
                {
                    _company.Disconnect();
                }

                Logger.Info("Contract Management Add-On shut down successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during shutdown: " + ex.Message, ex);
            }
        }
    }
}
