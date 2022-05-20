namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     An implementation of <see cref="IMeterAggregator{IBonusDevice}" />
    /// </summary>
    public class BonusMeterAggregator : MeterAggregatorBase<IBonusDevice>
    {
        public BonusMeterAggregator(IMeterManager meterManager)
            : base(meterManager, MeterMap.BonusMeters)
        {
        }
    }
}