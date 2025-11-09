# Multi-Language Localization Test Suite

## Overview

This comprehensive test suite validates the multi-language (English/Arabic) implementation for the Contract Management Add-on, ensuring everything works correctly without bugs or crashes.

---

## Test Coverage

### 📊 Total Test Count: **150+ Tests**

The test suite includes:

1. **LocalizationTests.cs** - 30+ tests
   - Core LanguageManager functionality
   - Language loading and switching
   - String retrieval and formatting
   - Singleton pattern validation
   - Thread safety tests

2. **FormattingTests.cs** - 32+ tests
   - Date formatting (English/Arabic)
   - Number formatting with correct separators
   - Currency formatting with proper symbols
   - Culture consistency tests
   - Edge cases (min/max values, zero, negatives)

3. **ResourceValidationTests.cs** - 20+ tests
   - Resource file existence and structure
   - XML validation
   - Metadata verification
   - Key uniqueness and consistency
   - Value completeness
   - Format string validation
   - Character encoding verification

4. **RTLLayoutTests.cs** - 25+ tests
   - RTL detection for Arabic
   - Layout mirroring calculations
   - Text alignment
   - Button ordering
   - Unicode and bidirectional text
   - Performance tests

5. **IntegrationTests.cs** - 40+ tests
   - Complete user workflows
   - Multi-form scenarios
   - Data display integration
   - Message and validation integration
   - Menu and navigation
   - Status dropdowns
   - Report localization
   - Settings screen
   - Event handling
   - Performance tests
   - Edge cases

---

## Test Categories

### ✅ Functional Tests
- Language loading (English/Arabic)
- String retrieval with fallbacks
- Language switching
- Culture-aware formatting
- RTL layout support

### ✅ Data Integrity Tests
- Resource file validation
- Key consistency between languages
- No missing translations
- No duplicate keys
- Proper encoding

### ✅ UI/UX Tests
- Form localization
- Menu item translation
- Button and label text
- Dropdown values
- Validation messages
- Confirmation dialogs

### ✅ Performance Tests
- Fast string retrieval (100+ lookups < 50ms)
- RTL calculations (10,000 operations < 100ms)
- Rapid language switching
- Memory efficiency

### ✅ Edge Case Tests
- Missing keys
- Null/empty inputs
- Minimum/maximum values
- Rapid language changes
- Concurrent access

---

## How to Run Tests

### Method 1: Using NUnit Test Runner (Recommended)

```bash
# Install NUnit Console Runner
dotnet tool install -g NUnit.ConsoleRunner

# Run all tests
nunit3-console Tests/Localization/*.dll

# Run specific test file
nunit3-console Tests/Localization/LocalizationTests.dll
```

### Method 2: Using Custom Test Runner

```bash
# Compile and run the custom test runner
cd Tests/Localization
dotnet run RunLocalizationTests.cs
```

### Method 3: Using Visual Studio

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to execute all tests
3. View results in the Test Explorer window

### Method 4: Using Command Line

```bash
# Run tests using dotnet test
dotnet test --filter "Category=Localization"
```

---

## Test Results Interpretation

### Success Criteria

✅ **ALL TESTS PASSED** means:
- Multi-language system is working correctly
- No bugs or crashes detected
- All translations are complete
- RTL support is functioning properly
- Performance is acceptable
- System is ready for production

### If Tests Fail

❌ **FAILED TESTS** indicate:
- Missing translations
- Incorrect formatting
- RTL layout issues
- Resource file problems
- Performance bottlenecks

**Action:** Review the test report, identify the failed test, and fix the underlying issue.

---

## Test Reports

### Console Output

The test runner generates a formatted console output:

```
╔══════════════════════════════════════════════════════════════════╗
║     Contract Management Add-on - Localization Test Suite         ║
╚══════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────┐
│ Running: Core Localization Tests                                │
└─────────────────────────────────────────────────────────────────┘
Found 30 tests

  [✓ PASS] LoadLanguage_English_LoadsSuccessfully
  [✓ PASS] LoadLanguage_Arabic_LoadsSuccessfully
  [✓ PASS] GetString_English_ReturnsCorrectTranslation
  ...

  Results: 30 passed, 0 failed, 0 skipped

╔══════════════════════════════════════════════════════════════════╗
║                      TEST SUMMARY                                 ║
╚══════════════════════════════════════════════════════════════════╝

  Total Tests:    150
  Passed:         150 ✓
  Failed:         0
  Skipped:        0
  Success Rate:   100.0%
  Duration:       2.45 seconds

╔══════════════════════════════════════════════════════════════════╗
║              ✓ ALL TESTS PASSED SUCCESSFULLY! ✓                  ║
║                                                                   ║
║   Multi-language implementation is working correctly without      ║
║   bugs or crashes. The system is ready for production use.        ║
╚══════════════════════════════════════════════════════════════════╝
```

### HTML Report

An HTML report is automatically generated at:
```
Tests/TestReports/LocalizationTestReport_YYYYMMDD_HHMMSS.html
```

The HTML report includes:
- Executive summary with success rate
- Visual progress bar
- Detailed test results by suite
- Error messages and stack traces
- Test duration and performance metrics

---

## What Each Test File Tests

### 1. LocalizationTests.cs

**Purpose:** Validate core LanguageManager functionality

**Key Tests:**
- `LoadLanguage_English_LoadsSuccessfully()` - Verifies English loads correctly
- `LoadLanguage_Arabic_LoadsSuccessfully()` - Verifies Arabic loads correctly
- `GetString_English_ReturnsCorrectTranslation()` - Checks "Save" = "Save"
- `GetString_Arabic_ReturnsCorrectTranslation()` - Checks "حفظ" = "Save" in Arabic
- `LanguageChanged_EventFires()` - Validates event system
- `LanguageManager_ShouldBeSingleton()` - Ensures singleton pattern
- `LoadLanguage_ShouldBeThreadSafe()` - Tests concurrent access

**Coverage:** Core functionality, singleton, events, thread safety

---

### 2. FormattingTests.cs

**Purpose:** Validate culture-aware formatting

**Key Tests:**
- `FormatDate_ShortEnglish_FormatsCorrectly()` - "1/15/2025"
- `FormatDate_ShortArabic_FormatsCorrectly()` - Arabic date format
- `FormatNumber_English_UsesCorrectSeparators()` - "1,234,567.89"
- `FormatCurrency_USD_English_FormatsCorrectly()` - "$100,000.00"
- `FormatCurrency_SAR_FormatsCorrectly()` - "100,000.00 ر.س"
- `FormatCurrency_MiddleEastCurrencies_FormatsCorrectly()` - SAR, AED, EGP, etc.

**Coverage:** Dates, numbers, currencies, separators, symbol positioning

---

### 3. ResourceValidationTests.cs

**Purpose:** Validate resource file integrity

**Key Tests:**
- `ResourceFile_English_Exists()` - File exists
- `ResourceFile_English_IsValidXML()` - Valid XML structure
- `ResourceFile_English_HasCorrectStructure()` - Has Metadata and Strings
- `ResourceMetadata_English_IsCorrect()` - Language=English, Code=en-US, Direction=LTR
- `ResourceStrings_BothLanguages_ShouldHaveSameCount()` - English and Arabic have same number
- `ResourceKeys_EnglishKeysExistInArabic()` - No missing translations
- `ResourceKeys_English_ShouldBeUnique()` - No duplicate keys
- `ResourceValues_English_ShouldNotBeEmpty()` - No empty translations
- `FormatStrings_ShouldHaveMatchingParameterCounts()` - {0}, {1} consistent
- `RequiredKeys_ShouldExist()` - All required UI keys present
- `ResourceFile_Arabic_HasCorrectEncoding()` - UTF-8 encoding
- `ResourceStrings_Arabic_ContainArabicCharacters()` - Arabic text verified

**Coverage:** File existence, XML structure, metadata, keys, values, encoding

---

### 4. RTLLayoutTests.cs

**Purpose:** Validate Right-to-Left layout support

**Key Tests:**
- `IsRightToLeft_English_ReturnsFalse()` - LTR for English
- `IsRightToLeft_Arabic_ReturnsTrue()` - RTL for Arabic
- `CalculateRTLPosition_ReturnsCorrectMirroredPosition()` - Position mirroring math
- `GetDefaultAlignment_English_ReturnsLeft()` - Left alignment for English
- `GetDefaultAlignment_Arabic_ReturnsRight()` - Right alignment for Arabic
- `GetButtonOrder_English_ReturnsOKCancel()` - OK-Cancel order
- `GetButtonOrder_Arabic_ReturnsCancelOK()` - Cancel-OK order (reversed)
- `ArabicText_ContainsRTLCharacters()` - Unicode verification
- `CalculateRTLPosition_Performance_ExecutesQuickly()` - Performance test

**Coverage:** RTL detection, layout calculations, alignment, button order, performance

---

### 5. IntegrationTests.cs

**Purpose:** End-to-end workflow validation

**Key Tests:**
- `CompleteWorkflow_EnglishToArabic_WorksCorrectly()` - Full workflow
- `UserScenario_CreateInEnglish_ViewInArabic()` - Real user scenario
- `MultiFormScenario_AllFormsLocalize()` - Contract, IPC, CO forms
- `DisplayContractData_BothLanguages_FormatsCorrectly()` - Data display
- `ValidationMessages_DisplayCorrectly()` - Error messages
- `MenuItems_LocalizeCorrectly()` - Menu localization
- `StatusDropdowns_LocalizeCorrectly()` - Dropdown values
- `ReportColumns_LocalizeCorrectly()` - Report headers
- `SettingsScreen_LocalizesCompletely()` - Settings UI
- `LanguageChange_EventFires_UIUpdates()` - Dynamic updates
- `LoadManyFormFields_PerformanceIsGood()` - Performance test
- `RapidLanguageSwitching_HandlesCorrectly()` - Stress test
- `MissingKeys_HandleGracefully()` - Error handling

**Coverage:** Complete workflows, user scenarios, UI integration, performance

---

## Test Data

### Sample Translations Tested

| Key | English | Arabic |
|-----|---------|--------|
| Common_Save | Save | حفظ |
| Common_Cancel | Cancel | إلغاء |
| Contract_Code | Contract Code | رمز العقد |
| Contract_Name | Contract Name | اسم العقد |
| Status_Active | Active | نشط |
| Msg_SaveSuccess | Record saved successfully. | تم حفظ السجل بنجاح. |

### Sample Formatting Tested

| Type | English | Arabic |
|------|---------|--------|
| Date | 1/15/2025 | ١٥‏/٠١‏/٢٠٢٥ |
| Number | 1,234,567.89 | ١٬٢٣٤٬٥٦٧٫٨٩ |
| USD | $100,000.00 | 100,000.00 $ |
| SAR | 100,000.00 SAR | 100,000.00 ر.س |

---

## Continuous Testing

### Recommended Testing Schedule

- **Before Every Commit:** Run relevant tests
- **Before Pull Request:** Run all tests
- **Before Release:** Full test suite + manual testing
- **After Adding Translations:** Run resource validation tests
- **After Changing Formatting:** Run formatting tests

### Automated Testing

Add to your CI/CD pipeline:

```yaml
# .github/workflows/localization-tests.yml
name: Localization Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
      - name: Run Localization Tests
        run: dotnet test --filter "Category=Localization"
```

---

## Troubleshooting

### Common Issues

#### Issue: Tests can't find resource files

**Solution:**
```csharp
// Ensure resource files are set to "Copy to Output Directory"
// Or update path in tests
_resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization", "Resources");
```

#### Issue: Arabic text shows as "???"

**Solution:**
- Verify resource file has UTF-8 encoding
- Check console supports UTF-8: `Console.OutputEncoding = System.Text.Encoding.UTF8;`

#### Issue: Tests fail on specific culture settings

**Solution:**
- Tests explicitly set culture
- Reset culture in TearDown methods
- Ensure thread culture is set correctly

---

## Adding New Tests

### Template for New Test

```csharp
[Test]
[Description("Clear description of what this tests")]
public void ComponentName_Scenario_ExpectedResult()
{
    // Arrange
    _languageManager.LoadLanguage(SupportedLanguage.English);
    var input = "test data";

    // Act
    var result = _languageManager.SomeMethod(input);

    // Assert
    result.Should().Be("expected output");
}
```

### Best Practices

1. **One assertion per concept** - Keep tests focused
2. **Clear naming** - ComponentName_Scenario_ExpectedResult
3. **Arrange-Act-Assert** - Follow AAA pattern
4. **Independent tests** - Each test should work standalone
5. **Clean up** - Reset state in TearDown
6. **Descriptive errors** - Use descriptive assertion messages

---

## Test Metrics

### Target Metrics

- ✅ **Code Coverage:** > 95%
- ✅ **Success Rate:** 100%
- ✅ **Performance:** All tests < 5 seconds total
- ✅ **Maintainability:** Clear, readable tests

### Actual Results (Expected)

- **Total Tests:** 150+
- **Test Coverage:** Core functionality, UI, data, edge cases
- **Performance:** < 3 seconds for full suite
- **Reliability:** Deterministic, repeatable results

---

## Validation Checklist

Before deploying to production, verify:

- [ ] All 150+ tests pass
- [ ] No missing translations (500+ keys in both languages)
- [ ] RTL layout works correctly for Arabic
- [ ] Date/number/currency formatting is culture-aware
- [ ] Language switching works without crashes
- [ ] Performance is acceptable
- [ ] Resource files are valid XML with UTF-8 encoding
- [ ] All UI elements are translated
- [ ] Messages and validation work in both languages
- [ ] Reports and forms localize correctly

---

## Support

### For Test Failures

1. Review the HTML test report
2. Check the console output for specific errors
3. Run individual test files to isolate issues
4. Verify resource files are up to date
5. Check that all NuGet packages are installed

### For Adding New Tests

1. Follow the template above
2. Add tests to appropriate test file
3. Run tests locally before committing
4. Update this documentation

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-06 | Initial test suite with 150+ tests |
| | | - Core localization tests |
| | | - Formatting tests |
| | | - Resource validation tests |
| | | - RTL layout tests |
| | | - Integration tests |
| | | - Custom test runner |
| | | - HTML reporting |

---

## Summary

This comprehensive test suite ensures the multi-language implementation is:

✅ **Correct** - All translations work as expected
✅ **Complete** - No missing translations or features
✅ **Robust** - Handles edge cases and errors gracefully
✅ **Performant** - Fast enough for production use
✅ **Reliable** - Works consistently without crashes

**When all tests pass, the system is confirmed to be working correctly without bugs or crashes and is ready for production deployment.**

---

**© 2025 Contract Management Add-on - All Rights Reserved**
