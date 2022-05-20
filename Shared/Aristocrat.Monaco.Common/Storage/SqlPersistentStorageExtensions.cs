namespace Aristocrat.Monaco.Common.Storage
{
    using System;
    using System.Data.SQLite;
    using log4net;

    /// <summary>
    ///     Definition of the SqlPersistentStorageExtensions class
    /// </summary>
    public static class SqlPersistentStorageExtensions
    {
        /// <summary>
        ///     Configures the supplied connection for basic SQL tracing using the <see cref="SQLiteConnection" /> Update event.
        /// </summary>
        /// <param name="connection">The connection to subscribe to for its Update event</param>
        /// <param name="ownerType">The Type of the owner of this connection</param>
        public static void ConfigureTracing(this SQLiteConnection connection, Type ownerType)
        {
            var logger = LogManager.GetLogger(ownerType);

            if (logger.IsDebugEnabled)
            {
                connection.Update += (sender, e) =>
                    logger.Debug($"{e.Event.ToString()} RowID {e.RowId} in {e.Table}.");
            }
        }
    }
}