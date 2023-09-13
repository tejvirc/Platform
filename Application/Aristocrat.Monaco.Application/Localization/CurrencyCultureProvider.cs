namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Currency;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Implements localization logic for currency culture provider.
    /// </summary>
    public class CurrencyCultureProvider : CultureProvider
    {
        // ReSharper disable once PossibleNullReferenceException
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ConfigurationExtensionPath = "/Currency/Configuration";

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _disableManager;
        private readonly ILocalization _localization;

        private IDictionary<string, CurrencyDefaultsCurrencyInfo> _currencyDefaults = new Dictionary<string, CurrencyDefaultsCurrencyInfo>();
        private Collection<NoteDefinitions> _noteDefinitions = new();
        private readonly object _setCurrencyLock = new object();

        private INoteAcceptor _noteAcceptor;
        private Currencies _currencies;


        public CurrencyCultureProvider()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ILocalization>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        public CurrencyCultureProvider(
            IEventBus eventBus,
            IPropertiesManager properties,
            ILocalization localization,
            ISystemDisableManager disableManager)
            : base(localization)
        {
            _eventBus = eventBus;
            _properties = properties;
            _localization = localization;
            _disableManager = disableManager;
        }

        private static Guid CurrencyIsoInvalidDisableKey => ApplicationConstants.CurrencyIsoInvalidDisableKey;

        public override string ProviderName => CultureFor.Currency;

        public Currency ConfiguredCurrency { get; private set; }

        public IDictionary<string, CurrencyDefaultsCurrencyInfo> CurrencyDefaultFormat => _currencyDefaults;

        protected override void OnInitialize()
        {
            _eventBus.Subscribe<PropertyChangedEvent>(this, _ => Initialize(), PropertyChangedEventFilter);
            _eventBus.Subscribe<ServiceRemovedEvent>(this, _ => NoteAcceptorRemoved(), ServiceRemovedEventFilter);
            _eventBus.Subscribe<InspectedEvent>(this, _ => Initialize());
            _eventBus.Subscribe<InspectionFailedEvent>(this, _ => Initialize());
        }

        protected override void OnConfigure()
        {
            lock (_setCurrencyLock)
            {
                SetTicketCurrency();
                SetCurrency();
            }

            _eventBus?.Publish(new CurrencyCultureChangedEvent());
        }

        private static bool PropertyChangedEventFilter(PropertyChangedEvent evt)
        {
            return evt.PropertyName is ApplicationConstants.CurrencyId
                or ApplicationConstants.LocalizationCurrentCulture
                or PropertyKey.NoteIn;
        }

        private static bool ServiceRemovedEventFilter(ServiceRemovedEvent evt)
        {
            return evt.ServiceType == typeof(INoteAcceptor);
        }

        private void NoteAcceptorRemoved()
        {
            lock (_setCurrencyLock)
            {
                _noteAcceptor = null;
            }
        }

        private static IEnumerable<CultureInfo> GetNonNeutralCultureInfos()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(culture => !string.IsNullOrEmpty(culture.Name) && !culture.IsNeutralCulture);
        }

        private void Initialize()
        {
            lock (_setCurrencyLock)
            {
                _noteAcceptor = ServiceManager.GetInstance()
                    .TryGetService<IDeviceRegistryService>()
                    ?.GetDevice<INoteAcceptor>();

                // Get the currency configurations from CurrencyDefaults
                _currencyDefaults = CurrencyCultureHelper.GetCurrencyDefaults();

                SetCurrencyNoteDefinition();

                OnConfigure();
            }
        }

        private void SetCurrencyNoteDefinition()
        {
            try
            {
                lock (_setCurrencyLock)
                {
                    // get the currencies from configurations
                    _currencies = ConfigurationUtilities.GetConfiguration(
                        ConfigurationExtensionPath,
                        () => default(Currencies));

                    if (_currencies != null)
                    {
                        _noteDefinitions = new Collection<NoteDefinitions>();

                        var currency = _currencies.CurrencyDefinitions.Currency;
                        {
                            var excludedDenoms = new Collection<int>();

                            if (currency == null)
                            {
                                return;
                            }

                            if (currency.ExcludedDenominations != null)
                            {
                                foreach (var denomination in currency.ExcludedDenominations)
                                {
                                    excludedDenoms.Add(denomination);
                                }
                            }

                            // get the currency override format 
                            var format = GetCurrencyOverrideFormat(currency.CurrencyCode);

                            _noteDefinitions.Add(
                                new NoteDefinitions(
                                        currency.CurrencyCode,
                                        excludedDenoms,
                                        format?.Multiplier ?? 0,
                                        format?.MinorUnitSymbol));
                        }
                    }
                    else
                    {
                        Logger.Debug("Currency file missing, no currency overrides loaded");
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Logger.Debug("No currency overrides loaded");
            }

            _properties.SetProperty(HardwareConstants.NoteDefinitions, _noteDefinitions);

            _noteAcceptor?.SetNoteDefinitions(_noteDefinitions);
        }

        private string GetConfiguredCurrencyCode()
        {
            lock (_setCurrencyLock)
            {
                var currencyCode = _properties.GetValue(ApplicationConstants.CurrencyId, string.Empty);
                if (string.IsNullOrEmpty(currencyCode))
                {
                    currencyCode = CurrencyCultureHelper.GetDefaultCurrencyCode(string.Empty);
                    if (!string.IsNullOrEmpty(currencyCode))
                    {
                        _properties.SetProperty(ApplicationConstants.CurrencyId, currencyCode);
                    }
                    else
                    {
                        _disableManager.Enable(CurrencyIsoInvalidDisableKey);
                        return currencyCode;
                    }
                }

                // Don't need to check currency mismatch if it is No Currency
                if (!CurrencyCultureHelper.IsNoCurrency(currencyCode) &&
                   _noteAcceptor?.GetSupportedNotes(currencyCode).Count == 0)
                {
                    var foundCurrencySymbol = GetNonNeutralCultureInfos()
                        .Select(culture => new RegionInfo(culture.Name))
                        .FirstOrDefault(region => _noteAcceptor?.GetSupportedNotes(region.ISOCurrencySymbol)?.Any() == true)
                        ?.ISOCurrencySymbol;
                    if (foundCurrencySymbol != null)
                    {
                        _disableManager.Disable(
                            CurrencyIsoInvalidDisableKey,
                            SystemDisablePriority.Immediate,
                            () => Localizer.ForLockup().FormatString(
                                ResourceKeys.InvalidNoteAcceptorFirmware,
                                foundCurrencySymbol));
                        return currencyCode;
                    }
                }

                _disableManager.Enable(CurrencyIsoInvalidDisableKey);
                return currencyCode;
            }
        }

        private void SetCurrency()
        {
            lock (_setCurrencyLock)
            {
                var currencyCode = GetConfiguredCurrencyCode();

                if (string.IsNullOrEmpty(currencyCode))
                {
                    return;
                }

                CultureInfo cultureInfo = null;
                double currencyMultiplier = Currency.DefaultCurrencyMultiplier;

                if (CurrencyCultureHelper.IsNoCurrency(currencyCode))
                {
                    cultureInfo = SetNoCurrencyFormat(currencyCode);
                }

                if (cultureInfo == null)
                {
                    cultureInfo = _localization.CurrentCulture;

                    RegionInfo region;

                    var cultureName = cultureInfo.Name;
                    if (!string.IsNullOrEmpty(cultureName))
                    {
                        region = new RegionInfo(cultureName);
                        if (currencyCode.Equals(region.ISOCurrencySymbol, StringComparison.InvariantCultureIgnoreCase))
                        {
                            cultureInfo = new CultureInfo(cultureName);
                        }
                        else
                        {
                            FindCultureInfo(currencyCode, ref cultureInfo);
                            region = new RegionInfo(cultureInfo.Name);
                        }
                    }
                    else
                    {
                        FindCultureInfo(currencyCode, ref cultureInfo);
                        region = new RegionInfo(cultureInfo.Name);
                    }

                    UpdateNoteAcceptor(currencyCode);

                    var minorUnitSymbol =
                    _noteDefinitions != null && _noteDefinitions.Any(
                        a => a.Code == currencyCode && !string.IsNullOrEmpty(a.MinorUnitSymbol))
                        ? _noteDefinitions.First(a => a.Code == currencyCode).MinorUnitSymbol
                        : GetDefaultMinorUnitSymbol(currencyCode);

                    var (minorUnits, minorUnitsPlural, pluralizeMajorUnits, pluralizeMinorUnits) =
                        GetOverrideInformation(cultureInfo, currencyCode, ref minorUnitSymbol);

                    ConfiguredCurrency = new Currency(currencyCode, region, cultureInfo, minorUnitSymbol);
                    CurrencyExtensions.Currency = ConfiguredCurrency;

                    var defaultMultiplier = CurrencyExtensions.SetCultureInfo(
                        currencyCode,
                        cultureInfo,
                    minorUnits,
                    minorUnitsPlural,
                    pluralizeMajorUnits,
                    pluralizeMinorUnits,
                    minorUnitSymbol);

                    currencyMultiplier =
                            _noteDefinitions != null &&
                            _noteDefinitions.Any(a => a.Code == currencyCode && a.Multiplier > 0)
                                ? _noteDefinitions.First(a => a.Code == currencyCode).Multiplier
                                : defaultMultiplier;
                }
                _properties.SetProperty(ApplicationConstants.CurrencyMultiplierKey, currencyMultiplier);
            }
        }

        private CultureInfo LoadCultureInfo(string locale, string currencyCode)
        {
            CultureInfo cultureInfo = null;

            try
            {
                if (CurrencyCultureHelper.IsNoCurrency(currencyCode))
                {
                    // If selected currency is No Currency, use the invariant culture
                    return (CultureInfo)CultureInfo.InvariantCulture.Clone();
                }

                if (string.IsNullOrEmpty(locale))
                {
                    FindCultureInfo(currencyCode, ref cultureInfo);
                }
                else
                {
                    // Use the locale language and the BNA currency code to find the correct culture for this player ticket locale
                    cultureInfo = locale.Length == 2
                        ? GetNonNeutralCultureInfos()
                            .FirstOrDefault(
                                c => c.TwoLetterISOLanguageName == locale &&
                                     new RegionInfo(c.Name).ISOCurrencySymbol == currencyCode)
                        : new CultureInfo(locale);

                }
            }
            catch (Exception e)
            {
                cultureInfo = _localization.CurrentCulture;

                Logger.Error($"Failed to find the culture for ticket, configured locale: {locale}, currency: {currencyCode}", e);
            }

            return cultureInfo;
        }

        private void SetTicketCultureInfo(PlayerTicketSelectionArrayEntry entry)
        {
            var currencyCode = _properties.GetValue(ApplicationConstants.CurrencyId, string.Empty);

            var valueCultureInfo = LoadCultureInfo(
                entry.CurrencyValueLocale,
                currencyCode);
            var wordsCultureInfo = LoadCultureInfo(
                entry.CurrencyWordsLocale,
                currencyCode);

            if (valueCultureInfo == null || wordsCultureInfo == null)
            {
                return;
            }

            if (!CurrencyCultureHelper.IsNoCurrency(currencyCode))
            {
                var minorUnitSymbol =
                    _noteDefinitions != null && _noteDefinitions.Any(
                        a => a.Code == currencyCode && !string.IsNullOrEmpty(a.MinorUnitSymbol))
                        ? _noteDefinitions.First(a => a.Code == currencyCode).MinorUnitSymbol
                        : GetDefaultMinorUnitSymbol(currencyCode);

                var selectedOverride = GetOverrideSelectionFromCurrencyDefault(valueCultureInfo, currencyCode, ref minorUnitSymbol);

                var (minorUnits, minorUnitsPlural, pluralizeMajorUnits, pluralizeMinorUnits) =
                    GetOverrideInformation(wordsCultureInfo, currencyCode, ref minorUnitSymbol, selectedOverride);

                TicketCurrencyExtensions.SetCultureInfo(
                    entry.Locale,
                    valueCultureInfo,
                    wordsCultureInfo,
                    minorUnits,
                    minorUnitsPlural,
                    pluralizeMajorUnits,
                    pluralizeMinorUnits,
                    minorUnitSymbol);
            }
            else
            {
                // No currency is configured
                TicketCurrencyExtensions.SetCultureInfo(
                    entry.Locale,
                    valueCultureInfo,
                    NoCurrency.NoCurrencyName);
            }
        }

        private void SetTicketCurrency()
        {
            lock (_setCurrencyLock)
            {
                var currencyCode = _properties.GetValue(ApplicationConstants.CurrencyId, string.Empty);
                if (string.IsNullOrEmpty(currencyCode))
                {
                    return;
                }

                // Some jurisdictions have multiple ticket locales which means multiple currencies. For jurisdictions that have no selectable PlayerTicket locales, one 
                // will automatically be loaded in the LocalizationPlayerTicketSelectable property
                var playerTicketSelectableLocales = _properties.GetValue(
                    ApplicationConstants.LocalizationPlayerTicketSelectable,
                    new PlayerTicketSelectionArrayEntry[] { });
                if (playerTicketSelectableLocales.Length > 0)
                {
                    foreach (var entry in playerTicketSelectableLocales)
                    {
                        SetTicketCultureInfo(entry);
                    }

                    // Select the first one in the list
                    TicketCurrencyExtensions.PlayerTicketLocale = playerTicketSelectableLocales[0].Locale;
                }
            }
        }

        private CultureInfo SetNoCurrencyFormat(string currencyCode)
        {
            var noCurrencyCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            var currencyDesc = _properties.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);

            int id = CurrencyCultureHelper.GetNoCurrencyFormatId(currencyDesc);

            if (id >= 0)
            {
                CurrencyCultureHelper.ConfigureNoCurrencyCultureFormat(id, noCurrencyCulture);

                CurrencyExtensions.SetCultureInfo(
                    currencyCode,
                    noCurrencyCulture,
                    null,
                    null,
                    false,
                    false,
                    null);
            }

            // No Currency is not a valid ISO currency code, so we can't set it in the BNA
            var supportedCurrencies = _noteAcceptor?.GetSupportedCurrencies();
            if (supportedCurrencies?.Count > 0)
            {
                Logger.Debug($"No currency '{currencyDesc}' is configured. Use currency '{supportedCurrencies[0].ToString()}' to set BNA.");
                // Use the first supported currency to set the BNA
                UpdateNoteAcceptor(supportedCurrencies[0].ToString());
            }


            ConfiguredCurrency = new NoCurrency(id, noCurrencyCulture);
            CurrencyExtensions.Currency = ConfiguredCurrency;

            return noCurrencyCulture;
        }

        private void UpdateNoteAcceptor(string currencyCode)
        {
            _noteAcceptor?.SetIsoCode(currencyCode);

            if (!_properties.GetValue(PropertyKey.NoteIn, true))
            {
                if (_noteAcceptor?.Denominations.Count > 0)
                {
                    foreach (var denom in _noteAcceptor.Denominations.ToArray())
                    {
                        _noteAcceptor.UpdateDenom(denom, false);
                    }
                }
            }
        }

        private (string minorUnits, string minorUnitsPlural, bool pluralizeMajorUnits, bool pluralizeMinorUnits)
            GetOverrideInformation(CultureInfo cultureInfo, string currencyCode, ref string minorUnitSymbol, CurrencyDefaultsCurrencyInfoFormat selectedOverride = null)
        {
            //Default unit return values
            var (defaultMinorUnits, defaultMinorUnitsPlural) =
                SetFormatDefaults(cultureInfo, currencyCode, ref minorUnitSymbol);
            var unitResults = (defaultMinorUnits, defaultMinorUnitsPlural, true, true);
            if (string.IsNullOrEmpty(currencyCode) ||
                !_currencyDefaults.ContainsKey(currencyCode) ||
                _currencyDefaults[currencyCode] == null)
            {
                return unitResults;
            }

            var configuredCurrency = GetConfiguredCurrency(currencyCode);


            var currencyDescription = _properties.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);
            if (selectedOverride == null)
            {
                foreach (var overrideFormat in _currencyDefaults[currencyCode].Formats)
                {
                    overrideFormat.ExcludePluralizeMajorUnits = configuredCurrency?.Format?.ExcludePluralizeMajorUnits;
                    overrideFormat.ExcludePluralizeMinorUnits = configuredCurrency?.Format?.ExcludePluralizeMinorUnits;
                    unitResults = SetFormatOverrides(overrideFormat, cultureInfo, ref minorUnitSymbol);
                    if (currencyDescription.Equals(cultureInfo.GetFormattedDescription(currencyCode), StringComparison.InvariantCultureIgnoreCase))
                    {
                        return unitResults;
                    }
                }
            }
            else
            {
                // set the exclude plural for units
                selectedOverride.ExcludePluralizeMajorUnits = configuredCurrency?.Format?.ExcludePluralizeMajorUnits;
                selectedOverride.ExcludePluralizeMinorUnits = configuredCurrency?.Format?.ExcludePluralizeMinorUnits;

                return SetFormatOverrides(selectedOverride, cultureInfo, ref minorUnitSymbol);
            }

            return unitResults;
        }

        private (string minorUnits, string minorUnitsPlural) SetFormatDefaults(
            CultureInfo cultureInfo,
            string currencyCode,
            ref string minorUnitSymbol)
        {
            var minorUnits = string.Empty;
            var minorUnitsPlural = string.Empty;

            if (string.IsNullOrEmpty(currencyCode) || !_currencyDefaults.ContainsKey(currencyCode) ||
                _currencyDefaults[currencyCode] == null)
            {
                return (minorUnits, minorUnitsPlural);
            }

            var format = GetCurrencyOverrideFormat(currencyCode);

            minorUnits = format?.MinorUnits;
            minorUnitsPlural = format?.MinorUnitsPlural;


            if (format?.MinorUnitSymbol != null)
            {
                minorUnitSymbol = format.MinorUnitSymbol;
            }

            CurrencyCultureHelper.OverrideCultureInfoProperties(format, cultureInfo);

            return (minorUnits, minorUnitsPlural);
        }


        private static (string minorUnits, string minorUnitsPlural, bool pluralizeMajorUnits, bool pluralizeMinorUnits)
           SetFormatOverrides(ICurrencyOverride selectedOverride, CultureInfo cultureInfo, ref string minorUnitSymbol)

        {
            var pluralizeMajorUnits = true;
            var pluralizeMinorUnits = true;

            var minorUnits = selectedOverride?.MinorUnits;
            var minorUnitsPlural = selectedOverride?.MinorUnitsPlural;
            if (selectedOverride?.ExcludePluralizeMajorUnits != null)
            {
                ExcludePluralizeUnits(selectedOverride.ExcludePluralizeMajorUnits, ref pluralizeMajorUnits);
            }

            if (selectedOverride?.ExcludePluralizeMinorUnits != null)
            {
                ExcludePluralizeUnits(selectedOverride.ExcludePluralizeMinorUnits, ref pluralizeMinorUnits);
            }

            CurrencyCultureHelper.OverrideCultureInfoProperties(selectedOverride, cultureInfo);

            if (selectedOverride?.MinorUnitSymbol != null)
            {
                minorUnitSymbol = selectedOverride.MinorUnitSymbol;
            }

            return (minorUnits, minorUnitsPlural, pluralizeMajorUnits, pluralizeMinorUnits);

            void ExcludePluralizeUnits(string[] excludedCulture, ref bool pluralizeUnits)
            {
                foreach (var culture in excludedCulture)
                {
                    var excluded = new CultureInfo(culture);
                    if (cultureInfo.Name.Equals(excluded.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        pluralizeUnits = false;
                        break;
                    }
                }
            }
        }

        private CurrencyDefaultsCurrencyInfoFormat GetOverrideSelectionFromCurrencyDefault(CultureInfo cultureInfo, string currencyCode, ref string minorUnitSymbol)
        {
            if (!string.IsNullOrEmpty(currencyCode) && _currencyDefaults.ContainsKey(currencyCode) &&
                _currencyDefaults[currencyCode] != null)
            {
                var currencyDescription = _properties.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);

                var configuredCurrency = GetConfiguredCurrency(currencyCode);

                foreach (var overrides in _currencyDefaults[currencyCode].Formats)
                {
                    overrides.ExcludePluralizeMajorUnits = configuredCurrency?.Format?.ExcludePluralizeMajorUnits;
                    overrides.ExcludePluralizeMinorUnits = configuredCurrency?.Format?.ExcludePluralizeMinorUnits;

                    SetFormatOverrides(overrides, cultureInfo, ref minorUnitSymbol);

                    if (configuredCurrency?.Format?.id == overrides.id ||
                        configuredCurrency?.Format == null && MatchCulture(currencyDescription, cultureInfo))
                    {
                        return overrides;
                    }
                }
            }

            return null;
        }

        private void FindCultureInfo(string currencyCode, ref CultureInfo cultureInfo)
        {
            var currencyDescription = _properties.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);

            var cultures = GetNonNeutralCultureInfos().ToList();
            var foundInfo = cultures.FirstOrDefault(culture => culture.Name.Equals(_localization.CurrentCulture.Name, StringComparison.InvariantCultureIgnoreCase) &&
                                                               MatchCulture(currencyDescription, culture, currencyCode)) ??
                            cultures.FirstOrDefault(culture => MatchCulture(currencyDescription, culture, currencyCode)) ??
                            cultures.FirstOrDefault(culture => culture.Name.Equals(_localization.CurrentCulture.Name, StringComparison.InvariantCultureIgnoreCase));

            if (foundInfo != null)
            {
                cultureInfo = foundInfo;
            }
        }

        private bool MatchCulture(string description, CultureInfo culture, string currencyCode = null)
        {
            if (!string.IsNullOrEmpty(currencyCode) && !string.IsNullOrEmpty(culture.Name))
            {
                var region = new RegionInfo(culture.Name);

                if (!currencyCode.Equals(region.ISOCurrencySymbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(description))
            {
                return true;
            }

            var minorSymbol = string.Empty;

            _ = GetOverrideInformation(culture, currencyCode, ref minorSymbol);

            return description.Equals(culture.GetFormattedDescription(currencyCode), StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetDefaultMinorUnitSymbol(string currencyCode)
        {
            var currencyFormat = GetCurrencyOverrideFormat(currencyCode);

            return currencyFormat?.MinorUnitSymbol;
        }

        private CurrencyDefaultsCurrencyInfoFormat GetCurrencyOverrideFormat(string currencyCode)
        {
            var currency = _currencies?.CurrencyDefinitions.Currency;
            if (currency != null &&
                !currencyCode.Equals(currency.CurrencyCode, StringComparison.InvariantCultureIgnoreCase))
            {
                // the currency code doesn't match to the configured currency
                return null;
            }

            int formatId = currency?.Format?.id ?? 1;
            if (_currencyDefaults.TryGetValue(currencyCode, out var defaults))
            {
                if (defaults != null)
                {
                    return defaults.Formats?.FirstOrDefault(f => f.id == formatId);
                }
            }

            return null;
        }

        private CurrenciesCurrencyDefinitionsCurrency GetConfiguredCurrency(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
            {
                return null;
            }
            var currencies = ConfigurationUtilities.GetConfiguration(
                ConfigurationExtensionPath,
                () => default(Currencies));

            if (currencies == null)
            {
                return null;
            }

            if (!currencyCode.Equals(
                    currencies.CurrencyDefinitions?.Currency?.CurrencyCode,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return currencies.CurrencyDefinitions?.Currency;
        }
    }
}