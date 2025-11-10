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

        // Control IDs from .srf file (All ≤9 characters)
        // Standard SAP B1 buttons
        private const string BTN_OK = "1";          // Standard OK button
        private const string BTN_CANCEL = "2";      // Standard Cancel button
        private const string BTN_FIND = "btnFind";  // Find button
        private const string BTN_CUSTOMER = "btnCust"; // Customer chooser

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
        /// Create form from .srf file (Form Designer approach)
        /// </summary>
        private void CreateForm()
        {
            try
            {
                // Load form from .srf file (same directory as .exe)
                string formPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContractForm.srf");

                if (!System.IO.File.Exists(formPath))
                {
                    Logger.Error($"Form file not found: {formPath}");
                    Logger.Info($"Looking in: {AppDomain.CurrentDomain.BaseDirectory}");
                    throw new System.IO.FileNotFoundException($"Form file not found: {formPath}");
                }

                string formXml = System.IO.File.ReadAllText(formPath);

                // Replace placeholder with actual form UID
                formXml = formXml.Replace("FormUID_Placeholder", $"{FORM_TYPE}_1");

                // Load form into SAP B1
                _app.UIApp.LoadBatchActions(ref formXml);

                // Get the form instance
                _form = _app.UIApp.Forms.Item($"{FORM_TYPE}_1");

                Logger.Info($"Form {FORM_TYPE}_1 loaded from .srf file successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading form from .srf: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize form controls (controls already exist from .srf file)
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                // Controls are already created by the .srf file
                // We just need to set initial values and configure behaviors

                // Status combo is already populated in .srf with ValidValues
                // Set default status to Draft
                ComboBox cmbStatus = (ComboBox)_form.Items.Item("cmbStat").Specific;
                if (string.IsNullOrEmpty(cmbStatus.Value))
                {
                    cmbStatus.Select("D", BoSearchKey.psk_ByValue);
                }

                // Initialize grid - add one empty row
                Grid grdLines = (Grid)_form.Items.Item("grdLines").Specific;
                DataTable dtLines = _form.DataSources.DataTables.Item("DT_LINES");
                if (dtLines.Rows.Count == 0)
                {
                    dtLines.Rows.Add();
                }

                Logger.Info("Contract form controls initialized from .srf");
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
                        case BTN_OK:
                            // OK button - Save contract
                            SaveContract();
                            break;

                        case BTN_FIND:
                            // Find button - Search for contract
                            FindContract();
                            break;

                        case BTN_CUSTOMER:
                            // Customer chooser button
                            OpenCustomerChooser();
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
        /// Open customer chooser
        /// </summary>
        private void OpenCustomerChooser()
        {
            try
            {
                // Open SAP B1 Business Partner chooser
                _app.UIApp.ActivateMenuItem("4883"); // Business Partner master data menu
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening customer chooser", ex);
                _app.UIApp.StatusBar.SetText("Customer chooser - to be implemented",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
        }

        /// <summary>
        /// Get next contract code
        /// </summary>
        private string GetNextContractCode()
        {
            return "CON-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        // Helper methods removed - controls are now created from .srf file
    }
}
