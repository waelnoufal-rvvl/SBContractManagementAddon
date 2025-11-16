using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Contract management form
    /// </summary>
    public class ContractForm
    {
        private IContractManagementApp _app;
        private SAPbouiCOM.Form _form;
        private ContractService _contractService;
        private Contract _currentContract;
        private LanguageManager _lang;
        private bool _isRTL;

        private const string FORM_TYPE = "FRM_CONTRACT";

        // Control IDs
        private const string BTN_NEW = "btnNew";
        private const string BTN_SAVE = "btnSave";
        private const string BTN_DELETE = "btnDelete";
        private const string BTN_FIND = "btnFind";
        private const string TXT_CODE = "txtCode";
        private const string TXT_CUSTOMER = "txtCustomer";
        private const string TXT_DESC = "txtDesc";
        private const string DT_START = "dtStart";
        private const string DT_END = "dtEnd";
        private const string TXT_VALUE = "txtValue";
        private const string CMB_STATUS = "cmbStatus";
        private const string TXT_RETENTION = "txtRetention";
        private const string GRID_LINES = "gridLines";

        public ContractForm(IContractManagementApp app)
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
                AttachEvents();
                LoadNewContract();

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
            _form.Title = _lang.GetString("Form_Contract_Title");
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

                // Buttons (will be localized in LocalizeForm)
                AddButton(BTN_NEW, "", 20, 10, 80, 19);
                AddButton(BTN_SAVE, "", 110, 10, 80, 19);
                AddButton(BTN_DELETE, "", 200, 10, 80, 19);
                AddButton(BTN_FIND, "", 290, 10, 80, 19);

                // Contract Code
                AddLabel("lblCode", "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CODE, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Customer
                AddLabel("lblCustomer", "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTOMER, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Description
                AddLabel("lblDesc", "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_DESC, leftMargin + labelWidth + 10, topPosition, 400, 14);
                topPosition += rowHeight;

                // Start Date
                AddLabel("lblStart", "", leftMargin, topPosition, labelWidth, 14);
                AddEditText(DT_START, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // End Date
                AddLabel("lblEnd", "", leftMargin, topPosition, labelWidth, 14);
                AddEditText(DT_END, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Contract Value
                AddLabel("lblValue", "", leftMargin, topPosition, labelWidth, 14);
                AddEditText(TXT_VALUE, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus", "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CMB_STATUS, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Retention %
                AddLabel("lblRetention", "", leftMargin, topPosition, labelWidth, 14);
                AddEditText(TXT_RETENTION, leftMargin + labelWidth + 10, topPosition, fieldWidth, 14);
                topPosition += rowHeight;

                // Lines Grid
                topPosition += 10;
                AddLabel("lblLines", "", leftMargin, topPosition, labelWidth, 14);
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
                statusCombo.ValidValues.Add("Draft", _lang.GetString("Status_Draft"));
                statusCombo.ValidValues.Add("Active", _lang.GetString("Status_Active"));
                statusCombo.ValidValues.Add("OnHold", _lang.GetString("Status_OnHold"));
                statusCombo.ValidValues.Add("Completed", _lang.GetString("Status_Completed"));
                statusCombo.ValidValues.Add("Cancelled", _lang.GetString("Status_Cancelled"));
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
            Item item = _form.Items.Add(id, BoFormItemTypes.it_STATIC);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
            ((StaticText)item.Specific).Caption = caption;
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

        /// <summary>
        /// Localize all form controls
        /// </summary>
        private void LocalizeForm()
        {
            try
            {
                // Update form title
                _form.Title = _lang.GetString("Form_Contract_Title");

                // Localize buttons
                ((Button)_form.Items.Item(BTN_NEW).Specific).Caption = _lang.GetString("Common_New");
                ((Button)_form.Items.Item(BTN_SAVE).Specific).Caption = _lang.GetString("Common_Save");
                ((Button)_form.Items.Item(BTN_DELETE).Specific).Caption = _lang.GetString("Common_Delete");
                ((Button)_form.Items.Item(BTN_FIND).Specific).Caption = _lang.GetString("Common_Find");

                // Localize labels
                ((StaticText)_form.Items.Item("lblCode").Specific).Caption = _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item("lblCustomer").Specific).Caption = _lang.GetString("Contract_Customer");
                ((StaticText)_form.Items.Item("lblDesc").Specific).Caption = _lang.GetString("Contract_Description");
                ((StaticText)_form.Items.Item("lblStart").Specific).Caption = _lang.GetString("Contract_StartDate");
                ((StaticText)_form.Items.Item("lblEnd").Specific).Caption = _lang.GetString("Contract_EndDate");
                ((StaticText)_form.Items.Item("lblValue").Specific).Caption = _lang.GetString("Contract_TotalValue");
                ((StaticText)_form.Items.Item("lblStatus").Specific).Caption = _lang.GetString("Contract_Status");
                ((StaticText)_form.Items.Item("lblRetention").Specific).Caption = _lang.GetString("Contract_RetentionPercentage");
                ((StaticText)_form.Items.Item("lblLines").Specific).Caption = _lang.GetString("Contract_Lines");

                // Apply RTL if needed
                if (_isRTL)
                {
                    _lang.LocalizeForm(_form);
                }

                Logger.Info("Contract form localized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing form", ex);
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
