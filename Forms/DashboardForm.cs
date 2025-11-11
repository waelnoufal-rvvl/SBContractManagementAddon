using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Dashboard for contract analytics and overview
    /// </summary>
    public class DashboardForm
    {
        private ContractManagementApplication _app;
        private SAPbouiCOM.Form _form;
        private ContractService _contractService;

        private const string FORM_TYPE = "FRM_DASHBOARD_V2"; // Versioned to force fresh form creation

        public DashboardForm(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _contractService = new ContractService(_app.Company);
        }

        /// <summary>
        /// Show the form
        /// </summary>
        public void Show()
        {
            try
            {
                // Check if form already exists - TESTING MODE: Close it to force recreation
                try
                {
                    _form = _app.UIApp.Forms.Item($"{FORM_TYPE}_1");
                    if (_form != null)
                    {
                        Logger.Info($"Form {FORM_TYPE}_1 already exists - CLOSING IT to force recreation");
                        _form.Close();
                        _form = null;
                        System.Threading.Thread.Sleep(500);
                    }
                    Logger.Info($"Form {FORM_TYPE}_1 does not exist, creating new one");
                }
                catch (Exception ex)
                {
                    Logger.Info($"Exception checking for existing form: {ex.Message}. Creating new one.");
                }

                CreateForm();
                InitializeControls();
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
            _form.Title = "Contract Management Dashboard";
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
                AddLabel("stTitl2", "ContMgmt", leftMargin, topPosition, 400, 20);
                topPosition += 30;

                // Summary Cards
                AddLabel("stTotC2", "Total:", leftMargin, topPosition, 150, 14);
                AddLabel("stTotV2", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("stActC2", "Active:", leftMargin, topPosition, 150, 14);
                AddLabel("stActV2", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("stTotI2", "IPCs:", leftMargin, topPosition, 150, 14);
                AddLabel("stIPCV2", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += rowHeight;

                AddLabel("stTotO2", "Changes:", leftMargin, topPosition, 150, 14);
                AddLabel("stCOV2", "0", leftMargin + 160, topPosition, colWidth, 14);
                topPosition += 40;

                // Contract List Grid
                AddLabel("stList2", "Cntrcts:", leftMargin, topPosition, 200, 14);
                topPosition += 20;
                AddGrid("grdCnt2", leftMargin, topPosition, 850, 300);
                topPosition += 320;

                // Refresh button
                AddButton("btnRef2", "Refresh", leftMargin, topPosition, 100, 20);
                AddButton("btnExp2", "Export", leftMargin + 110, topPosition, 100, 20);

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
                ((StaticText)_form.Items.Item("stTotV2").Specific).Caption =
                    $"{totalContracts} contracts - {FormatterHelper.FormatCurrency(totalValue)}";
                ((StaticText)_form.Items.Item("stActV2").Specific).Caption =
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
                Grid grid = (Grid)_form.Items.Item("grdCnt2").Specific;
                grid.DataTable = _form.DataSources.DataTables.Add("DT_CONTRACTS");

                // Add columns
                grid.DataTable.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add("Customer", BoFieldsType.ft_AlphaNumeric, 100);
                grid.DataTable.Columns.Add("Description", BoFieldsType.ft_AlphaNumeric, 200);
                grid.DataTable.Columns.Add("Value", BoFieldsType.ft_Price);
                grid.DataTable.Columns.Add("Status", BoFieldsType.ft_AlphaNumeric, 20);
                grid.DataTable.Columns.Add("StartDate", BoFieldsType.ft_Date);
                grid.DataTable.Columns.Add("EndDate", BoFieldsType.ft_Date);

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
                    grid.DataTable.SetValue("StartDate", row, contract.StartDate);
                    grid.DataTable.SetValue("EndDate", row, contract.EndDate);
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
    }
}
