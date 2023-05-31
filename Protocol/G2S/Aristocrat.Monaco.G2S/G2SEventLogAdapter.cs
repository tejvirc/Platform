namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S;
    using Aristocrat.Monaco.Application;
    using Aristocrat.Monaco.Application.Contracts.TiltLogger;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.G2S.Common.Events;
    using Aristocrat.Monaco.G2S.Common.G2SEventLogger;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Localization.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Event log adapter for G2SEvents
    /// </summary>
    public class G2SEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        public string LogType => EventLogType.Protocol.GetDescription(typeof(EventLogType));

        public G2SEventLogAdapter() { }

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var g2sEventLogger = ServiceManager.GetInstance().GetService<IG2SEventLogger>();
            var g2sEventLogs = g2sEventLogger.Logs.OrderByDescending(l => l.TimeStamp).ToList();
            var events = from g2sEventLog in g2sEventLogs
                         let additionalInfo = new[]
                         {
                             GetDateAndTimeHeader(g2sEventLog.TimeStamp),
                             (ResourceKeys.G2SEventCode, g2sEventLog.EventCode),
                             (ResourceKeys.G2SEventCodeDescription, GetEventCodeDescription(g2sEventLog.EventCode))
                         }
                         let name = GetEventLogName(g2sEventLog)
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             g2sEventLog.TransactionId,
                             g2sEventLog.TimeStamp,
                             additionalInfo);

            return events;
        }

        public long GetMaxLogSequence() => -1;

        private string GetEventCodeDescription(string eventCode)
        {
            var attributes = typeof(EventCode).GetField(eventCode)?.CustomAttributes;
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

        private string GetEventLogName(G2SEventLogMessage eventLog)
        {
            bool hasInternalEvent = !string.IsNullOrEmpty(eventLog.InternalEventType);
            Type eventType = hasInternalEvent ? Type.GetType(eventLog.InternalEventType) : typeof(G2SEvent);

            var result = eventType?.Name ?? typeof(G2SEvent).Name;
            if (!hasInternalEvent)
            {
                result = $"{ result } { eventLog.EventCode }";
            }

            return result;
        }
    }
}
