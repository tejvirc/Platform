namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System.Data.Entity;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using AftTransferProvider;
    using BonusProvider;
    using Contracts.Client;
    using Contracts.SASProperties;
    using HandPay;
    using Kernel;
    using Progressive;
    using Protocol.Common.Storage;
    using Protocol.Common.Storage.Entity;
    using Protocol.Common.Storage.Repositories;
    using SimpleInjector;
    using SimpleInjector.Diagnostics;
    using Storage;
    using Storage.Models;
    using Storage.Repository;
    using VoucherValidation;

    internal static class SasServiceBuilder
    {
        private static readonly IPropertiesManager PropertyMan;

        static SasServiceBuilder()
        {
            var serviceMan = ServiceManager.GetInstance();
            PropertyMan = serviceMan.GetService<IPropertiesManager>();
        }

        internal static Container RegisterSasHandPay(this Container container)
        {
            var registration = Lifestyle.Singleton.CreateRegistration<SasHandPay>(container);
            container.AddRegistration(typeof(IHandpayValidator), registration);
            return container;
        }

        internal static Container RegisterSasFundsTransfer(this Container container)
        {
            // Register Aft services if enabled, Registers null Aft services if not enabled
            // Register Aft Lock handler
            var settings = PropertyMan.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (settings.AftAllowed)
            {
                container.Register<IAftLockHandler, AftLockHandler>(Lifestyle.Singleton);
            }
            else
            {
                container.Register<IAftLockHandler, NullAftLockHandler>(Lifestyle.Singleton);
            }

            // Register Aft Off Provider
            var registration = settings.TransferOutAllowed
                ? Lifestyle.Singleton.CreateRegistration<AftOffTransferProvider>(container)
                : Lifestyle.Singleton.CreateRegistration<NullAftOffTransferProvider>(container);

            container.AddRegistration(typeof(IWatTransferOffProvider), registration);
            container.AddRegistration(typeof(IAftOffTransferProvider), registration);
            container.Register<IHostCashOutProvider, HostCashOutProvider>(Lifestyle.Singleton);

            // Register Aft On Provider
            registration = settings.TransferInAllowed
                ? Lifestyle.Singleton.CreateRegistration<AftOnTransferProvider>(container)
                : Lifestyle.Singleton.CreateRegistration<NullAftOnTransferProvider>(container);

            container.AddRegistration(typeof(IWatTransferOnProvider), registration);
            container.AddRegistration(typeof(IAftOnTransferProvider), registration);
            return container;
        }

        internal static Container RegisterSasBonusing(this Container container)
        {
            // Register Aft Bonus if enabled
            var settings = PropertyMan.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            container.RegisterConditional<ISasBonusCallback, SasBonusProvider>(Lifestyle.Singleton, _ => settings.LegacyBonusAllowed || settings.AftBonusAllowed);
            container.RegisterConditional<ISasBonusCallback, NullBonusProvider>(Lifestyle.Singleton, _ => !settings.LegacyBonusAllowed && !settings.AftBonusAllowed);

            return container;
        }

        internal static Container RegisterSasTicketing(this Container container)
        {
            if (!PropertyMan.GetValue(SasProperties.TicketingSupportedKey, true))
            {
                return container;
            }

            var registration = Lifestyle.Singleton.CreateRegistration<SasVoucherValidation>(container);
            container.AddRegistration(typeof(IVoucherValidator), registration);
            container.AddRegistration(typeof(ITicketDataProvider), registration);
            return container;
        }

        internal static Container RegisterProgressives(this Container container)
        {
            container.Register<IProgressiveHitExceptionProvider, ProgressiveHitExceptionProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveWinDetailsProvider, ProgressiveWinDetailsProvider>(Lifestyle.Singleton);

            return container;
        }

        internal static Container RegisterDbContext(this Container container)
        {
            container.RegisterSingleton<IConnectionStringResolver, DefaultConnectionStringResolver>();
            container.Register<DbContext, SasContext>(Lifestyle.Scoped);
            container.Register(typeof(IRepository<>), typeof(Repository<>), Lifestyle.Scoped);

            var registration = Lifestyle.Transient.CreateRegistration<UnitOfWork>(container);
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ignore");
            container.AddRegistration(typeof(IUnitOfWork), registration);
            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
            container.RegisterSingleton(typeof(IStorageDataProvider<>), typeof(StorageDataProvider<>));

            return container;
        }
    }
}
