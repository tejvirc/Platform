namespace Aristocrat.Monaco.TestProtocol
{
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Configuration;
    using Gaming.Contracts.Session;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the TestProtocolRunnable class.
    /// </summary>
    [ProtocolCapability(
        protocol:CommsProtocol.Test,
        isValidationSupported:true,
        isFundTransferSupported: true,
        isProgressivesSupported:true,
        isCentralDeterminationSystemSupported:true)]
    // ReSharper disable once UnusedMember.Global
    public class TestProtocolRunnable : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ServiceWaiter _serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());

        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        private TestHandpayValidator _handpayValidator;
        private TestVoucherValidator _voucherValidator;
        private TestWatOffProvider _watOffProvider;
        private TestWatOnProvider _watOnProvider;
        private TestCurrencyValidator _currencyValidator;
        private CentralHandler _centralHandler;
        private TestProgressiveController _progressiveController;
        private TestIdentificationValidator _identificationValidator;

        private bool _waitForWat;

        private bool _disposed;

        protected override void OnInitialize()
        {
            Logger.Debug("Initializing...");

            var serviceManager = ServiceManager.GetInstance();

            _serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            _serviceWaiter.AddServiceToWaitFor<ICabinetService>();
            _serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerBank>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerSessionHistory>();
            _serviceWaiter.AddServiceToWaitFor<IAttendantService>();
            _serviceWaiter.AddServiceToWaitFor<ICentralProvider>();
            _serviceWaiter.AddServiceToWaitFor<IEmployeeLogin>();
            _serviceWaiter.AddServiceToWaitFor<IGameConfigurationProvider>();
            _serviceWaiter.AddServiceToWaitFor<IGameProvider>();

            _serviceWaiter.WaitForServices();

            _currencyValidator = new TestCurrencyValidator();
            _currencyValidator.Initialize();

            _voucherValidator = new TestVoucherValidator();
            _voucherValidator.Initialize();

            _handpayValidator = new TestHandpayValidator();
            _handpayValidator.Initialize();

            _watOffProvider = new TestWatOffProvider();
            _watOffProvider.Initialize();


            _identificationValidator = new TestIdentificationValidator();
            _identificationValidator.Initialize();

            if ((serviceManager.GetService<IValidationProvider>().Register(
                ProtocolNames.Test,
                _voucherValidator)))
            {
                Logger.Debug("Test protocol voucher validation provider is registered...");
            }
            if ((serviceManager.GetService<IValidationProvider>().Register(
                ProtocolNames.Test,
                _currencyValidator)))
            {
                Logger.Debug("Test protocol currency validation provider is registered...");
            }

            if ((serviceManager.GetService<IValidationProvider>().Register(
                ProtocolNames.Test,
                _handpayValidator)))
            {
                Logger.Debug("Test protocol handpay validation provider is registered...");
            }

            if ((serviceManager.GetService<IFundTransferProvider>().Register(
                ProtocolNames.Test,
                _watOffProvider)))
            {
                Logger.Debug("Test protocol wat off provider is registered...");
            }

            _waitForWat = true;

            ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .SetProperty(ApplicationConstants.ActiveProtocol, "Test");

            _serviceWaiter.AddServiceToWaitFor<IGameProvider>();
            _serviceWaiter.WaitForServices();

            EnableGames();

            var progressive = ServiceManager.GetInstance()
                .GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.FirstOrDefault(
                    p => p.Protocol == CommsProtocol.Test);

            if (progressive != null && progressive.IsProgressiveHandled)
            {
                _progressiveController = new TestProgressiveController();
                _progressiveController.Configure();
            }

            _centralHandler = new CentralHandler();

            // TODO : Move this to a central location after all protocols are initialized when multiple protocols are supported.
            serviceManager.GetService<IEventBus>().Publish(new ProtocolsInitializedEvent());

            Logger.Debug("Initialized.");
        }

        protected override void OnRun()
        {
            Logger.Debug("Running...");

            _shutdownEvent.WaitOne();

            Logger.Debug("Finished Run.");
        }

        protected override void OnStop()
        {
            Logger.Debug("Stopping...");

            _waitForWat = false;

            var serviceManager = ServiceManager.GetInstance();

            if ((serviceManager.GetService<IValidationProvider>().UnRegister(
                ProtocolNames.Test,
                _voucherValidator)))
            {
                Logger.Debug("Test protocol _voucherValidator is unRegistered...");
            }

            if ((serviceManager.GetService<IValidationProvider>().UnRegister(
                ProtocolNames.Test,
                _handpayValidator)))
            {
                Logger.Debug("Test protocol _handpayValidator is unRegistered...");
            }

            if ((serviceManager.GetService<IFundTransferProvider>().UnRegister(
                ProtocolNames.Test,
                _watOffProvider)))
            {
                Logger.Debug("Test protocol _watOffProvider is unRegistered...");
            }

            if (_watOnProvider != null && serviceManager.IsServiceAvailable<IWatTransferOnHandler>())
            {
                if ((serviceManager.GetService<IFundTransferProvider>().UnRegister(
                    ProtocolNames.Test,
                    _watOnProvider)))
                {
                    Logger.Debug("Test protocol _watOnProvider is unRegistered...");
                }
            }

            // Allow OnRun to exit
            _shutdownEvent?.Set();

            Logger.Debug("Stopped.");
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            Logger.Debug("Disposing...");

            if (disposing)
            {
                _watOnProvider?.Dispose();

                _serviceWaiter?.Dispose();

                _centralHandler?.Dispose();

                _progressiveController?.Dispose();

                _shutdownEvent?.Dispose();

                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }

            _watOnProvider = null;
            _serviceWaiter = null;
            _shutdownEvent = null;
            _centralHandler = null;
            _progressiveController = null;
            _disposed = true;

            base.Dispose(disposing);
        }

        private static void EnableGames()
        {
            var multiProtocolConfigurationProvider =
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>();

            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var gameConfiguration = ServiceManager.GetInstance().GetService<IGameConfigurationProvider>();
            var configurationProvider = ServiceManager.GetInstance().GetService<IConfigurationProvider>();

            var themeId = string.Empty;

            if (multiProtocolConfigurationProvider.MultiProtocolConfiguration.Any(x => x.Protocol == CommsProtocol.HHR))
            {
                return;
            }

            foreach (var game in gameProvider.GetGames())
            {
                try
                {
                    gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);

                    if (game.Enabled)
                    {
                        themeId = game.ThemeId;
                        continue;
                    }

                    var configuration = gameConfiguration.GetActive(game.ThemeId) ??
                                        configurationProvider.GetByThemeId(game.ThemeId).FirstOrDefault();

                    if (configuration == null)
                    {
                        // Enable the first denom for the first game we find
                        if (!game.ActiveDenominations.Any() &&
                            !game.ThemeId.Equals(themeId, StringComparison.InvariantCultureIgnoreCase) &&
                            multiProtocolConfigurationProvider.MultiProtocolConfiguration.All(x => x.Protocol == CommsProtocol.HHR))
                        {
                            themeId = game.ThemeId;

                            gameProvider.SetActiveDenominations(game.Id, game.SupportedDenominations.Take(1));
                        }
                    }
                    else
                    {
                        var activeDenoms = new List<long>();

                        foreach (var denom in game.Denominations)
                        {
                            if (denom.Active)
                            {
                                break;
                            }

                            var mapping = configuration.RestrictionDetails.Mapping?.FirstOrDefault(
                                c => denom.Value == c.Denomination && c.VariationId == game.VariationId);
                            if (mapping != null)
                            {
                                activeDenoms.Add(denom.Value);
                            }
                        }

                        if (activeDenoms.Any())
                        {
                            gameProvider.SetActiveDenominations(game.Id, activeDenoms);
                        }

                        if (gameConfiguration.GetActive(game.ThemeId) == null)
                        {
                            gameConfiguration.Apply(game.ThemeId, configuration);
                        }
                    }
                }
                catch (GamePlayCollisionException ex)
                {
                    Logger.Error($"Failed to enable - {game.Id} {game.ThemeId}", ex);
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void AddWatOnProvider()
        {
            Logger.Debug("Adding WAT-On provider...");

            _watOnProvider = null;

            var serviceManager = ServiceManager.GetInstance();
            while (_waitForWat && !serviceManager.IsServiceAvailable<IWatTransferOnHandler>())
            {
                Thread.Sleep(100);
            }

            if (_waitForWat)
            {
                _watOnProvider = new TestWatOnProvider();
                _watOnProvider.Initialize();

                if ((serviceManager.GetService<IFundTransferProvider>().Register(
                    ProtocolNames.Test,
                    _watOnProvider)))
                {
                    Logger.Debug("WAT-On provider added.");
                }

            }
            else
            {
                Logger.Debug("Adding WAT-On provider canceled.");
            }
        }
    }
}