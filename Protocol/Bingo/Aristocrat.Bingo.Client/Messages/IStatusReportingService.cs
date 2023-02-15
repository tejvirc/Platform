namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IStatusReportingService
    {
        /// <summary>
        ///     Sends the EGM Status and 4 meter values to the Bingo Server
        /// </summary>
        /// <param name="message">The status message data</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The task for sending the response to the bingo server</returns>
        Task ReportStatus(StatusMessage message, CancellationToken token = default);
    }
}