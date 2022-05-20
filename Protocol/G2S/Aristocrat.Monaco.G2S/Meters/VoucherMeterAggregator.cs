namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     An implementation of <see cref="IMeterAggregator{IVoucherDevice}" />
    /// </summary>
    public class VoucherMeterAggregator : MeterAggregatorBase<IVoucherDevice>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherMeterAggregator" /> class.
        /// </summary>
        /// <param name="meterManager">An instance of an IMeterManager.</param>
        public VoucherMeterAggregator(IMeterManager meterManager)
            : base(meterManager, MeterMap.VoucherMeters)
        {
        }
    }
}
