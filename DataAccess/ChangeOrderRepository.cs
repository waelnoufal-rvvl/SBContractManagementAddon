using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
    /// <summary>
    /// Repository for Change Order data access
    /// </summary>
    public class ChangeOrderRepository
    {
        private Company _company;

        public ChangeOrderRepository(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        /// <summary>
        /// Get all change orders for a contract
        /// </summary>
        public List<ChangeOrder> GetByContract(string contractCode)
        {
            List<ChangeOrder> changeOrders = new List<ChangeOrder>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT ""Code"", ""U_DocNum"", ""U_ContractCode"", ""U_CONumber"", ""U_CODate"",
                                 ""U_Type"", ""U_Description"", ""U_Amount"", ""U_Status""
                                 FROM ""@CO_HDR""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 ORDER BY ""U_CONumber""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    ChangeOrder co = new ChangeOrder
                    {
                        Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                        DocNum = SafeConversion.SafeToString(recordset.Fields.Item("U_DocNum").Value),
                        ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                        ChangeOrderNumber = SafeConversion.SafeToInt(recordset.Fields.Item("U_CONumber").Value),
                        ChangeOrderDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CODate").Value),
                        Type = SafeConversion.SafeToString(recordset.Fields.Item("U_Type").Value),
                        Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                        Amount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Amount").Value),
                        Status = SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value)
                    };

                    changeOrders.Add(co);
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {changeOrders.Count} change orders for contract {contractCode}");
                return changeOrders;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting change orders for contract {contractCode}", ex);
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
        /// Get change order by code
        /// </summary>
        public ChangeOrder GetByCode(string code)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CO_HDR"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(code)}'";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"Change Order not found: {code}");
                    return null;
                }

                ChangeOrder co = MapChangeOrder(recordset);

                // Get lines
                co.Lines = GetChangeOrderLines(code);

                Logger.Info($"Retrieved Change Order: {code}");
                return co;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting Change Order {code}", ex);
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
        /// Get change order lines
        /// </summary>
        private List<ChangeOrderLine> GetChangeOrderLines(string coCode)
        {
            List<ChangeOrderLine> lines = new List<ChangeOrderLine>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CO_LNS"" WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(coCode)}' ORDER BY ""LineId""";
                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    ChangeOrderLine line = new ChangeOrderLine
                    {
                        LineNum = SafeConversion.SafeToInt(recordset.Fields.Item("LineId").Value),
                        ItemCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ItemCode").Value),
                        Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                        Quantity = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Quantity").Value),
                        UnitOfMeasure = SafeConversion.SafeToString(recordset.Fields.Item("U_UoM").Value),
                        UnitPrice = SafeConversion.SafeToDouble(recordset.Fields.Item("U_UnitPrice").Value),
                        LineTotal = SafeConversion.SafeToDouble(recordset.Fields.Item("U_LineTotal").Value),
                        Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value)
                    };

                    lines.Add(line);
                    recordset.MoveNext();
                }

                return lines;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting Change Order lines for {coCode}", ex);
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
        /// Create new change order
        /// </summary>
        public string Create(ChangeOrder changeOrder)
        {
            try
            {
                Logger.Info($"Creating Change Order for contract: {changeOrder.ContractCode}");

                // Validate
                var errors = changeOrder.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                // Use UDO to create
                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CHANGEORDER");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set header fields
                generalData.SetProperty("U_ContractCode", changeOrder.ContractCode);
                generalData.SetProperty("U_CONumber", changeOrder.ChangeOrderNumber);
                generalData.SetProperty("U_CODate", changeOrder.ChangeOrderDate);
                generalData.SetProperty("U_Type", changeOrder.Type);
                generalData.SetProperty("U_Description", changeOrder.Description);
                generalData.SetProperty("U_Amount", changeOrder.Amount);
                generalData.SetProperty("U_AddDays", changeOrder.AdditionalDays);
                generalData.SetProperty("U_Status", changeOrder.Status);
                generalData.SetProperty("U_RequestedBy", changeOrder.RequestedBy ?? "");
                generalData.SetProperty("U_Justification", changeOrder.Justification ?? "");
                generalData.SetProperty("U_Remarks", changeOrder.Remarks ?? "");

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_Currency", changeOrder.Currency ?? "USD");
                generalData.SetProperty("U_BaseCurrency", changeOrder.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", changeOrder.ExchangeRate);
                generalData.SetProperty("U_BaseCurrAmt", changeOrder.BaseCurrencyAmount);
                generalData.SetProperty("U_FXGainLoss", changeOrder.FXGainLoss);
                if (changeOrder.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", changeOrder.LastFXUpdateDate.Value);

                // Add lines
                GeneralDataCollection lines = generalData.Child("CO_LNS");
                foreach (var line in changeOrder.Lines)
                {
                    GeneralData lineData = lines.Add();
                    lineData.SetProperty("U_ItemCode", line.ItemCode ?? "");
                    lineData.SetProperty("U_Description", line.Description);
                    lineData.SetProperty("U_Quantity", line.Quantity);
                    lineData.SetProperty("U_UoM", line.UnitOfMeasure);
                    lineData.SetProperty("U_UnitPrice", line.UnitPrice);
                    lineData.SetProperty("U_LineTotal", line.LineTotal);
                    lineData.SetProperty("U_Remarks", line.Remarks ?? "");
                }

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);
                string code = result.GetProperty("Code").ToString();

                Logger.Info($"Change Order created successfully: {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating Change Order for contract {changeOrder.ContractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update change order
        /// </summary>
        public void Update(ChangeOrder changeOrder)
        {
            try
            {
                Logger.Info($"Updating Change Order: {changeOrder.Code}");

                // Validate
                var errors = changeOrder.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CHANGEORDER");

                // Get existing data
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", changeOrder.Code);
                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update header fields
                generalData.SetProperty("U_Description", changeOrder.Description);
                generalData.SetProperty("U_Amount", changeOrder.Amount);
                generalData.SetProperty("U_Status", changeOrder.Status);
                generalData.SetProperty("U_Remarks", changeOrder.Remarks ?? "");

                // PHASE 1: Multi-Currency fields
                generalData.SetProperty("U_Currency", changeOrder.Currency ?? "USD");
                generalData.SetProperty("U_BaseCurrency", changeOrder.BaseCurrency ?? "USD");
                generalData.SetProperty("U_ExchangeRate", changeOrder.ExchangeRate);
                generalData.SetProperty("U_BaseCurrAmt", changeOrder.BaseCurrencyAmount);
                generalData.SetProperty("U_FXGainLoss", changeOrder.FXGainLoss);
                if (changeOrder.LastFXUpdateDate.HasValue)
                    generalData.SetProperty("U_LastFXUpdate", changeOrder.LastFXUpdateDate.Value);

                if (!string.IsNullOrEmpty(changeOrder.ApprovedBy))
                {
                    generalData.SetProperty("U_ApprovedBy", changeOrder.ApprovedBy);
                }

                if (changeOrder.ApprovedDate.HasValue)
                {
                    generalData.SetProperty("U_ApprovedDate", changeOrder.ApprovedDate.Value);
                }

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"Change Order updated successfully: {changeOrder.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating Change Order {changeOrder.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get next change order number for a contract
        /// HANA & SQL Server compatible (uses COALESCE instead of ISNULL)
        /// </summary>
        public int GetNextCONumber(string contractCode)
        {
            try
            {
                string query = $@"SELECT COALESCE(MAX(""U_CONumber""), 0) + 1 AS ""NextNum""
                                 FROM ""@CO_HDR""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'";

                object result = DatabaseHelper.ExecuteScalar(_company, query);
                return SafeConversion.SafeToInt(result, 1);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting next CO number for contract {contractCode}", ex);
                return 1;
            }
        }

        /// <summary>
        /// Get total approved change order amount for a contract
        /// HANA & SQL Server compatible (uses COALESCE instead of ISNULL)
        /// </summary>
        public double GetTotalApprovedCOAmount(string contractCode)
        {
            try
            {
                string query = $@"SELECT COALESCE(SUM(""U_Amount""), 0) AS ""TotalAmount""
                                 FROM ""@CO_HDR""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 AND ""U_Status"" = 'Approved'
                                 AND ""U_Type"" = 'Addition'";

                object result = DatabaseHelper.ExecuteScalar(_company, query);
                double additions = SafeConversion.SafeToDouble(result, 0);

                query = $@"SELECT COALESCE(SUM(""U_Amount""), 0) AS ""TotalAmount""
                          FROM ""@CO_HDR""
                          WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                          AND ""U_Status"" = 'Approved'
                          AND ""U_Type"" = 'Deduction'";

                result = DatabaseHelper.ExecuteScalar(_company, query);
                double deductions = SafeConversion.SafeToDouble(result, 0);

                return additions - deductions;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting total CO amount for contract {contractCode}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Map recordset to ChangeOrder object
        /// Uses SafeConversion to handle NULL values without throwing exceptions
        /// </summary>
        private ChangeOrder MapChangeOrder(Recordset recordset)
        {
            ChangeOrder co = new ChangeOrder
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("U_DocNum").Value),
                ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                ChangeOrderNumber = SafeConversion.SafeToInt(recordset.Fields.Item("U_CONumber").Value),
                ChangeOrderDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CODate").Value),
                Type = SafeConversion.SafeToString(recordset.Fields.Item("U_Type").Value),
                Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                Amount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Amount").Value),
                AdditionalDays = SafeConversion.SafeToInt(recordset.Fields.Item("U_AddDays").Value),
                Status = SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value),
                RequestedBy = SafeConversion.SafeToString(recordset.Fields.Item("U_RequestedBy").Value),
                ApprovedBy = SafeConversion.SafeToString(recordset.Fields.Item("U_ApprovedBy").Value),
                Justification = SafeConversion.SafeToString(recordset.Fields.Item("U_Justification").Value),
                Remarks = SafeConversion.SafeToString(recordset.Fields.Item("U_Remarks").Value),
                ApprovedDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_ApprovedDate").Value),

                // PHASE 1: Multi-Currency fields
                Currency = SafeConversion.SafeToString(recordset.Fields.Item("U_Currency").Value),
                BaseCurrency = SafeConversion.SafeToString(recordset.Fields.Item("U_BaseCurrency").Value),
                ExchangeRate = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ExchangeRate").Value, 1.0),
                BaseCurrencyAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_BaseCurrAmt").Value),
                FXGainLoss = SafeConversion.SafeToDouble(recordset.Fields.Item("U_FXGainLoss").Value),
                LastFXUpdateDate = SafeConversion.SafeToNullableDateTime(recordset.Fields.Item("U_LastFXUpdate").Value)
            };

            return co;
        }
    }
}
