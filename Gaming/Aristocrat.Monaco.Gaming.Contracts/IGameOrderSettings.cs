namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Maintains the attract and Icon order for games.
    /// </summary>
    public interface IGameOrderSettings : IService
    {
        /// <summary>
        ///     Sets the game attract order.
        /// </summary>
        /// <param name="games">Enabled games</param>
        /// <param name="gameOrderConfig">Config of ordered list found in lobby.config.xml</param>
        void SetAttractOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig);

        /// <summary>
        ///     Sets the icon order.
        /// </summary>
        /// <param name="games">Enabled games</param>
        /// <param name="gameOrderConfig">Config of ordered list found in lobby.config.xml</param>
        void SetIconOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig);

        /// <summary>
        ///     Sets the icon game order based on the saved order configuration.  y = Order[k] is the theme Id of the game at order
        ///     position k.
        /// </summary>
        /// <param name="gameOrder">The game order.</param>
        /// <param name="operatorChanged">Operator changed.</param>
        void SetIconOrder(IEnumerable<string> gameOrder, bool operatorChanged);

        /// <summary>
        ///     Gets the attract position priority (sort order) value
        /// </summary>
        /// <param name="gameId">The theme Id for the game</param>
        /// <returns></returns>
        int GetAttractPositionPriority(string gameId);

        /// <summary>
        ///     Gets the icon position priority (sort order) value
        /// </summary>
        /// <param name="gameId">The theme Id for the game</param>
        /// <returns></returns>
        int GetIconPositionPriority(string gameId);

        /// <summary>
        ///     Sets the position priority for an individual game
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="newPosition"></param>
        void UpdateIconPositionPriority(string gameId, int newPosition);

        /// <summary>
        ///     Invoked when a game is added/enabled.  This adds the new game in the
        ///     last order position of icon order.
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        void OnGameAdded(string themeId);

        /// <summary>
        ///     Removes a game from the icon game order
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        void RemoveGame(string themeId);

        /// <summary>
        ///     Checked to see if a game already exists in the icon order
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        bool Exists(string themeId);
    }
}