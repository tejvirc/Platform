namespace Aristocrat.Monaco.Application.UI.MeterPage
{
    using System;
    using Contracts;
    using Contracts.Extensions;
    using Kernel;

    /// <summary>
    ///     This is the display version of an <see cref="IMeter"/> that contains a currency value 
    /// </summary>
    [CLSCompliant(false)]
    public class ValueDisplayMeter : DisplayMeter
    {
        private readonly double _multiplier;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueDisplayMeter" /> class.
        /// </summary>
        public ValueDisplayMeter(string meterName, IMeter valueMeter, bool showLifetime, int order = 0)
            : base(meterName, valueMeter, showLifetime, order)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _multiplier = 1.0 / (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);
        }

        /// <summary>
        ///     The current meter value
        /// </summary>
        public double MeterValue => _multiplier * (ShowLifetime ? Meter.Lifetime : Meter.Period);

        /// <summary>
        ///     The current formatted meter value
        /// </summary>
        public new string Value => $"{MeterValue.FormattedCurrencyString()}";
    }
}