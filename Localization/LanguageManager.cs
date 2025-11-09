using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using SAPbouiCOM;

namespace ContractManagementAddon.Localization
{
    /// <summary>
    /// Manages localization and language switching for the Contract Management Add-on
    /// Supports English (LTR) and Arabic (RTL) languages
    /// </summary>
    public class LanguageManager
    {
        #region Singleton Pattern

        private static LanguageManager _instance;
        private static readonly object _lock = new object();

        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LanguageManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current active language
        /// </summary>
        public SupportedLanguage CurrentLanguage { get; private set; }

        /// <summary>
        /// Current culture info
        /// </summary>
        public CultureInfo CurrentCulture { get; private set; }

        /// <summary>
        /// Indicates if current language is Right-to-Left
        /// </summary>
        public bool IsRightToLeft => CurrentLanguage == SupportedLanguage.Arabic;

        /// <summary>
        /// Language resources dictionary
        /// </summary>
        private Dictionary<string, string> _resources;

        /// <summary>
        /// Path to language resource files
        /// </summary>
        private string _resourcePath;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when language changes
        /// </summary>
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        #endregion

        #region Constructor

        private LanguageManager()
        {
            _resources = new Dictionary<string, string>();
            _resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization", "Resources");

            // Initialize with default language (English)
            LoadLanguage(SupportedLanguage.English);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load and activate a language
        /// </summary>
        /// <param name="language">Language to load</param>
        public void LoadLanguage(SupportedLanguage language)
        {
            try
            {
                // Clear existing resources
                _resources.Clear();

                // Set current language
                var previousLanguage = CurrentLanguage;
                CurrentLanguage = language;

                // Set culture info
                switch (language)
                {
                    case SupportedLanguage.English:
                        CurrentCulture = new CultureInfo("en-US");
                        break;
                    case SupportedLanguage.Arabic:
                        CurrentCulture = new CultureInfo("ar-SA");
                        break;
                    default:
                        CurrentCulture = CultureInfo.InvariantCulture;
                        break;
                }

                // Set thread culture
                Thread.CurrentThread.CurrentCulture = CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = CurrentCulture;

                // Load language resources
                LoadResourceFile(language);

                // Fire language changed event
                if (previousLanguage != language)
                {
                    OnLanguageChanged(new LanguageChangedEventArgs(previousLanguage, CurrentLanguage));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load language: {language}", ex);
            }
        }

        /// <summary>
        /// Get localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public string GetString(string key)
        {
            return GetStringWithDefault(key, null);
        }

        /// <summary>
        /// Get localized string by key with default value
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Localized string</returns>
        public string GetStringWithDefault(string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue ?? string.Empty;

            if (_resources.ContainsKey(key))
                return _resources[key];

            // Log missing resource
            LogMissingResource(key);

            return defaultValue ?? $"[{key}]";
        }

        /// <summary>
        /// Get localized string by key with default value (compatibility overload)
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Localized string</returns>
        public string GetString(string key, string defaultValue)
        {
            return GetStringWithDefault(key, defaultValue);
        }

        /// <summary>
        /// Get localized string with parameters
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);

            if (args == null || args.Length == 0)
                return format;

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        /// <summary>
        /// Switch to next available language
        /// </summary>
        public void SwitchLanguage()
        {
            var languages = Enum.GetValues(typeof(SupportedLanguage)).Cast<SupportedLanguage>().ToArray();
            var currentIndex = Array.IndexOf(languages, CurrentLanguage);
            var nextIndex = (currentIndex + 1) % languages.Length;

            LoadLanguage(languages[nextIndex]);
        }

        /// <summary>
        /// Get all available languages
        /// </summary>
        /// <returns>Dictionary of language code and display name</returns>
        public Dictionary<SupportedLanguage, string> GetAvailableLanguages()
        {
            return new Dictionary<SupportedLanguage, string>
            {
                { SupportedLanguage.English, "English" },
                { SupportedLanguage.Arabic, "العربية" }
            };
        }

        /// <summary>
        /// Apply localization to SAP B1 form
        /// </summary>
        /// <param name="form">SAP B1 form</param>
        public void LocalizeForm(SAPbouiCOM.Form form)
        {
            if (form == null)
                return;

            try
            {
                // Set form direction for RTL
                if (IsRightToLeft)
                {
                    SetFormRightToLeft(form);
                }

                // Localize form title
                var formTitleKey = $"Form_{form.TypeEx}_Title";
                var title = GetStringWithDefault(formTitleKey, form.Title);
                if (title != form.Title)
                {
                    form.Title = title;
                }

                // Localize all items on form
                LocalizeFormItems(form);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error localizing form: {ex.Message}");
            }
        }

        /// <summary>
        /// Format date according to current culture
        /// </summary>
        /// <param name="date">Date to format</param>
        /// <param name="format">Date format (short, long, custom)</param>
        /// <returns>Formatted date string</returns>
        public string FormatDate(DateTime date, string format = "short")
        {
            switch (format.ToLower())
            {
                case "short":
                    return date.ToString("d", CurrentCulture);
                case "long":
                    return date.ToString("D", CurrentCulture);
                case "datetime":
                    return date.ToString("g", CurrentCulture);
                default:
                    return date.ToString(format, CurrentCulture);
            }
        }

        /// <summary>
        /// Format number according to current culture
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>Formatted number string</returns>
        public string FormatNumber(double number, int decimals = 2)
        {
            return number.ToString($"N{decimals}", CurrentCulture);
        }

        /// <summary>
        /// Format currency according to current culture
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <param name="currencyCode">Currency code (USD, EUR, SAR, etc.)</param>
        /// <returns>Formatted currency string</returns>
        public string FormatCurrency(double amount, string currencyCode = null)
        {
            if (string.IsNullOrEmpty(currencyCode))
            {
                return amount.ToString("C", CurrentCulture);
            }

            // Custom currency formatting
            var symbol = GetCurrencySymbol(currencyCode);
            var formatted = FormatNumber(amount, 2);

            // Currency symbol position depends on language
            if (IsRightToLeft)
            {
                return $"{formatted} {symbol}";
            }
            else
            {
                return $"{symbol}{formatted}";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load language resource file
        /// </summary>
        /// <param name="language">Language to load</param>
        private void LoadResourceFile(SupportedLanguage language)
        {
            var fileName = $"Resources.{language}.xml";
            var filePath = Path.Combine(_resourcePath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Language resource file not found: {filePath}");
            }

            // Load XML resource file
            var doc = XDocument.Load(filePath);
            var resources = doc.Root?.Element("Strings")?.Elements("String");

            if (resources != null)
            {
                foreach (var resource in resources)
                {
                    var key = resource.Attribute("Key")?.Value;
                    var value = resource.Value;

                    if (!string.IsNullOrEmpty(key))
                    {
                        _resources[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Set form to Right-to-Left mode
        /// </summary>
        /// <param name="form">SAP B1 form</param>
        private void SetFormRightToLeft(SAPbouiCOM.Form form)
        {
            try
            {
                // SAP B1 doesn't have built-in RTL support
                // We need to manually adjust item positions

                // Get form width
                var formWidth = form.Width;

                // Mirror all items horizontally
                for (int i = 0; i < form.Items.Count; i++)
                {
                    var item = form.Items.Item(i);

                    // Calculate mirrored position
                    var currentLeft = item.Left;
                    var itemWidth = item.Width;
                    var newLeft = formWidth - currentLeft - itemWidth;

                    item.Left = newLeft;

                    // Adjust text alignment for static texts and edit boxes
                    if (item.Type == BoFormItemTypes.it_STATIC)
                    {
                        var staticText = (StaticText)item.Specific;
                        // Right align text for RTL
                    }
                    else if (item.Type == BoFormItemTypes.it_EDIT)
                    {
                        var editText = (EditText)item.Specific;
                        // Right align text for RTL
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting RTL: {ex.Message}");
            }
        }

        /// <summary>
        /// Localize all items on form
        /// </summary>
        /// <param name="form">SAP B1 form</param>
        private void LocalizeFormItems(SAPbouiCOM.Form form)
        {
            for (int i = 0; i < form.Items.Count; i++)
            {
                var item = form.Items.Item(i);
                var itemKey = $"Item_{form.TypeEx}_{item.UniqueID}";

                try
                {
                    switch (item.Type)
                    {
                        case BoFormItemTypes.it_BUTTON:
                            var button = (SAPbouiCOM.Button)item.Specific;
                            button.Caption = GetStringWithDefault(itemKey, button.Caption);
                            break;

                        case BoFormItemTypes.it_STATIC:
                            var staticText = (StaticText)item.Specific;
                            staticText.Caption = GetStringWithDefault(itemKey, staticText.Caption);
                            break;

                        case BoFormItemTypes.it_FOLDER:
                            var folder = (Folder)item.Specific;
                            folder.Caption = GetStringWithDefault(itemKey, folder.Caption);
                            break;

                        case BoFormItemTypes.it_CHECK_BOX:
                            var checkbox = (SAPbouiCOM.CheckBox)item.Specific;
                            checkbox.Caption = GetStringWithDefault(itemKey, checkbox.Caption);
                            break;

                        case BoFormItemTypes.it_OPTION_BUTTON:
                            var option = (OptionBtn)item.Specific;
                            option.Caption = GetStringWithDefault(itemKey, option.Caption);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error localizing item {item.UniqueID}: {ex.Message}");
                }
            }

            // Localize matrix columns
            LocalizeMatrixColumns(form);
        }

        /// <summary>
        /// Localize matrix columns
        /// </summary>
        /// <param name="form">SAP B1 form</param>
        private void LocalizeMatrixColumns(SAPbouiCOM.Form form)
        {
            for (int i = 0; i < form.Items.Count; i++)
            {
                var item = form.Items.Item(i);

                if (item.Type == BoFormItemTypes.it_MATRIX)
                {
                    var matrix = (Matrix)item.Specific;

                    for (int j = 1; j <= matrix.Columns.Count; j++)
                    {
                        var column = matrix.Columns.Item(j);
                        var columnKey = $"Column_{form.TypeEx}_{item.UniqueID}_{column.UniqueID}";

                        column.TitleObject.Caption = GetStringWithDefault(columnKey, column.TitleObject.Caption);
                    }
                }
            }
        }

        /// <summary>
        /// Get currency symbol by code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Currency symbol</returns>
        private string GetCurrencySymbol(string currencyCode)
        {
            var symbols = new Dictionary<string, string>
            {
                { "USD", "$" },
                { "EUR", "€" },
                { "GBP", "£" },
                { "JPY", "¥" },
                { "SAR", "ر.س" },  // Saudi Riyal
                { "AED", "د.إ" },  // UAE Dirham
                { "EGP", "ج.م" },  // Egyptian Pound
                { "KWD", "د.ك" },  // Kuwaiti Dinar
                { "QAR", "ر.ق" },  // Qatari Riyal
                { "AUD", "A$" },
                { "CAD", "C$" },
                { "CHF", "CHF" },
                { "CNY", "¥" }
            };

            return symbols.ContainsKey(currencyCode.ToUpper())
                ? symbols[currencyCode.ToUpper()]
                : currencyCode;
        }

        /// <summary>
        /// Log missing resource key
        /// </summary>
        /// <param name="key">Missing resource key</param>
        private void LogMissingResource(string key)
        {
            var logPath = Path.Combine(_resourcePath, "MissingResources.log");
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Language: {CurrentLanguage}, Key: {key}{Environment.NewLine}";

            try
            {
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        /// <summary>
        /// Raise LanguageChanged event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnLanguageChanged(LanguageChangedEventArgs e)
        {
            LanguageChanged?.Invoke(this, e);
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Supported languages
    /// </summary>
    public enum SupportedLanguage
    {
        English,
        Arabic
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Language changed event arguments
    /// </summary>
    public class LanguageChangedEventArgs : EventArgs
    {
        public SupportedLanguage PreviousLanguage { get; }
        public SupportedLanguage NewLanguage { get; }

        public LanguageChangedEventArgs(SupportedLanguage previousLanguage, SupportedLanguage newLanguage)
        {
            PreviousLanguage = previousLanguage;
            NewLanguage = newLanguage;
        }
    }

    #endregion
}
