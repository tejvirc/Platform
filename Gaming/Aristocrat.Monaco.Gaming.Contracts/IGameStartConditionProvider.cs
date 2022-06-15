namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     An interface that we can use to register and find <see cref="IGameStartCondition"/>
    ///     instances. When we want to start a game round all conditions must return true or we
    ///     will refuse to start the game round.
    /// </summary>
    public interface IGameStartConditionProvider : IService
    {
        /// <summary>
        ///     Add a new IGameStartCondition that will be checked when we want to start a game
        ///     round. All conditions must return true in order to start a game round.
        /// </summary>
        /// <param name="condition">A game start condition that we want to register.</param>
        void AddGameStartCondition(IGameStartCondition condition);

        /// <summary>
        ///     Remove an IGameStartCondition that we had previously added to the provider.
        /// </summary>
        /// <param name="condition">A game start condition that we want to register.</param>
        void RemoveGameStartCondition(IGameStartCondition condition);

        /// <summary>
        ///     Check all of the IGameStartCondition objects that are registered and if any
        ///     fail then return false, indicating we do not wish to start a game round.
        /// </summary>
        bool CheckGameStartConditions();
    }
}