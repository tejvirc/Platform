namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;
    using System.Text;
    using Protocol.Common.Logging;

    /// <summary>
    ///     Base message for all aristocrat response messages from central server. Subclasses should populate these fields in
    ///     their constructors, so that
    ///     these features can be used by all messages.
    /// </summary>
    public class Response
    {
        private const int MaxLoggingWidth = 4096;

        /// <summary>
        ///     Constructor forces subclasses to provide the command number for this response.
        /// </summary>
        /// <param name="responseType">The command number for this response.</param>
        public Response(Command responseType)
        {
            Command = responseType;
        }

        /// <summary>
        ///     Default constructor that sets the command number to indicate this is an unknown response.
        /// </summary>
        public Response() : this(Command.CmdInvalidCommand) {}

        /// <summary>
        ///     The command number associated with this message. Will be populated by the subclass so we can use it when encoding.
        /// </summary>
        public Command Command;

        /// <summary>
        ///     Reply ID indicating the sequence id of request for which this response is received.
        /// </summary>
        public uint ReplyId;

        /// <summary>
        ///     Status of the message.
        /// </summary>
        public MessageStatus MessageStatus = MessageStatus.Success;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"MsgId={ReplyId}, Cmd={Command}, MsgStatus={MessageStatus}";
        }

        /// <summary>
        ///     Generates a string that can be logged for debugging. This function splits large messages across
        ///     multiple lines so that the log files are not so unwieldy.
        /// </summary>
        public string MessageData()
        {
            string resultString = this.ToJson();

            if (resultString.Length < MaxLoggingWidth)
            {
                return resultString;
            }

            StringBuilder resultOutput = new StringBuilder();
            var index = 0;
            while (index < resultString.Length)
            {
                string resultPart = resultString.Substring(index, Math.Min(MaxLoggingWidth, resultString.Length - index));
                if (index > 0)
                {
                    resultOutput.AppendLine();
                }

                resultOutput.Append(resultPart);

                index += MaxLoggingWidth;
            }

            return resultOutput.ToString();
        }
    }
}