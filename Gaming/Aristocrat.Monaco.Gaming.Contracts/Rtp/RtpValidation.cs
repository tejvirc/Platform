namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Collections.Generic;

    /// <summary>
    ///     This class ties a game to its respective validation results
    /// </summary>
    public class RtpValidation
    {
        /// <summary>
        ///     Gets or sets the game.
        /// </summary>
        public IGameProfile Game { get; set; }

        /// <summary>
        ///     Returns true if all validations are successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Gets or sets the validation results.
        /// </summary>
        public IList<(string wagerCategoryId, RtpValidationResult result)> ValidationResults { get; set; }
    }
}