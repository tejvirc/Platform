namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System.Collections.Generic;

    /// <summary>
    ///     Progressive settings.
    /// </summary>
    internal class ProgressiveSettings
    {
        /// <summary>
        ///     Gets or sets the shared sap levels.
        /// </summary>
        public IReadOnlyCollection<ProgressiveSharedLevelSettings> CustomSapLevels { get; set; }

        /// <summary>
        ///     Gets or sets the linked progressive level names.
        /// </summary>
        public IReadOnlyCollection<string> LinkedProgressiveLevelNames { get; set; }

        /// <summary>
        ///     Gets or sets the assigned progressive levels.
        /// </summary>
        public IReadOnlyCollection<ProgressiveLevelSettings> AssignedProgressiveLevels { get; set; }
    }
}
