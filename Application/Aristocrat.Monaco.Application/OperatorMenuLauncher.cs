namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     A service that provides functionality to launch the operator menu
    ///     via a direct command or via monitoring a designated event.  The
    ///     operator menu implementation is loaded as an addin using Mono.Addins.
    /// </summary>
    public sealed class OperatorMenuLauncher : IService, IOperatorMenuLauncher, IDisposable
    {
        private const string OperatorMenuExtensionPath = "/Application/IOperatorMenu";
        private const string JackpotKeyOpensOperatorMenu = "JackpotKeyOpensOperatorMenu";
        private const int Timeout = 5;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static int OperatorKeySwitch1LogicalId = 33;
        public static int OperatorKeySwitch2LogicalId = 4;
        public static int JackpotKeyLogicalId = 130;

        private static int _blockExitCount;

        private readonly ConcurrentDictionary<Guid, DateTime> _keyEnablers = new ConcurrentDictionary<Guid, DateTime>();

        private readonly object _lockObject = new object();
        private readonly object _blockExitLock = new object();
        private readonly ConcurrentQueue<bool> _openCloseQueue = new ConcurrentQueue<bool>();

        private ManualResetEvent _exitAllowed = new ManualResetEvent(true);
        private bool _isHidden;
        private bool _disposed;

        private IOperatorMenu _operatorMenu;

        public OperatorMenuLauncher()
            : this(ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
            // NOTE: Disabled at startup until someone (the lobby) removes the key
            _keyEnablers.TryAdd(ApplicationConstants.OperatorMenuInitializationKey, DateTime.UtcNow);
        }

        public OperatorMenuLauncher(ISystemDisableManager disableManager)
        {
            _systemDisableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        private string DisableMessage => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OperatorMenuActive);

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            _operatorMenu.Close();

            // ReSharper disable once UseNullPropagation
            if (_exitAllowed != null)
            {
                lock (_blockExitLock)
                {
                    _exitAllowed.Dispose();
                    _exitAllowed = null;
                }
            }

            Hide();

            _disposed = true;
        }

        /// <inheritdoc />
        public bool IsShowing { get; private set; }

        /// <inheritdoc />
        public bool IsOperatorKeyDisabled => _keyEnablers.Count > 0;

        /// <inheritdoc />
        public void DisableKey(Guid enabler)
        {
            _keyEnablers.TryAdd(enabler, DateTime.UtcNow);
        }

        /// <inheritdoc />
        public void EnableKey(Guid enabler)
        {
            _keyEnablers.TryRemove(enabler, out _);
        }

        /// <inheritdoc />
        public void TurnOperatorKey()
        {
            Logger.Info("Operator key turned");

            Show();
        }

        /// <inheritdoc />
        public void Hide()
        {
            Logger.Info("Hiding Operator Menu");
            lock (_lockObject)
            {
                if (IsShowing)
                {
                    _operatorMenu.Hide();
                    _isHidden = true;
                }
            }
        }

        public void Close()
        {
            _openCloseQueue.Enqueue(false);
            Logger.Info($"Adding Close to Queue.  Queue depth: {_openCloseQueue.Count}");
            CheckQueue();
        }

        private void CloseInternal()
        {
            Logger.Info("Closing Operator Menu");

            if (IsShowing)
            {
                var bus = ServiceManager.GetInstance().TryGetService<IEventBus>();
                bus?.Publish(new OperatorMenuExitingEvent());
                IsShowing = false;

                // not sure the timeout here is right--if this event isn't signaled, something is likely really wrong.
                _exitAllowed?.WaitOne(TimeSpan.FromSeconds(Timeout));

                _operatorMenu.Close();

                _systemDisableManager.Enable(ApplicationConstants.OperatorMenuLauncherDisableGuid);

                var propertyManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var currentOperatorId = (string)propertyManager.GetProperty(ApplicationConstants.CurrentOperatorId, string.Empty);
                bus?.Publish(new OperatorMenuExitedEvent(currentOperatorId));
            }
        }

        public bool Show()
        {
            if (IsOperatorKeyDisabled)
            {
                Logger.Info($"Operator menu can't show because it's disabled by - {string.Join(", ", _keyEnablers)}");
                return false;
            }

            _openCloseQueue.Enqueue(true);
            Logger.Info($"Adding Open to Queue.  Queue depth: {_openCloseQueue.Count}");
            CheckQueue();

            return true;
        }

        private void ShowInternal()
        {
            Logger.Info("Showing Operator Menu");

            if (!IsShowing)
            {
                IsShowing = true;
                _isHidden = false;

                var serviceManager = ServiceManager.GetInstance();
                var access = serviceManager.GetService<IOperatorMenuAccess>();

                var role = access.TechnicianMode ? ApplicationConstants.TechnicianRole : ApplicationConstants.DefaultRole;
                var propertyManager = serviceManager.GetService<IPropertiesManager>();
                propertyManager.SetProperty(ApplicationConstants.RolePropertyKey, role);

                var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
                var meterName = role + "Access";
                if (meterManager.IsMeterProvided(meterName))
                {
                    meterManager.GetMeter(meterName).Increment(1);
                }

                _systemDisableManager.Disable(ApplicationConstants.OperatorMenuLauncherDisableGuid, SystemDisablePriority.Immediate, () => DisableMessage);

                _operatorMenu.Show();
            }
            else if (_isHidden)
            {
                // Show from Hide()
                _operatorMenu.Show();
                _isHidden = false;
            }
        }

        /// <inheritdoc />
        public void Activate()
        {
            lock (_lockObject)
            {
                if (IsShowing)
                {
                    _operatorMenu.Activate();
                }
            }
        }

        /// <inheritdoc />
        public string Name => "Operator Menu Launcher";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOperatorMenuLauncher) };

        private bool _inOverlayMenu;
        private readonly ISystemDisableManager _systemDisableManager;

        /// <inheritdoc />
        public void Initialize()
        {
            var bus = ServiceManager.GetInstance().TryGetService<IEventBus>();
            bus?.Subscribe<OnEvent>(this, HandleEvent);
            bus?.Subscribe<OverlayMenuEnteredEvent>(this, _ => _inOverlayMenu = true);
            bus?.Subscribe<OverlayMenuExitedEvent>(this, _ => _inOverlayMenu = false);

            var node = MonoAddinsHelper.GetSingleTypeExtensionNode(OperatorMenuExtensionPath);
            _operatorMenu = (IOperatorMenu)node.CreateInstance();
        }

        public void PreventExit()
        {
            lock (_blockExitLock)
            {
                Logger.Info("Preventing Exit");
                _blockExitCount++;
                if (_blockExitCount == 1)
                {
                    _exitAllowed?.Reset();
                }
            }
        }

        public void AllowExit()
        {
            lock (_blockExitLock)
            {
                Logger.Info("Allowing Exit");
                _blockExitCount--;

                if (_blockExitCount <= 0)
                {
                    _exitAllowed?.Set();
                    _blockExitCount = 0;
                }
            }
        }

        private void CheckQueue()
        {
            Task.Run(
                () =>
                {
                    Logger.Info("Checking Queue");
                    lock (_lockObject)
                    {
                        if (_openCloseQueue.TryDequeue(out bool open))
                        {
                            if (open)
                            {
                                ShowInternal();
                            }
                            else
                            {
                                CloseInternal();
                            }
                        }
                    }
                });
        }

        private void HandleEvent(OnEvent theEvent)
        {
            if (_inOverlayMenu
                && !_systemDisableManager.CurrentDisableKeys.Except(new[] { ApplicationConstants.OperatorMenuLauncherDisableGuid, ApplicationConstants.OperatorKeyNotRemovedDisableKey }).Any())
            {
                return;
            }

            var operatorMenuConfig = ServiceManager.GetInstance().TryGetService<IOperatorMenuConfiguration>();

            if (theEvent.LogicalId == OperatorKeySwitch1LogicalId || theEvent.LogicalId == OperatorKeySwitch2LogicalId)
            {
                TurnOperatorKey();
            }
            else if (theEvent.LogicalId == JackpotKeyLogicalId)
            {
                if (operatorMenuConfig?.GetSetting(JackpotKeyOpensOperatorMenu, false) ?? false)
                {
                    Logger.Info("Treating Jackpot Key turn as Operator Menu Key turn");
                    TurnOperatorKey();
                }
            }
        }
    }
}
