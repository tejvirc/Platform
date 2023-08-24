namespace Aristocrat.Monaco.Application.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    using Contracts.Currency;
    using Hardware.Contracts.NoteAcceptor;
    using Common.Currency;
    using Contracts;
    using Kernel;
    using Localization;
    using log4net;

    using CurrencyDefaultsCurrencyInfo = Localization.CurrencyDefaultsCurrencyInfo;

    public static class CurrencyCultureHelper
    {
        private const string CurrencyDefaultXml = @".\CurrencyDefaults.xml";

        private static Dictionary<string, CultureInfo> _supportedCurrencies = null;

        public static IDictionary<string, CurrencyDefaultsCurrencyInfo> GetCurrencyDefaults()
        {
            IDictionary<string, CurrencyDefaultsCurrencyInfo> defaults =
                new ConcurrentDictionary<string, CurrencyDefaultsCurrencyInfo>();

            using (var sr = new StreamReader(CurrencyDefaultXml))
            {
                CurrencyDefaults currencyDefaults;

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sr.ReadToEnd())))
                {
                    var serializer = new XmlSerializer(typeof(CurrencyDefaults));
                    currencyDefaults = (CurrencyDefaults)serializer.Deserialize(stream);
                }

                if (currencyDefaults.CurrencyInfo != null)
                {
                    foreach (var info in currencyDefaults.CurrencyInfo)
                    {
                        defaults.Add(info.CurrencyCode, info);
                    }
                }
            }

            return defaults;
        }

        public static Dictionary<string, CultureInfo> GetSupportedCurrenciesFromWindows(ILog log)
        {
            if (_supportedCurrencies == null)
            {
                _supportedCurrencies = CurrencyLoader.GetCurrenciesFromWindows(log);
            }
            return _supportedCurrencies;
        }

        public static List<Currency> GetSupportedCurrencies(string defaultCurrencyCode,
            IDictionary<string, CurrencyDefaultsCurrencyInfo> currencyDefaultFormats,
            ILog logger,
            INoteAcceptor noteAcceptor,
            bool currencyChangeAllowed)
        {
            Dictionary<string, List<int>> currencyFormats = new();

            var set = new List<Currency>();

            // Get supported currencies from Windows system
            var currencies = GetSupportedCurrenciesFromWindows(logger);
            foreach (var currencyInfo in currencies)
            {
                if (!Enum.IsDefined(typeof(ISOCurrencyCode), currencyInfo.Key.ToUpper()) ||
                    !IsValidCurrency(noteAcceptor, currencyInfo.Key))
                {
                    // not a Monaco supported currency code
                    continue;
                }

                bool currencyAdded = false;
                var culture = currencyInfo.Value;
                var region = new RegionInfo(culture.Name);
                var currency = new Currency(currencyInfo.Key, region, culture);

                CurrencyDefaultsCurrencyInfo defaults = null;
                if (currencyDefaultFormats?.TryGetValue(currencyInfo.Key, out defaults) == true)
                {
                    if (defaults != null)
                    {
                        var preOverrideDescription = currency.Description;

                        foreach (var format in defaults.Formats)
                        {
                            // override the currency format in culture with configured format
                            OverrideCurrencyProperties(format, culture);

                            currency = new Currency(currencyInfo.Key, region, culture, format.MinorUnitSymbol);
                            bool hasOverride = false;

                            // check if the currency with format has been already added 
                            if (currencyFormats.ContainsKey(currency.IsoCode))
                            {
                                if (currencyFormats[currencyInfo.Key].All(f => f != format.id))
                                {
                                    currencyFormats[currencyInfo.Key].Add(format.id);
                                    hasOverride = true;
                                }
                            }
                            else
                            {
                                currencyFormats.Add(currencyInfo.Key, new List<int>() { format.id });
                                hasOverride = true;
                            }

                            if (hasOverride)
                            {
                                set.Add(currency);
                                currencyAdded = true;
                                logger.Info($"\"{preOverrideDescription}\" overridden as \"{currency.Description}\"");
                            }
                        }
                    }
                }

                if (currencyAdded ||
                    set.Any(c => c.IsoCode.Equals(currency.IsoCode, StringComparison.InvariantCultureIgnoreCase)) ||
                    !currencyChangeAllowed && currency.IsoCode != defaultCurrencyCode ||
                    currencyChangeAllowed && noteAcceptor != null && !(noteAcceptor.GetSupportedNotes(currencyInfo.Key).Count > 0))
                {
                    continue;
                }

                set.Add(currency);
            }

            // Add the currencies configured in CurrencyDefaults.xml, but not specified in the window cultures
            var nonSupportedCurrencies = currencyDefaultFormats.Where(c => !currencies.ContainsKey(c.Key));
            foreach (var nonSupportedCurrency in nonSupportedCurrencies)
            {
                if (!IsValidCurrency(noteAcceptor, nonSupportedCurrency.Key))
                {
                    continue;
                }

                var currencyDef = nonSupportedCurrency.Value;
                if (string.IsNullOrWhiteSpace(currencyDef.Name))
                {
                    logger.Debug($"The unsupported currency '{nonSupportedCurrency.Key}' doesn't have Name specified");
                    continue;
                }

                var formats = currencyDef.Formats;
                foreach (var format in formats)
                {
                    set.Add(new CustomCurrency(currencyDef.CurrencyCode, currencyDef.Name, format));
                }
            }

            return set;
        }

        public static void OverrideCurrencyProperties(ICurrencyFormatOverride overrides, CultureInfo cultureInfo)
        {
            if (overrides?.DecimalDigitsSpecified ?? false)
            {
                cultureInfo.NumberFormat.CurrencyDecimalDigits = overrides.DecimalDigits;
            }

            if (overrides?.PositivePatternSpecified ?? false)
            {
                cultureInfo.NumberFormat.CurrencyPositivePattern = overrides.PositivePattern;
            }

            if (overrides?.NegativePatternSpecified ?? false)
            {
                cultureInfo.NumberFormat.CurrencyNegativePattern = overrides.NegativePattern;
            }

            if (overrides?.DecimalSeparator != null)
            {
                cultureInfo.NumberFormat.CurrencyDecimalSeparator =
                    overrides.DecimalSeparator.Length == 0 ? " " : overrides.DecimalSeparator;
            }

            if (overrides?.GroupSeparator != null)
            {
                cultureInfo.NumberFormat.CurrencyGroupSeparator = overrides.GroupSeparator;
            }

            if (overrides?.Symbol != null)
            {
                cultureInfo.NumberFormat.CurrencySymbol = overrides.Symbol;
            }
        }

        public static void OverrideCurrencyProperties(CultureInfo fromCulture, CultureInfo toCulture)
        {
            toCulture.NumberFormat.CurrencyDecimalDigits = fromCulture.NumberFormat.CurrencyDecimalDigits;
            toCulture.NumberFormat.CurrencyPositivePattern = fromCulture.NumberFormat.CurrencyPositivePattern;
            toCulture.NumberFormat.CurrencyNegativePattern = fromCulture.NumberFormat.CurrencyNegativePattern;
            toCulture.NumberFormat.CurrencyDecimalSeparator = fromCulture.NumberFormat.CurrencyDecimalSeparator;
            toCulture.NumberFormat.CurrencyGroupSeparator = fromCulture.NumberFormat.CurrencyGroupSeparator;
            toCulture.NumberFormat.CurrencySymbol = fromCulture.NumberFormat.CurrencySymbol;
        }

        public static string GetDefaultCurrencyCode(string defaultValue = ApplicationConstants.DefaultCurrencyId)
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () => default(ApplicationConfiguration));

            return configuration?.Currency.Id ?? defaultValue;
        }

        public static List<NoCurrency> GetNoCurrencies()
        {
            List<NoCurrency> noCurrencies = new List<NoCurrency>();

            // go through No Currency format options and apply it to the culture info for each no currency
            foreach (var currencyDef in NoCurrencyOptions.Options)
            {
                NoCurrency noSymbolCurrency = new NoCurrency(currencyDef.Id);
                noCurrencies.Add(noSymbolCurrency);
            }

            return noCurrencies;
        }

        /// <summary>
        /// Get the format id of No Currency
        /// </summary>
        /// <param name="currencyDesc"></param>
        /// <returns></returns>
        public static int GetNoCurrencyFormatId(string currencyDesc)
        {
            var id = NoCurrencyOptions.Options.FirstOrDefault(o =>
                o.FormatString.Equals(currencyDesc.Substring(NoCurrency.NoCurrencyCode.Length + 1).Trim(), StringComparison.InvariantCultureIgnoreCase))?.Id;

            return id ?? -1;
        }

        /// <summary>
        /// Check if the currency code is No Currency
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <returns></returns>
        public static bool IsNoCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return false;
            }

            return currencyCode.Equals(NoCurrency.NoCurrencyCode, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsCustomizedCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return false;
            }

            // If the currency is not specified in windows culture, then it must be specified in the CurrencyDefaults configurations
            return !GetSupportedCurrenciesFromWindows(null).ContainsKey(currencyCode);
        }

        private static bool IsValidCurrency(INoteAcceptor noteAcceptor, string currencyCode)
        {
            if (noteAcceptor == null)
            {
                return true;
            }

            return noteAcceptor.GetSupportedNotes(currencyCode).Count > 0;
        }
    }
}
