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
    /// Interim Payment Certificate (IPC) form
    /// </summary>
    public class IPCForm
    {
        private ContractManagementApplication _app;
        private SAPbouiCOM.Form _form;
        private IPCService _ipcService;
        private LanguageManager _lang;
        private bool _isRTL;
        // Note: _currentIPC removed as it was not being used

        private const string FORM_TYPE = "FRM_IPC";

        public IPCForm(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _ipcService = new IPCService(_app.Company);
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
                Logger.Info("IPC form opened");

                _app.UIApp.StatusBar.SetText("IPC form - Simplified version for demonstration",
                    BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing IPC form", ex);
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
            _form.Title = _lang.GetString("Form_IPC_Title");
            _form.Width = 800;
            _form.Height = 600;
            _form.Left = 250;
            _form.Top = 120;
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

                // Title label
                AddLabel("lblTitle", "", leftMargin, 10, 400, 20);

                // Contract selection
                AddLabel("lblContract", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtContract", leftMargin + 130, topPosition, 200, 14);
                topPosition += rowHeight;

                // IPC Number
                AddLabel("lblIPCNum", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtIPCNum", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // IPC Date
                AddLabel("lblIPCDate", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtIPCDate", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Gross Amount
                AddLabel("lblGross", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtGross", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Retention
                AddLabel("lblRetention", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtRetention", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Net Amount
                AddLabel("lblNet", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtNet", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus", "", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbStatus", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight + 20;

                // Buttons
                AddButton("btnSave", "", leftMargin, topPosition, 80, 20);
                AddButton("btnSubmit", "", leftMargin + 90, topPosition, 80, 20);
                AddButton("btnApprove", "", leftMargin + 180, topPosition, 80, 20);
                AddButton("btnInvoice", "", leftMargin + 270, topPosition, 100, 20);

                // Lines grid
                topPosition += 30;
                AddLabel("lblLines", "", leftMargin, topPosition, 120, 14);
                topPosition += 20;
                AddGrid("gridLines", leftMargin, topPosition, 750, 250);

                Logger.Info("IPC form controls initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing IPC form controls", ex);
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
            combo.ValidValues.Add("Draft", _lang.GetString("Status_Draft"));
            combo.ValidValues.Add("Submitted", _lang.GetString("Status_Submitted"));
            combo.ValidValues.Add("Approved", _lang.GetString("Status_Approved"));
            combo.ValidValues.Add("Rejected", _lang.GetString("Status_Rejected"));
            combo.ValidValues.Add("Paid", _lang.GetString("Status_Paid"));
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
                _form.Title = _lang.GetString("Form_IPC_Title");

                // Localize labels
                ((StaticText)_form.Items.Item("lblTitle").Specific).Caption = _lang.GetString("Form_IPC_Title");
                ((StaticText)_form.Items.Item("lblContract").Specific).Caption = _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item("lblIPCNum").Specific).Caption = _lang.GetString("IPC_Number");
                ((StaticText)_form.Items.Item("lblIPCDate").Specific).Caption = _lang.GetString("IPC_Date");
                ((StaticText)_form.Items.Item("lblGross").Specific).Caption = _lang.GetString("IPC_GrossAmount");
                ((StaticText)_form.Items.Item("lblRetention").Specific).Caption = _lang.GetString("IPC_Retention");
                ((StaticText)_form.Items.Item("lblNet").Specific).Caption = _lang.GetString("IPC_NetAmount");
                ((StaticText)_form.Items.Item("lblStatus").Specific).Caption = _lang.GetString("IPC_Status");
                ((StaticText)_form.Items.Item("lblLines").Specific).Caption = _lang.GetString("IPC_Lines");

                // Localize buttons
                ((Button)_form.Items.Item("btnSave").Specific).Caption = _lang.GetString("Common_Save");
                ((Button)_form.Items.Item("btnSubmit").Specific).Caption = _lang.GetString("Common_Submit");
                ((Button)_form.Items.Item("btnApprove").Specific).Caption = _lang.GetString("Common_Approve");
                ((Button)_form.Items.Item("btnInvoice").Specific).Caption = _lang.GetString("IPC_CreateInvoice");

                // Apply RTL if needed
                if (_isRTL)
                {
                    _lang.LocalizeForm(_form);
                }

                Logger.Info("IPC form localized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error localizing IPC form", ex);
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
