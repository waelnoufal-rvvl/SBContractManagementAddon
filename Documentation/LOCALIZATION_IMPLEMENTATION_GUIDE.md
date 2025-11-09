# Localization Implementation Guide
## Multi-Language Support for Contract Management Add-on
**Version:** 1.0.0
**Date:** January 2025
**Supported Languages:** English, Arabic (العربية)

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Setup and Configuration](#setup-and-configuration)
4. [Using Localization in Code](#using-localization-in-code)
5. [RTL (Right-to-Left) Support](#rtl-support)
6. [Adding New Languages](#adding-new-languages)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The Contract Management Add-on supports multi-language user interface with:

- **Supported Languages:** English (en-US), Arabic (ar-SA)
- **Direction Support:** LTR (Left-to-Right) and RTL (Right-to-Left)
- **Features:**
  - Dynamic language switching
  - Culture-aware date and number formatting
  - Currency formatting per language
  - Complete translation of all UI elements
  - RTL layout support for Arabic

---

## Architecture

### Components

```
Localization/
├── LanguageManager.cs          # Core localization manager
├── Resources/
│   ├── Resources.English.xml   # English translations
│   └── Resources.Arabic.xml    # Arabic translations
└── README.md                    # This file
```

### Language Manager Class Diagram

```
┌────────────────────────────────┐
│    LanguageManager             │
│    (Singleton)                 │
├────────────────────────────────┤
│  Properties:                   │
│  - CurrentLanguage             │
│  - CurrentCulture              │
│  - IsRightToLeft               │
├────────────────────────────────┤
│  Methods:                      │
│  + LoadLanguage()              │
│  + GetString()                 │
│  + SwitchLanguage()            │
│  + LocalizeForm()              │
│  + FormatDate()                │
│  + FormatNumber()              │
│  + FormatCurrency()            │
└────────────────────────────────┘
```

### Resource File Format

XML-based resource files:

```xml
<Resources>
    <Metadata>
        <Language>English</Language>
        <LanguageCode>en-US</LanguageCode>
        <Direction>LTR</Direction>
    </Metadata>
    <Strings>
        <String Key="Common_Save">Save</String>
        <String Key="Common_Cancel">Cancel</String>
        <!-- ... more strings -->
    </Strings>
</Resources>
```

---

## Setup and Configuration

### 1. Initial Setup

**Step 1: Copy Files**

Ensure the following files are in your project:

```
ContractManagementAddon/
├── Localization/
│   ├── LanguageManager.cs
│   └── Resources/
│       ├── Resources.English.xml
│       └── Resources.Arabic.xml
```

**Step 2: Add Reference**

Add to your Add-on initialization code:

```csharp
using ContractManagementAddon.Localization;

public class AddonInitialization
{
    public void Initialize()
    {
        // Initialize Language Manager with default language
        LanguageManager.Instance.LoadLanguage(SupportedLanguage.English);

        // Subscribe to language change events (optional)
        LanguageManager.Instance.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
    {
        // Reload all open forms
        ReloadAllForms();

        // Show notification
        var msg = LanguageManager.Instance.GetString(
            "Msg_LanguageChanged",
            LanguageManager.Instance.GetString($"Language_{e.NewLanguage}")
        );
        Application.ShowMessageBox(msg);
    }
}
```

### 2. Load Language from User Preference

```csharp
public void LoadUserLanguage(string userId)
{
    // Get user's preferred language from database
    var userLang = GetUserPreferredLanguage(userId);

    // Load language
    switch (userLang)
    {
        case "ar-SA":
            LanguageManager.Instance.LoadLanguage(SupportedLanguage.Arabic);
            break;
        case "en-US":
        default:
            LanguageManager.Instance.LoadLanguage(SupportedLanguage.English);
            break;
    }
}
```

### 3. Save Language Preference

```csharp
public void SaveUserLanguagePreference(string userId, SupportedLanguage language)
{
    // Save to user-defined field or custom table
    var langCode = language == SupportedLanguage.Arabic ? "ar-SA" : "en-US";

    var sql = $@"
        UPDATE [@CM_USER_PREFS]
        SET U_Language = '{langCode}'
        WHERE U_UserID = '{userId}'";

    // Execute query
    ExecuteNonQuery(sql);
}
```

---

## Using Localization in Code

### 1. Localizing Forms

**Example: Contract Form**

```csharp
using ContractManagementAddon.Localization;
using SAPbouiCOM;

public class ContractForm
{
    private Form _form;
    private LanguageManager _lang = LanguageManager.Instance;

    public void CreateForm()
    {
        // Create SAP B1 form
        _form = SAPbouiCOM.Application.Forms.Item("frmContract");

        // Set form title
        _form.Title = _lang.GetString("Form_Contract_Title");

        // Localize all controls
        LocalizeControls();

        // Apply RTL layout if Arabic
        if (_lang.IsRightToLeft)
        {
            _lang.LocalizeForm(_form);
        }
    }

    private void LocalizeControls()
    {
        // Localize buttons
        var btnSave = (Button)_form.Items.Item("btnSave").Specific;
        btnSave.Caption = _lang.GetString("Common_Save");

        var btnCancel = (Button)_form.Items.Item("btnCancel").Specific;
        btnCancel.Caption = _lang.GetString("Common_Cancel");

        // Localize labels
        var lblContractCode = (StaticText)_form.Items.Item("lblCode").Specific;
        lblContractCode.Caption = _lang.GetString("Contract_Code");

        var lblContractName = (StaticText)_form.Items.Item("lblName").Specific;
        lblContractName.Caption = _lang.GetString("Contract_Name");

        var lblCustomer = (StaticText)_form.Items.Item("lblCustomer").Specific;
        lblCustomer.Caption = _lang.GetString("Contract_Customer");

        // Localize combo box values
        LocalizeContractTypeCombo();
    }

    private void LocalizeContractTypeCombo()
    {
        var cboType = (ComboBox)_form.Items.Item("cboType").Specific;

        // Clear existing items
        while (cboType.ValidValues.Count > 0)
        {
            cboType.ValidValues.Remove(0, BoSearchKey.psk_Index);
        }

        // Add localized items
        cboType.ValidValues.Add("FP", _lang.GetString("Contract_Type_FixedPrice"));
        cboType.ValidValues.Add("TM", _lang.GetString("Contract_Type_TimeAndMaterials"));
        cboType.ValidValues.Add("CP", _lang.GetString("Contract_Type_CostPlus"));
    }
}
```

### 2. Localizing Messages

```csharp
// Simple message
var message = _lang.GetString("Msg_SaveSuccess");
Application.ShowMessageBox(message);

// Message with parameters
var fieldName = _lang.GetString("Contract_Name");
var errorMsg = _lang.GetString("Msg_RequiredField", fieldName);
Application.ShowMessageBox(errorMsg);
// Displays: "Contract Name is required." (English)
// Displays: "اسم العقد مطلوب." (Arabic)

// Multiple parameters
var from = _lang.FormatDate(startDate);
var to = _lang.FormatDate(endDate);
var periodMsg = $"{_lang.GetString("Label_From")} {from} {_lang.GetString("Label_To")} {to}";
```

### 3. Formatting Dates

```csharp
var contract = GetContract("CNT-001");

// Short date format (culture-aware)
var shortDate = _lang.FormatDate(contract.StartDate, "short");
// English: "01/15/2025"
// Arabic: "١٥‏/٠١‏/٢٠٢٥"

// Long date format
var longDate = _lang.FormatDate(contract.StartDate, "long");
// English: "Wednesday, January 15, 2025"
// Arabic: "الأربعاء، 15 يناير 2025"

// Date-time format
var dateTime = _lang.FormatDate(DateTime.Now, "datetime");
// English: "01/15/2025 3:30 PM"
// Arabic: "١٥‏/٠١‏/٢٠٢٥ ٣:٣٠ م"

// Custom format
var custom = _lang.FormatDate(contract.StartDate, "dd-MMM-yyyy");
// English: "15-Jan-2025"
// Arabic: "15-ينا-2025"
```

### 4. Formatting Numbers

```csharp
var amount = 1234567.89;

// Number with 2 decimals
var formatted = _lang.FormatNumber(amount, 2);
// English: "1,234,567.89"
// Arabic: "١٬٢٣٤٬٥٦٧٫٨٩"

// Number with 0 decimals
var whole = _lang.FormatNumber(amount, 0);
// English: "1,234,568"
// Arabic: "١٬٢٣٤٬٥٦٨"
```

### 5. Formatting Currency

```csharp
var contractValue = 100000.00;

// Using system currency
var systemCurrency = _lang.FormatCurrency(contractValue);
// English: "$100,000.00"
// Arabic: "100,000.00 $"

// Specific currency
var usd = _lang.FormatCurrency(contractValue, "USD");
// English: "$100,000.00"
// Arabic: "100,000.00 $"

var sar = _lang.FormatCurrency(contractValue, "SAR");
// English: "ر.س100,000.00"
// Arabic: "100,000.00 ر.س"

var eur = _lang.FormatCurrency(contractValue, "EUR");
// English: "€100,000.00"
// Arabic: "100,000.00 €"
```

### 6. Language Switching

**Option 1: Language Selection Menu**

```csharp
public void CreateLanguageMenu()
{
    // Get SAP menu
    var menu = Application.Menus.Item("43520"); // Add-ons menu

    // Create language submenu
    var langMenu = menu.SubMenus.Add("mnuLang", "Language / اللغة", BoMenuType.mt_POPUP);

    // Add language options
    langMenu.SubMenus.Add("mnuLangEN", "English", BoMenuType.mt_STRING);
    langMenu.SubMenus.Add("mnuLangAR", "العربية", BoMenuType.mt_STRING);
}

public void HandleMenuClick(string menuId)
{
    switch (menuId)
    {
        case "mnuLangEN":
            LanguageManager.Instance.LoadLanguage(SupportedLanguage.English);
            ReloadCurrentForm();
            break;

        case "mnuLangAR":
            LanguageManager.Instance.LoadLanguage(SupportedLanguage.Arabic);
            ReloadCurrentForm();
            break;
    }
}
```

**Option 2: Language Toggle Button**

```csharp
public void AddLanguageToggleButton(Form form)
{
    // Add button to form
    var btnLang = form.Items.Add("btnLang", BoFormItemTypes.it_BUTTON);
    btnLang.Left = form.Width - 100;
    btnLang.Top = 5;
    btnLang.Width = 60;
    btnLang.Height = 19;

    var button = (Button)btnLang.Specific;
    button.Caption = _lang.CurrentLanguage == SupportedLanguage.English ? "عربي" : "EN";
}

private void BtnLang_PressedAfter(object sender, SBOItemEventArg e)
{
    // Toggle language
    LanguageManager.Instance.SwitchLanguage();

    // Reload form
    ReloadForm();
}
```

---

## RTL (Right-to-Left) Support

### Understanding RTL

Arabic is a Right-to-Left language, meaning:
- Text flows from right to left
- UI elements should mirror horizontally
- Numbers and English text remain LTR within RTL context

### Automatic RTL Layout

The `LanguageManager.LocalizeForm()` method automatically handles RTL:

```csharp
if (_lang.IsRightToLeft)
{
    _lang.LocalizeForm(_form);
}
```

This performs:
1. Mirrors all form items horizontally
2. Adjusts text alignment
3. Repositions buttons and controls

### Manual RTL Adjustments

For complex layouts, manual adjustments may be needed:

```csharp
private void ApplyRTL Layout()
{
    if (!_lang.IsRightToLeft)
        return;

    var formWidth = _form.Width;

    // Mirror buttons
    var btnSave = _form.Items.Item("btnSave");
    var btnCancel = _form.Items.Item("btnCancel");

    // Swap positions
    var saveLeft = btnSave.Left;
    btnSave.Left = formWidth - btnCancel.Left - btnCancel.Width;
    btnCancel.Left = formWidth - saveLeft - btnSave.Width;

    // Align matrix columns to right
    var matrix = (Matrix)_form.Items.Item("mtxLines").Specific;
    for (int i = 1; i <= matrix.Columns.Count; i++)
    {
        var column = matrix.Columns.Item(i);
        if (column.Type == BoFormItemTypes.it_EDIT ||
            column.Type == BoFormItemTypes.it_LINKED_BUTTON)
        {
            column.RightJustified = true;
        }
    }
}
```

### RTL Text Direction

For multiline text boxes:

```csharp
private void SetTextDirection(EditText editText)
{
    if (_lang.IsRightToLeft)
    {
        // SAP B1 doesn't have built-in RTL property
        // Use Windows API if needed
        SetWindowLong(editText.Handle, GWL_EXSTYLE,
            GetWindowLong(editText.Handle, GWL_EXSTYLE) | WS_EX_RTLREADING);
    }
}
```

---

## Adding New Languages

### Step 1: Create Resource File

Create new XML file: `Resources.[Language].xml`

Example: `Resources.French.xml`

```xml
<?xml version="1.0" encoding="utf-8"?>
<Resources>
    <Metadata>
        <Language>French</Language>
        <LanguageCode>fr-FR</LanguageCode>
        <Version>1.0.0</Version>
        <LastUpdated>2025-01-06</LastUpdated>
        <Direction>LTR</Direction>
    </Metadata>
    <Strings>
        <String Key="Common_Save">Enregistrer</String>
        <String Key="Common_Cancel">Annuler</String>
        <!-- ... translate all keys -->
    </Strings>
</Resources>
```

### Step 2: Update SupportedLanguage Enum

```csharp
public enum SupportedLanguage
{
    English,
    Arabic,
    French  // Add new language
}
```

### Step 3: Update LoadLanguage Method

```csharp
public void LoadLanguage(SupportedLanguage language)
{
    // ... existing code ...

    switch (language)
    {
        case SupportedLanguage.English:
            CurrentCulture = new CultureInfo("en-US");
            break;
        case SupportedLanguage.Arabic:
            CurrentCulture = new CultureInfo("ar-SA");
            break;
        case SupportedLanguage.French:
            CurrentCulture = new CultureInfo("fr-FR");
            break;
        default:
            CurrentCulture = CultureInfo.InvariantCulture;
            break;
    }

    // ... rest of code ...
}
```

### Step 4: Update GetAvailableLanguages Method

```csharp
public Dictionary<SupportedLanguage, string> GetAvailableLanguages()
{
    return new Dictionary<SupportedLanguage, string>
    {
        { SupportedLanguage.English, "English" },
        { SupportedLanguage.Arabic, "العربية" },
        { SupportedLanguage.French, "Français" }
    };
}
```

---

## Best Practices

### 1. Always Use Keys, Never Hardcode Strings

❌ **Bad:**
```csharp
button.Caption = "Save";
ShowMessage("Record saved successfully");
```

✅ **Good:**
```csharp
button.Caption = _lang.GetString("Common_Save");
ShowMessage(_lang.GetString("Msg_SaveSuccess"));
```

### 2. Provide Default Values

```csharp
// If key doesn't exist, show meaningful default
var label = _lang.GetString("NewFeature_Label", "New Feature");
```

### 3. Use Descriptive Keys

❌ **Bad:**
```csharp
<String Key="Btn1">Save</String>
<String Key="Msg1">Error</String>
```

✅ **Good:**
```csharp
<String Key="Btn_SaveContract">Save Contract</String>
<String Key="Msg_SaveContractError">Error saving contract</String>
```

### 4. Group Related Keys

```csharp
<!-- Contract Form -->
<String Key="Contract_Code">Contract Code</String>
<String Key="Contract_Name">Contract Name</String>
<String Key="Contract_Customer">Customer</String>

<!-- IPC Form -->
<String Key="IPC_Number">IPC Number</String>
<String Key="IPC_Date">IPC Date</String>
```

### 5. Handle Plurals Appropriately

Different languages have different plural rules:

```csharp
// English
<String Key="Label_Items_Singular">{0} item</String>
<String Key="Label_Items_Plural">{0} items</String>

// Arabic has more plural forms
<String Key="Label_Items_None">لا توجد عناصر</String>
<String Key="Label_Items_Single">عنصر واحد</String>
<String Key="Label_Items_Two">عنصران</String>
<String Key="Label_Items_Few">{0} عناصر</String>
<String Key="Label_Items_Many">{0} عنصراً</String>
```

```csharp
public string GetItemCountLabel(int count)
{
    if (_lang.CurrentLanguage == SupportedLanguage.Arabic)
    {
        if (count == 0)
            return _lang.GetString("Label_Items_None");
        else if (count == 1)
            return _lang.GetString("Label_Items_Single");
        else if (count == 2)
            return _lang.GetString("Label_Items_Two");
        else if (count >= 3 && count <= 10)
            return _lang.GetString("Label_Items_Few", count);
        else
            return _lang.GetString("Label_Items_Many", count);
    }
    else
    {
        return count == 1
            ? _lang.GetString("Label_Items_Singular", count)
            : _lang.GetString("Label_Items_Plural", count);
    }
}
```

### 6. Test with Longest Language

Some languages are longer than others. Test UI with German or Arabic which tend to be verbose:

```csharp
// English: "Save" (4 chars)
// German: "Speichern" (10 chars)
// Arabic: "حفظ" (3 chars but wider glyphs)
```

Ensure buttons and labels have adequate width.

### 7. Culture-Aware Sorting

```csharp
// Sort customer names according to culture
var customers = GetCustomers();
customers.Sort((a, b) =>
    string.Compare(a.Name, b.Name, _lang.CurrentCulture, CompareOptions.None));
```

### 8. Store Data in Neutral Format

In database, always store data in neutral format:

```csharp
// ✅ Store dates as ISO format
contractDTO.StartDate = contract.StartDate.ToString("yyyy-MM-dd");

// ✅ Store numbers without formatting
contractDTO.TotalValue = contract.TotalValue.ToString(CultureInfo.InvariantCulture);

// ✅ Format only for display
lblStartDate.Caption = _lang.FormatDate(contract.StartDate);
lblTotal.Caption = _lang.FormatCurrency(contract.TotalValue);
```

---

## Troubleshooting

### Problem: Resource File Not Found

**Error:** `FileNotFoundException: Language resource file not found`

**Solution:**
1. Verify file path:
   ```
   [Add-on Directory]/Localization/Resources/Resources.English.xml
   ```
2. Check file is set to "Copy to Output Directory"
3. Verify file name matches exactly (case-sensitive)

### Problem: Missing Translation Keys

**Symptom:** Text shows as `[KeyName]` instead of translated text

**Solution:**
1. Check `MissingResources.log` file in Resources folder
2. Add missing keys to resource file:
   ```xml
   <String Key="KeyName">Translated Text</String>
   ```
3. Reload language

### Problem: RTL Layout Not Working

**Symptom:** Arabic text shows LTR or form layout incorrect

**Solution:**
1. Ensure `LocalizeForm()` is called:
   ```csharp
   if (_lang.IsRightToLeft)
   {
       _lang.LocalizeForm(_form);
   }
   ```

2. Check metadata in resource file:
   ```xml
   <Direction>RTL</Direction>
   ```

3. For custom controls, manually set RTL properties

### Problem: Date/Number Format Incorrect

**Symptom:** Dates show in wrong format or with wrong calendar

**Solution:**
1. Always use `FormatDate()` and `FormatNumber()`:
   ```csharp
   // ❌ Don't use
   label.Caption = date.ToString();

   // ✅ Use
   label.Caption = _lang.FormatDate(date);
   ```

2. Check culture is set correctly:
   ```csharp
   Debug.WriteLine($"Current Culture: {_lang.CurrentCulture.Name}");
   ```

### Problem: Language Change Not Reflected

**Symptom:** Language changes but form still shows old language

**Solution:**
1. Forms must be reloaded after language change:
   ```csharp
   private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
   {
       // Close and reopen form
       _form.Close();
       CreateForm();
   }
   ```

2. Or refresh all controls:
   ```csharp
   private void RefreshLocalization()
   {
       LocalizeControls();
       _form.Refresh();
   }
   ```

### Problem: Arabic Text Shows as Question Marks (???)

**Symptom:** Arabic characters don't display correctly

**Solution:**
1. Ensure XML file encoding is UTF-8:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   ```

2. Verify SAP B1 supports Arabic fonts
3. Check Windows has Arabic language pack installed
4. Use Unicode fonts (Arial, Tahoma, etc.)

### Problem: Performance Issues with Large Forms

**Symptom:** Form loads slowly when localizing

**Solution:**
1. Cache localized strings:
   ```csharp
   private Dictionary<string, string> _cachedStrings = new Dictionary<string, string>();

   private string GetCachedString(string key)
   {
       if (!_cachedStrings.ContainsKey(key))
       {
           _cachedStrings[key] = _lang.GetString(key);
       }
       return _cachedStrings[key];
   }
   ```

2. Localize only visible items:
   ```csharp
   private void Form_VisibleAfter(object sender, SBOItemEventArg e)
   {
       if (e.FormUID == _form.UniqueID && !_isLocalized)
       {
           LocalizeControls();
           _isLocalized = true;
       }
   }
   ```

---

## Testing Checklist

Before deploying localization:

### Functional Testing
- [ ] All forms display in both languages
- [ ] Language switching works correctly
- [ ] All buttons and labels are translated
- [ ] Combo box values are localized
- [ ] Messages and errors show in correct language
- [ ] Reports display in selected language

### RTL Testing (Arabic)
- [ ] Form layout mirrors correctly
- [ ] Text alignment is right-aligned
- [ ] Buttons are in correct positions
- [ ] Matrix columns are right-aligned
- [ ] Mixed LTR/RTL text displays correctly

### Format Testing
- [ ] Dates display in correct format
- [ ] Numbers use correct separators
- [ ] Currency symbols are positioned correctly
- [ ] Percentages format properly

### Data Integrity
- [ ] Database values remain unchanged
- [ ] Saved data is language-independent
- [ ] Reports can be generated in either language
- [ ] Export/Import works regardless of language

### Performance Testing
- [ ] Forms load in reasonable time (< 3 seconds)
- [ ] Language switching is responsive (< 1 second)
- [ ] No memory leaks with language switching
- [ ] Large lists/grids perform well

---

## Additional Resources

### Language Codes Reference

| Language | Code | Culture | Direction |
|----------|------|---------|-----------|
| English (US) | en-US | English (United States) | LTR |
| Arabic (Saudi Arabia) | ar-SA | العربية (المملكة العربية السعودية) | RTL |
| French (France) | fr-FR | Français (France) | LTR |
| German (Germany) | de-DE | Deutsch (Deutschland) | LTR |
| Spanish (Spain) | es-ES | Español (España) | LTR |

### Currency Symbols

| Currency | Code | English Symbol | Arabic Symbol |
|----------|------|----------------|---------------|
| US Dollar | USD | $ | $ |
| Euro | EUR | € | € |
| British Pound | GBP | £ | £ |
| Saudi Riyal | SAR | SR | ر.س |
| UAE Dirham | AED | AED | د.إ |
| Egyptian Pound | EGP | E£ | ج.م |

### Useful Links

- [.NET Globalization](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/)
- [CultureInfo Class](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)
- [Arabic Language Support](https://docs.microsoft.com/en-us/globalization/localizability/arabic)
- [SAP Business One SDK](https://help.sap.com/sdk)

---

## Support

For localization issues or questions:

**Email:** localization@company.com
**Documentation:** [User Help Guide](USER_HELP_GUIDE.md)
**Technical Support:** support@company.com

---

**Document Version:** 1.0.0
**Last Updated:** January 2025
**Next Review:** March 2025
