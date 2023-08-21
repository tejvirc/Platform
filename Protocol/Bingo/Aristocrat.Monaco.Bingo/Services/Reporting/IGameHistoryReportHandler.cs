namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    /// <summary>
    ///     The handler for reporting the game history to the server
    /// </summary>
    public interface IGameHistoryReportHandler
    {
        /// <summary>
        ///     Adds the game history report to the queue
        /// </summary>
        /// <param name="message">The message to add to the queue</param>
        void AddReportToQueue(ReportGameOutcomeMessage message);
    }
}