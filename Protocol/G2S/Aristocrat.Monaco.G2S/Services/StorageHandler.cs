namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Handles storage events like clearing persistent storage
    /// </summary>
    public sealed class StorageHandler : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IMonacoContextFactory _contextFactory;

        public StorageHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IMonacoContextFactory>())
        {
        }

        public StorageHandler(
            IEventBus eventBus,
            IPersistentStorageManager persistentStorage,
            IMonacoContextFactory contextFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _eventBus.UnsubscribeAll(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(StorageHandler) };

        /// <inheritdoc />
        public void Initialize()
        {
            // Because of the order in which things are torn down we need to jump through some hoops to make sure our callback is invoked
            _eventBus.Subscribe<PersistentStorageClearStartedEvent>(
                this,
                _ => _persistentStorage.StorageClearingEventHandler += OnStorageClearing);
        }

        private void OnStorageClearing(object sender, StorageEventArgs e)
        {
            if (e.Level == PersistenceLevel.Transient)
            {
                return;
            }

            // Order is important - Must be reverse of the list in MonacoContext.cs
            var tables = new List<string>
            {
                "PackageLog",
                "IdReaderData",
                "VoucherData",
                "PrinterLog",
                "Transfer",
                "Script",
                "Package",
                "PackageError",
                "Module",
                "Certificate",
                "PkiConfiguration",
                "GatSpecialFunctionParameter",
                "GatSpecialFunction",
                "ComponentVerification",
                "GatVerificationRequest",
                "OptionConfigItem",
                "OptionConfigGroup",
                "OptionConfigDevice",
                "OptionChangeLog",
                "CommChangeLog",
                "CommHostConfigDevice",
                "CommHostConfigItem",
                "CommHostConfig",
                "ConfigChangeAuthorizeItem",
                "MeterSubscription",
                "SupportedEvent",
                "EventSubscription",
                "EventHandlerLog",
                "ProfileData",
                "Host"
            };

            // Exclusions - Just to be explicit
            tables.Remove("Transfer");
            tables.Remove("Package");
            tables.Remove("PackageError");
            tables.Remove("Module");

            if (e.Level != PersistenceLevel.Static)
            {
                tables.Remove("Certificate");
                tables.Remove("PkiConfiguration");
                tables.Remove("Host");
            }

            Logger.Info("Preparing to clear persistent storage on the G2S database");

            try
            {
                using var context = _contextFactory.Lock();
                foreach (var table in tables)
                {
                    context.Database.ExecuteSqlRaw($"DELETE FROM [{table}]");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to clear the G2S database", ex);
                throw;
            }
            finally
            {
                _contextFactory.Release();
            }

            if (_persistentStorage != null)
            {
                _persistentStorage.StorageClearingEventHandler -= OnStorageClearing;
            }

            Logger.Info("Finished clearing persistent storage on the G2S database");
        }
    }
}
