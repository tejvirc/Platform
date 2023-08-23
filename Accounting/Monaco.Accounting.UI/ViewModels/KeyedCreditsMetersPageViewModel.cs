namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Kernel;
    using Localization.Properties;

    [CLSCompliant(false)]
    public class KeyedCreditsMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly Tuple<string, string>[] _metersOn =
        {
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashableKeyedOnCreditsLabel),
                "KeyedOnCashable"),
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashablePromoKeyedOnCreditsLabel),
                "KeyedOnCashablePromotional"),
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket)
                    .GetString(ResourceKeys.NonCashablePromoKeyedOnCreditsLabel),
                "KeyedOnNonCashablePromotional")
        };

        private readonly Tuple<string, string>[] _metersOff =
        {
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashableKeyedOffCreditsLabel),
                "KeyedOffCashable"),
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashablePromoKeyedOffCreditsLabel),
                "KeyedOffCashablePromotional"),
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket)
                    .GetString(ResourceKeys.NonCashablePromoKeyedOffCreditsLabel),
                "KeyedOffNonCashablePromotional")
        };

        private long _keyedOnTotalCount;
        private string _keyedOnTotalValue;
        private string _keyedOffTotalValue;
        private long _keyedOffTotalCount;

        public KeyedCreditsMetersPageViewModel()
            : base(null)
        {
        }

        public ObservableCollection<CountDisplayMeter> KeyedOnMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public long KeyedOnTotalCount
        {
            get => _keyedOnTotalCount;
            set
            {
                _keyedOnTotalCount = value;
                OnPropertyChanged(nameof(KeyedOnTotalCount));
            }
        }

        public string KeyedOnTotalValue
        {
            get => _keyedOnTotalValue;
            set
            {
                _keyedOnTotalValue = value;
                OnPropertyChanged(nameof(KeyedOnTotalValue));
            }
        }

        public ObservableCollection<CountDisplayMeter> KeyedOffMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public long KeyedOffTotalCount
        {
            get => _keyedOffTotalCount;
            set
            {
                _keyedOffTotalCount = value;
                OnPropertyChanged(nameof(KeyedOffTotalCount));
            }
        }

        public string KeyedOffTotalValue
        {
            get => _keyedOffTotalValue;
            set
            {
                _keyedOffTotalValue = value;
                OnPropertyChanged(nameof(KeyedOffTotalValue));
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            // get a reference to the meter manager
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            ClearMeters();

            foreach (var (meterDisplayName, meterName) in _metersOn)
            {
                var count = meterManager.GetMeter(meterName + "Count");
                var value = meterManager.GetMeter(meterName + "Value");

                var meter = new CountDisplayMeter(meterDisplayName, count, value, ShowLifetime);
                meter.PropertyChanged += OnMeterChangedEvent;

                KeyedOnMeters.Add(meter);
            }

            foreach (var (meterDisplayName, meterName) in _metersOff)
            {
                var count = meterManager.GetMeter(meterName + "Count");
                var value = meterManager.GetMeter(meterName + "Value");

                var meter = new CountDisplayMeter(meterDisplayName, count, value, ShowLifetime);
                meter.PropertyChanged += OnMeterChangedEvent;

                KeyedOffMeters.Add(meter);
            }

            UpdateMeterTotals();
        }

        protected override void UpdateMeters()
        {
            base.UpdateMeters();

            foreach (var meter in KeyedOnMeters)
            {
                meter.ShowLifetime = ShowLifetime;
            }

            foreach (var meter in KeyedOffMeters)
            {
                meter.ShowLifetime = ShowLifetime;
            }

            UpdateMeterTotals();
        }

        protected override void DisposeInternal()
        {
            ClearMeters();

            base.DisposeInternal();
        }

        private void ClearMeters()
        {
            foreach (var meter in KeyedOnMeters)
            {
                meter.PropertyChanged -= OnMeterChangedEvent;
                meter.Dispose();
            }

            KeyedOnMeters.Clear();

            foreach (var meter in KeyedOffMeters)
            {
                meter.PropertyChanged -= OnMeterChangedEvent;
                meter.Dispose();
            }

            KeyedOffMeters.Clear();
        }

        private void OnMeterChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            UpdateMeterTotals();
        }

        private void UpdateMeterTotals()
        {
            KeyedOnTotalCount = KeyedOnMeters.Sum(m => m.Count);
            KeyedOnTotalValue = KeyedOnMeters.Sum(m => m.MeterValue).FormattedCurrencyString();

            KeyedOffTotalCount = KeyedOffMeters.Sum(m => m.Count);
            KeyedOffTotalValue = KeyedOffMeters.Sum(m => m.MeterValue).FormattedCurrencyString();
        }
    }
}