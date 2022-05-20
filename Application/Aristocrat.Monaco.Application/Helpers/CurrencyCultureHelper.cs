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
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Common.Currency;
    using Contracts;
    using Kernel;
    using Localization;
    using log4net;

    using CurrencyDefaultsCurrencyInfo = Localization.CurrencyDefaultsCurrencyInfo;

    public static class CurrencyCultureHelper
    {
        private const string CurrencyDefaultXml = @".\CurrencyDefaults.xml";

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

        public static List<Currency> GetSupportedCurrencies(string currencyCode,
            IDictionary<string, CurrencyDefaultsCurrencyInfo> currencyDefaults,
            ILog logger,
            INoteAcceptor noteAcceptor,
            bool currencyChangeAllowed)
        {
            Dictionary<string, List<int>> currencyFormats = new();

            var set = new List<Currency>();
            
            // Get supported currencies from Windows system
            var currencies = CurrencyLoader.GetCurrenciesFromWindows(logger);
            foreach(var currencyInfo in currencies)
            {
                if (!Enum.IsDefined(typeof(ISOCurrencyCode), currencyInfo.Key.ToUpper()))
                {
                    // not a supported currency code
                    continue;
                }

                bool currencyAdded = false;
                var culture = currencyInfo.Value;
                var region = new RegionInfo(culture.Name);
                var currency = new Currency(currencyInfo.Key, region, culture);

                if (currencyDefaults != null &&
                    currencyDefaults.TryGetValue(currencyInfo.Key, out var defaults))
                {
                    if (defaults != null)
                    {
                        var preOverrideDescription = currency.Description;

                        foreach (var format in defaults.Formats)
                        {
                            // override the currency format in culture with configured format
                            OverrideCultureInfoProperties(format, culture);

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
                    currencyCode == null ||
                    set.Any(c => c.IsoCode.Equals(currency.IsoCode, StringComparison.OrdinalIgnoreCase)) ||
                    !currencyChangeAllowed && currency.IsoCode != currencyCode ||
                    currencyChangeAllowed && noteAcceptor != null && !(noteAcceptor.GetSupportedNotes(currencyInfo.Key).Count > 0))
                {
                    continue;
                }

                set.Add(currency);
            }

            
            return set;
        }

        public static void OverrideCultureInfoProperties(ICurrencyOverride overrides, CultureInfo cultureInfo)
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

        public static string GetDefaultCurrencyCode(string defaultValue = ApplicationConstants.DefaultCurrencyId)
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () => default(ApplicationConfiguration));

            return configuration?.Currency.Id ?? defaultValue;
        }
    }
}
