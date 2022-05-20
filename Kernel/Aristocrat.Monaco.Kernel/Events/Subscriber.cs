namespace Aristocrat.Monaco.Kernel.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using log4net;

    /// <summary>
    ///     Represents a subscriber
    /// </summary>
    internal sealed class Subscriber : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(5);

        private readonly ActionBlock<PublishEventRequest> _messageProcessor;

        private readonly string _name;

        private readonly ConcurrentDictionary<Type, Subscription> _subscriptions =
            new ConcurrentDictionary<Type, Subscription>();

        private ManualResetEventSlim _completeEvent = new ManualResetEventSlim(false);

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Subscriber" /> class.
        /// </summary>
        /// <param name="name">An optional user-friendly name for the subscriber</param>
        public Subscriber(string name)
        {
            _name = name;

            _messageProcessor = new ActionBlock<PublishEventRequest>(
                async request =>
                {
                    if (!request.CancellationToken.IsCancellationRequested)
                    {
                        if (_subscriptions.TryGetValue(request.Event.GetType(), out var subscription))
                        {
                            await subscription.Handler.Invoke(request.Event, request.CancellationToken);

                            Logger.Debug($"Event {request.Event} handled by {_name}");
                        }
                    }
                });

            _messageProcessor.Completion.ContinueWith(
                t =>
                {
                    // If the message processor terminates due to a fault, we're going to throw the exception (if it's not null)
                    //  This will result in an unhandled exception, which is currently the desired behavior
                    if (t.IsFaulted)
                    {
                        if (t.Exception != null)
                        {
                            throw t.Exception.Flatten();
                        }

                        throw new TaskSchedulerException($"Event handling faulted for {_name}");
                    }
                });
        }

        ~Subscriber()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     The method used to post an event to the Subscriber
        /// </summary>
        /// <param name="request">The event request</param>
        public void Publish(PublishEventRequest request)
        {
            Logger.Debug(
                $"Adding event {request.Event} to queue for {_name} - Current Queue Depth {_messageProcessor.InputCount}");

            _messageProcessor.Post(request);
        }

        /// <summary>
        ///     Adds the provided handler
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <param name="handler">The event handler</param>
        /// <param name="filter">
        ///     A function to test the event for a condition. A return value of false will result in the event not
        ///     being published to the subscriber context.
        /// </param>
        public void Subscribe<T>(Func<T, CancellationToken, Task> handler, Predicate<T> filter)
            where T : IEvent
        {
            _subscriptions.AddOrUpdate(
                typeof(T),
                Subscription.Create(handler, filter),
                (key, value) => Subscription.Create(handler, filter));
        }

        /// <summary>
        ///     Removes the event subscription
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        public int UnSubscribe<T>()
            where T : IEvent
        {
            _subscriptions.TryRemove(typeof(T), out _);

            return _subscriptions.Count;
        }

        /// <summary>
        ///     Determines if this subscriber handles the event
        /// </summary>
        /// <typeparam name="T">The event type</typeparam>
        /// <returns>true if the subscriber handles the event</returns>
        public bool HandlesEvent<T>(T @event)
            where T : IEvent
        {
            if (!_subscriptions.TryGetValue(typeof(T), out var handler))
            {
                return false;
            }

            return handler.Filter(@event);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _subscriptions.Clear();

                Complete();

                _completeEvent.Dispose();
            }

            _completeEvent = null;

            _disposed = true;
        }

        private void Complete()
        {
            // If the block hasn't been completed we're going to synchronously wait for it to complete.
            //  This ensures that any event being processed can run to completion without the rug being ripped out from underneath it
            if (!_messageProcessor.Completion.IsCanceled && !_messageProcessor.Completion.IsFaulted &&
                !_messageProcessor.Completion.IsCompleted)
            {
                _messageProcessor.Completion.ContinueWith(_ => _completeEvent?.Set());
            }
            else
            {
                _completeEvent.Set();
            }

            _messageProcessor.Complete();

            _completeEvent.Wait(MaxWait);
        }
    }
}
