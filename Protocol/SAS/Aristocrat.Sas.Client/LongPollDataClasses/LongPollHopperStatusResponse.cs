namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Response class to hold the response of LP 4F - Send Current Hopper Status
    /// </summary>
    public class LongPollHopperStatusResponse : LongPollResponse
    {
        /// <summary>
        ///     Hopper Statuses
        /// </summary>
        public enum HopperStatus
        {
            /// <summary>HopperOk</summary>
            HopperOk = 0x00,

            /// <summary>FloodedOptics</summary>
            FloodedOptics = 0x01,

            /// <summary>ReverseCoin</summary>
            ReverseCoin = 0x02,

            /// <summary>CoinTooShort</summary>
            CoinTooShort = 0x03,

            /// <summary>CoinJam</summary>
            CoinJam = 0x04,

            /// <summary>HopperRunaway</summary>
            HopperRunaway = 0x05,

            /// <summary>OpticsDisconnected</summary>
            OpticsDisconnected = 0x06,

            /// <summary>HopperEmpty</summary>
            HopperEmpty = 0x07,

            /// <summary>Other</summary>
            Other = 0xFF
        }

        /// <summary>
        ///     Initializes a new empty LongPollHopperStatusResponse.
        /// </summary>
        public LongPollHopperStatusResponse()
        {
        }

        /// <summary>
        ///     Initializes a new empty LongPollHopperStatusResponse.
        /// </summary>
        /// <param name="percentFull">Current hopper level as 0-100%, or FF if unable to detect hopper level percentage.</param>
        /// <param name="level">Current hopper level in number of coins/tokens.</param>
        /// <param name="status">Hopper status.</param>
        public LongPollHopperStatusResponse(byte percentFull, long level, HopperStatus status)
        {
            PercentFull = percentFull;
            Level = level;
            Status = status;
        }

        /// <summary>
        ///     Current hopper level as 0-100%, or FF if unable to detect hopper level percentage.
        /// </summary>
        public byte PercentFull { get; set; }

        /// <summary>
        ///     Current hopper level in number of coins/tokens.
        /// </summary>
        public long Level { get; set; }

        /// <summary>
        ///     Hopper status.
        /// </summary>
        public HopperStatus Status { get; set; }
    }
}