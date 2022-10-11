namespace Aristocrat.Monaco.Bingo.Common.Extensions
{
    using Gaming.Contracts;

    public static class GameDetailExtensions
    {
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
    }
}