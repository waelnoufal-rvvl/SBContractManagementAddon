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
        private IContractManagementApp _app;
        private SAPbouiCOM.Form _form;
        private IPCService _ipcService;
        private LanguageManager _lang;
        private bool _isRTL;
        // Note: _currentIPC removed as it was not being used
        private string _currentIPCCode;

        private const string FORM_TYPE = "FRM_IPC";

        public IPCForm(IContractManagementApp app)
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
                AttachEvents();
                AttachEvents();

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
                AddLabel("lblContr", "", leftMargin, topPosition, 120, 14);
                AddTextBox("txtContr", leftMargin + 130, topPosition, 200, 14);
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
                AddLabel("lblReten", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtReten", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Advance Deduction
                AddLabel("lblAdv", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtAdv", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Material Deduction
                AddLabel("lblMat", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtMat", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Other Deduction
                AddLabel("lblOther", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtOther", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // VAT Amount
                AddLabel("lblVAT", "", leftMargin, topPosition, 120, 14);
                AddEditText("txtVAT", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Net Amount / Total Due
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
            // Values follow RVCM_ICP U_Status codes: D, S, A, P, R
            combo.ValidValues.Add("D", _lang.GetString("Status_Draft"));
            combo.ValidValues.Add("S", _lang.GetString("Status_Submitted"));
            combo.ValidValues.Add("A", _lang.GetString("Status_Approved"));
            combo.ValidValues.Add("R", _lang.GetString("Status_Rejected"));
            combo.ValidValues.Add("P", _lang.GetString("Status_Paid"));
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
                ((StaticText)_form.Items.Item("lblContr").Specific).Caption = _lang.GetString("Contract_Code");
                ((StaticText)_form.Items.Item("lblIPCNum").Specific).Caption = _lang.GetString("IPC_Number");
                ((StaticText)_form.Items.Item("lblIPCDate").Specific).Caption = _lang.GetString("IPC_Date");
                ((StaticText)_form.Items.Item("lblGross").Specific).Caption = _lang.GetString("IPC_GrossAmount");
                ((StaticText)_form.Items.Item("lblReten").Specific).Caption = _lang.GetString("IPC_Retention");
                ((StaticText)_form.Items.Item("lblAdv").Specific).Caption = _lang.GetString("IPC_AdvanceDeduct");
                ((StaticText)_form.Items.Item("lblMat").Specific).Caption = _lang.GetString("IPC_MaterialDeduct");
                ((StaticText)_form.Items.Item("lblOther").Specific).Caption = _lang.GetString("IPC_OtherDeduct");
                ((StaticText)_form.Items.Item("lblVAT").Specific).Caption = _lang.GetString("IPC_VATAmount");
                ((StaticText)_form.Items.Item("lblNet").Specific).Caption = _lang.GetString("IPC_TotalDue");
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



        /// <summary>
        /// Submit current IPC for approval
        /// </summary>
        private void SubmitIPC()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentIPCCode))
                {
                    _app.UIApp.StatusBar.SetText("Please save the IPC before submitting",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                _ipcService.SubmitIPC(_currentIPCCode);

                ComboBox cmbStatus = (ComboBox)_form.Items.Item("cmbStatus").Specific;
                cmbStatus.Select("S", BoSearchKey.psk_ByValue);

                _app.UIApp.StatusBar.SetText("IPC submitted for approval",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Error submitting IPC from form", ex);
                _app.UIApp.StatusBar.SetText($"Error submitting IPC: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Approve current IPC
        /// </summary>
        private void ApproveIPC()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentIPCCode))
                {
                    _app.UIApp.StatusBar.SetText("Please save and submit the IPC before approval",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                _ipcService.ApproveIPC(_currentIPCCode);

                ComboBox cmbStatus = (ComboBox)_form.Items.Item("cmbStatus").Specific;
                cmbStatus.Select("A", BoSearchKey.psk_ByValue);

                _app.UIApp.StatusBar.SetText("IPC approved",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Error approving IPC from form", ex);
                _app.UIApp.StatusBar.SetText($"Error approving IPC: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Attach form-level events
        /// </summary>
        private void AttachEvents()
        {
            _app.UIApp.ItemEvent += OnItemEvent;
        }

        /// <summary>
        /// Handle item events for this IPC form
        /// </summary>
        private void OnItemEvent(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;

            try
            {
                if (_form == null || pVal.FormUID != _form.UniqueID)
                    return;

                if (!pVal.BeforeAction && pVal.EventType == BoEventTypes.et_ITEM_PRESSED)
                {
                    switch (pVal.ItemUID)
                    {
                        case "btnSave":
                            SaveIPC();
                            break;
                        case "btnSubmit":
                            SubmitIPC();
                            break;
                        case "btnApprove":
                            ApproveIPC();
                            break;
                        // btnInvoice can be wired later to invoice creation
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling IPC form item event", ex);
                bubbleEvent = false;
            }
        }

        /// <summary>
        /// Read form values and create a new IPC via the service
        /// </summary>
        private void SaveIPC()
        {
            try
            {
                EditText txtContr = (EditText)_form.Items.Item("txtContr").Specific;
                string contractCode = txtContr.Value?.Trim();

                if (string.IsNullOrWhiteSpace(contractCode))
                {
                    _app.UIApp.StatusBar.SetText("Contract code is required for IPC",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                EditText txtIPCNum = (EditText)_form.Items.Item("txtIPCNum").Specific;
                EditText txtIPCDate = (EditText)_form.Items.Item("txtIPCDate").Specific;
                EditText txtGross = (EditText)_form.Items.Item("txtGross").Specific;
                EditText txtAdv = (EditText)_form.Items.Item("txtAdv").Specific;
                EditText txtMat = (EditText)_form.Items.Item("txtMat").Specific;
                EditText txtOther = (EditText)_form.Items.Item("txtOther").Specific;

                int ipcNumber = SafeConversion.SafeToInt(txtIPCNum.Value, 0);
                DateTime ipcDate = SafeConversion.SafeToDateTime(txtIPCDate.Value, DateTime.Today);
                double grossAmount = SafeConversion.SafeToDouble(txtGross.Value, 0);
                double advDeduct = SafeConversion.SafeToDouble(txtAdv.Value, 0);
                double matDeduct = SafeConversion.SafeToDouble(txtMat.Value, 0);
                double otherDeduct = SafeConversion.SafeToDouble(txtOther.Value, 0);

                if (grossAmount <= 0)
                {
                    _app.UIApp.StatusBar.SetText("Gross amount must be greater than zero",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return;
                }

                IPC ipc = new IPC
                {
                    ContractCode = contractCode,
                    IPCNumber = ipcNumber,
                    IPCDate = ipcDate,
                    AdvanceDeduction = advDeduct,
                    MaterialDeduction = matDeduct,
                    OtherDeduction = otherDeduct,
                    Status = "Draft",
                    Remarks = string.Empty
                };

                // Minimal line to support IPC calculation logic (uses gross amount)
                IPCLine line = new IPCLine
                {
                    LineNum = 1,
                    Description = "Summary",
                    Quantity = 1,
                    UnitPrice = grossAmount,
                    Amount = grossAmount
                };
                ipc.Lines.Add(line);

                string newCode = _ipcService.CreateIPC(ipc);
                _currentIPCCode = newCode;

                // After creation, ipc has calculated amounts; reflect them on the form
                ((EditText)_form.Items.Item("txtGross").Specific).Value = ipc.GrossAmount.ToString("N2");
                ((EditText)_form.Items.Item("txtReten").Specific).Value = ipc.RetentionAmount.ToString("N2");
                ((EditText)_form.Items.Item("txtAdv").Specific).Value = ipc.AdvanceDeduction.ToString("N2");
                ((EditText)_form.Items.Item("txtMat").Specific).Value = ipc.MaterialDeduction.ToString("N2");
                ((EditText)_form.Items.Item("txtOther").Specific).Value = ipc.OtherDeduction.ToString("N2");
                ((EditText)_form.Items.Item("txtVAT").Specific).Value = ipc.VATAmount.ToString("N2");
                ((EditText)_form.Items.Item("txtNet").Specific).Value = ipc.NetAmount.ToString("N2");

                ComboBox cmbStatus = (ComboBox)_form.Items.Item("cmbStatus").Specific;
                cmbStatus.Select("D", BoSearchKey.psk_ByValue);

                _app.UIApp.StatusBar.SetText($"IPC created successfully. Code: {newCode}",
                    BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Success);
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving IPC from form", ex);
                _app.UIApp.StatusBar.SetText($"Error saving IPC: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }
    }
}
