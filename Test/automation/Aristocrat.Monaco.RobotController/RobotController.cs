namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using Contracts;
    using Kernel;
    using SimpleInjector;

    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private readonly Guid _overlayTextGuid = new Guid("2774B299-E8FE-436C-B68C-F6CF8DCDB31B");
        private readonly System.Timers.Timer _sanityChecker;
        private readonly ThreadSafeHashSet<RobotStateAndOperations> _inProgressRequests;
        private IPropertiesManager _propertiesManager;
        private IGameProvider _gameProvider;
        private StateChecker _stateChecker;
        private RobotLogger _logger;
        private Dictionary<string, HashSet<IRobotOperations>> _modeOperations;
        private Dictionary<string, IList<Action>> _executionActions;
        private Dictionary<string, IList<Action>> _warmUpActions;
        private Automation _automator;
        private IEventBus _eventBus;
        private Container _container;
        private long _idleDuration;
        private string _configPath;
        private bool _enabled;

        public ThreadSafeHashSet<RobotStateAndOperations> InProgressRequests
        {
            get => _inProgressRequests;
        }

        public long IdleDuration
        {
            get => _idleDuration;
            set
            {
                _idleDuration = value;
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (!value)
                    {
                        PrintCurrentlyInProgressRequests();
                        DisablingRobot();
                    }
                    else
                    {
                        EnablingRobot();
                    }
                    _logger.Info($"RobotController Enable is now = [{_enabled}], Game = {Config.CurrentGame}, IdleDuration = {_idleDuration}", GetType().Name);
                }
            }
        }

        public RobotController()
        {
            _inProgressRequests = new ThreadSafeHashSet<RobotStateAndOperations>();
            _sanityChecker = new System.Timers.Timer()
            {
                Interval = 1000,
            };
            _sanityChecker.Elapsed += CheckSanity;

            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _container = new Container();
        }

        ~RobotController() => Dispose(false);

        public Configuration Config { get; private set; }

        protected override void OnInitialize()
        {
            AddServices();
            SubscribeToRobotEnabler();
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                Config = null;
                if (_sanityChecker is not null)
                {
                    _sanityChecker.Dispose();
                }
                _propertiesManager = null;
                _gameProvider = null;
                _stateChecker = null;
                _logger = null;
                if (_modeOperations is not null)
                {
                    foreach (var operations in _modeOperations)
                    {
                        foreach (var opertaion in operations.Value)
                        {
                            opertaion.Dispose();
                        }
                    }
                    _modeOperations.Clear();
                }
                if (_executionActions is not null)
                {
                    _executionActions.Clear();
                }
                if (_warmUpActions is not null)
                {
                    _warmUpActions.Clear();
                }
                _automator = null;
                _eventBus = null;

                if (_container is not null)
                {
                    _container.Dispose();
                }

                _container = null;
            }

            Disposed = true;
        }

        protected override void OnRun()
        {
        }

        protected override void OnStop()
        {

        }

        private void CoolDown(int milliseconds)
        {
            _logger.Info($"RobotController Is Cooling Down", GetType().Name);
            DisablingRobot($"Cooling Down for {milliseconds}");
            Task.Delay(Constants.CashOutDelayDuration).ContinueWith(_ => _eventBus.Publish(new CashOutButtonPressedEvent()));
            Task.Delay(milliseconds).ContinueWith(_ => EnablingRobot());
        }

        private void EnablingRobot()
        {
            _logger.Info($"RobotController Is Enabling", GetType().Name);
            RefreshRobotConfiguration();
            WarmUpRobot();
            ActivateRobotOperations();
            StartRobot();
        }

        private void ActivateRobotOperations()
        {
            foreach (var op in _modeOperations[Config.ActiveType.ToString()])
            {
                op.Reset();
                op.Execute();
            }
        }

        private void WarmUpRobot()
        {
            _logger.Info($"RobotController Is Warming Up", GetType().Name);
            SetMaxWinLimit();
            _sanityChecker.Start();
            _automator.SetOverlayText(Config.ActiveType.ToString(), false, _overlayTextGuid, InfoLocation.TopLeft);
            _automator.SetTimeLimitButtons(Config.GetTimeLimitButtons());
            foreach (var action in _warmUpActions[Config.ActiveType.ToString()])
            {
                action();
            }
        }

        private void RefreshRobotConfiguration()
        {
            _logger.Info($"RefreshRobotConfiguration Is Initiated", GetType().Name);
            Config = RobotControllerHelper.LoadConfiguration(_configPath);
            _modeOperations = RobotControllerHelper.InitializeModeDictionary(_container);

            _warmUpActions = new Dictionary<string, IList<Action>>()
            {
                { nameof(ModeType.Regular) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            InProgressRequests.TryAdd(RobotStateAndOperations.RegularMode);
                            SetCurrentlyActiveGameIfAny();
                        }
                    }
                },
                { nameof(ModeType.Super) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            InProgressRequests.TryAdd(RobotStateAndOperations.SuperMode);
                            SetCurrentlyActiveGameIfAny();
                        }
                    }
                },
                { nameof(ModeType.Uber) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            InProgressRequests.TryAdd(RobotStateAndOperations.UberMode);
                            SetCurrentlyActiveGameIfAny();
                        }
                    }
                }
            };

            _executionActions = new Dictionary<string, IList<Action>>()
            {
                { nameof(ModeType.Regular) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            if(!_stateChecker.IsGame)
                            {
                                Enabled = false;
                                return;
                            }
                            _eventBus.Publish(new BalanceCheckEvent());
                        }
                    }
                },
                { nameof(ModeType.Super) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            _automator.ExitLockup();
                            _eventBus.Publish(new GameLoadRequestEvent());
                        }
                    }
                },
                { nameof(ModeType.Uber) ,
                    new List<Action>()
                    {
                        () =>
                        {
                            _automator.ExitLockup();
                            _eventBus.Publish(new GameLoadRequestEvent());
                        }
                    }
                }
            };
        }

        private void StartRobot()
        {
            foreach (var action in _executionActions[Config.ActiveType.ToString()])
            {
                action();
            }
        }

        private void SetMaxWinLimit()
        {
            if (Config.Active.MaxWinLimitOverrideMilliCents > 0)
            {
                _logger.Info($"{nameof(SetMaxWinLimit)} Is Initiated", GetType().Name);
                _automator.SetMaxWinLimit(Config.Active.MaxWinLimitOverrideMilliCents);
            }
        }

        private bool SetCurrentlyActiveGameIfAny()
        {
            if (_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0) == 0)
            {
                return false;
            }

            _logger.Info($"{nameof(SetCurrentlyActiveGameIfAny)} Is Initiated", GetType().Name);
            var currentGame = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));
            Config.SetCurrentActiveGame(currentGame.ThemeName);
            return true;
        }

        private void DisablingRobot(string reason = "")
        {
            _automator.SetOverlayText(reason, false, _overlayTextGuid, InfoLocation.TopLeft);
            _sanityChecker.Stop();

            foreach (var op in _modeOperations[Config.ActiveType.ToString()])
            {
                op.Halt();
            }

            InProgressRequests.Clear();
            _modeOperations.Clear();
            _executionActions.Clear();
            _warmUpActions.Clear();
        }

        private void CheckSanity(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _idleDuration += 1000;
                IdleCheck();
            }
            catch (OverflowException)
            {
                _idleDuration = 0;
            }
        }

        private void SubscribeToRobotEnabler()
        {
            _eventBus.Subscribe<RobotControllerEnableEvent>(this, _ =>
            {
                _logger.Info("Exit requested Manually. Disabling.", GetType().Name);
                Enabled = !Enabled;
            });

            _eventBus.Subscribe<ExitRequestedEvent>(this, _ =>
            {
                _logger.Info("Exit requested. Disabling.", GetType().Name);
                Enabled = false;
            });
        }

        private void AddServices()
        {
            Task.Run(() =>
            {
                using var serviceWaiter = new ServiceWaiter(_eventBus);

                serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
                serviceWaiter.AddServiceToWaitFor<IGameProvider>();
                serviceWaiter.AddServiceToWaitFor<IContainerService>();
                serviceWaiter.AddServiceToWaitFor<IBank>();
                serviceWaiter.AddServiceToWaitFor<IPathMapper>();
                serviceWaiter.AddServiceToWaitFor<IGameService>();

                if (serviceWaiter.WaitForServices())
                {
                    ServicesUtilities.RegisterControllerServices(_container, this);
                    InitializeController();
                }
            });
        }

        private void IdleCheck()
        {
            if (_idleDuration > Constants.IdleTimeout)
            {
                //FreeGames can prevent changing the game states for up to 20+ minutes.
                if (_stateChecker.IsPrimaryGameStarted)
                {
                    _idleDuration = 0;
                    return;
                }
                _idleDuration = 0;
                _logger.Info("Idle for too long. Disabling.", GetType().Name);
                Enabled = false;
            }
        }

        private void PrintCurrentlyInProgressRequests()
        {
            var req = string.Join(", ", _inProgressRequests);
            _logger.Info($"InProgressRequests : {req}", GetType().Name);
        }

        private void InitializeController()
        {
            _configPath = Path.Combine(_container.GetInstance<IPathMapper>().GetDirectory(HardwareConstants.DataPath).FullName, Constants.ConfigurationFileName);
            _gameProvider = _container.GetInstance<IGameProvider>();
            _stateChecker = _container.GetInstance<StateChecker>();
            _propertiesManager = _container.GetInstance<IPropertiesManager>();
            _automator = _container.GetInstance<Automation>();
            _logger = _container.GetInstance<RobotLogger>();
        }
    }
}
