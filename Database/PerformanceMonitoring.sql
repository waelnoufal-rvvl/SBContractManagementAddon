-- ============================================================
-- Database Performance Monitoring Script
-- Contract Management Add-on
-- Database: SQL Server
-- Version: 1.0.0
-- ============================================================
--
-- PURPOSE: Monitor and analyze database performance for
--          Contract Management Add-on
--
-- USAGE:   sqlcmd -S localhost -d SBODemoUS -i PerformanceMonitoring.sql -o Results.txt
--
-- ============================================================

SET NOCOUNT ON;
GO

PRINT '============================================================';
PRINT 'CONTRACT MANAGEMENT ADD-ON - DATABASE PERFORMANCE REPORT';
PRINT 'Report Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================';
PRINT '';

-- ============================================================
-- 1. SLOW QUERIES ANALYSIS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '1. TOP 20 SLOWEST QUERIES (Contract Management Tables)';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT TOP 20
    qs.execution_count AS 'Executions',
    CAST(qs.total_elapsed_time / 1000000.0 AS DECIMAL(10,2)) AS 'Total Elapsed (sec)',
    CAST(qs.total_elapsed_time / qs.execution_count / 1000.0 AS DECIMAL(10,2)) AS 'Avg Elapsed (ms)',
    CAST(qs.total_worker_time / qs.execution_count / 1000.0 AS DECIMAL(10,2)) AS 'Avg CPU (ms)',
    qs.total_logical_reads / qs.execution_count AS 'Avg Reads',
    qs.total_logical_writes / qs.execution_count AS 'Avg Writes',
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS 'Query'
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
WHERE qt.text LIKE '%@CM_%'  -- Contract Management tables only
   OR qt.text LIKE '%CM_%'
ORDER BY qs.total_elapsed_time / qs.execution_count DESC;

PRINT '';
PRINT 'NOTE: Queries with Avg Elapsed > 1000ms should be optimized';
PRINT '';

-- ============================================================
-- 2. TABLE I/O STATISTICS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '2. TABLE I/O STATISTICS (Contract Management Tables)';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT
    OBJECT_NAME(s.object_id) AS 'Table Name',
    i.name AS 'Index Name',
    s.user_seeks AS 'Seeks',
    s.user_scans AS 'Scans',
    s.user_lookups AS 'Lookups',
    s.user_updates AS 'Updates',
    s.user_seeks + s.user_scans + s.user_lookups AS 'Total Reads',
    CASE
        WHEN s.user_seeks + s.user_scans + s.user_lookups = 0 THEN 0
        ELSE CAST(s.user_seeks AS FLOAT) / (s.user_seeks + s.user_scans + s.user_lookups) * 100
    END AS 'Seek %',
    CONVERT(VARCHAR(20), s.last_user_seek, 120) AS 'Last Seek',
    CONVERT(VARCHAR(20), s.last_user_scan, 120) AS 'Last Scan'
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) LIKE '%CM_%'
   OR OBJECT_NAME(s.object_id) LIKE '[@CM_%]'
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;

PRINT '';
PRINT 'NOTE: High Scans with low Seeks indicates missing indexes';
PRINT 'NOTE: Tables with 0 activity may indicate unused features';
PRINT '';

-- ============================================================
-- 3. MISSING INDEX RECOMMENDATIONS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '3. MISSING INDEX RECOMMENDATIONS';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT TOP 10
    OBJECT_NAME(d.object_id) AS 'Table Name',
    d.equality_columns AS 'Equality Columns',
    d.inequality_columns AS 'Inequality Columns',
    d.included_columns AS 'Included Columns',
    s.user_seeks AS 'Seeks',
    s.user_scans AS 'Scans',
    CAST(s.avg_total_user_cost AS DECIMAL(10,2)) AS 'Avg Cost',
    CAST(s.avg_user_impact AS DECIMAL(5,2)) AS 'Impact %',
    CAST(s.avg_total_user_cost * s.avg_user_impact * (s.user_seeks + s.user_scans) AS DECIMAL(15,2)) AS 'Priority Score',
    'CREATE INDEX [IX_' + OBJECT_NAME(d.object_id) + '_' +
        REPLACE(REPLACE(REPLACE(ISNULL(d.equality_columns, ''), ', ', '_'), '[', ''), ']', '') +
        CASE WHEN d.inequality_columns IS NOT NULL THEN '_' + REPLACE(REPLACE(REPLACE(d.inequality_columns, ', ', '_'), '[', ''), ']', '') ELSE '' END +
        '] ON ' + d.statement +
        ' (' + ISNULL(d.equality_columns, '') +
        CASE WHEN d.inequality_columns IS NOT NULL AND d.equality_columns IS NOT NULL THEN ', ' ELSE '' END +
        ISNULL(d.inequality_columns, '') + ')' +
        CASE WHEN d.included_columns IS NOT NULL THEN ' INCLUDE (' + d.included_columns + ')' ELSE '' END AS 'Suggested Index'
FROM sys.dm_db_missing_index_details d
INNER JOIN sys.dm_db_missing_index_groups g ON d.index_handle = g.index_handle
INNER JOIN sys.dm_db_missing_index_group_stats s ON g.index_group_handle = s.group_handle
WHERE OBJECT_NAME(d.object_id) LIKE '%CM_%'
   OR OBJECT_NAME(d.object_id) LIKE '[@CM_%]'
ORDER BY s.avg_total_user_cost * s.avg_user_impact * (s.user_seeks + s.user_scans) DESC;

PRINT '';
PRINT 'NOTE: Execute suggested index statements to improve performance';
PRINT 'NOTE: Test indexes on development environment first';
PRINT '';

-- ============================================================
-- 4. TABLE SIZE AND ROW COUNTS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '4. TABLE SIZE AND ROW COUNTS';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT
    t.name AS 'Table Name',
    p.rows AS 'Row Count',
    CAST((SUM(a.total_pages) * 8) / 1024.0 AS DECIMAL(10,2)) AS 'Total Space (MB)',
    CAST((SUM(a.used_pages) * 8) / 1024.0 AS DECIMAL(10,2)) AS 'Used Space (MB)',
    CAST((SUM(a.total_pages) * 8 - SUM(a.used_pages) * 8) / 1024.0 AS DECIMAL(10,2)) AS 'Unused Space (MB)'
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name LIKE '%CM_%' OR t.name LIKE '[@CM_%]'
GROUP BY t.name, p.rows
ORDER BY SUM(a.total_pages) DESC;

PRINT '';

-- ============================================================
-- 5. WAIT STATISTICS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '5. TOP WAIT STATISTICS (All Database)';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT TOP 15
    wait_type AS 'Wait Type',
    waiting_tasks_count AS 'Wait Count',
    CAST(wait_time_ms / 1000.0 AS DECIMAL(12,2)) AS 'Wait Time (sec)',
    CAST((wait_time_ms - signal_wait_time_ms) / 1000.0 AS DECIMAL(12,2)) AS 'Resource Wait (sec)',
    CAST(signal_wait_time_ms / 1000.0 AS DECIMAL(12,2)) AS 'Signal Wait (sec)',
    CAST(wait_time_ms * 100.0 / SUM(wait_time_ms) OVER() AS DECIMAL(5,2)) AS 'Percent',
    CASE wait_type
        WHEN 'LCK_M_S' THEN 'Shared lock wait - May indicate blocking'
        WHEN 'LCK_M_U' THEN 'Update lock wait - May indicate blocking'
        WHEN 'LCK_M_X' THEN 'Exclusive lock wait - Indicates blocking'
        WHEN 'PAGEIOLATCH_SH' THEN 'Waiting for data page read - Disk I/O issue'
        WHEN 'PAGEIOLATCH_EX' THEN 'Waiting for data page write - Disk I/O issue'
        WHEN 'WRITELOG' THEN 'Waiting for transaction log write - Log disk slow'
        WHEN 'ASYNC_NETWORK_IO' THEN 'Waiting for client to fetch data - Network slow'
        WHEN 'CXPACKET' THEN 'Parallel query coordination - Consider query tuning'
        WHEN 'SOS_SCHEDULER_YIELD' THEN 'CPU pressure - Need more CPU'
        ELSE 'See SQL Server documentation'
    END AS 'Description'
FROM sys.dm_os_wait_stats
WHERE wait_type NOT IN (
    'CLR_SEMAPHORE', 'LAZYWRITER_SLEEP', 'RESOURCE_QUEUE', 'SLEEP_TASK',
    'SLEEP_SYSTEMTASK', 'SQLTRACE_BUFFER_FLUSH', 'WAITFOR', 'LOGMGR_QUEUE',
    'CHECKPOINT_QUEUE', 'REQUEST_FOR_DEADLOCK_SEARCH', 'XE_TIMER_EVENT',
    'BROKER_TO_FLUSH', 'BROKER_TASK_STOP', 'CLR_MANUAL_EVENT',
    'CLR_AUTO_EVENT', 'DISPATCHER_QUEUE_SEMAPHORE', 'FT_IFTS_SCHEDULER_IDLE_WAIT',
    'XE_DISPATCHER_WAIT', 'XE_DISPATCHER_JOIN', 'SQLTRACE_INCREMENTAL_FLUSH_SLEEP',
    'ONDEMAND_TASK_QUEUE', 'BROKER_EVENTHANDLER', 'SLEEP_BPOOL_FLUSH',
    'DIRTY_PAGE_POLL', 'HADR_FILESTREAM_IOMGR_IOCOMPLETION'
)
AND wait_time_ms > 0
ORDER BY wait_time_ms DESC;

PRINT '';
PRINT 'NOTE: High wait times indicate performance bottlenecks';
PRINT '';

-- ============================================================
-- 6. BLOCKING SESSIONS
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '6. BLOCKING SESSIONS (Current)';
PRINT '------------------------------------------------------------';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.dm_os_waiting_tasks WHERE blocking_session_id <> 0)
BEGIN
    SELECT
        t.blocking_session_id AS 'Blocking Session',
        t.session_id AS 'Blocked Session',
        t.wait_type AS 'Wait Type',
        CAST(t.wait_duration_ms / 1000.0 AS DECIMAL(10,2)) AS 'Wait Duration (sec)',
        t.wait_resource AS 'Resource',
        OBJECT_NAME(p.object_id) AS 'Table Name',
        s1.login_name AS 'Blocking User',
        s1.program_name AS 'Blocking Program',
        s2.login_name AS 'Blocked User',
        s2.program_name AS 'Blocked Program',
        qt.text AS 'Blocked Query'
    FROM sys.dm_os_waiting_tasks t
    LEFT JOIN sys.dm_exec_sessions s1 ON t.blocking_session_id = s1.session_id
    LEFT JOIN sys.dm_exec_sessions s2 ON t.session_id = s2.session_id
    LEFT JOIN sys.dm_exec_requests r ON t.session_id = r.session_id
    CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) qt
    LEFT JOIN sys.partitions p ON t.wait_resource LIKE '%' + CAST(p.hobt_id AS VARCHAR(50)) + '%'
    WHERE t.blocking_session_id <> 0
    ORDER BY t.wait_duration_ms DESC;

    PRINT '';
    PRINT 'WARNING: Blocking detected! Review blocking queries.';
END
ELSE
BEGIN
    PRINT 'No blocking detected.';
END

PRINT '';

-- ============================================================
-- 7. LONG-RUNNING QUERIES
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '7. LONG-RUNNING QUERIES (Currently Executing > 5 seconds)';
PRINT '------------------------------------------------------------';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.dm_exec_requests WHERE total_elapsed_time > 5000)
BEGIN
    SELECT
        r.session_id AS 'Session',
        r.status AS 'Status',
        r.command AS 'Command',
        CAST(r.total_elapsed_time / 1000.0 AS DECIMAL(10,2)) AS 'Elapsed (sec)',
        CAST(r.cpu_time / 1000.0 AS DECIMAL(10,2)) AS 'CPU (sec)',
        r.reads AS 'Reads',
        r.writes AS 'Writes',
        r.logical_reads AS 'Logical Reads',
        r.granted_query_memory * 8 / 1024 AS 'Memory (MB)',
        s.login_name AS 'User',
        s.program_name AS 'Program',
        DB_NAME(r.database_id) AS 'Database',
        qt.text AS 'Query'
    FROM sys.dm_exec_requests r
    INNER JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
    CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) qt
    WHERE r.total_elapsed_time > 5000
      AND r.session_id <> @@SPID
    ORDER BY r.total_elapsed_time DESC;

    PRINT '';
    PRINT 'WARNING: Long-running queries detected!';
END
ELSE
BEGIN
    PRINT 'No long-running queries detected.';
END

PRINT '';

-- ============================================================
-- 8. INDEX FRAGMENTATION
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '8. INDEX FRAGMENTATION (Contract Management Tables)';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT
    OBJECT_NAME(ips.object_id) AS 'Table Name',
    i.name AS 'Index Name',
    ips.index_type_desc AS 'Index Type',
    ips.page_count AS 'Pages',
    CAST(ips.avg_fragmentation_in_percent AS DECIMAL(5,2)) AS 'Fragmentation %',
    CASE
        WHEN ips.avg_fragmentation_in_percent < 10 THEN 'Good'
        WHEN ips.avg_fragmentation_in_percent < 30 THEN 'Consider Reorganize'
        ELSE 'Rebuild Recommended'
    END AS 'Action',
    'ALTER INDEX [' + i.name + '] ON [' + OBJECT_NAME(ips.object_id) + '] ' +
    CASE
        WHEN ips.avg_fragmentation_in_percent < 30 THEN 'REORGANIZE;'
        ELSE 'REBUILD WITH (ONLINE = OFF);'
    END AS 'Suggested Command'
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.index_id > 0
  AND ips.page_count > 100
  AND (OBJECT_NAME(ips.object_id) LIKE '%CM_%' OR OBJECT_NAME(ips.object_id) LIKE '[@CM_%]')
ORDER BY ips.avg_fragmentation_in_percent DESC;

PRINT '';
PRINT 'NOTE: Fragmentation > 30% should be rebuilt';
PRINT 'NOTE: Fragmentation 10-30% should be reorganized';
PRINT '';

-- ============================================================
-- 9. QUERY PLAN CACHE
-- ============================================================

PRINT '------------------------------------------------------------';
PRINT '9. QUERY PLAN CACHE USAGE (Contract Management)';
PRINT '------------------------------------------------------------';
PRINT '';

SELECT
    cacheobjtype AS 'Cache Type',
    objtype AS 'Object Type',
    COUNT(*) AS 'Plan Count',
    CAST(SUM(size_in_bytes) / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS 'Total Size (MB)',
    CAST(SUM(CASE WHEN usecounts = 1 THEN size_in_bytes ELSE 0 END) / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS 'Single Use (MB)',
    CAST(SUM(CASE WHEN usecounts = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS 'Single Use %'
FROM sys.dm_exec_cached_plans cp
CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) qt
WHERE qt.text LIKE '%@CM_%' OR qt.text LIKE '%CM_%'
GROUP BY cacheobjtype, objtype
ORDER BY SUM(size_in_bytes) DESC;

PRINT '';

-- ============================================================
-- 10. SUMMARY AND RECOMMENDATIONS
-- ============================================================

PRINT '============================================================';
PRINT 'SUMMARY AND RECOMMENDATIONS';
PRINT '============================================================';
PRINT '';

-- Calculate overall health score
DECLARE @SlowQueries INT, @MissingIndexes INT, @Blocking INT, @LongRunning INT, @HighFragmentation INT;
DECLARE @HealthScore INT = 100;

SELECT @SlowQueries = COUNT(*)
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
WHERE (qt.text LIKE '%@CM_%' OR qt.text LIKE '%CM_%')
  AND qs.total_elapsed_time / qs.execution_count > 1000000; -- > 1 second

SELECT @MissingIndexes = COUNT(*)
FROM sys.dm_db_missing_index_details d
WHERE OBJECT_NAME(d.object_id) LIKE '%CM_%';

SELECT @Blocking = COUNT(*)
FROM sys.dm_os_waiting_tasks
WHERE blocking_session_id <> 0;

SELECT @LongRunning = COUNT(*)
FROM sys.dm_exec_requests
WHERE total_elapsed_time > 5000;

SELECT @HighFragmentation = COUNT(*)
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
WHERE ips.avg_fragmentation_in_percent > 30
  AND ips.page_count > 100
  AND (OBJECT_NAME(ips.object_id) LIKE '%CM_%' OR OBJECT_NAME(ips.object_id) LIKE '[@CM_%]');

-- Deduct points for issues
SET @HealthScore = @HealthScore - (@SlowQueries * 10);
SET @HealthScore = @HealthScore - (@MissingIndexes * 5);
SET @HealthScore = @HealthScore - (@Blocking * 15);
SET @HealthScore = @HealthScore - (@LongRunning * 10);
SET @HealthScore = @HealthScore - (@HighFragmentation * 5);

IF @HealthScore < 0 SET @HealthScore = 0;

PRINT 'Database Health Score: ' + CAST(@HealthScore AS VARCHAR) + '/100';
PRINT '';

IF @HealthScore >= 90
    PRINT 'Overall Status: EXCELLENT ✅';
ELSE IF @HealthScore >= 70
    PRINT 'Overall Status: GOOD ✓';
ELSE IF @HealthScore >= 50
    PRINT 'Overall Status: FAIR ⚠';
ELSE
    PRINT 'Overall Status: POOR ❌ - Immediate attention required!';

PRINT '';
PRINT 'Issues Detected:';
PRINT '  Slow Queries (> 1 sec):        ' + CAST(@SlowQueries AS VARCHAR);
PRINT '  Missing Indexes:               ' + CAST(@MissingIndexes AS VARCHAR);
PRINT '  Blocking Sessions:             ' + CAST(@Blocking AS VARCHAR);
PRINT '  Long-Running Queries (> 5 sec): ' + CAST(@LongRunning AS VARCHAR);
PRINT '  High Fragmentation (> 30%):    ' + CAST(@HighFragmentation AS VARCHAR);
PRINT '';

PRINT 'Recommendations:';
IF @SlowQueries > 0
    PRINT '  1. Review and optimize slow queries (see section 1)';
IF @MissingIndexes > 0
    PRINT '  2. Create recommended indexes (see section 3)';
IF @Blocking > 0
    PRINT '  3. Resolve blocking sessions immediately (see section 6)';
IF @LongRunning > 0
    PRINT '  4. Kill or optimize long-running queries (see section 7)';
IF @HighFragmentation > 0
    PRINT '  5. Rebuild or reorganize fragmented indexes (see section 8)';
IF @HealthScore >= 90
    PRINT '  No critical issues detected. Continue monitoring.';

PRINT '';
PRINT '============================================================';
PRINT 'END OF REPORT';
PRINT 'Report Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '============================================================';

SET NOCOUNT OFF;
GO
