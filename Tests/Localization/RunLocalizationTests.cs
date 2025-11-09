using System;
using System.IO;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Console application to run all localization tests
    /// Usage: Simply run this file to execute all tests and generate reports
    /// </summary>
    public class RunLocalizationTests
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                // Create test runner
                var runner = new LocalizationTestRunner();

                // Run all tests
                var report = runner.RunAllTests();

                // Save HTML report
                var reportPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "TestReports",
                    $"LocalizationTestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                );

                // Create directory if doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

                // Save report
                runner.SaveHTMLReport(report, reportPath);

                // Display final status
                Console.WriteLine();
                if (report.AllTestsPassed)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("SUCCESS: All localization tests passed!");
                    Console.WriteLine("The multi-language implementation is working correctly without bugs or crashes.");
                    Console.ResetColor();
                    Environment.Exit(0);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"FAILURE: {report.FailedTests} test(s) failed.");
                    Console.WriteLine("Please review the errors and fix the issues.");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("CRITICAL ERROR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Environment.Exit(2);
            }
        }
    }
}
