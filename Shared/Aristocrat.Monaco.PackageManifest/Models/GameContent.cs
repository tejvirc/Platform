namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a game based product.
    /// </summary>
    public class GameContent : Product
    {
        /// <summary>
        ///     Gets or sets the package for this game
        /// </summary>
        public ManifestPackage Package { get; set; }

        /// <summary>
        ///     Gets or sets the game attributes for game content.
        /// </summary>
        /// <value>
        ///     The game attributes if the product type is a game.
        /// </value>
        public IEnumerable<GameAttributes> GameAttributes { get; set; }

        /// <summary>
        ///     Gets or sets the custom game math restrictions per jurisdictions for game content.
        /// </summary>
        public IEnumerable<JurisdictionRestriction> JurisdictionRestrictions { get; set; }
    }
}