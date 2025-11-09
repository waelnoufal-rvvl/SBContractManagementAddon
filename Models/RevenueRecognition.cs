using System;
using System.Collections.Generic;

namespace ContractManagementAddon.Models
{
    /// <summary>
    /// Performance Obligation - ASC 606 Step 2
    /// Represents a distinct good or service promised in a contract
    /// </summary>
    public class PerformanceObligation
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public int ObligationNumber { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // Good, Service, Bundle
        public double StandaloneSellingPrice { get; set; }
        public double AllocatedPrice { get; set; }
        public string RecognitionMethod { get; set; } // PointInTime, OverTime
        public string ProgressMethod { get; set; } // CostToCost, UnitsDelivered, Milestone, Time
        public double TotalEstimatedCost { get; set; }
        public double TotalUnits { get; set; }
        public string Status { get; set; } // NotStarted, InProgress, Completed
        public double CompletionPercentage { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }

        public List<PerformanceObligationLine> Lines { get; set; } = new List<PerformanceObligationLine>();

        // Constants for Type
        public const string TYPE_GOOD = "Good";
        public const string TYPE_SERVICE = "Service";
        public const string TYPE_BUNDLE = "Bundle";

        // Constants for Recognition Method
        public const string RECOG_POINT_IN_TIME = "PointInTime";
        public const string RECOG_OVER_TIME = "OverTime";

        // Constants for Progress Method
        public const string PROGRESS_COST_TO_COST = "CostToCost";
        public const string PROGRESS_UNITS_DELIVERED = "UnitsDelivered";
        public const string PROGRESS_MILESTONE = "Milestone";
        public const string PROGRESS_TIME = "Time";

        // Constants for Status
        public const string STATUS_NOT_STARTED = "NotStarted";
        public const string STATUS_IN_PROGRESS = "InProgress";
        public const string STATUS_COMPLETED = "Completed";

        /// <summary>
        /// Calculate completion percentage based on progress method
        /// </summary>
        public void CalculateCompletionPercentage()
        {
            switch (ProgressMethod)
            {
                case PROGRESS_COST_TO_COST:
                    // Calculate based on actual cost vs. estimated cost
                    if (TotalEstimatedCost > 0)
                    {
                        double actualCost = 0;
                        if (Lines != null)
                        {
                            foreach (var line in Lines)
                            {
                                actualCost += line.ActualCost;
                            }
                        }
                        CompletionPercentage = (actualCost / TotalEstimatedCost) * 100;
                    }
                    else
                    {
                        // No estimated cost - set to zero
                        CompletionPercentage = 0;
                    }
                    break;

                case PROGRESS_UNITS_DELIVERED:
                    // Calculate based on units delivered vs. total units
                    if (TotalUnits > 0)
                    {
                        double deliveredUnits = 0;
                        if (Lines != null)
                        {
                            foreach (var line in Lines)
                            {
                                deliveredUnits += line.Quantity * (line.CompletionPercentage / 100);
                            }
                        }
                        CompletionPercentage = (deliveredUnits / TotalUnits) * 100;
                    }
                    else
                    {
                        // No units - set to zero
                        CompletionPercentage = 0;
                    }
                    break;

                case PROGRESS_MILESTONE:
                    // Average of line completion percentages
                    if (Lines != null && Lines.Count > 0)
                    {
                        double totalCompletion = 0;
                        foreach (var line in Lines)
                        {
                            totalCompletion += line.CompletionPercentage;
                        }
                        CompletionPercentage = totalCompletion / Lines.Count;
                    }
                    else
                    {
                        // No lines - set to zero
                        CompletionPercentage = 0;
                    }
                    break;

                case PROGRESS_TIME:
                    // Time-based progress - set externally
                    // Don't modify CompletionPercentage here
                    break;
            }

            // Cap at 100%
            if (CompletionPercentage > 100)
            {
                CompletionPercentage = 100;
            }

            // Ensure non-negative
            if (CompletionPercentage < 0)
            {
                CompletionPercentage = 0;
            }
        }

        /// <summary>
        /// Calculate revenue to recognize based on completion percentage
        /// </summary>
        public double CalculateRevenueToRecognize()
        {
            if (RecognitionMethod == RECOG_POINT_IN_TIME)
            {
                // Revenue recognized only when obligation is completed
                return (CompletionPercentage >= 100) ? AllocatedPrice : 0;
            }
            else if (RecognitionMethod == RECOG_OVER_TIME)
            {
                // Revenue recognized proportionally based on progress
                return AllocatedPrice * (CompletionPercentage / 100);
            }

            return 0;
        }

        /// <summary>
        /// Validate performance obligation
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
            {
                errors.Add("Contract Code is required");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                errors.Add("Description is required");
            }

            if (StandaloneSellingPrice <= 0)
            {
                errors.Add("Standalone Selling Price must be greater than zero");
            }

            if (AllocatedPrice < 0)
            {
                errors.Add("Allocated Price cannot be negative");
            }

            if (string.IsNullOrWhiteSpace(RecognitionMethod))
            {
                errors.Add("Recognition Method is required");
            }

            if (RecognitionMethod == RECOG_OVER_TIME && string.IsNullOrWhiteSpace(ProgressMethod))
            {
                errors.Add("Progress Method is required for Over Time recognition");
            }

            return errors;
        }
    }

    /// <summary>
    /// Performance Obligation Line
    /// Detail line for tracking completion of obligation components
    /// </summary>
    public class PerformanceObligationLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public double EstimatedCost { get; set; }
        public double ActualCost { get; set; }
        public double CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Revenue Recognition Schedule - ASC 606 Step 5
    /// Tracks when revenue should be and has been recognized
    /// </summary>
    public class RevenueSchedule
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public string PerformanceObligationCode { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public double ScheduledRevenue { get; set; }
        public double RecognizedRevenue { get; set; }
        public double DeferredRevenue { get; set; }
        public double CumulativeRevenue { get; set; }
        public string RecognitionBasis { get; set; } // Cost, Milestone, Time, Units
        public double ProgressPercentage { get; set; }
        public string Status { get; set; } // Scheduled, Recognized, Adjusted
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }

        // Constants for Status
        public const string STATUS_SCHEDULED = "Scheduled";
        public const string STATUS_RECOGNIZED = "Recognized";
        public const string STATUS_ADJUSTED = "Adjusted";

        /// <summary>
        /// Calculate deferred revenue for this period
        /// </summary>
        public void CalculateDeferredRevenue()
        {
            // Deferred revenue should be the amount scheduled but not yet recognized
            // If recognized > scheduled (catch-up scenario), deferred is zero (not negative)
            DeferredRevenue = Math.Max(0, ScheduledRevenue - RecognizedRevenue);
        }

        /// <summary>
        /// Recognize revenue for this period
        /// </summary>
        public void RecognizeRevenue(double amount)
        {
            RecognizedRevenue = amount;
            Status = STATUS_RECOGNIZED;
            CalculateDeferredRevenue();
        }

        /// <summary>
        /// Validate revenue schedule
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
            {
                errors.Add("Contract Code is required");
            }

            // Validate PerformanceObligationCode
            if (string.IsNullOrWhiteSpace(PerformanceObligationCode))
            {
                errors.Add("Performance Obligation Code is required");
            }

            if (PeriodStartDate >= PeriodEndDate)
            {
                errors.Add("Period Start Date must be before Period End Date");
            }

            // Validate date is not too far in future
            if (PeriodStartDate > DateTime.Today.AddYears(10))
            {
                errors.Add("Period Start Date is too far in the future");
            }

            if (ScheduledRevenue < 0)
            {
                errors.Add("Scheduled Revenue cannot be negative");
            }

            if (RecognizedRevenue < 0)
            {
                errors.Add("Recognized Revenue cannot be negative");
            }

            // Allow recognized to exceed scheduled in catch-up scenarios
            // Just add a warning if significantly over (50%+)
            if (RecognizedRevenue > ScheduledRevenue * 1.5)
            {
                errors.Add($"Warning: Recognized Revenue ({RecognizedRevenue:N2}) significantly exceeds Scheduled Revenue ({ScheduledRevenue:N2})");
            }

            // Validate cumulative revenue
            if (CumulativeRevenue < 0)
            {
                errors.Add("Cumulative Revenue cannot be negative");
            }

            if (CumulativeRevenue < RecognizedRevenue)
            {
                errors.Add("Cumulative Revenue cannot be less than Recognized Revenue for this period");
            }

            // Validate status
            if (!string.IsNullOrWhiteSpace(Status))
            {
                if (Status != STATUS_SCHEDULED && Status != STATUS_RECOGNIZED && Status != STATUS_ADJUSTED)
                {
                    errors.Add($"Invalid status: {Status}. Must be one of: {STATUS_SCHEDULED}, {STATUS_RECOGNIZED}, {STATUS_ADJUSTED}");
                }
            }

            return errors;
        }
    }

    /// <summary>
    /// Deferred Revenue
    /// Tracks revenue that has been billed but not yet earned
    /// </summary>
    public class DeferredRevenue
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public string PerformanceObligationCode { get; set; }
        public string IPCCode { get; set; }
        public double BilledAmount { get; set; }
        public double RecognizedRevenue { get; set; }
        public double DeferredAmount { get; set; }
        public string DeferralReason { get; set; }
        public string ReleaseSchedule { get; set; } // JSON or schedule description
        public string Status { get; set; } // Active, Released, Cancelled
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }

        // Constants for Status
        public const string STATUS_ACTIVE = "Active";
        public const string STATUS_RELEASED = "Released";
        public const string STATUS_CANCELLED = "Cancelled";

        // Constants for Deferral Reason
        public const string REASON_AHEAD_OF_DELIVERY = "AheadOfDelivery";
        public const string REASON_PREPAYMENT = "Prepayment";
        public const string REASON_MILESTONE = "Milestone";

        /// <summary>
        /// Calculate deferred amount
        /// </summary>
        public void CalculateDeferredAmount()
        {
            DeferredAmount = BilledAmount - RecognizedRevenue;

            if (DeferredAmount <= 0)
            {
                Status = STATUS_RELEASED;
            }
        }

        /// <summary>
        /// Release deferred revenue
        /// </summary>
        public void ReleaseRevenue(double amount)
        {
            RecognizedRevenue += amount;
            CalculateDeferredAmount();
        }

        /// <summary>
        /// Validate deferred revenue
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
            {
                errors.Add("Contract Code is required");
            }

            if (BilledAmount < 0)
            {
                errors.Add("Billed Amount cannot be negative");
            }

            if (RecognizedRevenue < 0)
            {
                errors.Add("Recognized Revenue cannot be negative");
            }

            if (RecognizedRevenue > BilledAmount)
            {
                errors.Add("Recognized Revenue cannot exceed Billed Amount");
            }

            return errors;
        }
    }

    /// <summary>
    /// Contract Assets & Liabilities
    /// Balance sheet items per ASC 606
    /// </summary>
    public class ContractAssetsLiabilities
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public DateTime AsOfDate { get; set; }
        public double ContractAsset { get; set; } // Revenue recognized > Billed (Unbilled Revenue/WIP)
        public double ContractLiability { get; set; } // Billed > Revenue recognized (Deferred Revenue)
        public double NetPosition { get; set; } // Asset - Liability
        public string Currency { get; set; }
        public double BaseCurrencyValue { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }

        /// <summary>
        /// Calculate net position
        /// </summary>
        public void CalculateNetPosition()
        {
            NetPosition = ContractAsset - ContractLiability;
        }

        /// <summary>
        /// Validate contract assets/liabilities
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
            {
                errors.Add("Contract Code is required");
            }

            if (ContractAsset < 0)
            {
                errors.Add("Contract Asset cannot be negative");
            }

            if (ContractLiability < 0)
            {
                errors.Add("Contract Liability cannot be negative");
            }

            return errors;
        }
    }

    /// <summary>
    /// Revenue Recognition Log
    /// Audit trail for all revenue recognition transactions
    /// </summary>
    public class RevenueRecognitionLog
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public string PerformanceObligationCode { get; set; }
        public DateTime RecognitionDate { get; set; }
        public double RecognitionAmount { get; set; }
        public string RecognitionMethod { get; set; }
        public double CumulativeAmount { get; set; }
        public int? JournalEntryRef { get; set; }
        public string Notes { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }
    }

    /// <summary>
    /// Contract Backlog
    /// Tracks remaining contract value and revenue forecasting
    /// </summary>
    public class ContractBacklog
    {
        public string Code { get; set; }
        public string DocNum { get; set; }
        public string ContractCode { get; set; }
        public DateTime AsOfDate { get; set; }
        public double TotalContractValue { get; set; }
        public double BilledToDate { get; set; }
        public double RevenueRecognizedToDate { get; set; }
        public double RemainingBacklog { get; set; }
        public double Forecast30Days { get; set; }
        public double Forecast60Days { get; set; }
        public double Forecast90Days { get; set; }
        public double BurnRate { get; set; } // Average revenue per period
        public DateTime? EstimatedCompletionDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; }

        /// <summary>
        /// Calculate remaining backlog
        /// </summary>
        public void CalculateRemainingBacklog()
        {
            RemainingBacklog = TotalContractValue - RevenueRecognizedToDate;
        }

        /// <summary>
        /// Calculate burn rate (average revenue per month)
        /// </summary>
        public void CalculateBurnRate(DateTime contractStartDate)
        {
            if (RevenueRecognizedToDate > 0)
            {
                double monthsElapsed = (AsOfDate - contractStartDate).TotalDays / 30.0;
                if (monthsElapsed > 0)
                {
                    BurnRate = RevenueRecognizedToDate / monthsElapsed;
                }
            }
        }

        /// <summary>
        /// Estimate completion date based on burn rate
        /// </summary>
        public void EstimateCompletionDate()
        {
            if (BurnRate > 0 && RemainingBacklog > 0)
            {
                double monthsRemaining = RemainingBacklog / BurnRate;
                EstimatedCompletionDate = AsOfDate.AddDays(monthsRemaining * 30);
            }
        }

        /// <summary>
        /// Calculate revenue forecasts
        /// </summary>
        public void CalculateForecasts()
        {
            // Simple forecast based on burn rate
            if (BurnRate > 0)
            {
                Forecast30Days = Math.Min(BurnRate, RemainingBacklog);
                Forecast60Days = Math.Min(BurnRate * 2, RemainingBacklog);
                Forecast90Days = Math.Min(BurnRate * 3, RemainingBacklog);
            }
        }

        /// <summary>
        /// Validate backlog
        /// </summary>
        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ContractCode))
            {
                errors.Add("Contract Code is required");
            }

            if (TotalContractValue < 0)
            {
                errors.Add("Total Contract Value cannot be negative");
            }

            if (RevenueRecognizedToDate < 0)
            {
                errors.Add("Revenue Recognized To Date cannot be negative");
            }

            if (RevenueRecognizedToDate > TotalContractValue)
            {
                errors.Add("Revenue Recognized cannot exceed Total Contract Value");
            }

            return errors;
        }
    }

    /// <summary>
    /// Helper class for revenue recognition calculations
    /// </summary>
    public class RevenueRecognitionCalculator
    {
        /// <summary>
        /// Calculate revenue to recognize using cost-to-cost method
        /// </summary>
        public static double CalculateRevenueCostToCost(double totalContractPrice, double actualCostIncurred, double totalEstimatedCost)
        {
            if (totalEstimatedCost <= 0)
            {
                return 0;
            }

            double completionPercentage = actualCostIncurred / totalEstimatedCost;

            // Cap at 100%
            if (completionPercentage > 1.0)
            {
                completionPercentage = 1.0;
            }

            return totalContractPrice * completionPercentage;
        }

        /// <summary>
        /// Calculate revenue to recognize using units-delivered method
        /// </summary>
        public static double CalculateRevenueUnitsDelivered(double totalContractPrice, double unitsDelivered, double totalUnits)
        {
            if (totalUnits <= 0)
            {
                return 0;
            }

            double completionPercentage = unitsDelivered / totalUnits;

            // Cap at 100%
            if (completionPercentage > 1.0)
            {
                completionPercentage = 1.0;
            }

            return totalContractPrice * completionPercentage;
        }

        /// <summary>
        /// Calculate revenue to recognize using time-based method
        /// </summary>
        public static double CalculateRevenueTimeBased(double totalContractPrice, DateTime startDate, DateTime endDate, DateTime currentDate)
        {
            if (currentDate <= startDate)
            {
                return 0;
            }

            if (currentDate >= endDate)
            {
                return totalContractPrice;
            }

            double totalDays = (endDate - startDate).TotalDays;
            double elapsedDays = (currentDate - startDate).TotalDays;

            if (totalDays <= 0)
            {
                return 0;
            }

            return totalContractPrice * (elapsedDays / totalDays);
        }
    }
}
