namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using log4net;

    public abstract class SasLongPollParser<TResponse, TData> : ILongPollParser where TData : LongPollData, new() where TResponse : LongPollResponse
    {
        // create a logger for use in derived classes
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly TData Data = new TData();

        // Until the InjectHandler method is called we will return null to indicate we don't handle the long poll
        protected Func<TData, TResponse> Handler = data => null;

        /// <inheritdoc/>>
        public IHostAcknowledgementHandler Handlers { get; set; }

        /// <inheritdoc/>>
        public LongPoll Command { get; }

        /// <inheritdoc/>>
        public abstract IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command);

        /// <inheritdoc/>>
        public virtual void InjectHandler(object handler)
        {
            Handler = ((ISasLongPollHandler<TResponse, TData>)handler).Handle;
        }

        /// <summary>
        ///     Handles processing the long poll
        /// </summary>
        /// <param name="data">The data needed by the long poll</param>
        /// <returns>The data needed to compose a response to the long poll</returns>
        public TResponse Handle(TData data)
        {
            return Handler(data);
        }

        /// <summary>
        ///     Generates a SAS ACK response for the long poll
        /// </summary>
        /// <param name="command">the long poll message</param>
        /// <param name="callbacks">the optional implied ack callback methods</param>
        /// <returns>The ACK response to the message</returns>
        protected Collection<byte> AckLongPoll(IReadOnlyCollection<byte> command, IHostAcknowledgementHandler callbacks = null)
        {
            Handlers = callbacks;
            return new Collection<byte> { command.First() };
        }

        /// <summary>
        ///     Generates a SAS NACK response for the long poll
        /// </summary>
        /// <param name="command">the long poll message</param>
        /// <returns>The NACK response to the message</returns>
        protected Collection<byte> NackLongPoll(IReadOnlyCollection<byte> command)
        {
            return new Collection<byte> { (byte)(command.First() | SasConstants.Nack) };
        }

        /// <summary>
        ///     Generates a SAS Busy response for the long poll
        /// </summary>
        /// <param name="command">the long poll message</param>
        /// <returns>The BUSY response to the message</returns>
        protected Collection<byte> BusyResponse(IReadOnlyCollection<byte> command)
        {
            return new Collection<byte> { command.First(), 0x00 };
        }

        /// <summary>
        ///     Instantiates a new instance of the SasLongPollParser class
        /// </summary>
        /// <param name="command">The long poll command</param>
        protected SasLongPollParser(LongPoll command)
        {
            Command = command;
        }
    }
}