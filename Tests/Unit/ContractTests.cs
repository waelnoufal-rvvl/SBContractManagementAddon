using System;
using System.Collections.Generic;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Models;
using ContractManagementAddon.Tests.Utilities;

namespace ContractManagementAddon.Tests.Unit.Models
{
    /// <summary>
    /// Unit tests for Contract model
    /// Tests business logic, calculations, and validations
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    public class ContractTests
    {
        [SetUp]
        public void Setup()
        {
            TestDataGenerator.ResetCounters();
        }

        #region CalculateTotal Tests

        [Test]
        [Description("CalculateTotal should sum all line totals correctly")]
        public void CalculateTotal_WithValidLines_ReturnsCorrectSum()
        {
            // Arrange
            var contract = new Contract
            {
                Lines = new List<ContractLine>
                {
                    new ContractLine { LineTotal = 1000 },
                    new ContractLine { LineTotal = 2000 },
                    new ContractLine { LineTotal = 3500 }
                }
            };

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(6500,
                "because the sum of line totals is 1000 + 2000 + 3500 = 6500");
        }

        [Test]
        [Description("CalculateTotal should handle null lines collection")]
        public void CalculateTotal_WithNullLines_SetsToZero()
        {
            // Arrange
            var contract = new Contract
            {
                Lines = null,
                TotalValue = 100 // Set to non-zero initially
            };

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(0,
                "because null lines should result in zero total");
        }

        [Test]
        [Description("CalculateTotal should handle empty lines collection")]
        public void CalculateTotal_WithEmptyLines_SetsToZero()
        {
            // Arrange
            var contract = new Contract
            {
                Lines = new List<ContractLine>(),
                TotalValue = 100
            };

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(0,
                "because empty lines collection should result in zero total");
        }

        [Test]
        [Description("CalculateTotal should handle negative line totals")]
        public void CalculateTotal_WithNegativeLineTotals_CalculatesCorrectly()
        {
            // Arrange
            var contract = new Contract
            {
                Lines = new List<ContractLine>
                {
                    new ContractLine { LineTotal = 5000 },
                    new ContractLine { LineTotal = -1000 }, // Credit/deduction
                    new ContractLine { LineTotal = 3000 }
                }
            };

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(7000,
                "because 5000 - 1000 + 3000 = 7000");
        }

        [Test]
        [Description("CalculateTotal should handle very large numbers")]
        public void CalculateTotal_WithLargeNumbers_CalculatesCorrectly()
        {
            // Arrange
            var contract = new Contract
            {
                Lines = new List<ContractLine>
                {
                    new ContractLine { LineTotal = 999999999 },
                    new ContractLine { LineTotal = 999999999 }
                }
            };

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(1999999998,
                "because large numbers should be handled correctly");
        }

        #endregion

        #region Validate Tests

        [Test]
        [Description("Validate should pass for valid contract")]
        public void Validate_WithValidContract_ReturnsNoErrors()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().BeEmpty("because the contract has all required fields");
        }

        [Test]
        [Description("Validate should fail for contract without code")]
        public void Validate_WithoutCode_ReturnsError()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.Code = null;

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().Contain("Contract Code is required");
        }

        [Test]
        [Description("Validate should fail for contract without customer")]
        public void Validate_WithoutCustomer_ReturnsError()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.CustomerCode = "";

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().Contain("Customer Code is required");
        }

        [Test]
        [Description("Validate should fail for end date before start date")]
        public void Validate_WithEndDateBeforeStartDate_ReturnsError()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.StartDate = DateTime.Today;
            contract.EndDate = DateTime.Today.AddDays(-1);

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().Contain("End Date must be after Start Date");
        }

        [Test]
        [Description("Validate should fail for negative total value")]
        public void Validate_WithNegativeTotalValue_ReturnsError()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.TotalValue = -100;

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().Contain("Total Value cannot be negative");
        }

        [Test]
        [Description("Validate should fail for zero exchange rate")]
        public void Validate_WithZeroExchangeRate_ReturnsError()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract(currency: "EUR");
            contract.ExchangeRate = 0;

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().Contain(e => e.Contains("Exchange Rate"));
        }

        #endregion

        #region Edge Cases and Business Rules

        [Test]
        [Description("Contract should allow zero value for framework contracts")]
        public void Contract_WithZeroValue_IsValid()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.TotalValue = 0;
            contract.Lines = new List<ContractLine>();

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().Be(0);
        }

        [Test]
        [Description("Contract should handle same-day start and end date")]
        public void Contract_WithSameDayStartAndEnd_IsValid()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract();
            contract.StartDate = DateTime.Today;
            contract.EndDate = DateTime.Today;

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().BeEmpty("because same-day contracts should be allowed");
        }

        [Test]
        [Description("Contract with many lines should calculate correctly")]
        public void Contract_With100Lines_CalculatesCorrectly()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract(lineCount: 100);

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().BeGreaterThan(0);
            contract.Lines.Count.Should().Be(100);
        }

        [Test]
        [Description("Contract total should match sum of lines")]
        public void Contract_TotalValue_ShouldMatchSumOfLines()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract(lineCount: 10);

            // Act
            contract.CalculateTotal();
            double expectedTotal = 0;
            foreach (var line in contract.Lines)
            {
                expectedTotal += line.LineTotal;
            }

            // Assert
            contract.TotalValue.Should().Be(expectedTotal,
                "because total should equal sum of all line totals");
        }

        #endregion

        #region Performance Tests

        [Test]
        [Description("CalculateTotal should complete quickly for large contracts")]
        [Timeout(1000)] // Must complete in 1 second
        public void CalculateTotal_With1000Lines_CompletesQuickly()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract(lineCount: 1000);

            // Act
            contract.CalculateTotal();

            // Assert
            contract.TotalValue.Should().BeGreaterThan(0);
        }

        [Test]
        [Description("Validate should complete quickly for complex contract")]
        [Timeout(500)] // Must complete in 0.5 seconds
        public void Validate_ComplexContract_CompletesQuickly()
        {
            // Arrange
            var contract = TestDataGenerator.CreateTestContract(lineCount: 100);

            // Act
            var errors = contract.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        #endregion

        #region Test Data Generator Tests

        [Test]
        [Description("TestDataGenerator should create valid contract")]
        public void TestDataGenerator_CreateTestContract_ReturnsValidContract()
        {
            // Act
            var contract = TestDataGenerator.CreateTestContract();

            // Assert
            contract.Should().NotBeNull();
            contract.Code.Should().NotBeNullOrEmpty();
            contract.CustomerCode.Should().NotBeNullOrEmpty();
            contract.TotalValue.Should().BeGreaterThan(0);
            contract.Lines.Should().NotBeEmpty();
            contract.Validate().Should().BeEmpty();
        }

        [Test]
        [Description("TestDataGenerator should create multiple unique contracts")]
        public void TestDataGenerator_CreateMultipleContracts_ReturnsUniqueContracts()
        {
            // Act
            var contracts = TestDataGenerator.CreateTestContracts(10);

            // Assert
            contracts.Should().HaveCount(10);
            contracts.Should().OnlyHaveUniqueItems(c => c.Code,
                "because each contract should have a unique code");
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            // Cleanup after each test
            TestDataGenerator.ResetCounters();
        }
    }
}
