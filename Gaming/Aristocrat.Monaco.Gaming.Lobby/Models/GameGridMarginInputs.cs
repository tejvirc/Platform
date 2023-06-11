namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using System.Windows;

public class GameGridMarginInputs
{
    public GameGridMarginInputs(
        int gameCount,
        bool tabView,
        bool subTabVisible,
        bool bottomLabelVisible,
        double screenHeight,
        bool extraLargeIconLayout,
        Size gameIconSize,
        bool multipleGameAssociatedSapLevelTwoEnabled,
        bool hasProgressiveLabelDisplay)
    {
        GameCount = gameCount;
        TabView = tabView;
        SubTabVisible = subTabVisible;
        BottomLabelVisible = bottomLabelVisible;
        ScreenHeight = screenHeight;
        ExtraLargeIconLayout = extraLargeIconLayout;
        GameIconSize = gameIconSize;
        MultipleGameAssociatedSapLevelTwoEnabled = multipleGameAssociatedSapLevelTwoEnabled;
        HasProgressiveLabelDisplay = hasProgressiveLabelDisplay;
    }

    public int GameCount { get; }

    public bool TabView { get; }

    public bool SubTabVisible { get; }

    public bool BottomLabelVisible { get; }

    public double ScreenHeight { get; }

    /// <summary>
    ///     Is the current tab hosting extra large icons
    /// </summary>
    public bool ExtraLargeIconLayout { get; }

    /// <summary>
    ///     The dimensions of the game icon
    /// </summary>
    public Size GameIconSize { get; }

    /// <summary>
    ///     Is the level 2 associated SAP enabled. It will affect the arrangement
    /// </summary>
    public bool MultipleGameAssociatedSapLevelTwoEnabled { get; }

    /// <summary>
    ///     Are the associated SAPs per game enabled. It will affect the arrangement
    /// </summary>
    public bool HasProgressiveLabelDisplay { get; }
}
