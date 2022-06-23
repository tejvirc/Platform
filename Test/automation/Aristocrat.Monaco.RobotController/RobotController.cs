namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Contracts;
    using Kernel;
    using log4net;

    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private readonly Configuration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly ILog _logger;
        private readonly Dictionary<string, IRobotService> _serviceCollection;
        private IBank _bank;
        private IEventBus _eventBus;
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public RobotController(Configuration config,
                               ILobbyStateManager lobbyStateManager,
                               IBank bank,
                               ILog logger,
                               IEventBus eventBus,
                               bool enabled)
        {
            _config = config;
            _lobbyStateManager = lobbyStateManager;
            _bank = bank;
            _logger = logger;
            _eventBus = eventBus;
            Enabled = enabled;
            _serviceCollection = new Dictionary<string, IRobotService>();
        }

        protected override void OnInitialize()
        {
            SuperRobotInitialization();
        }

        private void SuperRobotInitialization()
        {
            _serviceCollection.Add(typeof(BalanceCheck).ToString(), new BalanceCheck(_config, _lobbyStateManager, _bank, _logger, _eventBus));
        }

        protected override void OnRun()
        {
            foreach (var service in _serviceCollection)
            {
                service.Value.Execute();
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
