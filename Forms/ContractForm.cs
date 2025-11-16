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
    /// Comprehensive contract management form with full localization
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

        // Control IDs (max 10 chars for SAP B1)
        // Buttons
        private const string BTN_NEW = "btnNew";
        private const string BTN_SAVE = "btnSave";
        private const string BTN_DELETE = "btnDelete";
        private const string BTN_FIND = "btnFind";
        private const string BTN_BROWSE = "btnBrowse";

        // General Information Section
        private const string LBL_GEN = "lblGen";
        private const string LBL_CODE = "lblCode";
        private const string TXT_CODE = "txtCode";
        private const string LBL_NAME = "lblName";
        private const string TXT_NAME = "txtName";

        // Customer Section
        private const string LBL_CUST = "lblCust";
        private const string LBL_CUSTCD = "lblCustCd";
        private const string TXT_CUSTCD = "txtCustCd";
        private const string LBL_CUSTNM = "lblCustNm";
        private const string TXT_CUSTNM = "txtCustNm";

        // Contact Information Section
        private const string LBL_CONTACT = "lblContct";
        private const string LBL_PERSON = "lblPerson";
        private const string TXT_PERSON = "txtPerson";
        private const string LBL_EMAIL = "lblEmail";
        private const string TXT_EMAIL = "txtEmail";
        private const string LBL_PHONE = "lblPhone";
        private const string TXT_PHONE = "txtPhone";

        // Dates Section
        private const string LBL_DATES = "lblDates";
        private const string LBL_START = "lblStart";
        private const string TXT_START = "txtStart";
        private const string LBL_END = "lblEnd";
        private const string TXT_END = "txtEnd";

        // Financial Information Section
        private const string LBL_FINAN = "lblFinan";
        private const string LBL_TYPE = "lblType";
        private const string CBO_TYPE = "cboType";
        private const string LBL_CURR = "lblCurr";
        private const string CBO_CURR = "cboCurr";
        private const string LBL_RATE = "lblRate";
        private const string TXT_RATE = "txtRate";
        private const string LBL_TOTAL = "lblTotal";
        private const string TXT_TOTAL = "txtTotal";
        private const string LBL_STATUS = "lblStatus";
        private const string CBO_STATUS = "cboStatus";
        private const string LBL_RETEN = "lblReten";
        private const string TXT_RETEN = "txtReten";

        // Contract Lines Section
        private const string LBL_LINES = "lblLines";
        private const string MTX_LINES = "mtxLines";

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
                PopulateComboBoxes();
                AttachEvents();
                LoadNewContract();

                _form.Visible = true;
                Logger.Info("Contract form opened");

                _app.UIApp.StatusBar.SetText("Contract Management Form",
                    BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing Contract form", ex);
                _app.UIApp.StatusBar.SetText($"Error opening contract form: {ex.Message}",
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
            _form.Width = 900;
            _form.Height = 750;
            _form.Left = 100;
            _form.Top = 50;
        }

        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                int leftMargin = 20;
                int topPosition = 15;
                int labelWidth = 120;
                int textWidth = 250;
                int rowHeight = 25;
                int sectionGap = 30;

                // ===== ACTION BUTTONS (TOP) =====
                AddButton(BTN_NEW, "", 20, 10, 80, 19);
                AddButton(BTN_SAVE, "", 110, 10, 80, 19);
                AddButton(BTN_DELETE, "", 200, 10, 80, 19);
                AddButton(BTN_FIND, "", 290, 10, 80, 19);

                topPosition = 45; // Start below buttons

                // ===== GENERAL INFORMATION SECTION =====
                AddSectionLabel(LBL_GEN, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Contract Code
                AddLabel(LBL_CODE, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CODE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Contract Name
                AddLabel(LBL_NAME, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_NAME, leftMargin + labelWidth + 5, topPosition, 400, 14);
                topPosition += sectionGap;

                // ===== CUSTOMER INFORMATION SECTION =====
                AddSectionLabel(LBL_CUST, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Customer Code
                AddLabel(LBL_CUSTCD, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTCD, leftMargin + labelWidth + 5, topPosition, 150, 14);
                AddButton(BTN_BROWSE, "...", leftMargin + labelWidth + 160, topPosition, 40, 14);
                topPosition += rowHeight;

                // Customer Name
                AddLabel(LBL_CUSTNM, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTNM, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                ((EditText)_form.Items.Item(TXT_CUSTNM).Specific).Active = false; // Read-only
                topPosition += sectionGap;

                // ===== CONTACT INFORMATION SECTION =====
                AddSectionLabel(LBL_CONTACT, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Contact Person
                AddLabel(LBL_PERSON, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_PERSON, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Email
                AddLabel(LBL_EMAIL, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_EMAIL, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Phone
                AddLabel(LBL_PHONE, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_PHONE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== DATES SECTION =====
                AddSectionLabel(LBL_DATES, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Start Date
                AddLabel(LBL_START, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_START, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // End Date
                AddLabel(LBL_END, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_END, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== FINANCIAL INFORMATION SECTION =====
                AddSectionLabel(LBL_FINAN, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Contract Type
                AddLabel(LBL_TYPE, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_TYPE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Currency
                AddLabel(LBL_CURR, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_CURR, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Exchange Rate
                AddLabel(LBL_RATE, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_RATE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Total Value
                AddLabel(LBL_TOTAL, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_TOTAL, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                ((EditText)_form.Items.Item(TXT_TOTAL).Specific).Active = false; // Read-only
                topPosition += rowHeight;

                // Retention %
                AddLabel(LBL_RETEN, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_RETEN, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Status
                AddLabel(LBL_STATUS, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_STATUS, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== CONTRACT LINES SECTION =====
                AddSectionLabel(LBL_LINES, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Matrix for contract lines
                AddMatrix(MTX_LINES, leftMargin, topPosition, 850, 180);
                InitializeMatrix();

                Logger.Info("Contract form controls initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing Contract form controls", ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize matrix columns
        /// </summary>
        private void InitializeMatrix()
        {
            try
            {
                Matrix matrix = (Matrix)_form.Items.Item(MTX_LINES).Specific;

                // Create columns
                matrix.Columns.Add("ColItem", BoFormItemTypes.it_EDIT);
                matrix.Columns.Add("ColDesc", BoFormItemTypes.it_EDIT);
                matrix.Columns.Add("ColQty", BoFormItemTypes.it_EDIT);
                matrix.Columns.Add("ColPrice", BoFormItemTypes.it_EDIT);
                matrix.Columns.Add("ColTotal", BoFormItemTypes.it_EDIT);

                // Set column properties
                Column colItem = (Column)matrix.Columns.Item("ColItem");
                colItem.Width = 120;
                colItem.Editable = true;

                Column colDesc = (Column)matrix.Columns.Item("ColDesc");
                colDesc.Width = 350;
                colDesc.Editable = true;

                Column colQty = (Column)matrix.Columns.Item("ColQty");
                colQty.Width = 100;
                colQty.Editable = true;
                colQty.RightJustified = true;

                Column colPrice = (Column)matrix.Columns.Item("ColPrice");
                colPrice.Width = 120;
                colPrice.Editable = true;
                colPrice.RightJustified = true;

                Column colTotal = (Column)matrix.Columns.Item("ColTotal");
                colTotal.Width = 120;
                colTotal.Editable = false;
                colTotal.RightJustified = true;

                // Add one empty row
                matrix.AddRow();
                matrix.ClearRowData(1);

                Logger.Info("Contract matrix initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing matrix", ex);
                throw;
            }
        }

        /// <summary>
        /// Populate combo boxes with values
        /// </summary>
        private void PopulateComboBoxes()
        {
            try
            {
                // Contract Type
                ComboBox cboType = (ComboBox)_form.Items.Item(CBO_TYPE).Specific;
                cboType.ValidValues.Add("FP", _lang.GetString("Contract_Type_FixedPrice"));
                cboType.ValidValues.Add("TM", _lang.GetString("Contract_Type_TimeAndMaterials"));
                cboType.ValidValues.Add("CP", _lang.GetString("Contract_Type_CostPlus"));
                if (cboType.ValidValues.Count > 0)
                    cboType.Select(0, BoSearchKey.psk_Index);

                // Status
                ComboBox cboStatus = (ComboBox)_form.Items.Item(CBO_STATUS).Specific;
                cboStatus.ValidValues.Add("Draft", _lang.GetString("Status_Draft"));
                cboStatus.ValidValues.Add("Active", _lang.GetString("Status_Active"));
                cboStatus.ValidValues.Add("OnHold", _lang.GetString("Status_OnHold"));
                cboStatus.ValidValues.Add("Completed", _lang.GetString("Status_Completed"));
                cboStatus.ValidValues.Add("Cancelled", _lang.GetString("Status_Cancelled"));
                if (cboStatus.ValidValues.Count > 0)
                    cboStatus.Select(0, BoSearchKey.psk_Index);

                // Currency - populate from system
                ComboBox cboCurr = (ComboBox)_form.Items.Item(CBO_CURR).Specific;
                var recordset = (SAPbobsCOM.Recordset)_app.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                recordset.DoQuery("SELECT CurrCode, CurrName FROM OCRN WHERE CurrCode IS NOT NULL ORDER BY CurrCode");

                while (!recordset.EoF)
                {
                    string code = recordset.Fields.Item("CurrCode").Value.ToString();
                    string name = recordset.Fields.Item("CurrName").Value.ToString();
                    cboCurr.ValidValues.Add(code, $"{code} - {name}");
                    recordset.MoveNext();
                }

                if (cboCurr.ValidValues.Count > 0)
                    cboCurr.Select(0, BoSearchKey.psk_Index);

                // Set default exchange rate
                ((EditText)_form.Items.Item(TXT_RATE).Specific).Value = "1.0";

                Logger.Info("Combo boxes populated successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error populating combo boxes", ex);
            }
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

                // Action Buttons
                ((Button)_form.Items.Item(BTN_NEW).Specific).Caption = _lang.GetString("Common_New");
                ((Button)_form.Items.Item(BTN_SAVE).Specific).Caption = _lang.GetString("Common_Save");
                ((Button)_form.Items.Item(BTN_DELETE).Specific).Caption = _lang.GetString("Common_Delete");
                ((Button)_form.Items.Item(BTN_FIND).Specific).Caption = _lang.GetString("Common_Find");
                ((Button)_form.Items.Item(BTN_BROWSE).Specific).Caption = "...";

                // Section labels
                ((StaticText)_form.Items.Item(LBL_GEN).Specific).Caption = _lang.GetString("Section_GeneralInformation");
                ((StaticText)_form.Items.Item(LBL_CUST).Specific).Caption = _lang.GetString("Contract_Customer");
                ((StaticText)_form.Items.Item(LBL_CONTACT).Specific).Caption = _lang.GetString("Section_ContactInformation");
                ((StaticText)_form.Items.Item(LBL_DATES).Specific).Caption = _lang.GetString("Section_Dates");
                ((StaticText)_form.Items.Item(LBL_FINAN).Specific).Caption = _lang.GetString("Section_FinancialInformation");
                ((StaticText)_form.Items.Item(LBL_LINES).Specific).Caption = _lang.GetString("Section_ContractLines");

                // General Information
                ((StaticText)_form.Items.Item(LBL_CODE).Specific).Caption = _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item(LBL_NAME).Specific).Caption = _lang.GetString("Contract_Name");

                // Customer Information
                ((StaticText)_form.Items.Item(LBL_CUSTCD).Specific).Caption = _lang.GetString("Contract_CustomerCode");
                ((StaticText)_form.Items.Item(LBL_CUSTNM).Specific).Caption = _lang.GetString("Contract_CustomerName");

                // Contact Information
                ((StaticText)_form.Items.Item(LBL_PERSON).Specific).Caption = _lang.GetString("Contract_ContactPerson");
                ((StaticText)_form.Items.Item(LBL_EMAIL).Specific).Caption = _lang.GetString("Contract_Email");
                ((StaticText)_form.Items.Item(LBL_PHONE).Specific).Caption = _lang.GetString("Contract_Phone");

                // Dates
                ((StaticText)_form.Items.Item(LBL_START).Specific).Caption = _lang.GetString("Contract_StartDate");
                ((StaticText)_form.Items.Item(LBL_END).Specific).Caption = _lang.GetString("Contract_EndDate");

                // Financial Information
                ((StaticText)_form.Items.Item(LBL_TYPE).Specific).Caption = _lang.GetString("Contract_Type");
                ((StaticText)_form.Items.Item(LBL_CURR).Specific).Caption = _lang.GetString("Contract_Currency");
                ((StaticText)_form.Items.Item(LBL_RATE).Specific).Caption = _lang.GetString("Contract_ExchangeRate");
                ((StaticText)_form.Items.Item(LBL_TOTAL).Specific).Caption = _lang.GetString("Contract_TotalValue");
                ((StaticText)_form.Items.Item(LBL_RETEN).Specific).Caption = _lang.GetString("Contract_RetentionPercentage");
                ((StaticText)_form.Items.Item(LBL_STATUS).Specific).Caption = _lang.GetString("Contract_Status");

                // Matrix columns
                LocalizeMatrixColumns();

                // Apply RTL if needed
                if (_isRTL)
                {
                    _lang.LocalizeForm(_form);
                }

                Logger.Info("Contract form localized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing Contract form", ex);
            }
        }

        /// <summary>
        /// Localize matrix columns
        /// </summary>
        private void LocalizeMatrixColumns()
        {
            try
            {
                Matrix matrix = (Matrix)_form.Items.Item(MTX_LINES).Specific;

                ((Column)matrix.Columns.Item("ColItem")).TitleObject.Caption = _lang.GetString("ContractLine_ItemCode");
                ((Column)matrix.Columns.Item("ColDesc")).TitleObject.Caption = _lang.GetString("ContractLine_Description");
                ((Column)matrix.Columns.Item("ColQty")).TitleObject.Caption = _lang.GetString("ContractLine_Quantity");
                ((Column)matrix.Columns.Item("ColPrice")).TitleObject.Caption = _lang.GetString("ContractLine_UnitPrice");
                ((Column)matrix.Columns.Item("ColTotal")).TitleObject.Caption = _lang.GetString("ContractLine_LineTotal");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing matrix columns", ex);
            }
        }

        /// <summary>
        /// Attach event handlers
        /// </summary>
        private void AttachEvents()
        {
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

                if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
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

                        case BTN_BROWSE:
                            BrowseCustomer();
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
        /// Browse for customer
        /// </summary>
        private void BrowseCustomer()
        {
            try
            {
                // In production, implement choose-from-list for customers
                _app.UIApp.StatusBar.SetText("Customer browse - to be implemented",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception ex)
            {
                Logger.Error("Error browsing customer", ex);
            }
        }

        /// <summary>
        /// Load new contract
        /// </summary>
        private void LoadNewContract()
        {
            _currentContract = new Contract();
            _currentContract.Code = GetNextContractCode();
            _currentContract.StartDate = DateTime.Today;
            _currentContract.EndDate = DateTime.Today.AddYears(1);
            _currentContract.ExchangeRate = 1.0;
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
                ((EditText)_form.Items.Item(TXT_NAME).Specific).Value = contract.Description ?? "";
                ((EditText)_form.Items.Item(TXT_CUSTCD).Specific).Value = contract.CustomerCode ?? "";
                ((EditText)_form.Items.Item(TXT_CUSTNM).Specific).Value = contract.CustomerName ?? "";

                // Contact fields - would come from customer record
                ((EditText)_form.Items.Item(TXT_PERSON).Specific).Value = "";
                ((EditText)_form.Items.Item(TXT_EMAIL).Specific).Value = "";
                ((EditText)_form.Items.Item(TXT_PHONE).Specific).Value = "";

                ((EditText)_form.Items.Item(TXT_START).Specific).Value = contract.StartDate.ToString("yyyyMMdd");
                ((EditText)_form.Items.Item(TXT_END).Specific).Value = contract.EndDate.ToString("yyyyMMdd");
                ((EditText)_form.Items.Item(TXT_RATE).Specific).Value = contract.ExchangeRate.ToString();
                ((EditText)_form.Items.Item(TXT_TOTAL).Specific).Value = contract.TotalValue.ToString();
                ((EditText)_form.Items.Item(TXT_RETEN).Specific).Value = contract.RetentionPercentage.ToString();

                // Select status
                ComboBox statusCombo = (ComboBox)_form.Items.Item(CBO_STATUS).Specific;
                try
                {
                    statusCombo.Select(contract.Status, BoSearchKey.psk_ByValue);
                }
                catch
                {
                    if (statusCombo.ValidValues.Count > 0)
                        statusCombo.Select(0, BoSearchKey.psk_Index);
                }

                LoadLinesToMatrix(contract.Lines);
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading contract to form", ex);
                throw;
            }
        }

        /// <summary>
        /// Load lines to matrix
        /// </summary>
        private void LoadLinesToMatrix(System.Collections.Generic.List<ContractLine> lines)
        {
            try
            {
                Matrix matrix = (Matrix)_form.Items.Item(MTX_LINES).Specific;
                matrix.Clear();

                if (lines == null || lines.Count == 0)
                {
                    matrix.AddRow();
                    matrix.ClearRowData(1);
                    return;
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    matrix.AddRow();
                    int row = i + 1;

                    ((EditText)matrix.Columns.Item("ColItem").Cells.Item(row).Specific).Value = line.ItemCode ?? "";
                    ((EditText)matrix.Columns.Item("ColDesc").Cells.Item(row).Specific).Value = line.ItemDescription ?? "";
                    ((EditText)matrix.Columns.Item("ColQty").Cells.Item(row).Specific).Value = line.Quantity.ToString();
                    ((EditText)matrix.Columns.Item("ColPrice").Cells.Item(row).Specific).Value = line.UnitPrice.ToString();
                    ((EditText)matrix.Columns.Item("ColTotal").Cells.Item(row).Specific).Value = line.LineTotal.ToString();
                }

                matrix.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading lines to matrix", ex);
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
                Description = ((EditText)_form.Items.Item(TXT_NAME).Specific).Value,
                CustomerCode = ((EditText)_form.Items.Item(TXT_CUSTCD).Specific).Value,
                CustomerName = ((EditText)_form.Items.Item(TXT_CUSTNM).Specific).Value,
                StartDate = DateTime.ParseExact(((EditText)_form.Items.Item(TXT_START).Specific).Value, "yyyyMMdd", null),
                EndDate = DateTime.ParseExact(((EditText)_form.Items.Item(TXT_END).Specific).Value, "yyyyMMdd", null),
                ExchangeRate = double.Parse(((EditText)_form.Items.Item(TXT_RATE).Specific).Value),
                TotalValue = double.Parse(((EditText)_form.Items.Item(TXT_TOTAL).Specific).Value),
                RetentionPercentage = double.Parse(((EditText)_form.Items.Item(TXT_RETEN).Specific).Value),
                Status = ((ComboBox)_form.Items.Item(CBO_STATUS).Specific).Selected.Value,
                Currency = ((ComboBox)_form.Items.Item(CBO_CURR).Specific).Selected.Value
            };

            // Get lines from matrix (simplified - in production, implement full grid data retrieval)
            // TODO: Implement matrix data retrieval

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

        /// <summary>
        /// Handle language change event
        /// </summary>
        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            try
            {
                _isRTL = _lang.IsRightToLeft;
                LocalizeForm();
                PopulateComboBoxes(); // Refresh combo box values with new language
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling language change", ex);
            }
        }

        // Helper methods for adding controls
        private void AddSectionLabel(string id, string caption, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_STATIC);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
            StaticText txt = (StaticText)item.Specific;
            txt.Caption = caption;
            item.FontSize = 11;
        }

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

        private void AddComboBox(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_COMBO_BOX);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
        }

        private void AddMatrix(string id, int left, int top, int width, int height)
        {
            Item item = _form.Items.Add(id, BoFormItemTypes.it_MATRIX);
            item.Left = left;
            item.Top = top;
            item.Width = width;
            item.Height = height;
        }
    }
}
