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

        private readonly IServiceManager _serviceManager;
        private readonly CurrencyCultureProvider _currencyCultureProvider;

        private INoteAcceptor _noteAcceptor;
        private bool _requireZeroCredit;
        private Currency _selectedCurrency;
        private List<Currency> _currencies;

        public MachineSetupPageViewModel(CurrencyCultureProvider currencyCultureProvider, INoteAcceptor noteAcceptor)
            : base(true)
        {
            _serviceManager = ServiceManager.GetInstance();
            _currencyCultureProvider = currencyCultureProvider ?? throw new ArgumentNullException(nameof(currencyCultureProvider));
            _noteAcceptor = noteAcceptor ?? throw new ArgumentNullException(nameof(noteAcceptor));

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

        public List<Currency> Currencies
        {
            get => _currencies;
            set
            {
                _currencies = value;
                RaisePropertyChanged(nameof(Currencies));
            }
        }

        public Currency SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency != value)
                {
                    _selectedCurrency = value;
                    RaisePropertyChanged(nameof(SelectedCurrency));
                }
            }
        }

        public bool RequireZeroCredit
        {
            get => _requireZeroCredit;
            set
            {
                if (_requireZeroCredit != value)
                {
                    _requireZeroCredit = value;
                    RaisePropertyChanged(nameof(RequireZeroCredit));
                }
            }
        }

        public bool RequireZeroCreditChangeAllowed { get; }

        public bool CurrencyChangeAllowed { get; }

        protected override void Loaded()
        { 
            _currencies = new List<Currency>();
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
            _currencies = GetSupportedCurrencies(currencyCode).ToList<Currency>();

            Currencies = _currencies;

            var currency = Currencies.FirstOrDefault(c => c.Description == currencyDescription) ??
                           Currencies.FirstOrDefault(c => c.IsoCode == currencyCode) ??
                           Currencies.FirstOrDefault();
            SelectedCurrency = currency;
        }

        protected override void SaveChanges()
        {
            base.SaveChanges();

            if (SelectedCurrency == null)
            {
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
                _noteAcceptor,
                CurrencyChangeAllowed);

            var orderedSet = currencies.OrderBy(a => a.DisplayName).ToList();
            // Append No Currency options
            return orderedSet.Concat(CurrencyCultureHelper.GetNoCurrencies());
        }
    }
}