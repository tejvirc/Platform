namespace Aristocrat.Monaco.Application.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A component that provides the coordination for the switch between the disabling and enabling of the NoteAcceptor.
    /// </summary>
    public sealed class NoteAcceptorCoordinator : IService, IDisposable
    {
        private const string AddinNoteAcceptorDisableExtensionPoint = "/Application/NoteAcceptorDisable";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<NoteAcceptorDisableNode> _configuredDisableNodes = new List<NoteAcceptorDisableNode>();
        private readonly List<NoteAcceptorDisableNode> _pendingEnableRequests = new List<NoteAcceptorDisableNode>();
        private readonly object _locker = new object();
        private INoteAcceptor _device;
        private bool _disposed;

        /// <summary>
        ///     Gets the disable nodes waiting for an enable.
        ///     This property is for tracing purpose.
        /// </summary>
        [CLSCompliant(false)]
        public ICollection<NoteAcceptorDisableNode> PendingEnableRequests
        {
            get
            {
                List<NoteAcceptorDisableNode> clone;
                lock (_locker)
                {
                    clone = new List<NoteAcceptorDisableNode>(_pendingEnableRequests);
                }

                return clone;
            }
        }

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
        public ICollection<Type> ServiceTypes => new[] { typeof(NoteAcceptorCoordinator) };

        public void Initialize()
        {
            Logger.Info("Initializing NoteAcceptorCoordinator...");

            // Saves all disable and enable events for checking whether any duplicate event types are defined
            var disableAndEnableEvents = new HashSet<Type>();

            // Gather all configured disable nodes and subscribe the disable and enable events to the Event Bus.
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            foreach (var disableNode in MonoAddinsHelper.GetSelectedNodes<NoteAcceptorDisableNode>(AddinNoteAcceptorDisableExtensionPoint))
            {
                var eventsToSubscribe = new List<Type>();
                string logMessage;

                if (disableNode.EventType == null)
                {
                    logMessage = $"No valid disable event type defined in AddinId {disableNode.Addin.Id}";
                    Logger.Fatal(logMessage);
                    throw new NoteAcceptorCoordinatingException(logMessage);
                }

                if (disableAndEnableEvents.Contains(disableNode.EventType))
                {
                    logMessage =
                        $"The disable event type {disableNode.EventType} defined in AddinId {disableNode.Addin.Id} has been used before";
                    Logger.Fatal(logMessage);
                    throw new NoteAcceptorCoordinatingException(logMessage);
                }

                disableAndEnableEvents.Add(disableNode.EventType);
                eventsToSubscribe.Add(disableNode.EventType);

                if (disableNode.EnableNodes.Count == 0)
                {
                    logMessage = $"There is no enable nodes defined in the disable event {disableNode.EventType}.";
                    Logger.Fatal(logMessage);
                    throw new NoteAcceptorCoordinatingException(logMessage);
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

        private void DisableNoteAcceptor(NoteAcceptorDisableNode disableNode)
        {
            lock (_locker)
            {
                if (_device == null)
                {
                    _device = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                }
                
                if (_device != null)
                {
                    Logger.Info($"Disabling the note acceptor: {disableNode.Reason}");

                    // Is the note service enabled?
                    if (_device.Enabled)
                    {
                        // Yes, disable for this disabled reason.
                        _device.Disable(disableNode.DisabledReason);
                    }
                    else
                    {
                        // No, note acceptor service is already disabled.  Are we disabling for some reason other than error and the note acceptor service is not already disabled for this reason?
                        // *NOTE* This prevents duplicate system/back-end disables from being logged.
                        if (disableNode.DisabledReason != DisabledReasons.Error &&
                            (_device.ReasonDisabled & disableNode.DisabledReason) > 0 == false)
                        {
                            // Yes, disable for this reason.
                            _device.Disable(disableNode.DisabledReason);
                        }
                    }
                }

                _pendingEnableRequests.Add(disableNode);
            }
        }

        private void EnableNoteAcceptor(NoteAcceptorDisableNode disableNode)
        {
            lock (_locker)
            {
                // Remove pending enable request for this disable node.
                _pendingEnableRequests.Remove(disableNode);

                var remaining = _pendingEnableRequests.Count(r => r.DisabledReason == disableNode.DisabledReason);
                if (remaining == 0)
                {
                    var device = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                    if (device == null)
                    {
                        Logger.Warn("Cannot enable note acceptor because note acceptor service not avaiable.");
                    }
                    else
                    {
                        Logger.Info($"Enabling the note acceptor - {disableNode.DisabledReason}");
                        device.Enable(disableNode.EnabledReason);
                    }
                }
                else
                {
                    Logger.Warn(
                        $"Cannot enable note acceptor because {remaining} pending enable requests, {disableNode.DisabledReason}");
                }

                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus.Publish(new PendingEnableRequestRemovedEvent());
            }
        }

        private void ReceiveEvent(IEvent data)
        {
            if (data is DoorBaseEvent door && door.LogicalId != (int)DoorLogicalId.CashBox)
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
                    DisableNoteAcceptor(disableNode);
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
                    EnableNoteAcceptor(node);
                }
            }
            else if (receivedEventType == typeof(SystemEnabledEvent))
            {
                Logger.Debug(
                    $"Enable Event received: {receivedEventType} with disable nodes count {disableNodesToEnable.Count}");

                var device = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                if (device != null)
                {
                    // *NOTE* This will enable the note acceptor if it was disabled by the system but there is no disable node.  This may be
                    // the case if an initial system enable (IE. from the game at boot) was initially ignored because of some other error and
                    // then the error is cleared.
                    if (!device.Enabled && device.ReasonDisabled.HasFlag(DisabledReasons.System))
                    {
                        // Yes, enable the note acceptor for reason System.
                        Logger.Info("Enabling the note acceptor");
                        device.Enable(EnabledReasons.System);
                    }
                }
            }
        }
    }
}