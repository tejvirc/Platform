namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Progressives;

    /// <summary>
    ///     The progressive award service
    /// </summary>
    public interface IProgressiveAwardService
    {
        /// <summary>
        ///     Inform progressive server that a progressive has been awarded.
        /// </summary>
        /// <param name="message">The progressive award request message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for awarding a progressive</returns>
        Task<ProgressiveAwardResponse> AwardProgressive(ProgressiveAwardRequestMessage message, CancellationToken token = default);
    }
}
