using System;
using System.IO;
using System.Configuration;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Logging utility for the add-on
    /// </summary>
    public static class Logger
    {
        private static string _logPath;
        private static bool _enableDebug;
        private static readonly object _lockObject = new object();

        static Logger()
        {
            try
            {
                _logPath = ConfigurationManager.AppSettings["LogPath"] ?? @"C:\Logs\ContractManagement\";

                // Safe parsing of debug mode setting - prevents crash if config has invalid value
                string debugConfig = ConfigurationManager.AppSettings["EnableDebugMode"];
                _enableDebug = string.IsNullOrEmpty(debugConfig) ? true :
                               (bool.TryParse(debugConfig, out bool debugResult) ? debugResult : true);

                // Create log directory if it doesn't exist
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch (Exception ex)
            {
                // Fallback to temp directory if configuration fails
                _logPath = Path.Combine(Path.GetTempPath(), "ContractManagement");
                Directory.CreateDirectory(_logPath);
                _enableDebug = true; // Default to debug enabled on initialization failure

                // Use Console.WriteLine instead of WriteLog to avoid potential recursion in static constructor
                Console.WriteLine($"[LOGGER] Failed to initialize logger from config: {ex.Message}. Using fallback path: {_logPath}");
            }
        }

        /// <summary>
        /// Log informational message
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            WriteLog("ERROR", fullMessage);
        }

        /// <summary>
        /// Log debug message (only if debug mode enabled)
        /// </summary>
        public static void Debug(string message)
        {
            if (_enableDebug)
            {
                WriteLog("DEBUG", message);
            }
        }

        /// <summary>
        /// Write log entry to file
        /// </summary>
        private static void WriteLog(string level, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logEntry = $"[{timestamp}] [{level}] {message}";

                    // Log file name with date
                    string logFileName = $"ContractManagement_{DateTime.Now:yyyyMMdd}.log";
                    string logFilePath = Path.Combine(_logPath, logFileName);

                    // Write to file
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

                    // Also write to console for debugging
                    Console.WriteLine(logEntry);
                }
                catch (Exception ex)
                {
                    // If logging fails, write to console only
                    Console.WriteLine($"[LOGGER ERROR] Failed to write log: {ex.Message}");
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                }
            }
        }

        /// <summary>
        /// Get current log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            string logFileName = $"ContractManagement_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(_logPath, logFileName);
        }

        /// <summary>
        /// Clear old log files (older than specified days)
        /// </summary>
        public static void ClearOldLogs(int daysToKeep = 30)
        {
            try
            {
                DirectoryInfo logDir = new DirectoryInfo(_logPath);
                FileInfo[] logFiles = logDir.GetFiles("ContractManagement_*.log");

                foreach (FileInfo file in logFiles)
                {
                    if (file.LastWriteTime < DateTime.Now.AddDays(-daysToKeep))
                    {
                        file.Delete();
                        Info($"Deleted old log file: {file.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error("Failed to clear old logs", ex);
            }
        }
    }
}
