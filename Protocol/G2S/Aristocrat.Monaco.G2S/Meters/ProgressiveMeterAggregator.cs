namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;

    public class ProgressiveMeterAggregator : MeterAggregatorBase<IProgressiveDevice>
    {
        public ProgressiveMeterAggregator(IMeterManager meterManager)
            : base(meterManager, MeterMap.ProgressiveMeters)
        {
        }
    }
}
