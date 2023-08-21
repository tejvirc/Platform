namespace Aristocrat.Monaco.Gaming.UI.Models
{
    /// <summary>
    ///     Details for an attract presentation
    /// </summary>
    public interface IAttractDetails
    {
        /// <summary>
        ///     Gets The path for the topper attract video
        /// </summary>
        string TopperAttractVideoPath { get; }

        /// <summary>
        ///     Gets the path for the top attract video
        /// </summary>
        string TopAttractVideoPath { get; }

        /// <summary>
        ///     Gets the path to the bottom attract video
        /// </summary>
        string BottomAttractVideoPath { get; }

        /// <summary>
        ///     Gets the locale specific attract path for the top
        /// </summary>
        /// <param name="localeCode">The locale code to get the video for</param>
        /// <returns>The video path for the provided locale code</returns>
        string GetTopAttractVideoPathByLocaleCode(string localeCode);

        /// <summary>
        ///     Gets the locale specific attract path for the topper
        /// </summary>
        /// <param name="localeCode">The locale code to get the video for</param>
        /// <returns>The video path for the provided locale code</returns>
        string GetTopperAttractVideoPathByLocaleCode(string localeCode);

        /// <summary>
        ///     Gets the locale specific attract path for the bottom
        /// </summary>
        /// <param name="localeCode">The locale code to get the video for</param>
        /// <returns>The video path for the provided locale code</returns>
        string GetBottomAttractVideoPathByLocaleCode(string localeCode);
    }
}