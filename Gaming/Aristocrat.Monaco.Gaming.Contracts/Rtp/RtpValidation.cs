namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Collections.Generic;

    /// <summary>
    ///     ties a game to its respective validation results
    /// </summary>
    /// TODO Edit XML Comment Template for 
    public class RtpValidation
    {
        /// <summary>
        /// Gets or sets the game.
        /// </summary>
        /// TODO Edit XML Comment Template for 
        public IGameProfile Game { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// TODO Edit XML Comment Template for 
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation results.
        /// </summary>
        /// TODO Edit XML Comment Template for 
        public IList<(string wagerCategoryId, RtpValidationResult result)> ValidationResults { get; set; }
    }
}