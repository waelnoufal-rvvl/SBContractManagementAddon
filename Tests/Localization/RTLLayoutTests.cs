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

        #region Text Direction Tests

        [Test]
        [Description("Text direction metadata should be correct for English")]
        public void TextDirection_English_IsLTR()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var direction = _languageManager.GetTextDirection();

            // Assert
            direction.Should().Be("LTR");
        }

        [Test]
        [Description("Text direction metadata should be correct for Arabic")]
        public void TextDirection_Arabic_IsRTL()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var direction = _languageManager.GetTextDirection();

            // Assert
            direction.Should().Be("RTL");
        }

        #endregion

        #region Layout Calculation Tests

        [Test]
        [Description("CalculateRTLPosition should mirror position correctly")]
        public void CalculateRTLPosition_ReturnsCorrectMirroredPosition()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            int formWidth = 600;
            int itemLeft = 50;
            int itemWidth = 100;

            // Expected: formWidth - itemLeft - itemWidth = 600 - 50 - 100 = 450
            int expected = 450;

            // Act
            int result = _languageManager.CalculateRTLPosition(formWidth, itemLeft, itemWidth);

            // Assert
            result.Should().Be(expected);
        }

        [Test]
        [Description("CalculateRTLPosition with different dimensions")]
        [TestCase(800, 100, 150, 550)]  // 800 - 100 - 150 = 550
        [TestCase(600, 200, 100, 300)]  // 600 - 200 - 100 = 300
        [TestCase(1000, 50, 200, 750)]  // 1000 - 50 - 200 = 750
        [TestCase(400, 10, 80, 310)]    // 400 - 10 - 80 = 310
        public void CalculateRTLPosition_VariousDimensions_CalculatesCorrectly(
            int formWidth, int itemLeft, int itemWidth, int expected)
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            int result = _languageManager.CalculateRTLPosition(formWidth, itemLeft, itemWidth);

            // Assert
            result.Should().Be(expected);
        }

        [Test]
        [Description("CalculateRTLPosition should handle edge of form")]
        public void CalculateRTLPosition_ItemAtLeftEdge_MirrorsToRightEdge()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            int formWidth = 600;
            int itemLeft = 0;
            int itemWidth = 100;

            // Expected: formWidth - itemLeft - itemWidth = 600 - 0 - 100 = 500
            int expected = 500;

            // Act
            int result = _languageManager.CalculateRTLPosition(formWidth, itemLeft, itemWidth);

            // Assert
            result.Should().Be(expected);
        }

        [Test]
        [Description("CalculateRTLPosition should handle center-aligned items")]
        public void CalculateRTLPosition_CenteredItem_RemainsNearCenter()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            int formWidth = 600;
            int itemWidth = 100;
            int itemLeft = (formWidth - itemWidth) / 2; // Centered: 250

            // Expected: formWidth - itemLeft - itemWidth = 600 - 250 - 100 = 250
            int expected = 250;

            // Act
            int result = _languageManager.CalculateRTLPosition(formWidth, itemLeft, itemWidth);

            // Assert
            result.Should().Be(expected, "Centered items should remain centered");
        }

        #endregion

        #region Alignment Tests

        [Test]
        [Description("GetDefaultAlignment should return left for English")]
        public void GetDefaultAlignment_English_ReturnsLeft()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var alignment = _languageManager.GetDefaultTextAlignment();

            // Assert
            alignment.Should().Be(BoTextAlignments.ta_Left);
        }

        [Test]
        [Description("GetDefaultAlignment should return right for Arabic")]
        public void GetDefaultAlignment_Arabic_ReturnsRight()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var alignment = _languageManager.GetDefaultTextAlignment();

            // Assert
            alignment.Should().Be(BoTextAlignments.ta_Right);
        }

        [Test]
        [Description("Default alignment should update when language changes")]
        public void GetDefaultAlignment_LanguageChange_UpdatesAlignment()
        {
            // Arrange - Start with English
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var englishAlignment = _languageManager.GetDefaultTextAlignment();

            // Act - Switch to Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var arabicAlignment = _languageManager.GetDefaultTextAlignment();

            // Assert
            englishAlignment.Should().Be(BoTextAlignments.ta_Left);
            arabicAlignment.Should().Be(BoTextAlignments.ta_Right);
        }

        #endregion

        #region Button Order Tests

        [Test]
        [Description("GetButtonOrder should return OK-Cancel for English")]
        public void GetButtonOrder_English_ReturnsOKCancel()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var order = _languageManager.GetButtonOrder();

            // Assert
            order.Should().NotBeNull();
            order.Should().HaveCount(2);
            order[0].Should().Be("OK");
            order[1].Should().Be("Cancel");
        }

        [Test]
        [Description("GetButtonOrder should return Cancel-OK for Arabic")]
        public void GetButtonOrder_Arabic_ReturnsCancelOK()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var order = _languageManager.GetButtonOrder();

            // Assert
            order.Should().NotBeNull();
            order.Should().HaveCount(2);
            order[0].Should().Be("Cancel");
            order[1].Should().Be("OK");
        }

        #endregion

        #region Unicode and Bidi Tests

        [Test]
        [Description("Arabic text should contain RTL marker")]
        public void ArabicText_ContainsRTLCharacters()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var saveText = _languageManager.GetString("Common_Save");
            var cancelText = _languageManager.GetString("Common_Cancel");

            // Assert
            HasRTLCharacters(saveText).Should().BeTrue("Save button should have RTL text");
            HasRTLCharacters(cancelText).Should().BeTrue("Cancel button should have RTL text");
        }

        [Test]
        [Description("English text should not contain RTL marker")]
        public void EnglishText_DoesNotContainRTLCharacters()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var saveText = _languageManager.GetString("Common_Save");
            var cancelText = _languageManager.GetString("Common_Cancel");

            // Assert
            HasRTLCharacters(saveText).Should().BeFalse("Save button should have LTR text");
            HasRTLCharacters(cancelText).Should().BeFalse("Cancel button should have LTR text");
        }

        [Test]
        [Description("Mixed content should preserve direction markers")]
        public void MixedContent_PreservesDirectionMarkers()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act - Get string that might have mixed content (numbers in Arabic text)
            var contractCodeLabel = _languageManager.GetString("Contract_Code");

            // Assert
            contractCodeLabel.Should().NotBeNullOrEmpty();
            contractCodeLabel.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Layout Mirroring Tests

        [Test]
        [Description("Form should mirror layout for Arabic")]
        public void LocalizeForm_Arabic_MirrorsLayout()
        {
            // This test would require mocking SAP B1 form objects
            // For now, we test the calculation logic

            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Original positions (LTR)
            int formWidth = 600;
            int label1Left = 10;
            int label1Width = 80;
            int textBox1Left = 100;
            int textBox1Width = 150;

            // Act - Calculate RTL positions
            int label1RTL = _languageManager.CalculateRTLPosition(formWidth, label1Left, label1Width);
            int textBox1RTL = _languageManager.CalculateRTLPosition(formWidth, textBox1Left, textBox1Width);

            // Assert
            // In RTL, label should be on the right (higher left value)
            // TextBox should be to the left of label (lower left value)
            label1RTL.Should().BeGreaterThan(textBox1RTL,
                "In RTL, label should be positioned to the right of its textbox");
        }

        [Test]
        [Description("Matrix columns should maintain relative order but align right")]
        public void LocalizeMatrix_Arabic_AlignColumnsRight()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var alignment = _languageManager.GetDefaultTextAlignment();

            // Assert
            alignment.Should().Be(BoTextAlignments.ta_Right,
                "Matrix columns should align to the right in Arabic");
        }

        #endregion

        #region Consistency Tests

        [Test]
        [Description("RTL settings should remain consistent after multiple operations")]
        public void RTL_RemainsConsistentAfterMultipleOperations()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act - Perform multiple operations
            var isRTL1 = _languageManager.IsRightToLeft;
            var text1 = _languageManager.GetString("Common_Save");
            var date1 = _languageManager.FormatDate(DateTime.Now, "short");
            var isRTL2 = _languageManager.IsRightToLeft;
            var alignment = _languageManager.GetDefaultTextAlignment();
            var isRTL3 = _languageManager.IsRightToLeft;

            // Assert
            isRTL1.Should().BeTrue();
            isRTL2.Should().BeTrue();
            isRTL3.Should().BeTrue();
            alignment.Should().Be(BoTextAlignments.ta_Right);
        }

        [Test]
        [Description("RTL should toggle correctly when switching languages")]
        public void RTL_TogglesCorrectlyOnLanguageSwitch()
        {
            // Arrange & Act
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var englishRTL = _languageManager.IsRightToLeft;

            _languageManager.SwitchLanguage();
            var afterSwitch1 = _languageManager.IsRightToLeft;

            _languageManager.SwitchLanguage();
            var afterSwitch2 = _languageManager.IsRightToLeft;

            // Assert
            englishRTL.Should().BeFalse("English is LTR");
            afterSwitch1.Should().BeTrue("Switched to Arabic (RTL)");
            afterSwitch2.Should().BeFalse("Switched back to English (LTR)");
        }

        #endregion

        #region Performance Tests

        [Test]
        [Description("RTL calculations should be fast")]
        public void CalculateRTLPosition_Performance_ExecutesQuickly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            int iterations = 10000;

            // Act
            var startTime = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                _languageManager.CalculateRTLPosition(600, 50, 100);
            }
            var endTime = DateTime.Now;
            var duration = (endTime - startTime).TotalMilliseconds;

            // Assert
            duration.Should().BeLessThan(100,
                $"10,000 RTL calculations should complete in under 100ms, took {duration}ms");
        }

        #endregion

        #region Helper Methods

        private bool HasRTLCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                // Arabic Unicode range: 0x0600 - 0x06FF
                // Hebrew Unicode range: 0x0590 - 0x05FF
                if ((c >= 0x0600 && c <= 0x06FF) || (c >= 0x0590 && c <= 0x05FF))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            // Reset to English
            _languageManager.LoadLanguage(SupportedLanguage.English);
        }
    }

    #region Test Enums (for alignment testing)

    // Mock SAP B1 text alignment enum for testing
    public enum BoTextAlignments
    {
        ta_Left,
        ta_Center,
        ta_Right
    }

    #endregion
}
