namespace Aristocrat.Monaco.Application.CoinAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Hardware.Contracts;
    using Hardware.Contracts.CoinAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A component that provides the coordination for the switch between the disabling and enabling of the CoinAcceptor.
    /// </summary>
    public sealed class CoinAcceptorCoordinator : IService, IDisposable
    {
        private const string AddinCoinAcceptorDisableExtensionPoint = "/Application/CoinAcceptorDisable";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<CoinAcceptorDisableNode> _configuredDisableNodes = new List<CoinAcceptorDisableNode>();
        private readonly List<CoinAcceptorDisableNode> _pendingEnableRequests = new List<CoinAcceptorDisableNode>();
        private readonly object _locker = new object();
        private ICoinAcceptor _device;
        private bool _disposed;
        private IPropertiesManager _propertiesManager;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(CoinAcceptorCoordinator) };

        public void Initialize()
        {
            Logger.Info($"Initializing {nameof(CoinAcceptorCoordinator)} ...");

            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            // Saves all disable and enable events for checking whether any duplicate event types are defined
            var disableAndEnableEvents = new HashSet<Type>();

            // Gather all configured disable nodes and subscribe the disable and enable events to the Event Bus.
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            foreach (var disableNode in MonoAddinsHelper.GetSelectedNodes<CoinAcceptorDisableNode>(AddinCoinAcceptorDisableExtensionPoint))
            {
                var eventsToSubscribe = new List<Type>();
                string logMessage;

                if (disableNode.EventType == null)
                {
                    logMessage = $"No valid disable event type defined in AddinId {disableNode.Addin.Id}";
                    Logger.Fatal(logMessage);
                    throw new CoinAcceptorCoordinatingException(logMessage);
                }

                if (disableAndEnableEvents.Contains(disableNode.EventType))
                {
                    logMessage =
                        $"The disable event type {disableNode.EventType} defined in AddinId {disableNode.Addin.Id} has been used before";
                    Logger.Fatal(logMessage);
                    throw new CoinAcceptorCoordinatingException(logMessage);
                }

                disableAndEnableEvents.Add(disableNode.EventType);
                eventsToSubscribe.Add(disableNode.EventType);

                if (disableNode.EnableNodes.Count == 0)
                {
                    logMessage = $"There is no enable nodes defined in the disable event {disableNode.EventType}.";
                    Logger.Fatal(logMessage);
                    throw new CoinAcceptorCoordinatingException(logMessage);
                }

                foreach (var enableType in disableNode.EnableNodes)
                {
                    disableAndEnableEvents.Add(enableType);
                    eventsToSubscribe.Add(enableType);
                }

                foreach (var eventType in eventsToSubscribe)
                {
                    eventBus.Subscribe(this, eventType, ReceiveEvent);
                }

                _configuredDisableNodes.Add(disableNode);
            }
        }

        private void DisableCoinAcceptor(CoinAcceptorDisableNode disableNode)
        {
            lock (_locker)
            {
                if (_device == null)
                {
                    _device = ServiceManager.GetInstance().TryGetService<ICoinAcceptor>();
                }

                if (_device != null)
                {
                    Logger.Info($"Disabling the Coin acceptor: {disableNode.Reason}");

                    // Is the coin acceptor service enabled?
                    if (_device.Enabled)
                    {
                        // Yes, disable for this disabled reason.
                        _device.Disable(disableNode.DisabledReason);
                    }
                    else
                    {
                        // No, coin acceptor service is already disabled.  Are we disabling for some reason other than error and the coin acceptor service is not already disabled for this reason?
                        // *NOTE* This prevents duplicate system/back-end disables from being logged.
                        if (disableNode.DisabledReason != DisabledReasons.Error &&
                                ((_device.ReasonDisabled & disableNode.DisabledReason) <= 0))
                        {
                            // Yes, disable for this reason.
                            _device.Disable(disableNode.DisabledReason);
                        }
                    }
                }

                _pendingEnableRequests.Add(disableNode);
            }
        }

        private void EnableCoinAcceptor(CoinAcceptorDisableNode disableNode)
        {
            lock (_locker)
            {
                // Remove pending enable request for this disable node.
                _pendingEnableRequests.Remove(disableNode);

                var remaining = _pendingEnableRequests.Count(r => r.DisabledReason == disableNode.DisabledReason);
                if (remaining == 0)
                {
                    var device = ServiceManager.GetInstance().TryGetService<ICoinAcceptor>();
                    if (device == null)
                    {
                        Logger.Warn("Cannot enable Coin acceptor because Coin acceptor service not avaiable.");
                    }
                    else
                    {
                        Logger.Info($"Enabling the Coin acceptor - {disableNode.EnabledReason}");
                        device.Enable(disableNode.EnabledReason);
                    }
                }
                else
                {
                    Logger.Warn(
                        $"Cannot enable Coin acceptor because {remaining} pending enable requests, {disableNode.DisabledReason}");
                }
            }
        }

        private void ReceiveEvent(IEvent data)
        {
            if (_propertiesManager.GetValue(HardwareConstants.CoinAcceptorDiagnosticMode, false))
            {
                return;
            }
            var receivedEventType = data.GetType();

            Logger.Debug($"Event received: {receivedEventType}");

            // Check if it is a disable request
            var disableNode = _configuredDisableNodes.Find(disableItem => disableItem.EventType == receivedEventType);
            if (disableNode != null)
            {
                var pendingNode =
                    _pendingEnableRequests.Find(disableItem => disableItem.EventType == receivedEventType);

                if (pendingNode == null)
                {
                    Logger.Info($"Disable Event received: {receivedEventType} reason: {disableNode.Reason}");
                    DisableCoinAcceptor(disableNode);
                }
                else
                {
                    Logger.Warn(
                        $"Disable Event type already received: {receivedEventType} reason: {pendingNode.Reason}");
                }

                return;
            }

            // Check if it is an enable request
            var disableNodesToEnable = _pendingEnableRequests.FindAll(
                disableItem => disableItem.EnableNodes.Contains(receivedEventType));
            if (disableNodesToEnable.Count > 0)
            {
                foreach (var node in disableNodesToEnable)
                {
                    Logger.Info($"Enable Event received: {receivedEventType}, disable reason: {node.Reason}");
                    EnableCoinAcceptor(node);
                }
            }
            else if (receivedEventType == typeof(SystemEnabledEvent))
            {
                Logger.Debug(
                    $"Enable Event received: {receivedEventType} with disable nodes count {disableNodesToEnable.Count}");

                var device = ServiceManager.GetInstance().TryGetService<ICoinAcceptor>();
                if (device != null)
                {
                    // *NOTE* This will enable the coin acceptor if it was disabled by the system but there is no disable node.  This may be
                    // the case if an initial system enable (IE. from the game at boot) was initially ignored because of some other error and
                    // then the error is cleared.
                    if (!device.Enabled && device.ReasonDisabled.HasFlag(DisabledReasons.System))
                    {
                        // Yes, enable the coin acceptor for reason System.
                        Logger.Info("Enabling the coin acceptor");
                        device.Enable(EnabledReasons.System);
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
            else
            {
                // do nothing
            }
        }
    }
}
