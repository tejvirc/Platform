namespace Aristocrat.Bingo.Client.Messages
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Progressives;

    /// <summary>
    ///     The progressive contribution service
    /// </summary>
    public interface IProgressiveContributionService
    {
        /// <summary>
        ///     Contribute to a progressive with the server
        /// </summary>
        /// <param name="message">The progressive contribution request message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for contributing to a progressive</returns>
        Task<ProgressiveContributionResponse> Contribute(ProgressiveContributionRequestMessage message, CancellationToken token = default);

        /// <summary>
        ///     Get all the games that are registered to use the specified progressive.
        /// </summary>
        /// <param name="progressiveLevelId">The progressive level Id</param>
        /// <returns>An enumeration of tuples of game title Id and denomination that are registered to use the specified progressive</returns>
        Task<IEnumerable<(int, long)>> GetGamesUsingProgressive(int progressiveLevelId);
    }
}
