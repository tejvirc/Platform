namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CompositionRoot;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    ///     Handles storage events like clearing persistent storage
    /// </summary>
    public sealed class StorageHandler : IService, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <inheritdoc />
        public void Dispose()
        {
            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(StorageHandler) };

        /// <inheritdoc />
        public void Initialize()
        {
            // Because of the order in which things are torn down we need to jump through some hoops to make sure our callback is invoked
            ServiceManager.GetInstance()
                .GetService<IEventBus>()
                .Subscribe<PersistentStorageClearStartedEvent>(
                    this,
                    _ =>
                    {
                        ServiceManager.GetInstance().GetService<IPersistentStorageManager>().StorageClearingEventHandler
                            += OnStorageClearing;
                    });
        }

        private static void OnStorageClearing(object sender, StorageEventArgs e)
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

            var factory = new DbContextFactory();
            try
            {
                using (var context = factory.Lock())
                {
                    foreach (var table in tables)
                    {
                        context.Database.ExecuteSqlRaw($"DELETE FROM [{table}]");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to clear the G2S database", ex);
                throw;
            }
            finally
            {
                factory.Release();
            }

            var storageManager = ServiceManager.GetInstance().TryGetService<IPersistentStorageManager>();
            if (storageManager != null)
            {
                storageManager.StorageClearingEventHandler -= OnStorageClearing;
            }

            Logger.Info("Finished clearing persistent storage on the G2S database");
        }
    }
}
