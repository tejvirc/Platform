namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using System.Windows.Controls;

    /// <summary>
    ///     Static extensions for media player enumerations
    /// </summary>
    public static class MediaPlayerExtensions
    {
        /// <summary>
        ///     <see cref="DisplayPosition">DisplayPosition</see> is Left or Right
        /// </summary>
        public static bool IsMenu(this DisplayPosition position)
        {
            return position == DisplayPosition.Left || position == DisplayPosition.Right;
        }

        /// <summary>
        ///     <see cref="DisplayPosition">DisplayPosition</see> is Top or Bottom
        /// </summary>
        public static bool IsBanner(this DisplayPosition position)
        {
            return position == DisplayPosition.Top || position == DisplayPosition.Bottom;
        }

        /// <summary>
        ///     Media Player has a <see cref="ScreenType">ScreenType</see> of Primary and a <see cref="DisplayType">DisplayType</see> of Overlay
        /// </summary>
        public static bool IsPrimaryOverlay(this IMediaPlayer player)
        {
            return player.ScreenType == ScreenType.Primary && player.DisplayType == DisplayType.Overlay;
        }

        /// <summary>
        ///     Converts <see cref="DisplayPosition">DisplayPosition</see> to <see cref="Dock">Dock</see> value
        /// </summary>
        public static Dock ToDock(this DisplayPosition position)
        {
            switch (position)
            {
                case DisplayPosition.Left:
                    return Dock.Left;
                case DisplayPosition.Right:
                    return Dock.Right;
                case DisplayPosition.Top:
                    return Dock.Top;
                case DisplayPosition.Bottom:
                    return Dock.Bottom;
                default:
                    return Dock.Right;
            }
        }
    }
}
