using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.DataAccess;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Business logic service for Change Orders
    /// </summary>
    public class ChangeOrderService
    {
        private Company _company;
        private ChangeOrderRepository _coRepo;
        private ContractRepository _contractRepo;
        private IPCRepository _ipcRepo;
        private CurrencyService _currencyService;
        private RevenueRecognitionService _revenueService;

        public ChangeOrderService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _coRepo = new ChangeOrderRepository(_company);
            _contractRepo = new ContractRepository(_company);
            _ipcRepo = new IPCRepository(_company);
            _currencyService = new CurrencyService(_company);
            _revenueService = new RevenueRecognitionService(_company);
        }

        /// <summary>
        /// Get all change orders for a contract
        /// </summary>
        public List<ChangeOrder> GetChangeOrdersByContract(string contractCode)
        {
            try
            {
                return _coRepo.GetByContract(contractCode);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetChangeOrdersByContract: {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get change order by code
        /// </summary>
        public ChangeOrder GetChangeOrder(string code)
        {
            try
            {
                return _coRepo.GetByCode(code);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetChangeOrder: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create new change order
        /// </summary>
        public string CreateChangeOrder(ChangeOrder changeOrder)
        {
            try
            {
                // Validate contract exists
                Contract contract = _contractRepo.GetByCode(changeOrder.ContractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {changeOrder.ContractCode}");
                }

                // Validate change order amount against contract value
                if (changeOrder.Type == "Deduction")
                {
                    // Get total of all approved change orders
                    double totalCOs = _coRepo.GetTotalApprovedCOAmount(changeOrder.ContractCode);
                    double potentialNewValue = contract.TotalValue + totalCOs + changeOrder.GetSignedAmount();

                    if (potentialNewValue < 0)
                    {
                        throw new Exception($"Change order deduction would make contract value negative. " +
                            $"Contract value: {contract.TotalValue:N2}, " +
                            $"Existing COs: {totalCOs:N2}, " +
                            $"This CO: {changeOrder.Amount:N2}, " +
                            $"Resulting value: {potentialNewValue:N2}");
                    }

                    // Warn if reducing contract value to less than already billed
                    double totalBilled = _ipcRepo.GetTotalIPCAmount(changeOrder.ContractCode);
                    if (potentialNewValue < totalBilled)
                    {
                        Logger.Warning($"Warning: Change order will reduce contract value below billed amount. " +
                            $"New value: {potentialNewValue:N2}, Already billed: {totalBilled:N2}");
                        // Don't throw - allow but warn
                    }
                }

                // Auto-assign CO number
                if (changeOrder.ChangeOrderNumber == 0)
                {
                    changeOrder.ChangeOrderNumber = _coRepo.GetNextCONumber(changeOrder.ContractCode);
                }

                // Calculate total from lines
                changeOrder.CalculateTotal();

                // Validate
                var errors = changeOrder.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Set audit fields
                changeOrder.CreatedDate = DateTime.Now;
                changeOrder.CreatedBy = _company.UserName;
                changeOrder.RequestedBy = changeOrder.RequestedBy ?? _company.UserName;

                // Create in database
                return _coRepo.Create(changeOrder);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CreateChangeOrder", ex);
                throw;
            }
        }

        /// <summary>
        /// Update existing change order
        /// </summary>
        public void UpdateChangeOrder(ChangeOrder changeOrder)
        {
            try
            {
                // Check if change order exists
                ChangeOrder existing = _coRepo.GetByCode(changeOrder.Code);
                if (existing == null)
                {
                    throw new Exception($"Change Order not found: {changeOrder.Code}");
                }

                // Business rule: Cannot update approved change orders
                if (existing.Status == "Approved")
                {
                    throw new Exception("Cannot update approved change order");
                }

                // Validate
                var errors = changeOrder.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Update in database
                _coRepo.Update(changeOrder);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdateChangeOrder: {changeOrder.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Submit change order for approval
        /// </summary>
        public void SubmitChangeOrder(string code)
        {
            try
            {
                ChangeOrder co = _coRepo.GetByCode(code);
                if (co == null)
                {
                    throw new Exception($"Change Order not found: {code}");
                }

                if (co.Status != "Draft")
                {
                    throw new Exception($"Can only submit draft change orders. Current status: {co.Status}");
                }

                co.Status = "Submitted";
                _coRepo.Update(co);

                Logger.Info($"Change Order {code} submitted for approval");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error submitting change order: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Approve change order
        /// </summary>
        public void ApproveChangeOrder(string code)
        {
            try
            {
                ChangeOrder co = _coRepo.GetByCode(code);
                if (co == null)
                {
                    throw new Exception($"Change Order not found: {code}");
                }

                if (co.Status != "Submitted")
                {
                    throw new Exception($"Can only approve submitted change orders. Current status: {co.Status}");
                }

                // Get contract and update total value
                Contract contract = _contractRepo.GetByCode(co.ContractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {co.ContractCode}");
                }

                co.Status = "Approved";
                co.ApprovedBy = _company.UserName;
                co.ApprovedDate = DateTime.Now;

                _coRepo.Update(co);

                // Update contract value if financial change order
                if (co.Type == "Addition" || co.Type == "Deduction")
                {
                    double signedAmount = co.GetSignedAmount();
                    contract.TotalValue += signedAmount;
                    contract.ModifiedDate = DateTime.Now;
                    contract.ModifiedBy = _company.UserName;

                    // Recalculate base currency value
                    contract.UpdateBaseCurrencyValue();

                    _contractRepo.Update(contract);
                    Logger.Info($"Contract {contract.Code} value updated by {signedAmount}. New value: {contract.TotalValue}");
                }

                // Update contract end date if time extension
                if (co.Type == "TimeExtension" && co.AdditionalDays > 0)
                {
                    contract.EndDate = contract.EndDate.AddDays(co.AdditionalDays);
                    _contractRepo.Update(contract);
                    Logger.Info($"Contract {contract.Code} end date extended by {co.AdditionalDays} days");
                }

                Logger.Info($"Change Order {code} approved by {_company.UserName}");

                // PHASE 1: Revenue Recognition Integration
                // Update revenue recognition when change order is approved
                if (co.Type == "Addition" || co.Type == "Deduction")
                {
                    try
                    {
                        Logger.Info($"Updating revenue recognition for contract {co.ContractCode} due to change order {code}");

                        // Re-determine transaction price (Step 3)
                        double newTransactionPrice = _revenueService.DetermineTransactionPrice(contract);
                        Logger.Info($"New transaction price for contract {co.ContractCode}: {newTransactionPrice}");

                        // Re-allocate transaction price to performance obligations (Step 4)
                        var obligations = _revenueService.GetPerformanceObligations(co.ContractCode);
                        if (obligations != null && obligations.Count > 0)
                        {
                            _revenueService.AllocateTransactionPrice(co.ContractCode, obligations);
                            Logger.Info($"Transaction price reallocated for contract {co.ContractCode}");
                        }

                        // Update contract assets/liabilities
                        _revenueService.UpdateContractAssetsLiabilities(co.ContractCode);

                        // Regenerate revenue schedule with new contract value
                        _revenueService.GenerateRevenueSchedule(
                            co.ContractCode,
                            contract.StartDate,
                            contract.EndDate,
                            intervalMonths: 1
                        );

                        Logger.Info($"Revenue recognition updated for contract {co.ContractCode}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error updating revenue recognition for change order {code}: {ex.Message}", ex);
                        // Don't throw - CO approval should succeed even if RR fails
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error approving change order: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Reject change order
        /// </summary>
        public void RejectChangeOrder(string code, string reason)
        {
            try
            {
                ChangeOrder co = _coRepo.GetByCode(code);
                if (co == null)
                {
                    throw new Exception($"Change Order not found: {code}");
                }

                if (co.Status != "Submitted")
                {
                    throw new Exception($"Can only reject submitted change orders. Current status: {co.Status}");
                }

                co.Status = "Rejected";
                co.Remarks = (co.Remarks ?? "") + "\n[REJECTED] " + reason;

                _coRepo.Update(co);

                Logger.Info($"Change Order {code} rejected: {reason}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error rejecting change order: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate change order summary for contract
        /// </summary>
        public Dictionary<string, object> GetChangeOrderSummary(string contractCode)
        {
            try
            {
                var changeOrders = _coRepo.GetByContract(contractCode);

                double totalAdditions = 0;
                double totalDeductions = 0;
                int totalDays = 0;
                int countDraft = 0;
                int countSubmitted = 0;
                int countApproved = 0;
                int countRejected = 0;

                foreach (var co in changeOrders)
                {
                    if (co.Status == "Approved")
                    {
                        if (co.Type == "Addition")
                            totalAdditions += co.Amount;
                        else if (co.Type == "Deduction")
                            totalDeductions += co.Amount;
                        else if (co.Type == "TimeExtension")
                            totalDays += co.AdditionalDays;
                    }

                    switch (co.Status)
                    {
                        case "Draft": countDraft++; break;
                        case "Submitted": countSubmitted++; break;
                        case "Approved": countApproved++; break;
                        case "Rejected": countRejected++; break;
                    }
                }

                var summary = new Dictionary<string, object>
                {
                    ["TotalChangeOrders"] = changeOrders.Count,
                    ["TotalAdditions"] = totalAdditions,
                    ["TotalDeductions"] = totalDeductions,
                    ["NetChange"] = totalAdditions - totalDeductions,
                    ["TotalDaysExtended"] = totalDays,
                    ["CountDraft"] = countDraft,
                    ["CountSubmitted"] = countSubmitted,
                    ["CountApproved"] = countApproved,
                    ["CountRejected"] = countRejected
                };

                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting change order summary: {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get impact analysis for a change order
        /// </summary>
        public Dictionary<string, object> GetImpactAnalysis(string contractCode, double changeAmount, int additionalDays)
        {
            try
            {
                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Get current change order totals
                double currentCOAmount = _coRepo.GetTotalApprovedCOAmount(contractCode);

                // Calculate new values
                double newContractValue = contract.TotalValue + currentCOAmount + changeAmount;
                DateTime newEndDate = contract.EndDate.AddDays(additionalDays);

                // Get IPC totals
                double totalIPC = new IPCRepository(_company).GetTotalIPCAmount(contractCode);

                var impact = new Dictionary<string, object>
                {
                    ["CurrentContractValue"] = contract.TotalValue,
                    ["CurrentCOValue"] = currentCOAmount,
                    ["NewCOValue"] = changeAmount,
                    ["NewContractValue"] = newContractValue,
                    ["CurrentEndDate"] = contract.EndDate,
                    ["AdditionalDays"] = additionalDays,
                    ["NewEndDate"] = newEndDate,
                    ["TotalIPCAmount"] = totalIPC,
                    ["RemainingBalance"] = newContractValue - totalIPC,
                    ["ChangePercentage"] = Math.Round((changeAmount / contract.TotalValue) * 100, 2)
                };

                return impact;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error calculating impact analysis: {contractCode}", ex);
                throw;
            }
        }
    }
}
