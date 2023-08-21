namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_CLOSE_TRAN
    ///     Struct:   MessageCloseTran
    ///     Request:  (Various)
    /// </summary>
    public class CloseTranResponse : Response
    {
        /// <summary>
        /// </summary>
        public CloseTranResponse()
            : base(Command.CmdCloseTran)
        {
        }

        /// <summary>
        ///     Status as returned by server.
        /// </summary>
        public Status Status;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"MsgId={ReplyId}, Cmd={Command}, Status={Status}";
        }
    }
}