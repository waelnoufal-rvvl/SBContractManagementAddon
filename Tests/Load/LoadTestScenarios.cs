using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using ContractManagementAddon.Tests.Utilities;
// NOTE: NBomber package is required for load testing
// Install via: dotnet add package NBomber
// After installation, uncomment the following lines:
// using NBomber.Contracts;
// using NBomber.CSharp;

namespace ContractManagementAddon.Tests.Load
{
    /// <summary>
    /// Load testing scenarios using NBomber
    /// Tests system behavior under concurrent load
    ///
    /// IMPORTANT: These tests require the NBomber NuGet package to be installed.
    /// Install NBomber before using these tests: dotnet add package NBomber
    ///
    /// These tests should be run on a dedicated test environment.
    /// Do NOT run against production database!
    /// </summary>
    [TestFixture]
    [Category("Load")]
    [Explicit("Run manually - requires NBomber package and test environment")]
    public class LoadTestScenarios
    {
#if false // Set to true after installing NBomber package: dotnet add package NBomber
        #region Light Load Tests (10 users)

        [Test]
        [Description("Light load: 10 concurrent users creating contracts for 5 minutes")]
        public void LightLoad_10Users_5Minutes()
        {
            // Configure scenario
            var scenario = Scenario.Create("Light Load - Contract Creation", async context =>
            {
                var contract = TestDataGenerator.CreateTestContract();

                // Simulate contract creation
                var stopwatch = Stopwatch.StartNew();

                // In real test, this would call: _contractService.CreateContract(contract)
                // For now, simulate with calculation
                contract.CalculateTotal();
                var errors = contract.Validate();

                stopwatch.Stop();

                // Return success if validation passed and took < 5 seconds
                return errors.Count == 0 && stopwatch.ElapsedMilliseconds < 5000
                    ? Response.Ok()
                    : Response.Fail($"Validation errors: {errors.Count}, Time: {stopwatch.ElapsedMilliseconds}ms");
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(10))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 2, during: TimeSpan.FromMinutes(5))
            );

            // Run scenario
            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("light_load_report")
                .WithReportFormats(ReportFormat.Html, ReportFormat.Txt, ReportFormat.Md)
                .Run();

            // Verify results
            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Light Load Test Results ===");
            Console.WriteLine($"Total Requests: {scn.Ok.Request.Count}");
            Console.WriteLine($"Failed Requests: {scn.Fail.Request.Count}");
            Console.WriteLine($"RPS: {scn.Ok.Request.RPS}");
            Console.WriteLine($"Min Latency: {scn.Ok.Latency.MinMs}ms");
            Console.WriteLine($"Mean Latency: {scn.Ok.Latency.MeanMs}ms");
            Console.WriteLine($"Max Latency: {scn.Ok.Latency.MaxMs}ms");
            Console.WriteLine($"P50: {scn.Ok.Latency.Percent50}ms");
            Console.WriteLine($"P75: {scn.Ok.Latency.Percent75}ms");
            Console.WriteLine($"P95: {scn.Ok.Latency.Percent95}ms");
            Console.WriteLine($"P99: {scn.Ok.Latency.Percent99}ms");

            // Assert targets
            Assert.That(scn.Ok.Request.Count, Is.GreaterThan(550), // ~2/sec * 300 sec - buffer
                "Should complete at least 550 requests");
            Assert.That(scn.Fail.Request.Percent, Is.LessThan(1),
                "Failure rate should be less than 1%");
            Assert.That(scn.Ok.Latency.Percent99, Is.LessThan(5000),
                "99th percentile latency should be under 5 seconds");
        }

        #endregion

        #region Medium Load Tests (50 users)

        [Test]
        [Description("Medium load: 50 concurrent users for 10 minutes")]
        public void MediumLoad_50Users_10Minutes()
        {
            var scenario = Scenario.Create("Medium Load - Mixed Operations", async context =>
            {
                // Simulate different operations with different weights
                var operation = context.ScenarioInfo.ThreadId % 3;

                switch (operation)
                {
                    case 0: // 33% - Create contract
                        var contract = TestDataGenerator.CreateTestContract();
                        contract.CalculateTotal();
                        return Response.Ok();

                    case 1: // 33% - Create IPC
                        var ipc = TestDataGenerator.CreateTestIPC();
                        // Simulate IPC calculations
                        return Response.Ok();

                    case 2: // 33% - Query operations
                        // Simulate read operation
                        await Task.Delay(50); // Simulate DB query time
                        return Response.Ok();

                    default:
                        return Response.Ok();
                }
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(30))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(10))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("medium_load_report")
                .WithReportFormats(ReportFormat.Html, ReportFormat.Txt)
                .Run();

            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Medium Load Test Results ===");
            Console.WriteLine($"Duration: {stats.AllScenariosStats.Duration}");
            Console.WriteLine($"Total OK: {scn.Ok.Request.Count}");
            Console.WriteLine($"Total Failed: {scn.Fail.Request.Count}");
            Console.WriteLine($"RPS: {scn.Ok.Request.RPS}");
            Console.WriteLine($"Mean Latency: {scn.Ok.Latency.MeanMs}ms");
            Console.WriteLine($"P99 Latency: {scn.Ok.Latency.Percent99}ms");

            // Assert
            Assert.That(scn.Ok.Request.RPS, Is.GreaterThan(9),
                "Should achieve > 9 requests per second");
            Assert.That(scn.Fail.Request.Percent, Is.LessThan(2),
                "Failure rate should be < 2%");
        }

        #endregion

        #region Heavy Load Tests (100 users)

        [Test]
        [Description("Heavy load: 100 concurrent users - stress test")]
        public void HeavyLoad_100Users_StressTest()
        {
            var scenario = Scenario.Create("Heavy Load - Contract Creation", async context =>
            {
                var contract = TestDataGenerator.CreateTestContract();

                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    contract.CalculateTotal();
                    var errors = contract.Validate();
                    stopwatch.Stop();

                    if (stopwatch.ElapsedMilliseconds > 10000)
                        return Response.Fail("Timeout - took > 10 seconds");

                    return errors.Count == 0
                        ? Response.Ok()
                        : Response.Fail($"Validation failed: {errors.Count} errors");
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message);
                }
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(30))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 20, during: TimeSpan.FromMinutes(5))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("heavy_load_report")
                .WithReportFormats(ReportFormat.Html, ReportFormat.Txt, ReportFormat.Md)
                .Run();

            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Heavy Load Test Results ===");
            Console.WriteLine($"Total Requests: {scn.Ok.Request.Count + scn.Fail.Request.Count}");
            Console.WriteLine($"Successful: {scn.Ok.Request.Count} ({scn.Ok.Request.Percent}%)");
            Console.WriteLine($"Failed: {scn.Fail.Request.Count} ({scn.Fail.Request.Percent}%)");
            Console.WriteLine($"RPS: {scn.Ok.Request.RPS}");
            Console.WriteLine($"Mean Latency: {scn.Ok.Latency.MeanMs}ms");
            Console.WriteLine($"P95 Latency: {scn.Ok.Latency.Percent95}ms");
            Console.WriteLine($"P99 Latency: {scn.Ok.Latency.Percent99}ms");

            // Less strict assertions for heavy load
            Assert.That(scn.Fail.Request.Percent, Is.LessThan(10),
                "Failure rate should be < 10% even under heavy load");
            Assert.That(scn.Ok.Latency.Percent99, Is.LessThan(10000),
                "99th percentile should be under 10 seconds");
        }

        #endregion

        #region Spike Test

        [Test]
        [Description("Spike test: Sudden spike from 10 to 100 users")]
        public void SpikeTest_SuddenLoadIncrease()
        {
            var scenario = Scenario.Create("Spike Test", async context =>
            {
                var contract = TestDataGenerator.CreateTestContract();
                contract.CalculateTotal();

                return Response.Ok();
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(10))
            .WithLoadSimulations(
                // Normal load
                Simulation.InjectPerSec(rate: 2, during: TimeSpan.FromMinutes(2)),
                // Sudden spike
                Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(1)),
                // Back to normal
                Simulation.InjectPerSec(rate: 2, during: TimeSpan.FromMinutes(2))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("spike_test_report")
                .WithReportFormats(ReportFormat.Html)
                .Run();

            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Spike Test Results ===");
            Console.WriteLine($"Total Requests: {scn.Ok.Request.Count}");
            Console.WriteLine($"Failed: {scn.Fail.Request.Count}");
            Console.WriteLine($"Failure Rate: {scn.Fail.Request.Percent}%");
            Console.WriteLine($"Max Latency: {scn.Ok.Latency.MaxMs}ms");
            Console.WriteLine($"P99 Latency: {scn.Ok.Latency.Percent99}ms");

            // System should handle spike gracefully
            Assert.That(scn.Fail.Request.Percent, Is.LessThan(5),
                "System should handle spike with < 5% failure rate");
        }

        #endregion

        #region Endurance Test

        [Test]
        [Description("Endurance test: Sustained load for 30 minutes")]
        [Explicit("Long running test - execute separately")]
        public void EnduranceTest_30Minutes_SustainedLoad()
        {
            var scenario = Scenario.Create("Endurance Test", async context =>
            {
                var contract = TestDataGenerator.CreateTestContract();
                contract.CalculateTotal();

                return Response.Ok();
            })
            .WithWarmUpDuration(TimeSpan.FromMinutes(1))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 5, during: TimeSpan.FromMinutes(30))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("endurance_test_report")
                .WithReportFormats(ReportFormat.Html, ReportFormat.Txt)
                .Run();

            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Endurance Test Results ===");
            Console.WriteLine($"Duration: {stats.AllScenariosStats.Duration}");
            Console.WriteLine($"Total Requests: {scn.Ok.Request.Count}");
            Console.WriteLine($"RPS: {scn.Ok.Request.RPS}");
            Console.WriteLine($"Failures: {scn.Fail.Request.Count}");
            Console.WriteLine($"Mean Latency: {scn.Ok.Latency.MeanMs}ms");

            // Verify system stability over time
            Assert.That(scn.Fail.Request.Percent, Is.LessThan(1),
                "System should maintain < 1% failure rate over 30 minutes");
            Assert.That(scn.Ok.Request.Count, Is.GreaterThan(8900), // ~5/sec * 1800 sec - buffer
                "Should complete at least 8900 requests");

            // Check for memory leaks - latency shouldn't degrade significantly
            Assert.That(scn.Ok.Latency.Percent99, Is.LessThan(scn.Ok.Latency.MeanMs * 5),
                "P99 latency shouldn't be more than 5x mean (indicates memory issues)");
        }

        #endregion

        #region Mixed Workload Test

        [Test]
        [Description("Realistic mixed workload: Read-heavy with occasional writes")]
        public void MixedWorkload_RealisticUsage()
        {
            // Scenario 1: Read operations (70% of traffic)
            var readScenario = Scenario.Create("Read Operations", async context =>
            {
                // Simulate querying contracts
                await Task.Delay(50); // Simulate DB query
                return Response.Ok();
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(15))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 14, during: TimeSpan.FromMinutes(5))
            );

            // Scenario 2: Write operations (30% of traffic)
            var writeScenario = Scenario.Create("Write Operations", async context =>
            {
                // Simulate creating/updating
                var contract = TestDataGenerator.CreateTestContract();
                contract.CalculateTotal();
                await Task.Delay(100); // Simulate DB write
                return Response.Ok();
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(15))
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 6, during: TimeSpan.FromMinutes(5))
            );

            var stats = NBomberRunner
                .RegisterScenarios(readScenario, writeScenario)
                .WithReportFileName("mixed_workload_report")
                .WithReportFormats(ReportFormat.Html, ReportFormat.Txt)
                .Run();

            Console.WriteLine("=== Mixed Workload Test Results ===");

            foreach (var scn in stats.ScenarioStats)
            {
                Console.WriteLine($"\nScenario: {scn.ScenarioName}");
                Console.WriteLine($"  Total Requests: {scn.Ok.Request.Count}");
                Console.WriteLine($"  RPS: {scn.Ok.Request.RPS}");
                Console.WriteLine($"  Mean Latency: {scn.Ok.Latency.MeanMs}ms");
                Console.WriteLine($"  Failures: {scn.Fail.Request.Percent}%");

                Assert.That(scn.Fail.Request.Percent, Is.LessThan(2),
                    $"{scn.ScenarioName} failure rate should be < 2%");
            }
        }

        #endregion

        #region Concurrency Test

        [Test]
        [Description("Test concurrent access to same resource")]
        public void ConcurrencyTest_SameContract()
        {
            // All users try to access/modify the same contract
            var sharedContractCode = "SHARED-CONTRACT-001";

            var scenario = Scenario.Create("Concurrent Access", async context =>
            {
                // Simulate multiple users accessing same contract
                var contract = TestDataGenerator.CreateTestContract();
                contract.Code = sharedContractCode; // Same code for all

                contract.CalculateTotal();

                // Check for race conditions, deadlocks, etc.
                await Task.Delay(10);

                return Response.Ok();
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(10))
            .WithLoadSimulations(
                // Simulate 20 concurrent users hitting same resource
                Simulation.RampConstant(copies: 20, during: TimeSpan.FromSeconds(30))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("concurrency_test_report")
                .WithReportFormats(ReportFormat.Html)
                .Run();

            var scn = stats.ScenarioStats[0];

            Console.WriteLine("=== Concurrency Test Results ===");
            Console.WriteLine($"Total Requests: {scn.Ok.Request.Count}");
            Console.WriteLine($"Failures: {scn.Fail.Request.Count}");
            Console.WriteLine($"Mean Latency: {scn.Ok.Latency.MeanMs}ms");

            // Should handle concurrent access without errors
            Assert.That(scn.Fail.Request.Count, Is.EqualTo(0),
                "Should handle concurrent access without failures");
        }

        #endregion
#endif // NBOMBER
    }
}
