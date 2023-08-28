namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using Application.Contracts;

    /// <summary>
    ///     The Information about the bonus meter
    /// </summary>
    public class BonusInfoMeter : IDisposable
    {
        /// <summary>
        ///     Create a new Meter
        /// </summary>
        /// <param name="meterName"></param>
        /// <param name="showLifetime"></param>
        /// <param name="meter"></param>
        public BonusInfoMeter(string meterName, bool showLifetime, IMeter meter)
        {
            MeterName = meterName;
            ShowLifetime = showLifetime;
            Meter = meter ?? throw new ArgumentNullException();
        }

        /// <summary>
        ///     The name of the meter to display
        /// </summary>
        public string MeterName { get; }

        /// <summary>
        ///     Show lifetime vs period
        /// </summary>
        public bool ShowLifetime { get; }

        /// <summary>
        ///     The actual meter for the bonus
        /// </summary>
        public IMeter Meter { get; }

        /// <summary>
        ///     Calculate the value of the meter
        /// </summary>
        public long MeterValue => ShowLifetime ? Meter.Lifetime : Meter.Session;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Meter is CompositeMeter meter)
            {
                meter.Dispose();
            }
        }
    }
}
