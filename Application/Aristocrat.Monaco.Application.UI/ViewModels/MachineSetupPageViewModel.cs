namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Helpers;
    using Localization;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Contracts.Currency;

    using CurrencyDefaultsCurrencyInfo = Localization.CurrencyDefaultsCurrencyInfo;

    [CLSCompliant(false)]
    public class MachineSetupPageViewModel : MachineSetupViewModelBase
    {
        private IDictionary<string, CurrencyDefaultsCurrencyInfo> _currencyDefaults = new ConcurrentDictionary<string, CurrencyDefaultsCurrencyInfo>();

        private readonly CurrencyCultureProvider _currencyCultureProvider;

        private bool _requireZeroCredit;
        private Currency _selectedCurrency;
        private IReadOnlyCollection<Currency> _currencies;

        public MachineSetupPageViewModel(CurrencyCultureProvider currencyCultureProvider)
            : base(true)
        {
            _currencyCultureProvider = currencyCultureProvider ?? throw new ArgumentNullException(nameof(currencyCultureProvider));

            // This param is expected to be null when note acceptor is unchecked from Hardware Configuration page
            _noteAcceptor = noteAcceptor;

            if (SerialNumber.EditedValue == "0")
            {
                // Don't need to default to 0 for this page
                SerialNumber.LiveValue = string.Empty;
            }

            _requireZeroCredit = !PropertiesManager.GetValue(
                ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                true);

            RequireZeroCreditChangeAllowed = PropertiesManager.GetValue(
                ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEditable,
                true);

            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () =>
                    new ApplicationConfiguration
                    {
                        Currency = new ApplicationConfigurationCurrency { Configurable = false }
                    });
            CurrencyChangeAllowed = configuration.Currency.Configurable;
        }

        public IReadOnlyCollection<Currency> Currencies
        {
            get => _currencies;
            set
            {
                _currencies = value;
                OnPropertyChanged(nameof(Currencies));
            }
        }

        public Currency SelectedCurrency
        {
            get => _selectedCurrency;
            set => SetProperty(ref _selectedCurrency, value);
        }

        public bool RequireZeroCredit
        {
            get => _requireZeroCredit;
            set => SetProperty(ref _requireZeroCredit, value);
        }

        public bool RequireZeroCreditChangeAllowed { get; }

        public bool CurrencyChangeAllowed { get; }

        protected override void Loaded()
        {
            var currencyCode = PropertiesManager.GetValue(
                ApplicationConstants.CurrencyId,
                string.Empty);
            if (string.IsNullOrEmpty(currencyCode))
            {
                currencyCode = CurrencyCultureHelper.GetDefaultCurrencyCode();
            }

            Logger.Info($"CultureInfo.CurrentCulture.Name {CultureInfo.CurrentCulture.Name} - currencyCode {currencyCode}");

            _currencyDefaults = _currencyCultureProvider.CurrencyDefaultFormat;
            var currencyDescription = (string)PropertiesManager.GetProperty(
                ApplicationConstants.CurrencyDescription,
                string.Empty);
            var currencies = GetSupportedCurrencies(currencyCode).ToList();
            var currency = currencies.FirstOrDefault(c => c.Description == currencyDescription) ??
                           currencies.FirstOrDefault(c => c.IsoCode == currencyCode) ??
                           currencies.FirstOrDefault();

            Currencies = currencies;
            SelectedCurrency = currency;
        }

        protected override void SaveChanges()
        {
            base.SaveChanges();

            if (SelectedCurrency == null)
            {
                Logger.Warn("No selected currency found when trying to save");

                // A change made for inspection tool to Save on Next button click broke auto config on this page
                // It doesn't get loaded before calling save, so none of the currency info is setup correctly
                Loaded();
            }

            PropertiesManager.SetProperty(ApplicationConstants.CurrencyDescription, SelectedCurrency.Description);
            PropertiesManager.SetProperty(ApplicationConstants.CurrencyId, SelectedCurrency.IsoCode);
            PropertiesManager.SetProperty(
                ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                !RequireZeroCredit);

            _currencyCultureProvider.Configure();
        }

        protected override void LoadAutoConfiguration()
        {
            string value = null;

            AutoConfigurator.GetValue(ApplicationConstants.Currency, ref value);
            if (value != null)
            {
                var currency = Currencies.FirstOrDefault(c => c.IsoCode == value) ??
                               Currencies.FirstOrDefault(c => c.Description == value);
                if (currency != null)
                {
                    SelectedCurrency = currency;
                }
            }

            value = null;
            AutoConfigurator.GetValue(ApplicationConstants.RequireZeroCredit, ref value);
            if (value != null && bool.TryParse(value, out var requireZeroCredit))
            {
                RequireZeroCredit = requireZeroCredit;
            }

            base.LoadAutoConfiguration();
        }

        private IEnumerable<Currency> GetSupportedCurrencies(string currencyCode)
        {
            var currencies = CurrencyCultureHelper.GetSupportedCurrencies(
                currencyCode,
                _currencyDefaults,
                Logger,
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>(),
                CurrencyChangeAllowed);

            var orderedSet = currencies.OrderBy(a => a.DisplayName).ToList();
            // Append No Currency options
            return orderedSet.Concat(CurrencyCultureHelper.GetNoCurrencies());
        }
    }
}