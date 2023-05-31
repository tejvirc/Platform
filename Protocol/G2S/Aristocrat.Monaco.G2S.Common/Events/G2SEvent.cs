namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Localization.Properties;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Event used to notify after a G2S event code has been or would have been sent.
    /// </summary>
    public class G2SEvent : BaseEvent
    {
        /// <summary>
        /// Event associated with the event code. May be null if not associated with an original event.
        /// </summary>
        public IEvent InternalEvent { get; protected set; }

        /// <summary>
        /// G2S event code that was sent.
        /// </summary>
        public string EventCode { get; protected set; }

        /// <summary>
        /// Creates an event that is associated with the sending of a G2S event code.
        /// </summary>
        /// <param name="eventCode">G2S event code that was sent.</param>
        /// <param name="associatedEvent">Event associated with this event code.</param>
        /// <exception cref="ArgumentException"></exception>
        public G2SEvent(string eventCode, IEvent associatedEvent = null)
        {
            EventCode = !string.IsNullOrEmpty(eventCode) ? eventCode : throw new ArgumentException(nameof(eventCode));

            InternalEvent = associatedEvent;
        }

        /// <inheritdoc/>
        public (string, string)[] GetAdditionalInfo()
        {
            return new (string, string)[]
            {
                (ResourceKeys.G2SEventCode, EventCode),
                (ResourceKeys.G2SEventCodeDescription, GetEventCodeDescription())
            };
        }

        private string GetEventCodeDescription()
        {
            var attributes = typeof(Aristocrat.G2S.EventCode).GetField(EventCode)?.CustomAttributes;
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    var description = attribute.NamedArguments.FirstOrDefault(
                        n => n.MemberName.Equals("Description", StringComparison.OrdinalIgnoreCase));

                    if (description != default(CustomAttributeNamedArgument))
                    {
                        return description.TypedValue.ToString();
                    }
                }
            }

            return string.Empty;
        }
    }
}
