namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;

    /// <summary>
    ///     Represents a message that we want to send or receive from the HHR server. The corresponding binary
    ///     struct will be populated with these values and other protocol specific information and then serialized
    ///     for transmission, or vice versa.
    ///     Command:  CMD_CLOSE_TRAN_ERROR
    ///     Struct:   MessageCloseTranError
    ///     Request:  (Various)
    /// </summary>
    public class CloseTranErrorResponse : Response
    {
        /// <summary>
        /// </summary>
        public CloseTranErrorResponse()
            : base(Command.CmdCloseTranError)
        {
        }

        /// <summary>
        ///     Status as returned by server.
        /// </summary>
        public Status Status;

        /// <summary>
        ///     Wait time before retry.
        /// </summary>
        public TimeSpan RetryTime;

        /// <summary>
        ///     Error text
        /// </summary>
        public string ErrorText;

        /// <summary>
        ///     Error code
        /// </summary>
        public uint ErrorCode;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"MsgId={ReplyId}, Cmd={Command}, Status={Status}, ErrCode={ErrorCode}, ErrMsg={ErrorText}";
        }
    }
}