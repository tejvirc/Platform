namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using Contracts;
    using Contracts.Extensions;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter" /> that is denomination-specific
    /// </summary>
    [CLSCompliant(false)]
    public class DenominationDisplayMeter : DisplayMeter
    {
        private readonly int _denomination;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DenominationDisplayMeter" /> class.
        /// </summary>
        public DenominationDisplayMeter(int denomination, IMeter meter, bool showLifetime)
            : base($"{denomination.FormattedCurrencyString()}", meter, showLifetime)
        {
            _denomination = denomination;
        }

        /// <summary>
        ///     The current count of the Meter
        /// </summary>
        public override long Count => ShowLifetime ? Meter.Lifetime : Meter.Period;

        /// <summary>
        ///     The current total value of all bills of this denomination
        /// </summary>
        public double Total => Count * _denomination;

        /// <summary>
        ///     The current formatted Total amount
        /// </summary>
        public new string Value => _denomination > -1 ? $"{Total.FormattedCurrencyString()}" : base.Value;

        /// <inheritdoc />
        protected override void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            base.OnMeterChangedEvent(sender, e);
            RaisePropertyChanged(nameof(Count));
        }
    }
}