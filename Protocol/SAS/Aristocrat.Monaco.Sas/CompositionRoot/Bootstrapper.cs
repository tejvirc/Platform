namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Common.Container;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// This class is mainly used to compose the object graph for this layer
    /// </summary>
    internal static class Bootstrapper
    {
        private static readonly List<IService> AllServices = new List<IService>();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal static Container ConfigureContainer()
        {
            var container = new Container();
            container.AddResolveUnregisteredType(typeof(Bootstrapper).FullName, Logger);

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.ResolveUnregisteredConcreteTypes = true;
            return container.RegisterSasHost()
                .RegisterClientBuilder()
                .RegisterSasHandPay()
                .RegisterSasFundsTransfer()
                .RegisterSasBonusing()
                .RegisterSasTicketing()
                .RegisterProgressives()
                .RegisterSasHostComponents()
                .RegisterExternalServices()
                .RegisterInternalHelpers()
                .ConfigureConsumers()
                .RegisterValidationHandler()
                .RegisterTicketingComponents()
                .RegisterSasHandlers()
                .RegisterDbContext();
        }

        internal static void EnableServices(Container container)
        {
            OnAddingService(new Base.SasContainerService(container));

            // Add Sas Host into ServiceManager
            OnAddingService(container.GetInstance<ISasHost>() as IService);

            // Add this Sas service into ServiceManager
            var validatorProvider = ServiceManager.GetInstance().GetService<IValidationProvider>();
            var handpayValidator = container.GetInstance<IHandpayValidator>();
            OnAddProtocolService(
                handpayValidator,
                validatorProvider.Register(ProtocolNames.SAS, handpayValidator));

            var propertyMan = container.GetInstance<IPropertiesManager>();
            var settings = propertyMan.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (settings.AftAllowed)
            {
                // Add Aft On / Off into ServiceManager
                OnAddingService(container.GetInstance<IAftLockHandler>() as IService);
                var fundTransferProvider = ServiceManager.GetInstance().GetService<IFundTransferProvider>();
                var watTransferOffProvider = container.GetInstance<IWatTransferOffProvider>();
                OnAddProtocolService(
                    watTransferOffProvider,
                    fundTransferProvider.Register(ProtocolNames.SAS, watTransferOffProvider));
                var watTransferOnProvider = container.GetInstance<IWatTransferOnProvider>();
                OnAddProtocolService(
                    watTransferOnProvider,
                    fundTransferProvider.Register(ProtocolNames.SAS, watTransferOnProvider));
            }

            // Add Sas Ticket Validation into ServiceManager
            var voucherValidator = container.GetInstance<IVoucherValidator>();
            OnAddProtocolService(
                voucherValidator,
                validatorProvider.Register(ProtocolNames.SAS, voucherValidator));

            if ((bool)propertyMan.GetProperty(SasProperties.MeterChangeNotificationSupportedKey, true))
            {
                OnAddingService(container.GetInstance<ISasChangeRequestManager>() as IService);
            }
        }

        internal static void OnExiting()
        {
            var serviceManager = ServiceManager.GetInstance();
            AllServices.ForEach(service => serviceManager.RemoveService(service));
            AllServices.Clear();
        }

        internal static void OnAddingService(IService service)
        {
            ServiceManager.GetInstance().AddService(service);
            AllServices.Add(service);
            service.Initialize();
        }

        private static void OnAddProtocolService(IService service, bool isSuccess)
        {
            if (!isSuccess)
            {
                return;
            }

            AllServices.Add(service);
            service.Initialize();
        }

        private static Container ConfigureConsumers(this Container container)
        {
            container.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
            return container;
        }
    }
}
