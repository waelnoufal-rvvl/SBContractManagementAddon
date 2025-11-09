using System;
using System.Collections.Generic;
using SAPbouiCOM;
using ContractManagementAddon.Localization;
using ContractManagementAddon.Models;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Example: Localized Contract Form
    /// Demonstrates proper implementation of multi-language support
    /// </summary>
    public class ContractFormLocalized
    {
        #region Private Fields

        private SAPbouiCOM.Application _application;
        private Form _form;
        private LanguageManager _lang;
        private bool _isRTL;

        // Form items
        private EditText _txtContractCode;
        private EditText _txtContractName;
        private EditText _txtCustomerCode;
        private EditText _txtCustomerName;
        private EditText _txtContactPerson;
        private EditText _txtEmail;
        private EditText _txtPhone;
        private EditText _txtStartDate;
        private EditText _txtEndDate;
        private ComboBox _cboContractType;
        private ComboBox _cboCurrency;
        private EditText _txtExchangeRate;
        private EditText _txtTotalValue;
        private Button _btnSave;
        private Button _btnCancel;
        private Button _btnBrowseCustomer;
        private Button _btnLanguage;
        private StaticText _lblStatus;
        private Matrix _mtxLines;

        #endregion

        #region Constructor

        public ContractFormLocalized(SAPbouiCOM.Application application)
        {
            _application = application;
            _lang = LanguageManager.Instance;
            _isRTL = _lang.IsRightToLeft;

            // Subscribe to language change event
            _lang.LanguageChanged += OnLanguageChanged;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create and show the form
        /// </summary>
        public void Show()
        {
            try
            {
                // Create form from XML or programmatically
                CreateForm();

                // Localize all controls
                LocalizeForm();

                // Apply RTL layout if needed
                if (_isRTL)
                {
                    ApplyRTLLayout();
                }

                // Show form
                _form.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Error creating form", ex);
            }
        }

        #endregion

        #region Form Creation

        private void CreateForm()
        {
            // Create form definition XML
            var formXML = CreateFormXML();

            // Load form from XML
            _application.LoadBatchActions(ref formXML);

            // Get form instance
            var formUID = "frmContract_" + DateTime.Now.Ticks;
            _form = _application.Forms.Item(formUID);

            // Get form items
            GetFormItems();

            // Set default values
            SetDefaultValues();

            // Wire up events
            WireUpEvents();
        }

        private string CreateFormXML()
        {
            // This is a simplified example - normally you'd use XmlDocument or load from file
            return $@"
                <Application>
                    <forms>
                        <action type='add'>
                            <form uid='frmContract_{DateTime.Now.Ticks}' type='frmContract'
                                  width='800' height='600' title='Contract'
                                  left='100' top='100' client_width='800' client_height='600'>
                                <datasources>
                                    <dbdatasources>
                                        <action type='add'>
                                            <datasource type='db' tablename='@CM_CONTRACTS' />
                                        </action>
                                    </dbdatasources>
                                </datasources>
                                <items>
                                    <!-- Form items will be created here -->
                                </items>
                            </form>
                        </action>
                    </forms>
                </Application>";
        }

        private void GetFormItems()
        {
            // Get references to form controls
            _txtContractCode = (EditText)_form.Items.Item("txtCode").Specific;
            _txtContractName = (EditText)_form.Items.Item("txtName").Specific;
            _txtCustomerCode = (EditText)_form.Items.Item("txtCustCode").Specific;
            _txtCustomerName = (EditText)_form.Items.Item("txtCustName").Specific;
            _txtContactPerson = (EditText)_form.Items.Item("txtContact").Specific;
            _txtEmail = (EditText)_form.Items.Item("txtEmail").Specific;
            _txtPhone = (EditText)_form.Items.Item("txtPhone").Specific;
            _txtStartDate = (EditText)_form.Items.Item("txtStart").Specific;
            _txtEndDate = (EditText)_form.Items.Item("txtEnd").Specific;
            _cboContractType = (ComboBox)_form.Items.Item("cboType").Specific;
            _cboCurrency = (ComboBox)_form.Items.Item("cboCurr").Specific;
            _txtExchangeRate = (EditText)_form.Items.Item("txtRate").Specific;
            _txtTotalValue = (EditText)_form.Items.Item("txtTotal").Specific;
            _btnSave = (Button)_form.Items.Item("btnSave").Specific;
            _btnCancel = (Button)_form.Items.Item("btnCancel").Specific;
            _btnBrowseCustomer = (Button)_form.Items.Item("btnBrowse").Specific;
            _btnLanguage = (Button)_form.Items.Item("btnLang").Specific;
            _lblStatus = (StaticText)_form.Items.Item("lblStatus").Specific;
            _mtxLines = (Matrix)_form.Items.Item("mtxLines").Specific;
        }

        #endregion

        #region Localization

        private void LocalizeForm()
        {
            // Set form title
            _form.Title = _lang.GetString("Form_Contract_Title");

            // Localize labels
            LocalizeLabels();

            // Localize buttons
            LocalizeButtons();

            // Localize combo boxes
            LocalizeComboBoxes();

            // Localize matrix columns
            LocalizeMatrixColumns();

            // Localize tooltips
            LocalizeTooltips();
        }

        private void LocalizeLabels()
        {
            // General Information section
            SetLabelText("lblGenInfo", "Section_GeneralInformation");
            SetLabelText("lblCode", "Contract_Code");
            SetLabelText("lblName", "Contract_Name");
            SetLabelText("lblCustomer", "Contract_Customer");
            SetLabelText("lblContact", "Contract_ContactPerson");
            SetLabelText("lblEmail", "Contract_Email");
            SetLabelText("lblPhone", "Contract_Phone");

            // Dates section
            SetLabelText("lblDates", "Section_Dates");
            SetLabelText("lblStart", "Contract_StartDate");
            SetLabelText("lblEnd", "Contract_EndDate");

            // Financial section
            SetLabelText("lblFinancial", "Section_FinancialInformation");
            SetLabelText("lblType", "Contract_Type");
            SetLabelText("lblCurr", "Contract_Currency");
            SetLabelText("lblRate", "Contract_ExchangeRate");
            SetLabelText("lblTotal", "Contract_TotalValue");

            // Contract Lines section
            SetLabelText("lblLines", "Section_ContractLines");

            // Status
            SetLabelText("lblStatus", "Contract_Status");
        }

        private void LocalizeButtons()
        {
            _btnSave.Caption = _lang.GetString("Common_Save");
            _btnCancel.Caption = _lang.GetString("Common_Cancel");
            _btnBrowseCustomer.Caption = _lang.GetString("Common_Browse");

            // Language toggle button shows other language
            _btnLanguage.Caption = _lang.CurrentLanguage == SupportedLanguage.English
                ? "عربي"
                : "EN";
        }

        private void LocalizeComboBoxes()
        {
            // Contract Type
            LocalizeContractTypeCombo();

            // Currency combo is populated from database
            // Just ensure display names are localized if needed
        }

        private void LocalizeContractTypeCombo()
        {
            // Clear existing values
            while (_cboContractType.ValidValues.Count > 0)
            {
                _cboContractType.ValidValues.Remove(0, BoSearchKey.psk_Index);
            }

            // Add localized values
            _cboContractType.ValidValues.Add("FP", _lang.GetString("Contract_Type_FixedPrice"));
            _cboContractType.ValidValues.Add("TM", _lang.GetString("Contract_Type_TimeAndMaterials"));
            _cboContractType.ValidValues.Add("CP", _lang.GetString("Contract_Type_CostPlus"));

            // Set default
            if (string.IsNullOrEmpty(_cboContractType.Value))
            {
                _cboContractType.Select("FP", BoSearchKey.psk_ByValue);
            }
        }

        private void LocalizeMatrixColumns()
        {
            // Line number
            ((Column)_mtxLines.Columns.Item("#")).TitleObject.Caption = "#";

            // Item code
            ((Column)_mtxLines.Columns.Item("ColItem")).TitleObject.Caption = _lang.GetString("ContractLine_ItemCode");

            // Description
            ((Column)_mtxLines.Columns.Item("ColDesc")).TitleObject.Caption = _lang.GetString("ContractLine_Description");

            // Quantity
            ((Column)_mtxLines.Columns.Item("ColQty")).TitleObject.Caption = _lang.GetString("ContractLine_Quantity");

            // Unit price
            ((Column)_mtxLines.Columns.Item("ColPrice")).TitleObject.Caption = _lang.GetString("ContractLine_UnitPrice");

            // Line total
            ((Column)_mtxLines.Columns.Item("ColTotal")).TitleObject.Caption = _lang.GetString("ContractLine_LineTotal");

            // If RTL, right-align numeric columns
            if (_isRTL)
            {
                ((Column)_mtxLines.Columns.Item("ColQty")).RightJustified = true;
                ((Column)_mtxLines.Columns.Item("ColPrice")).RightJustified = true;
                ((Column)_mtxLines.Columns.Item("ColTotal")).RightJustified = true;
            }
        }

        private void LocalizeTooltips()
        {
            // Set tooltips for buttons (if supported by SAP B1 version)
            try
            {
                _form.Items.Item("btnSave").AffectsFormMode = true;
                // Tooltips might not be directly supported, but we can use status bar
            }
            catch
            {
                // Tooltip not supported in this SAP version
            }
        }

        private void SetLabelText(string itemId, string resourceKey)
        {
            try
            {
                var item = _form.Items.Item(itemId);
                if (item.Type == BoFormItemTypes.it_STATIC)
                {
                    var label = (StaticText)item.Specific;
                    label.Caption = _lang.GetString(resourceKey);
                }
            }
            catch
            {
                // Item not found or not a label
            }
        }

        #endregion

        #region RTL Support

        private void ApplyRTLLayout()
        {
            try
            {
                // Use LanguageManager's built-in RTL support
                _lang.LocalizeForm(_form);

                // Additional custom RTL adjustments
                ApplyCustomRTL();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying RTL: {ex.Message}");
            }
        }

        private void ApplyCustomRTL()
        {
            var formWidth = _form.Width;

            // Mirror button positions
            MirrorButton("btnSave", formWidth);
            MirrorButton("btnCancel", formWidth);

            // Adjust browse button to be on left of text box in RTL
            var txtCustomer = _form.Items.Item("txtCustCode");
            var btnBrowse = _form.Items.Item("btnBrowse");

            btnBrowse.Left = txtCustomer.Left - btnBrowse.Width - 2;
        }

        private void MirrorButton(string itemId, int formWidth)
        {
            try
            {
                var item = _form.Items.Item(itemId);
                var newLeft = formWidth - item.Left - item.Width;
                item.Left = newLeft;
            }
            catch
            {
                // Item not found
            }
        }

        #endregion

        #region Data Display with Localization

        /// <summary>
        /// Display contract data with proper localization
        /// </summary>
        public void DisplayContract(Contract contract)
        {
            // Contract code and name - no formatting needed
            _txtContractCode.Value = contract.Code;
            _txtContractName.Value = contract.Description; // Use Description instead of Name

            // Customer
            _txtCustomerCode.Value = contract.CustomerCode;
            _txtCustomerName.Value = contract.CustomerName;
            // Note: ContactPerson, Email, Phone are not in Contract model
            // These would need to be fetched from the Customer record if needed
            //_txtContactPerson.Value = contract.ContactPerson;
            //_txtEmail.Value = contract.Email;
            //_txtPhone.Value = contract.Phone;

            // Dates - use localized format
            _txtStartDate.Value = _lang.FormatDate(contract.StartDate, "short");
            _txtEndDate.Value = _lang.FormatDate(contract.EndDate, "short");

            // Contract type - Status is used instead of ContractType
            _cboContractType.Select(contract.Status, BoSearchKey.psk_ByValue);

            // Currency
            _cboCurrency.Select(contract.Currency, BoSearchKey.psk_ByValue);

            // Exchange rate - use localized number format
            _txtExchangeRate.Value = _lang.FormatNumber(contract.ExchangeRate, 6);

            // Total value - use localized currency format
            _txtTotalValue.Value = _lang.FormatCurrency(contract.TotalValue, contract.Currency);

            // Status - use localized status text
            var statusKey = $"Status_{contract.Status}";
            _lblStatus.Caption = _lang.GetString(statusKey, contract.Status.ToString());

            // Load contract lines
            LoadContractLines(contract.Lines);
        }

        private void LoadContractLines(List<ContractLine> lines)
        {
            _mtxLines.Clear();

            if (lines == null || lines.Count == 0)
                return;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                // Add row
                if (i > 0)
                    _mtxLines.AddRow();

                var row = i + 1;

                // Item code
                ((EditText)_mtxLines.Columns.Item("ColItem").Cells.Item(row).Specific).Value = line.ItemCode;

                // Description
                ((EditText)_mtxLines.Columns.Item("ColDesc").Cells.Item(row).Specific).Value = line.ItemDescription;

                // Quantity - localized number
                ((EditText)_mtxLines.Columns.Item("ColQty").Cells.Item(row).Specific).Value =
                    _lang.FormatNumber(line.Quantity, 2);

                // Unit price - localized currency
                ((EditText)_mtxLines.Columns.Item("ColPrice").Cells.Item(row).Specific).Value =
                    _lang.FormatNumber(line.UnitPrice, 2);

                // Line total - localized currency
                ((EditText)_mtxLines.Columns.Item("ColTotal").Cells.Item(row).Specific).Value =
                    _lang.FormatCurrency(line.LineTotal);
            }

            _mtxLines.LoadFromDataSource();
        }

        #endregion

        #region Event Handlers

        private void WireUpEvents()
        {
            // NOTE: SAP B1 UI API doesn't support direct event handlers on items/forms
            // Events should be handled through Application.ItemEvent in EventManager
            // This is a demonstration/placeholder for event handling logic

            // TODO: Implement event handling through EventManager.HandleItemEvent
            // Example pattern:
            // - Register form type in EventManager
            // - Handle BoEventTypes.et_ITEM_PRESSED for button clicks
            // - Handle BoEventTypes.et_FORM_CLOSE for form close
        }

        // Event handler methods kept for reference but should be called from EventManager
        private void HandleSaveClick()
        {
            try
            {
                // Validate
                if (!ValidateForm())
                    return;

                // Save contract
                SaveContract();

                // Show success message
                var msg = _lang.GetString("Msg_SaveSuccess");
                _application.StatusBar.SetText(msg, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                ShowError(_lang.GetString("Msg_SaveError", ex.Message), ex);
            }
        }

        private void HandleCancelClick()
        {
            _form.Close();
        }

        private void HandleBrowseCustomerClick()
        {
            // Open customer browse window
            // This would typically open a SAP B1 choose-from-list
        }

        private void HandleLanguageClick()
        {
            // Toggle language
            _lang.SwitchLanguage();

            // Note: Form will be reloaded via OnLanguageChanged event
        }

        private void HandleFormClose()
        {
            // Cleanup
            _lang.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            // Language changed - reload form
            _isRTL = _lang.IsRightToLeft;

            // Save current data
            var currentData = GetFormData();

            // Close current form
            _form.Close();

            // Recreate form with new language
            Show();

            // Restore data
            SetFormData(currentData);

            // Show message
            var msg = _lang.GetString("Msg_LanguageChanged",
                _lang.GetString($"Language_{_lang.CurrentLanguage}"));
            _application.MessageBox(msg);
        }

        #endregion

        #region Validation with Localized Messages

        private bool ValidateForm()
        {
            var errors = new List<string>();

            // Validate contract name
            if (string.IsNullOrWhiteSpace(_txtContractName.Value))
            {
                errors.Add(_lang.GetString("Val_ContractNameRequired"));
            }

            // Validate customer
            if (string.IsNullOrWhiteSpace(_txtCustomerCode.Value))
            {
                errors.Add(_lang.GetString("Val_CustomerRequired"));
            }

            // Validate dates
            if (string.IsNullOrWhiteSpace(_txtStartDate.Value))
            {
                errors.Add(_lang.GetString("Val_StartDateRequired"));
            }

            if (string.IsNullOrWhiteSpace(_txtEndDate.Value))
            {
                errors.Add(_lang.GetString("Val_EndDateRequired"));
            }

            // Validate end date >= start date
            if (!string.IsNullOrWhiteSpace(_txtStartDate.Value) &&
                !string.IsNullOrWhiteSpace(_txtEndDate.Value))
            {
                DateTime startDate, endDate;
                if (DateTime.TryParse(_txtStartDate.Value, out startDate) &&
                    DateTime.TryParse(_txtEndDate.Value, out endDate))
                {
                    if (endDate < startDate)
                    {
                        errors.Add(_lang.GetString("Msg_EndDateBeforeStart"));
                    }
                }
            }

            // Validate currency
            if (string.IsNullOrWhiteSpace(_cboCurrency.Value))
            {
                errors.Add(_lang.GetString("Val_CurrencyRequired"));
            }

            // Validate at least one line
            if (_mtxLines.RowCount == 0)
            {
                errors.Add(_lang.GetString("Val_MinOneLine"));
            }

            // If errors, show message
            if (errors.Count > 0)
            {
                var errorMsg = string.Join("\n", errors);
                _application.MessageBox(errorMsg);
                return false;
            }

            return true;
        }

        #endregion

        #region Helper Methods

        private void SetDefaultValues()
        {
            // Set current date
            _txtStartDate.Value = _lang.FormatDate(DateTime.Today, "short");
            _txtEndDate.Value = _lang.FormatDate(DateTime.Today.AddYears(1), "short");

            // Set default contract type
            _cboContractType.Select("FP", BoSearchKey.psk_ByValue);

            // Set default currency (system currency)
            _cboCurrency.Select(0, BoSearchKey.psk_Index);

            // Set exchange rate to 1.0
            _txtExchangeRate.Value = _lang.FormatNumber(1.0, 4);
        }

        private void SaveContract()
        {
            // Implementation of save logic
            // Would typically save to SAP B1 database using DI API
        }

        private object GetFormData()
        {
            // Return current form data as object
            return new
            {
                ContractName = _txtContractName.Value,
                CustomerCode = _txtCustomerCode.Value
                // ... other fields
            };
        }

        private void SetFormData(object data)
        {
            // Restore form data
            // Implementation depends on data structure
        }

        private void ShowError(string message, Exception ex = null)
        {
            var fullMessage = ex != null ? $"{message}\n{ex.Message}" : message;
            _application.MessageBox(fullMessage);

            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex}");
            }
        }

        #endregion
    }
}
