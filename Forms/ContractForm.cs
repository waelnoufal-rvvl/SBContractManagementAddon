using System;
using System.Globalization;
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
        private const string CFL_CUST = "CFL_CUST";
        private const string CFL_QREF = "CFL_QREF";

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
        private const string LBL_CUSTT = "lblCusTp";
        private const string CBO_CUSTT = "cboCusTp";

        // Additional Information Section
        private const string LBL_ADD = "lblAdd";
        private const string LBL_PROJCD = "lblPrjCd";
        private const string TXT_PROJCD = "txtPrjCd";
        private const string LBL_PROJNM = "lblPrjNm";
        private const string TXT_PROJNM = "txtPrjNm";
        private const string LBL_SECTOR = "lblSect";
        private const string TXT_SECTOR = "txtSect";
        private const string LBL_USRTYP = "lblUsrTp";
        private const string TXT_USRTYP = "txtUsrTp";
        private const string LBL_UNIT = "lblUnit";
        private const string TXT_UNIT = "txtUnit";
        private const string LBL_REGION = "lblRegn";
        private const string TXT_REGION = "txtRegn";
        private const string LBL_QREF = "lblQRef";
        private const string TXT_QREF = "txtQRef";
        private const string BTN_QREF = "btnQRef";

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
        private const string LBL_DUR = "lblDur";
        private const string TXT_DUR = "txtDur";

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
        private const string LBL_APPR = "lblAppr";
        private const string CBO_APPR = "cboAppr";

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
                int rightMargin = 450;
                int rightTopPosition = 45;
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

                // ===== PRICE QUOTATION REFERENCE (TOP) =====
                AddLabel(LBL_QREF, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_QREF, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                AddButton(BTN_QREF, "...", leftMargin + labelWidth + 5 + textWidth + 5, topPosition, 40, 14);
                topPosition += rowHeight + sectionGap;

                // Create quotation choose-from-list now that control exists
                CreateQuotationChooseFromList();

                // ===== GENERAL INFORMATION SECTION =====
                AddSectionLabel(LBL_GEN, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Contract Code
                AddLabel(LBL_CODE, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CODE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Contract Name (Title)
                AddLabel(LBL_NAME, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_NAME, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== CUSTOMER INFORMATION SECTION =====
                AddSectionLabel(LBL_CUST, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Customer Code
                AddLabel(LBL_CUSTCD, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTCD, leftMargin + labelWidth + 5, topPosition, 150, 14);
                AddButton(BTN_BROWSE, "...", leftMargin + labelWidth + 160, topPosition, 40, 14);
                _form.Items.Item(BTN_BROWSE).Visible = false; // hide unused browse button
                topPosition += rowHeight;

                // Create customer choose-from-list now that control exists
                CreateCustomerChooseFromList();

                // Customer Name
                AddLabel(LBL_CUSTNM, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_CUSTNM, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                _form.Items.Item(TXT_CUSTNM).Enabled = false; // Read-only
                topPosition += rowHeight;

                // Customer Type
                AddLabel(LBL_CUSTT, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_CUSTT, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== ADDITIONAL INFORMATION SECTION (RIGHT COLUMN) =====
                AddSectionLabel(LBL_ADD, "", rightMargin, rightTopPosition, 400, 16);
                rightTopPosition += 22;

                // Project Number
                AddLabel(LBL_PROJCD, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_PROJCD, rightMargin + labelWidth + 5, rightTopPosition, 150, 14);
                rightTopPosition += rowHeight;

                // Project Name
                AddLabel(LBL_PROJNM, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_PROJNM, rightMargin + labelWidth + 5, rightTopPosition, 250, 14);
                rightTopPosition += rowHeight;

                // Sector
                AddLabel(LBL_SECTOR, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_SECTOR, rightMargin + labelWidth + 5, rightTopPosition, 200, 14);
                rightTopPosition += rowHeight;

                // User Type
                AddLabel(LBL_USRTYP, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_USRTYP, rightMargin + labelWidth + 5, rightTopPosition, 200, 14);
                rightTopPosition += rowHeight;

                // Unit Number
                AddLabel(LBL_UNIT, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_UNIT, rightMargin + labelWidth + 5, rightTopPosition, 200, 14);
                rightTopPosition += rowHeight;

                // Region
                AddLabel(LBL_REGION, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_REGION, rightMargin + labelWidth + 5, rightTopPosition, 200, 14);
                rightTopPosition += sectionGap;

                // ===== CONTACT INFORMATION SECTION (LEFT COLUMN) =====
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

                // ===== FINANCIAL INFORMATION SECTION (LEFT COLUMN BASIC) =====
                AddSectionLabel(LBL_FINAN, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Contract Type
                AddLabel(LBL_TYPE, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_TYPE, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Total Value
                AddLabel(LBL_TOTAL, "", leftMargin, topPosition, labelWidth, 14);
                AddTextBox(TXT_TOTAL, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += rowHeight;

                // Status
                AddLabel(LBL_STATUS, "", leftMargin, topPosition, labelWidth, 14);
                AddComboBox(CBO_STATUS, leftMargin + labelWidth + 5, topPosition, textWidth, 14);
                topPosition += sectionGap;

                // ===== DATES SECTION (RIGHT COLUMN) =====
                AddSectionLabel(LBL_DATES, "", rightMargin, rightTopPosition, 400, 16);
                rightTopPosition += 22;

                // Start Date (date picker)
                AddLabel(LBL_START, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_START, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                rightTopPosition += rowHeight;

                // End Date (date picker)
                AddLabel(LBL_END, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_END, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                rightTopPosition += rowHeight;

                // Duration (days)
                AddLabel(LBL_DUR, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_DUR, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                _form.Items.Item(TXT_DUR).Enabled = false;
                rightTopPosition += sectionGap;

                // Remaining financial fields under Duration on right column
                // Currency
                AddLabel(LBL_CURR, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddComboBox(CBO_CURR, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                rightTopPosition += rowHeight;

                // Retention %
                AddLabel(LBL_RETEN, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddTextBox(TXT_RETEN, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                rightTopPosition += rowHeight;

                // Approval Status
                AddLabel(LBL_APPR, "", rightMargin, rightTopPosition, labelWidth, 14);
                AddComboBox(CBO_APPR, rightMargin + labelWidth + 5, rightTopPosition, textWidth, 14);
                rightTopPosition += sectionGap;

                // Align lines section under whichever column is taller
                topPosition = Math.Max(topPosition, rightTopPosition);

                // ===== CONTRACT LINES SECTION =====
                AddSectionLabel(LBL_LINES, "", leftMargin, topPosition, 400, 16);
                topPosition += 22;

                // Matrix for contract lines
                AddMatrix(MTX_LINES, leftMargin, topPosition, 850, 180);
                InitializeMatrix();

                // Initialize date pickers after controls are created
                InitializeDatePickers();

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

                // Create columns as per contract lines table RVCM_CNTRCT1
                // Order: Item Code, Description, Cost Code, Stage, Quantity, Unit, Unit Price,
                // Discount %, Line Total (before tax), Tax Code, Tax Rate, Tax Amount,
                // Line Total Inc Tax, Planned Date, Actual Date, Status, Remarks
                matrix.Columns.Add("ColItem", BoFormItemTypes.it_EDIT);     // Item Code
                matrix.Columns.Add("ColDesc", BoFormItemTypes.it_EDIT);     // Item Description
                matrix.Columns.Add("ColCost", BoFormItemTypes.it_EDIT);     // Cost Code
                matrix.Columns.Add("ColStage", BoFormItemTypes.it_EDIT);    // Stage
                matrix.Columns.Add("ColQty", BoFormItemTypes.it_EDIT);      // Quantity
                matrix.Columns.Add("ColUnit", BoFormItemTypes.it_EDIT);     // Unit
                matrix.Columns.Add("ColPrice", BoFormItemTypes.it_EDIT);    // Unit Price
                matrix.Columns.Add("ColDisc", BoFormItemTypes.it_EDIT);     // Discount %
                matrix.Columns.Add("ColTotal", BoFormItemTypes.it_EDIT);    // Line Total (before tax, calculated)
                matrix.Columns.Add("ColTaxC", BoFormItemTypes.it_EDIT);     // Tax Code
                matrix.Columns.Add("ColTaxR", BoFormItemTypes.it_EDIT);     // Tax Rate
                matrix.Columns.Add("ColTaxA", BoFormItemTypes.it_EDIT);     // Tax Amount
                matrix.Columns.Add("ColTotInc", BoFormItemTypes.it_EDIT);   // Line Total Including Tax
                matrix.Columns.Add("ColPlan", BoFormItemTypes.it_EDIT);     // Planned Date
                matrix.Columns.Add("ColAct", BoFormItemTypes.it_EDIT);      // Actual Date
                matrix.Columns.Add("ColStat", BoFormItemTypes.it_EDIT);     // Status (P/I/C)
                matrix.Columns.Add("ColRem", BoFormItemTypes.it_EDIT);      // Remarks

                // Set column properties (widths, editability and alignment)
                Column colItem = (Column)matrix.Columns.Item("ColItem");
                colItem.Width = 90;
                // Disable editing on Item Code to prevent focus/interaction
                colItem.Editable = false;

                Column colDesc = (Column)matrix.Columns.Item("ColDesc");
                colDesc.Width = 200;
                colDesc.Editable = true;

                Column colCost = (Column)matrix.Columns.Item("ColCost");
                colCost.Width = 90;
                colCost.Editable = true;

                Column colStage = (Column)matrix.Columns.Item("ColStage");
                colStage.Width = 90;
                colStage.Editable = true;

                Column colQty = (Column)matrix.Columns.Item("ColQty");
                colQty.Width = 60;
                colQty.Editable = true;
                colQty.RightJustified = true;

                Column colUnit = (Column)matrix.Columns.Item("ColUnit");
                colUnit.Width = 50;
                colUnit.Editable = true;

                Column colPrice = (Column)matrix.Columns.Item("ColPrice");
                colPrice.Width = 80;
                colPrice.Editable = true;
                colPrice.RightJustified = true;

                Column colDisc = (Column)matrix.Columns.Item("ColDisc");
                colDisc.Width = 60;
                colDisc.Editable = true;
                colDisc.RightJustified = true;

                Column colTotal = (Column)matrix.Columns.Item("ColTotal");
                colTotal.Width = 100;
                colTotal.Editable = false; // calculated
                colTotal.RightJustified = true;

                Column colTaxC = (Column)matrix.Columns.Item("ColTaxC");
                colTaxC.Width = 70;
                colTaxC.Editable = true;

                Column colTaxR = (Column)matrix.Columns.Item("ColTaxR");
                colTaxR.Width = 60;
                colTaxR.Editable = true;
                colTaxR.RightJustified = true;

                Column colTaxA = (Column)matrix.Columns.Item("ColTaxA");
                colTaxA.Width = 90;
                colTaxA.Editable = false; // calculated
                colTaxA.RightJustified = true;

                Column colTotInc = (Column)matrix.Columns.Item("ColTotInc");
                colTotInc.Width = 110;
                colTotInc.Editable = false; // calculated
                colTotInc.RightJustified = true;

                Column colPlan = (Column)matrix.Columns.Item("ColPlan");
                colPlan.Width = 90;
                colPlan.Editable = true;

                Column colAct = (Column)matrix.Columns.Item("ColAct");
                colAct.Width = 90;
                colAct.Editable = true;

                Column colStat = (Column)matrix.Columns.Item("ColStat");
                colStat.Width = 60;
                colStat.Editable = true;

                Column colRem = (Column)matrix.Columns.Item("ColRem");
                colRem.Width = 150;
                colRem.Editable = true;

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
                // Contract Type (store readable text as value)
                ComboBox cboType = (ComboBox)_form.Items.Item(CBO_TYPE).Specific;
                cboType.ValidValues.Add("Unit Price", _lang.GetString("Contract_Type_UnitPrice", "Unit Price"));
                cboType.ValidValues.Add("Fixed Price", _lang.GetString("Contract_Type_FixedPrice"));
                cboType.ValidValues.Add("Time & Materials", _lang.GetString("Contract_Type_TimeAndMaterials"));
                cboType.ValidValues.Add("Cost Plus", _lang.GetString("Contract_Type_CostPlus"));
                if (cboType.ValidValues.Count > 0)
                    cboType.Select("Unit Price", BoSearchKey.psk_ByValue);

                // Status (model uses readable values; repository maps to codes)
                ComboBox cboStatus = (ComboBox)_form.Items.Item(CBO_STATUS).Specific;
                cboStatus.ValidValues.Add("Draft", _lang.GetString("Status_Draft"));
                cboStatus.ValidValues.Add("Active", _lang.GetString("Status_Active"));
                cboStatus.ValidValues.Add("OnHold", _lang.GetString("Status_OnHold"));
                cboStatus.ValidValues.Add("Completed", _lang.GetString("Status_Completed"));
                cboStatus.ValidValues.Add("Cancelled", _lang.GetString("Status_Cancelled"));
                if (cboStatus.ValidValues.Count > 0)
                    cboStatus.Select(0, BoSearchKey.psk_Index);

                // Customer Type (C = Customer, S = Supplier, L = Lead) - UI shows full text
                ComboBox cboCustType = (ComboBox)_form.Items.Item(CBO_CUSTT).Specific;
                cboCustType.ValidValues.Add("Customer",
                    _lang.GetString("CustomerType_Customer", "Customer"));
                cboCustType.ValidValues.Add("Supplier",
                    _lang.GetString("CustomerType_Supplier", "Supplier"));
                cboCustType.ValidValues.Add("Lead",
                    _lang.GetString("CustomerType_Lead", "Lead"));
                if (cboCustType.ValidValues.Count > 0)
                    cboCustType.Select(0, BoSearchKey.psk_Index);

                // Currency - populate from system from SAP B1 currency table (OCRN)
                ComboBox cboCurr = (ComboBox)_form.Items.Item(CBO_CURR).Specific;
                var recordset = (SAPbobsCOM.Recordset)_app.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                try
                {
                    string query;
                    if (_app.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        query =
                            "SELECT \"CurrCode\" AS CurrCode, \"CurrName\" AS CurrName " +
                            "FROM \"OCRN\" WHERE \"CurrCode\" IS NOT NULL ORDER BY \"CurrCode\"";
                    }
                    else
                    {
                        query =
                            "SELECT CurrCode, CurrName FROM OCRN WHERE CurrCode IS NOT NULL ORDER BY CurrCode";
                    }

                    recordset.DoQuery(query);

                    while (!recordset.EoF)
                    {
                        string code = recordset.Fields.Item("CurrCode").Value.ToString();
                        string name = recordset.Fields.Item("CurrName").Value.ToString();
                        // Use readable currency name as both Value and Description (your previously working behavior)
                        cboCurr.ValidValues.Add(name, name);
                        recordset.MoveNext();
                    }

                    if (cboCurr.ValidValues.Count > 0)
                        cboCurr.Select(0, BoSearchKey.psk_Index);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }

                // Approval Status (P = Pending, A = Approved, R = Rejected) - UI shows full text
                ComboBox cboAppr = (ComboBox)_form.Items.Item(CBO_APPR).Specific;
                cboAppr.ValidValues.Add("Pending", _lang.GetString("Status_Pending"));
                cboAppr.ValidValues.Add("Approved", _lang.GetString("Status_Approved"));
                cboAppr.ValidValues.Add("Rejected", _lang.GetString("Status_Rejected"));
                if (cboAppr.ValidValues.Count > 0)
                    cboAppr.Select("Pending", BoSearchKey.psk_ByValue);

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
                ((StaticText)_form.Items.Item(LBL_GEN).Specific).Caption =
                    _lang.GetString("Section_GeneralInformation");
                ((StaticText)_form.Items.Item(LBL_CUST).Specific).Caption =
                    _lang.GetString("Contract_Customer");
                ((StaticText)_form.Items.Item(LBL_ADD).Specific).Caption =
                    _lang.GetString("Section_AdditionalInformation");
                ((StaticText)_form.Items.Item(LBL_CONTACT).Specific).Caption =
                    _lang.GetString("Section_ContactInformation");
                ((StaticText)_form.Items.Item(LBL_DATES).Specific).Caption =
                    _lang.GetString("Section_Dates");
                ((StaticText)_form.Items.Item(LBL_FINAN).Specific).Caption =
                    _lang.GetString("Section_FinancialInformation");
                ((StaticText)_form.Items.Item(LBL_LINES).Specific).Caption =
                    _lang.GetString("Section_ContractLines");

                // General Information
                ((StaticText)_form.Items.Item(LBL_CODE).Specific).Caption =
                    _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item(LBL_NAME).Specific).Caption =
                    _lang.GetString("Contract_Title", "Contract Title");

                // Customer Information
                ((StaticText)_form.Items.Item(LBL_CUSTCD).Specific).Caption =
                    _lang.GetString("Contract_CustomerCode");
                ((StaticText)_form.Items.Item(LBL_CUSTNM).Specific).Caption =
                    _lang.GetString("Contract_CustomerName");
                ((StaticText)_form.Items.Item(LBL_CUSTT).Specific).Caption =
                    _lang.GetString("Contract_CustomerType", "Customer Type");

                // Additional Information
                ((StaticText)_form.Items.Item(LBL_PROJCD).Specific).Caption =
                    _lang.GetString("Contract_ProjectNumber", "Project No.");
                ((StaticText)_form.Items.Item(LBL_PROJNM).Specific).Caption =
                    _lang.GetString("Contract_ProjectName", "Project Name");
                ((StaticText)_form.Items.Item(LBL_SECTOR).Specific).Caption =
                    _lang.GetString("Contract_Sector", "Sector");
                ((StaticText)_form.Items.Item(LBL_USRTYP).Specific).Caption =
                    _lang.GetString("Contract_UserType", "User Type");
                ((StaticText)_form.Items.Item(LBL_UNIT).Specific).Caption =
                    _lang.GetString("Contract_UnitNumber", "Unit Number");
                ((StaticText)_form.Items.Item(LBL_REGION).Specific).Caption =
                    _lang.GetString("Contract_Region", "Region");
                ((StaticText)_form.Items.Item(LBL_QREF).Specific).Caption =
                    _lang.GetString("Contract_PriceQuoteRef", "Price Quote Ref");

                // Contact Information
                ((StaticText)_form.Items.Item(LBL_PERSON).Specific).Caption =
                    _lang.GetString("Contract_ContactPerson");
                ((StaticText)_form.Items.Item(LBL_EMAIL).Specific).Caption = _lang.GetString("Contract_Email");
                ((StaticText)_form.Items.Item(LBL_PHONE).Specific).Caption = _lang.GetString("Contract_Phone");

                // Dates
                ((StaticText)_form.Items.Item(LBL_START).Specific).Caption =
                    _lang.GetString("Contract_StartDate");
                ((StaticText)_form.Items.Item(LBL_END).Specific).Caption =
                    _lang.GetString("Contract_EndDate");
                ((StaticText)_form.Items.Item(LBL_DUR).Specific).Caption =
                    _lang.GetString("Contract_DurationDays", "Duration (days)");

                // Financial Information
                ((StaticText)_form.Items.Item(LBL_TYPE).Specific).Caption =
                    _lang.GetString("Contract_Type");
                ((StaticText)_form.Items.Item(LBL_CURR).Specific).Caption =
                    _lang.GetString("Contract_Currency");
                ((StaticText)_form.Items.Item(LBL_TOTAL).Specific).Caption =
                    _lang.GetString("Contract_TotalValue");
                ((StaticText)_form.Items.Item(LBL_RETEN).Specific).Caption =
                    _lang.GetString("Contract_RetentionPct", "Retention %");
                ((StaticText)_form.Items.Item(LBL_STATUS).Specific).Caption =
                    _lang.GetString("Contract_Status");
                ((StaticText)_form.Items.Item(LBL_APPR).Specific).Caption =
                    _lang.GetString("Contract_ApprovalStatus", "Approval Status");

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

                ((Column)matrix.Columns.Item("ColItem")).TitleObject.Caption =
                    _lang.GetString("ContractLine_ItemCode");
                ((Column)matrix.Columns.Item("ColDesc")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Description");
                ((Column)matrix.Columns.Item("ColCost")).TitleObject.Caption =
                    _lang.GetString("ContractLine_CostCode", "Cost Code");
                ((Column)matrix.Columns.Item("ColStage")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Stage", "Stage");
                ((Column)matrix.Columns.Item("ColQty")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Quantity");
                ((Column)matrix.Columns.Item("ColUnit")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Unit");
                ((Column)matrix.Columns.Item("ColPrice")).TitleObject.Caption =
                    _lang.GetString("ContractLine_UnitPrice");
                ((Column)matrix.Columns.Item("ColDisc")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Discount");
                ((Column)matrix.Columns.Item("ColTotal")).TitleObject.Caption =
                    _lang.GetString("ContractLine_LineTotal");
                ((Column)matrix.Columns.Item("ColTaxC")).TitleObject.Caption =
                    _lang.GetString("ContractLine_TaxCode", "Tax Code");
                ((Column)matrix.Columns.Item("ColTaxR")).TitleObject.Caption =
                    _lang.GetString("ContractLine_TaxRate", "Tax Rate");
                ((Column)matrix.Columns.Item("ColTaxA")).TitleObject.Caption =
                    _lang.GetString("ContractLine_TaxAmount");
                ((Column)matrix.Columns.Item("ColTotInc")).TitleObject.Caption =
                    _lang.GetString("ContractLine_LineTotalInc", "Line Total Inc");
                ((Column)matrix.Columns.Item("ColPlan")).TitleObject.Caption =
                    _lang.GetString("ContractLine_PlannedDate", "Planned Date");
                ((Column)matrix.Columns.Item("ColAct")).TitleObject.Caption =
                    _lang.GetString("ContractLine_ActualDate", "Actual Date");
                ((Column)matrix.Columns.Item("ColStat")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Status", "Status");
                ((Column)matrix.Columns.Item("ColRem")).TitleObject.Caption =
                    _lang.GetString("ContractLine_Remarks", "Remarks");
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

                  // Disable focus/click events on Item Code column in contract lines matrix
                  if (pVal.ItemUID == MTX_LINES &&
                      pVal.ColUID == "ColItem" &&
                      pVal.BeforeAction &&
                      (pVal.EventType == BoEventTypes.et_CLICK ||
                       pVal.EventType == BoEventTypes.et_MATRIX_LINK_PRESSED ||
                       pVal.EventType == BoEventTypes.et_VALIDATE ||
                       pVal.EventType == BoEventTypes.et_LOST_FOCUS))
                  {
                      bubbleEvent = false;
                      return;
                  }

                  // Handle Choose-From-List selection (e.g., customer code)
                  if (pVal.EventType == BoEventTypes.et_CHOOSE_FROM_LIST && !pVal.BeforeAction)
                  {
                      HandleChooseFromList(ref pVal);
                      return;
                  }
  
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

                          case BTN_QREF:
                              BrowseQuotation();
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
          /// Handle Choose-From-List events (customer selection, etc.).
          /// </summary>
          private void HandleChooseFromList(ref ItemEvent pVal)
          {
              try
              {
                  ChooseFromListEvent cflEvent = (ChooseFromListEvent)pVal;

                  if (cflEvent.SelectedObjects == null)
                      return;

                  DataTable dataTable = cflEvent.SelectedObjects;
                  if (dataTable.Rows.Count == 0)
                      return;

                  if (cflEvent.ChooseFromListUID == CFL_CUST)
                  {
                      HandleCustomerChooseFromList(dataTable);
                  }
                  else if (cflEvent.ChooseFromListUID == CFL_QREF)
                  {
                      HandleQuotationChooseFromList(dataTable);
                  }
              }
              catch (Exception ex)
              {
                  Logger.Error("Error handling ChooseFromList event", ex);
              }
          }

          private void HandleCustomerChooseFromList(DataTable dataTable)
          {
              string cardCode = dataTable.GetValue("CardCode", 0).ToString();
              string cardName = dataTable.GetValue("CardName", 0).ToString();

              ((EditText)_form.Items.Item(TXT_CUSTCD).Specific).Value = cardCode;
              ((EditText)_form.Items.Item(TXT_CUSTNM).Specific).Value = cardName;

              // Load additional customer info from OCRD
              SAPbobsCOM.Recordset recordset =
                  (SAPbobsCOM.Recordset)_app.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

              try
              {
                  string query =
                      $"SELECT CardType, Phone1, E_Mail FROM OCRD WHERE CardCode = '{DatabaseHelper.EscapeSqlString(cardCode)}'";
                  recordset.DoQuery(query);

                  if (!recordset.EoF)
                  {
                      string cardType =
                          SafeConversion.SafeToString(recordset.Fields.Item("CardType").Value);

                      ComboBox custTypeCombo = (ComboBox)_form.Items.Item(CBO_CUSTT).Specific;
                      if (!string.IsNullOrEmpty(cardType))
                      {
                          try
                          {
                              // CardType is C/S/L; we localize to readable in LoadContractToForm / mapping
                              custTypeCombo.Select(
                                  cardType == "C" ? "Customer" :
                                  cardType == "S" ? "Supplier" :
                                  cardType == "L" ? "Lead" : "Customer",
                                  BoSearchKey.psk_ByValue);
                          }
                          catch
                          {
                              // ignore if value not in list
                          }
                      }

                      ((EditText)_form.Items.Item(TXT_PHONE).Specific).Value =
                          SafeConversion.SafeToString(recordset.Fields.Item("Phone1").Value);
                      ((EditText)_form.Items.Item(TXT_EMAIL).Specific).Value =
                          SafeConversion.SafeToString(recordset.Fields.Item("E_Mail").Value);
                  }
              }
              finally
              {
                  System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
              }
          }

          private void HandleQuotationChooseFromList(DataTable dataTable)
          {
              try
              {
                  string docEntryStr = dataTable.GetValue("DocEntry", 0).ToString();
                  string docNumStr = dataTable.GetValue("DocNum", 0).ToString();
                  string cardCode = dataTable.GetValue("CardCode", 0).ToString();
                  string cardName = dataTable.GetValue("CardName", 0).ToString();

                  // Load quotation lines into contract matrix
                  // Header fields (quotation reference, customer) are intentionally not updated here
                  // to avoid SAP B1 focus/value restrictions during CFL handling.
                  if (int.TryParse(docEntryStr, out int docEntry))
                  {
                      LoadQuotationLinesToMatrix(docEntry);
                  }
              }
              catch (Exception ex)
              {
                  Logger.Error("Error handling quotation ChooseFromList", ex);
              }
          }

        /// <summary>
        /// Browse for customer
        /// </summary>
        private void BrowseCustomer()
        {
            try
            {
                // Focus customer code field; Choose-From-List is bound to it
                _form.Items.Item(TXT_CUSTCD).Click(BoCellClickType.ct_Regular);
                ((EditText)_form.Items.Item(TXT_CUSTCD).Specific).Active = true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error browsing customer", ex);
            }
        }

        /// <summary>
        /// Browse for sales quotation (price quotation reference)
        /// </summary>
        private void BrowseQuotation()
        {
            try
            {
                // Focus quotation reference field; Choose-From-List is bound to it
                _form.Items.Item(TXT_QREF).Click(BoCellClickType.ct_Regular);
                ((EditText)_form.Items.Item(TXT_QREF).Specific).Active = true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error browsing quotation", ex);
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

                // Customer type
                  ComboBox custTypeCombo = (ComboBox)_form.Items.Item(CBO_CUSTT).Specific;
                  try
                  {
                      string uiCustType = string.Empty;
                      switch (contract.CustomerType)
                      {
                          case "C":
                              uiCustType = "Customer";
                              break;
                          case "S":
                              uiCustType = "Supplier";
                              break;
                          case "L":
                              uiCustType = "Lead";
                              break;
                      }

                      if (!string.IsNullOrEmpty(uiCustType))
                          custTypeCombo.Select(uiCustType, BoSearchKey.psk_ByValue);
                      else if (custTypeCombo.ValidValues.Count > 0)
                          custTypeCombo.Select(0, BoSearchKey.psk_Index);
                  }
                  catch
                  {
                      if (custTypeCombo.ValidValues.Count > 0)
                          custTypeCombo.Select(0, BoSearchKey.psk_Index);
                  }

                // Additional information
                ((EditText)_form.Items.Item(TXT_PROJCD).Specific).Value = contract.ProjectCode ?? "";
                ((EditText)_form.Items.Item(TXT_PROJNM).Specific).Value = contract.ProjectName ?? "";
                ((EditText)_form.Items.Item(TXT_SECTOR).Specific).Value = contract.Sector ?? "";
                ((EditText)_form.Items.Item(TXT_USRTYP).Specific).Value = contract.UserType ?? "";
                ((EditText)_form.Items.Item(TXT_UNIT).Specific).Value = contract.UnitNumber ?? "";
                ((EditText)_form.Items.Item(TXT_REGION).Specific).Value = contract.Region ?? "";
                ((EditText)_form.Items.Item(TXT_QREF).Specific).Value = contract.PriceQuoteRef ?? "";

                // Contact fields - would come from customer record
                ((EditText)_form.Items.Item(TXT_PERSON).Specific).Value = "";
                ((EditText)_form.Items.Item(TXT_EMAIL).Specific).Value = "";
                ((EditText)_form.Items.Item(TXT_PHONE).Specific).Value = "";

                ((EditText)_form.Items.Item(TXT_START).Specific).Value = contract.StartDate.ToString("yyyyMMdd");
                ((EditText)_form.Items.Item(TXT_END).Specific).Value = contract.EndDate.ToString("yyyyMMdd");
                  ((EditText)_form.Items.Item(TXT_DUR).Specific).Value = contract.DurationDays.ToString();
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

                  // Approval status
                  ComboBox apprCombo = (ComboBox)_form.Items.Item(CBO_APPR).Specific;
                  try
                  {
                      string uiAppr = "Pending";
                      switch (contract.ApprovalStatus)
                      {
                          case "A":
                              uiAppr = "Approved";
                              break;
                          case "R":
                              uiAppr = "Rejected";
                              break;
                          case "P":
                          default:
                              uiAppr = "Pending";
                              break;
                      }

                      apprCombo.Select(uiAppr, BoSearchKey.psk_ByValue);
                  }
                  catch
                  {
                      if (apprCombo.ValidValues.Count > 0)
                          apprCombo.Select(0, BoSearchKey.psk_Index);
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
                    ((EditText)matrix.Columns.Item("ColUnit").Cells.Item(row).Specific).Value = line.UnitOfMeasure ?? "";
                    ((EditText)matrix.Columns.Item("ColPrice").Cells.Item(row).Specific).Value = line.UnitPrice.ToString();
                    ((EditText)matrix.Columns.Item("ColTotal").Cells.Item(row).Specific).Value = line.LineTotal.ToString();
                    // Other RVCM_CNTRCT1 fields (Cost Code, Stage, Discount, Tax, Dates, Status, Remarks)
                    // are not yet mapped on the ContractLine model and remain empty for now.
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

                // Basic date validation at UI level: EndDate must be after StartDate
                if (contract.EndDate <= contract.StartDate)
                {
                    _app.UIApp.StatusBar.SetText(
                        _lang.GetString("Contract_Val_EndDateAfterStart", "End date must be after start date"),
                        BoMessageTime.bmt_Short,
                        BoStatusBarMessageType.smt_Error);
                    return;
                }

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
                // Basic header fields
                Contract contract = new Contract
                {
                Code = ((EditText)_form.Items.Item(TXT_CODE).Specific).Value,
                Description = ((EditText)_form.Items.Item(TXT_NAME).Specific).Value,
                CustomerCode = ((EditText)_form.Items.Item(TXT_CUSTCD).Specific).Value,
                CustomerName = ((EditText)_form.Items.Item(TXT_CUSTNM).Specific).Value,
                StartDate = DateTime.ParseExact(
                    ((EditText)_form.Items.Item(TXT_START).Specific).Value,
                    "yyyyMMdd",
                    null),
                EndDate = DateTime.ParseExact(
                    ((EditText)_form.Items.Item(TXT_END).Specific).Value,
                    "yyyyMMdd",
                    null),
                TotalValue = SafeConversion.SafeToDouble(
                    ((EditText)_form.Items.Item(TXT_TOTAL).Specific).Value),
                RetentionPercentage = SafeConversion.SafeToDouble(
                    ((EditText)_form.Items.Item(TXT_RETEN).Specific).Value),
                Status = ((ComboBox)_form.Items.Item(CBO_STATUS).Specific).Selected.Value,
                Currency = ((ComboBox)_form.Items.Item(CBO_CURR).Specific).Selected.Value
            };

            // Additional header fields mapped to @RVCM_CNTRCT
            // Customer type: map readable value to code C/S/L
            string uiCustomerType = ((ComboBox)_form.Items.Item(CBO_CUSTT).Specific).Selected.Value;
            switch (uiCustomerType)
            {
                case "Customer":
                    contract.CustomerType = "C";
                    break;
                case "Supplier":
                    contract.CustomerType = "S";
                    break;
                case "Lead":
                    contract.CustomerType = "L";
                    break;
                default:
                    contract.CustomerType = string.Empty;
                    break;
            }
            contract.ProjectCode = ((EditText)_form.Items.Item(TXT_PROJCD).Specific).Value;
            contract.ProjectName = ((EditText)_form.Items.Item(TXT_PROJNM).Specific).Value;
            contract.Sector = ((EditText)_form.Items.Item(TXT_SECTOR).Specific).Value;
            contract.UserType = ((EditText)_form.Items.Item(TXT_USRTYP).Specific).Value;
            contract.UnitNumber = ((EditText)_form.Items.Item(TXT_UNIT).Specific).Value;
            contract.Region = ((EditText)_form.Items.Item(TXT_REGION).Specific).Value;
            contract.PriceQuoteRef = ((EditText)_form.Items.Item(TXT_QREF).Specific).Value;

            // Approval Status: map readable value to code P/A/R
            string uiApproval = ((ComboBox)_form.Items.Item(CBO_APPR).Specific).Selected.Value;
            switch (uiApproval)
            {
                case "Pending":
                    contract.ApprovalStatus = "P";
                    break;
                case "Approved":
                    contract.ApprovalStatus = "A";
                    break;
                case "Rejected":
                    contract.ApprovalStatus = "R";
                    break;
                default:
                    contract.ApprovalStatus = "P";
                    break;
            }

                // Keep DurationDays in sync with dates
                contract.DurationDays = (contract.EndDate - contract.StartDate).Days;

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

        /// <summary>
        /// Ensure date pickers are bound as date user data sources so SAP B1 shows calendar controls.
        /// </summary>
        private void InitializeDatePickers()
        {
            try
            {
                var uds = _form.DataSources.UserDataSources;

                const string dsStart = "UDS_STDATE";
                const string dsEnd = "UDS_ENDATE";

                try
                {
                    var _ = uds.Item(dsStart);
                }
                catch
                {
                    uds.Add(dsStart, BoDataType.dt_DATE);
                }

                try
                {
                    var _ = uds.Item(dsEnd);
                }
                catch
                {
                    uds.Add(dsEnd, BoDataType.dt_DATE);
                }

                EditText start = (EditText)_form.Items.Item(TXT_START).Specific;
                start.DataBind.SetBound(true, "", dsStart);

                EditText end = (EditText)_form.Items.Item(TXT_END).Specific;
                end.DataBind.SetBound(true, "", dsEnd);
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing date pickers", ex);
            }
        }

        /// <summary>
        /// Load SAP B1 sales quotation lines into the contract lines matrix.
        /// </summary>
        private void LoadQuotationLinesToMatrix(int quotationDocEntry)
        {
            try
            {
                Matrix matrix = (Matrix)_form.Items.Item(MTX_LINES).Specific;
                matrix.Clear();

                SAPbobsCOM.Recordset rs =
                    (SAPbobsCOM.Recordset)_app.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                try
                {
                    string query;
                    string unitField;
                    if (_app.Company.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        // HANA is case-sensitive; QUT1 uses lower-case unitMsr
                        query =
                            "SELECT \"ItemCode\", \"Dscription\", \"Quantity\", \"unitMsr\", \"Price\", \"DiscPrcnt\", " +
                            "\"LineTotal\", \"TaxCode\", \"VatPrcnt\", \"VatSum\" " +
                            $"FROM \"QUT1\" WHERE \"DocEntry\" = {quotationDocEntry} ORDER BY \"LineNum\"";
                        unitField = "unitMsr";
                    }
                    else
                    {
                        query =
                            "SELECT ItemCode, Dscription, Quantity, UnitMsr, Price, DiscPrcnt, " +
                            "LineTotal, TaxCode, VatPrcnt, VatSum " +
                            $"FROM QUT1 WHERE DocEntry = {quotationDocEntry} ORDER BY LineNum";
                        unitField = "UnitMsr";
                    }

                    rs.DoQuery(query);

                    int row = 0;
                    while (!rs.EoF)
                    {
                        matrix.AddRow();
                        row++;

                        string itemCode = SafeConversion.SafeToString(rs.Fields.Item("ItemCode").Value);
                        string desc = SafeConversion.SafeToString(rs.Fields.Item("Dscription").Value);
                        // Sanitize description to avoid invalid characters/lengths
                        if (!string.IsNullOrEmpty(desc))
                        {
                            desc = desc.Replace("\r", " ").Replace("\n", " ");
                            if (desc.Length > 254)
                                desc = desc.Substring(0, 254);
                        }

                        double qty = SafeConversion.SafeToDouble(rs.Fields.Item("Quantity").Value);
                        string unit = SafeConversion.SafeToString(rs.Fields.Item(unitField).Value);
                        double price = SafeConversion.SafeToDouble(rs.Fields.Item("Price").Value);
                        double lineTotal = SafeConversion.SafeToDouble(rs.Fields.Item("LineTotal").Value);

                        // Use a minimal safe subset of fields to avoid SAP internal errors:
                        // only Item, Description, Quantity, Unit, Unit Price, Line Total.
                        string qtyStr = qty.ToString();
                        string totalStr = lineTotal.ToString();

                        SetMatrixCell(matrix, "ColItem", row, itemCode);
                        SetMatrixCell(matrix, "ColDesc", row, desc);
                        SetMatrixCell(matrix, "ColQty", row, qtyStr);
                        SetMatrixCell(matrix, "ColUnit", row, unit);
                        SetMatrixCell(matrix, "ColTotal", row, totalStr);

                        rs.MoveNext();
                    }

                    if (row == 0)
                    {
                        matrix.AddRow();
                        matrix.ClearRowData(1);
                    }
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rs);
                }

                Logger.Info($"Loaded quotation {quotationDocEntry} lines into contract matrix");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading quotation lines to matrix", ex);
            }
        }

        /// <summary>
        /// Configure Choose-From-List for customer code field.
        /// </summary>
        private void CreateCustomerChooseFromList()
        {
            try
            {
                ChooseFromListCollection cfls = _form.ChooseFromLists;

                // Avoid duplicate creation
                try
                {
                    var existing = cfls.Item(CFL_CUST);
                    if (existing != null)
                        return;
                }
                catch
                {
                    // Not found - continue to create
                }

                // Ensure the customer code field is data-bound, required for CFL
                try
                {
                    var uds = _form.DataSources.UserDataSources;
                    const string dataSourceId = "UDS_CUSTCD";

                    // Ensure the user data source exists
                    try
                    {
                        var _ = uds.Item(dataSourceId);
                    }
                    catch
                    {
                        uds.Add(dataSourceId, BoDataType.dt_SHORT_TEXT, 20);
                    }

                    EditText boundCustCode = (EditText)_form.Items.Item(TXT_CUSTCD).Specific;
                    boundCustCode.DataBind.SetBound(true, "", dataSourceId);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error binding customer code data source for CFL", ex);
                }

                ChooseFromListCreationParams cflParams =
                    (ChooseFromListCreationParams)_app.UIApp.CreateObject(
                        BoCreatableObjectType.cot_ChooseFromListCreationParams);
                cflParams.MultiSelection = false;
                cflParams.ObjectType = "2"; // Business Partner
                cflParams.UniqueID = CFL_CUST;

                ChooseFromList cfl = cfls.Add(cflParams);

                // Restrict to customers (CardType = 'C')
                Conditions conditions = cfl.GetConditions();
                Condition condition = conditions.Add();
                condition.Alias = "CardType";
                condition.Operation = BoConditionOperation.co_EQUAL;
                condition.CondVal = "C";
                cfl.SetConditions(conditions);

                // Bind customer code edit text to CFL
                EditText custCode = (EditText)_form.Items.Item(TXT_CUSTCD).Specific;
                custCode.ChooseFromListUID = CFL_CUST;
                custCode.ChooseFromListAlias = "CardCode";
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating customer ChooseFromList", ex);
            }
        }

        /// <summary>
        /// Safely set a matrix cell value, swallowing "Bad Value" errors that can occur due to UI constraints.
        /// </summary>
        private void SetMatrixCell(Matrix matrix, string columnId, int row, string value)
        {
            try
            {
                ((EditText)matrix.Columns.Item(columnId).Cells.Item(row).Specific).Value = value ?? string.Empty;
            }
            catch
            {
                // Ignore COMExceptions like "Form - Bad Value" so loading continues
            }
        }

        /// <summary>
        /// Configure Choose-From-List for quotation reference (Sales Quotation list).
        /// </summary>
        private void CreateQuotationChooseFromList()
        {
            try
            {
                ChooseFromListCollection cfls = _form.ChooseFromLists;

                // Avoid duplicate creation
                try
                {
                    var existing = cfls.Item(CFL_QREF);
                    if (existing != null)
                        return;
                }
                catch
                {
                    // Not found - continue to create
                }

                // Ensure the quotation ref field is data-bound (required for CFL)
                try
                {
                    var uds = _form.DataSources.UserDataSources;
                    const string dataSourceId = "UDS_QREF";

                    try
                    {
                        var _ = uds.Item(dataSourceId);
                    }
                    catch
                    {
                        uds.Add(dataSourceId, BoDataType.dt_SHORT_TEXT, 50);
                    }

                    EditText boundQuoteRef = (EditText)_form.Items.Item(TXT_QREF).Specific;
                    boundQuoteRef.DataBind.SetBound(true, "", dataSourceId);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error binding quotation ref data source for CFL", ex);
                }

                ChooseFromListCreationParams cflParams =
                    (ChooseFromListCreationParams)_app.UIApp.CreateObject(
                        BoCreatableObjectType.cot_ChooseFromListCreationParams);
                cflParams.MultiSelection = false;
                cflParams.ObjectType = "23"; // Sales Quotation
                cflParams.UniqueID = CFL_QREF;

                ChooseFromList cfl = cfls.Add(cflParams);

                // Optionally restrict to open quotations: DocStatus = 'O'
                Conditions conditions = cfl.GetConditions();
                Condition condition = conditions.Add();
                condition.Alias = "DocStatus";
                condition.Operation = BoConditionOperation.co_EQUAL;
                condition.CondVal = "O";
                cfl.SetConditions(conditions);

                // Bind quotation ref edit text to CFL (DocNum)
                EditText quoteRef = (EditText)_form.Items.Item(TXT_QREF).Specific;
                quoteRef.ChooseFromListUID = CFL_QREF;
                quoteRef.ChooseFromListAlias = "DocNum";
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating quotation ChooseFromList", ex);
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
