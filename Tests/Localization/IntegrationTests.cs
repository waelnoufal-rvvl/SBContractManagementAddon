using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using ContractManagementAddon.Localization;

namespace ContractManagementAddon.Tests.Localization
{
    /// <summary>
    /// Integration tests for complete localization workflows
    /// Tests real-world scenarios and end-to-end functionality
    /// </summary>
    [TestFixture]
    [Category("Localization")]
    [Category("Integration")]
    public class IntegrationTests
    {
        private LanguageManager _languageManager;

        [SetUp]
        public void Setup()
        {
            _languageManager = LanguageManager.Instance;
            _languageManager.LoadLanguage(SupportedLanguage.English);
        }

        #region Complete Workflow Tests

        [Test]
        [Description("Complete workflow: Load English, display form, switch to Arabic, reload")]
        public void CompleteWorkflow_EnglishToArabic_WorksCorrectly()
        {
            // Phase 1: Load English
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var formTitle_EN = _languageManager.GetString("Form_Contract_Title");
            var saveBtn_EN = _languageManager.GetString("Common_Save");
            var isRTL_EN = _languageManager.IsRightToLeft;

            formTitle_EN.Should().Be("Contract Management");
            saveBtn_EN.Should().Be("Save");
            isRTL_EN.Should().BeFalse();

            // Phase 2: Switch to Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var formTitle_AR = _languageManager.GetString("Form_Contract_Title");
            var saveBtn_AR = _languageManager.GetString("Common_Save");
            var isRTL_AR = _languageManager.IsRightToLeft;

            formTitle_AR.Should().Be("إدارة العقود");
            saveBtn_AR.Should().Be("حفظ");
            isRTL_AR.Should().BeTrue();

            // Phase 3: Verify different values
            formTitle_EN.Should().NotBe(formTitle_AR);
            saveBtn_EN.Should().NotBe(saveBtn_AR);
        }

        [Test]
        [Description("User scenario: Create contract in English, view in Arabic")]
        public void UserScenario_CreateInEnglish_ViewInArabic()
        {
            // Simulate user creating a contract in English
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var labels = new Dictionary<string, string>
            {
                ["Code"] = _languageManager.GetString("Contract_Code"),
                ["Name"] = _languageManager.GetString("Contract_Name"),
                ["Customer"] = _languageManager.GetString("Contract_Customer"),
                ["StartDate"] = _languageManager.GetString("Contract_StartDate"),
                ["EndDate"] = _languageManager.GetString("Contract_EndDate"),
                ["TotalValue"] = _languageManager.GetString("Contract_TotalValue")
            };

            // Verify English labels
            labels["Code"].Should().Be("Contract Code");
            labels["Name"].Should().Be("Contract Name");
            labels["Customer"].Should().Be("Customer");

            // User switches to Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var labelsAR = new Dictionary<string, string>
            {
                ["Code"] = _languageManager.GetString("Contract_Code"),
                ["Name"] = _languageManager.GetString("Contract_Name"),
                ["Customer"] = _languageManager.GetString("Contract_Customer"),
                ["StartDate"] = _languageManager.GetString("Contract_StartDate"),
                ["EndDate"] = _languageManager.GetString("Contract_EndDate"),
                ["TotalValue"] = _languageManager.GetString("Contract_TotalValue")
            };

            // Verify Arabic labels
            labelsAR["Code"].Should().Be("رمز العقد");
            labelsAR["Name"].Should().Be("اسم العقد");
            labelsAR["Customer"].Should().Be("العميل");

            // All labels should be different
            foreach (var key in labels.Keys)
            {
                labels[key].Should().NotBe(labelsAR[key], $"{key} should have different translations");
            }
        }

        [Test]
        [Description("Multi-form scenario: Contract, IPC, and CO forms all localize correctly")]
        public void MultiFormScenario_AllFormsLocalize()
        {
            // Test all three main forms
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var contractForm = _languageManager.GetString("Form_Contract_Title");
            var ipcForm = _languageManager.GetString("Form_IPC_Title");
            var coForm = _languageManager.GetString("Form_CO_Title");

            contractForm.Should().Be("Contract Management");
            ipcForm.Should().Be("Interim Payment Certificate");
            coForm.Should().Be("Change Order");

            // Switch to Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var contractFormAR = _languageManager.GetString("Form_Contract_Title");
            var ipcFormAR = _languageManager.GetString("Form_IPC_Title");
            var coFormAR = _languageManager.GetString("Form_CO_Title");

            contractFormAR.Should().Be("إدارة العقود");
            ipcFormAR.Should().Be("شهادة الدفع المرحلية");
            coFormAR.Should().Be("أمر التغيير");

            // All should be different
            contractForm.Should().NotBe(contractFormAR);
            ipcForm.Should().NotBe(ipcFormAR);
            coForm.Should().NotBe(coFormAR);
        }

        #endregion

        #region Data Display Integration Tests

        [Test]
        [Description("Display contract data with proper formatting in both languages")]
        public void DisplayContractData_BothLanguages_FormatsCorrectly()
        {
            // Simulate contract data
            var contractDate = new DateTime(2025, 1, 15);
            var contractValue = 1500000.50;
            var currency = "USD";

            // English display
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var dateEN = _languageManager.FormatDate(contractDate, "short");
            var valueEN = _languageManager.FormatCurrency(contractValue, currency);

            dateEN.Should().Be("1/15/2025");
            valueEN.Should().Contain("$");
            valueEN.Should().Contain("1,500,000.50");

            // Arabic display
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var dateAR = _languageManager.FormatDate(contractDate, "short");
            var valueAR = _languageManager.FormatCurrency(contractValue, currency);

            dateAR.Should().NotBe(dateEN, "Dates should format differently");
            valueAR.Should().Contain("$");
            valueAR.Should().EndWith("$", "Arabic currency should have symbol after amount");
        }

        [Test]
        [Description("Display IPC with multiple currencies")]
        public void DisplayIPC_MultipleCurrencies_FormatsCorrectly()
        {
            var amount = 50000.00;

            // Test multiple Middle East currencies in Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var sar = _languageManager.FormatCurrency(amount, "SAR");
            var aed = _languageManager.FormatCurrency(amount, "AED");
            var egp = _languageManager.FormatCurrency(amount, "EGP");

            sar.Should().Contain("ر.س");
            aed.Should().Contain("د.إ");
            egp.Should().Contain("ج.م");

            // All should have the amount
            sar.Should().Contain("50,000");
            aed.Should().Contain("50,000");
            egp.Should().Contain("50,000");
        }

        [Test]
        [Description("Format percentage values correctly in both languages")]
        public void FormatPercentages_BothLanguages_Consistent()
        {
            var percentage = 15.5;

            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);
            var percentEN = _languageManager.FormatNumber(percentage, 1);
            percentEN.Should().Be("15.5");

            // Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var percentAR = _languageManager.FormatNumber(percentage, 1);
            percentAR.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Message and Validation Integration Tests

        [Test]
        [Description("Display validation messages in correct language")]
        public void ValidationMessages_DisplayCorrectly()
        {
            // English validation
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var requiredMsg = _languageManager.GetString("Msg_RequiredField", "Contract Name");
            var successMsg = _languageManager.GetString("Msg_SaveSuccess");
            var errorMsg = _languageManager.GetString("Msg_SaveError");

            requiredMsg.Should().Contain("Contract Name");
            requiredMsg.Should().Contain("required");
            successMsg.Should().Contain("success");
            errorMsg.Should().Contain("error");

            // Arabic validation
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var requiredMsgAR = _languageManager.GetString("Msg_RequiredField", "اسم العقد");
            var successMsgAR = _languageManager.GetString("Msg_SaveSuccess");
            var errorMsgAR = _languageManager.GetString("Msg_SaveError");

            requiredMsgAR.Should().Contain("اسم العقد");
            successMsgAR.Should().NotBe(successMsg);
            errorMsgAR.Should().NotBe(errorMsg);

            // All should contain Arabic characters
            HasArabicCharacters(successMsgAR).Should().BeTrue();
            HasArabicCharacters(errorMsgAR).Should().BeTrue();
        }

        [Test]
        [Description("Confirmation dialogs show correct messages")]
        public void ConfirmationDialogs_ShowCorrectMessages()
        {
            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var deleteConfirm = _languageManager.GetString("Msg_ConfirmDelete");
            var cancelConfirm = _languageManager.GetString("Msg_ConfirmCancel");

            deleteConfirm.Should().NotBeNullOrEmpty();
            cancelConfirm.Should().NotBeNullOrEmpty();

            // Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var deleteConfirmAR = _languageManager.GetString("Msg_ConfirmDelete");
            var cancelConfirmAR = _languageManager.GetString("Msg_ConfirmCancel");

            deleteConfirmAR.Should().NotBe(deleteConfirm);
            cancelConfirmAR.Should().NotBe(cancelConfirm);
        }

        #endregion

        #region Menu and Navigation Tests

        [Test]
        [Description("Menu items localize correctly")]
        public void MenuItems_LocalizeCorrectly()
        {
            // English menus
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var menuContract = _languageManager.GetString("Menu_Contract");
            var menuIPC = _languageManager.GetString("Menu_IPC");
            var menuCO = _languageManager.GetString("Menu_CO");
            var menuReports = _languageManager.GetString("Menu_Reports");

            menuContract.Should().Be("Contract Management");
            menuIPC.Should().Be("IPC Management");
            menuCO.Should().Be("Change Orders");
            menuReports.Should().Be("Reports");

            // Arabic menus
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var menuContractAR = _languageManager.GetString("Menu_Contract");
            var menuIPCAR = _languageManager.GetString("Menu_IPC");
            var menuCOAR = _languageManager.GetString("Menu_CO");
            var menuReportsAR = _languageManager.GetString("Menu_Reports");

            menuContractAR.Should().Be("إدارة العقود");
            menuIPCAR.Should().Be("إدارة شهادات الدفع");
            menuCOAR.Should().Be("أوامر التغيير");
            menuReportsAR.Should().Be("التقارير");
        }

        #endregion

        #region Status and Dropdown Integration Tests

        [Test]
        [Description("Status values localize correctly in dropdowns")]
        public void StatusDropdowns_LocalizeCorrectly()
        {
            // English statuses
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var statuses = new Dictionary<string, string>
            {
                ["Active"] = _languageManager.GetString("Status_Active"),
                ["Completed"] = _languageManager.GetString("Status_Completed"),
                ["Cancelled"] = _languageManager.GetString("Status_Cancelled"),
                ["OnHold"] = _languageManager.GetString("Status_OnHold")
            };

            statuses["Active"].Should().Be("Active");
            statuses["Completed"].Should().Be("Completed");
            statuses["Cancelled"].Should().Be("Cancelled");
            statuses["OnHold"].Should().Be("On Hold");

            // Arabic statuses
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var statusesAR = new Dictionary<string, string>
            {
                ["Active"] = _languageManager.GetString("Status_Active"),
                ["Completed"] = _languageManager.GetString("Status_Completed"),
                ["Cancelled"] = _languageManager.GetString("Status_Cancelled"),
                ["OnHold"] = _languageManager.GetString("Status_OnHold")
            };

            statusesAR["Active"].Should().Be("نشط");
            statusesAR["Completed"].Should().Be("مكتمل");
            statusesAR["Cancelled"].Should().Be("ملغي");
            statusesAR["OnHold"].Should().Be("معلق");

            // All should be different
            foreach (var key in statuses.Keys)
            {
                statuses[key].Should().NotBe(statusesAR[key]);
            }
        }

        [Test]
        [Description("Contract type dropdown localizes correctly")]
        public void ContractTypeDropdown_LocalizesCorrectly()
        {
            // English
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var types = new[]
            {
                _languageManager.GetString("Contract_Type_FixedPrice"),
                _languageManager.GetString("Contract_Type_TimeAndMaterials"),
                _languageManager.GetString("Contract_Type_CostPlus")
            };

            types[0].Should().Be("Fixed Price");
            types[1].Should().Be("Time and Materials");
            types[2].Should().Be("Cost Plus");

            // Arabic
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var typesAR = new[]
            {
                _languageManager.GetString("Contract_Type_FixedPrice"),
                _languageManager.GetString("Contract_Type_TimeAndMaterials"),
                _languageManager.GetString("Contract_Type_CostPlus")
            };

            typesAR[0].Should().Be("سعر ثابت");
            typesAR[1].Should().Be("وقت ومواد");
            typesAR[2].Should().Be("التكلفة الإضافية");

            // All should be different
            for (int i = 0; i < types.Length; i++)
            {
                types[i].Should().NotBe(typesAR[i]);
            }
        }

        #endregion

        #region Report Integration Tests

        [Test]
        [Description("Report headers and columns localize correctly")]
        public void ReportColumns_LocalizeCorrectly()
        {
            // English report
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var reportTitle = _languageManager.GetString("Report_ContractList");
            var columns = new[]
            {
                _languageManager.GetString("Contract_Code"),
                _languageManager.GetString("Contract_Name"),
                _languageManager.GetString("Contract_Customer"),
                _languageManager.GetString("Contract_Status"),
                _languageManager.GetString("Contract_TotalValue")
            };

            reportTitle.Should().Be("Contract List Report");
            columns[0].Should().Be("Contract Code");
            columns[1].Should().Be("Contract Name");

            // Arabic report
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var reportTitleAR = _languageManager.GetString("Report_ContractList");
            var columnsAR = new[]
            {
                _languageManager.GetString("Contract_Code"),
                _languageManager.GetString("Contract_Name"),
                _languageManager.GetString("Contract_Customer"),
                _languageManager.GetString("Contract_Status"),
                _languageManager.GetString("Contract_TotalValue")
            };

            reportTitleAR.Should().Be("تقرير قائمة العقود");
            columnsAR[0].Should().Be("رمز العقد");
            columnsAR[1].Should().Be("اسم العقد");

            // All should be different
            reportTitle.Should().NotBe(reportTitleAR);
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i].Should().NotBe(columnsAR[i]);
            }
        }

        #endregion

        #region Settings Integration Tests

        [Test]
        [Description("Settings screen localizes completely")]
        public void SettingsScreen_LocalizesCompletely()
        {
            // English settings
            _languageManager.LoadLanguage(SupportedLanguage.English);

            var settingsTitle = _languageManager.GetString("Settings_Title");
            var settingsGeneral = _languageManager.GetString("Settings_General");
            var settingsLanguage = _languageManager.GetString("Settings_Language");
            var settingsCurrency = _languageManager.GetString("Settings_DefaultCurrency");

            settingsTitle.Should().Be("Settings");
            settingsGeneral.Should().Be("General Settings");
            settingsLanguage.Should().Be("Language");
            settingsCurrency.Should().Be("Default Currency");

            // Arabic settings
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            var settingsTitleAR = _languageManager.GetString("Settings_Title");
            var settingsGeneralAR = _languageManager.GetString("Settings_General");
            var settingsLanguageAR = _languageManager.GetString("Settings_Language");
            var settingsCurrencyAR = _languageManager.GetString("Settings_DefaultCurrency");

            settingsTitleAR.Should().Be("الإعدادات");
            settingsGeneralAR.Should().Be("الإعدادات العامة");
            settingsLanguageAR.Should().Be("اللغة");
            settingsCurrencyAR.Should().Be("العملة الافتراضية");
        }

        #endregion

        #region Event Integration Tests

        [Test]
        [Description("Language change event fires and UI updates")]
        public void LanguageChange_EventFires_UIUpdates()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            bool eventFired = false;
            SupportedLanguage oldLang = SupportedLanguage.English;
            SupportedLanguage newLang = SupportedLanguage.English;

            _languageManager.LanguageChanged += (sender, e) =>
            {
                eventFired = true;
                oldLang = e.PreviousLanguage;
                newLang = e.NewLanguage;
            };

            // Capture UI values before change
            var titleBefore = _languageManager.GetString("Form_Contract_Title");

            // Act - Switch language
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);

            // Capture UI values after change
            var titleAfter = _languageManager.GetString("Form_Contract_Title");

            // Assert
            eventFired.Should().BeTrue("Language change event should fire");
            oldLang.Should().Be(SupportedLanguage.English);
            newLang.Should().Be(SupportedLanguage.Arabic);
            titleBefore.Should().NotBe(titleAfter, "UI should update after language change");
        }

        #endregion

        #region Performance Integration Tests

        [Test]
        [Description("Loading 100 form fields should be fast")]
        public void LoadManyFormFields_PerformanceIsGood()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.Arabic);
            var keys = new List<string>();

            // Collect common keys
            for (int i = 0; i < 20; i++)
            {
                keys.Add("Common_Save");
                keys.Add("Common_Cancel");
                keys.Add("Contract_Code");
                keys.Add("Contract_Name");
                keys.Add("IPC_Number");
            }

            // Act - Time the loading
            var startTime = DateTime.Now;
            foreach (var key in keys)
            {
                var value = _languageManager.GetString(key);
                value.Should().NotBeNullOrEmpty();
            }
            var endTime = DateTime.Now;
            var duration = (endTime - startTime).TotalMilliseconds;

            // Assert
            duration.Should().BeLessThan(50,
                $"Loading 100 strings should take less than 50ms, took {duration}ms");
        }

        #endregion

        #region Edge Case Integration Tests

        [Test]
        [Description("Handle rapid language switching")]
        public void RapidLanguageSwitching_HandlesCorrectly()
        {
            // Act - Switch rapidly
            for (int i = 0; i < 10; i++)
            {
                _languageManager.SwitchLanguage();
                var text = _languageManager.GetString("Common_Save");
                text.Should().NotBeNullOrEmpty();
            }

            // Should end up back at English (even number of switches)
            _languageManager.CurrentLanguage.Should().Be(SupportedLanguage.English);
        }

        [Test]
        [Description("Handle missing keys gracefully in production scenario")]
        public void MissingKeys_HandleGracefully()
        {
            // Arrange
            _languageManager.LoadLanguage(SupportedLanguage.English);

            // Act - Request non-existent keys
            var missing1 = _languageManager.GetString("NonExistent_Key1");
            var missing2 = _languageManager.GetString("NonExistent_Key2", "Default Value");

            // Assert - Should not crash
            missing1.Should().Be("[NonExistent_Key1]");
            missing2.Should().Be("Default Value");

            // Should still work after missing keys
            var existingKey = _languageManager.GetString("Common_Save");
            existingKey.Should().Be("Save");
        }

        #endregion

        #region Helper Methods

        private bool HasArabicCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.Any(c => c >= 0x0600 && c <= 0x06FF);
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
