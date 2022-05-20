namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using Common;

    /// <summary>
    ///     Interface for Consumers to use to report bingo server events
    /// </summary>
    public interface IReportEventQueueService
    {
        /// <summary>
        ///     Adds a bingo server event to the queue
        /// </summary>
        /// <param name="eventType">The event type</param>
        void AddNewEventToQueue(ReportableEvent eventType);
    }
}