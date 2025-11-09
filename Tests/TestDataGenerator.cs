using System;
using System.Collections.Generic;
using ContractManagementAddon.Models;

namespace ContractManagementAddon.Tests.Utilities
{
    /// <summary>
    /// Test data generator for Contract Management Add-on
    /// Creates realistic test data for unit tests, integration tests, and load tests
    /// </summary>
    public static class TestDataGenerator
    {
        private static Random _random = new Random();
        private static int _contractCounter = 1;
        private static int _ipcCounter = 1;
        private static int _coCounter = 1;

        #region Contract Generation

        /// <summary>
        /// Creates a single test contract with default values
        /// </summary>
        public static Contract CreateTestContract(
            string currency = "USD",
            int lineCount = 5,
            double totalValue = 100000)
        {
            var contractCode = $"TEST-CONTRACT-{_contractCounter:D6}";
            _contractCounter++;

            var contract = new Contract
            {
                Code = contractCode,
                CustomerCode = GetRandomCustomer(),
                CustomerName = GetRandomCustomerName(),
                Description = $"Test Contract {contractCode}",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6),
                Status = "Active",
                Currency = currency,
                BaseCurrency = "USD",
                ExchangeRate = currency == "USD" ? 1.0 : GetRandomExchangeRate(),
                TotalValue = totalValue,
                TotalIPCAmount = 0,
                TotalPaid = 0,
                RetentionPercentage = 10,
                PaymentTerms = "Net 30",
                CreatedDate = DateTime.Now,
                CreatedBy = "TestUser",
                Lines = new List<ContractLine>()
            };

            // Generate lines
            double valuePerLine = totalValue / lineCount;
            for (int i = 1; i <= lineCount; i++)
            {
                contract.Lines.Add(new ContractLine
                {
                    LineNum = i,
                    ItemCode = $"ITEM-{i:D3}",
                    ItemDescription = $"Test Item {i}",
                    Quantity = 10,
                    UnitPrice = valuePerLine / 10,
                    LineTotal = valuePerLine
                });
            }

            return contract;
        }

        /// <summary>
        /// Creates multiple test contracts
        /// </summary>
        public static List<Contract> CreateTestContracts(int count)
        {
            var contracts = new List<Contract>();
            var currencies = new[] { "USD", "EUR", "GBP", "JPY", "CAD" };

            for (int i = 0; i < count; i++)
            {
                var currency = currencies[i % currencies.Length];
                var totalValue = _random.Next(10000, 1000000);
                var lineCount = _random.Next(3, 15);

                contracts.Add(CreateTestContract(currency, lineCount, totalValue));
            }

            return contracts;
        }

        /// <summary>
        /// Creates a contract with specific characteristics for testing
        /// </summary>
        public static Contract CreateTestContractWithCharacteristics(
            bool hasNullLines = false,
            bool hasZeroValue = false,
            bool hasInvalidDates = false,
            bool hasInvalidCurrency = false)
        {
            var contract = CreateTestContract();

            if (hasNullLines)
                contract.Lines = null;

            if (hasZeroValue)
            {
                contract.TotalValue = 0;
                foreach (var line in contract.Lines ?? new List<ContractLine>())
                {
                    line.LineTotal = 0;
                }
            }

            if (hasInvalidDates)
            {
                contract.StartDate = DateTime.Today.AddYears(100);
                contract.EndDate = DateTime.Today.AddYears(-1);
            }

            if (hasInvalidCurrency)
            {
                contract.Currency = "INVALID";
            }

            return contract;
        }

        #endregion

        #region IPC Generation

        /// <summary>
        /// Creates a test IPC for a contract
        /// </summary>
        public static IPC CreateTestIPC(string contractCode = null, double amount = 20000)
        {
            var ipcCode = $"TEST-IPC-{_ipcCounter:D6}";
            _ipcCounter++;

            if (contractCode == null)
            {
                contractCode = $"TEST-CONTRACT-{_random.Next(1, 1000):D6}";
            }

            var ipc = new IPC
            {
                Code = ipcCode,
                DocNum = _ipcCounter.ToString(),
                ContractCode = contractCode,
                IPCDate = DateTime.Today,
                IPCNumber = _ipcCounter,
                Remarks = $"Test IPC {ipcCode}",
                GrossAmount = amount,
                RetentionAmount = amount * 0.1,
                NetAmount = amount * 0.9,
                PreviousIPCTotal = 0,
                CurrentAmount = amount,
                Status = "Draft",
                Currency = "USD",
                BaseCurrency = "USD",
                ExchangeRate = 1.0,
                CreatedDate = DateTime.Now,
                CreatedBy = "TestUser",
                Lines = new List<IPCLine>()
            };

            // Generate IPC lines
            int lineCount = _random.Next(2, 6);
            double amountPerLine = amount / lineCount;

            for (int i = 1; i <= lineCount; i++)
            {
                ipc.Lines.Add(new IPCLine
                {
                    LineNum = i,
                    Description = $"IPC Line {i}",
                    Amount = amountPerLine,
                    CompletionPercentage = _random.Next(10, 90)
                });
            }

            return ipc;
        }

        /// <summary>
        /// Creates multiple IPCs for a contract
        /// </summary>
        public static List<IPC> CreateTestIPCs(string contractCode, int count)
        {
            var ipcs = new List<IPC>();
            double totalContractValue = 100000;
            double amountPerIPC = totalContractValue / count;

            for (int i = 0; i < count; i++)
            {
                var ipc = CreateTestIPC(contractCode, amountPerIPC);
                ipc.IPCNumber = i + 1;
                ipc.PreviousIPCTotal = amountPerIPC * i;
                ipcs.Add(ipc);
            }

            return ipcs;
        }

        #endregion

        #region Change Order Generation

        /// <summary>
        /// Creates a test change order
        /// </summary>
        public static ChangeOrder CreateTestChangeOrder(
            string contractCode = null,
            string type = "Addition",
            double amount = 10000)
        {
            var coCode = $"TEST-CO-{_coCounter:D6}";
            _coCounter++;

            if (contractCode == null)
            {
                contractCode = $"TEST-CONTRACT-{_random.Next(1, 1000):D6}";
            }

            return new ChangeOrder
            {
                Code = coCode,
                DocNum = _coCounter.ToString(),
                ContractCode = contractCode,
                ChangeOrderNumber = _coCounter,
                Type = type, // Addition or Deduction
                Amount = amount,
                Description = $"Test Change Order {coCode}",
                Justification = "Test reason for change order",
                ChangeOrderDate = DateTime.Today,
                Status = "Draft",
                CreatedDate = DateTime.Now,
                CreatedBy = "TestUser"
            };
        }

        #endregion

        #region Performance Obligation Generation

        /// <summary>
        /// Creates test performance obligations for a contract
        /// </summary>
        public static List<PerformanceObligation> CreatePerformanceObligations(
            string contractCode,
            int count = 3)
        {
            var obligations = new List<PerformanceObligation>();
            var progressMethods = new[] { "Cost-to-Cost", "Units", "Milestone", "Time-Based" };

            for (int i = 1; i <= count; i++)
            {
                obligations.Add(new PerformanceObligation
                {
                    Code = $"PO-{contractCode}-{i:D2}",
                    ContractCode = contractCode,
                    Description = $"Performance Obligation {i}",
                    AllocatedPrice = 30000 + (_random.Next(-5000, 5000)),
                    ProgressMethod = progressMethods[i % progressMethods.Length],
                    CompletionPercentage = 0,
                    Status = "Active",
                    TotalEstimatedCost = 25000,
                    CreateDate = DateTime.Now,
                    Lines = new List<PerformanceObligationLine>()
                });
            }

            return obligations;
        }

        #endregion

        #region Currency and Exchange Rate Generation

        /// <summary>
        /// Creates test currency
        /// </summary>
        public static Currency CreateTestCurrency(string code = "EUR")
        {
            return new Currency
            {
                Code = code,
                Name = GetCurrencyName(code),
                Symbol = GetCurrencySymbol(code),
                DecimalPlaces = 2,
                IsActive = true,
                CreateDate = DateTime.Now
            };
        }

        /// <summary>
        /// Creates test exchange rate
        /// </summary>
        public static ExchangeRate CreateTestExchangeRate(
            string fromCurrency = "USD",
            string toCurrency = "EUR",
            double rate = 0.85)
        {
            return new ExchangeRate
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate,
                RateDate = DateTime.Today,
                RateSource = "Test",
                CreateDate = DateTime.Now
            };
        }

        #endregion

        #region Revenue Recognition Generation

        /// <summary>
        /// Creates test revenue schedule
        /// </summary>
        public static RevenueSchedule CreateTestRevenueSchedule(
            string contractCode,
            string poCode)
        {
            return new RevenueSchedule
            {
                Code = $"RS-{contractCode}-{_random.Next(1000, 9999)}",
                ContractCode = contractCode,
                PerformanceObligationCode = poCode,
                PeriodStartDate = DateTime.Today.AddDays(-30),
                PeriodEndDate = DateTime.Today,
                ScheduledRevenue = 10000,
                RecognizedRevenue = 8000,
                CumulativeRevenue = 8000,
                RecognitionBasis = "Cost-to-Cost",
                ProgressPercentage = 80,
                Status = "Recognized",
                CreateDate = DateTime.Now
            };
        }

        /// <summary>
        /// Creates test deferred revenue
        /// </summary>
        public static DeferredRevenue CreateTestDeferredRevenue(
            string contractCode,
            string poCode)
        {
            return new DeferredRevenue
            {
                Code = $"DR-{contractCode}-{_random.Next(1000, 9999)}",
                ContractCode = contractCode,
                PerformanceObligationCode = poCode,
                IPCCode = $"TEST-IPC-{_random.Next(1, 1000):D6}",
                BilledAmount = 10000,
                RecognizedRevenue = 6000,
                DeferredAmount = 4000,
                DeferralReason = "Revenue not yet earned",
                ReleaseSchedule = "As performance obligations are satisfied",
                Status = "Active",
                CreateDate = DateTime.Now
            };
        }

        #endregion

        #region Complete Workflow Generation

        /// <summary>
        /// Creates complete test workflow (Contract + IPCs + POs + Revenue)
        /// </summary>
        public static CompleteWorkflowData CreateCompleteWorkflow()
        {
            var contract = CreateTestContract();
            var ipcs = CreateTestIPCs(contract.Code, 3);
            var changeOrder = CreateTestChangeOrder(contract.Code);
            var performanceObligations = CreatePerformanceObligations(contract.Code, 3);
            var revenueSchedules = new List<RevenueSchedule>();
            var deferredRevenues = new List<DeferredRevenue>();

            foreach (var po in performanceObligations)
            {
                revenueSchedules.Add(CreateTestRevenueSchedule(contract.Code, po.Code));
                deferredRevenues.Add(CreateTestDeferredRevenue(contract.Code, po.Code));
            }

            return new CompleteWorkflowData
            {
                Contract = contract,
                IPCs = ipcs,
                ChangeOrder = changeOrder,
                PerformanceObligations = performanceObligations,
                RevenueSchedules = revenueSchedules,
                DeferredRevenues = deferredRevenues
            };
        }

        #endregion

        #region Performance Test Data Generation

        /// <summary>
        /// Creates large dataset for performance testing
        /// </summary>
        public static PerformanceTestData CreatePerformanceTestData(
            int contracts = 1000,
            int ipcsPerContract = 5,
            int linesPerContract = 10)
        {
            var data = new PerformanceTestData
            {
                Contracts = new List<Contract>(),
                IPCs = new List<IPC>(),
                TotalRecords = contracts * (1 + ipcsPerContract)
            };

            for (int i = 0; i < contracts; i++)
            {
                var contract = CreateTestContract(
                    lineCount: linesPerContract,
                    totalValue: _random.Next(50000, 500000)
                );
                data.Contracts.Add(contract);

                // Create IPCs for this contract
                var ipcs = CreateTestIPCs(contract.Code, ipcsPerContract);
                data.IPCs.AddRange(ipcs);
            }

            return data;
        }

        #endregion

        #region Load Test Data Generation

        /// <summary>
        /// Creates data for load testing scenarios
        /// </summary>
        public static LoadTestData CreateLoadTestData(
            int users = 50,
            int contractsPerUser = 10)
        {
            var data = new LoadTestData
            {
                Users = users,
                ContractsPerUser = contractsPerUser,
                TotalContracts = users * contractsPerUser,
                UserData = new List<UserTestData>()
            };

            for (int userId = 1; userId <= users; userId++)
            {
                var userData = new UserTestData
                {
                    UserId = userId,
                    UserName = $"TestUser{userId:D4}",
                    Contracts = CreateTestContracts(contractsPerUser)
                };
                data.UserData.Add(userData);
            }

            return data;
        }

        #endregion

        #region Helper Methods

        private static string GetRandomCustomer()
        {
            var customers = new[] { "C00001", "C00002", "C00003", "C00004", "C00005" };
            return customers[_random.Next(customers.Length)];
        }

        private static string GetRandomCustomerName()
        {
            var names = new[]
            {
                "Acme Corporation",
                "Global Industries Inc.",
                "Tech Solutions Ltd.",
                "Manufacturing Co.",
                "Service Partners LLC"
            };
            return names[_random.Next(names.Length)];
        }

        private static double GetRandomExchangeRate()
        {
            return 0.5 + (_random.NextDouble() * 1.5); // Between 0.5 and 2.0
        }

        private static string GetCurrencyName(string code)
        {
            var names = new Dictionary<string, string>
            {
                { "USD", "US Dollar" },
                { "EUR", "Euro" },
                { "GBP", "British Pound" },
                { "JPY", "Japanese Yen" },
                { "CAD", "Canadian Dollar" }
            };
            return names.ContainsKey(code) ? names[code] : code;
        }

        private static string GetCurrencySymbol(string code)
        {
            var symbols = new Dictionary<string, string>
            {
                { "USD", "$" },
                { "EUR", "€" },
                { "GBP", "£" },
                { "JPY", "¥" },
                { "CAD", "C$" }
            };
            return symbols.ContainsKey(code) ? symbols[code] : code;
        }

        /// <summary>
        /// Resets counters (useful for test cleanup)
        /// </summary>
        public static void ResetCounters()
        {
            _contractCounter = 1;
            _ipcCounter = 1;
            _coCounter = 1;
        }

        #endregion
    }

    #region Test Data Classes

    /// <summary>
    /// Complete workflow test data
    /// </summary>
    public class CompleteWorkflowData
    {
        public Contract Contract { get; set; }
        public List<IPC> IPCs { get; set; }
        public ChangeOrder ChangeOrder { get; set; }
        public List<PerformanceObligation> PerformanceObligations { get; set; }
        public List<RevenueSchedule> RevenueSchedules { get; set; }
        public List<DeferredRevenue> DeferredRevenues { get; set; }
    }

    /// <summary>
    /// Performance test data
    /// </summary>
    public class PerformanceTestData
    {
        public List<Contract> Contracts { get; set; }
        public List<IPC> IPCs { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Load test data
    /// </summary>
    public class LoadTestData
    {
        public int Users { get; set; }
        public int ContractsPerUser { get; set; }
        public int TotalContracts { get; set; }
        public List<UserTestData> UserData { get; set; }
    }

    /// <summary>
    /// User-specific test data for load testing
    /// </summary>
    public class UserTestData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<Contract> Contracts { get; set; }
    }

    #endregion
}
