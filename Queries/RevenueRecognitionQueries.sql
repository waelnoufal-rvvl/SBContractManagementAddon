-- =====================================================
-- SAP B1 Query Scripts for Revenue Recognition
-- Contract Management Add-on - Phase 1
-- ASC 606 / IFRS 15 Compliance Reporting
-- =====================================================
-- Compatible with: SQL Server & SAP HANA
-- Version: 1.0
-- Date: November 2025
-- =====================================================

-- =====================================================
-- QUERY 1: Revenue Recognition Schedule
-- QRY_CM_REVENUE_SCHEDULE
-- Purpose: Period-by-period revenue recognition tracking
-- =====================================================
SELECT
    T0."Code" AS "Schedule Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T1."U_Description" AS "Contract Description",
    T0."U_PerfOblCode" AS "Performance Obligation",
    T0."U_PeriodStartDate" AS "Period Start",
    T0."U_PeriodEndDate" AS "Period End",
    T0."U_ScheduledRevenue" AS "Scheduled Revenue",
    T0."U_RecognizedRevenue" AS "Recognized Revenue",
    T0."U_DeferredRevenue" AS "Deferred Revenue",
    T0."U_CumulativeRevenue" AS "Cumulative Revenue",
    T0."U_RecogBasis" AS "Recognition Basis",
    T0."U_ProgressPct" AS "Progress %",
    T0."U_Status" AS "Status",
    T0."U_CreateDate" AS "Created Date",
    T0."U_CreateUser" AS "Created By"
FROM "@CM_REV_SCHEDULE" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T0."U_Status" IN ('Scheduled', 'Recognized')
ORDER BY T0."U_ContractCode", T0."U_PeriodStartDate";

-- =====================================================
-- QUERY 2: Performance Obligations Status
-- QRY_CM_PERF_OBLIGATIONS
-- Purpose: Track performance obligations and their completion
-- =====================================================
SELECT
    T0."Code" AS "Obligation Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T1."U_TotalValue" AS "Contract Value",
    T0."U_OblNumber" AS "Obligation #",
    T0."U_Description" AS "Description",
    T0."U_Type" AS "Type",
    T0."U_StandaloneSP" AS "Standalone Selling Price",
    T0."U_AllocatedPrice" AS "Allocated Price",
    T0."U_RecogMethod" AS "Recognition Method",
    T0."U_ProgressMethod" AS "Progress Method",
    T0."U_Status" AS "Status",
    T0."U_CompletionPct" AS "Completion %",
    T0."U_TotalEstCost" AS "Total Est. Cost",
    T0."U_TotalUnits" AS "Total Units",
    -- Calculate revenue to recognize
    CASE
        WHEN T0."U_RecogMethod" = 'PointInTime' THEN
            CASE WHEN T0."U_CompletionPct" >= 100 THEN T0."U_AllocatedPrice" ELSE 0 END
        WHEN T0."U_RecogMethod" = 'OverTime' THEN
            T0."U_AllocatedPrice" * (T0."U_CompletionPct" / 100)
        ELSE 0
    END AS "Revenue to Recognize",
    T0."U_CreateDate" AS "Created Date"
FROM "@CM_PERF_OBL" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T1."U_Status" IN ('Active', 'Completed')
ORDER BY T0."U_ContractCode", T0."U_OblNumber";

-- =====================================================
-- QUERY 3: Deferred Revenue Report
-- QRY_CM_DEFERRED_REVENUE
-- Purpose: Track billed but unearned revenue
-- =====================================================
SELECT
    T0."Code" AS "Deferred Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T0."U_IPCCode" AS "IPC Code",
    T0."U_BilledAmount" AS "Billed Amount",
    T0."U_RecognizedRevenue" AS "Recognized Revenue",
    T0."U_DeferredAmount" AS "Deferred Amount",
    T0."U_DeferralReason" AS "Deferral Reason",
    T0."U_Status" AS "Status",
    -- Calculate deferral percentage
    CASE
        WHEN T0."U_BilledAmount" > 0 THEN
            (T0."U_DeferredAmount" / T0."U_BilledAmount") * 100
        ELSE 0
    END AS "Deferral %",
    T0."U_CreateDate" AS "Created Date",
    T0."U_CreateUser" AS "Created By"
FROM "@CM_DEFERRED_REV" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T0."U_Status" = 'Active'
    AND T0."U_DeferredAmount" > 0
ORDER BY T0."U_DeferredAmount" DESC;

-- =====================================================
-- QUERY 4: Contract Assets & Liabilities
-- QRY_CM_CONTRACT_ASSETS_LIABILITIES
-- Purpose: Balance sheet reporting per ASC 606
-- =====================================================
SELECT
    T0."Code" AS "Record Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T1."U_Description" AS "Contract Description",
    T0."U_AsOfDate" AS "As Of Date",
    T0."U_ContractAsset" AS "Contract Asset (Unbilled Revenue)",
    T0."U_ContractLiability" AS "Contract Liability (Deferred Revenue)",
    T0."U_NetPosition" AS "Net Position",
    T0."U_Currency" AS "Currency",
    T0."U_BaseCurrencyValue" AS "Base Currency Value",
    -- Calculate asset/liability ratio
    CASE
        WHEN (T0."U_ContractAsset" + T0."U_ContractLiability") > 0 THEN
            (T0."U_ContractAsset" / (T0."U_ContractAsset" + T0."U_ContractLiability")) * 100
        ELSE 0
    END AS "Asset Ratio %",
    T0."U_CreateDate" AS "Created Date"
FROM "@CM_CONTRACT_ASSETS" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
ORDER BY T0."U_AsOfDate" DESC, T0."U_ContractCode";

-- =====================================================
-- QUERY 5: Revenue Backlog Report
-- QRY_CM_REVENUE_BACKLOG
-- Purpose: Track remaining contract value and forecasts
-- =====================================================
SELECT
    T0."Code" AS "Backlog Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T1."U_Description" AS "Contract Description",
    T0."U_AsOfDate" AS "As Of Date",
    T0."U_TotalContractValue" AS "Total Contract Value",
    T0."U_BilledToDate" AS "Billed To Date",
    T0."U_RevenueRecogToDate" AS "Revenue Recognized To Date",
    T0."U_RemainingBacklog" AS "Remaining Backlog",
    -- Calculate completion percentage
    CASE
        WHEN T0."U_TotalContractValue" > 0 THEN
            (T0."U_RevenueRecogToDate" / T0."U_TotalContractValue") * 100
        ELSE 0
    END AS "Completion %",
    T0."U_BurnRate" AS "Burn Rate (Monthly)",
    T0."U_Forecast30Days" AS "30-Day Forecast",
    T0."U_Forecast60Days" AS "60-Day Forecast",
    T0."U_Forecast90Days" AS "90-Day Forecast",
    T0."U_EstCompletionDate" AS "Est. Completion Date",
    -- Calculate months remaining
    CASE
        WHEN T0."U_BurnRate" > 0 THEN
            T0."U_RemainingBacklog" / T0."U_BurnRate"
        ELSE NULL
    END AS "Months Remaining"
FROM "@CM_BACKLOG" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T0."U_RemainingBacklog" > 0
ORDER BY T0."U_RemainingBacklog" DESC;

-- =====================================================
-- QUERY 6: Revenue Recognition Summary by Contract
-- QRY_CM_REVENUE_SUMMARY
-- Purpose: High-level revenue summary per contract
-- =====================================================
SELECT
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_Description" AS "Contract Description",
    T0."U_TotalValue" AS "Contract Value",
    T0."U_Currency" AS "Currency",
    T0."U_Status" AS "Status",
    -- Performance Obligations
    (SELECT COUNT(*) FROM "@CM_PERF_OBL" WHERE "U_ContractCode" = T0."Code") AS "# Obligations",
    -- Total Allocated Price
    (SELECT COALESCE(SUM("U_AllocatedPrice"), 0) FROM "@CM_PERF_OBL"
     WHERE "U_ContractCode" = T0."Code") AS "Total Allocated Price",
    -- Revenue Recognized
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") AS "Revenue Recognized",
    -- Billed Amount (from IPCs)
    (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" IN ('Approved', 'Paid')) AS "Total Billed",
    -- Deferred Revenue
    (SELECT COALESCE(SUM("U_DeferredAmount"), 0) FROM "@CM_DEFERRED_REV"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" = 'Active') AS "Deferred Revenue",
    -- Calculate Contract Asset/Liability
    CASE
        WHEN (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code") >
             (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
              WHERE "U_ContractCode" = T0."Code"
              AND "U_Status" IN ('Approved', 'Paid'))
        THEN (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code") -
             (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
              WHERE "U_ContractCode" = T0."Code"
              AND "U_Status" IN ('Approved', 'Paid'))
        ELSE 0
    END AS "Contract Asset (Unbilled)",
    CASE
        WHEN (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
              WHERE "U_ContractCode" = T0."Code"
              AND "U_Status" IN ('Approved', 'Paid')) >
             (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code")
        THEN (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
              WHERE "U_ContractCode" = T0."Code"
              AND "U_Status" IN ('Approved', 'Paid')) -
             (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code")
        ELSE 0
    END AS "Contract Liability (Deferred)",
    -- Revenue Recognition Percentage
    CASE
        WHEN T0."U_TotalValue" > 0 THEN
            ((SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code") / T0."U_TotalValue") * 100
        ELSE 0
    END AS "Recognition %",
    -- Average Completion
    (SELECT COALESCE(AVG("U_CompletionPct"), 0) FROM "@CM_PERF_OBL"
     WHERE "U_ContractCode" = T0."Code") AS "Avg Completion %"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Status" IN ('Active', 'Completed')
ORDER BY T0."Code";

-- =====================================================
-- QUERY 7: Revenue Waterfall Chart Data
-- QRY_CM_REVENUE_WATERFALL
-- Purpose: Visual representation of revenue flow
-- =====================================================
SELECT
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T1."U_TotalValue" AS "Contract Value",
    -- Performance Obligations Allocated
    (SELECT COALESCE(SUM("U_AllocatedPrice"), 0) FROM "@CM_PERF_OBL"
     WHERE "U_ContractCode" = T0."U_ContractCode") AS "Allocated Price",
    -- Scheduled Revenue
    SUM(T0."U_ScheduledRevenue") AS "Total Scheduled",
    -- Recognized Revenue
    SUM(T0."U_RecognizedRevenue") AS "Total Recognized",
    -- Deferred Revenue
    SUM(T0."U_DeferredRevenue") AS "Total Deferred",
    -- Remaining to Schedule
    T1."U_TotalValue" -
    (SELECT COALESCE(SUM("U_ScheduledRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."U_ContractCode") AS "Remaining to Schedule",
    -- Remaining to Recognize
    T1."U_TotalValue" -
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."U_ContractCode") AS "Remaining to Recognize"
FROM "@CM_REV_SCHEDULE" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T1."U_Status" IN ('Active', 'Completed')
GROUP BY T0."U_ContractCode", T1."U_CustomerName", T1."U_TotalValue"
ORDER BY T0."U_ContractCode";

-- =====================================================
-- QUERY 8: Unbilled Revenue (Work in Progress)
-- QRY_CM_UNBILLED_REVENUE
-- Purpose: Track revenue recognized but not yet billed
-- =====================================================
SELECT
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_Description" AS "Contract Description",
    T0."U_TotalValue" AS "Contract Value",
    -- Revenue Recognized
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") AS "Revenue Recognized",
    -- Billed Amount
    (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" IN ('Approved', 'Paid')) AS "Total Billed",
    -- Unbilled Revenue (WIP)
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") -
    (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" IN ('Approved', 'Paid')) AS "Unbilled Revenue (WIP)",
    -- WIP as percentage of contract
    CASE
        WHEN T0."U_TotalValue" > 0 THEN
            (((SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
               WHERE "U_ContractCode" = T0."Code") -
              (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
               WHERE "U_ContractCode" = T0."Code"
               AND "U_Status" IN ('Approved', 'Paid'))) / T0."U_TotalValue") * 100
        ELSE 0
    END AS "WIP %",
    T0."U_Status" AS "Status"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Status" IN ('Active', 'Completed')
    -- Only show contracts with unbilled revenue
    AND (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
         WHERE "U_ContractCode" = T0."Code") >
        (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
         WHERE "U_ContractCode" = T0."Code"
         AND "U_Status" IN ('Approved', 'Paid'))
ORDER BY "Unbilled Revenue (WIP)" DESC;

-- =====================================================
-- QUERY 9: Multi-Currency Revenue Report
-- QRY_CM_MULTICURRENCY_REVENUE
-- Purpose: Revenue summary with multi-currency support
-- =====================================================
SELECT
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_Description" AS "Contract Description",
    T0."U_Currency" AS "Currency",
    T0."U_TotalValue" AS "Contract Value (Original)",
    T0."U_BaseCurrency" AS "Base Currency",
    T0."U_ExchangeRate" AS "Exchange Rate",
    T0."U_BaseCurrValue" AS "Contract Value (Base)",
    -- Revenue in Original Currency
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") AS "Revenue Recognized (Original)",
    -- Revenue in Base Currency
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") * T0."U_ExchangeRate" AS "Revenue Recognized (Base)",
    T0."U_FXGainLoss" AS "FX Gain/Loss",
    T0."U_LastFXUpdate" AS "Last FX Update",
    T0."U_Status" AS "Status"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Status" IN ('Active', 'Completed')
    AND T0."U_Currency" IS NOT NULL
ORDER BY T0."U_Currency", T0."Code";

-- =====================================================
-- QUERY 10: FX Gain/Loss Report
-- QRY_CM_FX_GAIN_LOSS
-- Purpose: Foreign exchange impact on contracts
-- =====================================================
SELECT
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_Currency" AS "Currency",
    T0."U_TotalValue" AS "Contract Value",
    T0."U_BaseCurrency" AS "Base Currency",
    T0."U_ExchangeRate" AS "Current Rate",
    T0."U_BaseCurrValue" AS "Base Currency Value",
    T0."U_FXGainLoss" AS "FX Gain/Loss",
    -- FX Gain/Loss Percentage
    CASE
        WHEN T0."U_BaseCurrValue" > 0 THEN
            (T0."U_FXGainLoss" / T0."U_BaseCurrValue") * 100
        ELSE 0
    END AS "FX Impact %",
    T0."U_LastFXUpdate" AS "Last FX Update",
    T0."U_Status" AS "Status"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Currency" <> T0."U_BaseCurrency"
    AND T0."U_Status" IN ('Active', 'Completed')
    AND ABS(T0."U_FXGainLoss") > 0
ORDER BY ABS(T0."U_FXGainLoss") DESC;

-- =====================================================
-- QUERY 11: Revenue Recognition Audit Trail
-- QRY_CM_REVENUE_LOG
-- Purpose: Complete audit trail of revenue recognition events
-- =====================================================
SELECT
    T0."Code" AS "Log Code",
    T0."U_ContractCode" AS "Contract Code",
    T1."U_CustomerName" AS "Customer",
    T0."U_PerfOblCode" AS "Performance Obligation",
    T0."U_RecognitionDate" AS "Recognition Date",
    T0."U_RecognitionAmount" AS "Recognition Amount",
    T0."U_RecogMethod" AS "Recognition Method",
    T0."U_CumulativeAmount" AS "Cumulative Amount",
    T0."U_JournalEntryRef" AS "Journal Entry",
    T0."U_Notes" AS "Notes",
    T0."U_CreateDate" AS "Created Date",
    T0."U_CreateUser" AS "Created By"
FROM "@CM_REV_LOG" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
ORDER BY T0."U_RecognitionDate" DESC, T0."U_ContractCode";

-- =====================================================
-- QUERY 12: Revenue Recognition by Period
-- QRY_CM_REVENUE_BY_PERIOD
-- Purpose: Revenue recognized grouped by period
-- =====================================================
SELECT
    YEAR(T0."U_PeriodStartDate") AS "Year",
    MONTH(T0."U_PeriodStartDate") AS "Month",
    T0."U_PeriodStartDate" AS "Period Start",
    T0."U_PeriodEndDate" AS "Period End",
    COUNT(DISTINCT T0."U_ContractCode") AS "# Contracts",
    COUNT(*) AS "# Schedule Entries",
    SUM(T0."U_ScheduledRevenue") AS "Total Scheduled",
    SUM(T0."U_RecognizedRevenue") AS "Total Recognized",
    SUM(T0."U_DeferredRevenue") AS "Total Deferred",
    -- Recognition Rate
    CASE
        WHEN SUM(T0."U_ScheduledRevenue") > 0 THEN
            (SUM(T0."U_RecognizedRevenue") / SUM(T0."U_ScheduledRevenue")) * 100
        ELSE 0
    END AS "Recognition Rate %"
FROM "@CM_REV_SCHEDULE" T0
WHERE T0."U_Status" IN ('Scheduled', 'Recognized')
GROUP BY YEAR(T0."U_PeriodStartDate"), MONTH(T0."U_PeriodStartDate"),
         T0."U_PeriodStartDate", T0."U_PeriodEndDate"
ORDER BY T0."U_PeriodStartDate" DESC;

-- =====================================================
-- QUERY 13: Revenue Recognition Compliance Check
-- QRY_CM_REVENUE_COMPLIANCE
-- Purpose: Identify potential compliance issues
-- =====================================================
SELECT
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_TotalValue" AS "Contract Value",
    T0."U_Status" AS "Status",
    -- Check 1: Contract has performance obligations
    (SELECT COUNT(*) FROM "@CM_PERF_OBL" WHERE "U_ContractCode" = T0."Code") AS "# Obligations",
    -- Check 2: Transaction price allocated
    (SELECT COALESCE(SUM("U_AllocatedPrice"), 0) FROM "@CM_PERF_OBL"
     WHERE "U_ContractCode" = T0."Code") AS "Allocated Price",
    -- Check 3: Revenue recognition started
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") AS "Revenue Recognized",
    -- Compliance Issues
    CASE
        WHEN (SELECT COUNT(*) FROM "@CM_PERF_OBL" WHERE "U_ContractCode" = T0."Code") = 0
        THEN 'Missing Performance Obligations'
        WHEN ABS((SELECT COALESCE(SUM("U_AllocatedPrice"), 0) FROM "@CM_PERF_OBL"
                  WHERE "U_ContractCode" = T0."Code") - T0."U_TotalValue") > 0.01
        THEN 'Allocation Mismatch'
        WHEN T0."U_Status" = 'Active' AND
             (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
              WHERE "U_ContractCode" = T0."Code") = 0
        THEN 'No Revenue Recognized'
        ELSE 'Compliant'
    END AS "Compliance Status"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Status" IN ('Active', 'Completed')
ORDER BY
    CASE
        WHEN (SELECT COUNT(*) FROM "@CM_PERF_OBL" WHERE "U_ContractCode" = T0."Code") = 0 THEN 1
        WHEN ABS((SELECT COALESCE(SUM("U_AllocatedPrice"), 0) FROM "@CM_PERF_OBL"
                  WHERE "U_ContractCode" = T0."Code") - T0."U_TotalValue") > 0.01 THEN 2
        ELSE 3
    END;

-- =====================================================
-- QUERY 14: Top Contracts by Unbilled Revenue
-- QRY_CM_TOP_UNBILLED
-- Purpose: Identify contracts with highest WIP
-- =====================================================
SELECT TOP 20
    T0."Code" AS "Contract Code",
    T0."U_CustomerName" AS "Customer",
    T0."U_Description" AS "Contract Description",
    T0."U_TotalValue" AS "Contract Value",
    -- Revenue Recognized
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") AS "Revenue Recognized",
    -- Billed Amount
    (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" IN ('Approved', 'Paid')) AS "Billed",
    -- Unbilled Revenue
    (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
     WHERE "U_ContractCode" = T0."Code") -
    (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
     WHERE "U_ContractCode" = T0."Code"
     AND "U_Status" IN ('Approved', 'Paid')) AS "Unbilled Revenue",
    T0."U_Status" AS "Status"
FROM "@CONTRACT_HDR" T0
WHERE T0."U_Status" IN ('Active', 'Completed')
    AND (SELECT COALESCE(SUM("U_RecognizedRevenue"), 0) FROM "@CM_REV_SCHEDULE"
         WHERE "U_ContractCode" = T0."Code") >
        (SELECT COALESCE(SUM("U_GrossAmount"), 0) FROM "@IPC_HDR"
         WHERE "U_ContractCode" = T0."Code"
         AND "U_Status" IN ('Approved', 'Paid'))
ORDER BY "Unbilled Revenue" DESC;

-- =====================================================
-- QUERY 15: Revenue Forecast Summary
-- QRY_CM_REVENUE_FORECAST
-- Purpose: Forward-looking revenue forecast
-- =====================================================
SELECT
    'Next 30 Days' AS "Forecast Period",
    COUNT(DISTINCT T0."U_ContractCode") AS "# Contracts",
    SUM(T0."U_Forecast30Days") AS "Forecasted Revenue"
FROM "@CM_BACKLOG" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T1."U_Status" = 'Active'

UNION ALL

SELECT
    'Next 60 Days' AS "Forecast Period",
    COUNT(DISTINCT T0."U_ContractCode") AS "# Contracts",
    SUM(T0."U_Forecast60Days") AS "Forecasted Revenue"
FROM "@CM_BACKLOG" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T1."U_Status" = 'Active'

UNION ALL

SELECT
    'Next 90 Days' AS "Forecast Period",
    COUNT(DISTINCT T0."U_ContractCode") AS "# Contracts",
    SUM(T0."U_Forecast90Days") AS "Forecasted Revenue"
FROM "@CM_BACKLOG" T0
LEFT JOIN "@CONTRACT_HDR" T1 ON T0."U_ContractCode" = T1."Code"
WHERE T1."U_Status" = 'Active'

ORDER BY "Forecast Period";

-- =====================================================
-- END OF REVENUE RECOGNITION QUERIES
-- =====================================================
-- Usage Instructions:
-- 1. Open SAP Business One
-- 2. Go to: Tools > Queries > Query Generator
-- 3. Copy each query above into Query Generator
-- 4. Save with the query name (e.g., QRY_CM_REVENUE_SCHEDULE)
-- 5. Assign to appropriate query category
-- 6. Set user permissions
-- 7. Create Query Print Layouts for formatted output
-- =====================================================
