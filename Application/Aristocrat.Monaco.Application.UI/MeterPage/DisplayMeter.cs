namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter"/>. 
    ///     This is created by the meters page viewmodel implementation for use in meters page UIs.
    /// </summary>
    [CLSCompliant(false)]
    public class DisplayMeter : BaseViewModel, IDisposable
    {
        private bool _showLifetime;
        private bool _useOperatorCultureForCurrency;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayMeter"/> class.
        /// </summary>
        /// <param name="meterName">The text to display on the meters page in the Meter column</param>
        /// <param name="meter">The meter that provides the Period and Lifetime values that are displayed in the Value column</param>
        /// <param name="showLifetime">Whether to show the period values or the lifetime values</param>
        /// <param name="order">The placement of the meter on the screen relative to other meters</param>
        /// <param name="displayPeriod">Show the meter on the screen in Period mode</param>
        /// <param name="showNotApplicable">if meter not found, show N/A in the audit menu</param>
        /// <param name="showNotApplicable">Use the current operator culture rather than the currency culture</param>
        public DisplayMeter(
            string meterName,
            IMeter meter,
            bool showLifetime,
            int order = 0,
            bool displayPeriod = true,
            bool showNotApplicable = false,
            bool useOperatorCultureForCurrency = false)
        {
            Name = meterName;
            Meter = meter;
            Order = order;
            DisplayPeriod = displayPeriod;
            ShowNotApplicable = showNotApplicable;
            _showLifetime = showLifetime;
            _useOperatorCultureForCurrency = useOperatorCultureForCurrency;

            if (Meter != null)
            {
                Meter.MeterChangedEvent += OnMeterChangedEvent;
            }
        }

        /// <summary>
        ///     The <see cref="IMeter"/> associated with this DisplayMeter
        /// </summary>
        public IMeter Meter { get; }

        /// <summary>
        ///     The display name of the meter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Display of the count meter. This is too allow the derived class to catch the
        /// property change for binding
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public virtual long Count { get; }

        /// <summary>
        ///     The display order of this meter relative to other meters
        /// </summary>
        public int Order { get; }

        /// <summary>
        ///     Show the meter on the screen in Period mode
        /// </summary>
        public bool DisplayPeriod { get; }

        /// <summary>
        ///     if meter cannot be found, display "N/A"
        /// </summary>
        public bool ShowNotApplicable { get; }

        /// <summary>
        ///    Binding property to hide the row
        /// </summary>
        public bool HideRowForPeriod => !DisplayPeriod && !ShowLifetime;

        /// <summary>
        ///     The formatted value of the meter
        /// </summary>
        public virtual string Value
        {
            get
            {
                if (Meter == null)
                {
                    return ShowNotApplicable ?
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MeterNotApplicable) :
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MeterNotFound);
                }

                var meterValue = ShowLifetime ? Meter.Lifetime : Meter.Period;
                RaisePropertyChanged(nameof(HideRowForPeriod));

                var culture = _useOperatorCultureForCurrency ?
                    Localizer.For(CultureFor.Operator).CurrentCulture : CurrencyExtensions.CurrencyCultureInfo;

                return Meter.Classification.CreateValueString(meterValue, culture);
            }
        }

        /// <summary>
        ///     Whether to show the lifetime value of the meter versus the period value
        /// </summary>
        public bool ShowLifetime
        {
            get => _showLifetime;
            set
            {
                SetProperty(ref _showLifetime, value, nameof(Value), nameof(Count));
                RaisePropertyChanged(nameof(HideRowForPeriod));
            }
        }

        public void OnMeterChangedEvent()
        {
            OnMeterChangedEvent(this, null);
        }

        /// <summary>
        ///     This event is called when the Meter property changes
        /// </summary>
        protected virtual void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Value));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of managed and native resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (Meter != null)
                {
                    Meter.MeterChangedEvent -= OnMeterChangedEvent;
                }
            }

            _disposed = true;
        }
    }
}
