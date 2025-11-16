using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Dashboard for contract analytics and overview
    /// </summary>
    public class DashboardForm
    {
        private IContractManagementApp _app;
        private SAPbouiCOM.Form _form;
        private ContractService _contractService;
        private LanguageManager _lang;
        private bool _isRTL;

        private const string FORM_TYPE = "FRM_DASHBOARD";

        public DashboardForm(IContractManagementApp app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _contractService = new ContractService(_app.Company);
            _lang = LanguageManager.Instance;
            _isRTL = _lang.IsRightToLeft;
            _lang.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// Show the form
        /// </summary>
        public void Show()
        {
            try
            {
                // Check if form already exists
                try
                {
                    _form = _app.UIApp.Forms.Item($"{FORM_TYPE}_1");
                    _form.Select();
                    return;
                }
                catch
                {
                    // Form doesn't exist, create new one
                }

                CreateForm();
                InitializeControls();
                LocalizeForm();
                LoadDashboardData();

                _form.Visible = true;
                Logger.Info("Dashboard form opened");

                _app.UIApp.StatusBar.SetText("Contract Management Dashboard",
                    BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing Dashboard form", ex);
                _app.UIApp.StatusBar.SetText($"Error opening dashboard: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Create form structure
        /// </summary>
        private void CreateForm()
        {
            FormCreationParams formParams = (FormCreationParams)_app.UIApp.CreateObject(BoCreatableObjectType.cot_FormCreationParams);
            formParams.UniqueID = $"{FORM_TYPE}_1";
            formParams.FormType = FORM_TYPE;
            formParams.BorderStyle = BoFormBorderStyle.fbs_Sizable;

            _form = _app.UIApp.Forms.AddEx(formParams);
            _form.Title = _lang.GetString("Form_Dashboard_Title");
            _form.Width = 900;
            _form.Height = 650;
            _form.Left = 150;
            _form.Top = 80;
        }

        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                int leftMargin = 20;
                int topPosition = 20;
                int colWidth = 200;
                int rowHeight = 30;

                // Title
                AddLabel("lblTitle", "", leftMargin, topPosition, 400, 20);
                topPosition += 30;

                // Summary Cards
                AddLabel("lblTotCont", "", leftMargin, topPosition, 150, 14);
                AddLabel("lblTotVal", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("lblActCont", "", leftMargin, topPosition, 150, 14);
                AddLabel("lblActVal", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("lblTotIPC", "", leftMargin, topPosition, 150, 14);
                AddLabel("lblIPCVal", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("lblTotCO", "", leftMargin, topPosition, 150, 14);
                AddLabel("lblCOValue", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += 40;

                // Contract List Grid
                AddLabel("lblContLst", "", leftMargin, topPosition, 200, 14);
                topPosition += 20;
                AddGrid("grdContr", leftMargin, topPosition, 850, 300);
                topPosition += 320;

                // Refresh button
                AddButton("btnRefresh", "", leftMargin, topPosition, 100, 20);
                AddButton("btnExport", "", leftMargin + 110, topPosition, 100, 20);

                Logger.Info("Dashboard form controls initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing Dashboard form controls", ex);
                throw;
            }
        }

        /// <summary>
        /// Load dashboard data
        /// </summary>
        private void LoadDashboardData()
        {
            try
            {
                Logger.Info("Loading dashboard data...");

                // Get all contracts
                var contracts = _contractService.GetAllContracts();

                // Calculate summary statistics
                int totalContracts = contracts.Count;
                int activeContracts = 0;
                double totalValue = 0;
                double activeValue = 0;

                foreach (var contract in contracts)
                {
                    totalValue += contract.TotalValue;

                    if (contract.Status == "Active")
                    {
                        activeContracts++;
                        activeValue += contract.TotalValue;
                    }
                }

                // Update summary labels
                ((StaticText)_form.Items.Item("lblTotVal").Specific).Caption =
                    $"{totalContracts} contracts - {FormatterHelper.FormatCurrency(totalValue)}";
                ((StaticText)_form.Items.Item("lblActVal").Specific).Caption =
                    $"{activeContracts} contracts - {FormatterHelper.FormatCurrency(activeValue)}";

                // Load contracts to grid
                LoadContractsToGrid(contracts);

                Logger.Info("Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading dashboard data", ex);
                throw;
            }
        }

        /// <summary>
        /// Load contracts to grid
        /// </summary>
        private void LoadContractsToGrid(System.Collections.Generic.List<Models.Contract> contracts)
        {
            try
            {
                Grid grid = (Grid)_form.Items.Item("grdContr").Specific;
                grid.DataTable = _form.DataSources.DataTables.Add("DT_CONTRACTS");

                // Add columns
                grid.DataTable.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add("Customer", BoFieldsType.ft_AlphaNumeric, 100);
                grid.DataTable.Columns.Add("Description", BoFieldsType.ft_AlphaNumeric, 200);
                grid.DataTable.Columns.Add("Value", BoFieldsType.ft_Price);
                grid.DataTable.Columns.Add("Status", BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add("Start Date", BoFieldsType.ft_Date);
                grid.DataTable.Columns.Add("End Date", BoFieldsType.ft_Date);

                // Load data
                grid.DataTable.Rows.Clear();
                foreach (var contract in contracts)
                {
                    int row = grid.DataTable.Rows.Count; // Get current row count before adding
                    grid.DataTable.Rows.Add(); // Add() returns void in SAP B1 UI API
                    grid.DataTable.SetValue("Code", row, contract.Code);
                    grid.DataTable.SetValue("Customer", row, contract.CustomerName ?? contract.CustomerCode);
                    grid.DataTable.SetValue("Description", row, contract.Description);
                    grid.DataTable.SetValue("Value", row, contract.TotalValue);
                    grid.DataTable.SetValue("Status", row, contract.Status);
                    grid.DataTable.SetValue("Start Date", row, contract.StartDate);
                    grid.DataTable.SetValue("End Date", row, contract.EndDate);
                }

                grid.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading contracts to grid", ex);
                throw;
            }
        }

        // Helper methods for adding controls
        private void AddButton(string id, string caption, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_BUTTON);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
            ((Button)item.Specific).Caption = caption;
        }

        private void AddLabel(string id, string caption, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_STATIC);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
            ((StaticText)item.Specific).Caption = caption;
        }

        private void AddGrid(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_GRID);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
        }

        /// <summary>
        /// Localize all form controls
        /// </summary>
        private void LocalizeForm()
        {
            try
            {
                // Update form title
                _form.Title = _lang.GetString("Form_Dashboard_Title");

                // Localize labels
                ((StaticText)_form.Items.Item("lblTitle").Specific).Caption = _lang.GetString("Form_Dashboard_Title");
                ((StaticText)_form.Items.Item("lblTotCont").Specific).Caption = _lang.GetString("Dashboard_TotalContracts");
                ((StaticText)_form.Items.Item("lblActCont").Specific).Caption = _lang.GetString("Dashboard_ActiveContracts");
                ((StaticText)_form.Items.Item("lblTotIPC").Specific).Caption = _lang.GetString("Dashboard_TotalIPCs");
                ((StaticText)_form.Items.Item("lblTotCO").Specific).Caption = _lang.GetString("Dashboard_TotalChangeOrders");
                ((StaticText)_form.Items.Item("lblContLst").Specific).Caption = _lang.GetString("Dashboard_ActiveContractsList");

                // Localize buttons
                ((Button)_form.Items.Item("btnRefresh").Specific).Caption = _lang.GetString("Common_Refresh");
                ((Button)_form.Items.Item("btnExport").Specific).Caption = _lang.GetString("Common_Export");

                // Apply RTL if needed
                if (_isRTL)
                {
                    _lang.LocalizeForm(_form);
                }

                Logger.Info("Dashboard form localized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing Dashboard form", ex);
            }
        }

        /// <summary>
        /// Handle language change event
        /// </summary>
        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            try
            {
                _isRTL = _lang.IsRightToLeft;
                LocalizeForm();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling language change", ex);
            }
        }
    }
}
