using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Contract management form
    /// </summary>
    public class ContractForm
    {
        private ContractManagementApplication _app;
        private SAPbouiCOM.Form _form;
        private ContractService _contractService;
        private Contract _currentContract;

        private const string FORM_TYPE = "FRM_CONTRACT_V3"; // Changed to V3 to force fresh form creation with fixed labels

        // Control IDs - Made unique to avoid SAP B1 caching issues
        private const string BTN_NEW = "btnNew_v3";
        private const string BTN_SAVE = "btnSave_v3";
        private const string BTN_DELETE = "btnDelete_v3";
        private const string BTN_FIND = "btnFind_v3";
        private const string TXT_CODE = "txtCode_v3";
        private const string TXT_CUSTOMER = "txtCustomer_v3";
        private const string TXT_DESC = "txtDesc_v3";
        private const string DT_START = "dtStart_v3";
        private const string DT_END = "dtEnd_v3";
        private const string TXT_VALUE = "txtValue_v3";
        private const string CMB_STATUS = "cmbStatus_v3";
        private const string TXT_RETENTION = "txtRetention_v3";
        private const string GRID_LINES = "gridLines_v3";

        public ContractForm(ContractManagementApplication app)
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
                // DEBUG: Show which version is running (v2.4 = DIAGNOSTIC MODE)
                _app.UIApp.MessageBox($"ContractForm v2.4 - DIAGNOSTIC\nHardcoded label test\nCheck logs for details\nBuild: {System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location)}", 1, "OK", "", "");

                // Check if form already exists - TESTING MODE: Close it to force recreation
                try
                {
                    _form = _app.UIApp.Forms.Item($"{FORM_TYPE}_1");
                    if (_form != null)
                    {
                        Logger.Info($"Form {FORM_TYPE}_1 already exists - CLOSING IT to force recreation with updated labels");
                        _form.Close();
                        _form = null;
                        System.Threading.Thread.Sleep(500); // Give SAP B1 time to fully close the form
                    }
                    Logger.Info($"Form {FORM_TYPE}_1 does not exist, creating new one");
                }
                catch (Exception ex)
                {
                    Logger.Info($"Exception checking for existing form: {ex.Message}. Creating new one.");
                }

                CreateForm();
                Logger.Info("CreateForm() completed successfully");

                InitializeControls();
                Logger.Info("InitializeControls() completed successfully");

                AttachEvents();
                Logger.Info("AttachEvents() completed successfully");

                LoadNewContract();
                Logger.Info("LoadNewContract() completed successfully");

                _form.Visible = true;
                Logger.Info("Contract form opened");
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing Contract form", ex);
                _app.UIApp.StatusBar.SetText($"Error opening form: {ex.Message}",
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
            _form.Title = "Contract Management";
            _form.Width = 800;
            _form.Height = 600;
            _form.Left = 200;
            _form.Top = 100;
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
                int rowHeight = 25;
                int labelWidth = 120;
                int fieldWidth = 200;

                // Buttons
                AddButton(BTN_NEW, "New", 20, 10, 80, 19);
                AddButton(BTN_SAVE, "Save", 110, 10, 80, 19);
                AddButton(BTN_DELETE, "Delete", 200, 10, 80, 19);
                AddButton(BTN_FIND, "Find", 290, 10, 80, 19);

                // Contract Code
                AddLabel("lblCode_v3", "Code:", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CODE, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Customer
                AddLabel("lblCust_v3", "Cust:", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTOMER, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Description
                AddLabel("lblDesc_v3", "Desc:", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_DESC, leftMargin + labelWidth + 10, topPosition, 400, 14);
                topPosition += rowHeight;

                // Start Date
                AddLabel("lblStart_v3", "Start:", leftMargin, topPosition, labelWidth, 14);
                AddEditText(DT_START, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // End Date
                AddLabel("lblEnd_v3", "End:", leftMargin, topPosition, labelWidth, 14);
                AddEditText(DT_END, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Contract Value
                AddLabel("lblValue_v3", "Value:", leftMargin, topPosition, labelWidth, 14);
                AddEditText(TXT_VALUE, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus_v3", "Status:", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CMB_STATUS, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Retention %
                AddLabel("lblRetent_v3", "Retent%:", leftMargin, topPosition, labelWidth, 14);
                AddEditText(TXT_RETENTION, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Lines Grid
                topPosition += 10;
                AddLabel("lblLines_v3", "Lines:", leftMargin, topPosition, labelWidth, 14);
                topPosition += 20;
                AddGrid(GRID_LINES, leftMargin, topPosition, 750, 200);

                Logger.Info("Contract form controls initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing form controls", ex);
                throw;
            }
        }

        /// <summary>
        /// Attach event handlers
        /// </summary>
        private void AttachEvents()
        {
            _form.DataSources.UserDataSources.Add("DS_CODE", BoDataType.dt_SHORT_TEXT, 20);
            _app.UIApp.ItemEvent += OnItemEvent;
        }

        /// <summary>
        /// Handle item events
        /// </summary>
        private void OnItemEvent(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            try
            {
                if (pVal.FormUID != _form.UniqueID)
                    return;

                if (!pVal.BeforeAction)
                {
                    switch (pVal.ItemUID)
                    {
                        case BTN_NEW:
                            LoadNewContract();
                            break;

                        case BTN_SAVE:
                            SaveContract();
                            break;

                        case BTN_DELETE:
                            DeleteContract();
                            break;

                        case BTN_FIND:
                            FindContract();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling item event", ex);
                _app.UIApp.StatusBar.SetText($"Error: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                bubbleEvent = false;
            }
        }

        /// <summary>
        /// Load new contract
        /// </summary>
        private void LoadNewContract()
        {
            _currentContract = new Contract();
            _currentContract.Code = GetNextContractCode();
            LoadContractToForm(_currentContract);
        }

        /// <summary>
        /// Load contract data to form
        /// </summary>
        private void LoadContractToForm(Contract contract)
        {
            try
            {
                ((EditText)_form.Items.Item(TXT_CODE).Specific).Value = contract.Code;
                ((EditText)_form.Items.Item(TXT_CUSTOMER).Specific).Value = contract.CustomerCode ?? "";
                ((EditText)_form.Items.Item(TXT_DESC).Specific).Value = contract.Description ?? "";
                ((EditText)_form.Items.Item(DT_START).Specific).Value = contract.StartDate.ToString("yyyyMMdd");
                ((EditText)_form.Items.Item(DT_END).Specific).Value = contract.EndDate.ToString("yyyyMMdd");
                ((EditText)_form.Items.Item(TXT_VALUE).Specific).Value = contract.TotalValue.ToString();
                ((EditText)_form.Items.Item(TXT_RETENTION).Specific).Value = contract.RetentionPercentage.ToString();

                // Load status combo
                ComboBox statusCombo = (ComboBox)_form.Items.Item(CMB_STATUS).Specific;
                statusCombo.ValidValues.Add("Draft", "Draft");
                statusCombo.ValidValues.Add("Active", "Active");
                statusCombo.ValidValues.Add("OnHold", "On Hold");
                statusCombo.ValidValues.Add("Completed", "Completed");
                statusCombo.ValidValues.Add("Cancelled", "Cancelled");
                statusCombo.Select(contract.Status, BoSearchKey.psk_ByValue);

                LoadLinesToGrid(contract.Lines);
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading contract to form", ex);
                throw;
            }
        }

        /// <summary>
        /// Load lines to grid
        /// </summary>
        private void LoadLinesToGrid(System.Collections.Generic.List<ContractLine> lines)
        {
            try
            {
                Grid grid = (Grid)_form.Items.Item(GRID_LINES).Specific;
                grid.DataTable = _form.DataSources.DataTables.Add(GRID_LINES);

                // Add columns
                grid.DataTable.Columns.Add("Item", BoFieldsType.ft_AlphaNumeric);
                grid.DataTable.Columns.Add("Description", BoFieldsType.ft_AlphaNumeric);
                grid.DataTable.Columns.Add("Quantity", BoFieldsType.ft_Quantity);
                grid.DataTable.Columns.Add("Price", BoFieldsType.ft_Price);
                grid.DataTable.Columns.Add("Total", BoFieldsType.ft_Sum);

                // Load data
                foreach (var line in lines)
                {
                    int row = grid.DataTable.Rows.Count; // Get current row count before adding
                    grid.DataTable.Rows.Add(); // Add() returns void in SAP B1 UI API
                    grid.DataTable.SetValue("Item", row, line.ItemCode);
                    grid.DataTable.SetValue("Description", row, line.ItemDescription);
                    grid.DataTable.SetValue("Quantity", row, line.Quantity);
                    grid.DataTable.SetValue("Price", row, line.UnitPrice);
                    grid.DataTable.SetValue("Total", row, line.LineTotal);
                }

                grid.AutoResizeColumns();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading lines to grid", ex);
                throw;
            }
        }

        /// <summary>
        /// Save contract
        /// </summary>
        private void SaveContract()
        {
            try
            {
                // Get data from form
                Contract contract = GetContractFromForm();

                // Save
                if (string.IsNullOrEmpty(_currentContract?.Code) || _currentContract.Code != contract.Code)
                {
                    // Create new
                    _contractService.CreateContract(contract);
                    _app.UIApp.StatusBar.SetText($"Contract {contract.Code} created successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    // Update existing
                    _contractService.UpdateContract(contract);
                    _app.UIApp.StatusBar.SetText($"Contract {contract.Code} updated successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

                _currentContract = contract;
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving contract", ex);
                _app.UIApp.StatusBar.SetText($"Error saving contract: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Get contract data from form
        /// </summary>
        private Contract GetContractFromForm()
        {
            Contract contract = new Contract
            {
                Code = ((EditText)_form.Items.Item(TXT_CODE).Specific).Value,
                CustomerCode = ((EditText)_form.Items.Item(TXT_CUSTOMER).Specific).Value,
                Description = ((EditText)_form.Items.Item(TXT_DESC).Specific).Value,
                StartDate = DateTime.ParseExact(((EditText)_form.Items.Item(DT_START).Specific).Value, "yyyyMMdd", null),
                EndDate = DateTime.ParseExact(((EditText)_form.Items.Item(DT_END).Specific).Value, "yyyyMMdd", null),
                TotalValue = double.Parse(((EditText)_form.Items.Item(TXT_VALUE).Specific).Value),
                RetentionPercentage = double.Parse(((EditText)_form.Items.Item(TXT_RETENTION).Specific).Value),
                Status = ((ComboBox)_form.Items.Item(CMB_STATUS).Specific).Selected.Value
            };

            // Get lines from grid (simplified)
            // In production, implement full grid data retrieval

            return contract;
        }

        /// <summary>
        /// Delete contract
        /// </summary>
        private void DeleteContract()
        {
            try
            {
                if (_currentContract == null || string.IsNullOrEmpty(_currentContract.Code))
                {
                    _app.UIApp.StatusBar.SetText("No contract to delete",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    return;
                }

                _contractService.DeleteContract(_currentContract.Code);
                _app.UIApp.StatusBar.SetText($"Contract {_currentContract.Code} deleted",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                LoadNewContract();
            }
            catch (Exception ex)
            {
                Logger.Error("Error deleting contract", ex);
                _app.UIApp.StatusBar.SetText($"Error deleting contract: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Find contract
        /// </summary>
        private void FindContract()
        {
            // In production, implement search dialog
            _app.UIApp.StatusBar.SetText("Find function - to be implemented",
                BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
        }

        /// <summary>
        /// Get next contract code
        /// </summary>
        private string GetNextContractCode()
        {
            return "CON-" + DateTime.Now.ToString("yyyyMMddHHmmss");
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
            Logger.Info($"AddLabel called: id='{id}', caption='{caption}', length={caption.Length}");
            Item item = _form.Items.Add(id, BoFormItemTypes.it_STATIC);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;

            StaticText label = (StaticText)item.Specific;
            Logger.Info($"Setting caption for '{id}' to '{caption}'");
            label.Caption = caption;
            Logger.Info($"Caption set successfully. Verifying... actual value: '{label.Caption}'");
        }

        private void AddTextBox(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_EDIT);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
        }

        private void AddEditText(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_EDIT);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
        }

        private void AddComboBox(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_COMBO_BOX);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
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
