namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Client.Data;
    using Client.Messages;

    /// <summary>
    ///     Holds game data for all games and fetches race info on request.
    /// </summary>
    public interface IGameDataService
    {
        /// <summary>
        ///     Get parameters from CentralServer, returning cached information if we have already fetched it.
        /// </summary>
        /// <param name="reinitializeData">If true, force download of new data.</param>
        /// <returns>Null if unable to get the info. Caller should retry if unable to fetch.</returns>
        Task<ParameterResponse> GetGameParameters(bool reinitializeData = false);

        /// <summary>
        ///     Return data for each game ID received, returning cached information if we have already fetched it.
        /// </summary>
        /// <param name="reinitializeData">If true, force download of new data.</param>
        /// <returns>Empty list if unable to get the info. Caller should retry if unable to fetch.</returns>
        Task<IEnumerable<GameInfoResponse>> GetGameInfo(bool reinitializeData = false);

        /// <summary>
        ///     Get data for each progressive ID received, returning cached information if we have already fetched it.
        /// </summary>
        /// <param name="reinitializeData">If true, force download of new data.</param>
        /// <returns>Empty list if unable to get the info. Caller should retry if unable to fetch.</returns>
        Task<IEnumerable<ProgressiveInfoResponse>> GetProgressiveInfo(bool reinitializeData = false);

        /// <summary>
        ///     Fetch the race pattern information given the current game ID, bet amount, and number of lines.
        /// </summary>
        /// <returns>Null if unable to fetch the data. Caller should retry if unable to fetch.</returns>
        Task<RacePariResponse> GetRaceInformation(uint gameId, uint creditsPlayed, uint linesPlayed);

        /// <summary>
        ///     Returns GameOpenResponse for a given gameId.
        /// </summary>
        /// <param name="gameId">GameId for which GameInfo is required.</param>
        /// <returns></returns>
        Task<GameInfoResponse> GetGameOpen(uint gameId);

        /// <summary>
        ///     Returns RacePatterns as per gameId, numberOfCredits and linesPlayed.
        /// </summary>
        /// <param name="gameId">Game id selected to play game.</param>
        /// <param name="numberOfCredits">Number of credits being played.</param>
        /// <param name="totalLinesPlayed">Number of lines being played.</param>
        /// <returns>CRacePatterns for selected game, bet and lines.</returns>
        Task<CRacePatterns> GetRacePatterns(uint gameId, uint numberOfCredits, uint totalLinesPlayed);

        /// <summary>
        /// Return the prize location for a given ticket set and pattern index
        /// </summary>
        /// <param name="gameId">GameInfo Id received from server</param>
        /// <param name="credits">Credits for which pattern needs to fetched</param>
        /// <param name="patternIndex">Pattern index for the TicketSet</param>
        /// <returns>Correct index if found, else -1</returns>
        Task<int> GetPrizeLocationForAPattern(uint gameId, uint credits, int patternIndex);
    }
}