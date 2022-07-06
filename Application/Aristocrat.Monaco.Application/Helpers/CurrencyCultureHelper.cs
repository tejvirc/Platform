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
    using Aristocrat.Monaco.Application.Contracts.Extensions;

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

        public static List<Currency> GetSupportedCurrencies(string defaultCurrencyCode,
            IDictionary<string, CurrencyDefaultsCurrencyInfo> currencyDefaultFormats,
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

                if (currencyDefaultFormats != null &&
                    currencyDefaultFormats.TryGetValue(currencyInfo.Key, out var defaults))
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
                    set.Any(c => c.IsoCode.Equals(currency.IsoCode, StringComparison.OrdinalIgnoreCase)) ||
                    !currencyChangeAllowed && currency.IsoCode != defaultCurrencyCode ||
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

        public static List<NoCurrency> GetNoCurrencies()
        {
            List<NoCurrency> noCurrencies = new List<NoCurrency>();

            // go through No Currency format options and apply it to the culture info for each no currency
            foreach (var currencyDef in NoCurrencyOptions.Options)
            {
                CultureInfo currencyCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                ConfigureNoCurrencyCultureFormat(currencyDef.Id, currencyCulture);

                NoCurrency noSymbolCurrency = new NoCurrency(currencyDef.Id, currencyCulture);
                noCurrencies.Add(noSymbolCurrency);
            }

            return noCurrencies;
        }

        /// <summary>
        /// Configure the currency format in the culture info for No Currency
        /// </summary>
        /// <param name="id">format id</param>
        /// <param name="currencyCulture">Currency culture</param>
        public static void ConfigureNoCurrencyCultureFormat(int id, CultureInfo currencyCulture)
        {
            var format = NoCurrencyOptions.Get(id);
            currencyCulture.ApplyNoCurrencyFormat(format);
        }

        /// <summary>
        /// Get the format id of No Currency
        /// </summary>
        /// <param name="currencyDesc"></param>
        /// <returns></returns>
        public static int GetNoCurrencyFormatId(string currencyDesc)
        {
            var id = NoCurrencyOptions.Options.FirstOrDefault(o =>
                o.FormatString.Equals(currencyDesc.Substring(NoCurrency.NoCurrencyCode.Length + 1).Trim()))?.Id;

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

            return currencyCode.Equals(NoCurrency.NoCurrencyCode);
        }
    }
}
