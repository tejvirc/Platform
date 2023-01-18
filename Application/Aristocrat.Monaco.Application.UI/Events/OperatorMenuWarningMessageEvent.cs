namespace Aristocrat.Monaco.Application.UI.Events
{
    using System;
    using Kernel;
    using ProtoBuf;

    [ProtoContract]
    public class OperatorMenuWarningMessageEvent : BaseEvent
    {
        public OperatorMenuWarningMessageEvent(string message = null)
        {
            Message = message;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public OperatorMenuWarningMessageEvent() : this(null)
        {
        }

        [ProtoMember(1)]
        public string Message { get; }
    }
}
