namespace Aristocrat.Monaco.Gaming.Contracts.PlayerInfoDisplay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Model of found resources for PID screens
    /// </summary>
    public interface IPlayInfoDisplayResourcesModel
    {
        /// <summary>
        ///     List of all backgrounds
        /// </summary>
        IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> ScreenBackgrounds { get; }

        /// <summary>
        ///     List of all buttons
        /// </summary>
        IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> Buttons { get; }

        /// <summary>
        /// Looks up for background
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        string GetScreenBackground(ISet<string> tags);

        /// <summary>
        /// Looks up for button
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        string GetButton(ISet<string> tags);
    }
}