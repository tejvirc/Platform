namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     An implementation of <see cref="IMeterAggregator{TDevice}" />
    /// </summary>
    public class NoteAcceptorMeterAggregator : MeterAggregatorBase<INoteAcceptorDevice>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorMeterAggregator" /> class.
        /// </summary>
        /// <param name="meterManager">An instance of an IMeterManager.</param>
        public NoteAcceptorMeterAggregator(IMeterManager meterManager)
            : base(meterManager, MeterMap.CurrencyInMeters)
        {
        }
    }
}
