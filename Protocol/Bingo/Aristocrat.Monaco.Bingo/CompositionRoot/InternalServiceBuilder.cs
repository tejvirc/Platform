namespace Aristocrat.Monaco.Bingo.CompositionRoot
{
    using System.Reflection;
    using Aristocrat.Bingo.Client.Configuration;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Commands;
    using Common;
    using Common.CompositionRoot;
    using Common.Storage.Model;
    using Consumers;
    using GameEndWin;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Tickets;
    using Handpay;
    using Handpay.Strategies;
    using Monaco.Common.Container;
    using Monitors;
    using Services;
    using Services.Configuration;
    using Services.GamePlay;
    using Services.Reporting;
    using Services.Security;
    using SimpleInjector;
    using ConfigurationFactory = Common.IBingoStrategyFactory<Services.Configuration.IConfiguration, Services.Configuration.ConfigurationType>;
    using GameEndWinFactory = Common.IBingoStrategyFactory<GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;
    using JackpotDeterminationFactory = Common.IBingoStrategyFactory<Handpay.Strategies.IJackpotDeterminationStrategy, Common.Storage.Model.JackpotDetermination>;

    public static class InternalServiceBuilder
    {
        public static Container AddInternalServices(this Container container)
        {
            container.RegisterSingleton<IClientConfigurationProvider, BingoClientConfigurationProvider>();
            container.RegisterSingleton<INetworkInformationProvider, NetworkInformationProvider>();
            return container
                .SetupBingoGamePlay()
                .SetupCommandHandlers()
                .SetupCommandFactory()
                .SetupGameEndWinStrategy()
                .SetupJackpotStrategy();
        }

        private static Container SetupBingoGamePlay(this Container container)
        {
            container.RegisterSingleton<IBingoClientConnectionState, BingoClientConnectionState>();
            container.RegisterSingleton<IPseudoRandomNumberGenerator, MersenneTwisterRng>();
            container.RegisterSingleton<IBingoCardProvider, BingoCardProvider>();
            container.RegisterSingleton<IBingoDisableProvider, BingoDisableProvider>();
            container.RegisterSingleton<ICentralHandler, CentralHandler>();
            container.RegisterSingleton<IBingoGameOutcomeHandler, CentralHandler>();
            container.RegisterSingleton<ITotalWinValidator, TotalWinValidator>();
            container.RegisterSingleton<IReportTransactionQueueService, TransactionHandler>();
            container.RegisterSingleton<IReportEventQueueService, ReportEventHandler>();
            container.RegisterSingleton<IGameHistoryReportHandler, GameHistoryReportHandler>();
            container.RegisterSingleton<ISharedConsumer, SharedConsumerContext>();
            container.RegisterSingleton<IAcknowledgedQueue<ReportTransactionMessage, int>, AcknowledgedQueue<ReportTransactionMessage, int>>();
            container.RegisterSingleton<IAcknowledgedQueueHelper<ReportTransactionMessage, int>, TransactionAcknowledgedQueueHelper>();
            container.RegisterSingleton<IAcknowledgedQueue<ReportEventMessage, int>, AcknowledgedQueue<ReportEventMessage, int>>();
            container.RegisterSingleton<IAcknowledgedQueueHelper<ReportEventMessage, int>, EventAcknowledgedQueueHelper>();
            container.RegisterSingleton<IAcknowledgedQueue<ReportGameOutcomeMessage, long>, AcknowledgedQueue<ReportGameOutcomeMessage, long>>();
            container.RegisterSingleton<IAcknowledgedQueueHelper<ReportGameOutcomeMessage, long>, GameHistoryReportAcknowledgeQueueHelper>();
            container.RegisterSingleton<IBingoGameProvider, BingoGameProvider>();
            container.RegisterSingleton<IEgmStatusService, EgmStatusHandler>();
            container.RegisterSingleton<IGameRoundPrintFormatter, BingoRoundPrintFormatter>();
            container.RegisterSingleton<IBingoReplayRecovery, BingoReplayRecovery>();
            container.RegisterSingleton<ICertificateService, CertificateService>();
            container.RegisterSingleton<DynamicHelpMonitor>();
            container.RegisterSingleton<ClientActivityService>();
            container.RegisterManyAsCollection(typeof(IMeterMonitor), Assembly.GetExecutingAssembly());
            return container;
        }

        private static Container SetupCommandHandlers(this Container container)
        {
            container.Register(typeof(ICommandHandler<>), Assembly.GetExecutingAssembly());
            container.RegisterSingleton<ICommandHandlerFactory, CommandHandlerFactory>();
            return container;
        }

        private static Container SetupJackpotStrategy(this Container container)
        {
            var factory = new BingoStrategyFactory<IJackpotDeterminationStrategy, JackpotDetermination>(container);
            factory.Register<TotalWinsJackpotDeterminationStrategy>(JackpotDetermination.TotalWins, Lifestyle.Transient);
            factory.Register<InterimPatternJackpotDeterminationStrategy>(JackpotDetermination.InterimPattern, Lifestyle.Transient);
            container.RegisterInstance<JackpotDeterminationFactory>(factory);
            container.RegisterSingleton<HandpayService>();
            return container;
        }

        private static Container SetupGameEndWinStrategy(this Container container)
        {
            var factory = new BingoStrategyFactory<IGameEndWinStrategy, GameEndWinStrategy>(container);
            factory.Register<GameEndWinBonusCreditsStrategy>(GameEndWinStrategy.BonusCredits, Lifestyle.Transient);
            container.RegisterInstance<GameEndWinFactory>(factory);
            return container;
        }

        private static Container SetupCommandFactory(this Container container)
        {
            var configurationFactory = new BingoStrategyFactory<IConfiguration, ConfigurationType>(container);
            configurationFactory.Register<MachineAndGameConfiguration>(ConfigurationType.MachineAndGameConfiguration, Lifestyle.Transient);
            configurationFactory.Register<SystemConfiguration>(ConfigurationType.SystemConfiguration, Lifestyle.Transient);
            configurationFactory.Register<ComplianceConfiguration>(ConfigurationType.ComplianceConfiguration, Lifestyle.Transient);
            configurationFactory.Register<MessageConfiguration>(ConfigurationType.MessageConfiguration, Lifestyle.Transient);
            container.RegisterInstance<ConfigurationFactory>(configurationFactory);

            return container;
        }
    }
}
