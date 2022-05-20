namespace Aristocrat.Monaco.Gaming.UI.Models
{
    public class GameComboInfo
    {
        /// <summary>
        ///     Gets the game identifier of the game combo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets the unique identifier of the game combo.
        /// </summary>
        public long UniqueId { get; set; }

        /// <summary>
        ///     Gets the name of the game.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the theme of the game.
        /// </summary>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Gets the algorithm used to determine the payouts from the game.
        /// </summary>
        public string PaytableId { get; set; }

        /// <summary>
        ///     Gets the value of each credit wagered as part of the game
        /// </summary>
        public decimal Denomination { get; set; }
    }
}