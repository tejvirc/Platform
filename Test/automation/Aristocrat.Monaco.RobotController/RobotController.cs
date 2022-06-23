namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Test.Automation;
    using Contracts;
    using Kernel;
    using log4net;

    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private readonly Configuration _config;        
        private readonly ILog _logger;
        private readonly Dictionary<string, IRobotService> _serviceCollection;
        private IBank _bank;
        private IEventBus _eventBus;
        private IPropertiesManager _pm;
        private ILobbyStateManager _lobbyStateManager;
        private IGamePlayState _gamePlayState;
        private IGameService _gameService;
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public RobotController(Configuration config,
                               IBank bank,
                               ILog logger,
                               IEventBus eventBus,
                               bool enabled)
        {
            _config = config;
            _bank = bank;
            _logger = logger;
            _eventBus = eventBus;
            Enabled = enabled;
            _serviceCollection = new Dictionary<string, IRobotService>();
        }

        protected override void OnInitialize()
        {
            WaitForServices();
            SuperRobotInitialization();
        }
        private void WaitForServices()
        {
            Task.Run(() =>
            {
                using (var serviceWaiter = new ServiceWaiter(_eventBus))
                {
                    serviceWaiter.AddServiceToWaitFor<IPropertiesManager>();
                    serviceWaiter.AddServiceToWaitFor<ILobbyStateManager>();
                    serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
                    serviceWaiter.AddServiceToWaitFor<IGameService>();
                    if (serviceWaiter.WaitForServices())
                    {
                        _pm = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
                        _lobbyStateManager = ServiceManager.GetInstance().GetService<ILobbyStateManager>();
                        _gamePlayState = ServiceManager.GetInstance().GetService<IGamePlayState>();
                        _gameService = ServiceManager.GetInstance().GetService<IGameService>();
                    }
                }
            });
        }
        private void SuperRobotInitialization()
        {
            _serviceCollection.Add(typeof(BalanceCheck).ToString(), new BalanceCheck(_config, _lobbyStateManager, _gamePlayState, _bank, _logger, _eventBus));
            _serviceCollection.Add(typeof(ActionTouch).ToString(), new ActionTouch(_config, _lobbyStateManager, _logger, new Automation(_pm, _eventBus)));
            _serviceCollection.Add(typeof(ActionPlayer).ToString(), new ActionPlayer(_config, _lobbyStateManager, _logger, new Automation(_pm, _eventBus)));
        }

        protected override void OnRun()
        {
            foreach (var service in _serviceCollection)
            {
                service.Value.Execute();
            }
            while (Enabled)
            {
                //if(TryLoadGame()){

                //}
            }
        }

        protected override void OnStop()
        {
            foreach (var service in _serviceCollection)
            {
                service.Value.Halt();
            }
        }
    }
}
