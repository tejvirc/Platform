namespace Aristocrat.Monaco.Mgam.Services.Communications
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reactive;
    using System.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Common;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Options;
    using Aristocrat.Mgam.Client.Routing;
    using Attributes;
    using Common;
    using Common.Events;
    using Kernel;
    using Localization.Properties;
    using Lockup;
    using Notification;

    /// <summary>
    ///     Manages interaction with the communication services.
    /// </summary>
    internal sealed class CommsObserverService : ICommsObserver, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IAttributeManager _attributes;
        private readonly ILocalization _localization;
        private readonly IEgm _egm;
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;

        private SubscriptionList _subscriptions = new SubscriptionList();

        private bool _hostOnline;

        private bool _instanceRegistered;

        private bool _disposed;

        /// <summary>
        ///     Instantiates a new instance of the <see cref="CommsObserverService"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="attributes"><see cref="IAttributeManager"/>.</param>
        /// <param name="localization"><see cref="ILocalization"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="lockup"><see cref="ILockup"/>.</param>
        /// <param name="notificationLift"><see cref="INotificationLift"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        /// <param name="statuses"><see cref="ITransportStatusSubscription"/>.</param>
        public CommsObserverService(
            ILogger<CommsObserverService> logger,
            IEventBus eventBus,
            IPropertiesManager properties,
            IAttributeManager attributes,
            ILocalization localization,
            IEgm egm,
            ILockup lockup,
            INotificationLift notificationLift,
            IOptionsMonitor<ProtocolOptions> options,
            ITransportStatusSubscription statuses)
        {
            _logger = logger;
            _eventBus = eventBus;
            _properties = properties;
            _attributes = attributes;
            _localization = localization;
            _egm = egm;
            _lockup = lockup;
            _notificationLift = notificationLift;

            _subscriptions.Add(options.OnChange(OnDirectoryAddressChanged, name => name == nameof(ProtocolOptions.DirectoryResponseAddress)));

            _subscriptions.Add(
                statuses.Subscribe(
                    Observer.Create<TransportStatus>(
                        OnTransportStatusChanged,
                        ex => _logger.LogError("Subscribe transport status failure"))));

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~CommsObserverService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_subscriptions != null)
                {
                    _subscriptions.Dispose();
                }
            }

            _subscriptions = null;

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<InstanceRegisteredEvent>(this, Handle);
        }

        private static void OnDirectoryAddressChanged(ProtocolOptions options, string propertyName)
        {
            var port = unchecked((ushort)options.DirectoryResponseAddress.Port);

            Firewall.AddUdpRule($"{MgamConstants.FirewallDirectoryRuleName} - Port {port}", port);
        }

        private void OnTransportStatusChanged(TransportStatus status)
        {
            if (status.IsBroadcast)
            {
                return;
            }

            switch (status.Failure)
            {
                case TransportFailure.Malformed:
                    _lockup.LockupForEmployeeCard(_localization.For(CultureFor.Operator).GetString(ResourceKeys.VLTCommunicationError));
                    _notificationLift.Notify(NotificationCode.LockedMalformedMessage);
                    return;
                case TransportFailure.Timeout:
                    if (_egm.State < EgmState.Stopping)
                    {
                        _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.ResponseTimeout));
                    }
                    return;
                case TransportFailure.InvalidServerResponse:
                    if (_egm.State < EgmState.Stopping)
                    {
                        _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));
                    }
                    return;
            }

            switch (status.ConnectionState)
            {
                case ConnectionState.Idle:
                    OnIdle();
                    return;
                case ConnectionState.Lost:
                    _eventBus.Publish(new ConnectionLostEvent(status.EndPoint));
                    _notificationLift.Suspend();
                    _notificationLift.Notify(NotificationCode.LostConnection).Wait();
                    break;
                case ConnectionState.Connected:
                    _eventBus.Publish(new ConnectionEstablishedEvent(status.EndPoint));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var hostOnline = status.TransportState == TransportState.Up;

            if (hostOnline && !_hostOnline)
            {
                OnOnline(status.EndPoint.Address.ToString());
            }
            else if (!hostOnline && _hostOnline)
            {
                OnOffline();
            }

            _hostOnline = hostOnline;
        }

        private void OnOnline(string hostAddress)
        {
            _eventBus.Publish(new HostOnlineEvent(hostAddress));
            _properties.SetProperty(ApplicationConstants.CommunicationsOnline, true);
        }

        private void OnOffline()
        {
            _instanceRegistered = false;

            _eventBus.Publish(new HostOfflineEvent());
            _properties.SetProperty(ApplicationConstants.CommunicationsOnline, false);
        }   

        private void OnIdle()
        {
            if (!_instanceRegistered)
            {
                return;
            }

            try
            {
                using (var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(
                        _attributes.Get(AttributeNames.KeepAliveInterval, ProtocolConstants.DefaultKeepAliveInterval))))
                {
                    _egm.KeepAlive(cts.Token).Wait(CancellationToken.None);
                }
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex.Flatten().InnerException, "Send keep-alive message failure");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send keep-alive message failure");
            }
        }

        private void Handle(InstanceRegisteredEvent evt)
        {
            _instanceRegistered = true;
        }
    }
}
