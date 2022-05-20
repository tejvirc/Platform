namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_GT_READY_TO_PLAY
    ///     Struct:   GMessageReadyToPlay
    ///     Response: SMessageCommand
    /// </summary>
    public class ReadyToPlayRequest : Request
    {
        /// <summary>
        /// </summary>
        public ReadyToPlayRequest()
            : base(Command.CmdGtReadyToPlay)
        {
        }
    }
}