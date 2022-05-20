namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class RomSignatureData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the seed to use for this long poll
        /// </summary>
        public ushort Seed { get; set; }

        /// <summary>
        ///     Gets or sets the client number for this long poll
        /// </summary>
        public byte ClientNumber { get; set; }
    }
}