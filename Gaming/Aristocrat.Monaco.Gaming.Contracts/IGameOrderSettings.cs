namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using System.Collections.Generic;

    /// <summary>
    ///     Maintains the game order.
    /// </summary>
    public interface IGameOrderSettings : IService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="games"></param>
        /// <param name="gameOrderConfig"></param>
        void SetGameOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig);

        /// <summary>
        ///     Sets the game order based on the saved order configuration.  y = Order[k] is the theme Id of the game at order position k.
        /// </summary>
        /// <param name="gameOrder">The game order.</param>
        /// <param name="operatorChanged">Operator changed.</param>
        void SetGameOrder(IEnumerable<string> gameOrder, bool operatorChanged);

        /// <summary>
        ///     Gets the position priority (sort order) value
        /// </summary>
        /// <param name="gameId">The theme Id for the game</param>
        /// <returns></returns>
        int GetPositionPriority(string gameId);

        /// <summary>
        ///     Sets the position priority for an individual game
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="newPosition"></param>
        void UpdatePositionPriority(string gameId, int newPosition);

        /// <summary>
        ///     Invoked when a game is added/enabled.  This adds the new game in the
        ///     last order position.
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        void OnGameAdded(string themeId);

        /// <summary>
        ///     Removes a game from the game order
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        void RemoveGame(string themeId);

        /// <summary>
        ///     Checked to see if a game already exists in the game order
        /// </summary>
        /// <param name="themeId">The theme Id for the game.</param>
        bool Exists(string themeId);
    }
}
