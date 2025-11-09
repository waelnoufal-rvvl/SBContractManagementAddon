using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.DataAccess
{
    /// <summary>
    /// Repository for Revenue Recognition data access
    /// Handles all ASC 606 / IFRS 15 compliance data
    /// </summary>
    public class RevenueRecognitionRepository
    {
        private Company _company;

        public RevenueRecognitionRepository(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        #region Performance Obligations

        /// <summary>
        /// Get all performance obligations for a contract
        /// </summary>
        public List<PerformanceObligation> GetPerformanceObligationsByContract(string contractCode)
        {
            List<PerformanceObligation> obligations = new List<PerformanceObligation>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_PERF_OBL""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 ORDER BY ""U_OblNumber""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    PerformanceObligation obligation = MapPerformanceObligation(recordset);
                    obligation.Lines = GetPerformanceObligationLines(obligation.Code);
                    obligations.Add(obligation);
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {obligations.Count} performance obligations for contract {contractCode}");
                return obligations;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting performance obligations for contract {contractCode}", ex);
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
        /// Get performance obligation by code
        /// </summary>
        public PerformanceObligation GetPerformanceObligation(string code)
        {
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_PERF_OBL""
                                 WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(code)}'";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                if (recordset.EoF)
                {
                    Logger.Warning($"Performance obligation not found: {code}");
                    return null;
                }

                PerformanceObligation obligation = MapPerformanceObligation(recordset);
                obligation.Lines = GetPerformanceObligationLines(code);

                Logger.Info($"Retrieved performance obligation: {code}");
                return obligation;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting performance obligation {code}", ex);
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
        /// Get performance obligation lines
        /// </summary>
        private List<PerformanceObligationLine> GetPerformanceObligationLines(string obligationCode)
        {
            List<PerformanceObligationLine> lines = new List<PerformanceObligationLine>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_PERF_OBL_LNS""
                                 WHERE ""Code"" = '{DatabaseHelper.EscapeSqlString(obligationCode)}'
                                 ORDER BY ""LineId""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    PerformanceObligationLine line = new PerformanceObligationLine
                    {
                        LineNum = SafeConversion.SafeToInt(recordset.Fields.Item("LineId").Value),
                        ItemCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ItemCode").Value),
                        Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                        Quantity = SafeConversion.SafeToDouble(recordset.Fields.Item("U_Quantity").Value),
                        EstimatedCost = SafeConversion.SafeToDouble(recordset.Fields.Item("U_EstimatedCost").Value),
                        ActualCost = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ActualCost").Value),
                        CompletionPercentage = SafeConversion.SafeToDouble(recordset.Fields.Item("U_CompletionPct").Value)
                    };

                    lines.Add(line);
                    recordset.MoveNext();
                }

                return lines;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting performance obligation lines for {obligationCode}", ex);
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
        /// Create performance obligation
        /// </summary>
        public string CreatePerformanceObligation(PerformanceObligation obligation)
        {
            try
            {
                Logger.Info($"Creating performance obligation for contract: {obligation.ContractCode}");

                // Validate
                var errors = obligation.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                // Use UDO to create
                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_PERF_OBL");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set header fields
                generalData.SetProperty("U_ContractCode", obligation.ContractCode);
                generalData.SetProperty("U_OblNumber", obligation.ObligationNumber);
                generalData.SetProperty("U_Description", obligation.Description ?? "");
                generalData.SetProperty("U_Type", obligation.Type ?? PerformanceObligation.TYPE_SERVICE);
                generalData.SetProperty("U_StandaloneSP", obligation.StandaloneSellingPrice);
                generalData.SetProperty("U_AllocatedPrice", obligation.AllocatedPrice);
                generalData.SetProperty("U_RecogMethod", obligation.RecognitionMethod ?? PerformanceObligation.RECOG_OVER_TIME);
                generalData.SetProperty("U_ProgressMethod", obligation.ProgressMethod ?? "");
                generalData.SetProperty("U_TotalEstCost", obligation.TotalEstimatedCost);
                generalData.SetProperty("U_TotalUnits", obligation.TotalUnits);
                generalData.SetProperty("U_Status", obligation.Status ?? PerformanceObligation.STATUS_NOT_STARTED);
                generalData.SetProperty("U_CompletionPct", obligation.CompletionPercentage);
                generalData.SetProperty("U_CreateDate", DateTime.Now);
                generalData.SetProperty("U_CreateUser", _company.UserName);

                // Add lines
                if (obligation.Lines != null && obligation.Lines.Count > 0)
                {
                    GeneralDataCollection lines = generalData.Child("CM_PERF_OBL_LNS");
                    foreach (var line in obligation.Lines)
                    {
                        GeneralData lineData = lines.Add();
                        lineData.SetProperty("U_ItemCode", line.ItemCode ?? "");
                        lineData.SetProperty("U_Description", line.Description ?? "");
                        lineData.SetProperty("U_Quantity", line.Quantity);
                        lineData.SetProperty("U_EstimatedCost", line.EstimatedCost);
                        lineData.SetProperty("U_ActualCost", line.ActualCost);
                        lineData.SetProperty("U_CompletionPct", line.CompletionPercentage);
                    }
                }

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);
                string code = result.GetProperty("Code").ToString();

                Logger.Info($"Performance obligation created successfully: {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating performance obligation for contract {obligation.ContractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update performance obligation
        /// </summary>
        public void UpdatePerformanceObligation(PerformanceObligation obligation)
        {
            try
            {
                Logger.Info($"Updating performance obligation: {obligation.Code}");

                // Validate
                var errors = obligation.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_PERF_OBL");

                // Get existing data
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", obligation.Code);
                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update fields
                generalData.SetProperty("U_Description", obligation.Description ?? "");
                generalData.SetProperty("U_AllocatedPrice", obligation.AllocatedPrice);
                generalData.SetProperty("U_Status", obligation.Status ?? PerformanceObligation.STATUS_NOT_STARTED);
                generalData.SetProperty("U_CompletionPct", obligation.CompletionPercentage);
                generalData.SetProperty("U_TotalEstCost", obligation.TotalEstimatedCost);
                generalData.SetProperty("U_TotalUnits", obligation.TotalUnits);

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"Performance obligation updated successfully: {obligation.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating performance obligation {obligation.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Map recordset to PerformanceObligation object
        /// </summary>
        private PerformanceObligation MapPerformanceObligation(Recordset recordset)
        {
            return new PerformanceObligation
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                ObligationNumber = SafeConversion.SafeToInt(recordset.Fields.Item("U_OblNumber").Value),
                Description = SafeConversion.SafeToString(recordset.Fields.Item("U_Description").Value),
                Type = SafeConversion.SafeToString(recordset.Fields.Item("U_Type").Value),
                StandaloneSellingPrice = SafeConversion.SafeToDouble(recordset.Fields.Item("U_StandaloneSP").Value),
                AllocatedPrice = SafeConversion.SafeToDouble(recordset.Fields.Item("U_AllocatedPrice").Value),
                RecognitionMethod = SafeConversion.SafeToString(recordset.Fields.Item("U_RecogMethod").Value),
                ProgressMethod = SafeConversion.SafeToString(recordset.Fields.Item("U_ProgressMethod").Value),
                TotalEstimatedCost = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalEstCost").Value),
                TotalUnits = SafeConversion.SafeToDouble(recordset.Fields.Item("U_TotalUnits").Value),
                Status = SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value),
                CompletionPercentage = SafeConversion.SafeToDouble(recordset.Fields.Item("U_CompletionPct").Value),
                CreateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CreateDate").Value),
                CreateUser = SafeConversion.SafeToString(recordset.Fields.Item("U_CreateUser").Value)
            };
        }

        #endregion

        #region Revenue Schedule

        /// <summary>
        /// Get revenue schedule for a contract
        /// </summary>
        public List<RevenueSchedule> GetRevenueScheduleByContract(string contractCode)
        {
            List<RevenueSchedule> schedules = new List<RevenueSchedule>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_REV_SCHEDULE""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 ORDER BY ""U_PeriodStartDate""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    schedules.Add(MapRevenueSchedule(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {schedules.Count} revenue schedule entries for contract {contractCode}");
                return schedules;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting revenue schedule for contract {contractCode}", ex);
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
        /// Create revenue schedule entry
        /// </summary>
        public string CreateRevenueSchedule(RevenueSchedule schedule)
        {
            try
            {
                Logger.Info($"Creating revenue schedule entry for contract: {schedule.ContractCode}");

                // Validate
                var errors = schedule.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed: " + string.Join(", ", errors));
                }

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_REV_SCHEDULE");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set fields
                generalData.SetProperty("U_ContractCode", schedule.ContractCode);
                generalData.SetProperty("U_PerfOblCode", schedule.PerformanceObligationCode ?? "");
                generalData.SetProperty("U_PeriodStartDate", schedule.PeriodStartDate);
                generalData.SetProperty("U_PeriodEndDate", schedule.PeriodEndDate);
                generalData.SetProperty("U_ScheduledRevenue", schedule.ScheduledRevenue);
                generalData.SetProperty("U_RecognizedRevenue", schedule.RecognizedRevenue);
                generalData.SetProperty("U_DeferredRevenue", schedule.DeferredRevenue);
                generalData.SetProperty("U_CumulativeRevenue", schedule.CumulativeRevenue);
                generalData.SetProperty("U_RecogBasis", schedule.RecognitionBasis ?? "");
                generalData.SetProperty("U_ProgressPct", schedule.ProgressPercentage);
                generalData.SetProperty("U_Status", schedule.Status ?? RevenueSchedule.STATUS_SCHEDULED);
                generalData.SetProperty("U_CreateDate", DateTime.Now);
                generalData.SetProperty("U_CreateUser", _company.UserName);

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);
                string code = result.GetProperty("Code").ToString();

                Logger.Info($"Revenue schedule created successfully: {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating revenue schedule for contract {schedule.ContractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update revenue schedule entry
        /// </summary>
        public void UpdateRevenueSchedule(RevenueSchedule schedule)
        {
            try
            {
                Logger.Info($"Updating revenue schedule: {schedule.Code}");

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_REV_SCHEDULE");

                // Get existing data
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", schedule.Code);
                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update fields
                generalData.SetProperty("U_RecognizedRevenue", schedule.RecognizedRevenue);
                generalData.SetProperty("U_DeferredRevenue", schedule.DeferredRevenue);
                generalData.SetProperty("U_CumulativeRevenue", schedule.CumulativeRevenue);
                generalData.SetProperty("U_ProgressPct", schedule.ProgressPercentage);
                generalData.SetProperty("U_Status", schedule.Status);

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"Revenue schedule updated successfully: {schedule.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating revenue schedule {schedule.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Map recordset to RevenueSchedule object
        /// </summary>
        private RevenueSchedule MapRevenueSchedule(Recordset recordset)
        {
            return new RevenueSchedule
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                PerformanceObligationCode = SafeConversion.SafeToString(recordset.Fields.Item("U_PerfOblCode").Value),
                PeriodStartDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_PeriodStartDate").Value),
                PeriodEndDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_PeriodEndDate").Value),
                ScheduledRevenue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ScheduledRevenue").Value),
                RecognizedRevenue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_RecognizedRevenue").Value),
                DeferredRevenue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_DeferredRevenue").Value),
                CumulativeRevenue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_CumulativeRevenue").Value),
                RecognitionBasis = SafeConversion.SafeToString(recordset.Fields.Item("U_RecogBasis").Value),
                ProgressPercentage = SafeConversion.SafeToDouble(recordset.Fields.Item("U_ProgressPct").Value),
                Status = SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value),
                CreateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CreateDate").Value),
                CreateUser = SafeConversion.SafeToString(recordset.Fields.Item("U_CreateUser").Value)
            };
        }

        #endregion

        #region Deferred Revenue

        /// <summary>
        /// Get deferred revenue for a contract
        /// </summary>
        public List<DeferredRevenue> GetDeferredRevenueByContract(string contractCode)
        {
            List<DeferredRevenue> deferredRevenues = new List<DeferredRevenue>();
            Recordset recordset = null;

            try
            {
                string query = $@"SELECT * FROM ""@CM_DEFERRED_REV""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 AND ""U_Status"" = 'Active'
                                 ORDER BY ""U_CreateDate""";

                recordset = DatabaseHelper.ExecuteQuery(_company, query);

                while (!recordset.EoF)
                {
                    deferredRevenues.Add(MapDeferredRevenue(recordset));
                    recordset.MoveNext();
                }

                Logger.Info($"Retrieved {deferredRevenues.Count} deferred revenue entries for contract {contractCode}");
                return deferredRevenues;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting deferred revenue for contract {contractCode}", ex);
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
        /// Get total deferred revenue for a contract
        /// </summary>
        public double GetTotalDeferredRevenue(string contractCode)
        {
            try
            {
                string query = $@"SELECT COALESCE(SUM(""U_DeferredAmount""), 0) AS TotalDeferred
                                 FROM ""@CM_DEFERRED_REV""
                                 WHERE ""U_ContractCode"" = '{DatabaseHelper.EscapeSqlString(contractCode)}'
                                 AND ""U_Status"" = 'Active'";

                object result = DatabaseHelper.ExecuteScalar(_company, query);
                return SafeConversion.SafeToDouble(result, 0);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting total deferred revenue for contract {contractCode}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Create deferred revenue entry
        /// </summary>
        public string CreateDeferredRevenue(DeferredRevenue deferredRevenue)
        {
            try
            {
                Logger.Info($"Creating deferred revenue for contract: {deferredRevenue.ContractCode}");

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_DEFERRED_REV");
                GeneralData generalData = (GeneralData)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);

                // Set fields
                generalData.SetProperty("U_ContractCode", deferredRevenue.ContractCode);
                generalData.SetProperty("U_PerformObligCode", deferredRevenue.PerformanceObligationCode ?? "");
                generalData.SetProperty("U_IPCCode", deferredRevenue.IPCCode ?? "");
                generalData.SetProperty("U_BilledAmount", deferredRevenue.BilledAmount);
                generalData.SetProperty("U_RecognizedRevenue", deferredRevenue.RecognizedRevenue);
                generalData.SetProperty("U_DeferredAmount", deferredRevenue.DeferredAmount);
                generalData.SetProperty("U_DeferralReason", deferredRevenue.DeferralReason ?? "");
                generalData.SetProperty("U_ReleaseSchedule", deferredRevenue.ReleaseSchedule ?? "");
                generalData.SetProperty("U_Status", deferredRevenue.Status ?? DeferredRevenue.STATUS_ACTIVE);
                generalData.SetProperty("U_CreateDate", DateTime.Now);
                generalData.SetProperty("U_CreateUser", _company.UserName);

                // Add to database
                GeneralDataParams result = generalService.Add(generalData);
                string code = result.GetProperty("Code").ToString();

                Logger.Info($"Deferred revenue created successfully: {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating deferred revenue for contract {deferredRevenue.ContractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update deferred revenue entry
        /// </summary>
        public void UpdateDeferredRevenue(DeferredRevenue deferredRevenue)
        {
            try
            {
                Logger.Info($"Updating deferred revenue: {deferredRevenue.Code}");

                CompanyService companyService = _company.GetCompanyService();
                GeneralService generalService = companyService.GetGeneralService("CM_DEFERRED_REV");

                // Get by key
                GeneralDataParams generalParams = (GeneralDataParams)generalService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams);
                generalParams.SetProperty("Code", deferredRevenue.Code);

                GeneralData generalData = generalService.GetByParams(generalParams);

                // Update fields
                generalData.SetProperty("U_ContractCode", deferredRevenue.ContractCode);
                generalData.SetProperty("U_PerformObligCode", deferredRevenue.PerformanceObligationCode ?? "");
                generalData.SetProperty("U_IPCCode", deferredRevenue.IPCCode ?? "");
                generalData.SetProperty("U_BilledAmount", deferredRevenue.BilledAmount);
                generalData.SetProperty("U_RecognizedRevenue", deferredRevenue.RecognizedRevenue);
                generalData.SetProperty("U_DeferredAmount", deferredRevenue.DeferredAmount);
                generalData.SetProperty("U_DeferralReason", deferredRevenue.DeferralReason ?? "");
                generalData.SetProperty("U_ReleaseSchedule", deferredRevenue.ReleaseSchedule ?? "");
                generalData.SetProperty("U_Status", deferredRevenue.Status ?? DeferredRevenue.STATUS_ACTIVE);

                // Update in database
                generalService.Update(generalData);

                Logger.Info($"Deferred revenue updated successfully: {deferredRevenue.Code}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating deferred revenue {deferredRevenue.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Map recordset to DeferredRevenue object
        /// </summary>
        private DeferredRevenue MapDeferredRevenue(Recordset recordset)
        {
            return new DeferredRevenue
            {
                Code = SafeConversion.SafeToString(recordset.Fields.Item("Code").Value),
                DocNum = SafeConversion.SafeToString(recordset.Fields.Item("DocNum").Value),
                ContractCode = SafeConversion.SafeToString(recordset.Fields.Item("U_ContractCode").Value),
                PerformanceObligationCode = SafeConversion.SafeToString(recordset.Fields.Item("U_PerformObligCode").Value),
                IPCCode = SafeConversion.SafeToString(recordset.Fields.Item("U_IPCCode").Value),
                BilledAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_BilledAmount").Value),
                RecognizedRevenue = SafeConversion.SafeToDouble(recordset.Fields.Item("U_RecognizedRevenue").Value),
                DeferredAmount = SafeConversion.SafeToDouble(recordset.Fields.Item("U_DeferredAmount").Value),
                DeferralReason = SafeConversion.SafeToString(recordset.Fields.Item("U_DeferralReason").Value),
                ReleaseSchedule = SafeConversion.SafeToString(recordset.Fields.Item("U_ReleaseSchedule").Value),
                Status = SafeConversion.SafeToString(recordset.Fields.Item("U_Status").Value),
                CreateDate = SafeConversion.SafeToDateTime(recordset.Fields.Item("U_CreateDate").Value),
                CreateUser = SafeConversion.SafeToString(recordset.Fields.Item("U_CreateUser").Value)
            };
        }

        #endregion

        #region Revenue Recognition Log

        /// <summary>
        /// Create revenue recognition log entry
        /// </summary>
        public void LogRevenueRecognition(RevenueRecognitionLog log)
        {
            try
            {
                Logger.Info($"Logging revenue recognition for contract: {log.ContractCode}");

                string query = $@"
                    INSERT INTO ""@CM_REV_LOG""
                    (""Code"", ""U_ContractCode"", ""U_PerfOblCode"", ""U_RecognitionDate"",
                     ""U_RecognitionAmount"", ""U_RecogMethod"", ""U_CumulativeAmount"",
                     ""U_JournalEntryRef"", ""U_Notes"", ""U_CreateDate"", ""U_CreateUser"")
                    VALUES
                    ('{Guid.NewGuid().ToString()}',
                     '{DatabaseHelper.EscapeSqlString(log.ContractCode)}',
                     '{DatabaseHelper.EscapeSqlString(log.PerformanceObligationCode ?? "")}',
                     '{log.RecognitionDate:yyyyMMdd}',
                     {log.RecognitionAmount},
                     '{DatabaseHelper.EscapeSqlString(log.RecognitionMethod ?? "")}',
                     {log.CumulativeAmount},
                     {(log.JournalEntryRef.HasValue ? log.JournalEntryRef.Value.ToString() : "NULL")},
                     '{DatabaseHelper.EscapeSqlString(log.Notes ?? "")}',
                     '{DateTime.Now:yyyyMMdd}',
                     '{DatabaseHelper.EscapeSqlString(_company.UserName)}')";

                DatabaseHelper.ExecuteNonQuery(_company, query);

                Logger.Info("Revenue recognition logged successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error logging revenue recognition", ex);
                // Don't throw - logging failures shouldn't stop the process
            }
        }

        #endregion
    }
}
