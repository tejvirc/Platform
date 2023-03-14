namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Contracts;

    public class MarginConverter : IValueConverter
    {
        private const double BankImageTopMarginEn = 10;
        private const double BankImageTopMarginFr = 9;
        private const double DenomMarginAdjust = 150;
        private const double TopMarginAdjust = 150;
        private const double MajorBannerOffset = 55;
        private const double TopRowAdjust = 45;
        private const double TopRowToGameIconPanelAdjust = 115;
        private const double BottomOfGameIconAreaAdjust = 180;
        private readonly double _scaleBy = ScaleUtility.GetScale();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness(0, 0, 0, 0);
            }

            if (parameter != null && Enum.TryParse(parameter.ToString(), out LobbyViewMarginType type))
            {
                var gameCount = 0;
                var tabView = false;
                var bottomLabelVisible = false;
                double topMarginAdjust = 0;
                double denomMarginAdjust = 0;
                double screenHeight = 0;
                var extraLargeIcons = false;
                var gameIconSize = Size.Empty;

                if (value is GameGridMarginInputs inputs)
                {
                    screenHeight = inputs.ScreenHeight;
                    gameCount = inputs.GameCount;
                    tabView = inputs.TabView;
                    bottomLabelVisible = inputs.BottomLabelVisible;
                    extraLargeIcons = inputs.ExtraLargeIconLayout;
                    gameIconSize = inputs.GameIconSize;
                    if (inputs.ScreenHeight > ScaleUtility.BaseScreenHeight)
                    {
                        topMarginAdjust = TopMarginAdjust;
                        denomMarginAdjust = DenomMarginAdjust;
                    }

                    if (gameCount > 8 && inputs.SubTabVisible)
                    {
                        // If there is sub tabs, we need to give more space on the top
                        topMarginAdjust += 60;
                    }
                }
                else if (value is int count)
                {
                    gameCount = count;
                }

                // Lobby layout margins based on number of games
                var useSmallIcons = gameCount > 8;
                switch (type)
                {
                    case LobbyViewMarginType.GameGrid:
                        if (tabView)
                        {
                            if (extraLargeIcons)
                            {
                                // The game icons are centered horizontally, so no need to mess with the left/right margins. The top margin needs to be
                                // calculated from the point of view of the start of the 2nd row, which is the game icon panel row.
                                var adjustedGameIconPanelHeight = ScaleUtility.BaseScreenHeight - TopRowAdjust - TopRowToGameIconPanelAdjust - BottomOfGameIconAreaAdjust;
                                var topMargin1 = (adjustedGameIconPanelHeight - gameIconSize.Height) / 2;
                                var topMargin2 = TopRowToGameIconPanelAdjust + topMargin1 + MajorBannerOffset;

                                return new Thickness(0, topMargin2 * _scaleBy, 0, 0);
                            }

                            var offset = bottomLabelVisible ? 10 : 0;
                            var topOffset = screenHeight > ScaleUtility.BaseScreenHeight ? (gameCount > 4 ? 0 : -80) : 60;
                            var margin = gameCount <= 4
                                ? new Thickness(0, 325 - offset + topMarginAdjust - topOffset, 0, 0)
                                : gameCount <= 8
                                    ? new Thickness(0, 240 - offset + topMarginAdjust, 0, 0)
                                : new Thickness(0, 180 - offset + topMarginAdjust - topOffset, 0, 0);


                            return margin;
                        }

                        return useSmallIcons
                            ? new Thickness(0.0, 50.0, 0.0, 0)
                            : new Thickness(0.0, 80.0, 0.0, 0);
                    case LobbyViewMarginType.NewStar:
                        return useSmallIcons
                            ? new Thickness(-20.0, -20.0, 0.0, 0)
                            : new Thickness(-10.0, -10.0, 0.0, 0);
                    case LobbyViewMarginType.Bonus:
                        return useSmallIcons
                            ? new Thickness(15, 228.0, 0.0, 0)
                            : new Thickness(15, 277.0, 0.0, 0);
                    case LobbyViewMarginType.Banner:
                        return new Thickness(19.0, 0, 20.0, 8.0);
                    case LobbyViewMarginType.ProgressiveOverlay:
                        if (screenHeight > ScaleUtility.BaseScreenHeight)
                        {
                            // for marsX or any monitor with higher resolution
                            return new Thickness(0, 0, 0, gameCount <= 8 ? 128 : 168);
                        }
                        return new Thickness(0, 0, 0, 36);
                    case LobbyViewMarginType.ProgressiveOverlayText:
                        return new Thickness(0, 0, 0, value is bool selected ? (selected ? -5 : 0) : 0);
                    case LobbyViewMarginType.DenomLargeScreenLayout:
                        return new Thickness(0, 0, 0, 45 + denomMarginAdjust);
                }
            }

            // UPI Bank Image Margin
            var activeLocaleCode = value.ToString();
            return new Thickness(
                0,
                activeLocaleCode.ToUpper() == GamingConstants.FrenchCultureCode ? BankImageTopMarginFr : BankImageTopMarginEn,
                0,
                0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum LobbyViewMarginType
    {
        GameGrid,
        NewStar,
        Bonus,
        Banner,
        ProgressiveOverlay,
        ProgressiveOverlayText,
        DenomLargeScreenLayout
    }

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
}
