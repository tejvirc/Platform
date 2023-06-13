namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Contracts;
    using Contracts.Extensions;
    using Kernel;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter"/> that contains both a Value and a Count
    /// </summary>
    [CLSCompliant(false)]
    public class CountDisplayMeter : DisplayMeter
    {
        private readonly double _multiplier;
        private readonly bool _useOperatorCultureForCurrency;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="CountDisplayMeter" /> class.
        /// </summary>
        public CountDisplayMeter(string meterName, IMeter countMeter, IMeter valueMeter, bool showLifetime, int order = 0, bool useOperatorCultureForCurrency = false)
            : base(meterName, null, showLifetime, order, true, false, useOperatorCultureForCurrency)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = 1.0 / (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);

            _useOperatorCultureForCurrency = useOperatorCultureForCurrency;

            CountMeter = countMeter;
            CountMeter.MeterChangedEvent += OnMeterChangedEvent;

            ValueMeter = valueMeter;
            ValueMeter.MeterChangedEvent += OnMeterChangedEvent;
        }

        /// <summary>
        ///     Count meter
        /// </summary>
        public IMeter CountMeter { get; }

        /// <summary>
        ///     Value meter
        /// </summary>
        public IMeter ValueMeter { get; }

        /// <summary>
        ///     The current meter count
        /// </summary>
        public override long Count => ShowLifetime ? CountMeter.Lifetime : CountMeter.Period;

        /// <summary>
        ///     The current meter value
        /// </summary>
        public double MeterValue => _multiplier * (ShowLifetime ? ValueMeter.Lifetime : ValueMeter.Period);

        /// <summary>
        ///     The current formatted meter value
        /// </summary>
        public new string Value
        {
            get
            {
                var culture = _useOperatorCultureForCurrency ?
                    Localizer.For(CultureFor.Operator).CurrentCulture :
                    CurrencyExtensions.CurrencyCultureInfo;

                return MeterValue.FormattedCurrencyString(false, culture);
            }
        }

        /// <inheritdoc />
        protected override void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(nameof(MeterValue));
            RaisePropertyChanged(nameof(Value));
            RaisePropertyChanged(nameof(ShowLifetime));
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Meter != null)
            {
                Meter.MeterChangedEvent -= OnMeterChangedEvent;
            }

            if (CountMeter != null)
            {
                CountMeter.MeterChangedEvent -= OnMeterChangedEvent;
            }

            if (ValueMeter != null)
            {
                ValueMeter.MeterChangedEvent -= OnMeterChangedEvent;
            }
        }
    }
}
