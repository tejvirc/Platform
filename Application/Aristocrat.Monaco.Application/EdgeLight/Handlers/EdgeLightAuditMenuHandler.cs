namespace Aristocrat.Monaco.Application.EdgeLight.Handlers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.EdgeLight;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Monitors for the Audit Menu enter and exit status.
    /// </summary>
    public class EdgeLightAuditMenuHandler : IDisposable, IEdgeLightHandler
    {
        // ReSharper disable once PossibleNullReferenceException
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEdgeLightingStateManager _edgeLightingStateManager;

        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private bool _disposed;
        private IEdgeLightToken _edgeLightStateToken;

        public EdgeLightAuditMenuHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IEdgeLightingStateManager>()
            )
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EdgeLightAuditMenuHandler" /> class.</summary>
        public EdgeLightAuditMenuHandler(
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager,
            IEdgeLightingStateManager edgeLightingStateManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _edgeLightingStateManager = edgeLightingStateManager ??
                                        throw new ArgumentNullException(nameof(edgeLightingStateManager));
            Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => nameof(EdgeLightAuditMenuHandler);

        public bool Enabled => true;

        /// <summary>Releases allocated resources.</summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; False to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Initialize()
        {
            Logger.Debug(Name + " Initialize()");
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => OnOperatorModeEnter());
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, _ => OnOperatorModeExit());
            var inOperatorMode =
                _systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.OperatorMenuLauncherDisableGuid);
            if (inOperatorMode)
            {
                OnOperatorModeEnter();
            }
        }

        private void OnOperatorModeEnter()
        {
            if (_edgeLightStateToken != null)
            {
                return;
            }

            Logger.Debug("UpdateEdgeLight: Enter operator mode.");
            _edgeLightStateToken = _edgeLightingStateManager.SetState(EdgeLightState.OperatorMode);
        }

        private void OnOperatorModeExit()
        {
            if (_edgeLightStateToken == null)
            {
                return;
            }

            Logger.Debug("UpdateEdgeLight: exit operator mode.");
            _edgeLightingStateManager.ClearState(_edgeLightStateToken);
            _edgeLightStateToken = null;
        }
    }
}