namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Contracts;
    using Contracts.Extensions;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter" /> that is denomination-specific
    /// </summary>
    [CLSCompliant(false)]
    public class DenominationDisplayMeter : DisplayMeter
    {
        private readonly int _denomination;
        private readonly bool _useOperatorCultureForCurrency;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DenominationDisplayMeter" /> class.
        /// </summary>
        public DenominationDisplayMeter(int denomination, IMeter meter, bool showLifetime, bool useOperatorCultureForCurrency = false)
            : base($"{GetFormattedDenominationString(denomination, useOperatorCultureForCurrency)}", meter, showLifetime, 0, true, false, useOperatorCultureForCurrency)
        {
            _denomination = denomination;
            _useOperatorCultureForCurrency = useOperatorCultureForCurrency;
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
        public new string Value
        {
            get
            {
                if (_denomination > -1)
                {
                    var culture = _useOperatorCultureForCurrency ?
                        Localizer.For(CultureFor.Operator).CurrentCulture :
                        CurrencyExtensions.CurrencyCultureInfo;

                    return Total.FormattedCurrencyString(false, culture);
                }

                return base.Value;
            }
        }

        /// <inheritdoc />
        protected override void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            base.OnMeterChangedEvent(sender, e);
            RaisePropertyChanged(nameof(Count));
        }

        private static string GetFormattedDenominationString(int denomination, bool useOperatorCultureForCurrency)
        {
            var culture = useOperatorCultureForCurrency ?
                Localizer.For(CultureFor.Operator).CurrentCulture :
                CurrencyExtensions.CurrencyCultureInfo;

            return denomination.FormattedCurrencyString(null, culture);
        }
    }
}