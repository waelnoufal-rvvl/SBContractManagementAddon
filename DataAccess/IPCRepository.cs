using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
    /// <summary>
    /// Repository for IPC data access
    /// </summary>
    public class IPCRepository
    {
        private Company _company;

        public IPCRepository(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        /// <summary>
        /// Get all IPCs for a contract
        /// </summary>
        public List<IPC> GetByContract(string contractCode)
        {
            List<IPC> ipcs = new List<IPC>();
            Recordset recordset = null;

            try
            {
                // Use new RVCM ICP header table as per technical design
                string query = $@"SELECT ""Code"", ""DocNum"", ""U_ContractCode"", ""U_IPCNumber"", ""U_IPCDate"",
                                 ""U_GrossValue"", ""U_TotalDue"", ""U_Status""
                                 FROM ""@RVCM_ICP""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 ORDER BY ""U_IPCNumber""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    IPC ipc = new IPC
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                        ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                        IPCNumber = SafeConversion.SafeToInt(recordset.Fields.Item("U_IPCNumber").Value),
                        IPCDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_IPCDate").Value),
                        GrossAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_GrossValue").Value),
                        NetAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalDue").Value),
                        Status = MapDbStatusToModel(SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value))
                    };

                    ipcs.Add(ipc);
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {ipcs.Count} IPCs for contract {contractCode}");
                return ipcs;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPCs for contract {contractCode}", ex);
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
        /// Get IPC by code
        /// </summary>
        public IPC GetByCode(string code)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@RVCM_ICP"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(code)}'";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"IPC not found: {code}");
                    return null;
                }

                IPC ipc = MapIPC(recordset);

                // Get lines
                ipc.Lines = GetIPCLines(code);

                Logger.Info($"Retrieved IPC: {code}");
                return ipc;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPC {code}", ex);
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
        /// Get IPC lines
        /// </summary>
        private List<IPCLine> GetIPCLines(string ipcCode)
        {
            List<IPCLine> lines = new List<IPCLine>();
            Recordset recordset = null;

            try
            {
                // New ICP lines table @RVCM_ICP1
                string query = $@"SELECT * FROM ""@RVCM_ICP1"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(ipcCode)}' ORDER BY ""LineId""";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    IPCLine line = new IPCLine
                    {
                        LineNum = SafeConversion.SafeToInt(recordset.Fields.Item("LineId").Value),
                        Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                        Quantity = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Quantity").Value),
                        UnitPrice = SafeConversion.SafeToDouble(recordset.Fields.Item("U_UnitPrice").Value),
                        Amount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Amount").Value),
                        CompletionPercentage = SafeConversion.SafeToDouble(recordset.Fields.Item("U_CompletionPct").Value),
                        Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value)
                    };

                    lines.Add(line);
                    recordset.MoveNext();
                }

                return lines;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPC lines for {ipcCode}", ex);
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
        /// Create new IPC
        /// </summary>
        public string Create(IPC ipc)
        {
            try
            {
                Logger.Info($"Creating IPC for contract: {ipc.ContractCode}");

                // Validate
                var errors = ipc.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                // Use UDO to create
                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("IPC");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set header fields (see RVCM_IPC_Master_FINAL.b1f)
                generalData.SetProperty("U_ContractCode", ipc.ContractCode);
                generalData.SetProperty("U_IPCNumber", ipc.IPCNumber);
                generalData.SetProperty("U_IPCDate", ipc.IPCDate);
                generalData.SetProperty("U_Period", ipc.Period ?? string.Empty);
                generalData.SetProperty("U_GrossValue", ipc.GrossAmount);
                generalData.SetProperty("U_RetentionAmt", ipc.RetentionAmount);
                generalData.SetProperty("U_AdvDeduct", ipc.AdvanceDeduction);
                generalData.SetProperty("U_MatDeduct", ipc.MaterialDeduction);
                generalData.SetProperty("U_OtherDeduct", ipc.OtherDeduction);
                generalData.SetProperty("U_VATAmount", ipc.VATAmount);
                generalData.SetProperty("U_TotalDue", ipc.NetAmount);
                generalData.SetProperty("U_PreviousCertified", ipc.PreviousIPCTotal);
                generalData.SetProperty("U_CurrentCertified", ipc.CurrentAmount);
                generalData.SetProperty("U_Status", MapModelStatusToDb(ipc.Status));
                generalData.SetProperty("U_Remarks", ipc.Remarks ?? string.Empty);

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_Currency", ipc.Currency ?? "USD");
                generalData.SetProperty("U_BaseCurrency", ipc.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", ipc.ExchangeRate);
                generalData.SetProperty("U_BaseCurrGross", ipc.BaseCurrencyGrossAmount);
                generalData.SetProperty("U_BaseCurrNet", ipc.BaseCurrencyNetAmount);
                generalData.SetProperty("U_FXGainLoss", ipc.FXGainLoss);
                if (ipc.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", ipc.LastFXUpdateDate.Value);

                // Add lines
                GeneralDataCollection lines = generalData.Child("IPC_LNS");
                foreach (var line in ipc.Lines)
                {
                    GeneralData lineData = lines.Add();
                    lineData.SetProperty("U_Description", line.Description);
                    lineData.SetProperty("U_Quantity", line.Quantity);
                    lineData.SetProperty("U_UnitPrice", line.UnitPrice);
                    lineData.SetProperty("U_Amount", line.Amount);
                    lineData.SetProperty("U_CompletionPct", line.CompletionPercentage);
                    lineData.SetProperty("U_Remarks", line.Remarks ?? "");
                }

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);
                string code = result.GetProperty("Code").ToString();

                Logger.Info($"IPC created successfully: {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating IPC for contract {ipc.ContractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update IPC
        /// </summary>
        public void Update(IPC ipc)
        {
            try
            {
                Logger.Info($"Updating IPC: {ipc.Code}");

                // Validate
                var errors = ipc.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("IPC");

                // Get existing data
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", ipc.Code);
                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update header fields
                generalData.SetProperty("U_IPCDate", ipc.IPCDate);
                generalData.SetProperty("U_GrossValue", ipc.GrossAmount);
                generalData.SetProperty("U_RetentionAmt", ipc.RetentionAmount);
                generalData.SetProperty("U_AdvDeduct", ipc.AdvanceDeduction);
                generalData.SetProperty("U_MatDeduct", ipc.MaterialDeduction);
                generalData.SetProperty("U_OtherDeduct", ipc.OtherDeduction);
                generalData.SetProperty("U_VATAmount", ipc.VATAmount);
                generalData.SetProperty("U_TotalDue", ipc.NetAmount);
                generalData.SetProperty("U_PreviousCertified", ipc.PreviousIPCTotal);
                generalData.SetProperty("U_CurrentCertified", ipc.CurrentAmount);
                generalData.SetProperty("U_Status", MapModelStatusToDb(ipc.Status));
                generalData.SetProperty("U_Remarks", ipc.Remarks ?? string.Empty);

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_Currency", ipc.Currency ?? "USD");
                generalData.SetProperty("U_BaseCurrency", ipc.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", ipc.ExchangeRate);
                generalData.SetProperty("U_BaseCurrGross", ipc.BaseCurrencyGrossAmount);
                generalData.SetProperty("U_BaseCurrNet", ipc.BaseCurrencyNetAmount);
                generalData.SetProperty("U_FXGainLoss", ipc.FXGainLoss);
                if (ipc.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", ipc.LastFXUpdateDate.Value);

                if (!string.IsNullOrEmpty(ipc.ApprovedBy))
                {
                    generalData.SetProperty("U_ApprovedBy", ipc.ApprovedBy);
                }

                if (ipc.ApprovedDate.HasValue)
                {
                    generalData.SetProperty("U_ApprovedDate", ipc.ApprovedDate.Value);
                }

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"IPC updated successfully: {ipc.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating IPC {ipc.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get next IPC number for a contract
        /// HANA & SQL Server compatible (uses COALESCE instead of ISNULL)
        /// </summary>
        public int GetNextIPCNumber(string contractCode)
        {
            try
            {
                string query = $@"SELECT COALESCE(MAX(""U_IPCNumber""), 0) + 1 AS ""NextNum""
                                 FROM ""@RVCM_ICP""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'";

                object result = DatabaseHelper.ExecuteScalar(_company, query);
                return SafeConversion.SafeToInt(result, 1);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting next IPC number for contract {contractCode}", ex);
                return 1;
            }
        }

        /// <summary>
        /// Get total IPC amount for a contract
        /// HANA & SQL Server compatible (uses COALESCE instead of ISNULL)
        /// </summary>
        public double GetTotalIPCAmount(string contractCode)
        {
            try
            {
                string query = $@"SELECT COALESCE(SUM(""U_GrossValue""), 0) AS ""TotalAmount""
                                 FROM ""@RVCM_ICP""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 AND ""U_Status"" IN ('A', 'P')";

                object result = DatabaseHelper.ExecuteScalar(_company, query);
                return SafeConversion.SafeToDouble(result, 0);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting total IPC amount for contract {contractCode}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Map recordset to IPC object
        /// Uses SafeConversion to handle NULL values without throwing exceptions
        /// </summary>
        private IPC MapIPC(Recordset recordset)
        {
            IPC ipc = new IPC
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                IPCNumber = SafeConversion.SafeToInt(recordset.Fields.Item("U_IPCNumber").Value),
                IPCDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_IPCDate").Value),
                Period = SafeConversion.SafeToString(recordset.Fields.Item("U_Period").Value),
                GrossAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_GrossValue").Value),
                RetentionAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_RetentionAmt").Value),
                AdvanceDeduction = SafeConversion.SafeToDouble(recordset.Fields.Item("U_AdvDeduct").Value),
                MaterialDeduction = SafeConversion.SafeToDouble(recordset.Fields.Item("U_MatDeduct").Value),
                OtherDeduction = SafeConversion.SafeToDouble(recordset.Fields.Item("U_OtherDeduct").Value),
                VATAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_VATAmount").Value),
                NetAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalDue").Value),
                PreviousIPCTotal = SafeConversion.SafeToDouble(recordset.Fields.Item("U_PreviousCertified").Value),
                CurrentAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_CurrentCertified").Value),
                Status = MapDbStatusToModel(SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value)),
                ApprovedBy = SafeConversion.SafeToString(recordset.Fields.Item("U_ApprovedBy").Value),
                Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value),
                ApprovedDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_ApprovedDate").Value),

                // PHASE 1: Multi-Currency fields
                Currency = SafeConversion.SafeToString(recordset.Fields.Item("U_Currency").Value),
                BaseCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_BaseCurrency").Value),
                ExchangeRate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ExchangeRate").Value, 1.0),
                BaseCurrencyGrossAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_BaseCurrGross").Value),
                BaseCurrencyNetAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_BaseCurrNet").Value),
                FXGainLoss = SafeConversion.SafeToDouble(recordset.Fields.Item("U_FXGainLoss").Value),
                LastFXUpdateDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_LastFXUpdate").Value)
            };

            return ipc;
        }

        /// <summary>
        /// Map database status code (D/S/A/P/R) to model-friendly status text.
        /// </summary>
        private string MapDbStatusToModel(string dbStatus)
        {
            switch (dbStatus)
            {
                case "D": return "Draft";
                case "S": return "Submitted";
                case "A": return "Approved";
                case "P": return "Paid";
                case "R": return "Rejected";
                default:
                    return string.IsNullOrWhiteSpace(dbStatus) ? "Draft" : dbStatus;
            }
        }

        /// <summary>
        /// Map model status text to database status code.
        /// </summary>
        private string MapModelStatusToDb(string modelStatus)
        {
            switch (modelStatus)
            {
                case "Draft": return "D";
                case "Submitted": return "S";
                case "Approved": return "A";
                case "Paid": return "P";
                case "Rejected": return "R";
                default:
                    return string.IsNullOrWhiteSpace(modelStatus) ? "D" : modelStatus;
            }
        }
    }
}
