using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

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
        // Note: _currentIPC removed as it was not being used

        private const string FORM_TYPE = "FRM_IPC_V2"; // Versioned to force fresh form creation

        public IPCForm(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _ipcService = new IPCService(_app.Company);
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
                    if (_form != null)
                    {
                        Logger.Info($"Form {FORM_TYPE}_1 already exists, selecting it");
                        _form.Select();
                        return;
                    }
                    Logger.Info($"Form {FORM_TYPE}_1 does not exist, creating new one");
                }
                catch (Exception ex)
                {
                    Logger.Info($"Exception checking for existing form: {ex.Message}. Creating new one.");
                }

                CreateForm();
                InitializeControls();

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
            _form.Title = "Interim Payment Certificate";
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
                AddLabel("lblTitle", "IPC", leftMargin, 10, 400, 20);

                // Contract selection
                AddLabel("lblContract", "Cntrct:", leftMargin, topPosition, 120, 14);
                AddTextBox("txtContract", leftMargin + 130, topPosition, 200, 14);
                topPosition += rowHeight;

                // IPC Number
                AddLabel("lblIPCNum", "IPC #:", leftMargin, topPosition, 120, 14);
                AddTextBox("txtIPCNum", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // IPC Date
                AddLabel("lblIPCDate", "Date:", leftMargin, topPosition, 120, 14);
                AddEditText("txtIPCDate", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Gross Amount
                AddLabel("lblGross", "Gross:", leftMargin, topPosition, 120, 14);
                AddEditText("txtGross", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Retention
                AddLabel("lblRetention", "Retent:", leftMargin, topPosition, 120, 14);
                AddEditText("txtRetention", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Net Amount
                AddLabel("lblNet", "Net:", leftMargin, topPosition, 120, 14);
                AddEditText("txtNet", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus", "Status:", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbStatus", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight + 20;

                // Buttons
                AddButton("btnSave", "Save", leftMargin, topPosition, 80, 20);
                AddButton("btnSubmit", "Submit", leftMargin + 90, topPosition, 80, 20);
                AddButton("btnApprove", "Approve", leftMargin + 180, topPosition, 80, 20);
                AddButton("btnInvoice", "Create Invoice", leftMargin + 270, topPosition, 100, 20);

                // Lines grid
                topPosition += 30;
                AddLabel("lblLines", "Lines:", leftMargin, topPosition, 120, 14);
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
            combo.ValidValues.Add("Draft", "Draft");
            combo.ValidValues.Add("Submitted", "Submitted");
            combo.ValidValues.Add("Approved", "Approved");
            combo.ValidValues.Add("Rejected", "Rejected");
            combo.ValidValues.Add("Paid", "Paid");
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
