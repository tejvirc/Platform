namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Contracts.AlteredMediaLogger;
    using Contracts.Localization;
    using Contracts.TiltLogger;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Log adapter for handling/transforming Altered media(Software change) events/transactions.
    /// </summary>
    public class AlteredMediaEventLogAdapter : BaseEventLogAdapter, IEventLogAdapter
    {
        public string LogType => EventLogType.SoftwareChange.GetDescription(typeof(EventLogType));

        public IEnumerable<EventDescription> GetEventLogs()
        {
            var alteredMediaLogger = ServiceManager.GetInstance().GetService<IAlteredMediaLogger>();
            var alteredMediaLogs = alteredMediaLogger.Logs.OrderByDescending(l => l.TimeStamp).ToList();
            var events = from alteredMediaLog in alteredMediaLogs
                         let additionalInfo = new[]{
                             GetDateAndTimeHeader(alteredMediaLog.TimeStamp),
                             (ResourceKeys.MediaType2, alteredMediaLog.MediaType),
                             (ResourceKeys.Reason, alteredMediaLog.ReasonForChange),
                             (ResourceKeys.AuthenticationInformation, alteredMediaLog.Authentication)}
                         let name = string.Join(
                             EventLogUtilities.EventDescriptionNameDelimiter,
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SoftwareChange),
                             alteredMediaLog.MediaType + " " + alteredMediaLog.ReasonForChange)
                         select new EventDescription(
                             name,
                             "info",
                             LogType,
                             alteredMediaLog.TransactionId,
                             Guid.NewGuid(),
                             alteredMediaLog.TimeStamp,
                             additionalInfo);
            return events;
        }

        public long GetMaxLogSequence() => -1;
    }
}
