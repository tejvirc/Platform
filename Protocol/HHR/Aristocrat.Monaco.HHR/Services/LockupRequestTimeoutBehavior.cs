namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Client.WorkFlow;
    using Events;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Request timeout behavior which requires a lockup to be created when a Request timeout.
    /// </summary>
    public class LockupRequestTimeoutBehavior : IRequestTimeoutBehavior<LockupRequestTimeout>
    {
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private IEventBus _eventBus;

        public LockupRequestTimeoutBehavior()
        {
            _systemDisableManager = ServiceManager.GetInstance().TryGetService<ISystemDisableManager>() ??
                                    throw new ArgumentNullException(nameof(ISystemDisableManager));
            _eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>() ??
                        throw new ArgumentNullException(nameof(IEventBus));
        }

        /// <inheritdoc />
        public void OnEntry(LockupRequestTimeout requestTimeout)
        {
            _logger.Debug($"Disabling system as request timed out.");
            if (!_systemDisableManager.CurrentDisableKeys.Contains(requestTimeout.LockupKey))
            {
                _eventBus.Publish(new UnexpectedOrNoResponseEvent(requestTimeout));
            }
            _systemDisableManager.Disable(
                requestTimeout.LockupKey,
                SystemDisablePriority.Normal,
                () => requestTimeout.LockupString,
                true,
                () => requestTimeout.LockupHelpText);
        }

        /// <inheritdoc />
        public void OnExit(LockupRequestTimeout requestTimeout)
        {
            _logger.Debug($"Enabling system as request sent to server.");
            _systemDisableManager.Enable(requestTimeout.LockupKey);
        }
    }
}