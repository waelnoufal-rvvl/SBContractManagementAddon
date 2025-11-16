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
    /// Change Order management form
    /// </summary>
    public class ChangeOrderForm
    {
        private ContractManagementApplication _app;
        private SAPbouiCOM.Form _form;
        private ChangeOrderService _coService;
        private LanguageManager _lang;
        private bool _isRTL;
        // Note: _currentCO removed as it was not being used

        private const string FORM_TYPE = "FRM_CO";

        public ChangeOrderForm(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _coService = new ChangeOrderService(_app.Company);
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

                _form.Visible = true;
                Logger.Info("Change Order form opened");

                _app.UIApp.StatusBar.SetText("Change Order form - Simplified version for demonstration",
                    BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing Change Order form", ex);
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
            _form.Title = _lang.GetString("Form_ChangeOrder_Title");
            _form.Width = 750;
            _form.Height = 550;
            _form.Left = 270;
            _form.Top = 130;
        }

        /// <summary>
        /// Initialize form controls
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                int leftMargin = 20;
                int topPosition = 40;
                int rowHeight = 25;

                // Title
                AddLabel("lblTitle", "", leftMargin, 10, 300, 20);

                // Contract Code
                AddLabel("lblContract", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtContract", leftMargin + 130, topPosition, 200, 14);
                topPosition += rowHeight;

                // CO Number
                AddLabel("lblCONum", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtCONum", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // CO Date
                AddLabel("lblCODate", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtCODate", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Type
                AddLabel("lblType", "", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbType", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Description
                AddLabel("lblDesc", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtDesc", leftMargin + 130, topPosition, 400, 14);
                topPosition += rowHeight;

                // Amount
                AddLabel("lblAmount", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtAmount", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Additional Days
                AddLabel("lblDays", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtDays", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus", "", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbStatus", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight + 20;

                // Buttons
                AddButton("btnSave", "", leftMargin, topPosition, 80, 20);
                AddButton("btnSubmit", "", leftMargin + 90, topPosition, 80, 20);
                AddButton("btnApprove", "", leftMargin + 180, topPosition, 80, 20);
                AddButton("btnReject", "", leftMargin + 270, topPosition, 80, 20);
                AddButton("btnImpact", "", leftMargin + 360, topPosition, 120, 20);

                // Lines grid
                topPosition += 30;
                AddLabel("lblLines", "", leftMargin, topPosition, 150, 14);
                topPosition += 20;
                AddGrid("gridLines", leftMargin, topPosition, 700, 200);

                Logger.Info("Change Order form controls initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing Change Order form controls", ex);
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

            ComboBox combo = (ComboBox)item.Specific;
            if (id == "cmbType")
            {
                combo.ValidValues.Add("Addition", _lang.GetString("CO_Type_Addition"));
                combo.ValidValues.Add("Deduction", _lang.GetString("CO_Type_Deduction"));
                combo.ValidValues.Add("TimeExtension", _lang.GetString("CO_Type_TimeExtension"));
            }
            else if (id == "cmbStatus")
            {
                combo.ValidValues.Add("Draft", _lang.GetString("Status_Draft"));
                combo.ValidValues.Add("Submitted", _lang.GetString("Status_Submitted"));
                combo.ValidValues.Add("Approved", _lang.GetString("Status_Approved"));
                combo.ValidValues.Add("Rejected", _lang.GetString("Status_Rejected"));
            }
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
                _form.Title = _lang.GetString("Form_ChangeOrder_Title");

                // Localize labels
                ((StaticText)_form.Items.Item("lblTitle").Specific).Caption = _lang.GetString("Form_ChangeOrder_Title");
                ((StaticText)_form.Items.Item("lblContract").Specific).Caption = _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item("lblCONum").Specific).Caption = _lang.GetString("CO_Number");
                ((StaticText)_form.Items.Item("lblCODate").Specific).Caption = _lang.GetString("CO_Date");
                ((StaticText)_form.Items.Item("lblType").Specific).Caption = _lang.GetString("CO_Type");
                ((StaticText)_form.Items.Item("lblDesc").Specific).Caption = _lang.GetString("CO_Description");
                ((StaticText)_form.Items.Item("lblAmount").Specific).Caption = _lang.GetString("CO_Amount");
                ((StaticText)_form.Items.Item("lblDays").Specific).Caption = _lang.GetString("CO_AdditionalDays");
                ((StaticText)_form.Items.Item("lblStatus").Specific).Caption = _lang.GetString("CO_Status");
                ((StaticText)_form.Items.Item("lblLines").Specific).Caption = _lang.GetString("CO_Lines");

                // Localize buttons
                ((Button)_form.Items.Item("btnSave").Specific).Caption = _lang.GetString("Common_Save");
                ((Button)_form.Items.Item("btnSubmit").Specific).Caption = _lang.GetString("Common_Submit");
                ((Button)_form.Items.Item("btnApprove").Specific).Caption = _lang.GetString("Common_Approve");
                ((Button)_form.Items.Item("btnReject").Specific).Caption = _lang.GetString("Common_Reject");
                ((Button)_form.Items.Item("btnImpact").Specific).Caption = _lang.GetString("CO_ImpactAnalysis");

                // Apply RTL if needed
                if (_isRTL)
                {
                    _lang.LocalizeForm(_form);
                }

                Logger.Info("Change Order form localized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing Change Order form", ex);
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
