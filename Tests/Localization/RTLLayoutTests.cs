using System;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Localization;
using SAPbouiCOM;
using Moq;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Tests for Right-to-Left (RTL) layout functionality
    /// </summary>
    [TestFixture]
    [Category("Localization")]
    [Category("RTL")]
    public class RTLLayoutTests
    {
        private LanguageManager _languageManager;

        [SetUp]
        public void Setup()
        {
            _languageManager = LanguageManager.Instance;
        }

        #region RTL Detection Tests

        [Test]
        [Description("IsRightToLeft should be false for English")]
        public void IsRightToLeft_English_ReturnsFalse()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Assert
            _languageManager.IsRightToLeft.Should().BeFalse();
        }

        [Test]
        [Description("IsRightToLeft should be true for Arabic")]
        public void IsRightToLeft_Arabic_ReturnsTrue()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            _languageManager.IsRightToLeft.Should().BeTrue();
        }

        [Test]
        [Description("IsRightToLeft should update when language changes")]
        public void IsRightToLeft_SwitchLanguage_UpdatesCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var wasLTR = !_languageManager.IsRightToLeft;

            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var nowRTL = _languageManager.IsRightToLeft;

            // Assert
            wasLTR.Should().BeTrue("English should be LTR");
            nowRTL.Should().BeTrue("Arabic should be RTL");
        }

        #endregion

        #region Language Properties Tests

        [Test]
        [Description("CurrentLanguage should reflect loaded language")]
        public void CurrentLanguage_English_IsSet()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.English);
        }

        [Test]
        [Description("CurrentLanguage should reflect loaded language for Arabic")]
        public void CurrentLanguage_Arabic_IsSet()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.Arabic);
        }

        [Test]
        [Description("CurrentCulture should be set correctly for English")]
        public void CurrentCulture_English_IsEnUS()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Assert
            _languageManager.CurrentCulture.Name.Should().Be("en-US");
        }

        [Test]
        [Description("CurrentCulture should be set correctly for Arabic")]
        public void CurrentCulture_Arabic_IsArSA()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            _languageManager.CurrentCulture.Name.Should().Be("ar-SA");
        }

        #endregion

        #region Formatting Tests

        [Test]
        [Description("FormatDate should work with English culture")]
        public void FormatDate_English_IsFormatted()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var testDate = new DateTime(2025, 11, 10);

            // Act
            var formatted = _languageManager.FormatDate(testDate, "short");

            // Assert
            formatted.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("FormatDate should work with Arabic culture")]
        public void FormatDate_Arabic_IsFormatted()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var testDate = new DateTime(2025, 11, 10);

            // Act
            var formatted = _languageManager.FormatDate(testDate, "short");

            // Assert
            formatted.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("FormatNumber should work with English culture")]
        public void FormatNumber_English_IsFormatted()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var testNumber = 1234.56;

            // Act
            var formatted = _languageManager.FormatNumber(testNumber, 2);

            // Assert
            formatted.Should().NotBeNullOrEmpty();
            formatted.Should().Contain("1234");
        }

        [Test]
        [Description("FormatNumber should work with Arabic culture")]
        public void FormatNumber_Arabic_IsFormatted()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var testNumber = 1234.56;

            // Act
            var formatted = _languageManager.FormatNumber(testNumber, 2);

            // Assert
            formatted.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("FormatCurrency should position currency symbol correctly for English (LTR)")]
        public void FormatCurrency_English_SymbolFirst()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var testAmount = 1000.00;

            // Act
            var formatted = _languageManager.FormatCurrency(testAmount, "USD");

            // Assert
            formatted.Should().NotBeNullOrEmpty();
            formatted.Should().StartWith("$"); // USD symbol should be first in English
        }

        [Test]
        [Description("FormatCurrency should position currency symbol correctly for Arabic (RTL)")]
        public void FormatCurrency_Arabic_SymbolLast()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var testAmount = 1000.00;

            // Act
            var formatted = _languageManager.FormatCurrency(testAmount, "SAR");

            // Assert
            formatted.Should().NotBeNullOrEmpty();
            // Arabic currency symbol should be last for RTL
            formatted.Should().EndWith("ر.س");
        }

        #endregion

        #region Available Languages Tests

        [Test]
        [Description("GetAvailableLanguages should return English and Arabic")]
        public void GetAvailableLanguages_ReturnsEnglishAndArabic()
        {
            // Act
            var languages = _languageManager.GetAvailableLanguages();

            // Assert
            languages.Should().NotBeEmpty();
            languages.Should().ContainKey(SupportedLanguage.English);
            languages.Should().ContainKey(SupportedLanguage.Arabic);
        }

        [Test]
        [Description("Available languages should have correct display names")]
        public void GetAvailableLanguages_DisplayNamesAreCorrect()
        {
            // Act
            var languages = _languageManager.GetAvailableLanguages();

            // Assert
            languages[SupportedLanguage.English].Should().Be("English");
            languages[SupportedLanguage.Arabic].Should().Be("العربية");
        }

        #endregion

        #region String Localization Tests

        [Test]
        [Description("GetString should return the localized string")]
        public void GetString_ValidKey_ReturnsLocalizedString()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.GetString("Section_GeneralInformation");

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Test]
        [Description("GetString with missing key should return bracketed key")]
        public void GetString_MissingKey_ReturnsBracketedKey()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var missingKey = "NonExistent_Key_12345";

            // Act
            var result = _languageManager.GetString(missingKey);

            // Assert
            result.Should().Be($"[{missingKey}]");
        }

        [Test]
        [Description("GetString with default value should return default for missing key")]
        public void GetString_MissingKey_ReturnsDefault()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var missingKey = "NonExistent_Key_12345";
            var defaultValue = "Default Text";

            // Act
            var result = _languageManager.GetString(missingKey, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        #endregion

        #region Language Switch Tests

        [Test]
        [Description("SwitchLanguage should cycle through available languages")]
        public void SwitchLanguage_CyclesLanguages()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var initialLanguage = _languageManager.CurrentLanguage;

            // Act
            _languageManager.SwitchLanguage();
            var afterSwitch = _languageManager.CurrentLanguage;

            // Assert
            afterSwitch.Should().NotBe(initialLanguage);
        }

        #endregion
    }
}
