namespace Aristocrat.Monaco.Mgam.Services.Communications
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Common;
    using Aristocrat.Mgam.Client.Routing;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Provides a record of all the messages going to and from the server.
    /// </summary>
    public sealed class HostTranscripts : IHostTranscripts, IService, IDisposable
    {
        private const int HandledMessageQueueDepth = 500;

        private readonly IEventBus _eventBus;
        private readonly IEgm _egm;

        private readonly ConcurrentQueue<RoutedMessage> _handledMessages = new ConcurrentQueue<RoutedMessage>();
        private readonly IObservable<RoutedMessage> _messages;

        private readonly IObservable<ConnectionState> _states;

        private readonly IObservable<EventPattern<RegisteredInstanceEventArgs>> _instances;

        private SubscriptionList _subscriptions = new SubscriptionList();

        private InstanceInfo _currentInstance;

        private ConnectionState _currentState = ConnectionState.Lost;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTranscripts"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="egm"><see cref="IEgm"/>.</param>
        /// <param name="messages"><see cref="IRoutedMessagesSubscription"/>.</param>
        /// <param name="statuses"><see cref="ITransportStatusSubscription"/>.</param>
        public HostTranscripts(
            IEventBus eventBus,
            IEgm egm,
            IRoutedMessagesSubscription messages,
            ITransportStatusSubscription statuses)
        {
            _eventBus = eventBus;
            _egm = egm;

            SubscribeToEvents();

            _instances = Observable.FromEventPattern<RegisteredInstanceEventArgs>(
                h => InstanceRegistered += h,
                h => InstanceRegistered -= h);

            _messages = Observable.Create<RoutedMessage>(
                obs =>
                {
                    var subscription = messages.Subscribe(
                        Observer.Create<RoutedMessage>(
                            obs.OnNext,
                            obs.OnError,
                            obs.OnCompleted));
                    return () => { subscription.Dispose(); };
                });

            _states = Observable.Create<ConnectionState>(
                obs =>
                {
                    var subscription = statuses.Subscribe(
                        Observer.Create<TransportStatus>(
                            status =>
                            {
                                if (status.IsBroadcast || status.ConnectionState != ConnectionState.Connected && status.ConnectionState != ConnectionState.Lost)
                                {
                                    return;
                                }

                                obs.OnNext(_currentState);
                            },
                            obs.OnError,
                            obs.OnCompleted));
                    return () => { subscription.Dispose(); };
                });

            _subscriptions.Add(statuses.Subscribe(Observer.Create<TransportStatus>(TransportStatusChanged)));
            _subscriptions.Add(messages.Subscribe(Observer.Create<RoutedMessage>(MessageReceived)));
        }

        private event EventHandler<RegisteredInstanceEventArgs> InstanceRegistered;

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHostTranscripts) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<ConnectionState> observer)
        {
            observer.OnNext(_currentState);

            return _states.Subscribe(observer);
        }

        public IDisposable Subscribe(IObserver<RegisteredInstance> observer)
        {
            if (_currentInstance != null)
            {
                var e = new RegisteredInstanceEventArgs(_currentInstance);
                observer.OnNext(e.Instance);
            }

            return _instances.Select(e => e.EventArgs.Instance).Subscribe(observer);
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<RoutedMessage> observer)
        {
            _handledMessages.ToList().ForEach(observer.OnNext);

            return _messages.Subscribe(observer);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_subscriptions != null)
            {
                _subscriptions.Dispose();
            }

            _subscriptions = null;

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<InstanceRegisteredEvent>(this, Handle);
        }

        private void MessageReceived(RoutedMessage message)
        {
            _handledMessages.Enqueue(message);

            if (_handledMessages.Count > HandledMessageQueueDepth)
            {
                _handledMessages.TryDequeue(out _);
            }
        }

        private void TransportStatusChanged(TransportStatus status)
        {
            if (status.IsBroadcast || status.ConnectionState != ConnectionState.Connected && status.ConnectionState != ConnectionState.Lost)
            {
                return;
            }

            _currentState = status.ConnectionState;
        }

        private void Handle(InstanceRegisteredEvent evt)
        {
            _currentInstance = _egm.ActiveInstance;
            if (_currentInstance == null)
            {
                return;
            }

            RaiseInstanceRegistered(_currentInstance);
        }

        private void RaiseInstanceRegistered(InstanceInfo instance)
        {
            InstanceRegistered?.Invoke(this, new RegisteredInstanceEventArgs(instance));
        }
    }
}
