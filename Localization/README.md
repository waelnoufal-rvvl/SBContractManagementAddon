# Multi-Language Support

## Contract Management Add-on Localization

### Supported Languages

- **English (en-US)** - Left-to-Right (LTR)
- **Arabic (ar-SA)** - Right-to-Left (RTL) - العربية

---

## Quick Start

### 1. Initialize Language Manager

```csharp
using ContractManagementAddon.Localization;

// Initialize with English
LanguageManager.Instance.LoadLanguage(SupportedLanguage.English);

// Or with Arabic
LanguageManager.Instance.LoadLanguage(SupportedLanguage.Arabic);
```

### 2. Get Localized Strings

```csharp
var lang = LanguageManager.Instance;

// Simple string
string saveText = lang.GetString("Common_Save");
// English: "Save"
// Arabic: "حفظ"

// String with parameters
string errorMsg = lang.GetString("Msg_RequiredField", "Contract Name");
// English: "Contract Name is required."
// Arabic: "اسم العقد مطلوب."
```

### 3. Localize Forms

```csharp
// Apply localization to SAP B1 form
lang.LocalizeForm(form);

// For buttons and labels
button.Caption = lang.GetString("Common_Save");
label.Caption = lang.GetString("Contract_Code");
```

### 4. Format Dates and Numbers

```csharp
// Dates
string date = lang.FormatDate(DateTime.Now, "short");
// English: "01/15/2025"
// Arabic: "١٥‏/٠١‏/٢٠٢٥"

// Numbers
string number = lang.FormatNumber(1234.56, 2);
// English: "1,234.56"
// Arabic: "١٬٢٣٤٫٥٦"

// Currency
string amount = lang.FormatCurrency(100000, "USD");
// English: "$100,000.00"
// Arabic: "100,000.00 $"
```

---

## File Structure

```
Localization/
├── LanguageManager.cs              # Core localization engine
├── Resources/
│   ├── Resources.English.xml       # English translations (500+ strings)
│   ├── Resources.Arabic.xml        # Arabic translations (500+ strings)
│   └── MissingResources.log        # Log of missing translation keys
└── README.md                        # This file
```

---

## Key Features

### 1. Automatic RTL Support

Arabic language automatically applies Right-to-Left layout:

```csharp
if (lang.IsRightToLeft)
{
    // Form items are automatically mirrored
    lang.LocalizeForm(form);
}
```

### 2. Culture-Aware Formatting

All dates, numbers, and currencies automatically formatted according to current culture:

```csharp
// Current culture is set based on selected language
Thread.CurrentThread.CurrentCulture = lang.CurrentCulture;
Thread.CurrentThread.CurrentUICulture = lang.CurrentCulture;
```

### 3. Language Switching

Users can switch languages at runtime:

```csharp
// Switch to next language
lang.SwitchLanguage();

// Or select specific language
lang.LoadLanguage(SupportedLanguage.Arabic);

// Subscribe to language change event
lang.LanguageChanged += (sender, e) => {
    // Reload forms with new language
};
```

---

## Adding New Translations

### 1. Add to English Resource File

Edit `Resources/Resources.English.xml`:

```xml
<String Key="NewFeature_Title">New Feature</String>
<String Key="NewFeature_Description">This is a new feature</String>
```

### 2. Add to Arabic Resource File

Edit `Resources/Resources.Arabic.xml`:

```xml
<String Key="NewFeature_Title">ميزة جديدة</String>
<String Key="NewFeature_Description">هذه ميزة جديدة</String>
```

### 3. Use in Code

```csharp
string title = lang.GetString("NewFeature_Title");
string description = lang.GetString("NewFeature_Description");
```

---

## Translation Guidelines

### Naming Convention

Use hierarchical key names:

```
[Category]_[Control]_[Property]

Examples:
- Common_Save
- Contract_Name
- IPC_GrossAmount
- Msg_SaveSuccess
- Val_ContractCodeRequired
```

### Categories

- **Common_** - Common buttons and labels
- **Menu_** - Menu items
- **Form_** - Form titles
- **Contract_** - Contract-related
- **IPC_** - IPC-related
- **CO_** - Change Order-related
- **Revenue_** - Revenue Recognition
- **Currency_** - Currency and exchange rates
- **Status_** - Status values
- **Msg_** - Messages
- **Val_** - Validation messages
- **Report_** - Reports
- **Settings_** - Settings
- **Help_** - Help and About
- **Btn_** - Buttons
- **Tip_** - Tooltips
- **Label_** - Labels

---

## RTL (Right-to-Left) Considerations

### What Happens Automatically

When Arabic is selected:
1. Form items are mirrored horizontally
2. Text alignment changes to right
3. Buttons swap positions
4. Matrix columns align to right

### What Needs Manual Handling

```csharp
// Mixed LTR/RTL text (English names in Arabic interface)
string mixedText = $"{arabicLabel}: {englishValue}";

// Custom controls
if (lang.IsRightToLeft)
{
    customControl.RightToLeft = RightToLeft.Yes;
}
```

---

## Performance Tips

### 1. Cache Frequently Used Strings

```csharp
private string _saveText;
private string _cancelText;

public void Initialize()
{
    _saveText = lang.GetString("Common_Save");
    _cancelText = lang.GetString("Common_Cancel");
}
```

### 2. Localize Only When Needed

```csharp
// Localize on form load, not on every refresh
private bool _isLocalized = false;

public void LocalizeForm()
{
    if (!_isLocalized)
    {
        LocalizeControls();
        _isLocalized = true;
    }
}
```

---

## Common Issues and Solutions

### Issue: Text Shows as [KeyName]

**Cause:** Translation key not found

**Solution:** Add key to both resource files or check spelling

### Issue: Arabic Shows as "???"

**Cause:** Font doesn't support Arabic

**Solution:** Ensure font supports Unicode (Arial, Tahoma)

### Issue: RTL Layout Not Working

**Cause:** LocalizeForm() not called

**Solution:**
```csharp
if (lang.IsRightToLeft)
{
    lang.LocalizeForm(form);
}
```

### Issue: Dates Not Formatting

**Cause:** Using ToString() instead of FormatDate()

**Solution:**
```csharp
// ❌ Wrong
label.Caption = date.ToString();

// ✅ Correct
label.Caption = lang.FormatDate(date, "short");
```

---

## Available Methods

### LanguageManager Methods

| Method | Description | Example |
|--------|-------------|---------|
| `LoadLanguage(language)` | Load specific language | `LoadLanguage(SupportedLanguage.Arabic)` |
| `GetString(key)` | Get translated string | `GetString("Common_Save")` |
| `GetString(key, args)` | Get string with parameters | `GetString("Msg_Error", errorMessage)` |
| `SwitchLanguage()` | Toggle between languages | `SwitchLanguage()` |
| `LocalizeForm(form)` | Auto-localize SAP form | `LocalizeForm(myForm)` |
| `FormatDate(date, format)` | Format date by culture | `FormatDate(DateTime.Now, "short")` |
| `FormatNumber(number, decimals)` | Format number by culture | `FormatNumber(1234.56, 2)` |
| `FormatCurrency(amount, code)` | Format currency | `FormatCurrency(1000, "USD")` |
| `GetAvailableLanguages()` | Get all languages | `GetAvailableLanguages()` |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentLanguage` | `SupportedLanguage` | Currently active language |
| `CurrentCulture` | `CultureInfo` | Current culture info |
| `IsRightToLeft` | `bool` | True if Arabic selected |

---

## Example Usage

### Complete Form Example

See `Forms/ContractFormLocalized.cs` for full implementation example.

### Basic Implementation

```csharp
using ContractManagementAddon.Localization;

public class MyForm
{
    private LanguageManager _lang = LanguageManager.Instance;

    public void CreateForm()
    {
        // Set form title
        form.Title = _lang.GetString("Form_Contract_Title");

        // Localize buttons
        btnSave.Caption = _lang.GetString("Common_Save");
        btnCancel.Caption = _lang.GetString("Common_Cancel");

        // Display data with formatting
        lblDate.Caption = _lang.FormatDate(contract.StartDate);
        lblAmount.Caption = _lang.FormatCurrency(contract.TotalValue);

        // Apply RTL if needed
        if (_lang.IsRightToLeft)
        {
            _lang.LocalizeForm(form);
        }
    }
}
```

---

## Documentation

For complete documentation, see:
- **[Localization Implementation Guide](../Documentation/LOCALIZATION_IMPLEMENTATION_GUIDE.md)** - Complete developer guide
- **[User Help Guide](../Documentation/USER_HELP_GUIDE.md)** - End-user documentation

---

## Support

**Questions or Issues?**
- Email: localization@company.com
- Documentation: See guides in Documentation folder
- Technical Support: support@company.com

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-01-06 | Initial release with English and Arabic |

---

**© 2025 Contract Management Add-on - All Rights Reserved**
