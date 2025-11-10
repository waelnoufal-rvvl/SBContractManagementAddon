using System;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ContractManagementAddon.Core;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Forms
{
    /// <summary>
    /// Contract management form using SAP B1 SDK Framework pattern
    /// </summary>
    [FormAttribute("ContractManagementAddon.Forms.ContractForm", "Forms/ContractForm.b1f")]
    class ContractForm : UserFormBase
    {
        private ContractService _contractService;
        private Contract _currentContract;

        // Control declarations (from ContractForm.b1f)
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

        /// <summary>
        /// Parameterless constructor required by Framework
        /// </summary>
        public ContractForm()
        {
        }

        /// <summary>
        /// Initialize component - called by Framework after form is loaded
        /// </summary>
        public override void OnInitializeComponent()
        {
            try
            {
                Logger.Info("OnInitializeComponent started");

                // Get controls from framework-loaded form
                this.stTitle = ((StaticText)(this.GetItem("stTitle").Specific));
                this.stCode = ((StaticText)(this.GetItem("stCode").Specific));
                this.txtCode = ((EditText)(this.GetItem("txtCode").Specific));
                this.stCust = ((StaticText)(this.GetItem("stCust").Specific));
                this.txtCust = ((EditText)(this.GetItem("txtCust").Specific));
                this.btnCust = ((Button)(this.GetItem("btnCust").Specific));
                this.txtCName = ((EditText)(this.GetItem("txtCName").Specific));
                this.stDesc = ((StaticText)(this.GetItem("stDesc").Specific));
                this.txtDesc = ((EditText)(this.GetItem("txtDesc").Specific));
                this.stStart = ((StaticText)(this.GetItem("stStart").Specific));
                this.txtStart = ((EditText)(this.GetItem("txtStart").Specific));
                this.stEnd = ((StaticText)(this.GetItem("stEnd").Specific));
                this.txtEnd = ((EditText)(this.GetItem("txtEnd").Specific));
                this.stValue = ((StaticText)(this.GetItem("stValue").Specific));
                this.txtValue = ((EditText)(this.GetItem("txtValue").Specific));
                this.stStatus = ((StaticText)(this.GetItem("stStatus").Specific));
                this.cmbStat = ((ComboBox)(this.GetItem("cmbStat").Specific));
                this.stRet = ((StaticText)(this.GetItem("stRet").Specific));
                this.txtRet = ((EditText)(this.GetItem("txtRet").Specific));
                this.grdLines = ((Grid)(this.GetItem("grdLines").Specific));
                this.btnOK = ((Button)(this.GetItem("1").Specific));
                this.btnCancel = ((Button)(this.GetItem("2").Specific));
                this.btnFind = ((Button)(this.GetItem("btnFind").Specific));

                // Get data sources
                this.dtHead = this.UIAPIRawForm.DataSources.DataTables.Item("DT_HEAD");
                this.dtLines = this.UIAPIRawForm.DataSources.DataTables.Item("DT_LINES");

                // Attach event handlers
                this.btnOK.ClickBefore += new _IButtonEvents_ClickBeforeEventHandler(this.btnOK_ClickBefore);
                this.btnFind.ClickBefore += new _IButtonEvents_ClickBeforeEventHandler(this.btnFind_ClickBefore);
                this.btnCust.ClickBefore += new _IButtonEvents_ClickBeforeEventHandler(this.btnCust_ClickBefore);

                Logger.Info("OnInitializeComponent completed");

                // Call custom initialization
                this.OnCustomInitialize();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in OnInitializeComponent", ex);
                throw;
            }
        }

        /// <summary>
        /// Custom initialization logic
        /// </summary>
        private void OnCustomInitialize()
        {
            try
            {
                Logger.Info("OnCustomInitialize started");

                // Initialize contract service
                // Note: We need access to Company object - will need to refactor Application access
                SAPbobsCOM.Company company = (SAPbobsCOM.Company)SAPbouiCOM.Framework.SAPbouiCOM.Framework.Application.SBO_Application.Company.GetDICompany();
                _contractService = new ContractService(company);

                // Set default status to Draft if empty
                if (string.IsNullOrEmpty(cmbStat.Value))
                {
                    cmbStat.Select("D", BoSearchKey.psk_ByValue);
                }

                // Initialize grid - add one empty row if needed
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
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Error initializing form: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// OK button click handler
        /// </summary>
        private void btnOK_ClickBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                Logger.Info("OK button clicked - saving contract");
                SaveContract();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in btnOK_ClickBefore", ex);
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Error: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }

        /// <summary>
        /// Find button click handler
        /// </summary>
        private void btnFind_ClickBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                Logger.Info("Find button clicked");
                FindContract();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in btnFind_ClickBefore", ex);
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Error: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }

        /// <summary>
        /// Customer chooser button click handler
        /// </summary>
        private void btnCust_ClickBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

            try
            {
                Logger.Info("Customer chooser button clicked");
                OpenCustomerChooser();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in btnCust_ClickBefore", ex);
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Error: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
            }
        }

        /// <summary>
        /// Load new contract
        /// </summary>
        private void LoadNewContract()
        {
            try
            {
                _currentContract = new Contract();
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
        private void LoadContractToForm(Contract contract)
        {
            try
            {
                // Load header data using DataTable
                dtHead.Rows.Clear();
                dtHead.Rows.Add();

                dtHead.SetValue("Code", 0, contract.Code ?? "");
                dtHead.SetValue("Customer", 0, contract.CustomerCode ?? "");
                dtHead.SetValue("CustName", 0, ""); // Will be populated by customer chooser
                dtHead.SetValue("Descript", 0, contract.Description ?? "");
                dtHead.SetValue("StartDate", 0, contract.StartDate);
                dtHead.SetValue("EndDate", 0, contract.EndDate);
                dtHead.SetValue("Value", 0, contract.TotalValue);
                dtHead.SetValue("Status", 0, contract.Status ?? "D");
                dtHead.SetValue("Retention", 0, contract.RetentionPercentage);

                // Load lines if any
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
                // Get data from form
                Contract contract = GetContractFromForm();

                // Save
                if (string.IsNullOrEmpty(_currentContract?.Code) || _currentContract.Code != contract.Code)
                {
                    // Create new
                    _contractService.CreateContract(contract);
                    SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Contract {contract.Code} created successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }
                else
                {
                    // Update existing
                    _contractService.UpdateContract(contract);
                    SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Contract {contract.Code} updated successfully",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

                _currentContract = contract;

                Logger.Info($"Contract {contract.Code} saved successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving contract", ex);
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText($"Error saving contract: {ex.Message}",
                    BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        /// <summary>
        /// Get contract data from form
        /// </summary>
        private Contract GetContractFromForm()
        {
            try
            {
                Contract contract = new Contract
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

                // Get lines from grid
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
            // In production, implement search dialog
            SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText("Find function - to be implemented",
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
                // Open SAP B1 Business Partner chooser
                SAPbouiCOM.Framework.Application.SBO_Application.ActivateMenuItem("4883"); // Business Partner master data menu
                Logger.Info("Customer chooser opened");
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening customer chooser", ex);
                SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetText("Customer chooser - to be implemented",
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
