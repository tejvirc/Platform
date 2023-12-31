﻿namespace Aristocrat.Monaco.Hhr.CompositionRoot
{
    using System;
    using System.Data.Entity;
    using System.Reflection;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Client.Communication;
    using Client.Extensions;
    using Client.Mappings;
    using Client.WorkFlow;
    using Commands;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Protocol.Common.Storage;
    using Protocol.Common.Storage.Entity;
    using Protocol.Common.Storage.Repositories;
    using Services;
    using Services.GamePlay;
    using Services.Progressive;
    using SimpleInjector;
    using SimpleInjector.Diagnostics;
    using Storage;
    using Storage.Helpers;
    using Storage.Models;

    public static class Bootstrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void AddDbContext(this Container container)
        {
            container.RegisterSingleton<IConnectionStringResolver, DefaultConnectionStringResolver>();

            container.Register<DbContext, HHRContext>(Lifestyle.Scoped);

            container.RegisterConditional(typeof(IRepository<>), typeof(Repository<>), Lifestyle.Scoped, _ => true);

            var registration = Lifestyle.Transient.CreateRegistration<UnitOfWork>(container);
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ignore");
            container.AddRegistration(typeof(IUnitOfWork), registration);

            container.RegisterSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        }

        public static void InitializeBase(Container container)
        {
            var serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());

            serviceWaiter.AddServiceToWaitFor<ICentralProvider>();
            serviceWaiter.AddServiceToWaitFor<IBank>();
            serviceWaiter.AddServiceToWaitFor<IGameProvider>();
            serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            serviceWaiter.AddServiceToWaitFor<ICabinetService>();
            serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            serviceWaiter.AddServiceToWaitFor<IPlayerBank>();
            serviceWaiter.AddServiceToWaitFor<IPlayerSessionHistory>();
            serviceWaiter.AddServiceToWaitFor<IAttendantService>();
            serviceWaiter.AddServiceToWaitFor<IIdentificationProvider>();
            serviceWaiter.AddServiceToWaitFor<IProtocolLinkedProgressiveAdapter>();
            serviceWaiter.AddServiceToWaitFor<IPaymentDeterminationProvider>();
            serviceWaiter.AddServiceToWaitFor<IOutcomeValidatorProvider>();
            serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
            serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            serviceWaiter.AddServiceToWaitFor<IGameStartConditionProvider>();

            if (serviceWaiter.WaitForServices())
            {
                InitializeGames();
                ConfigureContainer(container);
            }
        }

        private static void ConfigureContainer(Container container)
        {
            container.AddExternalServices();

            container.AddInternalServices();

            container.AddMappings();

            container.AddClientServices();

            container.AddCommandHandlers();

            container.AddDbContext();
        }

        private static void AddInternalServices(this Container container)
        {
            container.RegisterSingleton<LockupManager>();
            container.Register(typeof(IRequestTimeoutBehavior<>), typeof(IRequestTimeoutBehavior<>).Assembly);

            container.RegisterSingleton<CentralHandler>();
            container.RegisterSingleton<IPrizeDeterminationService, PrizeDeterminationService>();
            container.RegisterSingleton<IPlayerSessionService, PlayerSessionService>();
            container.RegisterSingleton<IManualHandicapRaceInfoService, ManualHandicapRaceInfoService>();
            container.RegisterSingleton<IGameDataService, GameDataService>();
            container.RegisterSingleton<CreditInService>();
            container.RegisterSingleton<CreditOutService>();
            container.RegisterSingleton<IProgressiveUpdateService, ProgressiveUpdateService>();
            container.RegisterSingleton<ProgressiveHitHandler>();
            container.RegisterSingleton<IProgressiveBroadcastService, ProgressiveBroadcastService>();
            container.RegisterSingleton<IProgressiveAssociation, ProgressiveAssociation>();
            container.RegisterSingleton<ICommunicationService, CommunicationService>();
            container.RegisterSingleton<InitializationStateManager>();
            container.RegisterSingleton<HandpayService>();
            container.RegisterSingleton<PrizeInfoValidator>();
            container.RegisterSingleton<IPrizeInformationEntityHelper, PrizeInformationEntityHelper>();
            container.RegisterSingleton<BonusService>();
            container.RegisterSingleton<IGamePlayEntityHelper, GamePlayEntityHelper>();
            container.RegisterSingleton<IPendingRequestEntityHelper, PendingRequestEntityHelper>();
            container.RegisterSingleton<IGameRecoveryService, GameRecoveryService>();
            container.RegisterSingleton<SequenceIdObserver>();
            container.RegisterSingleton<IServiceProvider>(() => container);
            container.RegisterSingleton<IRequestTimeoutBehaviorService, RequestTimeoutBehaviorHandler>();
            container.RegisterSingleton<IGameSelectionVerificationService, GameSelectionVerificationService>();
            container.RegisterSingleton<IManualHandicapEntityHelper, ManualHandicapEntityHelper>();
            container.RegisterSingleton<IProgressiveUpdateEntityHelper, ProgressiveUpdateEntityHelper>();
            container.RegisterSingleton<ITransactionIdProvider, TransactionIdProvider>();
        }

        private static void AddClientServices(this Container container)
        {
            var egmSettings = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            container.RegisterClientServices();

            var encryptionKey = egmSettings.GetValue(HHRPropertyNames.EncryptionKey, string.Empty);
            container.RegisterSingleton<ICryptoProvider>(
                () => new CryptoProvider(encryptionKey));
            container.RegisterSingleton<ISequenceIdManager>(
                () => new SequenceIdManager(egmSettings.GetValue(HHRPropertyNames.SequenceId, 1u)));
            container.RegisterSingleton(
                () => ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IRuntimeFlagHandler>());
        }

        private static void AddExternalServices(this Container container)
        {
            var serviceManager = ServiceManager.GetInstance();
            container.RegisterInstance(serviceManager.GetService<ICentralProvider>());
            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IGameProvider>());
            container.RegisterInstance(serviceManager.GetService<IBank>());
            container.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IPathMapper>());
            container.RegisterInstance(serviceManager.GetService<IGameHistory>());
            container.RegisterInstance(serviceManager.GetService<IIdProvider>());
            container.RegisterInstance(serviceManager.GetService<IProgressiveLevelProvider>());
            container.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            container.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            container.RegisterInstance(serviceManager.GetService<ICabinetDetectionService>());
            container.RegisterInstance(serviceManager.GetService<IProtocolLinkedProgressiveAdapter>());
            container.RegisterInstance(serviceManager.GetService<IPaymentDeterminationProvider>());
            container.RegisterInstance(serviceManager.GetService<IOutcomeValidatorProvider>());
            container.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            container.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            container.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            container.RegisterInstance(serviceManager.GetService<IGameStartConditionProvider>());
        }

        private static void AddCommandHandlers(this Container container)
        {
            container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);
            container.RegisterSingleton<ICommandHandlerFactory, CommandHandlerFactory>();
        }

        private static void AddMappings(this Container container)
        {
            container.Register<MapperProvider>();
            container.RegisterSingleton(() => container.GetInstance<MapperProvider>().GetMapper());
        }

        private static void InitializeGames()
        {
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            foreach (var game in gameProvider.GetGames())
            {
                try
                {
                    gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);
                }
                catch (GamePlayCollisionException ex)
                {
                    Logger.Error($"Failed to enable - {game.Id} {game.ThemeId}", ex);
                }
            }
        }
    }
}