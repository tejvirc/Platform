namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     Model for defining the custom game math per jurisdiction.
    ///     Note that these restrictions are a whitelist and only those defined here will be allowed.
    /// </summary>
    public class JurisdictionRestriction
    {
        /// <summary>
        ///     The allowed variation Ids
        /// </summary>
        public IEnumerable<string> AllowedVariationIds { get; set; }

        /// <summary>
        ///     The allowed bet line preset Ids
        /// </summary>
        public IEnumerable<int> AllowedBetLinePresets { get; set; }

        /// <summary>
        ///     The allowed denominations
        /// </summary>
        public IEnumerable<long> AllowedDenoms { get; set; }

        /// <summary>
        ///     The jurisdiction id of the jurisdiction these restrictions should be applied to
        /// </summary>
        public string JurisdictionId { get; set; }
    }
}