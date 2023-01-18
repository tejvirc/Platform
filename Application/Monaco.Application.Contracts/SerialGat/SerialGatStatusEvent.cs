namespace Aristocrat.Monaco.Application.Contracts.SerialGat
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to disseminate serial GAT status message
    /// </summary>
    [ProtoContract]
    public class SerialGatStatusEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="statusMessage">Status message</param>
        public SerialGatStatusEvent(string statusMessage)
        {
            StatusMessage = statusMessage;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public SerialGatStatusEvent()
        {
        }

        /// <summary>
        ///     Status message text
        /// </summary>
        [ProtoMember(1)]
        public string StatusMessage { get; }
    }
}