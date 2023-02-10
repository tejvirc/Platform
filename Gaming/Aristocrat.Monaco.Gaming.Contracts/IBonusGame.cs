namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Defines a Bonus Game
    /// </summary>
    public interface IBonusGame
    {
        /// <summary>
        ///     Gets the identifier of the bonus game
        /// </summary>
        /// <value>
        ///     The identifier of the bonus game
        /// </value>
        long Id { get; }

        /// <summary>
        ///     Gets the name of the bonus game
        /// </summary>
        string Name { get; }
    }
}