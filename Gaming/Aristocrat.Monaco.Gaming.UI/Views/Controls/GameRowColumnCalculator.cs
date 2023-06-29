namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    using Contracts;
    using Kernel;

    public static class GameRowColumnCalculator
    {
        private const int LargeTileHighGameCount = 7;
        private const int LargeTileLowGameCount = 3;
        private const int SmallTileHighGameCount = 21;
        private const int SmallTileModerateGameCount = 8;
        private const int SmallTileLowGameCount = 4;
        private const int StandardColumnCount = 3;
        private const int TwoRows = 2;
        private const int OneRow = 1;

        public static (int Rows, int Cols) ExtraLargeIconRowColCount { get; } = (1, 2);

        public static (int Rows, int Cols) CalculateRowColCount(int childCount, bool isExtraLargeGameIconTabActive)
        {
            var properties = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            if (properties != null)
            {
                var lobbyConfig = properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
                if (lobbyConfig.MidKnightLobbyEnabled && childCount < LargeTileHighGameCount)
                {
                    // If there are more than 6 games, use the standard calculator
                    return childCount > LargeTileLowGameCount ? (TwoRows, StandardColumnCount) : (OneRow, StandardColumnCount);
                }
            }

            if (isExtraLargeGameIconTabActive)
            {
                return ExtraLargeIconRowColCount;
            }

            var rows = childCount > SmallTileHighGameCount ? 4 : childCount > SmallTileModerateGameCount ? 3 : childCount > SmallTileLowGameCount ? 2 : 1;
            var cols = childCount > 0 ? childCount > SmallTileModerateGameCount ? (childCount + (rows - 1)) / rows : SmallTileLowGameCount : 0;

            return (rows, cols);
        }
    }
}