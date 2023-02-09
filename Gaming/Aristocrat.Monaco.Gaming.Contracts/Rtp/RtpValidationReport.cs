namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// talk about how it's a 1-to-1 relationship with games (IGameProfile)
    /// </summary>
    /// TODO Edit XML Comment Template for
    public class RtpValidationReport
    {
        private readonly IReadOnlyCollection<(IGameProfile game, RtpValidation validation)> _validations;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpValidationReport" /> class.
        /// </summary>
        /// <param name="validations">The validations.</param>
        /// TODO Edit XML Comment Template for #ctor
        public RtpValidationReport(IList<(IGameProfile game, RtpValidation validation)> validations)
        {
            if (!validations.Any())
            {
                throw new ArgumentException(
                    "Cannot construct an RtpValidationReport object with an empty collection.",
                    nameof(validations));
            }

            _validations = new Collection<(IGameProfile, RtpValidation)>(validations.ToArray());
        }

        /// <summary>
        ///     Gets the passed games.
        /// </summary>
        /// TODO Edit XML Comment Template for PassedGames
        public IReadOnlyList<(IGameProfile, RtpValidation)> PassedGames =>
            _validations.Where(v => v.validation.IsValid).ToArray();

        /// <summary>
        ///     Gets the failed games.
        /// </summary>
        /// TODO Edit XML Comment Template for FailedGames
        public IReadOnlyList<(IGameProfile, RtpValidation)> FailedGames =>
            _validations.Where(v => !v.validation.IsValid).ToArray();

        /// <summary>
        ///     <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid => !FailedGames.Any();

        /// <summary>
        ///     Gets the validation object for game. This contains information about the validation.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns>The validation object for the given game.</returns>
        public RtpValidation GetValidationInfo(IGameProfile game)
        {
            var entry = _validations.FirstOrDefault(result => result.game.Id == game.Id);
            if (entry == default((IGameProfile, RtpValidation)))
            {
                throw new Exception(
                    $"No validation exists in {nameof(RtpValidationReport)} for game={game.ThemeId}-{game.VariationId}");
            }

            return entry.validation;
        }
    }
}