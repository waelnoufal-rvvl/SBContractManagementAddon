using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Tests for date, number, and currency formatting
    /// </summary>
    [TestFixture]
    [Category("Localization")]
    [Category("Formatting")]
    public class FormattingTests
    {
        private LanguageManager _languageManager;

        [SetUp]
        public void Setup()
        {
            _languageManager = LanguageManager.Instance;
        }

        #region Date Formatting Tests

        [Test]
        [Description("FormatDate should format short date correctly in English")]
        public void FormatDate_ShortEnglish_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var date = new DateTime(2025, 1, 15);

            // Act
            var result = _languageManager.FormatDate(date, "short");

            // Assert
            result.Should().Be("1/15/2025");
        }

        [Test]
        [Description("FormatDate should format short date correctly in Arabic")]
        public void FormatDate_ShortArabic_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var date = new DateTime(2025, 1, 15);

            // Act
            var result = _languageManager.FormatDate(date, "short");

            // Assert
            // Arabic uses different number glyphs
            result.Should().NotBeNullOrEmpty();
            result.Should().Match(r => r.Contains("2025") || r.Contains("٢٠٢٥"));
        }

        [Test]
        [Description("FormatDate should format long date correctly in English")]
        public void FormatDate_LongEnglish_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var date = new DateTime(2025, 1, 15);

            // Act
            var result = _languageManager.FormatDate(date, "long");

            // Assert
            result.Should().Contain("Wednesday");
            result.Should().Contain("January");
            result.Should().Contain("15");
            result.Should().Contain("2025");
        }

        [Test]
        [Description("FormatDate should format long date correctly in Arabic")]
        public void FormatDate_LongArabic_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var date = new DateTime(2025, 1, 15);

            // Act
            var result = _languageManager.FormatDate(date, "long");

            // Assert
            result.Should().NotBeNullOrEmpty();
            // Should contain Arabic day/month names
            result.Length.Should().BeGreaterThan(10);
        }

        [Test]
        [Description("FormatDate should format datetime correctly")]
        public void FormatDate_DateTime_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var date = new DateTime(2025, 1, 15, 15, 30, 0);

            // Act
            var result = _languageManager.FormatDate(date, "datetime");

            // Assert
            result.Should().Contain("1/15/2025");
            result.Should().Contain("3:30");
            result.Should().Contain("PM");
        }

        [Test]
        [Description("FormatDate should handle custom format")]
        public void FormatDate_CustomFormat_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var date = new DateTime(2025, 1, 15);

            // Act
            var result = _languageManager.FormatDate(date, "yyyy-MM-dd");

            // Assert
            result.Should().Be("2025-01-15");
        }

        [Test]
        [Description("FormatDate should work for different years")]
        [TestCase(2024, 12, 31)]
        [TestCase(2025, 1, 1)]
        [TestCase(2025, 6, 15)]
        [TestCase(2025, 12, 31)]
        public void FormatDate_DifferentDates_FormatsCorrectly(int year, int month, int day)
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var date = new DateTime(year, month, day);

            // Act
            var result = _languageManager.FormatDate(date, "short");

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain(year.ToString());
        }

        #endregion

        #region Number Formatting Tests

        [Test]
        [Description("FormatNumber should format with correct decimal separator in English")]
        public void FormatNumber_English_UsesCorrectSeparators()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var number = 1234567.89;

            // Act
            var result = _languageManager.FormatNumber(number, 2);

            // Assert
            result.Should().Be("1,234,567.89");
        }

        [Test]
        [Description("FormatNumber should format with correct separators in Arabic")]
        public void FormatNumber_Arabic_UsesCorrectSeparators()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var number = 1234567.89;

            // Act
            var result = _languageManager.FormatNumber(number, 2);

            // Assert
            result.Should().NotBeNullOrEmpty();
            // Arabic may use different number glyphs and separators
            result.Length.Should().BeGreaterThan(5);
        }

        [Test]
        [Description("FormatNumber should respect decimal places")]
        [TestCase(1234.5678, 0, "1,235")]
        [TestCase(1234.5678, 1, "1,234.6")]
        [TestCase(1234.5678, 2, "1,234.57")]
        [TestCase(1234.5678, 4, "1,234.5678")]
        public void FormatNumber_DecimalPlaces_FormatsCorrectly(double number, int decimals, string expected)
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.FormatNumber(number, decimals);

            // Assert
            result.Should().Be(expected);
        }

        [Test]
        [Description("FormatNumber should handle negative numbers")]
        public void FormatNumber_NegativeNumber_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var number = -1234.56;

            // Act
            var result = _languageManager.FormatNumber(number, 2);

            // Assert
            result.Should().Contain("-");
            result.Should().Contain("1,234.56");
        }

        [Test]
        [Description("FormatNumber should handle zero")]
        public void FormatNumber_Zero_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.FormatNumber(0, 2);

            // Assert
            result.Should().Be("0.00");
        }

        [Test]
        [Description("FormatNumber should handle very large numbers")]
        public void FormatNumber_LargeNumber_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var number = 999999999999.99;

            // Act
            var result = _languageManager.FormatNumber(number, 2);

            // Assert
            result.Should().Contain(",");
            result.Should().Contain("999,999,999,999.99");
        }

        [Test]
        [Description("FormatNumber should handle very small numbers")]
        public void FormatNumber_SmallNumber_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var number = 0.000001;

            // Act
            var result = _languageManager.FormatNumber(number, 6);

            // Assert
            result.Should().Be("0.000001");
        }

        #endregion

        #region Currency Formatting Tests

        [Test]
        [Description("FormatCurrency should format USD correctly in English")]
        public void FormatCurrency_USD_English_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var amount = 100000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "USD");

            // Assert
            result.Should().Contain("$");
            result.Should().Contain("100,000.00");
            result.Should().StartWith("$");
        }

        [Test]
        [Description("FormatCurrency should format USD correctly in Arabic")]
        public void FormatCurrency_USD_Arabic_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var amount = 100000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "USD");

            // Assert
            result.Should().Contain("$");
            // In Arabic, symbol should be after number
            result.Should().EndWith("$");
        }

        [Test]
        [Description("FormatCurrency should format SAR correctly")]
        public void FormatCurrency_SAR_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var amount = 100000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "SAR");

            // Assert
            result.Should().Contain("ر.س");
            result.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("FormatCurrency should format EUR correctly")]
        public void FormatCurrency_EUR_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var amount = 100000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "EUR");

            // Assert
            result.Should().Contain("€");
            result.Should().Contain("100,000.00");
        }

        [Test]
        [Description("FormatCurrency should format GBP correctly")]
        public void FormatCurrency_GBP_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var amount = 100000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "GBP");

            // Assert
            result.Should().Contain("£");
            result.Should().Contain("100,000.00");
        }

        [Test]
        [Description("FormatCurrency should handle negative amounts")]
        public void FormatCurrency_NegativeAmount_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var amount = -5000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, "USD");

            // Assert
            result.Should().Contain("-");
            result.Should().Contain("$");
            result.Should().Contain("5,000.00");
        }

        [Test]
        [Description("FormatCurrency should handle zero")]
        public void FormatCurrency_Zero_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.FormatCurrency(0, "USD");

            // Assert
            result.Should().Contain("$");
            result.Should().Contain("0.00");
        }

        [Test]
        [Description("FormatCurrency without currency code should use system currency")]
        public void FormatCurrency_NoCurrencyCode_UsesSystemCurrency()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var amount = 1000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("1,000.00");
        }

        [Test]
        [Description("FormatCurrency should handle different Middle East currencies")]
        [TestCase("SAR", "ر.س")]
        [TestCase("AED", "د.إ")]
        [TestCase("EGP", "ج.م")]
        [TestCase("KWD", "د.ك")]
        [TestCase("QAR", "ر.ق")]
        public void FormatCurrency_MiddleEastCurrencies_FormatsCorrectly(string currencyCode, string symbol)
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var amount = 1000.00;

            // Act
            var result = _languageManager.FormatCurrency(amount, currencyCode);

            // Assert
            result.Should().Contain(symbol);
        }

        #endregion

        #region Culture Consistency Tests

        [Test]
        [Description("Culture should remain consistent after multiple operations")]
        public void Culture_RemainsConsistentAfterMultipleOperations()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var date = new DateTime(2025, 1, 15);
            var number = 1234.56;

            // Act
            var dateResult = _languageManager.FormatDate(date, "short");
            var numberResult = _languageManager.FormatNumber(number, 2);
            var currencyResult = _languageManager.FormatCurrency(100, "SAR");

            // Assert
            _languageManager.CurrentCulture.Name.Should().Be("ar-SA");
            dateResult.Should().NotBeNullOrEmpty();
            numberResult.Should().NotBeNullOrEmpty();
            currencyResult.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("Formatting should adapt when language changes")]
        public void Formatting_AdaptsToLanguageChange()
        {
            // Arrange
            var number = 1234.56;

            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var englishResult = _languageManager.FormatNumber(number, 2);

            // Switch to Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var arabicResult = _languageManager.FormatNumber(number, 2);

            // Assert
            englishResult.Should().NotBe(arabicResult);
            englishResult.Should().Be("1,234.56");
        }

        #endregion

        #region Edge Cases

        [Test]
        [Description("FormatDate should handle minimum date")]
        public void FormatDate_MinimumDate_DoesNotThrow()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            Action act = () => _languageManager.FormatDate(DateTime.MinValue, "short");

            // Assert
            act.Should().NotThrow();
        }

        [Test]
        [Description("FormatDate should handle maximum date")]
        public void FormatDate_MaximumDate_DoesNotThrow()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            Action act = () => _languageManager.FormatDate(DateTime.MaxValue, "short");

            // Assert
            act.Should().NotThrow();
        }

        [Test]
        [Description("FormatNumber should handle maximum double")]
        public void FormatNumber_MaximumDouble_DoesNotThrow()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            Action act = () => _languageManager.FormatNumber(double.MaxValue, 2);

            // Assert
            act.Should().NotThrow();
        }

        [Test]
        [Description("FormatNumber should handle minimum double")]
        public void FormatNumber_MinimumDouble_DoesNotThrow()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            Action act = () => _languageManager.FormatNumber(double.MinValue, 2);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            // Reset to English
            _languageManager.LoadLanguage(SupportedLanguage.English);
        }
    }
}
