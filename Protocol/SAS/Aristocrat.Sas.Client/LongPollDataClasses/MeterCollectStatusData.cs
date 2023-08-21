namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>The meter collect status of egm</summary>
    public enum MeterCollectStatus
    {
        /// <summary>Lifetime-to-date meters were initialized, cleared, added, removed, or rearranged</summary>
        LifetimeMeterChange = 0x00,

        /// <summary>Enabled games/denoms changed or the set of paytables available to the player is changed</summary>
        GameDenomPaytableChange = 0x01,

        /// <summary>Not in meter change pending state</summary>
        NotInPendingChange = 0x80
    }

    /// <summary>Holds the response data for Meter Collect Status response</summary>
    public class MeterCollectStatusData : LongPollResponse
    {

        public MeterCollectStatusData()
            : this(MeterCollectStatus.NotInPendingChange)
        {
        }

        public MeterCollectStatusData(MeterCollectStatus status)
        {
            Status = status;
        }

        public MeterCollectStatus Status { get; }
    }
}
