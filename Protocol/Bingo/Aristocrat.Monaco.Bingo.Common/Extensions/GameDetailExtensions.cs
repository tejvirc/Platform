namespace Aristocrat.Monaco.Bingo.Common.Extensions
{
    using System.Linq;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

    public static class GameDetailExtensions
    {
        /// <summary>
        ///     Gets the game's title id
        /// </summary>
        /// <param name="gameDetail">The game detail</param>
        /// <returns>The game's title id</returns>
        public static string GetBingoTitleId(this IGameDetail gameDetail)
        {
            if (!string.IsNullOrEmpty(gameDetail.CdsTitleId))
            {
                return gameDetail.CdsTitleId;
            }

            if (!string.IsNullOrEmpty(gameDetail.CdsThemeId))
            {
                return gameDetail.CdsThemeId;
            }

            return gameDetail.ThemeName;
        }

        /// <summary>
        ///     Gets the game's title id as an int.
        /// </summary>
        /// <param name="gameDetail">The game details</param>
        /// <returns>The game's title id as an int, or 0 if no title id found</returns>
        public static int GetBingoTitleIdInt(this IGameDetail gameDetail)
        {
            return int.TryParse(gameDetail.GetBingoTitleId(), out var id) ? id : 0;
        }

        /// <summary>
        ///     Get the first sub game title id as an int
        /// </summary>
        /// <param name="gameDetail">The game details</param>
        /// <param name="transaction">The transaction with the game index</param>
        /// <returns>The sub game's title id as an int, or -1 if no title id found</returns>
        public static int GetSubGameTitleId(this IGameDetail gameDetail, CentralTransaction transaction)
        {
            // if there aren't any sub games then exit early
            if (transaction.AdditionalInfo.Count() <= 1)
            {
                return -1;
            }

            // The main game will be 0 and side bet games will be 1
            var gameIndex = transaction.AdditionalInfo?.First(x => x.GameIndex > 0).GameId ?? -1;
            var subGameId = gameDetail?.SupportedSubGames?.Where(id => id.Id == gameIndex).FirstOrDefault()?.CdsTitleId ?? string.Empty;
            
            return int.TryParse(subGameId, out var i) ? i : -1;
        }
    }
}