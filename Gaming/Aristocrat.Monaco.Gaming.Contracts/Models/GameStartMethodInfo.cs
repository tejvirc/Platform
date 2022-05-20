namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    /// <summary>
    ///     Defines the ways to start game.
    /// </summary>
    public class GameStartMethodInfo
    {
        /// <summary>
        /// </summary>
        /// <param name="value"> The (enum) value for game start method</param>
        /// <param name="description"> The description for corresponding game start method</param>
        public GameStartMethodInfo(GameStartMethodOption value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        ///     Gets the Value for game start method
        /// </summary>
        public GameStartMethodOption Value { get; }

        /// <summary>
        ///     Gets the Description for game start method
        /// </summary>
        public string Description { get; }
    }
}
