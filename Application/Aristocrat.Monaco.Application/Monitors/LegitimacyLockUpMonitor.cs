namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the LegitimacyLockUpMonitor class
    /// </summary>
    public class LegitimacyLockUpMonitor : IService, IDisposable
    {
        /// <summary>Name of persistent storage block containing LegitimacyLockUpMonitor.</summary>
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.LegitimacyLockUpMonitor";
        private const string LegitimacylockUpText = "Non Retail Bank Fault: collect data and log files";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _disposed;


        /// <summary>
        ///     Initializes a new instance of the <see cref="LegitimacyLockUpMonitor" /> class.
        /// </summary>
        public LegitimacyLockUpMonitor()
        {
            Logger.Info("Creating LegitimacyLockUpMonitor");
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            if (!persistentStorage.BlockExists(BlockName))
            {
                var block = persistentStorage.CreateBlock(PersistenceLevel.Transient, BlockName, 1);
                using (var transaction = block.StartTransaction())
                {
                    transaction["LockUp"] = false;
                    transaction.Commit();
                }
            }
            else
            {
                var block = persistentStorage.GetBlock(BlockName);

                if ((bool)block["LockUp"])
                {
                    ServiceManager.GetInstance().TryGetService<ISystemDisableManager>()?.Disable(Guid.NewGuid(), SystemDisablePriority.Immediate, () => LegitimacylockUpText);
                }
            }

            Logger.Info("Creating LegitimacyLockUpMonitor completed!");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => typeof(LegitimacyLockUpMonitor).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(LegitimacyLockUpMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Subscribe();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            _disposed = true;
        }

        private void Subscribe()
        {
#if !(RETAIL)
            var bus = ServiceManager.GetInstance().GetService<IEventBus>();
            bus.Subscribe<LegitimacyLockUpEvent>(this, HandleEvent);
#endif
        }

        private void HandleEvent(LegitimacyLockUpEvent eventData)
        {
            var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var block = persistentStorage.GetBlock(BlockName);

            using (var transaction = block.StartTransaction())
            {
                transaction["LockUp"] = true;
                transaction.Commit();
            }

            ServiceManager.GetInstance().TryGetService<ISystemDisableManager>()?.Disable(Guid.NewGuid(), SystemDisablePriority.Immediate, () => LegitimacylockUpText);
        }
    }
}
