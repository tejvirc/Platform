namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;

    /// <summary>
    ///     Provides methods to support reporting events to the bingo server
    /// </summary>
    public interface IReportEventService
    {
        /// <summary>
        ///     Reports an Event
        /// </summary>
        /// <param name="message">The event message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for acknowledging the event report</returns>
        Task<ReportEventAck> ReportEvent(ReportEventMessage message, CancellationToken token);
    }
}