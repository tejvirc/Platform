namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a configuration restriction
    /// </summary>
    public class Configuration
    {
        /// <summary>
        ///     Gets or sets the configuration name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the game configuration
        /// </summary>
        public GameConfiguration GameConfiguration { get; set; }
    }
}