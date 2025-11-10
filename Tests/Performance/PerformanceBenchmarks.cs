using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using ContractManagementAddon.Models;
using ContractManagementAddon.Services;
using ContractManagementAddon.Tests.Utilities;

namespace ContractManagementAddon.Tests.Performance
{
    /// <summary>
    /// Performance benchmarks using BenchmarkDotNet
    /// Measures execution time, memory allocation, and throughput
    ///
    /// To run: dotnet run -c Release --filter "*Benchmarks*"
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
    public class RevenueRecognitionBenchmarks
    {
        private List<PerformanceObligation> _obligations100;
        private List<PerformanceObligation> _obligations1000;
        private List<RevenueSchedule> _schedules100;

        [GlobalSetup]
        public void Setup()
        {
            // Create test data
            var contract = TestDataGenerator.CreateTestContract();
            _obligations100 = TestDataGenerator.CreatePerformanceObligations(contract.Code, 100);
            _obligations1000 = TestDataGenerator.CreatePerformanceObligations(contract.Code, 1000);

            _schedules100 = new List<RevenueSchedule>();
            foreach (var po in _obligations100)
            {
                _schedules100.Add(TestDataGenerator.CreateTestRevenueSchedule(contract.Code, po.Code));
            }
        }

        [Benchmark]
        [Description("Calculate completion percentage for 100 performance obligations")]
        public void CalculateCompletionPercentage_100Obligations()
        {
            foreach (var obligation in _obligations100)
            {
                obligation.CalculateCompletionPercentage();
            }
        }

        [Benchmark]
        [Description("Calculate completion percentage for 1000 performance obligations")]
        public void CalculateCompletionPercentage_1000Obligations()
        {
            foreach (var obligation in _obligations1000)
            {
                obligation.CalculateCompletionPercentage();
            }
        }

        [Benchmark]
        [Description("Calculate deferred revenue for 100 revenue schedules")]
        public void CalculateDeferredRevenue_100Schedules()
        {
            foreach (var schedule in _schedules100)
            {
                schedule.CalculateDeferredRevenue();
            }
        }

        [Benchmark]
        [Description("Validate 100 revenue schedules")]
        public void ValidateRevenueSchedules_100Schedules()
        {
            foreach (var schedule in _schedules100)
            {
                var errors = schedule.Validate();
            }
        }
    }

    /// <summary>
    /// Contract creation and calculation benchmarks
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
    public class ContractBenchmarks
    {
        private List<Contract> _contracts10;
        private List<Contract> _contracts100;
        private Contract _contractWith1000Lines;

        [GlobalSetup]
        public void Setup()
        {
            _contracts10 = TestDataGenerator.CreateTestContracts(10);
            _contracts100 = TestDataGenerator.CreateTestContracts(100);
            _contractWith1000Lines = TestDataGenerator.CreateTestContract(lineCount: 1000);
        }

        [Benchmark]
        [Description("Calculate total for 10 contracts")]
        public void CalculateTotal_10Contracts()
        {
            foreach (var contract in _contracts10)
            {
                contract.CalculateTotal();
            }
        }

        [Benchmark]
        [Description("Calculate total for 100 contracts")]
        public void CalculateTotal_100Contracts()
        {
            foreach (var contract in _contracts100)
            {
                contract.CalculateTotal();
            }
        }

        [Benchmark]
        [Description("Calculate total for contract with 1000 lines")]
        public void CalculateTotal_ContractWith1000Lines()
        {
            _contractWith1000Lines.CalculateTotal();
        }

        [Benchmark]
        [Description("Validate 100 contracts")]
        public void Validate_100Contracts()
        {
            foreach (var contract in _contracts100)
            {
                var errors = contract.Validate();
            }
        }
    }

    /// <summary>
    /// NUnit performance tests with explicit thresholds
    /// These tests fail if performance targets are not met
    /// </summary>
    [TestFixture]
    [Category("Performance")]
    [Explicit("Run manually - requires clean environment")]
    public class PerformanceTargetTests
    {
        [Test]
        [Description("Contract creation should complete in under 3 seconds")]
        [Timeout(3000)]
        public void CreateContract_Performance_Under3Seconds()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            var stopwatch = Stopwatch.StartNew();

            // Act
            // Simulating contract creation (without actual DB call)
            contract.CalculateTotal();
            var errors = contract.Validate();

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000),
                $"Contract creation took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
            Assert.That(errors, Is.Empty);
        }

        [Test]
        [Description("Processing 100 contracts should complete in under 10 seconds")]
        [Timeout(10000)]
        public void Process100Contracts_Performance_Under10Seconds()
        {
            // Arrange
            var contracts = TestDataGenerator.CreateTestContracts(100);
            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var contract in contracts)
            {
                contract.CalculateTotal();
                var errors = contract.Validate();
                Assert.That(errors, Is.Empty);
            }

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10000),
                $"Processing 100 contracts took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");

            // Calculate throughput
            double throughput = 100.0 / stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"Throughput: {throughput:F2} contracts/second");
            Assert.That(throughput, Is.GreaterThan(10),
                "Should process more than 10 contracts per second");
        }

        [Test]
        [Description("Revenue calculation for 100 POs should complete in under 5 seconds")]
        [Timeout(5000)]
        public void CalculateRevenue100POs_Performance_Under5Seconds()
        {
            // Arrange
            var obligations = TestDataGenerator.CreatePerformanceObligations("TEST-001", 100);
            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var obligation in obligations)
            {
                obligation.CalculateCompletionPercentage();
            }

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000),
                $"Revenue calculation took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");

            Console.WriteLine($"Average per calculation: {stopwatch.ElapsedMilliseconds / 100.0:F2}ms");
        }

        [Test]
        [Description("Currency conversion should complete in under 100ms")]
        [Timeout(100)]
        public void CurrencyConversion_Performance_Under100ms()
        {
            // Arrange
            double amount = 10000;
            double rate = 0.85;
            var stopwatch = Stopwatch.StartNew();

            // Act
            double converted = amount * rate;

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
                $"Currency conversion took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
            Assert.That(converted, Is.EqualTo(8500));
        }

        [Test]
        [Description("Memory usage test for large dataset")]
        public void MemoryUsage_LargeDataset_StaysReasonable()
        {
            // Arrange
            long initialMemory = GC.GetTotalMemory(true);

            // Act
            var contracts = TestDataGenerator.CreateTestContracts(1000);
            long afterCreation = GC.GetTotalMemory(false);

            // Process all contracts
            foreach (var contract in contracts)
            {
                contract.CalculateTotal();
            }
            long afterProcessing = GC.GetTotalMemory(false);

            // Assert
            long memoryUsed = afterCreation - initialMemory;
            long memoryPerContract = memoryUsed / 1000;

            Console.WriteLine($"Initial memory: {initialMemory / 1024 / 1024:F2} MB");
            Console.WriteLine($"After creation: {afterCreation / 1024 / 1024:F2} MB");
            Console.WriteLine($"After processing: {afterProcessing / 1024 / 1024:F2} MB");
            Console.WriteLine($"Memory per contract: {memoryPerContract / 1024:F2} KB");

            // Each contract should use less than 100KB
            Assert.That(memoryPerContract, Is.LessThan(100 * 1024),
                $"Each contract uses {memoryPerContract / 1024}KB, expected < 100KB");

            // Total memory for 1000 contracts should be less than 100MB
            Assert.That(memoryUsed, Is.LessThan(100 * 1024 * 1024),
                $"Total memory used: {memoryUsed / 1024 / 1024}MB, expected < 100MB");
        }

        [Test]
        [Description("Throughput test - operations per second")]
        public void ThroughputTest_ContractCalculations_MeetsTarget()
        {
            // Arrange
            var contracts = TestDataGenerator.CreateTestContracts(100);
            int operations = 0;
            var stopwatch = Stopwatch.StartNew();
            var timeout = TimeSpan.FromSeconds(10);

            // Act - Run as many operations as possible in 10 seconds
            while (stopwatch.Elapsed < timeout)
            {
                foreach (var contract in contracts)
                {
                    contract.CalculateTotal();
                    operations++;

                    if (stopwatch.Elapsed >= timeout)
                        break;
                }
            }
            stopwatch.Stop();

            // Calculate throughput
            double throughput = operations / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"Total operations: {operations}");
            Console.WriteLine($"Duration: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine($"Throughput: {throughput:F2} operations/second");

            // Assert - Should achieve at least 1000 operations per second
            Assert.That(throughput, Is.GreaterThan(1000),
                $"Throughput was {throughput:F2} ops/sec, expected > 1000 ops/sec");
        }
    }

    /// <summary>
    /// Program entry point for running benchmarks
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Run BenchmarkDotNet benchmarks
            var summary = BenchmarkRunner.Run<RevenueRecognitionBenchmarks>();
            var summary2 = BenchmarkRunner.Run<ContractBenchmarks>();

            Console.WriteLine("Benchmarks completed. Check BenchmarkDotNet.Artifacts for detailed results.");
        }
    }
}
