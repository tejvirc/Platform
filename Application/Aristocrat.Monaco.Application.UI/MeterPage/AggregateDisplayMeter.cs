namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter" /> that aggregates the Values of
    ///     the given meters.
    /// </summary>
    [CLSCompliant(false)]
    public class AggregateDisplayMeter : DisplayMeter
    {
        private readonly IList<IMeter> _meters;
        private readonly MeterClassification _meterClassification;
        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateDisplayMeter" /> class.
        /// </summary>
        public AggregateDisplayMeter(string meterName, IList<IMeter> meters, bool showLifetime, MeterClassification meterClassification, int order, bool useOperatorCultureForCurrency = false)
            : base(meterName, null, showLifetime, order, true, false, useOperatorCultureForCurrency)
        {
            _meters = meters;
            _meterClassification = meterClassification;
        }

        /// <summary>
        ///     The current count of the Meter
        /// </summary>
        public override long Count => ShowLifetime ? Meter.Lifetime : Meter.Period;

        /// <summary>
        ///     The total amount of the meters Values
        /// </summary>
        public long AggregatedValue => _meters.Sum(meter => ShowLifetime ? meter.Lifetime : meter.Period);

        /// <summary>
        ///     The current formatted aggregated amount
        /// </summary>
        public override string Value => _meterClassification.CreateValueString(AggregatedValue);

        /// <inheritdoc />
        protected override void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            base.OnMeterChangedEvent(sender, e);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(AggregatedValue));
            OnPropertyChanged(nameof(Value));
        }
    }
}