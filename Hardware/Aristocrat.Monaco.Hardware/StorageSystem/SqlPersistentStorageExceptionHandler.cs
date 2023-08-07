namespace Aristocrat.Monaco.Hardware.StorageSystem
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.Data.Sqlite;
    using StorageAdapters;

    /// <summary>
    ///     Sql persistent storage accessor
    /// </summary>
    public static class SqlPersistentStorageExceptionHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static bool _faulted;

        /// <summary>
        ///     Clears the current faulted state.
        /// </summary>
        public static void ClearFaultedState()
        {
            _faulted = false;
        }

        /// <summary>
        ///     Handles an exception
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="eventType">The type of error event, optional</param>
        /// <returns>True if exception was handled</returns>
        public static bool Handle(Exception exception, StorageError eventType)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Logger.Error($"Persistent Storage failure: {exception} {exception.InnerException} {exception.StackTrace}");

            if (_faulted)
            {
                return false;
            }

            var bus = ServiceManager.GetInstance().TryGetService<IEventBus>();

            switch (exception)
            {
                case KeyNotFoundException _:
                    bus?.Publish(new StorageErrorEvent(StorageError.InvalidHandle));
                    break;
                case ArgumentException _:
                case SqliteException _:
                case InvalidOperationException _:
                    bus?.Publish(new StorageErrorEvent(eventType));
                    break;
                default:
                    if (!(ServiceManager.GetInstance().TryGetService<IPersistentStorageManager>() is SqlPersistentStorageManager manager) || manager.ClearStarted)
                    {
                        bus?.Publish(new StorageErrorEvent(eventType));
                    }
                    else
                    {
                        throw exception;
                    }

                    break;
            }

            _faulted = true;

            return true;
        }
    }
}