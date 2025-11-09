using System;
using System.Collections.Generic;

namespace ContractManagementAddon.Models
{
    /// <summary>
    /// Contract header model
    /// </summary>
    public class Contract
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } // Draft, Active, OnHold, Completed, Cancelled
        public double TotalValue { get; set; }
        public string Currency { get; set; }
        public double RetentionPercentage { get; set; }
        public string PaymentTerms { get; set; }
        public string ContractManager { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

        // Multi-Currency fields
        public string BaseCurrency { get; set; }           // System base currency
        public double ExchangeRate { get; set; }           // Rate from Currency to BaseCurrency
        public double BaseCurrencyValue { get; set; }      // TotalValue in base currency
        public double FXGainLoss { get; set; }             // Foreign exchange gain/loss
        public DateTime? LastFXUpdateDate { get; set; }    // When FX was last recalculated

        // Calculated fields
        public double TotalIPCAmount { get; set; }
        public double TotalRetention { get; set; }
        public double TotalPaid { get; set; }
        public double Balance { get; set; }
        public double CompletionPercentage { get; set; }

        // Collections
        public List<ContractLine> Lines { get; set; }

        public Contract()
        {
            Lines = new List<ContractLine>();
            Status = "Draft";
            Currency = "USD";
            BaseCurrency = "USD";
            ExchangeRate = 1.0;
            BaseCurrencyValue = 0.0;
            FXGainLoss = 0.0;
            RetentionPercentage = 10.0;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(6);
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Calculate total from lines
        /// </summary>
        public void CalculateTotal()
        {
            TotalValue = 0;
            if (Lines != null)
            {
                foreach (var line in Lines)
                {
                    TotalValue += line.LineTotal;
                }
            }
        }

        /// <summary>
        /// Calculate completion percentage
        /// </summary>
        public void CalculateCompletion()
        {
            if (TotalValue > 0)
            {
                CompletionPercentage = (TotalIPCAmount / TotalValue) * 100;
            }
            else
            {
                CompletionPercentage = 0;
            }
        }

        /// <summary>
        /// Validate contract data
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Code))
                errors.Add("Contract code is required");

            if (string.IsNullOrWhiteSpace(CustomerCode))
                errors.Add("Customer is required");

            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required");

            if (EndDate < StartDate)
                errors.Add("End date must be after start date");

            if (TotalValue <= 0)
                errors.Add("Total value must be greater than zero");

            if (RetentionPercentage < 0 || RetentionPercentage > 100)
                errors.Add("Retention percentage must be between 0 and 100");

            if (Lines == null || Lines.Count == 0)
                errors.Add("At least one line item is required");

            // Multi-currency validation
            if (string.IsNullOrWhiteSpace(Currency))
                errors.Add("Currency is required");

            if (string.IsNullOrWhiteSpace(BaseCurrency))
                errors.Add("Base currency is required");

            if (ExchangeRate <= 0)
                errors.Add("Exchange rate must be greater than zero");

            return errors;
        }

        /// <summary>
        /// Update base currency value using current exchange rate
        /// </summary>
        public void UpdateBaseCurrencyValue()
        {
            if (Currency == BaseCurrency)
            {
                ExchangeRate = 1.0;
                BaseCurrencyValue = TotalValue;
            }
            else
            {
                BaseCurrencyValue = TotalValue * ExchangeRate;
            }
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Calculate FX gain/loss by comparing current rate to original rate
        /// </summary>
        public void CalculateFXGainLoss(double currentExchangeRate)
        {
            if (Currency == BaseCurrency)
            {
                FXGainLoss = 0.0;
                return;
            }

            double currentBaseCurrencyValue = TotalValue * currentExchangeRate;
            FXGainLoss = currentBaseCurrencyValue - BaseCurrencyValue;
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Format total value with currency symbol
        /// </summary>
        public string FormatTotalValue(Currency currencyInfo)
        {
            if (currencyInfo == null)
                return TotalValue.ToString("N2");

            return currencyInfo.FormatAmount(TotalValue);
        }

        /// <summary>
        /// Get all amounts in base currency for reporting
        /// </summary>
        public ContractAmountsInBaseCurrency GetBaseCurrencyAmounts()
        {
            return new ContractAmountsInBaseCurrency
            {
                TotalValue = BaseCurrencyValue,
                TotalIPCAmount = TotalIPCAmount * ExchangeRate,
                TotalRetention = TotalRetention * ExchangeRate,
                TotalPaid = TotalPaid * ExchangeRate,
                Balance = Balance * ExchangeRate,
                FXGainLoss = FXGainLoss
            };
        }
    }

    /// <summary>
    /// Helper class for contract amounts in base currency
    /// </summary>
    public class ContractAmountsInBaseCurrency
    {
        public double TotalValue { get; set; }
        public double TotalIPCAmount { get; set; }
        public double TotalRetention { get; set; }
        public double TotalPaid { get; set; }
        public double Balance { get; set; }
        public double FXGainLoss { get; set; }
    }

    /// <summary>
    /// Contract line model
    /// </summary>
    public class ContractLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public double Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
        public string AccountCode { get; set; }
        public string CostCenter { get; set; }
        public string Remarks { get; set; }

        public ContractLine()
        {
            Quantity = 1;
            UnitOfMeasure = "EA";
        }

        /// <summary>
        /// Calculate line total
        /// </summary>
        public void CalculateLineTotal()
        {
            LineTotal = Quantity * UnitPrice;
        }
    }

    /// <summary>
    /// Interim Payment Certificate (IPC) model
    /// </summary>
    public class IPC
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public int IPCNumber { get; set; }
        public DateTime IPCDate { get; set; }
        public string Period { get; set; }
        public double GrossAmount { get; set; }
        public double RetentionAmount { get; set; }
        public double NetAmount { get; set; }
        public double PreviousIPCTotal { get; set; }
        public double CurrentAmount { get; set; }
        public string Status { get; set; } // Draft, Submitted, Approved, Rejected, Paid
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Remarks { get; set; }
        public string ARInvoiceDocEntry { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        // Multi-Currency fields
        public string Currency { get; set; }                    // IPC currency (usually same as contract)
        public string BaseCurrency { get; set; }                // System base currency
        public double ExchangeRate { get; set; }                // Rate at IPC date
        public double BaseCurrencyGrossAmount { get; set; }     // Gross amount in base currency
        public double BaseCurrencyNetAmount { get; set; }       // Net amount in base currency
        public double FXGainLoss { get; set; }                  // FX gain/loss vs contract rate
        public DateTime? LastFXUpdateDate { get; set; }         // When FX was last calculated

        // Collections
        public List<IPCLine> Lines { get; set; }

        public IPC()
        {
            Lines = new List<IPCLine>();
            Status = "Draft";
            Currency = "USD";
            BaseCurrency = "USD";
            ExchangeRate = 1.0;
            BaseCurrencyGrossAmount = 0.0;
            BaseCurrencyNetAmount = 0.0;
            FXGainLoss = 0.0;
            IPCDate = DateTime.Today;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Calculate IPC amounts
        /// </summary>
        public void CalculateAmounts(double retentionPercentage)
        {
            // Validate retention percentage
            if (retentionPercentage < 0 || retentionPercentage > 100)
            {
                throw new ArgumentException($"Retention percentage must be between 0 and 100. Got: {retentionPercentage}");
            }

            // Validate Lines collection
            if (Lines == null)
            {
                throw new InvalidOperationException("IPC Lines cannot be null");
            }

            GrossAmount = 0;
            foreach (var line in Lines)
            {
                // Validate line amount is non-negative
                if (line.Amount < 0)
                {
                    throw new InvalidOperationException($"IPC line amount cannot be negative: {line.Amount}");
                }
                GrossAmount += line.Amount;
            }

            RetentionAmount = GrossAmount * (retentionPercentage / 100);
            NetAmount = GrossAmount - RetentionAmount;
            CurrentAmount = GrossAmount - PreviousIPCTotal;

            // Validate results
            if (NetAmount < 0)
            {
                throw new InvalidOperationException($"Net amount cannot be negative. Check retention percentage and gross amount.");
            }
        }

        /// <summary>
        /// Validate IPC data
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
                errors.Add("Contract is required");

            if (IPCNumber <= 0)
                errors.Add("IPC number must be greater than zero");

            if (Lines == null || Lines.Count == 0)
                errors.Add("At least one line item is required");

            if (GrossAmount <= 0)
                errors.Add("Gross amount must be greater than zero");

            // Multi-currency validation
            if (string.IsNullOrWhiteSpace(Currency))
                errors.Add("Currency is required");

            if (string.IsNullOrWhiteSpace(BaseCurrency))
                errors.Add("Base currency is required");

            if (ExchangeRate <= 0)
                errors.Add("Exchange rate must be greater than zero");

            return errors;
        }

        /// <summary>
        /// Update base currency amounts using current exchange rate
        /// </summary>
        public void UpdateBaseCurrencyAmounts()
        {
            if (Currency == BaseCurrency)
            {
                ExchangeRate = 1.0;
                BaseCurrencyGrossAmount = GrossAmount;
                BaseCurrencyNetAmount = NetAmount;
            }
            else
            {
                BaseCurrencyGrossAmount = GrossAmount * ExchangeRate;
                BaseCurrencyNetAmount = NetAmount * ExchangeRate;
            }
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Calculate FX gain/loss compared to contract exchange rate
        /// </summary>
        public void CalculateFXGainLoss(double contractExchangeRate)
        {
            if (Currency == BaseCurrency)
            {
                FXGainLoss = 0.0;
                return;
            }

            double amountAtContractRate = NetAmount * contractExchangeRate;
            double amountAtIPCRate = NetAmount * ExchangeRate;
            FXGainLoss = amountAtIPCRate - amountAtContractRate;
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Format gross amount with currency symbol
        /// </summary>
        public string FormatGrossAmount(Currency currencyInfo)
        {
            if (currencyInfo == null)
                return GrossAmount.ToString("N2");

            return currencyInfo.FormatAmount(GrossAmount);
        }

        /// <summary>
        /// Get all amounts in base currency for reporting
        /// </summary>
        public IPCAmountsInBaseCurrency GetBaseCurrencyAmounts()
        {
            return new IPCAmountsInBaseCurrency
            {
                GrossAmount = BaseCurrencyGrossAmount,
                RetentionAmount = RetentionAmount * ExchangeRate,
                NetAmount = BaseCurrencyNetAmount,
                CurrentAmount = CurrentAmount * ExchangeRate,
                FXGainLoss = FXGainLoss
            };
        }
    }

    /// <summary>
    /// Helper class for IPC amounts in base currency
    /// </summary>
    public class IPCAmountsInBaseCurrency
    {
        public double GrossAmount { get; set; }
        public double RetentionAmount { get; set; }
        public double NetAmount { get; set; }
        public double CurrentAmount { get; set; }
        public double FXGainLoss { get; set; }
    }

    /// <summary>
    /// IPC line model
    /// </summary>
    public class IPCLine
    {
        public int LineNum { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double CompletionPercentage { get; set; }
        public string Remarks { get; set; }

        public IPCLine()
        {
            Quantity = 1;
        }

        /// <summary>
        /// Calculate line amount
        /// </summary>
        public void CalculateAmount()
        {
            Amount = Quantity * UnitPrice;
        }
    }

    /// <summary>
    /// Change Order model
    /// </summary>
    public class ChangeOrder
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public int ChangeOrderNumber { get; set; }
        public DateTime ChangeOrderDate { get; set; }
        public string Type { get; set; } // Addition, Deduction, TimeExtension
        public string Description { get; set; }
        public double Amount { get; set; }
        public int AdditionalDays { get; set; }
        public string Status { get; set; } // Draft, Submitted, Approved, Rejected
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Justification { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        // Multi-Currency fields
        public string Currency { get; set; }                // Change order currency (usually same as contract)
        public string BaseCurrency { get; set; }            // System base currency
        public double ExchangeRate { get; set; }            // Rate at change order date
        public double BaseCurrencyAmount { get; set; }      // Amount in base currency
        public double FXGainLoss { get; set; }              // FX gain/loss vs contract rate
        public DateTime? LastFXUpdateDate { get; set; }     // When FX was last calculated

        // Collections
        public List<ChangeOrderLine> Lines { get; set; }

        public ChangeOrder()
        {
            Lines = new List<ChangeOrderLine>();
            Status = "Draft";
            Currency = "USD";
            BaseCurrency = "USD";
            ExchangeRate = 1.0;
            BaseCurrencyAmount = 0.0;
            FXGainLoss = 0.0;
            ChangeOrderDate = DateTime.Today;
            CreatedDate = DateTime.Now;
            Type = "Addition";
        }

        /// <summary>
        /// Calculate total from lines
        /// </summary>
        public void CalculateTotal()
        {
            Amount = 0;
            foreach (var line in Lines)
            {
                Amount += line.LineTotal;
            }
        }

        /// <summary>
        /// Validate change order data
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
                errors.Add("Contract is required");

            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required");

            if (ChangeOrderNumber <= 0)
                errors.Add("Change order number must be greater than zero");

            if (Lines == null || Lines.Count == 0)
                errors.Add("At least one line item is required");

            // Multi-currency validation
            if (string.IsNullOrWhiteSpace(Currency))
                errors.Add("Currency is required");

            if (string.IsNullOrWhiteSpace(BaseCurrency))
                errors.Add("Base currency is required");

            if (ExchangeRate <= 0)
                errors.Add("Exchange rate must be greater than zero");

            return errors;
        }

        /// <summary>
        /// Update base currency amount using current exchange rate
        /// </summary>
        public void UpdateBaseCurrencyAmount()
        {
            if (Currency == BaseCurrency)
            {
                ExchangeRate = 1.0;
                BaseCurrencyAmount = Amount;
            }
            else
            {
                BaseCurrencyAmount = Amount * ExchangeRate;
            }
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Calculate FX gain/loss compared to contract exchange rate
        /// </summary>
        public void CalculateFXGainLoss(double contractExchangeRate)
        {
            if (Currency == BaseCurrency)
            {
                FXGainLoss = 0.0;
                return;
            }

            double amountAtContractRate = Amount * contractExchangeRate;
            double amountAtChangeOrderRate = Amount * ExchangeRate;
            FXGainLoss = amountAtChangeOrderRate - amountAtContractRate;
            LastFXUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Format amount with currency symbol
        /// </summary>
        public string FormatAmount(Currency currencyInfo)
        {
            if (currencyInfo == null)
                return Amount.ToString("N2");

            return currencyInfo.FormatAmount(Amount);
        }

        /// <summary>
        /// Get signed amount (negative for deductions)
        /// </summary>
        public double GetSignedAmount()
        {
            if (Type == "Deduction")
                return -Amount;
            return Amount;
        }

        /// <summary>
        /// Get signed base currency amount (negative for deductions)
        /// </summary>
        public double GetSignedBaseCurrencyAmount()
        {
            if (Type == "Deduction")
                return -BaseCurrencyAmount;
            return BaseCurrencyAmount;
        }
    }

    /// <summary>
    /// Change Order line model
    /// </summary>
    public class ChangeOrderLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
        public string Remarks { get; set; }

        public ChangeOrderLine()
        {
            Quantity = 1;
            UnitOfMeasure = "EA";
        }

        /// <summary>
        /// Calculate line total
        /// </summary>
        public void CalculateLineTotal()
        {
            LineTotal = Quantity * UnitPrice;
        }
    }
}
