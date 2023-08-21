namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;

    public class HandpayMeterAggregator : MeterAggregatorBase<IHandpayDevice>
    {
        public HandpayMeterAggregator(IMeterManager meterManager)
            : base(meterManager, MeterMap.HandpayMeters)
        {
        }
    }
}
