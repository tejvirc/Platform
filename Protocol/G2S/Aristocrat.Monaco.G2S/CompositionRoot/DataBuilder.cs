namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Data.CommConfig;
    using Data.EventHandler;
    using Data.Hosts;
    using Data.Meters;
    using Data.OptionConfig;
    using Data.Packages;
    using Data.Printers;
    using Data.Profile;
    using Data.Voucher;
    using Data.IdReader;
    using SimpleInjector;

    /// <summary>
    ///     Handles configuring the G2S Data.
    /// </summary>
    internal static class DataBuilder
    {
        /// <summary>
        ///     Registers Data with the container.
        /// </summary>
        /// <param name="this">The container.</param>
        /// <param name="connectionString">The connection string.</param>
        internal static void RegisterData(this Container @this, string connectionString)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            @this.Register<IHostRepository, HostRepository>(Lifestyle.Singleton);
            @this.Register<IProfileDataRepository, ProfileDataRepository>(Lifestyle.Singleton);

            @this.Register<IEventHandlerLogRepository, EventHandlerLogRepository>(Lifestyle.Singleton);
            @this.Register<IEventSubscriptionRepository, EventSubscriptionRepository>(Lifestyle.Singleton);
            @this.Register<ISupportedEventRepository, SupportedEventRepository>(Lifestyle.Singleton);

            @this.Register<IMeterSubscriptionRepository, MeterSubscriptionRepository>(Lifestyle.Singleton);
            @this.Register<IOptionChangeLogRepository, OptionChangeLogRepository>(Lifestyle.Singleton);
            @this.Register<IOptionConfigDeviceRepository, OptionConfigDeviceRepository>(Lifestyle.Singleton);

            @this.Register<ICommChangeLogRepository, CommChangeLogRepository>(Lifestyle.Singleton);
            @this.Register<ICommHostConfigItemRepository, CommHostConfigItemRepository>(Lifestyle.Singleton);
            @this.Register<ICommHostConfigRepository, CommHostConfigRepository>(Lifestyle.Singleton);
            @this.Register<IConfigChangeAuthorizeItemRepository, ConfigChangeAuthorizeItemRepository>(Lifestyle.Singleton);
            @this.Register<IOptionConfigItemRepository, OptionConfigItemRepository>(Lifestyle.Singleton);

            @this.Register<IPrintLogRepository, PrintLogRepository>(Lifestyle.Singleton);
            @this.Register<IPackageLogRepository, PackageLogRepository>(Lifestyle.Singleton);
            @this.Register<IVoucherDataRepository, VoucherDataRepository>(Lifestyle.Singleton);
            @this.Register<IIdReaderDataRepository, IdReaderDataRepository>(Lifestyle.Singleton);
        }
    }
}
