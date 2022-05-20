namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Data;

    /// <summary>
    ///     Service responsible for determining the prize for a spin based on race data received from server.
    /// </summary>
    public interface IPrizeDeterminationService
    {
        /// <summary>
        ///     Use race cards and horse information to determine or verify the prize won for a game.
        /// </summary>
        /// <param name="gameId">Id of the game we are playing.</param>
        /// <param name="numberOfCredits">Number of credits being played</param>
        /// <param name="denomination">Denomination being played</param>
        /// <param name="transactionId">EGM transaction ID which we can use</param>
        /// <param name="isRecovering">Whether prize is being determined from a outcome request via recovery.</param>
        /// <param name="token">Cancellation token to make this operation cancellable.</param>
        /// <returns></returns>
        Task<PrizeInformation> DeterminePrize(uint gameId, uint numberOfCredits, uint denomination, long transactionId, bool isRecovering, CancellationToken token = default);

        /// <summary>
        ///     Requests and returns the RaceInfo received from the server
        /// </summary>
        /// <param name="gameId">The game Id</param>
        /// <param name="numberOfCredits">The number of credits</param>
        /// <returns></returns>
        Task<CRaceInfo?> RequestRaceInfo(uint gameId, uint numberOfCredits);

        /// <summary>
        ///     Sets the horse picks
        /// </summary>
        /// <param name="picks">The horse picks, quick or manual</param>
        void SetHandicapPicks(IReadOnlyCollection<string> picks);

        /// <summary>
        ///     Clears all flags indicating we are doing a manual handicap, so that when we start
        ///     a game we will "Auto-Pick" a brand new game rather than "Quick-Pick" the current one.
        /// </summary>
        void ClearManualHandicapData();
    }
}