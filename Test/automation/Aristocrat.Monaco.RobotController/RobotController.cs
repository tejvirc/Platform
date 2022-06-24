namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using Contracts;
    using Kernel;
    using log4net;

    internal struct RobotInfo
    {
        public Configuration Config;
        public Automation Automator;
        public ILog Logger;
        public IEventBus EventBus;
        public IPropertiesManager PropertiesManager;
        public ILobbyStateManager LobbyStateManager;
        public IGamePlayState GamePlayState;
        public IContainerService ContainerService;
    }
    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private Configuration _config = new Configuration();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, IRobotOperations> _operationCollection;
        private readonly Guid _overlayTextGuid = new Guid("2774B299-E8FE-436C-B68C-F6CF8DCDB31B");
        private Automation _automator;
        private IEventBus _eventBus;
        private IPropertiesManager _propertyManager;
        private ILobbyStateManager _lobbyStateManager;
        private IGamePlayState _gamePlayState;
        private IContainerService _containerService;
        private long _idleDuration;
        private bool _enabled;
        public bool Enabled
        {
            get{ return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (!value)
                    {
                        DisablingRobot();
                    }
                    else
                    {
                        EnablingRobot();
                        StartingSuperRobot();
                    }
                    _logger.Info($"RobotController is now [{_enabled}]");
                }
            }
        }

        private void StartingSuperRobot()
        {
            _eventBus.Publish(new LoadGameEvent());
        }

        private void EnablingRobot()
        {
            SuperRobotInitialization();
            SubscribeToEvents();
            _automator.SetOverlayText(_config.ActiveType.ToString(), false, _overlayTextGuid, InfoLocation.TopLeft);
            _automator.SetTimeLimitButtons(_config.GetTimeLimitButtons());
            //Todo: we need to Dispose the SuperRobot
            //_automator.SetSpeed(_config.Speed);
            foreach (var service in _operationCollection)
            {
                service.Value.Execute();
            }
            StartSuperRobot();
        }

        private void SubscribeToEvents()
        {
            //subscribe events her
        }

        private void StartSuperRobot()
        {
            //_eventBus.Publish(new LoadGameEvent());
        }

        private void SetupClassProperties()
        {
            _propertyManager = _containerService.Container.GetInstance<IPropertiesManager>();
            _lobbyStateManager = _containerService.Container.GetInstance<ILobbyStateManager>();
            _automator = _automator ?? new Automation(_propertyManager, _eventBus);
        }

        private void DisablingRobot()
        {
            _automator.SetOverlayText("", true, _overlayTextGuid, InfoLocation.TopLeft);
            //_automator.ResetSpeed();
            foreach (var service in _operationCollection)
            {
                service.Value.Halt();
            }
            _operationCollection.Clear();
            UnsubscribeToEvents();
        }

        private void UnsubscribeToEvents()
        {
            //unsubscribe events her
            //_eventBus.Unsubscribe<type>(this);
        }

        public RobotController()
        {
            _operationCollection = new Dictionary<string, IRobotOperations>();
        }

        protected override void OnInitialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            WaitForServices();
            SubscribeToRobotEnabler();
        }

        private void SubscribeToRobotEnabler()
        {
            _eventBus.Subscribe<RobotControllerEnableEvent>(this, _ =>
            {
                Enabled = !Enabled;
            });
        }

        private void WaitForServices()
        {
            Task.Run((Action)(() =>
            {
                using (var serviceWaiter = new ServiceWaiter(_eventBus))
                {
                    serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
                    serviceWaiter.AddServiceToWaitFor<IContainerService>();
                    if (serviceWaiter.WaitForServices())
                    {
                        _gamePlayState = ServiceManager.GetInstance().GetService<IGamePlayState>();
                        _containerService = ServiceManager.GetInstance().GetService<IContainerService>();
                        SetupClassProperties();
                    }
                }
            }));
        }

        private void LoadConfiguration()
        {
            var configPath = Path.Combine(
                ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(HardwareConstants.DataPath).FullName,
                Constants.ConfigurationFileName);

            _logger.Info("Loading configuration: " + configPath);

            _config = Configuration.Load(configPath);

            if (_config != null)
            {                
                _logger.Info(_config.ToString());
            }
        }


        private void SuperRobotInitialization()
        {
            var robotInfo = new RobotInfo
            {
                Config = _config,
                Automator = _automator,
                ContainerService = _containerService,
                EventBus = _eventBus,
                GamePlayState = _gamePlayState,
                LobbyStateManager = _lobbyStateManager,
                Logger = _logger,
                PropertiesManager = _propertyManager
            };
            //_serviceCollection.Add(typeof(BalanceCheck).ToString(), new BalanceCheck(_config, _lobbyStateManager, _gamePlayState, _bank, _logger, _eventBus));
            //_serviceCollection.Add(typeof(ActionTouch).ToString(), new ActionTouch(_config, _lobbyStateManager, _logger, _automator, _eventBus));
            //_serviceCollection.Add(typeof(ActionPlayer).ToString(), new ActionPlayer(_config, _lobbyStateManager, _logger, _automator, _eventBus));
            _operationCollection.Add(typeof(LoadGame).ToString(), new LoadGame(robotInfo));
        }

        protected override void OnRun()
        {
            LoadConfiguration();
        }

        protected override void OnStop()
        {
            
        }
//Todo:
        private void IdleCheck()
        {
            if (_idleDuration > Constants.IdleTimeout)
            {
                _idleDuration = 0;
                _logger.Info("Idle for too long. Disabling.");
                Enabled = false;
            }
        }
    }
}
