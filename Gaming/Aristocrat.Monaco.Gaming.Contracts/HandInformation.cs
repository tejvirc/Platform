namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Holds information about cards in a hand
    /// </summary>
    public class HandInformation
    {
        /// <summary>
        ///     Gets or sets card values
        /// </summary>
        public GameCard[] Cards { get; set; } = { GameCard.Unknown, GameCard.Unknown, GameCard.Unknown, GameCard.Unknown, GameCard.Unknown };

        /// <summary>
        ///     Gets or sets card held state
        /// </summary>
        public HoldStatus[] IsHeld { get; set; } = { HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld };

        /// <summary>
        ///     Gets or sets a value indicating whether this is the final hand in the game
        /// </summary>
        public bool FinalHand { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"\nCards: {Cards[0]}, {Cards[1]}, {Cards[2]}, {Cards[3]}, {Cards[4]}\n"
                   + $"Held: {IsHeld[0]}, {IsHeld[1]}, {IsHeld[2]}, {IsHeld[3]}, {IsHeld[4]}\nFinal Hand: {FinalHand}";
        }
    }
}