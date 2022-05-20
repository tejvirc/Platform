namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    /// <summary>
    ///     The Simple ACK Response interface
    /// </summary>
    public interface ISimpleAckResponse
    {
        /// <summary>
        ///     Gets or sets the command id
        /// </summary>
        HarkeyCommandId CommandId { get; set; }

        /// <summary>
        ///     Gets or sets the response code
        /// </summary>
        int ResponseCode { get; set; }
    }
}