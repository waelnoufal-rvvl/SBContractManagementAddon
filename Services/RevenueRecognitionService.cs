using System;
using System.Collections.Generic;
using System.Linq;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.DataAccess;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Revenue Recognition Service - ASC 606 / IFRS 15 Compliance
    /// Implements the five-step revenue recognition model
    /// </summary>
    public class RevenueRecognitionService
    {
        private Company _company;
        private RevenueRecognitionRepository _revenueRepo;
        private ContractRepository _contractRepo;
        private IPCRepository _ipcRepo;

        public RevenueRecognitionService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _revenueRepo = new RevenueRecognitionRepository(_company);
            _contractRepo = new ContractRepository(_company);
            _ipcRepo = new IPCRepository(_company);
        }

        #region ASC 606 Step 1: Identify the Contract

        /// <summary>
        /// ASC 606 Step 1: Validate contract meets revenue recognition criteria
        /// </summary>
        public bool ValidateContractCriteria(Contract contract)
        {
            try
            {
                Logger.Info($"Validating contract criteria for: {contract.Code}");

                // ASC 606 requires:
                // 1. Contract is approved and parties are committed
                // 2. Rights regarding goods/services can be identified
                // 3. Payment terms can be identified
                // 4. Contract has commercial substance
                // 5. Collection is probable

                List<string> validationErrors = new List<string>();

                // 1. Check if contract is approved
                if (contract.Status != "Active" && contract.Status != "Completed")
                {
                    validationErrors.Add("Contract must be Active or Completed status");
                }

                // 2. Check if contract has line items (rights to goods/services)
                if (contract.Lines == null || contract.Lines.Count == 0)
                {
                    validationErrors.Add("Contract must have line items");
                }

                // 3. Check if payment terms are identified
                if (string.IsNullOrWhiteSpace(contract.PaymentTerms))
                {
                    validationErrors.Add("Payment terms must be specified");
                }

                // 4. Commercial substance (has value > 0)
                if (contract.TotalValue <= 0)
                {
                    validationErrors.Add("Contract must have positive total value");
                }

                // 5. Collection probability (customer must exist)
                if (string.IsNullOrWhiteSpace(contract.CustomerCode))
                {
                    validationErrors.Add("Customer must be specified");
                }

                if (validationErrors.Count > 0)
                {
                    Logger.Warning($"Contract {contract.Code} failed validation: {string.Join(", ", validationErrors)}");
                    return false;
                }

                Logger.Info($"Contract {contract.Code} meets ASC 606 criteria");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error validating contract criteria: {contract.Code}", ex);
                return false;
            }
        }

        #endregion

        #region ASC 606 Step 2: Identify Performance Obligations

        /// <summary>
        /// ASC 606 Step 2: Identify distinct performance obligations in the contract
        /// Creates performance obligations based on contract lines
        /// </summary>
        public List<PerformanceObligation> IdentifyPerformanceObligations(string contractCode)
        {
            try
            {
                Logger.Info($"Identifying performance obligations for contract: {contractCode}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Check if contract meets criteria
                if (!ValidateContractCriteria(contract))
                {
                    throw new Exception("Contract does not meet ASC 606 criteria");
                }

                List<PerformanceObligation> obligations = new List<PerformanceObligation>();

                // Validate contract has line items
                if (contract.Lines == null || contract.Lines.Count == 0)
                {
                    throw new Exception("Contract must have line items to create performance obligations");
                }

                // Group contract lines into performance obligations
                // Each distinct good/service becomes a performance obligation
                int obligationNumber = 1;

                foreach (var line in contract.Lines)
                {
                    PerformanceObligation obligation = new PerformanceObligation
                    {
                        ContractCode = contractCode,
                        ObligationNumber = obligationNumber++,
                        Description = line.ItemDescription,
                        Type = DetermineObligationType(line),
                        StandaloneSellingPrice = line.UnitPrice * line.Quantity,
                        AllocatedPrice = 0, // Will be allocated in Step 4
                        RecognitionMethod = DetermineRecognitionMethod(line),
                        ProgressMethod = DetermineProgressMethod(line),
                        TotalEstimatedCost = 0, // To be updated during execution
                        TotalUnits = line.Quantity,
                        Status = PerformanceObligation.STATUS_NOT_STARTED,
                        CompletionPercentage = 0,
                        CreateDate = DateTime.Now,
                        CreateUser = _company.UserName
                    };

                    // Add line detail
                    obligation.Lines.Add(new PerformanceObligationLine
                    {
                        LineNum = 1,
                        ItemCode = line.ItemCode,
                        Description = line.ItemDescription,
                        Quantity = line.Quantity,
                        EstimatedCost = 0,
                        ActualCost = 0,
                        CompletionPercentage = 0
                    });

                    obligations.Add(obligation);
                }

                Logger.Info($"Identified {obligations.Count} performance obligations for contract {contractCode}");
                return obligations;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error identifying performance obligations for contract {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Determine obligation type based on line item
        /// </summary>
        private string DetermineObligationType(ContractLine line)
        {
            // Default to Service type
            // In a real implementation, this would check item master data
            return PerformanceObligation.TYPE_SERVICE;
        }

        /// <summary>
        /// Determine recognition method (Point in Time vs. Over Time)
        /// </summary>
        private string DetermineRecognitionMethod(ContractLine line)
        {
            // Default to Over Time for construction/service contracts
            // Point in Time would be for goods delivered at once
            return PerformanceObligation.RECOG_OVER_TIME;
        }

        /// <summary>
        /// Determine progress measurement method
        /// </summary>
        private string DetermineProgressMethod(ContractLine line)
        {
            // Default to Cost-to-Cost method
            // Could be overridden based on contract type or item type
            return PerformanceObligation.PROGRESS_COST_TO_COST;
        }

        /// <summary>
        /// Get performance obligations for a contract
        /// </summary>
        public List<PerformanceObligation> GetPerformanceObligations(string contractCode)
        {
            try
            {
                return _revenueRepo.GetPerformanceObligationsByContract(contractCode);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting performance obligations for contract {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create and save performance obligations for a contract
        /// </summary>
        public void CreatePerformanceObligations(string contractCode)
        {
            try
            {
                Logger.Info($"Creating performance obligations for contract: {contractCode}");

                // Check if obligations already exist
                List<PerformanceObligation> existing = _revenueRepo.GetPerformanceObligationsByContract(contractCode);
                if (existing.Count > 0)
                {
                    Logger.Warning($"Performance obligations already exist for contract {contractCode}");
                    return;
                }

                // Identify obligations
                List<PerformanceObligation> obligations = IdentifyPerformanceObligations(contractCode);

                // Allocate transaction price (Step 4)
                AllocateTransactionPrice(contractCode, obligations);

                // Save to database
                foreach (var obligation in obligations)
                {
                    string code = _revenueRepo.CreatePerformanceObligation(obligation);
                    Logger.Info($"Created performance obligation: {code}");
                }

                Logger.Info($"Successfully created {obligations.Count} performance obligations for contract {contractCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating performance obligations for contract {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region ASC 606 Step 3: Determine Transaction Price

        /// <summary>
        /// ASC 606 Step 3: Determine the transaction price
        /// Includes variable consideration, financing components, etc.
        /// </summary>
        public double DetermineTransactionPrice(Contract contract)
        {
            try
            {
                Logger.Info($"Determining transaction price for contract: {contract.Code}");

                // Start with contract total value
                double transactionPrice = contract.TotalValue;

                // Adjust for variable consideration (penalties, bonuses, etc.)
                // In a full implementation, this would check for:
                // - Performance bonuses
                // - Penalties
                // - Discounts
                // - Price concessions

                // Adjust for significant financing component
                // Not implemented in this version

                // Adjust for non-cash consideration
                // Not implemented in this version

                // Adjust for consideration payable to customer
                // Not implemented in this version

                Logger.Info($"Transaction price for contract {contract.Code}: {transactionPrice}");
                return transactionPrice;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error determining transaction price for contract {contract.Code}", ex);
                throw;
            }
        }

        #endregion

        #region ASC 606 Step 4: Allocate Transaction Price

        /// <summary>
        /// ASC 606 Step 4: Allocate transaction price to performance obligations
        /// Uses relative standalone selling price method
        /// </summary>
        public void AllocateTransactionPrice(string contractCode, List<PerformanceObligation> obligations)
        {
            try
            {
                Logger.Info($"Allocating transaction price for contract: {contractCode}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Get transaction price
                double transactionPrice = DetermineTransactionPrice(contract);

                // Calculate total standalone selling price
                double totalStandaloneSP = 0;
                foreach (var obligation in obligations)
                {
                    totalStandaloneSP += obligation.StandaloneSellingPrice;
                }

                if (totalStandaloneSP <= 0)
                {
                    throw new Exception("Total standalone selling price must be greater than zero");
                }

                // Allocate transaction price proportionally
                foreach (var obligation in obligations)
                {
                    // Allocated Price = Transaction Price × (Standalone SP / Total Standalone SP)
                    obligation.AllocatedPrice = transactionPrice * (obligation.StandaloneSellingPrice / totalStandaloneSP);

                    Logger.Info($"Allocated {obligation.AllocatedPrice} to obligation {obligation.ObligationNumber} " +
                               $"(Standalone SP: {obligation.StandaloneSellingPrice})");
                }

                Logger.Info($"Successfully allocated transaction price for contract {contractCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error allocating transaction price for contract {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region ASC 606 Step 5: Recognize Revenue

        /// <summary>
        /// ASC 606 Step 5: Recognize revenue as performance obligations are satisfied
        /// </summary>
        public void RecognizeRevenue(string contractCode, DateTime periodDate)
        {
            try
            {
                Logger.Info($"Recognizing revenue for contract {contractCode} as of {periodDate:yyyy-MM-dd}");

                // Get all performance obligations
                List<PerformanceObligation> obligations = _revenueRepo.GetPerformanceObligationsByContract(contractCode);

                if (obligations.Count == 0)
                {
                    Logger.Warning($"No performance obligations found for contract {contractCode}");
                    return;
                }

                double totalRevenueRecognized = 0;

                foreach (var obligation in obligations)
                {
                    // Update completion percentage
                    obligation.CalculateCompletionPercentage();

                    // Calculate revenue to recognize
                    double revenueToRecognize = obligation.CalculateRevenueToRecognize();

                    // Create or update revenue schedule
                    UpdateRevenueSchedule(obligation, periodDate, revenueToRecognize);

                    // Log revenue recognition
                    LogRevenueRecognition(obligation, periodDate, revenueToRecognize);

                    // Update obligation status
                    if (obligation.CompletionPercentage >= 100)
                    {
                        obligation.Status = PerformanceObligation.STATUS_COMPLETED;
                    }
                    else if (obligation.CompletionPercentage > 0)
                    {
                        obligation.Status = PerformanceObligation.STATUS_IN_PROGRESS;
                    }

                    // Update obligation in database
                    _revenueRepo.UpdatePerformanceObligation(obligation);

                    totalRevenueRecognized += revenueToRecognize;

                    Logger.Info($"Recognized revenue: {revenueToRecognize} for obligation {obligation.ObligationNumber} " +
                               $"(Completion: {obligation.CompletionPercentage:F2}%)");
                }

                // Update contract with total recognized revenue
                UpdateContractRevenueFields(contractCode, totalRevenueRecognized);

                Logger.Info($"Total revenue recognized for contract {contractCode}: {totalRevenueRecognized}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error recognizing revenue for contract {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Recognize revenue based on IPC approval
        /// </summary>
        public void RecognizeRevenueFromIPC(string ipcCode)
        {
            try
            {
                Logger.Info($"Recognizing revenue from IPC: {ipcCode}");

                IPC ipc = _ipcRepo.GetByCode(ipcCode);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {ipcCode}");
                }

                if (ipc.Status != "Approved" && ipc.Status != "Paid")
                {
                    Logger.Warning($"IPC {ipcCode} is not approved. Status: {ipc.Status}");
                    return;
                }

                // Get performance obligations for the contract
                List<PerformanceObligation> obligations = _revenueRepo.GetPerformanceObligationsByContract(ipc.ContractCode);

                // Validate obligations exist
                if (obligations == null || obligations.Count == 0)
                {
                    Logger.Warning($"No performance obligations found for contract {ipc.ContractCode}. Cannot recognize revenue from IPC {ipcCode}.");
                    return;
                }

                // Calculate total billed amount
                double totalBilled = ipc.GrossAmount;

                // Get total allocated price
                double totalAllocatedPrice = obligations.Sum(o => o.AllocatedPrice);

                // Validate total allocated price
                if (totalAllocatedPrice <= 0)
                {
                    Logger.Warning($"Total allocated price is zero or negative for contract {ipc.ContractCode}. Cannot recognize revenue from IPC {ipcCode}.");
                    return;
                }

                // Recognize revenue proportionally
                foreach (var obligation in obligations)
                {
                    // Skip obligations with zero or negative allocated price
                    if (obligation.AllocatedPrice <= 0)
                    {
                        Logger.Warning($"Skipping obligation {obligation.Code} with zero/negative allocated price");
                        continue;
                    }

                    // Calculate proportion of this IPC for this obligation
                    double obligationShare = (obligation.AllocatedPrice / totalAllocatedPrice) * totalBilled;

                    // Check if revenue should be deferred
                    bool shouldDefer = ShouldDeferRevenue(obligation, obligationShare);

                    if (shouldDefer)
                    {
                        // Create deferred revenue entry
                        CreateDeferredRevenueEntry(ipc, obligation, obligationShare);
                    }
                    else
                    {
                        // Recognize revenue immediately
                        UpdateRevenueSchedule(obligation, ipc.IPCDate, obligationShare);
                    }
                }

                // Update contract assets/liabilities
                UpdateContractAssetsLiabilities(ipc.ContractCode);

                Logger.Info($"Revenue recognition from IPC {ipcCode} completed");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error recognizing revenue from IPC {ipcCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update revenue schedule for a performance obligation
        /// </summary>
        private void UpdateRevenueSchedule(PerformanceObligation obligation, DateTime periodDate, double revenueAmount)
        {
            try
            {
                // Get existing schedule for the period
                List<RevenueSchedule> schedules = _revenueRepo.GetRevenueScheduleByContract(obligation.ContractCode);

                // Find schedule for current period (month)
                DateTime periodStart = new DateTime(periodDate.Year, periodDate.Month, 1);
                DateTime periodEnd = periodStart.AddMonths(1).AddDays(-1);

                RevenueSchedule schedule = schedules.FirstOrDefault(s =>
                    s.PerformanceObligationCode == obligation.Code &&
                    s.PeriodStartDate == periodStart);

                bool isNewSchedule = false;

                if (schedule == null)
                {
                    // Create new schedule
                    isNewSchedule = true;
                    schedule = new RevenueSchedule
                    {
                        ContractCode = obligation.ContractCode,
                        PerformanceObligationCode = obligation.Code,
                        PeriodStartDate = periodStart,
                        PeriodEndDate = periodEnd,
                        ScheduledRevenue = revenueAmount,
                        RecognizedRevenue = revenueAmount,
                        RecognitionBasis = obligation.ProgressMethod,
                        ProgressPercentage = obligation.CompletionPercentage,
                        Status = RevenueSchedule.STATUS_RECOGNIZED,
                        CreateDate = DateTime.Now,
                        CreateUser = _company.UserName
                    };
                }
                else
                {
                    // Update existing schedule - SET to the total for the period, not ADD
                    // This prevents double-counting if method is called multiple times
                    if (revenueAmount > schedule.RecognizedRevenue)
                    {
                        schedule.RecognizedRevenue = revenueAmount;
                        schedule.ScheduledRevenue = Math.Max(schedule.ScheduledRevenue, revenueAmount);
                        schedule.ProgressPercentage = obligation.CompletionPercentage;
                        schedule.Status = RevenueSchedule.STATUS_RECOGNIZED;
                    }
                }

                // Calculate cumulative revenue (all prior periods + current)
                if (isNewSchedule)
                {
                    // For new schedule, sum all existing prior periods plus this new one
                    double cumulativeRevenue = schedules
                        .Where(s => s.PerformanceObligationCode == obligation.Code && s.PeriodEndDate < periodStart)
                        .Sum(s => s.RecognizedRevenue) + schedule.RecognizedRevenue;
                    schedule.CumulativeRevenue = cumulativeRevenue;
                }
                else
                {
                    // For existing schedule, recalculate from all schedules including this one
                    double cumulativeRevenue = schedules
                        .Where(s => s.PerformanceObligationCode == obligation.Code && s.PeriodEndDate < periodEnd)
                        .Sum(s => s.RecognizedRevenue) + schedule.RecognizedRevenue;
                    schedule.CumulativeRevenue = cumulativeRevenue;
                }

                schedule.CalculateDeferredRevenue();

                // Only one database operation
                if (isNewSchedule)
                {
                    _revenueRepo.CreateRevenueSchedule(schedule);
                    Logger.Info($"Created revenue schedule for obligation {obligation.Code}, period {periodStart:yyyy-MM-dd}, amount {revenueAmount:N2}");
                }
                else
                {
                    _revenueRepo.UpdateRevenueSchedule(schedule);
                    Logger.Info($"Updated revenue schedule for obligation {obligation.Code}, period {periodStart:yyyy-MM-dd}, amount {revenueAmount:N2}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating revenue schedule for obligation {obligation.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Log revenue recognition transaction
        /// </summary>
        private void LogRevenueRecognition(PerformanceObligation obligation, DateTime recognitionDate, double amount)
        {
            try
            {
                RevenueRecognitionLog log = new RevenueRecognitionLog
                {
                    ContractCode = obligation.ContractCode,
                    PerformanceObligationCode = obligation.Code,
                    RecognitionDate = recognitionDate,
                    RecognitionAmount = amount,
                    RecognitionMethod = obligation.RecognitionMethod,
                    CumulativeAmount = 0, // Would be calculated from schedule
                    JournalEntryRef = null, // Would link to GL posting
                    Notes = $"Revenue recognized for {obligation.Description}",
                    CreateDate = DateTime.Now,
                    CreateUser = _company.UserName
                };

                _revenueRepo.LogRevenueRecognition(log);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error logging revenue recognition for obligation {obligation.Code}", ex);
                // Don't throw - logging failures shouldn't stop the process
            }
        }

        #endregion

        #region Deferred Revenue Management

        /// <summary>
        /// Determine if revenue should be deferred
        /// </summary>
        private bool ShouldDeferRevenue(PerformanceObligation obligation, double billedAmount)
        {
            // Revenue should be deferred if:
            // 1. Recognition method is Over Time and obligation is not yet started/completed
            // 2. Billed amount exceeds revenue earned based on progress

            if (obligation.RecognitionMethod == PerformanceObligation.RECOG_OVER_TIME)
            {
                double earnedRevenue = obligation.CalculateRevenueToRecognize();
                if (billedAmount > earnedRevenue)
                {
                    return true; // Billing ahead of performance
                }
            }

            return false;
        }

        /// <summary>
        /// Create deferred revenue entry
        /// </summary>
        private void CreateDeferredRevenueEntry(IPC ipc, PerformanceObligation obligation, double amount)
        {
            try
            {
                double earnedRevenue = obligation.CalculateRevenueToRecognize();
                double deferredAmount = amount - earnedRevenue;

                if (deferredAmount <= 0)
                {
                    return; // Nothing to defer
                }

                DeferredRevenue deferredRevenue = new DeferredRevenue
                {
                    ContractCode = ipc.ContractCode,
                    PerformanceObligationCode = obligation.Code,
                    IPCCode = ipc.Code,
                    BilledAmount = amount,
                    RecognizedRevenue = earnedRevenue,
                    DeferredAmount = deferredAmount,
                    DeferralReason = DeferredRevenue.REASON_AHEAD_OF_DELIVERY,
                    Status = DeferredRevenue.STATUS_ACTIVE,
                    CreateDate = DateTime.Now,
                    CreateUser = _company.UserName
                };

                _revenueRepo.CreateDeferredRevenue(deferredRevenue);

                Logger.Info($"Created deferred revenue entry: Billed={amount}, Earned={earnedRevenue}, Deferred={deferredAmount}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating deferred revenue entry", ex);
                throw;
            }
        }

        /// <summary>
        /// Release deferred revenue as performance obligations are satisfied
        /// </summary>
        public void ReleaseDeferredRevenue(string contractCode)
        {
            try
            {
                Logger.Info($"Releasing deferred revenue for contract: {contractCode}");

                List<DeferredRevenue> deferredRevenues = _revenueRepo.GetDeferredRevenueByContract(contractCode);

                // Validate deferred revenues exist
                if (deferredRevenues == null || deferredRevenues.Count == 0)
                {
                    Logger.Info($"No deferred revenue to release for contract {contractCode}");
                    return;
                }

                List<PerformanceObligation> obligations = _revenueRepo.GetPerformanceObligationsByContract(contractCode);

                // Validate obligations exist
                if (obligations == null || obligations.Count == 0)
                {
                    Logger.Warning($"No performance obligations found for contract {contractCode}. Cannot release deferred revenue.");
                    return;
                }

                foreach (var deferred in deferredRevenues)
                {
                    if (deferred.Status != DeferredRevenue.STATUS_ACTIVE)
                    {
                        continue;
                    }

                    // Find the SPECIFIC performance obligation for this deferred revenue
                    PerformanceObligation obligation = obligations.FirstOrDefault(o => o.Code == deferred.PerformanceObligationCode);

                    if (obligation == null)
                    {
                        Logger.Warning($"Performance obligation {deferred.PerformanceObligationCode} not found for deferred revenue {deferred.Code}");
                        continue;
                    }

                    // Calculate how much can be released based on progress
                    double earnedRevenue = obligation.CalculateRevenueToRecognize();
                    double alreadyRecognized = deferred.RecognizedRevenue;
                    double canRelease = Math.Min(deferred.DeferredAmount, earnedRevenue - alreadyRecognized);

                    if (canRelease > 0)
                    {
                        deferred.ReleaseRevenue(canRelease);

                        // Update deferred revenue record in database
                        _revenueRepo.UpdateDeferredRevenue(deferred);

                        Logger.Info($"Released {canRelease} deferred revenue for obligation {obligation.Code}. " +
                            $"Remaining deferred: {deferred.DeferredAmount}");
                    }
                }

                Logger.Info($"Deferred revenue release completed for contract {contractCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error releasing deferred revenue for contract {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region Contract Assets & Liabilities

        /// <summary>
        /// Update contract assets and liabilities
        /// Contract Asset = Revenue Recognized > Billed (Unbilled Revenue)
        /// Contract Liability = Billed > Revenue Recognized (Deferred Revenue)
        /// </summary>
        public void UpdateContractAssetsLiabilities(string contractCode)
        {
            try
            {
                Logger.Info($"Updating contract assets/liabilities for: {contractCode}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Calculate total billed (from IPCs)
                double totalBilled = _ipcRepo.GetTotalIPCAmount(contractCode);

                // Calculate total revenue recognized
                List<RevenueSchedule> schedules = _revenueRepo.GetRevenueScheduleByContract(contractCode);
                double totalRevenueRecognized = schedules.Sum(s => s.RecognizedRevenue);

                // Calculate contract asset/liability
                double contractAsset = 0;
                double contractLiability = 0;

                if (totalRevenueRecognized > totalBilled)
                {
                    // We've earned more than we've billed - Contract Asset (Unbilled Revenue/WIP)
                    contractAsset = totalRevenueRecognized - totalBilled;
                }
                else if (totalBilled > totalRevenueRecognized)
                {
                    // We've billed more than we've earned - Contract Liability (Deferred Revenue)
                    contractLiability = totalBilled - totalRevenueRecognized;
                }

                Logger.Info($"Contract Assets: {contractAsset}, Contract Liabilities: {contractLiability}");

                // This would be saved to @CM_CONTRACT_ASSETS table in a full implementation
                // For now, we log the values
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating contract assets/liabilities for {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region Revenue Forecasting & Backlog

        /// <summary>
        /// Calculate contract backlog and revenue forecast
        /// </summary>
        public ContractBacklog CalculateBacklog(string contractCode)
        {
            try
            {
                Logger.Info($"Calculating backlog for contract: {contractCode}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Get revenue recognized to date
                List<RevenueSchedule> schedules = _revenueRepo.GetRevenueScheduleByContract(contractCode);
                double revenueRecognizedToDate = schedules.Sum(s => s.RecognizedRevenue);

                // Get billed to date
                double billedToDate = _ipcRepo.GetTotalIPCAmount(contractCode);

                // Create backlog record
                ContractBacklog backlog = new ContractBacklog
                {
                    ContractCode = contractCode,
                    AsOfDate = DateTime.Now,
                    TotalContractValue = contract.TotalValue,
                    BilledToDate = billedToDate,
                    RevenueRecognizedToDate = revenueRecognizedToDate,
                    CreateDate = DateTime.Now,
                    CreateUser = _company.UserName
                };

                // Calculate remaining backlog
                backlog.CalculateRemainingBacklog();

                // Calculate burn rate
                backlog.CalculateBurnRate(contract.StartDate);

                // Estimate completion date
                backlog.EstimateCompletionDate();

                // Calculate revenue forecasts
                backlog.CalculateForecasts();

                Logger.Info($"Backlog calculated: Remaining={backlog.RemainingBacklog}, " +
                           $"BurnRate={backlog.BurnRate}, " +
                           $"EstCompletion={backlog.EstimatedCompletionDate:yyyy-MM-dd}");

                return backlog;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error calculating backlog for contract {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Update contract with recognized revenue totals
        /// </summary>
        private void UpdateContractRevenueFields(string contractCode, double totalRevenueRecognized)
        {
            try
            {
                // In a full implementation, this would update the contract record
                // with revenue recognition fields added to the contract model
                Logger.Info($"Contract {contractCode} total revenue recognized: {totalRevenueRecognized}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating contract revenue fields for {contractCode}", ex);
                // Don't throw - this is informational
            }
        }

        /// <summary>
        /// Get revenue recognition summary for a contract
        /// </summary>
        public Dictionary<string, object> GetRevenueSummary(string contractCode)
        {
            try
            {
                Logger.Info($"Getting revenue summary for contract: {contractCode}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Get performance obligations
                List<PerformanceObligation> obligations = _revenueRepo.GetPerformanceObligationsByContract(contractCode);

                // Get revenue schedules
                List<RevenueSchedule> schedules = _revenueRepo.GetRevenueScheduleByContract(contractCode);

                // Get deferred revenue
                double totalDeferred = _revenueRepo.GetTotalDeferredRevenue(contractCode);

                // Calculate totals
                double totalAllocatedPrice = obligations.Sum(o => o.AllocatedPrice);
                double totalRevenueRecognized = schedules.Sum(s => s.RecognizedRevenue);
                double totalBilled = _ipcRepo.GetTotalIPCAmount(contractCode);

                var summary = new Dictionary<string, object>
                {
                    ["ContractCode"] = contractCode,
                    ["ContractValue"] = contract.TotalValue,
                    ["PerformanceObligationCount"] = obligations.Count,
                    ["TotalAllocatedPrice"] = totalAllocatedPrice,
                    ["TotalRevenueRecognized"] = totalRevenueRecognized,
                    ["TotalBilled"] = totalBilled,
                    ["TotalDeferred"] = totalDeferred,
                    ["ContractAsset"] = Math.Max(0, totalRevenueRecognized - totalBilled),
                    ["ContractLiability"] = Math.Max(0, totalBilled - totalRevenueRecognized),
                    ["RevenueRecognitionPercentage"] = (contract.TotalValue > 0) ?
                        (totalRevenueRecognized / contract.TotalValue * 100) : 0,
                    ["AverageCompletion"] = obligations.Count > 0 ?
                        obligations.Average(o => o.CompletionPercentage) : 0
                };

                Logger.Info($"Revenue summary generated for contract {contractCode}");
                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting revenue summary for contract {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Generate revenue recognition schedule for a period
        /// </summary>
        public void GenerateRevenueSchedule(string contractCode, DateTime startDate, DateTime endDate, int intervalMonths = 1)
        {
            try
            {
                Logger.Info($"Generating revenue schedule for contract {contractCode} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Get performance obligations
                List<PerformanceObligation> obligations = _revenueRepo.GetPerformanceObligationsByContract(contractCode);

                if (obligations.Count == 0)
                {
                    Logger.Warning($"No performance obligations found for contract {contractCode}");
                    return;
                }

                // Generate schedule for each period
                DateTime currentPeriodStart = startDate;

                while (currentPeriodStart < endDate)
                {
                    DateTime currentPeriodEnd = currentPeriodStart.AddMonths(intervalMonths).AddDays(-1);
                    if (currentPeriodEnd > endDate)
                    {
                        currentPeriodEnd = endDate;
                    }

                    // Calculate scheduled revenue for this period
                    // For time-based recognition, divide evenly across periods
                    int totalPeriods = ((endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month) / intervalMonths;
                    if (totalPeriods <= 0) totalPeriods = 1;

                    foreach (var obligation in obligations)
                    {
                        double scheduledRevenue = obligation.AllocatedPrice / totalPeriods;

                        RevenueSchedule schedule = new RevenueSchedule
                        {
                            ContractCode = contractCode,
                            PerformanceObligationCode = obligation.Code,
                            PeriodStartDate = currentPeriodStart,
                            PeriodEndDate = currentPeriodEnd,
                            ScheduledRevenue = scheduledRevenue,
                            RecognizedRevenue = 0,
                            DeferredRevenue = scheduledRevenue,
                            CumulativeRevenue = 0,
                            RecognitionBasis = obligation.ProgressMethod,
                            ProgressPercentage = 0,
                            Status = RevenueSchedule.STATUS_SCHEDULED,
                            CreateDate = DateTime.Now,
                            CreateUser = _company.UserName
                        };

                        _revenueRepo.CreateRevenueSchedule(schedule);
                    }

                    currentPeriodStart = currentPeriodEnd.AddDays(1);
                }

                Logger.Info($"Revenue schedule generated successfully for contract {contractCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error generating revenue schedule for contract {contractCode}", ex);
                throw;
            }
        }

        #endregion
    }
}
