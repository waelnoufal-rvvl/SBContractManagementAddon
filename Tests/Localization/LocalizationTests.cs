using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Unit tests for LanguageManager core functionality
    /// </summary>
    [TestFixture]
    [Category("Localization")]
    public class LocalizationTests
    {
        private LanguageManager _languageManager;

        [SetUp]
        public void Setup()
        {
            _languageManager = LanguageManager.Instance;
        }

        #region Language Loading Tests

        [Test]
        [Description("LoadLanguage should load English successfully")]
        public void LoadLanguage_English_LoadsSuccessfully()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.English);
            _languageManager.CurrentCulture.Name.Should().Be("en-US");
            _languageManager.IsRightToLeft.Should().BeFalse();
        }

        [Test]
        [Description("LoadLanguage should load Arabic successfully")]
        public void LoadLanguage_Arabic_LoadsSuccessfully()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.Arabic);
            _languageManager.CurrentCulture.Name.Should().Be("ar-SA");
            _languageManager.IsRightToLeft.Should().BeTrue();
        }

        [Test]
        [Description("LoadLanguage should set thread culture correctly")]
        public void LoadLanguage_ShouldSetThreadCulture()
        {
            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            System.Threading.Thread.CurrentThread.CurrentCulture.Name.Should().Be("ar-SA");
            System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Should().Be("ar-SA");
        }

        [Test]
        [Description("LoadLanguage should fire LanguageChanged event")]
        public void LoadLanguage_ShouldFireLanguageChangedEvent()
        {
            // Arrange
            bool eventFired = false;
            SupportedLanguage oldLang = SupportedLanguage.English;
            SupportedLanguage newLang = SupportedLanguage.English;

            _languageManager.LoadLanguage(SupportedLanguage.English);

            _languageManager.LanguageChanged += (sender, e) =>
            {
                eventFired = true;
                oldLang = e.PreviousLanguage;
                newLang = e.NewLanguage;
            };

            // Act
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Assert
            eventFired.Should().BeTrue();
            oldLang.Should().Be(SupportedLanguage.English);
            newLang.Should().Be(SupportedLanguage.Arabic);
        }

        [Test]
        [Description("LoadLanguage same language should not fire event")]
        public void LoadLanguage_SameLanguage_ShouldNotFireEvent()
        {
            // Arrange
            bool eventFired = false;
            _languageManager.LoadLanguage(SupportedLanguage.English);

            _languageManager.LanguageChanged += (sender, e) =>
            {
                eventFired = true;
            };

            // Act
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Assert
            eventFired.Should().BeFalse();
        }

        #endregion

        #region GetString Tests

        [Test]
        [Description("GetString should return correct English translation")]
        public void GetString_English_ReturnsCorrectTranslation()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var save = _languageManager.GetString("Common_Save");
            var cancel = _languageManager.GetString("Common_Cancel");

            // Assert
            save.Should().Be("Save");
            cancel.Should().Be("Cancel");
        }

        [Test]
        [Description("GetString should return correct Arabic translation")]
        public void GetString_Arabic_ReturnsCorrectTranslation()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var save = _languageManager.GetString("Common_Save");
            var cancel = _languageManager.GetString("Common_Cancel");

            // Assert
            save.Should().Be("حفظ");
            cancel.Should().Be("إلغاء");
        }

        [Test]
        [Description("GetString with missing key should return default value")]
        public void GetString_MissingKey_ReturnsDefaultValue()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var defaultValue = "Default Text";

            // Act
            var result = _languageManager.GetString("NonExistentKey", defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        [Description("GetString with missing key and no default should return key in brackets")]
        public void GetString_MissingKeyNoDefault_ReturnsKeyInBrackets()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.GetString("NonExistentKey");

            // Assert
            result.Should().Be("[NonExistentKey]");
        }

        [Test]
        [Description("GetString with parameters should format correctly")]
        public void GetString_WithParameters_FormatsCorrectly()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.GetString("Msg_RequiredField", "Contract Name");

            // Assert
            result.Should().Contain("Contract Name");
            result.Should().Contain("required");
        }

        [Test]
        [Description("GetString with null key should return empty string")]
        public void GetString_NullKey_ReturnsEmptyString()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.GetString(null);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        [Description("GetString with empty key should return empty string")]
        public void GetString_EmptyKey_ReturnsEmptyString()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act
            var result = _languageManager.GetString("");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region SwitchLanguage Tests

        [Test]
        [Description("SwitchLanguage should toggle between languages")]
        public void SwitchLanguage_ShouldToggleBetweenLanguages()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act - Switch to Arabic
            _languageManager.SwitchLanguage();

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.Arabic);

            // Act - Switch back to English
            _languageManager.SwitchLanguage();

            // Assert
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.English);
        }

        #endregion

        #region GetAvailableLanguages Tests

        [Test]
        [Description("GetAvailableLanguages should return all supported languages")]
        public void GetAvailableLanguages_ShouldReturnAllLanguages()
        {
            // Act
            var languages = _languageManager.GetAvailableLanguages();

            // Assert
            languages.Should().NotBeNull();
            languages.Should().HaveCount(2);
            languages.Should().ContainKey(SupportedLanguage.English);
            languages.Should().ContainKey(SupportedLanguage.Arabic);
        }

        [Test]
        [Description("GetAvailableLanguages should return language names")]
        public void GetAvailableLanguages_ShouldReturnLanguageNames()
        {
            // Act
            var languages = _languageManager.GetAvailableLanguages();

            // Assert
            languages[SupportedLanguage.English].Should().Be("English");
            languages[SupportedLanguage.Arabic].Should().Be("العربية");
        }

        #endregion

        #region Common Strings Tests

        [Test]
        [Description("All common strings should be available in both languages")]
        [TestCase("Common_Yes")]
        [TestCase("Common_No")]
        [TestCase("Common_OK")]
        [TestCase("Common_Cancel")]
        [TestCase("Common_Save")]
        [TestCase("Common_Close")]
        [TestCase("Common_Add")]
        [TestCase("Common_Edit")]
        [TestCase("Common_Delete")]
        [TestCase("Common_Search")]
        public void CommonStrings_ShouldBeAvailableInBothLanguages(string key)
        {
            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var english = _languageManager.GetString(key);
            english.Should().NotBeNullOrEmpty();
            english.Should().NotStartWith("[");

            // Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var arabic = _languageManager.GetString(key);
            arabic.Should().NotBeNullOrEmpty();
            arabic.Should().NotStartWith("[");

            // Should be different
            english.Should().NotBe(arabic);
        }

        #endregion

        #region Singleton Tests

        [Test]
        [Description("LanguageManager should be singleton")]
        public void LanguageManager_ShouldBeSingleton()
        {
            // Act
            var instance1 = LanguageManager.Instance;
            var instance2 = LanguageManager.Instance;

            // Assert
            instance1.Should().BeSameAs(instance2);
        }

        [Test]
        [Description("LanguageManager state should persist across instances")]
        public void LanguageManager_StateShouldPersist()
        {
            // Arrange
            var instance1 = LanguageManager.Instance;
            instance1.LoadLanguage(SupportedLanguage.Arabic);

            // Act
            var instance2 = LanguageManager.Instance;

            // Assert
            instance2.CurrentLanguage.Should().Be(SupportedLanguage.Arabic);
        }

        #endregion

        #region Thread Safety Tests

        [Test]
        [Description("LoadLanguage should be thread-safe")]
        public void LoadLanguage_ShouldBeThreadSafe()
        {
            // Arrange
            var tasks = new System.Threading.Tasks.Task[10];

            // Act - Load languages from multiple threads
            for (int i = 0; i < tasks.Length; i++)
            {
                var lang = i % 2 == 0 ? SupportedLanguage.English : SupportedLanguage.Arabic;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    _languageManager.LoadLanguage(lang);
                    var result = _languageManager.GetString("Common_Save");
                    result.Should().NotBeNullOrEmpty();
                });
            }

            // Wait for all tasks
            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert - No exceptions should be thrown
            foreach (var task in tasks)
            {
                task.IsCompleted.Should().BeTrue();
                task.IsFaulted.Should().BeFalse();
            }
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
