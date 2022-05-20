namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Data class for the DisableEnable long polls
    /// </summary>
    public class EnableDisableData : LongPollMultiDenomAwareData
    {
        /// <summary> Gets or sets the id of the game to enable/disable </summary>
        public int Id { get; set; }

        /// <summary> Gets or sets a value indicating whether to enable the game or not </summary>
        public bool Enable { get; set; }
    }

    public class EnableDisableResponse : LongPollMultiDenomAwareResponse
    {
        /// <summary> Gets or sets a value indicating whether the operation was successful </summary>
        public bool Succeeded { get; set; }

        /// <summary> Gets or sets a value indicating whether the game is busy </summary>
        public bool Busy { get; set; }
    }
}