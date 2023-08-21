namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Provides a mechanism to control the transactions and commits for two or more blocks
    /// </summary>
    public interface IScopedTransaction : IDisposable
    {
        /// <summary>
        ///     Commits the associated transactions
        /// </summary>
        void Complete();
    }
}