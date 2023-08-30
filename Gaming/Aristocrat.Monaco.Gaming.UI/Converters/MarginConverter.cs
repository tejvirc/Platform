namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Aristocrat.Monaco.Gaming.Contracts;

    public class MarginConverter : IValueConverter
    {
        private const double BankImageTopMarginEn = 10;
        private const double BankImageTopMarginFr = 9;

        private const double GrandBannerOffset = 100;
        private const double MajorBannerOffset = 55;
        private const double TopRowAdjust = 60;
        private const double TopRowToGameIconPanelAdjust = 115;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness(0);
            }

            if (parameter == null || !Enum.TryParse(parameter.ToString(), out LobbyViewMarginType type))
            {
                // UPI Bank Image Margin (for MultiLanguageUPI.xaml)
                var activeLocaleCode = value.ToString();
                return new Thickness(
                    0,
                    activeLocaleCode.ToUpper() == GamingConstants.FrenchCultureCode ? BankImageTopMarginFr : BankImageTopMarginEn,
                    0,
                    0);
            }

            var gameCount = 0;
            var tabView = false;
            var bottomLabelVisible = false;
            var sharedMajorVisible = true;
            double topMarginAdjust = 0;
            var extraLargeIcons = false;

            if (value is GameGridMarginInputs inputs)
            {
                gameCount = inputs.GameCount;
                tabView = inputs.TabView;
                bottomLabelVisible = inputs.BottomLabelVisible;
                sharedMajorVisible = inputs.MultipleGameAssociatedSapLevelTwoEnabled;
                extraLargeIcons = inputs.ExtraLargeIconLayout;

                if (gameCount > 8 && inputs.SubTabVisible)
                {
                    // If there is sub tabs, we need to give more space on the top
                    topMarginAdjust += TopRowAdjust;
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
                    if (!tabView)
                    {
                        return useSmallIcons
                            ? new Thickness(0.0, 50.0, 0.0, 0)
                            : new Thickness(0.0, 80.0, 0.0, 0);
                    }

                    if (extraLargeIcons)
                    {
                        // The game icons are centered horizontally, so no need to mess with the left/right margins. The top margin needs to be
                        // calculated from the point of view of the start of the 2nd row, which is the game icon panel row.
                        var topMargin = TopRowToGameIconPanelAdjust + GrandBannerOffset + MajorBannerOffset;
                        if (!sharedMajorVisible)
                        {
                            // Move the margin up a bit when the individual major jackpot is shown
                            topMargin -= 15;
                        }

                        return new Thickness(0, topMargin, 0, 0);
                    }

                    var offset = bottomLabelVisible ? 10 : 0;
                    var margin = gameCount <= 4
                        ? new Thickness(0, 325 - offset + topMarginAdjust, 0, 0)
                        : gameCount <= 8
                            ? new Thickness(0, 240 - offset + topMarginAdjust, 0, 0)
                            : new Thickness(0, 180 - offset + topMarginAdjust, 0, 0);
                    return margin;

                case LobbyViewMarginType.GameIcon:
                    // Add space at the top of the image if there is an individual major banner
                    if (extraLargeIcons && !sharedMajorVisible)
                    {
                        topMarginAdjust += MajorBannerOffset;
                    }

                    return new Thickness(0, topMarginAdjust, 0, 0);

                case LobbyViewMarginType.NewStar:
                    return useSmallIcons
                        ? new Thickness(-20.0, -20.0, 0.0, 0)
                        : new Thickness(-10.0, -10.0, 0.0, 0);

                case LobbyViewMarginType.Bonus:
                    return useSmallIcons
                        ? new Thickness(15, 228.0, 0.0, 0)
                        : new Thickness(15, 277.0, 0.0, 0);

                case LobbyViewMarginType.ProgressiveOverlayText:
                    return new Thickness(0, 0, 0, value is bool selected ? (selected ? -5 : 0) : 0);

                default:
                    return new Thickness(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum LobbyViewMarginType
    {
        GameGrid,
        GameIcon,
        NewStar,
        Bonus,
        ProgressiveOverlayText
    }

    public class GameGridMarginInputs
    {
        public GameGridMarginInputs(
            int gameCount,
            bool tabView,
            bool subTabVisible,
            bool bottomLabelVisible,
            double gameWindowHeight,
            bool extraLargeIconLayout,
            Size gameIconSize,
            bool multipleGameAssociatedSapLevelTwoEnabled)
        {
            GameCount = gameCount;
            TabView = tabView;
            SubTabVisible = subTabVisible;
            BottomLabelVisible = bottomLabelVisible;
            GameWindowHeight = gameWindowHeight;
            ExtraLargeIconLayout = extraLargeIconLayout;
            GameIconSize = gameIconSize;
            MultipleGameAssociatedSapLevelTwoEnabled = multipleGameAssociatedSapLevelTwoEnabled;
        }

        public int GameCount { get; }

        public bool TabView { get; }

        public bool SubTabVisible { get; }

        public bool BottomLabelVisible { get; }

        public double GameWindowHeight { get; }

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
    }
}
