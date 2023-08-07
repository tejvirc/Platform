namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aft;
    using AftTransferProvider;
    using Aristocrat.Sas.Client;
    using Base;
    using ChangeRequests;
    using Common.Container;
    using Contracts.Client;
    using Exceptions;
    using Handlers;
    using HandPay;
    using Kernel;
    using log4net;
    using Mono.Addins;
    using SimpleInjector;
    using Ticketing;
    using Voucher;
    using VoucherValidation;

    /// <summary>
    ///     This class is used to find, load Sas Host and configure it to Container
    /// </summary>
    internal static class SasHostBuilder
    {
        private const string HostExtensionPath = "/Protocol/Sas/Services/Host";
        private const string ServicesConfigurationGroupNameBase = "SasServicesConfigurationGroup";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public static Container RegisterSasHost(this Container container)
        {
            var sasHost = LoadSasHost();
            container.RegisterInstance(typeof(ISasHost), sasHost);
            container.RegisterInstance(typeof(IPlatformCallbacks), sasHost);
            return container;
        }

        public static Container RegisterClientBuilder(this Container container)
        {
            container.Register<ISasExceptionHandler, SasExceptionHandler>(Lifestyle.Singleton);
            return container;
        }

        internal static Container RegisterSasHostComponents(this Container container)
        {
            container.Register<IAftRegistrationProvider, AftRegistrationProvider>(Lifestyle.Singleton);
            container.Register<ISasNoteAcceptorProvider, SasNoteAcceptorProvider>(Lifestyle.Singleton);
            container.Register<IPrinter, Printer>(Lifestyle.Singleton);
            container.Register<ICurrency, Currency>(Lifestyle.Singleton);
            container.Register<ISasTransactionCoordinator, TransactionCoordinator>(Lifestyle.Singleton);
            container.Register<ISasTicketPrintedHandler, SasTicketPrintedHandler>(Lifestyle.Singleton);
            container.Register<ISasDisableProvider, SasDisableProvider>(Lifestyle.Singleton);
            container.Register<ISasHandPayCommittedHandler, SasHandPayCommittedHandler>(Lifestyle.Singleton);
            container.Register<IAftTransferProvider, AftTransferProvider>(Lifestyle.Singleton);
            container.Register<IAftHistoryBuffer, AftHistoryBuffer>(Lifestyle.Singleton);
            container.Register<IAftTransferAssociations, AftTransferAssociations>(Lifestyle.Singleton);

            var aftTopLevelClasses = new List<Registration>
            {
                Lifestyle.Singleton.CreateRegistration(typeof(AftInterrogate), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftInterrogateStatusOnly), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferFullPartial), container)
            };

            container.Collection.Register(typeof(IAftRequestProcessorTransferCode), aftTopLevelClasses);
            var aftClasses = new List<Registration>
            {
                Lifestyle.Singleton.CreateRegistration(
                    typeof(AftTransferBonusCoinOutWinFromHostToGamingMachine),
                    container),
                Lifestyle.Singleton.CreateRegistration(
                    typeof(AftTransferBonusJackpotWinFromHostToGamingMachine),
                    container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferDebitFromHostToGamingMachine), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferDebitFromHostToTicket), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferInHouseFromGameMachineToHost), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferInHouseFromHostToGameMachine), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferInHouseFromHostToTicket), container),
                Lifestyle.Singleton.CreateRegistration(typeof(AftTransferWinAmountFromGameMachineToHost), container)
            };

            container.Collection.Register(typeof(IAftRequestProcessor), aftClasses);
            container.Register<ISasMeterChangeHandler, MeterChangeExceptionHandler>(Lifestyle.Singleton);
            container.Register<ISasChangeRequestManager, SasChangeRequestManager>(Lifestyle.Singleton);
            container.Register<IRteStatusProvider, LP0ERealTimeEventReportingHandler>(Lifestyle.Singleton);

            return container;
        }

        internal static Container RegisterSasHandlers(this Container container)
        {
            container.RegisterManyForOpenGeneric(
                typeof(ISasLongPollHandler<,>),
                true,
                Assembly.GetExecutingAssembly());
            return container;
        }

        internal static Container RegisterValidationHandler(this Container container)
        {
            container.RegisterManyAsCollection(
                typeof(IValidationHandler),
                Assembly.GetExecutingAssembly());
            container.Register<IHostValidationProvider, HostValidationProvider>(Lifestyle.Singleton);
            container.Register<SasValidationHandlerFactory>(Lifestyle.Singleton);
            container.Register<IEnhancedValidationProvider, EnhancedValidationProvider>(Lifestyle.Singleton);
            return container;
        }

        internal static Container RegisterTicketingComponents(this Container container)
        {
            container.Register<ITicketingCoordinator, TicketingCoordinator>(Lifestyle.Singleton);
            container.Register<ISasVoucherInProvider, SasVoucherInProvider>(Lifestyle.Singleton);
            return container;
        }

        private static ISasHost LoadSasHost()
        {
            var group = AddinConfigurationGroupNode.Get(ServicesConfigurationGroupNameBase);
            if (group == null)
            {
                throw new SasHostDiscoveryException(
                    "Unable to get the group: " + ServicesConfigurationGroupNameBase);
            }

            var nodeList = MonoAddinsHelper.GetConfiguredExtensionNodes<TypeExtensionNode>(
                new List<AddinConfigurationGroupNode> { group },
                HostExtensionPath,
                true);
            if (nodeList?.Count == 0)
            {
                throw new SasHostDiscoveryException("Unable to find the Sas Host from the path: " + HostExtensionPath);
            }

            Logger.Debug($"creating host {nodeList?.First().Type.Name}");
            return (ISasHost)nodeList?.First().CreateInstance();
        }
    }
}