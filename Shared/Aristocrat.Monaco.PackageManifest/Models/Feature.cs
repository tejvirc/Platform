namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines feature supported by game
    /// </summary
    public class Feature
    {
        /// <summary>Gets or sets the Name of the feature for example Lucky Chance Spin</summary>
        public string Name { get; set; }

        /// <summary>default value of enable/disable feature set by game</summary>
        public bool Enable { get; set; }

        /// <summary>true indicate that feature can enabled/disabled by operator </summary>
        public bool Editable { get; set; }

        /// <summary>Information about statistics.</summary>
        public IList<StatInfo> StatInfo { get; set; }
    }
}