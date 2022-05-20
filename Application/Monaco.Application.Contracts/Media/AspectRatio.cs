namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     Defines an aspect ratio Width:Height
    /// </summary>
    public struct AspectRatio
    {
        /// <summary>
        ///     Gets the Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     Gets the height
        /// </summary>
        public int Height { get; }

        /// <summary>
        ///     Creates a new instance of a <see cref="AspectRatio" />
        /// </summary>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        public AspectRatio(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Width}:{Height}";
        }
    }
}