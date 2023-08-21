namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Defines a screen available to display a media player
    /// </summary>
    public interface IMediaScreen
    {
        /// <summary>
        ///     Gets the screen type
        /// </summary>
        ScreenType Type { get; }

        /// <summary>
        ///     Gets the description
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets the screen height
        /// </summary>
        int Height { get; }

        /// <summary>
        ///     Gets the screen width
        /// </summary>
        int Width { get; }
    }
}