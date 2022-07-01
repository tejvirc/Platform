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

    public sealed class RobotController : BaseRunnable, IRobotController
    {
        private Configuration _config;
        private readonly Guid _overlayTextGuid = new Guid("2774B299-E8FE-436C-B68C-F6CF8DCDB31B");
        private readonly Timer _sanityChecker;
        private RobotLogger _logger;
        private Dictionary<string, HashSet<IRobotOperations>> _modeOperations;
        private System.Threading.Timer _coolDownTimer;
        private Automation _automator;
        private IEventBus _eventBus;
        private Container _container;
        private long _idleDuration;
        private bool _enabled;
        public long IdleDuration { get => _idleDuration; set { _idleDuration = value; } }
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
                        DisposeCoolDownTimer();
                    }
                    else
                    {
                        CoolDownTimerInitialization();
                        EnablingRobot();
                    }
                    _logger.Info($"RobotController is now [{_enabled}]", GetType().Name);
                }
            }
        }
        private void DisposeCoolDownTimer()
        {
            _coolDownTimer?.Dispose();
        }
        public RobotController()
        {
            _sanityChecker = new Timer()
            {
                Interval = 1000,
            };
            _sanityChecker.Elapsed += CheckSanity;
        }
        private void CoolDown(int milliseconds)
        {
            _automator.EnableExitToLobby(true);
            DisablingRobot("Cool Down");
            _eventBus.Publish(new CashoutRequestEvent());
            Task.Delay(milliseconds).ContinueWith(_ => EnablingRobot());
        }
        protected override void OnInitialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            WaitForServices();
            SubscribeToRobotEnabler();
        }
        protected override void OnRun()
        {
        }
        private void StartingSuperRobot()
        {
            _automator.ExitLockup();
            _config.SelectNextGame();
            _eventBus.Publish(new GameLoadRequestEvent());
            BalanceCheckWithDelay(1000);
        }
        private void EnablingRobot()
        {
            _sanityChecker.Start();
            _automator.SetOverlayText(_config.ActiveType.ToString(), false, _overlayTextGuid, InfoLocation.TopLeft);
            _automator.SetTimeLimitButtons(_config.GetTimeLimitButtons());
            //Todo: we need to Dispose the SuperRobot
            _automator.SetSpeed(_config.Speed);
            StartRobot();
            foreach (var op in _modeOperations[_config.ActiveType.ToString()])
            {
                op.Execute();
            }
            StartingSuperRobot();
        }
        private void CoolDownTimerInitialization()
        {
            var twoHours = 2 * 3600 * 1000;
            var tenMinutes = 10 * 60 * 1000;
            _coolDownTimer = new System.Threading.Timer((s) =>
            {
                CoolDown(tenMinutes);
            }, null, twoHours, twoHours);
        }
        private void BalanceCheckWithDelay(int milliseconds)
        {
            Task.Delay(milliseconds).ContinueWith(_ =>
                                                  {
                                                      _eventBus.Publish(new BalanceCheckEvent());
                                                  });
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
            _automator = _container.GetInstance<Automation>();
            _logger = _container.GetInstance<RobotLogger>();
            _config = _container.GetInstance<Configuration>();
            _modeOperations = Bootstrapper.InitializeModeDictionary(_container);
        }
        private void DisablingRobot(string reason = "")
        {
            _automator.SetOverlayText(reason, false, _overlayTextGuid, InfoLocation.TopLeft);
            _automator.ResetSpeed();
            _sanityChecker.Stop();
            foreach (var op in _modeOperations[_config.ActiveType.ToString()])
            {
                op.Halt();
            }
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
        private void SubscribeToRobotEnabler()
        {
            _eventBus.Subscribe<RobotControllerEnableEvent>(this, _ =>
            {
                Enabled = !Enabled;
            });

            _eventBus.Subscribe<ExitRequestedEvent>(this, _ =>
            {
                _logger.Info("Exit requested. Disabling.", GetType().Name);
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
                        _container = Bootstrapper.InitializeContainer();
                        _container.RegisterInstance(this);
                        SetupClassProperties();
                    }
                }
            }));
        }
        protected override void OnStop()
        {

        }
        private void IdleCheck()
        {
            if (_idleDuration > Constants.IdleTimeout)
            {
                _idleDuration = 0;
                _logger.Info("Idle for too long. Disabling.", GetType().Name);
                Enabled = false;
            }
        }
    }
}
