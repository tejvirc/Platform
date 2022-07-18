namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System.Data.Entity;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using AftTransferProvider;
    using Aristocrat.Monaco.Sas.Base;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using BonusProvider;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Eft;
    using EftTransferProvider;
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
            var aftOffTransferProvider = Lifestyle.Singleton.CreateRegistration<AftOffTransferProvider>(container);
            var nullOffTransferProvider = Lifestyle.Singleton.CreateRegistration<NullOffTransferProvider>(container);
            var eftOffTransferProvider = Lifestyle.Singleton.CreateRegistration<EftOffTransferProvider>(container);

            if (settings.EftAllowed)
            {
                container.AddRegistration(typeof(IWatTransferOffProvider), settings.TransferOutAllowed ? (eftOffTransferProvider) : nullOffTransferProvider);
                container.AddRegistration(typeof(IEftOffTransferProvider), settings.TransferOutAllowed ? eftOffTransferProvider : nullOffTransferProvider);
                container.AddRegistration(typeof(IAftOffTransferProvider), nullOffTransferProvider);
                
            }
            else if (settings.AftAllowed)
            {
                container.AddRegistration(typeof(IWatTransferOffProvider), settings.TransferOutAllowed ? aftOffTransferProvider : nullOffTransferProvider);
                container.AddRegistration(typeof(IAftOffTransferProvider), settings.TransferOutAllowed ? aftOffTransferProvider : nullOffTransferProvider);
                container.AddRegistration(typeof(IEftOffTransferProvider), nullOffTransferProvider);
            }
            else
            {
                container.AddRegistration(typeof(IWatTransferOffProvider), nullOffTransferProvider);
                container.AddRegistration(typeof(IAftOffTransferProvider), nullOffTransferProvider);
                container.AddRegistration(typeof(IEftOffTransferProvider), nullOffTransferProvider);
            }

            container.Register<IEftHostCashOutProvider, EftHostCashoutProvider>(Lifestyle.Singleton); 
            container.Register<IAftHostCashOutProvider, AftHostCashOutProvider>(Lifestyle.Singleton);

            var nullOnTransferProvider = Lifestyle.Singleton.CreateRegistration<NullOnTransferProvider>(container);
            var aftOnTransferProvider = Lifestyle.Singleton.CreateRegistration<AftOnTransferProvider>(container);
            var eftOnTransferProvider = Lifestyle.Singleton.CreateRegistration<EftOnTransferProvider>(container);


            if (settings.EftAllowed)
            {
                container.AddRegistration(typeof(IWatTransferOnProvider), settings.TransferInAllowed ? eftOnTransferProvider : nullOnTransferProvider);
                container.AddRegistration(typeof(IAftOnTransferProvider), nullOnTransferProvider);
                container.AddRegistration(typeof(IEftOnTransferProvider), settings.TransferInAllowed ? eftOnTransferProvider : nullOnTransferProvider);
            }
            else if (settings.AftAllowed)
            {
                container.AddRegistration(typeof(IWatTransferOnProvider), settings.TransferInAllowed ? aftOnTransferProvider : nullOnTransferProvider);
                container.AddRegistration(typeof(IAftOnTransferProvider), settings.TransferInAllowed ? aftOnTransferProvider : nullOnTransferProvider);
                container.AddRegistration(typeof(IEftOnTransferProvider), nullOnTransferProvider);
            }
            else
            {
                container.AddRegistration(typeof(IWatTransferOnProvider), nullOnTransferProvider);
                container.AddRegistration(typeof(IAftOnTransferProvider), nullOnTransferProvider);
                container.AddRegistration(typeof(IEftOnTransferProvider), nullOnTransferProvider);
            }

            container.Register<IEftStateController, EftStateController>(Lifestyle.Singleton);

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
