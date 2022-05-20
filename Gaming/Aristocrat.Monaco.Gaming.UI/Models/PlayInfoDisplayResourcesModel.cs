namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.PlayerInfoDisplay;

    /// <inheritdoc cref="IPlayInfoDisplayResourcesModel" />
    public class PlayInfoDisplayResourcesModel : IPlayInfoDisplayResourcesModel
    {
        public PlayInfoDisplayResourcesModel()
        {
            ScreenBackgrounds = Array.Empty<(HashSet<string> Tags, string FilePath)>();
            Buttons = Array.Empty<(HashSet<string> Tags, string FilePath)>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> ScreenBackgrounds { get; set; }

        /// <inheritdoc />
        public IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> Buttons { get; set; }

        /// <inheritdoc />
        public string GetScreenBackground(ISet<string> tags)
        {
            return GetByTag(ScreenBackgrounds, tags);
        }

        /// <inheritdoc />
        public string GetButton(ISet<string> tags)
        {
            return GetByTag(Buttons, tags);
        }


        private string GetByTag(IEnumerable<(HashSet<string> Tags, string FilePath)> collection, ISet<string> tags)
        {
            return collection.FirstOrDefault(x => tags.IsSubsetOf(x.Tags)).FilePath;
        }
    }
}