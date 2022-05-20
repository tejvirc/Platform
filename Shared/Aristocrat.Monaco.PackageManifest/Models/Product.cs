namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     A product
    /// </summary>
    public class Product
    {
        /// <summary>
        ///     Gets or sets the identifier of the product.
        /// </summary>
        /// <value>
        ///     The identifier of the product.
        /// </value>
        public string ProductId { get; set; }

        /// <summary>
        ///     Gets or sets the game DLL name of the product.
        /// </summary>
        /// <value>
        ///     The game DLL name of the product.
        /// </value>
        public string GameDll { get; set; }

        /// <summary>
        ///     Gets or sets the icon type to use
        /// </summary>
        /// <value>
        ///     The game icon type of the product
        /// </value>
        public t_iconType IconType { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the manifest.
        /// </summary>
        /// <value>
        ///     The identifier of the manifest.
        /// </value>
        public string ManifestId { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the release number.
        /// </summary>
        /// <value>
        ///     The release number.
        /// </value>
        public string ReleaseNumber { get; set; }

        /// <summary>
        ///     Gets or sets the date of the release.
        /// </summary>
        /// <value>
        ///     The date of the release.
        /// </value>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the information describing the detailed.
        /// </summary>
        /// <value>
        ///     Information describing the detailed.
        /// </value>
        public string DetailedDescription { get; set; }

        /// <summary>
        ///     Gets or sets the install sequence.
        /// </summary>
        /// <value>
        ///     The install sequence.
        /// </value>
        public string InstallSequence { get; set; }

        /// <summary>
        ///     Gets or sets the number of mechanical reels
        /// </summary>
        /// <value>
        ///     The number of mechanical reels.
        /// </value>
        public int MechanicalReels { get; set; }

        /// <summary>
        ///     Gets or sets the home stops for mechanical reels
        /// </summary>
        /// <value>
        ///     array of numbers representing the stops.
        ///     array[0] is the stop for the leftmost reel
        /// </value>
        public int[] MechanicalReelHomeStops { get; set; }

        /// <summary>
        ///     Gets or sets the uninstall sequence.
        /// </summary>
        /// <value>
        ///     The uninstall sequence.
        /// </value>
        public string UninstallSequence { get; set; }

        /// <summary>
        ///     Gets the graphics for all the pkg:localization.
        /// </summary>
        /// <value>
        ///     The key is the local code (e.g., "fr_CA" or "en_US").
        ///     The value is a list of Graphic elements.
        /// </value>
        public Dictionary<string, IEnumerable<Graphic>> Graphics { get; } =
            new Dictionary<string, IEnumerable<Graphic>>();

        /// <summary>
        ///     Gets the upgrade actions
        /// </summary>
        public IEnumerable<UpgradeAction> UpgradeActions { get; set; }

        /// <summary>
        ///     Gets the product configurations
        /// </summary>
        public IEnumerable<Configuration> Configurations { get; set; }

        /// <summary>
        ///     Gets or sets the default configuration.
        /// </summary>
        public Configuration DefaultConfiguration { get; set; }
    }
}