using System;
using System.Collections.Generic;
using SAPbobsCOM;
using ContractManagementAddon.Models;
using ContractManagementAddon.DataAccess;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Business logic service for IPCs
    /// </summary>
    public class IPCService
    {
        private Company _company;
        private IPCRepository _ipcRepo;
        private ContractRepository _contractRepo;
        private CalculationService _calcService;
        private CurrencyService _currencyService;
        private RevenueRecognitionService _revenueService;

        public IPCService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
            _ipcRepo = new IPCRepository(_company);
            _contractRepo = new ContractRepository(_company);
            _calcService = new CalculationService();
            _currencyService = new CurrencyService(_company);
            _revenueService = new RevenueRecognitionService(_company);
        }

        /// <summary>
        /// Get all IPCs for a contract
        /// </summary>
        public List<IPC> GetIPCsByContract(string contractCode)
        {
            try
            {
                return _ipcRepo.GetByContract(contractCode);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetIPCsByContract: {contractCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get IPC by code
        /// </summary>
        public IPC GetIPC(string code)
        {
            try
            {
                return _ipcRepo.GetByCode(code);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetIPC: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create new IPC
        /// </summary>
        public string CreateIPC(IPC ipc)
        {
            try
            {
                // Validate contract exists
                Contract contract = _contractRepo.GetByCode(ipc.ContractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {ipc.ContractCode}");
                }

                // Auto-assign IPC number
                if (ipc.IPCNumber == 0)
                {
                    ipc.IPCNumber = _ipcRepo.GetNextIPCNumber(ipc.ContractCode);
                }

                // Get previous IPC total
                ipc.PreviousIPCTotal = _ipcRepo.GetTotalIPCAmount(ipc.ContractCode);

                // Calculate amounts
                ipc.CalculateAmounts(contract.RetentionPercentage);

                // PHASE 1: Multi-Currency logic
                ApplyCurrencyLogic(ipc, contract);

                // Validate
                var errors = ipc.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Business rule: Check if IPC amount doesn't exceed contract balance
                double totalIPC = ipc.PreviousIPCTotal + ipc.GrossAmount;
                if (totalIPC > contract.TotalValue)
                {
                    throw new Exception($"IPC amount exceeds contract value. Contract: {contract.TotalValue}, Total IPC: {totalIPC}");
                }

                // Set audit fields
                ipc.CreatedDate = DateTime.Now;
                ipc.CreatedBy = _company.UserName;

                // Create in database
                return _ipcRepo.Create(ipc);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CreateIPC", ex);
                throw;
            }
        }

        /// <summary>
        /// Update existing IPC
        /// </summary>
        public void UpdateIPC(IPC ipc)
        {
            try
            {
                // Check if IPC exists
                IPC existing = _ipcRepo.GetByCode(ipc.Code);
                if (existing == null)
                {
                    throw new Exception($"IPC not found: {ipc.Code}");
                }

                // Business rule: Cannot update approved or paid IPCs
                if (existing.Status == "Approved" || existing.Status == "Paid")
                {
                    throw new Exception("Cannot update IPC that is already approved or paid");
                }

                // Validate
                var errors = ipc.Validate();
                if (errors.Count > 0)
                {
                    throw new Exception("Validation failed:\n" + string.Join("\n", errors));
                }

                // Update in database
                _ipcRepo.Update(ipc);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdateIPC: {ipc.Code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Submit IPC for approval
        /// </summary>
        public void SubmitIPC(string code)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(code);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {code}");
                }

                if (ipc.Status != "Draft")
                {
                    throw new Exception($"Can only submit draft IPCs. Current status: {ipc.Status}");
                }

                ipc.Status = "Submitted";
                _ipcRepo.Update(ipc);

                Logger.Info($"IPC {code} submitted for approval");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error submitting IPC: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Approve IPC
        /// </summary>
        public void ApproveIPC(string code)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(code);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {code}");
                }

                if (ipc.Status != "Submitted")
                {
                    throw new Exception($"Can only approve submitted IPCs. Current status: {ipc.Status}");
                }

                ipc.Status = "Approved";
                ipc.ApprovedBy = _company.UserName;
                ipc.ApprovedDate = DateTime.Now;

                _ipcRepo.Update(ipc);

                Logger.Info($"IPC {code} approved by {_company.UserName}");

                // PHASE 1: Revenue Recognition Integration
                // Trigger revenue recognition when IPC is approved
                try
                {
                    Logger.Info($"Triggering revenue recognition for IPC {code}");

                    // Recognize revenue from this IPC
                    // This will:
                    // 1. Update performance obligation completion percentages
                    // 2. Recognize revenue based on progress
                    // 3. Handle deferred revenue if billed ahead of performance
                    // 4. Log all revenue recognition transactions
                    _revenueService.RecognizeRevenueFromIPC(code);

                    // Update contract assets and liabilities on balance sheet
                    _revenueService.UpdateContractAssetsLiabilities(ipc.ContractCode);

                    Logger.Info($"Revenue recognition completed for IPC {code}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error during revenue recognition for IPC {code}: {ex.Message}", ex);
                    // Don't throw - IPC approval should succeed even if RR fails
                    // Finance team can manually adjust later
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error approving IPC: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Reject IPC
        /// </summary>
        public void RejectIPC(string code, string reason)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(code);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {code}");
                }

                if (ipc.Status != "Submitted")
                {
                    throw new Exception($"Can only reject submitted IPCs. Current status: {ipc.Status}");
                }

                ipc.Status = "Rejected";
                ipc.Remarks = (ipc.Remarks ?? "") + "\n[REJECTED] " + reason;

                _ipcRepo.Update(ipc);

                Logger.Info($"IPC {code} rejected: {reason}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error rejecting IPC: {code}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create AR Invoice from IPC
        /// </summary>
        public int CreateARInvoice(string ipcCode)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(ipcCode);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {ipcCode}");
                }

                if (ipc.Status != "Approved")
                {
                    throw new Exception("Can only create invoice from approved IPC");
                }

                Contract contract = _contractRepo.GetByCode(ipc.ContractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {ipc.ContractCode}");
                }

                // Create AR Invoice
                Documents arInvoice = (Documents)_company.GetBusinessObject(BoObjectTypes.oInvoices);

                // Header
                arInvoice.CardCode = contract.CustomerCode;
                arInvoice.DocDate = DateTime.Today;
                arInvoice.DocDueDate = DateTime.Today.AddDays(30);
                arInvoice.Comments = $"IPC #{ipc.IPCNumber} for Contract {contract.Code}";

                // Lines
                foreach (var line in ipc.Lines)
                {
                    arInvoice.Lines.ItemDescription = line.Description;
                    arInvoice.Lines.Quantity = line.Quantity;
                    arInvoice.Lines.UnitPrice = line.UnitPrice;
                    arInvoice.Lines.Add();
                }

                // Add retention line if applicable
                if (ipc.RetentionAmount > 0)
                {
                    arInvoice.Lines.ItemDescription = "Retention";
                    arInvoice.Lines.Quantity = 1;
                    arInvoice.Lines.UnitPrice = -ipc.RetentionAmount;
                    arInvoice.Lines.Add();
                }

                // Create invoice
                if (arInvoice.Add() != 0)
                {
                    string error = _company.GetLastErrorDescription();
                    throw new Exception($"Failed to create AR Invoice: {error}");
                }

                // Get created invoice DocEntry
                string docEntry = _company.GetNewObjectKey();

                // Update IPC with invoice reference
                ipc.ARInvoiceDocEntry = docEntry;
                ipc.Status = "Paid";
                _ipcRepo.Update(ipc);

                Logger.Info($"AR Invoice {docEntry} created from IPC {ipcCode}");

                return Convert.ToInt32(docEntry);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating AR Invoice from IPC: {ipcCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate IPC summary for contract
        /// </summary>
        public Dictionary<string, object> GetIPCSummary(string contractCode)
        {
            try
            {
                var ipcs = _ipcRepo.GetByContract(contractCode);

                double totalGross = 0;
                double totalNet = 0;
                double totalRetention = 0;
                int countDraft = 0;
                int countSubmitted = 0;
                int countApproved = 0;
                int countPaid = 0;

                foreach (var ipc in ipcs)
                {
                    totalGross += ipc.GrossAmount;
                    totalNet += ipc.NetAmount;
                    totalRetention += ipc.RetentionAmount;

                    switch (ipc.Status)
                    {
                        case "Draft": countDraft++; break;
                        case "Submitted": countSubmitted++; break;
                        case "Approved": countApproved++; break;
                        case "Paid": countPaid++; break;
                    }
                }

                var summary = new Dictionary<string, object>
                {
                    ["TotalIPCs"] = ipcs.Count,
                    ["TotalGross"] = totalGross,
                    ["TotalNet"] = totalNet,
                    ["TotalRetention"] = totalRetention,
                    ["CountDraft"] = countDraft,
                    ["CountSubmitted"] = countSubmitted,
                    ["CountApproved"] = countApproved,
                    ["CountPaid"] = countPaid
                };

                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPC summary: {contractCode}", ex);
                throw;
            }
        }

        #region PHASE 1: Multi-Currency Methods

        /// <summary>
        /// Apply multi-currency logic to IPC
        /// Inherits currency from contract and calculates base currency amounts
        /// </summary>
        private void ApplyCurrencyLogic(IPC ipc, Contract contract)
        {
            try
            {
                // Inherit currency from contract
                ipc.Currency = contract.Currency;
                ipc.BaseCurrency = contract.BaseCurrency;

                // Get exchange rate for IPC date
                if (ipc.Currency == ipc.BaseCurrency)
                {
                    // Same currency, rate = 1.0
                    ipc.ExchangeRate = 1.0;
                    ipc.BaseCurrencyGrossAmount = ipc.GrossAmount;
                    ipc.BaseCurrencyNetAmount = ipc.NetAmount;
                    ipc.FXGainLoss = 0.0;
                }
                else
                {
                    // Get exchange rate for IPC date
                    ExchangeRate rate = _currencyService.GetExchangeRate(ipc.Currency, ipc.BaseCurrency, ipc.IPCDate);
                    if (rate == null)
                    {
                        // Try to get the most recent rate before IPC date
                        Logger.Warning($"No exchange rate found for {ipc.Currency}/{ipc.BaseCurrency} on {ipc.IPCDate:yyyy-MM-dd}");

                        // Try to get latest available rate
                        ExchangeRate latestRate = _currencyService.GetExchangeRate(ipc.Currency, ipc.BaseCurrency, DateTime.Today);

                        if (latestRate != null)
                        {
                            Logger.Info($"Using latest exchange rate: {latestRate.Rate} from {latestRate.RateDate:yyyy-MM-dd}");
                            ipc.ExchangeRate = latestRate.Rate;
                        }
                        else if (contract.ExchangeRate > 0)
                        {
                            Logger.Warning($"Using contract exchange rate as fallback: {contract.ExchangeRate}");
                            ipc.ExchangeRate = contract.ExchangeRate;
                        }
                        else
                        {
                            // Final fallback - cannot proceed
                            throw new Exception($"No exchange rate available for {ipc.Currency}/{ipc.BaseCurrency}. " +
                                $"Please add exchange rate in currency master before creating IPC.");
                        }
                    }
                    else
                    {
                        ipc.ExchangeRate = rate.Rate;
                    }

                    // Calculate base currency amounts
                    ipc.BaseCurrencyGrossAmount = ipc.GrossAmount * ipc.ExchangeRate;
                    ipc.BaseCurrencyNetAmount = ipc.NetAmount * ipc.ExchangeRate;

                    // Calculate FX gain/loss compared to contract rate
                    ipc.CalculateFXGainLoss(contract.ExchangeRate);
                }

                ipc.LastFXUpdateDate = DateTime.Now;

                Logger.Info($"Applied currency logic to IPC: Currency={ipc.Currency}, Rate={ipc.ExchangeRate}, BaseCurrencyGross={ipc.BaseCurrencyGrossAmount}, FX Gain/Loss={ipc.FXGainLoss}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error applying currency logic to IPC", ex);
                throw;
            }
        }

        /// <summary>
        /// Update exchange rate for an IPC and recalculate amounts
        /// </summary>
        public void UpdateIPCExchangeRate(string ipcCode)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(ipcCode);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {ipcCode}");
                }

                Contract contract = _contractRepo.GetByCode(ipc.ContractCode);
                if (contract == null)
                {
                    throw new Exception($"Contract not found: {ipc.ContractCode}");
                }

                if (ipc.Currency == ipc.BaseCurrency)
                {
                    Logger.Info($"IPC {ipcCode} is in base currency, no FX update needed");
                    return;
                }

                // Get current exchange rate
                ExchangeRate currentRate = _currencyService.GetExchangeRate(ipc.Currency, ipc.BaseCurrency, ipc.IPCDate);
                if (currentRate == null)
                {
                    throw new Exception($"Exchange rate not found for {ipc.Currency}/{ipc.BaseCurrency} on {ipc.IPCDate:yyyy-MM-dd}");
                }

                // Update exchange rate and recalculate
                ipc.ExchangeRate = currentRate.Rate;
                ipc.BaseCurrencyGrossAmount = ipc.GrossAmount * ipc.ExchangeRate;
                ipc.BaseCurrencyNetAmount = ipc.NetAmount * ipc.ExchangeRate;
                ipc.CalculateFXGainLoss(contract.ExchangeRate);
                ipc.LastFXUpdateDate = DateTime.Now;

                // Update in database
                _ipcRepo.Update(ipc);

                Logger.Info($"Updated exchange rate for IPC {ipcCode}: Rate={ipc.ExchangeRate}, FX Gain/Loss={ipc.FXGainLoss}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating exchange rate for IPC {ipcCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get IPC amounts in base currency for reporting
        /// </summary>
        public IPCAmountsInBaseCurrency GetIPCAmountsInBaseCurrency(string ipcCode)
        {
            try
            {
                IPC ipc = _ipcRepo.GetByCode(ipcCode);
                if (ipc == null)
                {
                    throw new Exception($"IPC not found: {ipcCode}");
                }

                return ipc.GetBaseCurrencyAmounts();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting IPC amounts in base currency: {ipcCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get total IPC amounts in base currency for a contract
        /// Used for consolidated reporting
        /// </summary>
        public Dictionary<string, double> GetContractIPCTotalsInBaseCurrency(string contractCode)
        {
            try
            {
                List<IPC> ipcs = _ipcRepo.GetByContract(contractCode);

                double totalGrossBase = 0;
                double totalNetBase = 0;
                double totalRetentionBase = 0;
                double totalFXGainLoss = 0;

                foreach (IPC ipc in ipcs)
                {
                    totalGrossBase += ipc.BaseCurrencyGrossAmount;
                    totalNetBase += ipc.BaseCurrencyNetAmount;
                    totalRetentionBase += ipc.RetentionAmount * ipc.ExchangeRate;
                    totalFXGainLoss += ipc.FXGainLoss;
                }

                return new Dictionary<string, double>
                {
                    ["TotalGrossBase"] = totalGrossBase,
                    ["TotalNetBase"] = totalNetBase,
                    ["TotalRetentionBase"] = totalRetentionBase,
                    ["TotalFXGainLoss"] = totalFXGainLoss,
                    ["IPCCount"] = ipcs.Count
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting contract IPC totals in base currency: {contractCode}", ex);
                throw;
            }
        }

        #endregion
    }
}
