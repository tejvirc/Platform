namespace Aristocrat.G2S.Emdi.Extensions
{
    using Monaco.Kernel;
    using Protocol.v21ext1b1;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for events
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Creates report events
        /// </summary>
        /// <param name="theEvent"></param>
        /// <param name="createItem"></param>
        /// <param name="eventCodes"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public static Task<IEnumerable<c_eventReportEventItem>> CreateReportEventsAsync<TEvent>(this TEvent theEvent, Func<TEvent, string, object> createItem, params string[] eventCodes)
            where TEvent : BaseEvent
        {
            return Task.FromResult(eventCodes.Select(
                code => new c_eventReportEventItem
                {
                    eventCode = code,
                    Item = createItem?.Invoke(theEvent, code)
                }));
        }

        /// <summary>
        /// Creates report events
        /// </summary>
        /// <param name="theEvent"></param>
        /// <param name="eventCodes"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public static Task<IEnumerable<c_eventReportEventItem>> CreateReportEventsAsync<TEvent>(this TEvent theEvent, params string[] eventCodes)
            where TEvent : BaseEvent
        {
            return theEvent.CreateReportEventsAsync(null, eventCodes);
        }
    }
}
