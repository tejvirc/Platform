namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using Contracts;

    /// <inheritdoc />
    public class LocaleGameGraphics : ILocaleGameGraphics
    {
        /// <inheritdoc />
        public string LocaleCode { get; set; }

        /// <inheritdoc />
        public string LargeIcon { get; set; }

        /// <inheritdoc />
        public string SmallIcon { get; set; }

        /// <inheritdoc />
        public string LargeTopPickIcon { get; set; }

        /// <inheritdoc />
        public string SmallTopPickIcon { get; set; }

        /// <inheritdoc />
        public string DenomButtonIcon { get; set; }

        /// <inheritdoc />
        public string DenomPanel { get; set; }

        /// <inheritdoc />
        public string TopAttractVideo { get; set; }

        /// <inheritdoc />
        public string BottomAttractVideo { get; set; }

        /// <inheritdoc />
        public string LoadingScreen { get; set; }

        /// <inheritdoc />
        public string TopperAttractVideo { get; set; }

        /// <inheritdoc />
        public IReadOnlyCollection<(string Color, string BackgroundFilePath)> BackgroundPreviewImages { get; set; }

        /// <inheritdoc />
        public IReadOnlyCollection<(HashSet<string> Tags, string FilePath)> PlayerInfoDisplayResources { get; set; }
    }
}
