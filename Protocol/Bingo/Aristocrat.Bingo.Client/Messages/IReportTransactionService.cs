namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides methods to support reporting Transactions to the bingo server
    /// </summary>
    public interface IReportTransactionService
    {
        /// <summary>
        ///     Reports a Transaction
        /// </summary>
        /// <param name="message">The transaction message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for acknowledging the transaction</returns>
        Task<ReportTransactionResponse> ReportTransaction(ReportTransactionMessage message, CancellationToken token);
    }
}