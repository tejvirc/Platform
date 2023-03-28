namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.CryptoRng;
    using Barkeeper;
    using BeagleBone;
    using Bonus;
    using Bonus.Strategies;
    using Commands;
    using Commands.RuntimeEvents;
    using Common.Container;
    using Configuration;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Bonus;
    using Contracts.Central;
    using Contracts.Configuration;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Process;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Contracts.Session;
    using GameRound;
    using Hardware.Contracts;
    using Kernel;
    using Monitor;
    using PackageManifest;
    using PackageManifest.Ati;
    using PackageManifest.Gsa;
    using PackageManifest.Models;
    using Payment;
    using Progressives;
    using Runtime;
    using Runtime.Client;
    using Runtime.Server;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using TowerLight;

    /// <summary>
    ///     Defines the Bootstrapper class
    /// </summary>
    internal static class Bootstrapper
    {
        /// <summary>
        ///     Initialize the container
        /// </summary>
        /// <param name="configureHost"></param>
        /// <returns>A container</returns>
        public static Container InitializeContainer(Action<Container> configureHost)
        {
            return ConfigureContainer(configureHost);
        }

        private static Container ConfigureContainer(Action<Container> configureHost)
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = ScopedLifestyle.Flowing;

            configureHost?.Invoke(container);

            container.Register<SnappService>(Lifestyle.Singleton);
            container.Register<SnappReelService>(Lifestyle.Singleton);
            container.Register<SnappPresentationService>(Lifestyle.Singleton);
            container.Register<IRuntime, RuntimeProxy>(Lifestyle.Singleton);
            container.Register<IReelService, ReelServiceProxy>(Lifestyle.Singleton);
            container.Register<IPresentationService, PresentationServiceProxy>(Lifestyle.Singleton);
            container.Register<IClientEndpointProvider<IRuntime>, RuntimeEndpointProvider>(Lifestyle.Singleton);
            container.Register<IClientEndpointProvider<IReelService>, RpcClientEndpointProvider<IReelService>>(Lifestyle.Singleton);
            container.Register<IClientEndpointProvider<IPresentationService>, RpcClientEndpointProvider<IPresentationService>>(Lifestyle.Singleton);
            container.Collection.Register<IServerEndpoint>(
                new[]
                {
                    Lifestyle.Singleton.CreateRegistration(typeof(SnappServer), container),
                });
            
            container.Register<IGameService, GameService>(Lifestyle.Singleton);
            container.Register<IGameProcess, GameProcess>(Lifestyle.Singleton);
            container.Register<IProcessManager, GameProcessManager>(Lifestyle.Singleton);
            container.Register<IProcessCommunication, GameProcessCommunication>(Lifestyle.Singleton);
            container.Register<IGameMeterManager, GameMeterManager>(Lifestyle.Singleton);
            container.Register<IManifest<GameContent>, GameManifest>(Lifestyle.Singleton);
            container.Register<IManifest<IEnumerable<ProgressiveDetail>>, ProgressiveManifest>(Lifestyle.Singleton);
            container.Register<IManifest<Image>, ImageManifest>(Lifestyle.Singleton);
            container.Register<IGameProvider, GameProvider>(Lifestyle.Singleton);
            container.Register<IGameCategoryService, GameCategoryService>(Lifestyle.Singleton);
            container.Register<IGameHelpTextProvider, GameHelpTextProvider>(Lifestyle.Singleton);
            container.Register<ICabinetState, CabinetState>(Lifestyle.Singleton);
            container.Register<ICabinetService, CabinetService>(Lifestyle.Singleton);
            container.Register<IGamePlayState, GamePlayState>(Lifestyle.Singleton);
            container.Register<IGameRecovery, GameRecovery>(Lifestyle.Singleton);
            container.Register<IGameDiagnostics, GameDiagnostics>(Lifestyle.Singleton);
            container.Register<IGameHistory, GameHistory>(Lifestyle.Singleton);
            container.Register<IGameCashOutRecovery, GameCashOutRecovery>(Lifestyle.Singleton);
            container.Register<IRuntimeProvider, RuntimeProvider>(Lifestyle.Singleton);
            container.Register<IButtonDeckFilter, ButtonDeckFilter>(Lifestyle.Singleton);
            container.Register<IPlayerBank, PlayerBank>(Lifestyle.Singleton);
            container.Register<IGameStorage, GameStorageManager>(Lifestyle.Singleton);
            container.Register<IRuntimeFlagHandler, RuntimeFlagHandler>(Lifestyle.Singleton);
            container.Register<IButtonLamps, ButtonLamps>(Lifestyle.Singleton);
            container.Register<IGameOrderSettings, GameOrderSettings>(Lifestyle.Singleton);
            container.Register<IOperatorMenuGamePlayMonitor, OperatorMenuGamePlayMonitor>(Lifestyle.Singleton);
            container.Register<ICurrencyInContainer, CurrencyInContainer>(Lifestyle.Singleton);
            container.Register<ILoggedEventContainer, LoggedEventContainer>(Lifestyle.Singleton);
            container.Register<IGameRoundMeterSnapshotProvider, GameRoundMeterSnapshotProvider>(Lifestyle.Singleton);
            container.Register<IPlayerService, PlayerService>(Lifestyle.Singleton);
            container.Register<IPlayerSessionHistory, PlayerSessionHistory>(Lifestyle.Singleton);
            container.Register<IMessageDisplayHandler, GameMessageDisplayHandler>(Lifestyle.Singleton);
            container.Register<IAttendantService, AttendantService>(Lifestyle.Singleton);
            container.Register<IReserveService, ReserveService>(Lifestyle.Singleton);
            container.Register<IUserActivityService, UserActivityService>(Lifestyle.Singleton);
            container.Register<IHardwareHelper, HardwareHelper>(Lifestyle.Singleton);
            container.Register<ITowerLightManager, TowerLightManager>(Lifestyle.Singleton);
            container.Register<IBonusHandler, BonusHandler>(Lifestyle.Singleton);
            container.Register<IFundsTransferDisable, FundsTransferDisable>(Lifestyle.Singleton);
            container.Register<ICashableLockupProvider, CashableLockupProvider>(Lifestyle.Singleton);
            container.Register<IBarkeeperPropertyProvider, BarkeeperPropertyProvider>(Lifestyle.Singleton);
            container.Register<IBarkeeperHandler, BarkeeperHandler>(Lifestyle.Singleton);
            container.Register<ICentralProvider, CentralProvider>(Lifestyle.Singleton);
            container.Register<IPaymentDeterminationProvider, PaymentDeterminationProvider>(Lifestyle.Singleton);
            container.Register<IGameStartConditionProvider, GameStartConditionProvider>(Lifestyle.Singleton);
            container.Register<IOutcomeValidatorProvider, OutcomeValidatorProvider>(Lifestyle.Singleton);
            container.Register<IConfigurationProvider, ConfigurationProvider>(Lifestyle.Singleton);
            container.Register<IGameConfigurationProvider, GameConfigurationProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveGameProvider, ProgressiveGameProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveConfigurationProvider, ProgressiveConfigurationProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveLevelProvider, ProgressiveLevelProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveErrorProvider, ProgressiveErrorProvider>(Lifestyle.Singleton);
            container.Register<ISharedSapProvider, SharedSapProvider>(Lifestyle.Singleton);
            container.Register<ILinkedProgressiveProvider, LinkedProgressiveProvider>(Lifestyle.Singleton);
            container.Register<IProgressiveBroadcastTimer, ProgressiveBroadcastTimer>(Lifestyle.Singleton);
            container.Register<ISapProvider, StandaloneProgressiveProvider>(Lifestyle.Singleton);
            container.Register<IProtocolLinkedProgressiveAdapter, ProtocolLinkedProgressiveAdapter>(Lifestyle.Singleton);
            container.Register<IHandpayRuntimeFlagsHelper, HandpayRuntimeFlagsHelper>(Lifestyle.Singleton);
            container.Register<IReplayRuntimeEventHandler, ReplayRuntimeEventHandler>(Lifestyle.Singleton);
            container.Register<ReelControllerMonitor>(Lifestyle.Singleton);

            var progressiveCalculatorFactory = new ProgressiveCalculatorFactory(container);
            progressiveCalculatorFactory.Register<StandardCalculator>(SapFundingType.Standard);
            progressiveCalculatorFactory.Register<StandardCalculator>(SapFundingType.LineBased);
            progressiveCalculatorFactory.Register<BulkCalculator>(SapFundingType.LineBasedAnte);
            progressiveCalculatorFactory.Register<BulkCalculator>(SapFundingType.Ante);
            progressiveCalculatorFactory.Register<BulkCalculator>(SapFundingType.BulkOnly);
            container.RegisterInstance<IProgressiveCalculatorFactory>(progressiveCalculatorFactory);
            container.Register<IProgressiveMeterManager, ProgressiveMeterManager>(Lifestyle.Singleton);

            container.Register<IGamingAccessEvaluation, AccessEvaluationService>(Lifestyle.Singleton);
            container.Register<IHandProvider, PokerHandProvider>(Lifestyle.Singleton);
            container.Register<IGameRoundInfoParserFactory, GameRoundInfoParserFactory>(Lifestyle.Singleton);
            container.RegisterManyAsCollection(typeof(IGameRoundInfoParser), typeof(IGameRoundInfoParser).Assembly);
            container.Register<ICashoutController, CashoutController>(Lifestyle.Singleton);
            container.Register<SystemDrivenAutoPlayHandler>(Lifestyle.Singleton);
            container.Register<IAutoPlayStatusProvider, AutoPlayStatusProvider>(Lifestyle.Singleton);
            container.Register<LandingStripController>(Lifestyle.Singleton);
            container.Register<DisplayableMessageRemover>(Lifestyle.Singleton);
            container.Register<IAttractConfigurationProvider, AttractConfigurationProvider>(Lifestyle.Singleton);
            container.Register<BeagleBoneHandler>(Lifestyle.Singleton);

            var gameInstaller = Lifestyle.Singleton.CreateRegistration<GameInstaller>(container);
            container.AddRegistration(typeof(IGameInstaller), gameInstaller);

            var softwareInstaller = Lifestyle.Singleton.CreateRegistration<SoftwareInstaller>(container);
            container.AddRegistration(typeof(ISoftwareInstaller), softwareInstaller);

            var gamePropertyProvider = Lifestyle.Singleton.CreateRegistration<PropertyProvider>(container);
            var browserPropertyProvider = Lifestyle.Singleton.CreateRegistration<BrowserPropertyProvider>(container);

            container.Collection.Register<IPropertyProvider>(
                new[] { gamePropertyProvider, gameInstaller, browserPropertyProvider });

            container.Register<GameMeterProvider>(Lifestyle.Singleton);
            container.Register<PlayerMeterProvider>(Lifestyle.Singleton);
            container.Register<BonusMeterProvider>(Lifestyle.Singleton);

            container.Register<ProgressiveMeterProvider>(Lifestyle.Singleton);
            container.Register<CabinetMeterProvider>(Lifestyle.Singleton);
            container.Collection.Register<IMeterProvider>(
                typeof(GameMeterProvider),
                typeof(PlayerMeterProvider),
                typeof(BonusMeterProvider),
                typeof(ProgressiveMeterProvider),
                typeof(CabinetMeterProvider));

            container.Register<IRandomStateProvider, RandomStateProvider>(Lifestyle.Singleton);
            // IPRNG implementations are keyed by PRNGLib.RandomType:
            var rngFactory = new RandomFactory(container);
            rngFactory.Register<AtiCryptoRng>(RandomType.Gaming, Lifestyle.Singleton);
            container.Register<IRandomFactory>(() => rngFactory, Lifestyle.Singleton);

            var runtimeEventHandlerFactory = new RuntimeEventHandlerFactory(container);
            runtimeEventHandlerFactory.Register<PrimaryEventHandler>(GameRoundEventState.Primary);
            runtimeEventHandlerFactory.Register<PresentEventHandler>(GameRoundEventState.Present);
            runtimeEventHandlerFactory.Register<FreeGameEventHandler>(GameRoundEventState.FreeGame);
            runtimeEventHandlerFactory.Register<AllowCashInEventHandler>(GameRoundEventState.AllowCashInDuringPlay);
            runtimeEventHandlerFactory.Register<WaitingForPlayerInputEventHandler>(GameRoundEventState.WaitingForPlayerInput);
            container.RegisterInstance<IRuntimeEventHandlerFactory>(runtimeEventHandlerFactory);

            var bonusStrategyFactory = new BonusStrategyFactory(container);
            bonusStrategyFactory.Register<Standard>(BonusMode.Standard);
            bonusStrategyFactory.Register<Standard>(BonusMode.NonDeductible);
            bonusStrategyFactory.Register<MultipleJackpotTime>(BonusMode.MultipleJackpotTime);
            bonusStrategyFactory.Register<WagerMatch>(BonusMode.WagerMatch);
            bonusStrategyFactory.Register<Standard>(BonusMode.WagerMatchAllAtOnce);
            bonusStrategyFactory.Register<GameWinBonusStrategy>(BonusMode.GameWin);

            container.RegisterInstance<IBonusStrategyFactory>(bonusStrategyFactory);

            container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);
            container.Register<ICommandHandlerFactory>(() => new CommandHandlerFactory(container), Lifestyle.Singleton);
            container.Register<ExcessiveMeterIncrementMonitor>(Lifestyle.Singleton);

            container.Register<RngCyclingService>(Lifestyle.Singleton);

            //#if !(RETAIL)
            //            PerformanceCounters.RegisterFromAttribute(typeof(ICommandHandler<>).Assembly);

            //            container.InterceptWith<PerformanceInterceptor>(
            //                serviceType => serviceType.Name.StartsWith("ICommandHandler`"));

            //            container.RegisterSingleton<PerformanceInterceptor>();
            //#endif

            // Register the consumers
            container.ConfigureConsumers();

            container.RegisterExternalServices();

            return container;
        }

        private static void ConfigureConsumers(this Container @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            @this.RegisterManyForOpenGeneric(typeof(IConsumer<>), typeof(Consumers.Consumes<>).Assembly);
        }
    }
}