using System;
using System.Data.Odbc;
using ContractManagementAddon.Core;

namespace ContractManagementAddon.Tests
{
    /// <summary>
    /// Simple console test runner for testing ODBC connection
    /// without requiring SAP Business One to be running
    ///
    /// HOW TO USE:
    /// This file is NOT active by default. The project is a Class Library (DLL).
    /// To use this test runner:
    ///
    /// 1. Temporarily change project to Console App:
    ///    - Right-click project → Properties → Application
    ///    - Change "Output type" from "Class Library" to "Console Application"
    ///    - Save changes
    ///
    /// 2. Press F5 to run and test ODBC connection
    ///
    /// 3. Change back to Class Library when done:
    ///    - Right-click project → Properties → Application
    ///    - Change "Output type" back to "Class Library"
    ///    - Save changes
    ///
    /// NOTE: Do NOT set this as StartupObject in project properties!
    /// </summary>
    class ConsoleTestRunner
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Contract Management Add-on - Console Test");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                // First, list all installed ODBC drivers
                ListInstalledOdbcDrivers();
                Console.WriteLine();

                TestOdbcConnection();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void ListInstalledOdbcDrivers()
        {
            Console.WriteLine("Listing installed ODBC drivers...");
            Console.WriteLine();

            try
            {
                // Check registry for ODBC drivers
                // 32-bit drivers on 64-bit Windows
                var drivers = new System.Collections.Generic.List<string>();

                // Try 32-bit registry location (since our app is x86)
                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
                    {
                        if (key != null)
                        {
                            Console.WriteLine("32-bit ODBC Drivers:");
                            foreach (var driverName in key.GetValueNames())
                            {
                                var value = key.GetValue(driverName);
                                if (value != null && value.ToString() == "Installed")
                                {
                                    drivers.Add(driverName);

                                    // Highlight SAP HANA drivers
                                    if (driverName.ToUpper().Contains("HANA") || driverName.ToUpper().Contains("HDB"))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"  ✓ {driverName} [SAP HANA]");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        Console.WriteLine($"    {driverName}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error reading 32-bit drivers: {ex.Message}");
                }

                if (drivers.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ No ODBC drivers found in 32-bit registry");
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("NOTE: This application is compiled as x86 (32-bit).");
                Console.WriteLine("You need the 32-bit SAP HANA ODBC driver installed.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error listing drivers: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void TestOdbcConnection()
        {
            Console.WriteLine("Testing ODBC Connection to SAP HANA...");
            Console.WriteLine();
            Console.WriteLine("Reading configuration from App.config...");

            try
            {
                // Read ODBC connection settings directly from App.config
                string server = System.Configuration.ConfigurationManager.AppSettings["HANAServer"];
                string port = System.Configuration.ConfigurationManager.AppSettings["HANAPort"];
                string database = System.Configuration.ConfigurationManager.AppSettings["HANADatabase"];
                string username = System.Configuration.ConfigurationManager.AppSettings["HANAUsername"];
                string password = System.Configuration.ConfigurationManager.AppSettings["HANAPassword"];

                Console.WriteLine($"  Server: {server}");
                Console.WriteLine($"  Port: {port}");
                Console.WriteLine($"  Database: {database}");
                Console.WriteLine($"  Username: {username}");
                Console.WriteLine($"  Password: {new string('*', password?.Length ?? 0)}");
                Console.WriteLine();

                // Common SAP HANA ODBC driver names to try
                string[] driverNames = new string[]
                {
                    "HDBODBC",           // Standard name
                    "HDBODBC32",         // 32-bit version
                    "HDBODBC64",         // 64-bit version (might not work with x86 app)
                    "HDBODBC.DLL",       // With extension
                    "SAP HANA ODBC",     // Full name
                    "SAP HANA",          // Short name
                    System.Configuration.ConfigurationManager.AppSettings["ODBCDriver"] ?? "HDBODBC"
                };

                bool connectionSuccessful = false;
                string workingDriver = null;

                foreach (string driverName in driverNames)
                {
                    try
                    {
                        Console.WriteLine($"Trying driver: {driverName}");

                        // Build connection string
                        string connectionString = $"Driver={{{driverName}}};ServerNode={server}:{port};Database={database};UID={username};PWD={password}";

                        Console.WriteLine($"Connection String: Driver={{{driverName}}};ServerNode={server}:{port};Database={database};UID={username};PWD=***");
                        Console.WriteLine();

                        // Create and open ODBC connection
                        using (OdbcConnection odbcConn = new OdbcConnection(connectionString))
                        {
                            odbcConn.Open();

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"✓ ODBC Connection established successfully with driver: {driverName}!");
                            Console.ResetColor();
                            Console.WriteLine($"  Data Source: {odbcConn.DataSource}");
                            Console.WriteLine($"  Database: {odbcConn.Database}");
                            Console.WriteLine($"  Driver: {driverName}");
                            Console.WriteLine($"  State: {odbcConn.State}");
                            Console.WriteLine($"  Server Version: {odbcConn.ServerVersion}");
                            Console.WriteLine();

                            // Test a simple query
                            Console.WriteLine("Testing simple query: SELECT 1 FROM DUMMY");
                            using (OdbcCommand cmd = new OdbcCommand("SELECT 1 FROM DUMMY", odbcConn))
                            {
                                object result = cmd.ExecuteScalar();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"✓ Query executed successfully! Result: {result}");
                                Console.ResetColor();
                            }

                            Console.WriteLine();
                            Console.WriteLine("Closing connection...");

                            connectionSuccessful = true;
                            workingDriver = driverName;
                        }

                        if (connectionSuccessful)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"✓ Connection closed successfully.");
                            Console.WriteLine();
                            Console.WriteLine($"SUCCESS! Working driver name: {workingDriver}");
                            Console.WriteLine($"Update your App.config with:");
                            Console.WriteLine($"  <add key=\"ODBCDriver\" value=\"{workingDriver}\" />");
                            Console.ResetColor();
                            break; // Exit the loop on success
                        }
                    }
                    catch (OdbcException odbcEx)
                    {
                        // Only show "driver not found" errors briefly, other errors in detail
                        if (odbcEx.Errors[0].SQLState == "IM002")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"  ✗ Driver '{driverName}' not found, trying next...");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        else
                        {
                            // This is a different error (connection failed, auth failed, etc.)
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"✗ Connection failed with driver '{driverName}'!");
                            Console.ResetColor();
                            Console.WriteLine($"  ODBC Error: {odbcEx.Message}");
                            Console.WriteLine($"  SQLSTATE: {odbcEx.Errors[0].SQLState}");
                            Console.WriteLine($"  Native Error: {odbcEx.Errors[0].NativeError}");
                            Console.WriteLine();

                            // Don't try other drivers if we got a non-driver-related error
                            if (odbcEx.Errors[0].SQLState != "IM002")
                            {
                                throw;
                            }
                        }
                    }
                }

                if (!connectionSuccessful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✗ Failed to connect with any known SAP HANA ODBC driver!");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("Please install the SAP HANA Client (32-bit) from:");
                    Console.WriteLine("  https://tools.hana.ondemand.com/#hanatools");
                    Console.WriteLine();
                    Console.WriteLine("Or run the ODBC Data Source Administrator (32-bit):");
                    Console.WriteLine("  C:\\Windows\\SysWOW64\\odbcad32.exe");
                    Console.WriteLine("And check which SAP HANA driver is installed.");
                }
            }
            catch (OdbcException odbcEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ ODBC Connection failed!");
                Console.ResetColor();
                Console.WriteLine($"  ODBC Error: {odbcEx.Message}");
                Console.WriteLine($"  SQLSTATE: {odbcEx.Errors[0].SQLState}");
                Console.WriteLine($"  Native Error: {odbcEx.Errors[0].NativeError}");
                Console.WriteLine();
                Console.WriteLine("Possible causes:");
                Console.WriteLine("  1. HDBODBC driver not installed");
                Console.WriteLine("  2. Incorrect server address or port");
                Console.WriteLine("  3. Invalid credentials");
                Console.WriteLine("  4. Network connectivity issues");
                Console.WriteLine("  5. Firewall blocking connection");
                Console.WriteLine();
                Console.WriteLine("To install HDBODBC driver:");
                Console.WriteLine("  Download SAP HANA Client from SAP Software Download Center");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ Connection test failed!");
                Console.ResetColor();
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine($"  Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner Error: {ex.InnerException.Message}");
                }
                Console.WriteLine();
                Console.WriteLine("Make sure the following are configured in App.config:");
                Console.WriteLine("  - HANAServer");
                Console.WriteLine("  - HANAPort");
                Console.WriteLine("  - HANADatabase");
                Console.WriteLine("  - HANAUsername");
                Console.WriteLine("  - HANAPassword");
            }
        }
    }
}
