namespace Aristocrat.Monaco.Mgam.Services.Lockup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Attribute;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Attributes;
    using Common;
    using Common.Events;
    using Event;
    using Kernel;
    using Localization.Properties;
    using Notification;

    /// <summary>
    ///     Implement the <see cref="ILockup"/> interface.
    /// </summary>
    public class Lockup : IService, ILockup, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly ITiltLogger _tiltLogger;
        private readonly ILogger<Lockup> _logger;
        private readonly INotificationLift _notification;
        private readonly IAttributeManager _attributeManager;

        private readonly List<Type> _handledTilts;

        private bool _disposed;

        /// <inheritdoc />
        public string Name => typeof(Lockup).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ILockup) };

        /// <summary>
        ///     Construct a <see cref="Lockup"/> object.
        /// </summary>
        /// <param name="bus">Instance of <see cref="IEventBus"/>.</param>
        /// <param name="disable">Instance of <see cref="ISystemDisableManager"/>.</param>
        /// <param name="tiltLogger">Instance of <see cref="ITiltLogger"/>.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/> for <see cref="Lockup"/>.</param>
        /// <param name="eventDispatcher">Instance of <see cref="IEventDispatcher"/>.</param>
        /// <param name="notification">Instance of <see cref="INotificationLift"/>.</param>
        /// <param name="attributeManager">Instance of <see cref="IAttributeManager"/>.</param>
        public Lockup(
            IEventBus bus,
            ISystemDisableManager disable,
            ITiltLogger tiltLogger,
            ILogger<Lockup> logger,
            IEventDispatcher eventDispatcher,
            INotificationLift notification,
            IAttributeManager attributeManager)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _disableManager = disable ?? throw new ArgumentNullException(nameof(disable));
            _tiltLogger = tiltLogger ?? throw new ArgumentNullException(nameof(tiltLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _attributeManager = attributeManager ?? throw new ArgumentNullException(nameof(attributeManager));

            _logger.LogDebug("Starting");

            _handledTilts = eventDispatcher.ConsumedEventTypes.ToList();

            _tiltLogger.TiltLogAppendedTilt += Handle;

            _bus.Subscribe<EmployeeLoggedInEvent>(this, Handle);
            _bus.Subscribe<EmployeeLoggedOutEvent>(this, Handle);
            _bus.Subscribe<AttributesUpdatedEvent>(this, Handle);
            _bus.Subscribe<SetAttributeFailedEvent>(this, Handle);

            if (!_attributeManager.Has(CustomAttributeNames.LockupAttributeName))
            {
                _attributeManager.Add(
                    new AttributeInfo
                    {
                        Scope = AttributeScope.Device,
                        Name = CustomAttributeNames.LockupAttributeName,
                        DefaultValue = string.Empty,
                        Type = AttributeValueType.String,
                        AccessType = AttributeAccessType.Device
                    });
            }
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void AddHostLock(string message) => Lockout(MgamConstants.ProtocolCommandDisableKey, message, SystemDisablePriority.Immediate);

        /// <inheritdoc />
        public void ClearHostLock() => ClearLockout(MgamConstants.ProtocolCommandDisableKey);

        /// <inheritdoc />
        public void LockupForEmployeeCard(string message = null, SystemDisablePriority priority = SystemDisablePriority.Immediate) => Lockout(MgamConstants.NeedEmployeeCardGuid, message, priority);

        /// <inheritdoc />
        public bool IsLockedForEmployeeCard => _disableManager.CurrentDisableKeys.Contains(MgamConstants.NeedEmployeeCardGuid);

        /// <inheritdoc />
        public bool IsLockedByHost => _disableManager.CurrentDisableKeys.Contains(MgamConstants.ProtocolCommandDisableKey);

        /// <inheritdoc />
        public bool IsEmployeeLoggedIn { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _bus.UnsubscribeAll(this);

                    _tiltLogger.TiltLogAppendedTilt -= Handle;
                }

                _disposed = true;
            }
        }

        private void Lockout(Guid uniqueId, string message, SystemDisablePriority priority)
        {
            if (IsEmployeeLoggedIn && (string.IsNullOrEmpty(message) || uniqueId.Equals(MgamConstants.NeedEmployeeCardGuid)))
            {
                _logger.LogDebug($"Ignoring Lockout request '{uniqueId}':" +
                             $" loggedIn={IsEmployeeLoggedIn}, _isLockedUpByHost={IsLockedByHost}");
                return;
            }

            _logger.LogDebug($"Lock up due to {uniqueId}, '{message}'");

            if (uniqueId.Equals(MgamConstants.NeedEmployeeCardGuid))
            {
                if (_disableManager.CurrentImmediateDisableKeys.Contains(uniqueId))
                {
                    return;
                }

                DisableSystem(
                    MgamConstants.NeedEmployeeCardGuid,
                    string.IsNullOrEmpty(message)
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupWithEmployeeCard)
                        : $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearLockupWithEmployeeCard)}\n{message}",
                    priority);
            }
            else
            {
                DisableSystem(uniqueId, message, priority);
            }
        }

        private void ClearLockout(Guid uniqueId)
        {
            _logger.LogDebug("Clear host lockout");
            EnableSystem(uniqueId);
            EnableSystem(MgamConstants.NeedEmployeeCardGuid);
        }

        private void Handle(SetAttributeFailedEvent evt)
        {
            switch (evt.Response)
            {
                case ServerResponseCode.InvalidDefaultValue:
                case ServerResponseCode.InvalidStringParameterTooLong:
                case ServerResponseCode.AttributeInvalidAttributeName:
                case ServerResponseCode.AttributeNameNotFound:
                case ServerResponseCode.AttributeWritePermissionDenied:
                    LockupForEmployeeCard($"Set Attribute Failed {evt.Response}");
                    break;
                case ServerResponseCode.InvalidInstanceId:
                case ServerResponseCode.VltServiceNotRegistered:
                case ServerResponseCode.ServerError:
                case ServerResponseCode.Unknown:
                    _bus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
                    break;
            }
        }

        private void Handle(EmployeeLoggedInEvent evt)
        {
            IsEmployeeLoggedIn = true;

            if (!IsLockedForEmployeeCard)
            {
                return;
            }

            _logger.LogDebug("Clear employee-clearable lockouts");
            EnableSystem(MgamConstants.NeedEmployeeCardGuid);
        }

        private void DisableSystem(Guid guid, string message, SystemDisablePriority priority)
        {
            _logger.LogDebug($"Disable system for [{guid}] '{message}'");
            _disableManager.Disable(guid, priority, () => message);

            if (guid == MgamConstants.ProtocolCommandDisableKey && !string.IsNullOrEmpty(message))
            {
                _attributeManager.Set(CustomAttributeNames.LockupAttributeName, message, AttributeSyncBehavior.LocalAndServer);
            }
        }

        private void EnableSystem(Guid guid)
        {
            _logger.LogDebug($"Enable system from [{guid}]");

            if (!_disableManager.CurrentDisableKeys.Contains(guid))
            {
                return;
            }

            if (guid == MgamConstants.ProtocolCommandDisableKey)
            {
                _attributeManager.Set(CustomAttributeNames.LockupAttributeName, string.Empty, AttributeSyncBehavior.LocalAndServer);
            }

            _disableManager.Enable(guid);

            if (guid == MgamConstants.NeedEmployeeCardGuid)
            {
                _bus.Publish(new LockupResolvedEvent());
            }
        }

        private void Handle(EmployeeLoggedOutEvent evt)
        {
            IsEmployeeLoggedIn = false;
        }

        private void Handle(object sender, TiltLogAppendedEventArgs arg)
        {
            if (arg.Message.Level != "tilt")
            {
                return;
            }

            _logger.LogDebug($"TiltLogger event '{arg.TiltType}'");

            if (_handledTilts.Contains(arg.TiltType))
            {
                _logger.LogDebug("Already handled elsewhere");
                return;
            }

            _logger.LogDebug("Unhandled elsewhere, so send unknown tilt notification");

            // Create a human-readable simple name of this tilt event type for the notification.
            // Examples:
            //  AssemblyName:   Aristocrat.Hardware.Contracts
            //  Type:           Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.DisabledEvent
            //  => Message:     NoteAcceptor Disabled
            //
            //  AssemblyName:   Aristocrat.Monaco.Kernel.Contracts
            //  Type:           Aristocrat.Monaco.Kernel.SystemDisabledEvent
            //  => Message:     SystemDisabled
            var assemblyNameParts = arg.TiltType.Assembly.GetName().Name.Split('.').ToList();
            var typeNameParts = arg.TiltType.FullName.Replace("Event", "").Split('.').ToList();
            typeNameParts.RemoveAll(p => assemblyNameParts.Contains(p));

            // If there was AdditionalInfo in the event, tack that onto the message.
            if (arg.Message.AdditionalInfos !=null && arg.Message.AdditionalInfos.Any())
            {
                typeNameParts.Add(arg.Message.GetAdditionalInfoString());
            }

            var displayMessage = string.Join(" ", typeNameParts.ToArray());
            _notification.Notify(NotificationCode.LockedTilt, displayMessage);
        }

        private void Handle(AttributesUpdatedEvent evt)
        {
            var lockup = _attributeManager.Get<string>(CustomAttributeNames.LockupAttributeName);
            if (!string.IsNullOrEmpty(lockup))
            {
                Lockout(MgamConstants.ProtocolCommandDisableKey, lockup, SystemDisablePriority.Immediate);
            }
        }
    }
}
