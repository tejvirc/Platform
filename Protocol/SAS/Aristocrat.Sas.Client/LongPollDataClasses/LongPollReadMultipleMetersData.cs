namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Holds data to request multiple meter values
    /// </summary>
    public class LongPollReadMultipleMetersData : LongPollData
    {
        public Collection<LongPollReadMeterData> Meters { get; } = new Collection<LongPollReadMeterData>();

        public long AccountingDenom { get; set; }
    }

    /// <summary>
    ///     Provides the data for multiple meter values
    /// </summary>
    public class LongPollReadMultipleMetersResponse : LongPollResponse
    {
        public LongPollReadMultipleMetersResponse()
            : this(new Dictionary<SasMeters, LongPollReadMeterResponse>())
        {
        }

        public LongPollReadMultipleMetersResponse(Dictionary<SasMeters, LongPollReadMeterResponse> meters)
        {
            Meters = meters;
        }

        public Dictionary<SasMeters, LongPollReadMeterResponse> Meters { get; }
    }
}