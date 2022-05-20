namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    /// <summary>
    ///     The Spin Response interface
    /// </summary>
    public interface ISpinResponse
    {
        /// <summary>
        ///     Gets or sets the sequence id for the command
        /// </summary>
        byte SequenceId { get; set; }

        /// <summary>
        ///     Gets or sets the first response code
        /// </summary>
        int ResponseCode1 { get; set; }

        /// <summary>
        ///     Gets or sets the second response code
        /// </summary>
        int ResponseCode2 { get; set; }
    }
}
