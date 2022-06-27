namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using WorkFlow;
    using Newtonsoft.Json;

    /// <summary>
    ///     Base message for all Ainsworth request messages to central server. Subclasses should populate these fields in
    ///     their constructors, so that these features can be used by all messages.
    /// </summary>
    public class Request
    {
        /// <summary>
        ///     The command associated with this message. Will be populated by the subclass so we can use it when encoding.
        /// </summary>
        public Command Command;

        /// <summary>
        ///     The timeout that we should wait for a response when sending this message. Zero indicates we do not expect a
        ///     response.
        /// </summary>
        public int TimeoutInMilliseconds = 5000;

        /// <summary>
        ///     The number of times to retry sending this message if we do not get a response within the timeout.
        /// </summary>
        public int RetryCount = 0;

        /// <summary>
        ///     SequenceId of the request.
        /// </summary>
        public uint SequenceId;

        /// <summary>
        ///     What happens when this request timeout.
        /// </summary>
        [JsonIgnore]
        public IRequestTimeout RequestTimeout = new IdleRequestTimeout();

        /// <summary>
        ///     Constructor forces subclasses to provide the command number for this response.
        /// </summary>
        /// <param name="requestType">The command number for this response.</param>
        public Request(Command requestType)
        {
            Command = requestType;
        }

        /// <summary>
        ///     Indicates whether this request is failed or not.
        ///     RequestTimeoutBehaviourHandler will retry failed requests.
        /// </summary>
		public bool IsFailed { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"MsgId={SequenceId}, Cmd={Command}";
        }
    }
}