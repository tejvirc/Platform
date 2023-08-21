namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a service for game cashout recovery.
    /// </summary>
    public interface IGameCashOutRecovery
    {
        /// <summary>
        ///     Gets a value indicating there is a cashout recovery.
        /// </summary>
        bool HasPending { get; }

        /// <summary>
        ///     Attempts to recover a cashout.
        /// </summary>
        /// <returns>True if it does recover a cashout.</returns>
        bool Recover();
    }
}