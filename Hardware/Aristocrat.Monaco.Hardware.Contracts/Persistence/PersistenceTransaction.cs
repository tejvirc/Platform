namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Provides a basic container for a transaction
    /// </summary>
    public static class PersistenceTransaction
    {
        [ThreadStatic] private static IPersistentStorageTransaction _transaction;

        [ThreadStatic] private static bool _ready;

        /// <summary>
        ///     Gets or sets the transaction
        /// </summary>
        public static IPersistentStorageTransaction Current
        {
            get => _transaction;
            set => _transaction = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the transaction is ready to be committed
        /// </summary>
        public static bool Ready
        {
            get => _ready;
            set => _ready = value;
        }
    }
}