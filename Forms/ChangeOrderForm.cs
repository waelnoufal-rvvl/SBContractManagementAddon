using System;
using SAPbouiCOM;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

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
        // Note: _currentCO removed as it was not being used

        private const string FORM_TYPE = "FRM_CO_V2"; // Versioned to force fresh form creation

        public ChangeOrderForm(ContractManagementApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _coService = new ChangeOrderService(_app.Company);
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
            _form.Title = "Change Order Management";
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
                AddLabel("lblTitle", "ChgOrder", leftMargin, 10, 300, 20);

                // Contract Code
                AddLabel("lblContract", "Cntrct:", leftMargin, topPosition, 120, 14);
                AddTextBox("txtContract", leftMargin + 130, topPosition, 200, 14);
                topPosition += rowHeight;

                // CO Number
                AddLabel("lblCONum", "CO Num:", leftMargin, topPosition, 120, 14);
                AddTextBox("txtCONum", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // CO Date
                AddLabel("lblCODate", "CO Date:", leftMargin, topPosition, 120, 14);
                AddEditText("txtCODate", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Type
                AddLabel("lblType", "Type:", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbType", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Description
                AddLabel("lblDesc", "Desc:", leftMargin, topPosition, 120, 14);
                AddTextBox("txtDesc", leftMargin + 130, topPosition, 400, 14);
                topPosition += rowHeight;

                // Amount
                AddLabel("lblAmount", "Amount:", leftMargin, topPosition, 120, 14);
                AddEditText("txtAmount", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight;

                // Additional Days
                AddLabel("lblDays", "Days:", leftMargin, topPosition, 120, 14);
                AddEditText("txtDays", leftMargin + 130, topPosition, 100, 14);
                topPosition += rowHeight;

                // Status
                AddLabel("lblStatus", "Status:", leftMargin, topPosition, 120, 14);
                AddComboBox("cmbStatus", leftMargin + 130, topPosition, 150, 14);
                topPosition += rowHeight + 20;

                // Buttons
                AddButton("btnSave", "Save", leftMargin, topPosition, 80, 20);
                AddButton("btnSubmit", "Submit", leftMargin + 90, topPosition, 80, 20);
                AddButton("btnApprove", "Approve", leftMargin + 180, topPosition, 80, 20);
                AddButton("btnReject", "Reject", leftMargin + 270, topPosition, 80, 20);
                AddButton("btnImpact", "Impact", leftMargin + 360, topPosition, 120, 20);

                // Lines grid
                topPosition += 30;
                AddLabel("lblLines", "Lines:", leftMargin, topPosition, 150, 14);
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
                combo.ValidValues.Add("Addition", "Addition");
                combo.ValidValues.Add("Deduction", "Deduction");
                combo.ValidValues.Add("TimeExtension", "Time Extension");
            }
            else if (id == "cmbStatus")
            {
                combo.ValidValues.Add("Draft", "Draft");
                combo.ValidValues.Add("Submitted", "Submitted");
                combo.ValidValues.Add("Approved", "Approved");
                combo.ValidValues.Add("Rejected", "Rejected");
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
    }
}
