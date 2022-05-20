namespace Aristocrat.G2S.Emdi.Host
{
    using Protocol.v21ext1b1;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for sending events and meters to media display content
    /// </summary>
    public interface IReporter
    {
        /// <summary>
        /// Sends a single event to media display content
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        Task ReportAsync(c_eventReportEventItem @event, params string[] eventCodes);

        /// <summary>
        /// Sends a list of events to media display content
        /// </summary>
        /// <param name="events"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        Task ReportAsync(IEnumerable<c_eventReportEventItem> events, params string[] eventCodes);

        /// <summary>
        /// Sends a single meter to media display content
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="meterNames"></param>
        /// <returns></returns>
        Task ReportAsync(c_meterInfo meter, params string[] meterNames);

        /// <summary>
        /// Sends a list of meters to media display content
        /// </summary>
        /// <param name="meters"></param>
        /// <param name="meterNames"></param>
        /// <returns></returns>
        Task ReportAsync(IEnumerable<c_meterInfo> meters, params string[] meterNames);
    }
}
