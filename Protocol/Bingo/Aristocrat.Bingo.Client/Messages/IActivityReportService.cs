namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IActivityReportService
    {
        /// <summary>
        ///     Reports activity to the server
        /// </summary>
        /// <param name="message">The message to report to the server</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The task for reporting activity to the bingo server</returns>
        Task<ActivityResponseMessage> ReportActivity(ActivityReportMessage message, CancellationToken token = default);
    }
}