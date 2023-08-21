namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Represents a condition that we wish to check before allowing a game round to start.
    /// </summary>
    public interface IGameStartCondition
    {
        /// <summary>
        ///     Checks if this condition is met. If any condition returns false then we will
        ///     not allow a game round to start.
        /// </summary>
        /// <returns>True if this condition is met and we wish to allow game round start.</returns>
        bool CanGameStart();
    }
}