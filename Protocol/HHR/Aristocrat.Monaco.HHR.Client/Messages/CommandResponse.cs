namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using Data;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_COMMAND
    ///     Struct:   SMessageCommand
    ///     Request:  GMessageReadyToPlay
    /// </summary>
    public class CommandResponse : Response
    {
        /// <summary>
        /// </summary>
        public CommandResponse()
            : base(Command.CmdCommand)
        {
        }

        /// <summary>
        /// </summary>
        public GtCommand ECommand { get; set; }

        /// <summary>
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// </summary>
        public uint Parameter { get; set; }
    }
}