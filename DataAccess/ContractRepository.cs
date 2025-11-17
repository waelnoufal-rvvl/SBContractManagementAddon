using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
        /// <summary>
        /// Repository for Contract data access
        /// </summary>
        public class ContractRepository
        {
        private Company _company;

        public ContractRepository(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        /// <summary>
        /// Get all contracts
        /// </summary>
        public List<Contract> GetAll()
        {
            List<Contract> contracts = new List<Contract>();
            Recordset recordset = null;

            try
            {
                // Use new RVCM contract header table as per technical design
                string query = @"SELECT ""Code"", ""DocNum"", ""U_CardCode"", ""U_CardName"",
                                ""U_ContractName"", ""U_ProjectNum"", ""U_ProjectName"", ""U_Status"",
                                ""U_TotalValue"", ""U_Currency"", ""U_ContractMgr""
                                FROM ""@RVCM_CNTRCT""
                                ORDER BY ""DocNum"" DESC";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    var contractName = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractName").Value);

                    Contract contract = new Contract
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                        CustomerCode = SafeConversion.SafeToString(recordset.Fields.Item("U_CardCode").Value),
                        CustomerName = SafeConversion.SafeToString(recordset.Fields.Item("U_CardName").Value),
                        ContractName = contractName,
                        Description = contractName,
                        ProjectCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ProjectNum").Value),
                        ProjectName = SafeConversion.SafeToString(recordset.Fields.Item("U_ProjectName").Value),
                        Status = MapDbStatusToModel(SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value)),
                        TotalValue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalValue").Value),
                        Currency = SafeConversion.SafeToString(recordset.Fields.Item("U_Currency").Value),
                        ContractManager = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractMgr").Value)
                    };

                    contracts.Add(contract);
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {contracts.Count} contracts");
                return contracts;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting all contracts", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get contract by code
        /// </summary>
        public Contract GetByCode(string code)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@RVCM_CNTRCT"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(code)}'";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"Contract not found: {code}");
                    return null;
                }

                Contract contract = MapContract(recordset);

                // Get lines
                contract.Lines = GetContractLines(code);

                Logger.Info($"Retrieved contract: {code}");
                return contract;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting contract {code}", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Get contract lines
        /// </summary>
        private List<ContractLine> GetContractLines(string contractCode)
        {
            List<ContractLine> lines = new List<ContractLine>();
            Recordset recordset = null;

            try
            {
                // New contract lines table @RVCM_CNTRCT1
                string query = $@"SELECT * FROM ""@RVCM_CNTRCT1"" WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}' ORDER BY ""LineId""";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    ContractLine line = new ContractLine
                    {
                        LineNum = SafeConversion.SafeToInt(recordset.Fields.Item("LineId").Value),
                        ItemCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ItemCode").Value),
                        ItemDescription = SafeConversion.SafeToString(recordset.Fields.Item("U_ItemDesc").Value),
                        Quantity = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Quantity").Value),
                        UnitOfMeasure = SafeConversion.SafeToString(recordset.Fields.Item("U_UoM").Value),
                        UnitPrice = SafeConversion.SafeToDouble(recordset.Fields.Item("U_UnitPrice").Value),
                        LineTotal = SafeConversion.SafeToDouble(recordset.Fields.Item("U_LineTotal").Value),
                        AccountCode = SafeConversion.SafeToString(recordset.Fields.Item("U_AccountCode").Value),
                        CostCenter = SafeConversion.SafeToString(recordset.Fields.Item("U_CostCenter").Value),
                        Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value)
                    };

                    lines.Add(line);
                    recordset.MoveNext();
                }

                return lines;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting contract lines for {contractCode}", ex);
                throw;
            }
            finally
            {
                if (recordset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(recordset);
                }
            }
        }

        /// <summary>
        /// Create new contract
        /// </summary>
        public string Create(Contract contract)
        {
            try
            {
                Logger.Info($"Creating contract: {contract.Code}");

                // Validate
                var errors = contract.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                // Use UDO to create (assumes UDO CONTRACT is bound to @RVCM_CNTRCT)
                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CONTRACT");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set header fields (RVCM contract schema)
                generalData.SetProperty("Code", contract.Code);
                generalData.SetProperty("U_CardCode", contract.CustomerCode);
                generalData.SetProperty("U_CardName", contract.CustomerName);
                generalData.SetProperty("U_CardType", contract.CustomerType ?? string.Empty);
                if (contract.ContactPersonId.HasValue)
                    generalData.SetProperty("U_CntctCode", contract.ContactPersonId.Value);
                generalData.SetProperty("U_ContractName", string.IsNullOrWhiteSpace(contract.ContractName)
                    ? contract.Description
                    : contract.ContractName);
                generalData.SetProperty("U_ProjectNum", contract.ProjectCode ?? "");
                generalData.SetProperty("U_ProjectName", contract.ProjectName ?? "");
                generalData.SetProperty("U_Sector", contract.Sector ?? string.Empty);
                generalData.SetProperty("U_UserType", contract.UserType ?? string.Empty);
                generalData.SetProperty("U_UnitNumber", contract.UnitNumber ?? string.Empty);
                generalData.SetProperty("U_Region", contract.Region ?? string.Empty);
                generalData.SetProperty("U_ContractType", contract.ContractType ?? string.Empty);
                generalData.SetProperty("U_StartDate", contract.StartDate);
                generalData.SetProperty("U_EndDate", contract.EndDate);
                generalData.SetProperty("U_Duration", contract.DurationDays);
                generalData.SetProperty("U_Status", MapModelStatusToDb(contract.Status));
                generalData.SetProperty("U_TotalValue", contract.TotalValue);
                generalData.SetProperty("U_Currency", contract.Currency);
                generalData.SetProperty("U_RetentionPct", contract.RetentionPercentage);
                generalData.SetProperty("U_PaymentTerms", contract.PaymentTerms ?? "");
                generalData.SetProperty("U_ContractMgr", contract.ContractManager ?? "");
                generalData.SetProperty("U_QuoteRef", contract.PriceQuoteRef ?? string.Empty);
                generalData.SetProperty("U_ApprovalSts", string.IsNullOrEmpty(contract.ApprovalStatus)
                    ? "P"
                    : contract.ApprovalStatus);
                generalData.SetProperty("U_Remarks", contract.Remarks ?? "");

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_BaseCurrency", contract.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", contract.ExchangeRate);
                generalData.SetProperty("U_BaseCurrValue", contract.BaseCurrencyValue);
                generalData.SetProperty("U_FXGainLoss", contract.FXGainLoss);
                if (contract.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", contract.LastFXUpdateDate.Value);

                // Add lines
                GeneralDataCollection lines = generalData.Child("CONTRACT_LNS");
                foreach (var line in contract.Lines)
                {
                    GeneralData lineData = lines.Add();
                    lineData.SetProperty("U_ItemCode", line.ItemCode ?? "");
                    lineData.SetProperty("U_ItemDesc", line.ItemDescription);
                    lineData.SetProperty("U_Quantity", line.Quantity);
                    lineData.SetProperty("U_UoM", line.UnitOfMeasure);
                    lineData.SetProperty("U_UnitPrice", line.UnitPrice);
                    lineData.SetProperty("U_LineTotal", line.LineTotal);
                    lineData.SetProperty("U_AccountCode", line.AccountCode ?? "");
                    lineData.SetProperty("U_CostCenter", line.CostCenter ?? "");
                    lineData.SetProperty("U_Remarks", line.Remarks ?? "");
                }

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);

                Logger.Info($"Contract created successfully: {contract.Code}");
                return contract.Code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating contract {contract.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update contract
        /// </summary>
        public void Update(Contract contract)
        {
            try
            {
                Logger.Info($"Updating contract: {contract.Code}");

                // Validate
                var errors = contract.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CONTRACT");

                // Get existing data
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", contract.Code);
                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update header fields (RVCM contract schema)
                generalData.SetProperty("U_CardCode", contract.CustomerCode);
                generalData.SetProperty("U_CardName", contract.CustomerName);
                generalData.SetProperty("U_CardType", contract.CustomerType ?? string.Empty);
                if (contract.ContactPersonId.HasValue)
                    generalData.SetProperty("U_CntctCode", contract.ContactPersonId.Value);
                generalData.SetProperty("U_ContractName", string.IsNullOrWhiteSpace(contract.ContractName)
                    ? contract.Description
                    : contract.ContractName);
                generalData.SetProperty("U_ProjectNum", contract.ProjectCode ?? "");
                generalData.SetProperty("U_ProjectName", contract.ProjectName ?? "");
                generalData.SetProperty("U_Sector", contract.Sector ?? string.Empty);
                generalData.SetProperty("U_UserType", contract.UserType ?? string.Empty);
                generalData.SetProperty("U_UnitNumber", contract.UnitNumber ?? string.Empty);
                generalData.SetProperty("U_Region", contract.Region ?? string.Empty);
                generalData.SetProperty("U_ContractType", contract.ContractType ?? string.Empty);
                generalData.SetProperty("U_StartDate", contract.StartDate);
                generalData.SetProperty("U_EndDate", contract.EndDate);
                generalData.SetProperty("U_Duration", contract.DurationDays);
                generalData.SetProperty("U_Status", MapModelStatusToDb(contract.Status));
                generalData.SetProperty("U_TotalValue", contract.TotalValue);
                generalData.SetProperty("U_Currency", contract.Currency);
                generalData.SetProperty("U_RetentionPct", contract.RetentionPercentage);
                generalData.SetProperty("U_PaymentTerms", contract.PaymentTerms ?? "");
                generalData.SetProperty("U_ContractMgr", contract.ContractManager ?? "");
                generalData.SetProperty("U_QuoteRef", contract.PriceQuoteRef ?? string.Empty);
                generalData.SetProperty("U_ApprovalSts", string.IsNullOrEmpty(contract.ApprovalStatus)
                    ? "P"
                    : contract.ApprovalStatus);
                generalData.SetProperty("U_Remarks", contract.Remarks ?? "");

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_BaseCurrency", contract.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", contract.ExchangeRate);
                generalData.SetProperty("U_BaseCurrValue", contract.BaseCurrencyValue);
                generalData.SetProperty("U_FXGainLoss", contract.FXGainLoss);
                if (contract.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", contract.LastFXUpdateDate.Value);

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"Contract updated successfully: {contract.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating contract {contract.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete contract
        /// </summary>
        public void Delete(string code)
        {
            try
            {
                Logger.Info($"Deleting contract: {code}");

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CONTRACT");

                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", code);

                generalService.Delete(generalParams);

                Logger.Info($"Contract deleted successfully: {code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error deleting contract {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Map recordset to contract object
        /// Uses SafeConversion to handle NULL values without throwing exceptions
        /// </summary>
        private Contract MapContract(Recordset recordset)
        {
            var contractName = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractName").Value);

            return new Contract
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                CustomerCode = SafeConversion.SafeToString(recordset.Fields.Item("U_CardCode").Value),
                CustomerName = SafeConversion.SafeToString(recordset.Fields.Item("U_CardName").Value),
                CustomerType = SafeConversion.SafeToString(recordset.Fields.Item("U_CardType").Value),
                ContactPersonId = SafeConversion.SafeToNullableInt(recordset.Fields.Item("U_CntctCode").Value),
                ContractName = contractName,
                Description = contractName,
                ProjectCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ProjectNum").Value),
                ProjectName = SafeConversion.SafeToString(recordset.Fields.Item("U_ProjectName").Value),
                Sector = SafeConversion.SafeToString(recordset.Fields.Item("U_Sector").Value),
                UserType = SafeConversion.SafeToString(recordset.Fields.Item("U_UserType").Value),
                UnitNumber = SafeConversion.SafeToString(recordset.Fields.Item("U_UnitNumber").Value),
                Region = SafeConversion.SafeToString(recordset.Fields.Item("U_Region").Value),
                StartDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_StartDate").Value),
                EndDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_EndDate").Value),
                DurationDays = SafeConversion.SafeToInt(recordset.Fields.Item("U_Duration").Value),
                Status = MapDbStatusToModel(SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value)),
                ContractType = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractType").Value),
                TotalValue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalValue").Value),
                Currency = SafeConversion.SafeToString(recordset.Fields.Item("U_Currency").Value),
                // Retention, payment terms, contract manager, remarks and FX fields are optional in RVCM schema
                RetentionPercentage = SafeConversion.SafeToDouble(recordset.Fields.Item("U_RetentionPct").Value),
                PaymentTerms = SafeConversion.SafeToString(recordset.Fields.Item("U_PaymentTerms").Value),
                ContractManager = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractMgr").Value),
                PriceQuoteRef = SafeConversion.SafeToString(recordset.Fields.Item("U_QuoteRef").Value),
                ApprovalStatus = SafeConversion.SafeToString(recordset.Fields.Item("U_ApprovalSts").Value),
                Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value),

                BaseCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_BaseCurrency").Value),
                ExchangeRate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ExchangeRate").Value, 1.0),
                BaseCurrencyValue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_BaseCurrValue").Value),
                FXGainLoss = SafeConversion.SafeToDouble(recordset.Fields.Item("U_FXGainLoss").Value),
                LastFXUpdateDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_LastFXUpdate").Value)
            };
        }

        /// <summary>
        /// Map database contract status code to model-friendly status text.
        /// </summary>
        private string MapDbStatusToModel(string dbStatus)
        {
            switch (dbStatus)
            {
                case "D": return "Draft";
                case "A": return "Active";
                case "H": return "OnHold";
                case "C": return "Completed";
                case "X": return "Cancelled";
                default:
                    return string.IsNullOrWhiteSpace(dbStatus) ? "Draft" : dbStatus;
            }
        }

        /// <summary>
        /// Map model contract status text to database status code.
        /// </summary>
        private string MapModelStatusToDb(string modelStatus)
        {
            switch (modelStatus)
            {
                case "Draft": return "D";
                case "Active": return "A";
                case "OnHold": return "H";
                case "Completed": return "C";
                case "Cancelled": return "X";
                default:
                    return string.IsNullOrWhiteSpace(modelStatus) ? "D" : modelStatus;
            }
        }
    }
}
