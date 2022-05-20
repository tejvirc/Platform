namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using Kernel;

    /// <summary>
    ///     This interface allows the operator menu pages to access game play state information
    /// </summary>
    /// <remarks>This interface should be implemented in the gaming layer</remarks>
    public interface IOperatorMenuGamePlayMonitor : IService
    {
        /// <summary>
        ///     Gets a value indicating whether or not the game is currently in the middle of a round.
        /// </summary>
        bool InGameRound { get; }

        /// <summary>
        ///     Gets a value indicating whether or not game is currently in replay.
        /// </summary>
        bool InReplay { get; }

        /// <summary>
        ///     Gets a value indicating whether or not game recovery is needed.
        /// </summary>
        bool IsRecoveryNeeded { get; }
    }
}