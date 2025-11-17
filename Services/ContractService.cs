using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.DataAccess;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Business logic service for Contracts
    /// </summary>
    public class ContractService
    {
        private Company _company;
        private ContractRepository _contractRepo;
        private IPCRepository _ipcRepo;
        //private ChangeOrderRepository _coRepo;
        private CurrencyService _currencyService;
        private RevenueRecognitionService _revenueService;

        public ContractService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _contractRepo = new ContractRepository(_company);
            _ipcRepo = new IPCRepository(_company);
            //_coRepo = new ChangeOrderRepository(_company);
            _currencyService = new CurrencyService(_company);
            _revenueService = new RevenueRecognitionService(_company);
        }

        /// <summary>
        /// Get all contracts
        /// </summary>
        public List<Contract> GetAllContracts()
        {
            try
            {
                return _contractRepo.GetAll();
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GetAllContracts", ex);
                throw;
            }
        }

        /// <summary>
        /// Get contract by code with calculated values
        /// </summary>
        public Contract GetContract(string code)
        {
            try
            {
                Contract contract = _contractRepo.GetByCode(code);

                if (contract != null)
                {
                    // Calculate summary values
                    CalculateContractSummary(contract);
                }

                return contract;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetContract: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create new contract
        /// </summary>
        public string CreateContract(Contract contract)
        {
            try
            {
                // Validate
                var errors = contract.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Calculate totals
                contract.CalculateTotal();

                // PHASE 1: Multi-Currency logic
                ApplyCurrencyLogic(contract);

                // Set audit fields
                contract.CreatedDate = DateTime.Now;
                contract.ModifiedDate = DateTime.Now;
                contract.CreatedBy = _company.UserName;
                contract.ModifiedBy = _company.UserName;

                // Create in database
                string contractCode = _contractRepo.Create(contract);

                // PHASE 1: Revenue Recognition Integration
                // Setup revenue recognition if contract is created in Active status
                if (contract.Status == "Active")
                {
                    SetupRevenueRecognition(contractCode, contract);
                }

                Logger.Info($"Contract {contractCode} created successfully");
                return contractCode;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CreateContract", ex);
                throw;
            }
        }

        /// <summary>
        /// Update existing contract
        /// </summary>
        public void UpdateContract(Contract contract)
        {
            try
            {
                // Validate
                var errors = contract.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Check if contract exists
                Contract existing = _contractRepo.GetByCode(contract.Code);
                if (existing == null)
                {
                    throw new Exception($"Contract not found: {contract.Code}");
                }

                // PHASE 1: Multi-Currency logic
                // Check if currency or amount changed, update FX calculations
                bool currencyChanged = contract.Currency != existing.Currency;
                bool amountChanged = Math.Abs(contract.TotalValue - existing.TotalValue) > 0.01;

                if (currencyChanged || amountChanged)
                {
                    ApplyCurrencyLogic(contract);

                    // Calculate FX gain/loss if currency changed
                    if (currencyChanged)
                    {
                        Logger.Info($"Contract {contract.Code} currency changed from {existing.Currency} to {contract.Currency}");
                    }
                }

                // Update modification fields
                contract.ModifiedDate = DateTime.Now;
                contract.ModifiedBy = _company.UserName;

                // Update in database
                _contractRepo.Update(contract);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdateContract: {contract.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete contract
        /// </summary>
        public void DeleteContract(string code)
        {
            try
            {
                // Check if contract has IPCs or COs
                var ipcs = _ipcRepo.GetByContract(code);
                //var cos = _coRepo.GetByContract(code);

                if (ipcs.Count > 0)
                {
                    throw new Exception("Cannot delete contract with existing IPCs. Delete IPCs first.");
                }

            /*    if (cos.Count > 0)
                {
                    throw new Exception("Cannot delete contract with existing Change Orders. Delete COs first.");
                }
*/
                _contractRepo.Delete(code);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in DeleteContract: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Change contract status
        /// </summary>
        public void ChangeStatus(string code, string newStatus)
        {
            try
            {
                // Validate status
                if (!ValidationHelper.IsValidStatus(newStatus))
                {
                    throw new Exception($"Invalid status: {newStatus}");
                }

                Contract contract = _contractRepo.GetByCode(code);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {code}");
                }

                // Business rules for status changes
                ValidateStatusChange(contract, newStatus);

                string oldStatus = contract.Status;
                contract.Status = newStatus;
                _contractRepo.Update(contract);

                // PHASE 1: Setup revenue recognition when status changes to Active
                if (oldStatus != "Active" && newStatus == "Active")
                {
                    SetupRevenueRecognition(code, contract);
                }

                Logger.Info($"Contract {code} status changed from {oldStatus} to {newStatus}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error changing contract status: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate contract summary with IPC and CO totals
        /// </summary>
        private void CalculateContractSummary(Contract contract)
        {
            try
            {
                // Get total IPC amount
                contract.TotalIPCAmount = _ipcRepo.GetTotalIPCAmount(contract.Code);

                // Calculate retention
                contract.TotalRetention = contract.TotalIPCAmount * (contract.RetentionPercentage / 100);

                // Net paid (IPC amount minus retention)
                contract.TotalPaid = contract.TotalIPCAmount - contract.TotalRetention;

                // Get change orders impact
                //double coImpact = _coRepo.GetTotalApprovedCOAmount(contract.Code);

                // Adjusted contract value
                //double adjustedValue = contract.TotalValue + coImpact;

                // Balance remaining
                contract.Balance = contract.TotalIPCAmount;

                // Completion percentage
                contract.CalculateCompletion();

                Logger.Debug($"Contract summary calculated for {contract.Code}: IPC={contract.TotalIPCAmount}, Balance={contract.Balance}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error calculating contract summary: {contract.Code}", ex);
                // Don't throw, just log - we want to return the contract even if summary calculation fails
            }
        }

        /// <summary>
        /// Validate status change business rules
        /// </summary>
        private void ValidateStatusChange(Contract contract, string newStatus)
        {
            // Draft -> Active: OK
            // Active -> OnHold: OK
            // OnHold -> Active: OK
            // Active -> Completed: Check if all IPCs are paid
            // Any -> Cancelled: OK (with warning)
            // Completed -> Active: Not allowed

            if (contract.Status == "Completed" && newStatus != "Completed")
            {
                throw new Exception("Cannot change status of completed contract");
            }

            if (newStatus == "Completed")
            {
                // Check if there are unpaid IPCs
                var ipcs = _ipcRepo.GetByContract(contract.Code);
                foreach (var ipc in ipcs)
                {
                    if (ipc.Status != "Paid")
                    {
                        throw new Exception($"Cannot complete contract with unpaid IPC: {ipc.DocNum}");
                    }
                }
            }
        }

        /// <summary>
        /// Get contract performance summary
        /// </summary>
        public Dictionary<string, object> GetPerformanceSummary(string code)
        {
            try
            {
                Contract contract = GetContract(code);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {code}");
                }

                var summary = new Dictionary<string, object>
                {
                    ["ContractCode"] = contract.Code,
                    ["ContractValue"] = contract.TotalValue,
                    ["TotalIPCAmount"] = contract.TotalIPCAmount,
                    ["TotalPaid"] = contract.TotalPaid,
                    ["TotalRetention"] = contract.TotalRetention,
                    ["Balance"] = contract.Balance,
                    ["CompletionPercentage"] = contract.CompletionPercentage,
                    ["DaysElapsed"] = (DateTime.Today - contract.StartDate).Days,
                    ["DaysRemaining"] = (contract.EndDate - DateTime.Today).Days,
                    ["TotalDays"] = (contract.EndDate - contract.StartDate).Days,
                    ["TimePercentage"] = Math.Round(((DateTime.Today - contract.StartDate).TotalDays / (contract.EndDate - contract.StartDate).TotalDays) * 100, 2)
                };

                // Add change order totals
                //var changeOrders = _coRepo.GetByContract(code);
                //summary["TotalChangeOrders"] = changeOrders.Count;
                //summary["ChangeOrderValue"] = _coRepo.GetTotalApprovedCOAmount(code);

                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting performance summary: {code}", ex);
                throw;
            }
        }

        #region PHASE 1: Multi-Currency Methods

        /// <summary>
        /// Apply multi-currency logic to contract
        /// Sets base currency, exchange rate, and base currency value
        /// </summary>
        private void ApplyCurrencyLogic(Contract contract)
        {
            try
            {
                // Get base currency from system
                Currency baseCurrency = _currencyService.GetBaseCurrency();
                if (baseCurrency == null)
                {
                    Logger.Warning("No base currency defined, defaulting to USD");
                    contract.BaseCurrency = "USD";
                }
                else
                {
                    contract.BaseCurrency = baseCurrency.Code;
                }

                // Validate contract currency exists
                Currency contractCurrency = _currencyService.GetCurrency(contract.Currency);
                if (contractCurrency == null)
                {
                    throw new Exception($"Invalid currency: {contract.Currency}. Please add it to the currency master.");
                }

                // Get exchange rate
                if (contract.Currency == contract.BaseCurrency)
                {
                    // Same currency, rate = 1.0
                    contract.ExchangeRate = 1.0;
                    contract.BaseCurrencyValue = contract.TotalValue;
                    contract.FXGainLoss = 0.0;
                }
                else
                {
                    // Get current exchange rate
                    ExchangeRate rate = _currencyService.GetExchangeRate(contract.Currency, contract.BaseCurrency, contract.StartDate);
                    if (rate == null)
                    {
                        throw new Exception($"Exchange rate not found for {contract.Currency}/{contract.BaseCurrency} on {contract.StartDate:yyyy-MM-dd}. Please add exchange rates.");
                    }

                    contract.ExchangeRate = rate.Rate;
                    contract.BaseCurrencyValue = contract.TotalValue * contract.ExchangeRate;
                    contract.FXGainLoss = 0.0; // Initialize FX gain/loss to zero for new contracts
                }

                contract.LastFXUpdateDate = DateTime.Now;

                Logger.Info($"Applied currency logic to contract: Currency={contract.Currency}, BaseCurrency={contract.BaseCurrency}, Rate={contract.ExchangeRate}, BaseCurrencyValue={contract.BaseCurrencyValue}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error applying currency logic to contract", ex);
                throw;
            }
        }

        /// <summary>
        /// Update exchange rate and recalculate base currency values for a contract
        /// Used to refresh FX calculations with current rates
        /// </summary>
        public void UpdateContractExchangeRate(string contractCode)
        {
            try
            {
                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                if (contract.Currency == contract.BaseCurrency)
                {
                    Logger.Info($"Contract {contractCode} is in base currency, no FX update needed");
                    return;
                }

                // Get current exchange rate
                ExchangeRate currentRate = _currencyService.GetExchangeRate(contract.Currency, contract.BaseCurrency);
                if (currentRate == null)
                {
                    throw new Exception($"Exchange rate not found for {contract.Currency}/{contract.BaseCurrency}");
                }

                // Calculate FX gain/loss
                double oldBaseCurrencyValue = contract.BaseCurrencyValue;
                contract.ExchangeRate = currentRate.Rate;
                contract.BaseCurrencyValue = contract.TotalValue * contract.ExchangeRate;
                contract.FXGainLoss = contract.BaseCurrencyValue - oldBaseCurrencyValue;
                contract.LastFXUpdateDate = DateTime.Now;

                // Update in database
                _contractRepo.Update(contract);

                Logger.Info($"Updated exchange rate for contract {contractCode}: Old Rate={oldBaseCurrencyValue/contract.TotalValue}, New Rate={contract.ExchangeRate}, FX Gain/Loss={contract.FXGainLoss}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating exchange rate for contract {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update exchange rates for all contracts in foreign currencies
        /// Used for periodic FX revaluation
        /// </summary>
        public Dictionary<string, double> UpdateAllContractExchangeRates()
        {
            Dictionary<string, double> fxImpact = new Dictionary<string, double>();

            try
            {
                List<Contract> contracts = _contractRepo.GetAll();
                int updatedCount = 0;
                double totalFXGainLoss = 0;

                foreach (Contract contract in contracts)
                {
                    // Skip completed or cancelled contracts
                    if (contract.Status == "Completed" || contract.Status == "Cancelled")
                        continue;

                    // Skip contracts in base currency
                    if (contract.Currency == contract.BaseCurrency)
                        continue;

                    try
                    {
                        UpdateContractExchangeRate(contract.Code);
                        updatedCount++;

                        // Reload to get updated FX gain/loss
                        Contract updated = _contractRepo.GetByCode(contract.Code);
                        totalFXGainLoss += updated.FXGainLoss;
                        fxImpact[contract.Code] = updated.FXGainLoss;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Failed to update exchange rate for contract {contract.Code}: {ex.Message}");
                        fxImpact[contract.Code] = 0;
                    }
                }

                Logger.Info($"Updated exchange rates for {updatedCount} contracts. Total FX Gain/Loss: {totalFXGainLoss}");
                fxImpact["_TOTAL_FX_IMPACT"] = totalFXGainLoss;
                fxImpact["_UPDATED_COUNT"] = updatedCount;

                return fxImpact;
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating all contract exchange rates", ex);
                throw;
            }
        }

        /// <summary>
        /// Get contract amounts in base currency for consolidated reporting
        /// </summary>
        public ContractAmountsInBaseCurrency GetContractAmountsInBaseCurrency(string contractCode)
        {
            try
            {
                Contract contract = _contractRepo.GetByCode(contractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {contractCode}");
                }

                // Calculate summary first to get latest IPC totals
                CalculateContractSummary(contract);

                // Return amounts in base currency
                return contract.GetBaseCurrencyAmounts();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting contract amounts in base currency: {contractCode}", ex);
                throw;
            }
        }

        #endregion

        #region PHASE 1: Revenue Recognition Integration

        /// <summary>
        /// Setup revenue recognition for a contract (ASC 606 / IFRS 15)
        /// Called when contract is created in Active status or when status changes to Active
        /// </summary>
        private void SetupRevenueRecognition(string contractCode, Contract contract)
        {
            try
            {
                Logger.Info($"Setting up revenue recognition for contract {contractCode}");

                // Step 1: Validate contract meets ASC 606 criteria
                if (!_revenueService.ValidateContractCriteria(contract))
                {
                    Logger.Warning($"Contract {contractCode} does not meet ASC 606 criteria. Revenue recognition not initialized.");
                    return;
                }

                // Step 2 & 4: Create and allocate performance obligations
                _revenueService.CreatePerformanceObligations(contractCode);
                Logger.Info($"Performance obligations created for contract {contractCode}");

                // Generate initial revenue schedule
                _revenueService.GenerateRevenueSchedule(
                    contractCode,
                    contract.StartDate,
                    contract.EndDate,
                    intervalMonths: 1  // Monthly schedule
                );
                Logger.Info($"Revenue schedule generated for contract {contractCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error setting up revenue recognition for contract {contractCode}: {ex.Message}", ex);
                // Don't throw - contract creation/status change should succeed even if RR setup fails
                // User can manually setup later
            }
        }

        #endregion
    }
}
