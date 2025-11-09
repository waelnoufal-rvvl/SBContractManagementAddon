using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Tests to validate resource file integrity and completeness
    /// </summary>
    [TestFixture]
    [Category("Localization")]
    [Category("ResourceValidation")]
    public class ResourceValidationTests
    {
        private string _resourcePath;
        private LanguageManager _languageManager;

        [SetUp]
        public void Setup()
        {
            _languageManager = LanguageManager.Instance;
            _resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization", "Resources");
        }

        #region Resource File Existence Tests

        [Test]
        [Description("English resource file should exist")]
        public void ResourceFile_English_Exists()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");

            // Assert
            File.Exists(filePath).Should().BeTrue($"English resource file should exist at {filePath}");
        }

        [Test]
        [Description("Arabic resource file should exist")]
        public void ResourceFile_Arabic_Exists()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            // Assert
            File.Exists(filePath).Should().BeTrue($"Arabic resource file should exist at {filePath}");
        }

        #endregion

        #region XML Structure Tests

        [Test]
        [Description("English resource file should be valid XML")]
        public void ResourceFile_English_IsValidXML()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");

            // Act & Assert
            Action act = () => XDocument.Load(filePath);
            act.Should().NotThrow("English resource file should be valid XML");
        }

        [Test]
        [Description("Arabic resource file should be valid XML")]
        public void ResourceFile_Arabic_IsValidXML()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            // Act & Assert
            Action act = () => XDocument.Load(filePath);
            act.Should().NotThrow("Arabic resource file should be valid XML");
        }

        [Test]
        [Description("English resource file should have correct structure")]
        public void ResourceFile_English_HasCorrectStructure()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");
            var doc = XDocument.Load(filePath);

            // Assert
            doc.Root.Should().NotBeNull();
            doc.Root.Name.LocalName.Should().Be("Resources");
            doc.Root.Element("Metadata").Should().NotBeNull();
            doc.Root.Element("Strings").Should().NotBeNull();
        }

        [Test]
        [Description("Arabic resource file should have correct structure")]
        public void ResourceFile_Arabic_HasCorrectStructure()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");
            var doc = XDocument.Load(filePath);

            // Assert
            doc.Root.Should().NotBeNull();
            doc.Root.Name.LocalName.Should().Be("Resources");
            doc.Root.Element("Metadata").Should().NotBeNull();
            doc.Root.Element("Strings").Should().NotBeNull();
        }

        #endregion

        #region Metadata Tests

        [Test]
        [Description("English resource should have correct metadata")]
        public void ResourceMetadata_English_IsCorrect()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");
            var doc = XDocument.Load(filePath);
            var metadata = doc.Root.Element("Metadata");

            // Assert
            metadata.Element("Language").Value.Should().Be("English");
            metadata.Element("LanguageCode").Value.Should().Be("en-US");
            metadata.Element("Direction").Value.Should().Be("LTR");
        }

        [Test]
        [Description("Arabic resource should have correct metadata")]
        public void ResourceMetadata_Arabic_IsCorrect()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");
            var doc = XDocument.Load(filePath);
            var metadata = doc.Root.Element("Metadata");

            // Assert
            metadata.Element("Language").Value.Should().Be("Arabic");
            metadata.Element("LanguageCode").Value.Should().Be("ar-SA");
            metadata.Element("Direction").Value.Should().Be("RTL");
        }

        #endregion

        #region String Count Tests

        [Test]
        [Description("Resource files should have minimum number of strings")]
        public void ResourceStrings_ShouldHaveMinimumCount()
        {
            // Arrange
            var englishPath = Path.Combine(_resourcePath, "Resources.English.xml");
            var arabicPath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            var englishDoc = XDocument.Load(englishPath);
            var arabicDoc = XDocument.Load(arabicPath);

            var englishCount = englishDoc.Root.Element("Strings").Elements("String").Count();
            var arabicCount = arabicDoc.Root.Element("Strings").Elements("String").Count();

            // Assert
            englishCount.Should().BeGreaterThan(100, "English should have at least 100 strings");
            arabicCount.Should().BeGreaterThan(100, "Arabic should have at least 100 strings");
        }

        [Test]
        [Description("English and Arabic should have same number of strings")]
        public void ResourceStrings_BothLanguages_ShouldHaveSameCount()
        {
            // Arrange
            var englishPath = Path.Combine(_resourcePath, "Resources.English.xml");
            var arabicPath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            var englishDoc = XDocument.Load(englishPath);
            var arabicDoc = XDocument.Load(arabicPath);

            var englishCount = englishDoc.Root.Element("Strings").Elements("String").Count();
            var arabicCount = arabicDoc.Root.Element("Strings").Elements("String").Count();

            // Assert
            englishCount.Should().Be(arabicCount,
                $"Both languages should have the same number of strings. English: {englishCount}, Arabic: {arabicCount}");
        }

        #endregion

        #region Key Validation Tests

        [Test]
        [Description("All string keys should be unique in English")]
        public void ResourceKeys_English_ShouldBeUnique()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");
            var doc = XDocument.Load(filePath);
            var keys = doc.Root.Element("Strings").Elements("String")
                .Select(e => e.Attribute("Key")?.Value)
                .ToList();

            // Assert
            var duplicates = keys.GroupBy(k => k)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            duplicates.Should().BeEmpty($"Found duplicate keys: {string.Join(", ", duplicates)}");
        }

        [Test]
        [Description("All string keys should be unique in Arabic")]
        public void ResourceKeys_Arabic_ShouldBeUnique()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");
            var doc = XDocument.Load(filePath);
            var keys = doc.Root.Element("Strings").Elements("String")
                .Select(e => e.Attribute("Key")?.Value)
                .ToList();

            // Assert
            var duplicates = keys.GroupBy(k => k)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            duplicates.Should().BeEmpty($"Found duplicate keys: {string.Join(", ", duplicates)}");
        }

        [Test]
        [Description("All English keys should exist in Arabic")]
        public void ResourceKeys_EnglishKeysExistInArabic()
        {
            // Arrange
            var englishPath = Path.Combine(_resourcePath, "Resources.English.xml");
            var arabicPath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            var englishKeys = GetResourceKeys(englishPath);
            var arabicKeys = GetResourceKeys(arabicPath);

            // Assert
            var missingKeys = englishKeys.Except(arabicKeys).ToList();
            missingKeys.Should().BeEmpty(
                $"Arabic is missing keys: {string.Join(", ", missingKeys.Take(10))}...");
        }

        [Test]
        [Description("All Arabic keys should exist in English")]
        public void ResourceKeys_ArabicKeysExistInEnglish()
        {
            // Arrange
            var englishPath = Path.Combine(_resourcePath, "Resources.English.xml");
            var arabicPath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            var englishKeys = GetResourceKeys(englishPath);
            var arabicKeys = GetResourceKeys(arabicPath);

            // Assert
            var extraKeys = arabicKeys.Except(englishKeys).ToList();
            extraKeys.Should().BeEmpty(
                $"Arabic has extra keys not in English: {string.Join(", ", extraKeys.Take(10))}...");
        }

        #endregion

        #region Value Validation Tests

        [Test]
        [Description("No string values should be empty in English")]
        public void ResourceValues_English_ShouldNotBeEmpty()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.English.xml");
            var doc = XDocument.Load(filePath);
            var emptyValues = doc.Root.Element("Strings").Elements("String")
                .Where(e => string.IsNullOrWhiteSpace(e.Value))
                .Select(e => e.Attribute("Key")?.Value)
                .ToList();

            // Assert
            emptyValues.Should().BeEmpty(
                $"English has empty values for keys: {string.Join(", ", emptyValues)}");
        }

        [Test]
        [Description("No string values should be empty in Arabic")]
        public void ResourceValues_Arabic_ShouldNotBeEmpty()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");
            var doc = XDocument.Load(filePath);
            var emptyValues = doc.Root.Element("Strings").Elements("String")
                .Where(e => string.IsNullOrWhiteSpace(e.Value))
                .Select(e => e.Attribute("Key")?.Value)
                .ToList();

            // Assert
            emptyValues.Should().BeEmpty(
                $"Arabic has empty values for keys: {string.Join(", ", emptyValues)}");
        }

        [Test]
        [Description("All common strings should be translated")]
        public void ResourceValues_CommonStrings_ShouldBeTranslated()
        {
            // Arrange
            var commonKeys = new[]
            {
                "Common_Save", "Common_Cancel", "Common_OK", "Common_Close",
                "Common_Add", "Common_Edit", "Common_Delete", "Common_Search"
            };

            // Load both languages
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var englishValues = commonKeys.ToDictionary(k => k, k => _languageManager.GetString(k));

            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var arabicValues = commonKeys.ToDictionary(k => k, k => _languageManager.GetString(k));

            // Assert
            foreach (var key in commonKeys)
            {
                englishValues[key].Should().NotStartWith("[", $"English translation missing for {key}");
                arabicValues[key].Should().NotStartWith("[", $"Arabic translation missing for {key}");
                englishValues[key].Should().NotBe(arabicValues[key],
                    $"{key} has same value in both languages - likely not translated");
            }
        }

        #endregion

        #region Format String Tests

        [Test]
        [Description("Format strings should have matching parameter counts")]
        public void FormatStrings_ShouldHaveMatchingParameterCounts()
        {
            // Arrange
            var englishPath = Path.Combine(_resourcePath, "Resources.English.xml");
            var arabicPath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            var englishStrings = GetResourceStrings(englishPath);
            var arabicStrings = GetResourceStrings(arabicPath);

            var mismatches = new List<string>();

            // Act
            foreach (var key in englishStrings.Keys)
            {
                if (!arabicStrings.ContainsKey(key))
                    continue;

                var englishCount = CountFormatPlaceholders(englishStrings[key]);
                var arabicCount = CountFormatPlaceholders(arabicStrings[key]);

                if (englishCount != arabicCount)
                {
                    mismatches.Add($"{key}: English has {englishCount} placeholders, Arabic has {arabicCount}");
                }
            }

            // Assert
            mismatches.Should().BeEmpty(
                $"Format string parameter mismatch:\n{string.Join("\n", mismatches.Take(10))}");
        }

        #endregion

        #region Required Keys Tests

        [Test]
        [Description("All required UI keys should exist")]
        [TestCase("Form_Contract_Title")]
        [TestCase("Form_IPC_Title")]
        [TestCase("Form_CO_Title")]
        [TestCase("Contract_Code")]
        [TestCase("Contract_Name")]
        [TestCase("IPC_Number")]
        [TestCase("CO_Number")]
        [TestCase("Common_Save")]
        [TestCase("Common_Cancel")]
        [TestCase("Msg_SaveSuccess")]
        public void RequiredKeys_ShouldExist(string key)
        {
            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var english = _languageManager.GetString(key);
            english.Should().NotStartWith("[", $"English translation missing for required key: {key}");

            // Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var arabic = _languageManager.GetString(key);
            arabic.Should().NotStartWith("[", $"Arabic translation missing for required key: {key}");
        }

        #endregion

        #region Character Encoding Tests

        [Test]
        [Description("Arabic resource file should have correct UTF-8 encoding")]
        public void ResourceFile_Arabic_HasCorrectEncoding()
        {
            // Arrange
            var filePath = Path.Combine(_resourcePath, "Resources.Arabic.xml");

            // Act
            var content = File.ReadAllText(filePath);

            // Assert
            content.Should().Contain("encoding=\"utf-8\"");
            content.Should().Contain("العربية"); // Should contain Arabic text
        }

        [Test]
        [Description("Arabic strings should contain Arabic characters")]
        public void ResourceStrings_Arabic_ContainArabicCharacters()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var save = _languageManager.GetString("Common_Save");
            var cancel = _languageManager.GetString("Common_Cancel");

            // Assert
            HasArabicCharacters(save).Should().BeTrue("Save button should have Arabic text");
            HasArabicCharacters(cancel).Should().BeTrue("Cancel button should have Arabic text");
        }

        #endregion

        #region Helper Methods

        private List<string> GetResourceKeys(string filePath)
        {
            var doc = XDocument.Load(filePath);
            return doc.Root.Element("Strings").Elements("String")
                .Select(e => e.Attribute("Key")?.Value)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();
        }

        private Dictionary<string, string> GetResourceStrings(string filePath)
        {
            var doc = XDocument.Load(filePath);
            return doc.Root.Element("Strings").Elements("String")
                .Where(e => e.Attribute("Key") != null)
                .ToDictionary(
                    e => e.Attribute("Key").Value,
                    e => e.Value
                );
        }

        private int CountFormatPlaceholders(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (text.Contains("{" + i + "}"))
                    count++;
            }
            return count;
        }

        private bool HasArabicCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.Any(c => c >= 0x0600 && c <= 0x06FF); // Arabic Unicode range
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            _languageManager.LoadLanguage(SupportedLanguage.English);
        }
    }
}
