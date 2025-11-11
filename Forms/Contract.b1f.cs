using System;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

// Alias to resolve ambiguity between SAPbouiCOM.Application and SAPbouiCOM.Framework.Application
using FrameworkApp = SAPbouiCOM.Framework.Application;
using ContractModel = ContractManagementAddon.Models.Contract;

namespace ContractManagementAddon.Forms
{
    [FormAttribute("ContractManagementAddon.Forms.Contract", "Forms/Contract.b1f")]
    class Contract : UserFormBase
    {
        private ContractService _contractService;
        private ContractModel _currentContract;

        // Control declarations
        private StaticText stTitle;
        private StaticText stCode;
        private EditText txtCode;
        private StaticText stCust;
        private EditText txtCust;
        private Button btnCust;
        private EditText txtCName;
        private StaticText stDesc;
        private EditText txtDesc;
        private StaticText stStart;
        private EditText txtStart;
        private StaticText stEnd;
        private EditText txtEnd;
        private StaticText stValue;
        private EditText txtValue;
        private StaticText stStatus;
        private ComboBox cmbStat;
        private StaticText stRet;
        private EditText txtRet;
        private Grid grdLines;
        private Button btnOK;
        private Button btnCancel;
        private Button btnFind;

        // Data sources
        private DataTable dtHead;
        private DataTable dtLines;

        public Contract()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            try
            {
                Logger.Info("OnInitializeComponent started");

                // Create data sources
                this.dtHead = this.UIAPIRawForm.DataSources.DataTables.Add("DT_HEAD");
                this.dtLines = this.UIAPIRawForm.DataSources.DataTables.Add("DT_LINES");

                // Add columns to dtHead
                this.dtHead.Columns.Add("Code", BoFieldsType.ft_AlphaNumeric, 50);
                this.dtHead.Columns.Add("Customer", BoFieldsType.ft_AlphaNumeric, 50);
                this.dtHead.Columns.Add("CustName", BoFieldsType.ft_AlphaNumeric, 100);
                this.dtHead.Columns.Add("Descript", BoFieldsType.ft_AlphaNumeric, 254);
                this.dtHead.Columns.Add("StartDate", BoFieldsType.ft_Date);
                this.dtHead.Columns.Add("EndDate", BoFieldsType.ft_Date);
                this.dtHead.Columns.Add("Value", BoFieldsType.ft_Sum);
                this.dtHead.Columns.Add("Status", BoFieldsType.ft_AlphaNumeric, 1);
                this.dtHead.Columns.Add("Retention", BoFieldsType.ft_Sum);

                // Add columns to dtLines
                this.dtLines.Columns.Add("LineNum", BoFieldsType.ft_AlphaNumeric, 10);
                this.dtLines.Columns.Add("ItemCode", BoFieldsType.ft_AlphaNumeric, 50);
                this.dtLines.Columns.Add("Descript", BoFieldsType.ft_AlphaNumeric, 254);
                this.dtLines.Columns.Add("Quantity", BoFieldsType.ft_Quantity);
                this.dtLines.Columns.Add("Price", BoFieldsType.ft_Price);
                this.dtLines.Columns.Add("Total", BoFieldsType.ft_Sum);

                // Create form items (controls)
                CreateFormItems();

                // Get references to created controls
                InitializeControlReferences();

                Logger.Info("OnInitializeComponent completed");

                this.OnCustomInitialize();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnInitializeComponent", ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize form event handlers. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.ClickAfter += new ClickAfterHandler(this.Form_ClickAfter);
        }

        /// <summary>
        /// Create all form items (controls)
        /// </summary>
        private void CreateFormItems()
        {
            Item oItem = null;
            StaticText oStatic = null;
            EditText oEdit = null;
            Button oButton = null;
            ComboBox oCombo = null;
            Folder oFolder = null;
            Grid oGrid = null;

            try
            {
                // Title
                oItem = this.UIAPIRawForm.Items.Add("stTitle", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 5;
                oItem.Width = 730;
                oItem.Height = 14;
                oItem.FromPane = 0;
                oItem.ToPane = 0;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Contract - Header";

                // Folder tabs
                oItem = this.UIAPIRawForm.Items.Add("fldGen", BoFormItemTypes.it_FOLDER);
                oItem.Left = 5;
                oItem.Top = 25;
                oItem.Width = 80;
                oItem.Height = 14;
                oFolder = (Folder)oItem.Specific;
                oFolder.Caption = "General";
                oFolder.GroupWith("fldLines");
                oFolder.Pane = 1;

                oItem = this.UIAPIRawForm.Items.Add("fldLines", BoFormItemTypes.it_FOLDER);
                oItem.Left = 90;
                oItem.Top = 25;
                oItem.Width = 80;
                oItem.Height = 14;
                oFolder = (Folder)oItem.Specific;
                oFolder.Caption = "Lines";
                oFolder.Pane = 2;

                // === PANE 1: GENERAL TAB ===

                // Code label
                oItem = this.UIAPIRawForm.Items.Add("stCode", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 50;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Code:";

                // Code text
                oItem = this.UIAPIRawForm.Items.Add("txtCode", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 50;
                oItem.Width = 150;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD", "Code");

                // Customer label
                oItem = this.UIAPIRawForm.Items.Add("stCust", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 70;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Customer:";

                // Customer text
                oItem = this.UIAPIRawForm.Items.Add("txtCust", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 70;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","Customer");

                // Customer chooser button
                oItem = this.UIAPIRawForm.Items.Add("btnCust", BoFormItemTypes.it_BUTTON);
                oItem.Left = 225;
                oItem.Top = 70;
                oItem.Width = 20;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oButton = (Button)oItem.Specific;
                oButton.Caption = "...";

                // Customer name text (read-only)
                oItem = this.UIAPIRawForm.Items.Add("txtCName", BoFormItemTypes.it_EDIT);
                oItem.Left = 250;
                oItem.Top = 70;
                oItem.Width = 250;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oItem.Enabled = false;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","CustName");

                // Description label
                oItem = this.UIAPIRawForm.Items.Add("stDesc", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 90;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Desc:";

                // Description text
                oItem = this.UIAPIRawForm.Items.Add("txtDesc", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 90;
                oItem.Width = 600;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","Descript");

                // Start Date label
                oItem = this.UIAPIRawForm.Items.Add("stStart", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 110;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Start:";

                // Start Date text
                oItem = this.UIAPIRawForm.Items.Add("txtStart", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 110;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","StartDate");

                // End Date label
                oItem = this.UIAPIRawForm.Items.Add("stEnd", BoFormItemTypes.it_STATIC);
                oItem.Left = 250;
                oItem.Top = 110;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "End:";

                // End Date text
                oItem = this.UIAPIRawForm.Items.Add("txtEnd", BoFormItemTypes.it_EDIT);
                oItem.Left = 360;
                oItem.Top = 110;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","EndDate");

                // Value label
                oItem = this.UIAPIRawForm.Items.Add("stValue", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 130;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Value:";

                // Value text
                oItem = this.UIAPIRawForm.Items.Add("txtValue", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 130;
                oItem.Width = 150;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","Value");

                // Status label
                oItem = this.UIAPIRawForm.Items.Add("stStatus", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 150;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Status:";

                // Status combo
                oItem = this.UIAPIRawForm.Items.Add("cmbStat", BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = 120;
                oItem.Top = 150;
                oItem.Width = 150;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oCombo = (ComboBox)oItem.Specific;
                oCombo.DataBind.SetBound(true, "DT_HEAD","Status");
                oCombo.ValidValues.Add("D", "Draft");
                oCombo.ValidValues.Add("A", "Active");
                oCombo.ValidValues.Add("C", "Closed");
                oCombo.ValidValues.Add("X", "Cancelled");

                // Retention label
                oItem = this.UIAPIRawForm.Items.Add("stRet", BoFormItemTypes.it_STATIC);
                oItem.Left = 10;
                oItem.Top = 170;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oStatic = (StaticText)oItem.Specific;
                oStatic.Caption = "Retent%:";

                // Retention text
                oItem = this.UIAPIRawForm.Items.Add("txtRet", BoFormItemTypes.it_EDIT);
                oItem.Left = 120;
                oItem.Top = 170;
                oItem.Width = 100;
                oItem.Height = 14;
                oItem.FromPane = 1;
                oItem.ToPane = 1;
                oEdit = (EditText)oItem.Specific;
                oEdit.DataBind.SetBound(true, "DT_HEAD","Retention");

                // === PANE 2: LINES TAB ===

                // Lines Grid
                oItem = this.UIAPIRawForm.Items.Add("grdLines", BoFormItemTypes.it_GRID);
                oItem.Left = 10;
                oItem.Top = 50;
                oItem.Width = 720;
                oItem.Height = 440;
                oItem.FromPane = 2;
                oItem.ToPane = 2;
                oGrid = (Grid)oItem.Specific;
                oGrid.DataTable = this.dtLines;

                // === BUTTONS ===

                // OK Button
                oItem = this.UIAPIRawForm.Items.Add("1", BoFormItemTypes.it_BUTTON);
                oItem.Left = 10;
                oItem.Top = 555;
                oItem.Width = 65;
                oItem.Height = 19;
                oButton = (Button)oItem.Specific;
                oButton.Caption = "OK";

                // Cancel Button
                oItem = this.UIAPIRawForm.Items.Add("2", BoFormItemTypes.it_BUTTON);
                oItem.Left = 80;
                oItem.Top = 555;
                oItem.Width = 65;
                oItem.Height = 19;
                oButton = (Button)oItem.Specific;
                oButton.Caption = "Cancel";

                // Find Button
                oItem = this.UIAPIRawForm.Items.Add("btnFind", BoFormItemTypes.it_BUTTON);
                oItem.Left = 150;
                oItem.Top = 555;
                oItem.Width = 65;
                oItem.Height = 19;
                oButton = (Button)oItem.Specific;
                oButton.Caption = "Find";

                Logger.Info("Form items created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating form items", ex);
                throw;
            }
        }

        /// <summary>
        /// Initialize control references
        /// </summary>
        private void InitializeControlReferences()
        {
            this.stTitle = (StaticText)this.GetItem("stTitle").Specific;
            this.stCode = (StaticText)this.GetItem("stCode").Specific;
            this.txtCode = (EditText)this.GetItem("txtCode").Specific;
            this.stCust = (StaticText)this.GetItem("stCust").Specific;
            this.txtCust = (EditText)this.GetItem("txtCust").Specific;
            this.btnCust = (Button)this.GetItem("btnCust").Specific;
            this.txtCName = (EditText)this.GetItem("txtCName").Specific;
            this.stDesc = (StaticText)this.GetItem("stDesc").Specific;
            this.txtDesc = (EditText)this.GetItem("txtDesc").Specific;
            this.stStart = (StaticText)this.GetItem("stStart").Specific;
            this.txtStart = (EditText)this.GetItem("txtStart").Specific;
            this.stEnd = (StaticText)this.GetItem("stEnd").Specific;
            this.txtEnd = (EditText)this.GetItem("txtEnd").Specific;
            this.stValue = (StaticText)this.GetItem("stValue").Specific;
            this.txtValue = (EditText)this.GetItem("txtValue").Specific;
            this.stStatus = (StaticText)this.GetItem("stStatus").Specific;
            this.cmbStat = (ComboBox)this.GetItem("cmbStat").Specific;
            this.stRet = (StaticText)this.GetItem("stRet").Specific;
            this.txtRet = (EditText)this.GetItem("txtRet").Specific;
            this.grdLines = (Grid)this.GetItem("grdLines").Specific;
            this.btnOK = (Button)this.GetItem("1").Specific;
            this.btnCancel = (Button)this.GetItem("2").Specific;
            this.btnFind = (Button)this.GetItem("btnFind").Specific;
        }

        /// <summary>
        /// Custom initialization
        /// </summary>
        private void OnCustomInitialize()
        {
            try
            {
                Logger.Info("OnCustomInitialize started");

                // Initialize contract service
                SAPbobsCOM.Company company = (SAPbobsCOM.Company)FrameworkApp.SBO_Application.Company.GetDICompany();
                _contractService = new ContractService(company);

                // Set default status to Draft
                if (string.IsNullOrEmpty(cmbStat.Value))
                {
                    cmbStat.Select("D", BoSearchKey.psk_ByValue);
                }

                // Initialize grid - add one empty row
                if (dtLines.Rows.Count == 0)
                {
                    dtLines.Rows.Add();
                }

                // Load new contract
                LoadNewContract();

                Logger.Info("OnCustomInitialize completed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnCustomInitialize", ex);
                FrameworkApp.SBO_Application.StatusBar.SetText($"Error initializing form: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Form click event handler
        /// </summary>
        private void Form_ClickAfter(SBOItemEventArg pVal)
        {
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1": // OK button
                        Logger.Info("OK button clicked - saving contract");
                        SaveContract();
                        break;

                    case "btnFind":
                        Logger.Info("Find button clicked");
                        FindContract();
                        break;

                    case "btnCust":
                        Logger.Info("Customer chooser button clicked");
                        OpenCustomerChooser();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in Form_ClickAfter", ex);
                FrameworkApp.SBO_Application.StatusBar.SetText($"Error: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Load new contract
        /// </summary>
        private void LoadNewContract()
        {
            try
            {
                _currentContract = new ContractModel();
                _currentContract.Code = GetNextContractCode();
                _currentContract.StartDate = DateTime.Now;
                _currentContract.EndDate = DateTime.Now.AddMonths(12);
                _currentContract.Status = "Draft";
                _currentContract.TotalValue = 0;
                _currentContract.RetentionPercentage = 0;

                LoadContractToForm(_currentContract);

                Logger.Info("New contract loaded");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading new contract", ex);
                throw;
            }
        }

        /// <summary>
        /// Load contract data to form
        /// </summary>
        private void LoadContractToForm(ContractModel contract)
        {
            try
            {
                dtHead.Rows.Clear();
                dtHead.Rows.Add();

                dtHead.SetValue("Code", 0, contract.Code ?? "");
                dtHead.SetValue("Customer", 0, contract.CustomerCode ?? "");
                dtHead.SetValue("CustName", 0, "");
                dtHead.SetValue("Descript", 0, contract.Description ?? "");
                dtHead.SetValue("StartDate", 0, contract.StartDate);
                dtHead.SetValue("EndDate", 0, contract.EndDate);
                dtHead.SetValue("Value", 0, contract.TotalValue);
                dtHead.SetValue("Status", 0, contract.Status ?? "D");
                dtHead.SetValue("Retention", 0, contract.RetentionPercentage);

                if (contract.Lines != null && contract.Lines.Count > 0)
                {
                    LoadLinesToGrid(contract.Lines);
                }

                Logger.Info("Contract loaded to form");
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
                dtLines.Rows.Clear();

                int lineNum = 1;
                foreach (var line in lines)
                {
                    int row = dtLines.Rows.Count;
                    dtLines.Rows.Add();
                    dtLines.SetValue("LineNum", row, lineNum.ToString());
                    dtLines.SetValue("ItemCode", row, line.ItemCode ?? "");
                    dtLines.SetValue("Descript", row, line.ItemDescription ?? "");
                    dtLines.SetValue("Quantity", row, line.Quantity);
                    dtLines.SetValue("Price", row, line.UnitPrice);
                    dtLines.SetValue("Total", row, line.LineTotal);
                    lineNum++;
                }

                grdLines.AutoResizeColumns();

                Logger.Info($"Loaded {lines.Count} lines to grid");
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
                ContractModel contract = GetContractFromForm();

                if (string.IsNullOrEmpty(_currentContract?.Code) || _currentContract.Code != contract.Code)
                {
                    _contractService.CreateContract(contract);
                    FrameworkApp.SBO_Application.StatusBar.SetText($"Contract {contract.Code} created successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    _contractService.UpdateContract(contract);
                    FrameworkApp.SBO_Application.StatusBar.SetText($"Contract {contract.Code} updated successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

                _currentContract = contract;

                Logger.Info($"Contract {contract.Code} saved successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving contract", ex);
                FrameworkApp.SBO_Application.StatusBar.SetText($"Error saving contract: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Get contract data from form
        /// </summary>
        private ContractModel GetContractFromForm()
        {
            try
            {
                ContractModel contract = new ContractModel
                {
                    Code = dtHead.GetValue("Code", 0).ToString(),
                    CustomerCode = dtHead.GetValue("Customer", 0).ToString(),
                    Description = dtHead.GetValue("Descript", 0).ToString(),
                    StartDate = (DateTime)dtHead.GetValue("StartDate", 0),
                    EndDate = (DateTime)dtHead.GetValue("EndDate", 0),
                    TotalValue = Convert.ToDouble(dtHead.GetValue("Value", 0)),
                    RetentionPercentage = Convert.ToDouble(dtHead.GetValue("Retention", 0)),
                    Status = dtHead.GetValue("Status", 0).ToString()
                };

                contract.Lines = new System.Collections.Generic.List<ContractLine>();
                for (int i = 0; i < dtLines.Rows.Count; i++)
                {
                    string itemCode = dtLines.GetValue("ItemCode", i)?.ToString();
                    if (!string.IsNullOrEmpty(itemCode))
                    {
                        ContractLine line = new ContractLine
                        {
                            ItemCode = itemCode,
                            ItemDescription = dtLines.GetValue("Descript", i)?.ToString() ?? "",
                            Quantity = Convert.ToDouble(dtLines.GetValue("Quantity", i)),
                            UnitPrice = Convert.ToDouble(dtLines.GetValue("Price", i)),
                            LineTotal = Convert.ToDouble(dtLines.GetValue("Total", i))
                        };
                        contract.Lines.Add(line);
                    }
                }

                Logger.Info($"Retrieved contract data from form: {contract.Code}");
                return contract;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting contract from form", ex);
                throw;
            }
        }

        /// <summary>
        /// Find contract
        /// </summary>
        private void FindContract()
        {
            FrameworkApp.SBO_Application.StatusBar.SetText("Find function - to be implemented",
                BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            Logger.Info("Find function called - to be implemented");
        }

        /// <summary>
        /// Open customer chooser
        /// </summary>
        private void OpenCustomerChooser()
        {
            try
            {
                FrameworkApp.SBO_Application.ActivateMenuItem("4883");
                Logger.Info("Customer chooser opened");
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening customer chooser", ex);
                FrameworkApp.SBO_Application.StatusBar.SetText("Customer chooser - to be implemented",
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
    }
}
