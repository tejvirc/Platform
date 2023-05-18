namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    using Contracts;
    using Kernel;

    public static class GameRowColumnCalculator
    {
        public static (int Rows, int Cols) ExtraLargeIconRowColCount { get; } = (1, 2);

        public static (int Rows, int Cols) CalculateRowColCount(int childCount, bool isExtraLargeGameIconTabActive)
        {
            var properties = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            if (properties != null)
            {
                var lobbyConfig = properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
                if (lobbyConfig.MidKnightLobbyEnabled && childCount < 7)
                {
                    // If there are more than 6 games, use the standard calculator
                    return childCount > 3 ? (2, 3) : (1, 3);
                }
            }
          
            if (isExtraLargeGameIconTabActive)
            {
                return ExtraLargeIconRowColCount;
            }

            var rows = childCount > 21 ? 4 : childCount > 8 ? 3 : childCount > 4 ? 2 : 1;
            var cols = childCount > 0 ? childCount > 8 ? (childCount + (rows - 1)) / rows : 4 : 0;

            return (rows, cols);
        }
    }
}