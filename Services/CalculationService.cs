using System;
using System.Collections.Generic;
using ContractManagementAddon.Models;
using ContractManagementAddon.Utilities;

namespace ContractManagementAddon.Services
{
    /// <summary>
    /// Calculation service for financial and project calculations
    /// </summary>
    public class CalculationService
    {
        /// <summary>
        /// Calculate retention amount
        /// </summary>
        public double CalculateRetention(double grossAmount, double retentionPercentage)
        {
            try
            {
                if (!ValidationHelper.IsValidPercentage(retentionPercentage))
                {
                    throw new ArgumentException("Invalid retention percentage");
                }

                return RoundMoney(grossAmount * (retentionPercentage / 100));
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating retention", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate net amount after retention
        /// </summary>
        public double CalculateNetAmount(double grossAmount, double retentionAmount)
        {
            return RoundMoney(grossAmount - retentionAmount);
        }

        /// <summary>
        /// Calculate completion percentage
        /// </summary>
        public double CalculateCompletionPercentage(double completedAmount, double totalAmount)
        {
            try
            {
                if (totalAmount <= 0)
                {
                    return 0;
                }

                return RoundPercentage((completedAmount / totalAmount) * 100);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating completion percentage", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate completion percentage based on contract quantity and cumulative quantity.
        /// Implements division-by-zero protection as per Document 7.
        /// </summary>
        public double CalculateCompletionByQuantity(double cumulativeQuantity, double contractQuantity)
        {
            try
            {
                if (contractQuantity <= 0)
                {
                    return 0;
                }

                return RoundPercentage((cumulativeQuantity / contractQuantity) * 100);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating quantity-based completion percentage", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate weighted completion based on multiple items
        /// </summary>
        public double CalculateWeightedCompletion(List<Tuple<double, double>> itemsWithCompletion)
        {
            try
            {
                if (itemsWithCompletion == null || itemsWithCompletion.Count == 0)
                {
                    return 0;
                }

                double totalValue = 0;
                double weightedSum = 0;

                foreach (var item in itemsWithCompletion)
                {
                    double value = item.Item1;
                    double completion = item.Item2;

                    totalValue += value;
                    weightedSum += (value * completion);
                }

                if (totalValue == 0)
                {
                    return 0;
                }

                return Math.Round(weightedSum / totalValue, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating weighted completion", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate project schedule performance index (SPI)
        /// </summary>
        public double CalculateSPI(DateTime startDate, DateTime endDate, double completionPercentage)
        {
            try
            {
                // SPI = Earned Value / Planned Value
                // Where:
                // - Earned Value = % Complete
                // - Planned Value = Time Elapsed / Total Time

                int totalDays = (endDate - startDate).Days;
                if (totalDays <= 0)
                {
                    return 1.0;
                }

                int elapsedDays = (DateTime.Today - startDate).Days;
                double plannedCompletion = Math.Min((double)elapsedDays / totalDays * 100, 100);

                if (plannedCompletion == 0)
                {
                    return 1.0;
                }

                double spi = completionPercentage / plannedCompletion;
                return Math.Round(spi, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating SPI", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate cost performance index (CPI)
        /// </summary>
        public double CalculateCPI(double earnedValue, double actualCost)
        {
            try
            {
                // CPI = Earned Value / Actual Cost

                if (actualCost <= 0)
                {
                    return 1.0;
                }

                double cpi = earnedValue / actualCost;
                return Math.Round(cpi, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating CPI", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate estimate at completion (EAC)
        /// </summary>
        public double CalculateEAC(double budgetAtCompletion, double cpi)
        {
            try
            {
                // EAC = BAC / CPI

                if (cpi <= 0)
                {
                    return budgetAtCompletion;
                }

                double eac = budgetAtCompletion / cpi;
                return Math.Round(eac, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating EAC", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate estimate to complete (ETC)
        /// </summary>
        public double CalculateETC(double eac, double actualCost)
        {
            try
            {
                // ETC = EAC - Actual Cost

                double etc = eac - actualCost;
                return Math.Round(etc, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating ETC", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate variance at completion (VAC)
        /// </summary>
        public double CalculateVAC(double budgetAtCompletion, double eac)
        {
            try
            {
                // VAC = BAC - EAC

                double vac = budgetAtCompletion - eac;
                return Math.Round(vac, 2);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating VAC", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate projected completion date
        /// </summary>
        public DateTime CalculateProjectedCompletionDate(DateTime startDate, DateTime plannedEndDate, double spi)
        {
            try
            {
                if (spi <= 0)
                {
                    return plannedEndDate;
                }

                int totalPlannedDays = (plannedEndDate - startDate).Days;
                int projectedTotalDays = (int)(totalPlannedDays / spi);

                return startDate.AddDays(projectedTotalDays);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating projected completion date", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate cash flow projection
        /// </summary>
        public Dictionary<DateTime, double> CalculateCashFlowProjection(
            double totalValue,
            DateTime startDate,
            DateTime endDate,
            string pattern = "linear")
        {
            try
            {
                var cashFlow = new Dictionary<DateTime, double>();

                int totalMonths = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;

                if (totalMonths <= 0)
                {
                    return cashFlow;
                }

                switch (pattern.ToLower())
                {
                    case "linear":
                        // Equal distribution
                        double monthlyAmount = totalValue / totalMonths;
                        for (int i = 0; i < totalMonths; i++)
                        {
                            DateTime month = startDate.AddMonths(i);
                            cashFlow[month] = Math.Round(monthlyAmount, 2);
                        }
                        break;

                    case "scurve":
                        // S-Curve: slow start, fast middle, slow end
                        for (int i = 0; i < totalMonths; i++)
                        {
                            DateTime month = startDate.AddMonths(i);
                            double progress = (double)i / totalMonths;
                            // Using cumulative normal distribution approximation
                            double cumulativePct = 1 / (1 + Math.Exp(-6 * (progress - 0.5)));
                            double previousPct = i == 0 ? 0 : 1 / (1 + Math.Exp(-6 * ((double)(i - 1) / totalMonths - 0.5)));
                            double monthlyPct = cumulativePct - previousPct;
                            cashFlow[month] = Math.Round(totalValue * monthlyPct, 2);
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unknown cash flow pattern: {pattern}");
                }

                return cashFlow;
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating cash flow projection", ex);
                throw;
            }
        }

        /// <summary>
        /// Calculate profitability metrics
        /// </summary>
        public Dictionary<string, double> CalculateProfitability(double revenue, double costs, double overhead)
        {
            try
            {
                double grossProfit = revenue - costs;
                double netProfit = grossProfit - overhead;
                double grossMargin = revenue > 0 ? (grossProfit / revenue) * 100 : 0;
                double netMargin = revenue > 0 ? (netProfit / revenue) * 100 : 0;

                return new Dictionary<string, double>
                {
                    ["Revenue"] = RoundMoney(revenue),
                    ["Costs"] = RoundMoney(costs),
                    ["Overhead"] = RoundMoney(overhead),
                    ["GrossProfit"] = RoundMoney(grossProfit),
                    ["NetProfit"] = RoundMoney(netProfit),
                    ["GrossMargin"] = RoundPercentage(grossMargin),
                    ["NetMargin"] = RoundPercentage(netMargin)
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating profitability", ex);
                throw;
            }
        }

        /// <summary>
        /// Monetary rounding helper (2 decimals, half-up)
        /// </summary>
        private double RoundMoney(double value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Percentage rounding helper (2 decimals, half-up)
        /// </summary>
        private double RoundPercentage(double value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
