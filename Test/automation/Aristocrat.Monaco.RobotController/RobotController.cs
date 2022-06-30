namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using Contracts;
    using Kernel;
    using log4net;
    using System.Timers;
    using SimpleInjector;
    using Aristocrat.Monaco.Kernel.Contracts;

    internal struct RobotInfo
    {
        public Configuration Config;
        public Automation Automator;
        public ILog Logger;
        public IEventBus EventBus;
        public IPropertiesManager PropertiesManager;
        public IContainerService ContainerService;
        public StateChecker StateChecker;
        public Action IdleDurationReset;
        public Action DisableRobotController;
        public Func<long> IdleDuration;
    }
    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private Configuration _config = new Configuration();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly HashSet<IRobotOperations> _operationCollection;
        private readonly Guid _overlayTextGuid = new Guid("2774B299-E8FE-436C-B68C-F6CF8DCDB31B");
        private readonly Timer _sanityChecker;
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
            get { return _enabled; }
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
        public RobotController()
        {
            _operationCollection = new HashSet<IRobotOperations>();
            _sanityChecker = new Timer()
            {
                Interval = 1000,
            };
            _sanityChecker.Elapsed += CheckSanity;
        }
        protected override void OnInitialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
           // var container = new Container();
            //RegisterExternalServices(container);
            WaitForServices();
            SubscribeToRobotEnabler();
        }
        protected override void OnRun()
        {
            LoadConfiguration();
        }
        private void StartingSuperRobot()
        {
            _automator.ExitLockup();
            _eventBus.Publish(new GameLoadRequestEvent());
        }

        private void EnablingRobot()
        {
            SuperRobotInitialization();
            SubscribeToEvents();
            _sanityChecker.Start();
            _automator.SetOverlayText(_config.ActiveType.ToString(), false, _overlayTextGuid, InfoLocation.TopLeft);
            _automator.SetTimeLimitButtons(_config.GetTimeLimitButtons());
            //Todo: we need to Dispose the SuperRobot
            _automator.SetSpeed(_config.Speed);
            StartRobot();
            foreach (var op in _operationCollection)
            {
                op.Execute();
            }
        }

        private void SubscribeToEvents()
        {
            GameProcessHungEvent();
            VoucherRejectedEvent();
        }

        private void VoucherRejectedEvent()
        {
            
        }

        private void GameProcessHungEvent()
        {
            // If the runtime process hangs, and the setting to not kill it is active, then stop the robot. 
            // This will allow someone to attach a debugger to investigate.
            var doNotKillRuntime = _propertyManager.GetValue("doNotKillRuntime", Common.Constants.False).ToUpperInvariant();
            if (doNotKillRuntime == Common.Constants.True)
            {
                _eventBus.Subscribe<GameProcessHungEvent>(this, _ => { Enabled = false; });
            }
        }

        private void StartRobot()
        {
            SetMaxWinLimit();
        }

        private void SetMaxWinLimit()
        {
            if (_config.Active.MaxWinLimitOverrideMilliCents > 0)
            {
                _automator.SetMaxWinLimit(_config.Active.MaxWinLimitOverrideMilliCents);
            }
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
            _automator.ResetSpeed();
            _sanityChecker.Stop();
            foreach (var op in _operationCollection)
            {
                op.Halt();
            }
            _operationCollection.Clear();
            UnsubscribeToEvents();
        }

        private void UnsubscribeToEvents()
        {
            //unsubscribe events her
            //_eventBus.Unsubscribe<type>(this);
        }

       

        private void CheckSanity(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _idleDuration = _idleDuration + 1000;
                IdleCheck();
            }
            catch (OverflowException)
            {
                _idleDuration = 0;
            }
        }

        

        private void RegisterExternalServices(Container container)
        {
            container.Register<GameOperations>(Lifestyle.Singleton);
        }

        private void SubscribeToRobotEnabler()
        {
            _eventBus.Subscribe<RobotControllerEnableEvent>(this, _ =>
            {
                Enabled = !Enabled;
            });

            _eventBus.Subscribe<ExitRequestedEvent>(this, _ =>
            {
                _logger.Info("Exit requested. Disabling.");
                Enabled = false;
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
                StateChecker = new StateChecker(_lobbyStateManager, _gamePlayState),
                Logger = _logger,
                PropertiesManager = _propertyManager,
                IdleDurationReset = IdleDurationReset,
                DisableRobotController = DisableRobotController,
                IdleDuration = IdleDuration
            };
            _operationCollection.Add(GameOperations.Instantiate(robotInfo));
            _operationCollection.Add(BalanceOperations.Instantiate(robotInfo));
            _operationCollection.Add(TouchOperation.Instantiate(robotInfo));
            _operationCollection.Add(PlayerOperations.Instantiate(robotInfo));
            _operationCollection.Add(CashoutOperations.Instantiate(robotInfo));
            _operationCollection.Add(LobbyOperations.Instantiate(robotInfo));
            _operationCollection.Add(AuditMenuOperations.Instantiate(robotInfo));
            _operationCollection.Add(new LockUpOperations(robotInfo));
            _operationCollection.Add(new OperatingHoursOperations(robotInfo));
        }

        private long IdleDuration()
        {
            return _idleDuration;
        }

        private void DisableRobotController()
        {
            Enabled = false;
        }

        private void IdleDurationReset()
        {
            _idleDuration = 0;
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
