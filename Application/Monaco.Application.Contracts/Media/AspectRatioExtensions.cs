namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using System;
    //using System.Windows;

    /// <summary>
    ///     A set of <see cref="AspectRatio" /> extensions
    /// </summary>
    public static class AspectRatioExtensions
    {
        /// <summary>
        ///     Gets the calculated width for a given height
        /// </summary>
        /// <param name="this">The aspect ration</param>
        /// <param name="height">The height</param>
        /// <returns>The calculated width</returns>
        public static int GetCalculatedWidth(this AspectRatio @this, int height)
        {
            return (int)Math.Round(@this.Width * height / (double)@this.Height);
        }

        /// <summary>
        ///     Gets the calculated width for a given width
        /// </summary>
        /// <param name="this">The aspect ration</param>
        /// <param name="width">The width</param>
        /// <returns>The calculated height</returns>
        public static int GetCalculatedHeight(this AspectRatio @this, int width)
        {
            return (int)Math.Round(@this.Height * width / (double)@this.Width);
        }

        ///// <summary>
        /////     Gets whether the given window has a portrait aspect ratio
        ///// </summary>
        ///// <param name="window">The window to test</param>
        ///// <returns>True if the window has a portrait aspect ratio</returns>
        //public static bool IsPortrait(this Window window)
        //{
        //    if (window == null)
        //    {
        //        return false;
        //    }

        //    var aspectRatio = window.ActualWidth / window.ActualHeight;
        //    var isPortrait = aspectRatio < 1.0;
        //    return isPortrait;
        //}
    }
}