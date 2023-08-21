namespace Aristocrat.Monaco.Application.Media
{
    using Contracts.Media;

    /// <summary>
    ///     Implementation of <see cref="IMediaScreen"/>
    /// </summary>
    internal class MediaScreen : IMediaScreen
    {
        /// <inheritdoc />
        public ScreenType Type { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public int Height { get; set; }

        /// <inheritdoc />
        public int Width { get; set; }
    }
}
