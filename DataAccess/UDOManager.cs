using System;
using SAPbobsCOM;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
    /// <summary>
    /// Manages User Defined Objects (UDO) creation and registration
    /// </summary>
    public class UDOManager
    {
        private Company _company;

        public UDOManager(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        /// <summary>
        /// Create all UDOs required for the add-on
        /// </summary>
        public void CreateUDOs()
        {
            try
            {
                Logger.Info("Creating User Defined Objects...");

                // Create user tables and fields
                CreateContractTable();
                CreateIPCTable();
                CreateChangeOrderTable();

                // Create approval workflow tables
                CreateApprovalWorkflowTables();

                // Create user permissions table
                CreateUserPermissionsTable();

                // Create audit log table
                CreateAuditLogTable();

                // PHASE 1: Create multi-currency tables
                CreateCurrencyTables();
                CreateExchangeRateTable();
                CreateCurrencyConversionLogTable();

                // PHASE 1: Create revenue recognition tables
                CreatePerformanceObligationTable();
                CreateRevenueScheduleTable();
                CreateDeferredRevenueTable();
                CreateContractAssetsTable();
                CreateRevenueLogTable();
                CreateBacklogTable();

                // Register UDOs
                RegisterContractUDO();
                RegisterIPCUDO();
                RegisterChangeOrderUDO();

                // PHASE 1: Register currency UDOs
                RegisterCurrencyUDO();
                RegisterExchangeRateUDO();

                // PHASE 1: Register revenue recognition UDOs
                RegisterPerformanceObligationUDO();
                RegisterRevenueScheduleUDO();
                RegisterDeferredRevenueUDO();

                Logger.Info("User Defined Objects created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create UDOs: " + ex.Message, ex);
                throw;
            }
            
        }

        /// <summary>
        /// Create Contract user table
        /// </summary>
        private void CreateContractTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Contract table...");

                // Check if table already exists
                if (DatabaseHelper.UserTableExists(_company, "CONTRACT_HDR"))
                {
                    Logger.Info("Contract table already exists");
                }
                else
                {
                    // Create header table
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CONTRACT_HDR";
                    userTable.TableDescription = "Contract Header";
                    userTable.TableType = BoUTBTableType.bott_MasterData;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Contract header table created");
                }

                // Create lines table
                if (!DatabaseHelper.UserTableExists(_company, "CONTRACT_LNS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CONTRACT_LNS";
                    userTable.TableDescription = "Contract Lines";
                    userTable.TableType = BoUTBTableType.bott_MasterDataLines;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Contract lines table created");
                }
                else
                {
                    Logger.Info("Contract lines table already exists");
                }

                // Create fields for header
                CreateContractFields();

                Logger.Info("Contract table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Contract table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Contract user fields
        /// </summary>
        private void CreateContractFields()
        {
            // Header fields
            AddUserField("CONTRACT_HDR", "CustomerCode", "Customer Code", BoFieldTypes.db_Alpha, 15);
            AddUserField("CONTRACT_HDR", "CustomerName", "Customer Name", BoFieldTypes.db_Alpha, 100);
            AddUserField("CONTRACT_HDR", "ProjectCode", "Project Code", BoFieldTypes.db_Alpha, 20);
            AddUserField("CONTRACT_HDR", "ProjectName", "Project Name", BoFieldTypes.db_Alpha, 100);
            AddUserField("CONTRACT_HDR", "Description", "Description", BoFieldTypes.db_Alpha, 254);
            AddUserField("CONTRACT_HDR", "StartDate", "Start Date", BoFieldTypes.db_Date);
            AddUserField("CONTRACT_HDR", "EndDate", "End Date", BoFieldTypes.db_Date);
            AddUserField("CONTRACT_HDR", "Status", "Status", BoFieldTypes.db_Alpha, 20);
            AddUserField("CONTRACT_HDR", "TotalValue", "Total Value", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
            AddUserField("CONTRACT_HDR", "Currency", "Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("CONTRACT_HDR", "RetentionPct", "Retention %", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Percentage);
            AddUserField("CONTRACT_HDR", "PaymentTerms", "Payment Terms", BoFieldTypes.db_Alpha, 100);
            AddUserField("CONTRACT_HDR", "ContractMgr", "Contract Manager", BoFieldTypes.db_Alpha, 50);
            AddUserField("CONTRACT_HDR", "Remarks", "Remarks", BoFieldTypes.db_Memo);
            AddUserField("CONTRACT_HDR", "AtcEntry", "Attachment Entry", BoFieldTypes.db_Numeric);

            // PHASE 1: Multi-Currency fields
            AddUserField("CONTRACT_HDR", "BaseCurrency", "Base Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("CONTRACT_HDR", "ExchangeRate", "Exchange Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Rate);
            AddUserField("CONTRACT_HDR", "BaseCurrValue", "Base Currency Value", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CONTRACT_HDR", "FXGainLoss", "FX Gain/Loss", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CONTRACT_HDR", "LastFXUpdate", "Last FX Update Date", BoFieldTypes.db_Date);

            // Line fields
            AddUserField("CONTRACT_LNS", "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 20);
            AddUserField("CONTRACT_LNS", "ItemDesc", "Item Description", BoFieldTypes.db_Alpha, 100);
            AddUserField("CONTRACT_LNS", "Quantity", "Quantity", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Quantity);
            AddUserField("CONTRACT_LNS", "UoM", "Unit of Measure", BoFieldTypes.db_Alpha, 10);
            AddUserField("CONTRACT_LNS", "UnitPrice", "Unit Price", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
            AddUserField("CONTRACT_LNS", "LineTotal", "Line Total", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CONTRACT_LNS", "AccountCode", "Account Code", BoFieldTypes.db_Alpha, 15);
            AddUserField("CONTRACT_LNS", "CostCenter", "Cost Center", BoFieldTypes.db_Alpha, 15);
            AddUserField("CONTRACT_LNS", "Remarks", "Remarks", BoFieldTypes.db_Memo);
        }

        /// <summary>
        /// Create IPC user table
        /// </summary>
        private void CreateIPCTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating IPC table...");

                if (DatabaseHelper.UserTableExists(_company, "IPC_HDR"))
                {
                    Logger.Info("IPC table already exists");
                }
                else
                {
                    // Create header table
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "IPC_HDR";
                    userTable.TableDescription = "IPC Header";
                    userTable.TableType = BoUTBTableType.bott_Document;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("IPC header table created");
                }

                // Create lines table
                if (!DatabaseHelper.UserTableExists(_company, "IPC_LNS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "IPC_LNS";
                    userTable.TableDescription = "IPC Lines";
                    userTable.TableType = BoUTBTableType.bott_DocumentLines;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("IPC lines table created");
                }
                else
                {
                    Logger.Info("IPC lines table already exists");
                }

                CreateIPCFields();

                Logger.Info("IPC table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating IPC table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create IPC user fields
        /// </summary>
        private void CreateIPCFields()
        {
            // Header fields
            AddUserField("IPC_HDR", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
            AddUserField("IPC_HDR", "IPCNumber", "IPC Number", BoFieldTypes.db_Numeric);
            AddUserField("IPC_HDR", "IPCDate", "IPC Date", BoFieldTypes.db_Date);
            AddUserField("IPC_HDR", "Period", "Period", BoFieldTypes.db_Alpha, 50);
            AddUserField("IPC_HDR", "GrossAmount", "Gross Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "RetentionAmt", "Retention Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "NetAmount", "Net Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "PrevIPCTotal", "Previous IPC Total", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "CurrentAmt", "Current Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "Status", "Status", BoFieldTypes.db_Alpha, 20);
            AddUserField("IPC_HDR", "ApprovedBy", "Approved By", BoFieldTypes.db_Alpha, 50);
            AddUserField("IPC_HDR", "ApprovedDate", "Approved Date", BoFieldTypes.db_Date);
            AddUserField("IPC_HDR", "Remarks", "Remarks", BoFieldTypes.db_Memo);
            AddUserField("IPC_HDR", "ARInvDocEntry", "AR Invoice DocEntry", BoFieldTypes.db_Numeric);
            AddUserField("IPC_HDR", "AtcEntry", "Attachment Entry", BoFieldTypes.db_Numeric);

            // PHASE 1: Multi-Currency fields
            AddUserField("IPC_HDR", "Currency", "Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("IPC_HDR", "BaseCurrency", "Base Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("IPC_HDR", "ExchangeRate", "Exchange Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Rate);
            AddUserField("IPC_HDR", "BaseCurrGross", "Base Currency Gross Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "BaseCurrNet", "Base Currency Net Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "FXGainLoss", "FX Gain/Loss", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_HDR", "LastFXUpdate", "Last FX Update Date", BoFieldTypes.db_Date);

            // Line fields
            AddUserField("IPC_LNS", "Description", "Description", BoFieldTypes.db_Alpha, 254);
            AddUserField("IPC_LNS", "Quantity", "Quantity", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Quantity);
            AddUserField("IPC_LNS", "UnitPrice", "Unit Price", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
            AddUserField("IPC_LNS", "Amount", "Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("IPC_LNS", "CompletionPct", "Completion %", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Percentage);
            AddUserField("IPC_LNS", "Remarks", "Remarks", BoFieldTypes.db_Memo);
        }

        /// <summary>
        /// Create Change Order user table
        /// </summary>
        private void CreateChangeOrderTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Change Order table...");

                if (DatabaseHelper.UserTableExists(_company, "CO_HDR"))
                {
                    Logger.Info("Change Order table already exists");
                }
                else
                {
                    // Create header table
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CO_HDR";
                    userTable.TableDescription = "Change Order Header";
                    userTable.TableType = BoUTBTableType.bott_Document;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Change Order header table created");
                }

                // Create lines table
                if (!DatabaseHelper.UserTableExists(_company, "CO_LNS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CO_LNS";
                    userTable.TableDescription = "Change Order Lines";
                    userTable.TableType = BoUTBTableType.bott_DocumentLines;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Change Order lines table created");
                }
                else
                {
                    Logger.Info("Change Order lines table already exists");
                }

                CreateChangeOrderFields();

                Logger.Info("Change Order table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Change Order table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Change Order user fields
        /// </summary>
        private void CreateChangeOrderFields()
        {
            // Header fields
            AddUserField("CO_HDR", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
            AddUserField("CO_HDR", "CONumber", "CO Number", BoFieldTypes.db_Numeric);
            AddUserField("CO_HDR", "CODate", "CO Date", BoFieldTypes.db_Date);
            AddUserField("CO_HDR", "Type", "Type", BoFieldTypes.db_Alpha, 20);
            AddUserField("CO_HDR", "Description", "Description", BoFieldTypes.db_Alpha, 254);
            AddUserField("CO_HDR", "Amount", "Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CO_HDR", "AddDays", "Additional Days", BoFieldTypes.db_Numeric);
            AddUserField("CO_HDR", "Status", "Status", BoFieldTypes.db_Alpha, 20);
            AddUserField("CO_HDR", "RequestedBy", "Requested By", BoFieldTypes.db_Alpha, 50);
            AddUserField("CO_HDR", "ApprovedBy", "Approved By", BoFieldTypes.db_Alpha, 50);
            AddUserField("CO_HDR", "ApprovedDate", "Approved Date", BoFieldTypes.db_Date);
            AddUserField("CO_HDR", "Justification", "Justification", BoFieldTypes.db_Memo);
            AddUserField("CO_HDR", "Remarks", "Remarks", BoFieldTypes.db_Memo);
            AddUserField("CO_HDR", "AtcEntry", "Attachment Entry", BoFieldTypes.db_Numeric);

            // PHASE 1: Multi-Currency fields
            AddUserField("CO_HDR", "Currency", "Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("CO_HDR", "BaseCurrency", "Base Currency", BoFieldTypes.db_Alpha, 3);
            AddUserField("CO_HDR", "ExchangeRate", "Exchange Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Rate);
            AddUserField("CO_HDR", "BaseCurrAmt", "Base Currency Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CO_HDR", "FXGainLoss", "FX Gain/Loss", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CO_HDR", "LastFXUpdate", "Last FX Update Date", BoFieldTypes.db_Date);

            // Line fields
            AddUserField("CO_LNS", "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 20);
            AddUserField("CO_LNS", "Description", "Description", BoFieldTypes.db_Alpha, 254);
            AddUserField("CO_LNS", "Quantity", "Quantity", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Quantity);
            AddUserField("CO_LNS", "UoM", "Unit of Measure", BoFieldTypes.db_Alpha, 10);
            AddUserField("CO_LNS", "UnitPrice", "Unit Price", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
            AddUserField("CO_LNS", "LineTotal", "Line Total", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
            AddUserField("CO_LNS", "Remarks", "Remarks", BoFieldTypes.db_Memo);
        }

        /// <summary>
        /// Add user field to table
        /// </summary>
        private void AddUserField(string tableName, string fieldName, string description,
            BoFieldTypes fieldType, int size = 0, BoFldSubTypes subType = BoFldSubTypes.st_None)
        {
            UserFieldsMD userField = null;

            try
            {
                // Check if field already exists
                if (DatabaseHelper.UserFieldExists(_company, tableName, fieldName))
                {
                    Logger.Debug($"Field {tableName}.{fieldName} already exists");
                    return;
                }

                userField = (UserFieldsMD)_company.GetBusinessObject(BoObjectTypes.oUserFields);
                userField.TableName = tableName;
                userField.Name = fieldName;
                userField.Description = description;
                userField.Type = fieldType;

                if (size > 0)
                {
                    userField.Size = size;
                }

                if (subType != BoFldSubTypes.st_None)
                {
                    userField.SubType = subType;
                }

                if (userField.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Debug($"Field {tableName}.{fieldName} created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating field {tableName}.{fieldName}: " + ex.Message, ex);
                throw;
            }
            finally
            {
                // Always release COM object to prevent reference count issues
                if (userField != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userField);
                }
            }
        }

        /// <summary>
        /// Register Contract UDO
        /// </summary>
        private void RegisterContractUDO()
        {
            try
            {
                Logger.Info("Registering Contract UDO...");

                // Check if UDO already exists
                if (UDOExists("CONTRACT"))
                {
                    Logger.Info("Contract UDO already registered");
                    return;
                }

                UserObjectsMD udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                udo.Code = "CONTRACT";
                udo.Name = "Contract";
                udo.ObjectType = BoUDOObjType.boud_MasterData;
                udo.TableName = "CONTRACT_HDR";

                // Add child table
                udo.ChildTables.TableName = "CONTRACT_LNS";
                udo.ChildTables.Add();

                // Find columns
                udo.FindColumns.ColumnAlias = "Code";
                udo.FindColumns.ColumnDescription = "Contract Code";
                udo.FindColumns.Add();

                if (udo.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Contract UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Contract UDO: " + ex.Message, ex);
                throw;
            }
            
        }

        /// <summary>
        /// Register IPC UDO
        /// </summary>
        private void RegisterIPCUDO()
        {
            try
            {
                Logger.Info("Registering IPC UDO...");

                if (UDOExists("IPC"))
                {
                    Logger.Info("IPC UDO already registered");
                    return;
                }

                UserObjectsMD udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                udo.Code = "IPC";
                udo.Name = "Interim Payment Certificate";
                udo.ObjectType = BoUDOObjType.boud_Document;
                udo.TableName = "IPC_HDR";

                udo.ChildTables.TableName = "IPC_LNS";
                udo.ChildTables.Add();

                udo.FindColumns.ColumnAlias = "DocNum";
                udo.FindColumns.ColumnDescription = "Document Number";
                udo.FindColumns.Add();

                if (udo.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("IPC UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering IPC UDO: " + ex.Message, ex);
                throw;
            }
            
        }

        /// <summary>
        /// Register Change Order UDO
        /// </summary>
        private void RegisterChangeOrderUDO()
        {
            try
            {
                Logger.Info("Registering Change Order UDO...");

                if (UDOExists("CHANGEORDER"))
                {
                    Logger.Info("Change Order UDO already registered");
                    return;
                }

                UserObjectsMD udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                udo.Code = "CHANGEORDER";
                udo.Name = "Change Order";
                udo.ObjectType = BoUDOObjType.boud_Document;
                udo.TableName = "CO_HDR";

                udo.ChildTables.TableName = "CO_LNS";
                udo.ChildTables.Add();

                udo.FindColumns.ColumnAlias = "DocNum";
                udo.FindColumns.ColumnDescription = "Document Number";
                udo.FindColumns.Add();

                if (udo.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Change Order UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Change Order UDO: " + ex.Message, ex);
                throw;
            }
            
        }

        /// <summary>
        /// Check if UDO exists
        /// </summary>
        private bool UDOExists(string code)
        {
            try
            {
                UserObjectsMD udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                return udo.GetByKey(code);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create approval workflow tables
        /// </summary>
        private void CreateApprovalWorkflowTables()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating approval workflow tables...");

                // Approval Request table
                if (!DatabaseHelper.UserTableExists(_company, "CM_APPR_REQ"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_APPR_REQ";
                    userTable.TableDescription = "Approval Requests";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    AddUserField("CM_APPR_REQ", "DocType", "Document Type", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_APPR_REQ", "DocCode", "Document Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_APPR_REQ", "Amount", "Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_APPR_REQ", "Status", "Status", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_APPR_REQ", "RequestDate", "Request Date", BoFieldTypes.db_Date);
                    AddUserField("CM_APPR_REQ", "RequestedBy", "Requested By", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_APPR_REQ", "CurrentStage", "Current Stage", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_REQ", "Remarks", "Remarks", BoFieldTypes.db_Memo);
                }

                // Approval Stages table
                if (!DatabaseHelper.UserTableExists(_company, "CM_APPR_STAG"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_APPR_STAG";
                    userTable.TableDescription = "Approval Stages";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    AddUserField("CM_APPR_STAG", "RequestID", "Request ID", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_STAG", "StageNum", "Stage Number", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_STAG", "StageName", "Stage Name", BoFieldTypes.db_Alpha, 100);
                    AddUserField("CM_APPR_STAG", "ApprRole", "Approver Role", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_APPR_STAG", "ReqApprov", "Required Approvals", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_STAG", "Status", "Status", BoFieldTypes.db_Alpha, 20);
                }

                // Approval Details table
                if (!DatabaseHelper.UserTableExists(_company, "CM_APPR_DTLS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_APPR_DTLS";
                    userTable.TableDescription = "Approval Details";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    AddUserField("CM_APPR_DTLS", "RequestID", "Request ID", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_DTLS", "StageNum", "Stage Number", BoFieldTypes.db_Numeric);
                    AddUserField("CM_APPR_DTLS", "Approver", "Approver", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_APPR_DTLS", "Decision", "Decision", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_APPR_DTLS", "Comments", "Comments", BoFieldTypes.db_Memo);
                    AddUserField("CM_APPR_DTLS", "ApprDate", "Approval Date", BoFieldTypes.db_Date);
                }

                Logger.Info("Approval workflow tables created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating approval workflow tables: " + ex.Message, ex);
                // Don't throw - these are optional tables
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create user permissions table
        /// </summary>
        private void CreateUserPermissionsTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating user permissions table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_USER_PERM"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_USER_PERM";
                    userTable.TableDescription = "User Permissions";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    AddUserField("CM_USER_PERM", "UserCode", "User Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_USER_PERM", "PermissionID", "Permission ID", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_USER_PERM", "HasPermission", "Has Permission", BoFieldTypes.db_Alpha, 1);
                    AddUserField("CM_USER_PERM", "ApprovalLimit", "Approval Limit", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                }

                Logger.Info("User permissions table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating user permissions table: " + ex.Message, ex);
                // Don't throw - this is an optional table
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create audit log table
        /// </summary>
        private void CreateAuditLogTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating audit log table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_AUDIT_LOG"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_AUDIT_LOG";
                    userTable.TableDescription = "Audit Log";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    AddUserField("CM_AUDIT_LOG", "UserCode", "User Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_AUDIT_LOG", "Action", "Action", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_AUDIT_LOG", "ResourceType", "Resource Type", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_AUDIT_LOG", "ResourceID", "Resource ID", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_AUDIT_LOG", "Granted", "Granted", BoFieldTypes.db_Alpha, 1);
                    AddUserField("CM_AUDIT_LOG", "LogDate", "Log Date", BoFieldTypes.db_Date);
                }

                Logger.Info("Audit log table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating audit log table: " + ex.Message, ex);
                // Don't throw - this is an optional table
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        #region Phase 1: Multi-Currency Tables

        /// <summary>
        /// Create Currency Master table
        /// </summary>
        private void CreateCurrencyTables()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Currency Master table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_CURRENCY"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_CURRENCY";
                    userTable.TableDescription = "Currency Master";
                    userTable.TableType = BoUTBTableType.bott_MasterData;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_CURRENCY", "Name", "Currency Name", BoFieldTypes.db_Alpha, 100);
                    AddUserField("CM_CURRENCY", "Symbol", "Currency Symbol", BoFieldTypes.db_Alpha, 10);
                    AddUserField("CM_CURRENCY", "DecimalPlaces", "Decimal Places", BoFieldTypes.db_Numeric, 0);
                    AddUserField("CM_CURRENCY", "IsActive", "Is Active", BoFieldTypes.db_Alpha, 1);
                    AddUserField("CM_CURRENCY", "IsBaseCurrency", "Is Base Currency", BoFieldTypes.db_Alpha, 1);
                    AddUserField("CM_CURRENCY", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_CURRENCY", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_CURRENCY", "ModifyDate", "Modify Date", BoFieldTypes.db_Date);
                    AddUserField("CM_CURRENCY", "ModifyUser", "Modify User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Currency Master table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating currency table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Exchange Rate table
        /// </summary>
        private void CreateExchangeRateTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Exchange Rate table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_EXCHANGE_RATE"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_EXCHANGE_RATE";
                    userTable.TableDescription = "Exchange Rate";
                    userTable.TableType = BoUTBTableType.bott_MasterData;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_EXCHANGE_RATE", "FromCurrency", "From Currency", BoFieldTypes.db_Alpha, 3);
                    AddUserField("CM_EXCHANGE_RATE", "ToCurrency", "To Currency", BoFieldTypes.db_Alpha, 3);
                    AddUserField("CM_EXCHANGE_RATE", "RateDate", "Rate Date", BoFieldTypes.db_Date);
                    AddUserField("CM_EXCHANGE_RATE", "Rate", "Exchange Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Rate);
                    AddUserField("CM_EXCHANGE_RATE", "RateSource", "Rate Source", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_EXCHANGE_RATE", "IsActive", "Is Active", BoFieldTypes.db_Alpha, 1);
                    AddUserField("CM_EXCHANGE_RATE", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_EXCHANGE_RATE", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Exchange Rate table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating exchange rate table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Currency Conversion Log table
        /// </summary>
        private void CreateCurrencyConversionLogTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Currency Conversion Log table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_CURRENCY_LOG"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_CURRENCY_LOG";
                    userTable.TableDescription = "Currency Conversion Log";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_CURRENCY_LOG", "DocumentType", "Document Type", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_CURRENCY_LOG", "DocumentCode", "Document Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_CURRENCY_LOG", "OriginalCurrency", "Original Currency", BoFieldTypes.db_Alpha, 3);
                    AddUserField("CM_CURRENCY_LOG", "OriginalAmount", "Original Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CURRENCY_LOG", "TargetCurrency", "Target Currency", BoFieldTypes.db_Alpha, 3);
                    AddUserField("CM_CURRENCY_LOG", "ConvertedAmount", "Converted Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CURRENCY_LOG", "ExchangeRate", "Exchange Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Rate);
                    AddUserField("CM_CURRENCY_LOG", "ConversionDate", "Conversion Date", BoFieldTypes.db_Date);
                    AddUserField("CM_CURRENCY_LOG", "ConversionUser", "Conversion User", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_CURRENCY_LOG", "ConversionReason", "Conversion Reason", BoFieldTypes.db_Memo);
                }

                Logger.Info("Currency Conversion Log table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating currency conversion log table: " + ex.Message, ex);
                // Don't throw - this is an optional table
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Register Currency UDO
        /// </summary>
        private void RegisterCurrencyUDO()
        {
            try
            {
                Logger.Info("Registering Currency UDO...");

                // Check if UDO already exists
                if (UDOExists("CM_CURRENCY"))
                {
                    Logger.Info("Currency UDO already registered");
                    return;
                }

                UserObjectsMD userObject = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                userObject.Code = "CM_CURRENCY";
                userObject.Name = "Currency Master";
                userObject.ObjectType = BoUDOObjType.boud_MasterData;
                userObject.TableName = "CM_CURRENCY";
                userObject.CanCancel = BoYesNoEnum.tNO;
                userObject.CanClose = BoYesNoEnum.tNO;
                userObject.CanDelete = BoYesNoEnum.tYES;
                userObject.CanCreateDefaultForm = BoYesNoEnum.tNO;
                userObject.CanFind = BoYesNoEnum.tYES;

                if (userObject.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Currency UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Currency UDO: " + ex.Message, ex);
                // Don't throw - UDO registration is optional
            }
        }

        /// <summary>
        /// Register Exchange Rate UDO
        /// </summary>
        private void RegisterExchangeRateUDO()
        {
            try
            {
                Logger.Info("Registering Exchange Rate UDO...");

                // Check if UDO already exists
                if (UDOExists("CM_EXCHANGE_RATE"))
                {
                    Logger.Info("Exchange Rate UDO already registered");
                    return;
                }

                UserObjectsMD userObject = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                userObject.Code = "CM_EXCHANGE_RATE";
                userObject.Name = "Exchange Rate";
                userObject.ObjectType = BoUDOObjType.boud_MasterData;
                userObject.TableName = "CM_EXCHANGE_RATE";
                userObject.CanCancel = BoYesNoEnum.tNO;
                userObject.CanClose = BoYesNoEnum.tNO;
                userObject.CanDelete = BoYesNoEnum.tYES;
                userObject.CanCreateDefaultForm = BoYesNoEnum.tNO;
                userObject.CanFind = BoYesNoEnum.tYES;

                if (userObject.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Exchange Rate UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Exchange Rate UDO: " + ex.Message, ex);
                // Don't throw - UDO registration is optional
            }
        }

        #endregion

        #region Phase 1: Revenue Recognition Tables

        /// <summary>
        /// Create Performance Obligation table with lines
        /// </summary>
        private void CreatePerformanceObligationTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Performance Obligation table...");

                // Create header table
                if (!DatabaseHelper.UserTableExists(_company, "CM_PERF_OBL"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_PERF_OBL";
                    userTable.TableDescription = "Performance Obligations";
                    userTable.TableType = BoUTBTableType.bott_MasterData;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Performance Obligation header table created");

                    // Add header fields
                    AddUserField("CM_PERF_OBL", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_PERF_OBL", "OblNumber", "Obligation Number", BoFieldTypes.db_Numeric);
                    AddUserField("CM_PERF_OBL", "Description", "Description", BoFieldTypes.db_Memo);
                    AddUserField("CM_PERF_OBL", "Type", "Type", BoFieldTypes.db_Alpha, 20); // Good, Service, Bundle
                    AddUserField("CM_PERF_OBL", "StandaloneSP", "Standalone Selling Price", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
                    AddUserField("CM_PERF_OBL", "AllocatedPrice", "Allocated Price", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Price);
                    AddUserField("CM_PERF_OBL", "RecogMethod", "Recognition Method", BoFieldTypes.db_Alpha, 20); // PointInTime, OverTime
                    AddUserField("CM_PERF_OBL", "ProgressMethod", "Progress Method", BoFieldTypes.db_Alpha, 20); // CostToCost, UnitsDelivered, Milestone, Time
                    AddUserField("CM_PERF_OBL", "TotalEstCost", "Total Estimated Cost", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_PERF_OBL", "TotalUnits", "Total Units", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Quantity);
                    AddUserField("CM_PERF_OBL", "Status", "Status", BoFieldTypes.db_Alpha, 20); // NotStarted, InProgress, Completed
                    AddUserField("CM_PERF_OBL", "CompletionPct", "Completion Percentage", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Percentage);
                    AddUserField("CM_PERF_OBL", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_PERF_OBL", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }
                else
                {
                    Logger.Info("Performance Obligation header table already exists");
                }

                // Create lines table
                if (!DatabaseHelper.UserTableExists(_company, "CM_PERF_OBL_LNS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_PERF_OBL_LNS";
                    userTable.TableDescription = "Performance Obligation Lines";
                    userTable.TableType = BoUTBTableType.bott_MasterDataLines;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    Logger.Info("Performance Obligation lines table created");

                    // Add line fields
                    AddUserField("CM_PERF_OBL_LNS", "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_PERF_OBL_LNS", "Description", "Description", BoFieldTypes.db_Alpha, 254);
                    AddUserField("CM_PERF_OBL_LNS", "Quantity", "Quantity", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Quantity);
                    AddUserField("CM_PERF_OBL_LNS", "EstimatedCost", "Estimated Cost", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_PERF_OBL_LNS", "ActualCost", "Actual Cost", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_PERF_OBL_LNS", "CompletionPct", "Completion %", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Percentage);
                }
                else
                {
                    Logger.Info("Performance Obligation lines table already exists");
                }

                Logger.Info("Performance Obligation table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Performance Obligation table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Revenue Recognition Schedule table
        /// </summary>
        private void CreateRevenueScheduleTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Revenue Recognition Schedule table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_REV_SCHEDULE"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_REV_SCHEDULE";
                    userTable.TableDescription = "Revenue Recognition Schedule";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_REV_SCHEDULE", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_REV_SCHEDULE", "PerfOblCode", "Performance Obligation Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_REV_SCHEDULE", "PeriodStartDate", "Period Start Date", BoFieldTypes.db_Date);
                    AddUserField("CM_REV_SCHEDULE", "PeriodEndDate", "Period End Date", BoFieldTypes.db_Date);
                    AddUserField("CM_REV_SCHEDULE", "ScheduledRevenue", "Scheduled Revenue", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_SCHEDULE", "RecognizedRevenue", "Recognized Revenue", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_SCHEDULE", "DeferredRevenue", "Deferred Revenue", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_SCHEDULE", "CumulativeRevenue", "Cumulative Revenue", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_SCHEDULE", "RecogBasis", "Recognition Basis", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_REV_SCHEDULE", "ProgressPct", "Progress Percentage", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Percentage);
                    AddUserField("CM_REV_SCHEDULE", "Status", "Status", BoFieldTypes.db_Alpha, 20); // Scheduled, Recognized, Adjusted
                    AddUserField("CM_REV_SCHEDULE", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_REV_SCHEDULE", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Revenue Recognition Schedule table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Revenue Schedule table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Deferred Revenue table
        /// </summary>
        private void CreateDeferredRevenueTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Deferred Revenue table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_DEFERRED_REV"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_DEFERRED_REV";
                    userTable.TableDescription = "Deferred Revenue";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_DEFERRED_REV", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_DEFERRED_REV", "PerformObligCode", "Performance Obligation Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_DEFERRED_REV", "IPCCode", "IPC Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_DEFERRED_REV", "BilledAmount", "Billed Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_DEFERRED_REV", "RecognizedRevenue", "Recognized Revenue", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_DEFERRED_REV", "DeferredAmount", "Deferred Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_DEFERRED_REV", "DeferralReason", "Deferral Reason", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_DEFERRED_REV", "ReleaseSchedule", "Release Schedule", BoFieldTypes.db_Memo);
                    AddUserField("CM_DEFERRED_REV", "Status", "Status", BoFieldTypes.db_Alpha, 20); // Active, Released, Cancelled
                    AddUserField("CM_DEFERRED_REV", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_DEFERRED_REV", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Deferred Revenue table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Deferred Revenue table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Contract Assets & Liabilities table
        /// </summary>
        private void CreateContractAssetsTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Contract Assets table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_CONTRACT_ASSETS"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_CONTRACT_ASSETS";
                    userTable.TableDescription = "Contract Assets & Liabilities";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_CONTRACT_ASSETS", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_CONTRACT_ASSETS", "AsOfDate", "As Of Date", BoFieldTypes.db_Date);
                    AddUserField("CM_CONTRACT_ASSETS", "ContractAsset", "Contract Asset", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CONTRACT_ASSETS", "ContractLiability", "Contract Liability", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CONTRACT_ASSETS", "NetPosition", "Net Position", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CONTRACT_ASSETS", "Currency", "Currency", BoFieldTypes.db_Alpha, 3);
                    AddUserField("CM_CONTRACT_ASSETS", "BaseCurrencyValue", "Base Currency Value", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_CONTRACT_ASSETS", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_CONTRACT_ASSETS", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Contract Assets table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Contract Assets table: " + ex.Message, ex);
                throw;
            }
            finally
            {
                if (userTable != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                }
            }
        }

        /// <summary>
        /// Create Revenue Recognition Log table
        /// </summary>
        private void CreateRevenueLogTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Revenue Recognition Log table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_REV_LOG"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_REV_LOG";
                    userTable.TableDescription = "Revenue Recognition Log";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_REV_LOG", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_REV_LOG", "PerfOblCode", "Performance Obligation Code", BoFieldTypes.db_Alpha, 50);
                    AddUserField("CM_REV_LOG", "RecognitionDate", "Recognition Date", BoFieldTypes.db_Date);
                    AddUserField("CM_REV_LOG", "RecognitionAmount", "Recognition Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_LOG", "RecogMethod", "Recognition Method", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_REV_LOG", "CumulativeAmount", "Cumulative Amount", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_REV_LOG", "JournalEntryRef", "Journal Entry Reference", BoFieldTypes.db_Numeric);
                    AddUserField("CM_REV_LOG", "Notes", "Notes", BoFieldTypes.db_Memo);
                    AddUserField("CM_REV_LOG", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_REV_LOG", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Revenue Recognition Log table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Revenue Log table: " + ex.Message, ex);
                // Don't throw - this is a logging table
            }
        }

        /// <summary>
        /// Create Contract Backlog table
        /// </summary>
        private void CreateBacklogTable()
        {
            UserTablesMD userTable = null;

            try
            {
                Logger.Info("Creating Contract Backlog table...");

                if (!DatabaseHelper.UserTableExists(_company, "CM_BACKLOG"))
                {
                    userTable = (UserTablesMD)_company.GetBusinessObject(BoObjectTypes.oUserTables);
                    userTable.TableName = "CM_BACKLOG";
                    userTable.TableDescription = "Contract Backlog";
                    userTable.TableType = BoUTBTableType.bott_NoObject;

                    if (userTable.Add() != 0)
                    {
                        throw new Exception(_company.GetLastErrorDescription());
                    }

                    
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(userTable);
                    userTable = null;

                    // Add fields
                    AddUserField("CM_BACKLOG", "ContractCode", "Contract Code", BoFieldTypes.db_Alpha, 20);
                    AddUserField("CM_BACKLOG", "AsOfDate", "As Of Date", BoFieldTypes.db_Date);
                    AddUserField("CM_BACKLOG", "TotalContractValue", "Total Contract Value", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "BilledToDate", "Billed To Date", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "RevenueRecogToDate", "Revenue Recognized To Date", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "RemainingBacklog", "Remaining Backlog", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "Forecast30Days", "Forecast 30 Days", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "Forecast60Days", "Forecast 60 Days", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "Forecast90Days", "Forecast 90 Days", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "BurnRate", "Burn Rate", BoFieldTypes.db_Float, subType: BoFldSubTypes.st_Sum);
                    AddUserField("CM_BACKLOG", "EstCompletionDate", "Estimated Completion Date", BoFieldTypes.db_Date);
                    AddUserField("CM_BACKLOG", "CreateDate", "Create Date", BoFieldTypes.db_Date);
                    AddUserField("CM_BACKLOG", "CreateUser", "Create User", BoFieldTypes.db_Alpha, 50);
                }

                Logger.Info("Contract Backlog table created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Backlog table: " + ex.Message, ex);
                // Don't throw - this is an analytical table
            }
        }

        /// <summary>
        /// Register Performance Obligation UDO
        /// </summary>
        private void RegisterPerformanceObligationUDO()
        {
            try
            {
                Logger.Info("Registering Performance Obligation UDO...");

                if (UDOExists("CM_PERF_OBL"))
                {
                    Logger.Info("Performance Obligation UDO already registered");
                    return;
                }

                UserObjectsMD userObject = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                userObject.Code = "CM_PERF_OBL";
                userObject.Name = "Performance Obligation";
                userObject.ObjectType = BoUDOObjType.boud_MasterData;
                userObject.TableName = "CM_PERF_OBL";
                userObject.CanCancel = BoYesNoEnum.tNO;
                userObject.CanClose = BoYesNoEnum.tNO;
                userObject.CanDelete = BoYesNoEnum.tYES;
                userObject.CanCreateDefaultForm = BoYesNoEnum.tNO;
                userObject.CanFind = BoYesNoEnum.tYES;

                // Add child table
                userObject.ChildTables.TableName = "CM_PERF_OBL_LNS";
                userObject.ChildTables.Add();

                if (userObject.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Performance Obligation UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Performance Obligation UDO: " + ex.Message, ex);
                // Don't throw - UDO registration is optional
            }
        }

        /// <summary>
        /// Register Revenue Schedule UDO
        /// </summary>
        private void RegisterRevenueScheduleUDO()
        {
            try
            {
                Logger.Info("Registering Revenue Schedule UDO...");

                if (UDOExists("CM_REV_SCHEDULE"))
                {
                    Logger.Info("Revenue Schedule UDO already registered");
                    return;
                }

                UserObjectsMD userObject = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                userObject.Code = "CM_REV_SCHEDULE";
                userObject.Name = "Revenue Recognition Schedule";
                userObject.ObjectType = BoUDOObjType.boud_MasterData;
                userObject.TableName = "CM_REV_SCHEDULE";
                userObject.CanCancel = BoYesNoEnum.tNO;
                userObject.CanClose = BoYesNoEnum.tNO;
                userObject.CanDelete = BoYesNoEnum.tYES;
                userObject.CanCreateDefaultForm = BoYesNoEnum.tNO;
                userObject.CanFind = BoYesNoEnum.tYES;

                if (userObject.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Revenue Schedule UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Revenue Schedule UDO: " + ex.Message, ex);
                // Don't throw - UDO registration is optional
            }
        }

        /// <summary>
        /// Register Deferred Revenue UDO
        /// </summary>
        private void RegisterDeferredRevenueUDO()
        {
            try
            {
                Logger.Info("Registering Deferred Revenue UDO...");

                if (UDOExists("CM_DEFERRED_REV"))
                {
                    Logger.Info("Deferred Revenue UDO already registered");
                    return;
                }

                UserObjectsMD userObject = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                userObject.Code = "CM_DEFERRED_REV";
                userObject.Name = "Deferred Revenue";
                userObject.ObjectType = BoUDOObjType.boud_MasterData;
                userObject.TableName = "CM_DEFERRED_REV";
                userObject.CanCancel = BoYesNoEnum.tNO;
                userObject.CanClose = BoYesNoEnum.tNO;
                userObject.CanDelete = BoYesNoEnum.tYES;
                userObject.CanCreateDefaultForm = BoYesNoEnum.tNO;
                userObject.CanFind = BoYesNoEnum.tYES;

                if (userObject.Add() != 0)
                {
                    throw new Exception(_company.GetLastErrorDescription());
                }

                Logger.Info("Deferred Revenue UDO registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error registering Deferred Revenue UDO: " + ex.Message, ex);
                // Don't throw - UDO registration is optional
            }
        }

        #endregion
    }
}
