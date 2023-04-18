namespace Aristocrat.Monaco.Bingo.Common.Extensions
{
    using Gaming.Contracts;

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
    }
}