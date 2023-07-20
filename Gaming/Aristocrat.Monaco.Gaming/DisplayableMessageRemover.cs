namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using log4net;

    public class DisplayableMessageRemover : IMessageDisplayHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IEnumerable<Type> RemovingMessages = new List<Type>
        {
            typeof(ClosedEvent),
            typeof(HandpayCompletedEvent),
            typeof(PlatformBootedEvent),
            typeof(VoucherRejectedEvent),
            typeof(CurrencyInCompletedEvent),
            typeof(DocumentRejectedEvent),
            typeof(VoucherRedeemedEvent),
            typeof(VoucherIssuedEvent),
            typeof(HardMeterOutCompletedEvent),
            typeof(WatTransferCommittedEvent),
            typeof(WatOnCompleteEvent),
            typeof(HandpayKeyedOffEvent)
        };

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IMessageDisplay _messageDisplay;
        private readonly List<Type> _removalReason = new();
        private readonly ConcurrentDictionary<Guid, RemovalMessage> _listeningMessages = new();

        private bool _disposed;

        public DisplayableMessageRemover(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IMessageDisplay messageDisplay)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            SetupEventListener();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            if (displayableMessage.ReasonEvent == null || !RemovingMessages.Contains(displayableMessage.ReasonEvent))
            {
                return;
            }

            Logger.Debug($"Adding the message {displayableMessage} for automatic clearing based on game play events");

            _listeningMessages[displayableMessage.Id] = new RemovalMessage(displayableMessage, new Queue<Type>(_removalReason));

            Logger.Debug($"Added the message {displayableMessage} for automatic clearing based on game play events");
        }

        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            Logger.Debug($"Removing message {displayableMessage}");

            bool result = _listeningMessages.TryRemove(displayableMessage.Id, out _); 

            Logger.Debug(result ? "Removed message" : "Message not present");
        }

        public void DisplayStatus(string message)
        {
        }

        public void ClearMessages()
        {
            Logger.Debug("Clearing the messages");

            _listeningMessages.Clear();

            Logger.Debug("Clearing the messages");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _messageDisplay.RemoveMessageDisplayHandler(this);
            }

            _disposed = true;
        }

        private void SetupEventListener()
        {
            var clearStyle = _propertiesManager.GetValue(GamingConstants.MessageClearStyle, MessageClearStyle.GameStart);
            switch (clearStyle)
            {
                case MessageClearStyle.GameStart:
                    _removalReason.Add(typeof(PrimaryGameStartedEvent));
                    _eventBus.Subscribe(this, typeof(PrimaryGameStartedEvent), ReceiveEvent);
                    break;
                case MessageClearStyle.NextGameEnd:
                    _removalReason.Add(typeof(PrimaryGameStartedEvent));
                    _removalReason.Add(typeof(PrimaryGameEndedEvent));
                    _eventBus.Subscribe(this, typeof(PrimaryGameStartedEvent), ReceiveEvent);
                    _eventBus.Subscribe(this, typeof(PrimaryGameEndedEvent), ReceiveEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Logger.Debug($"Setting up the message clear style as {clearStyle}");
            _messageDisplay.AddMessageDisplayHandler(this);
        }

        private void ReceiveEvent(IEvent @event)
        {
            var messages = _listeningMessages.ToArray().Where(x => x.Value.RequiredEvents.Peek() == @event.GetType());
            foreach (var message in messages)
            {
                message.Value.RequiredEvents.Dequeue();
                if (message.Value.RequiredEvents.Any())
                {
                    continue;
                }

                Logger.Debug($"Clearing the message {message} after receiving {@event}");
                _listeningMessages.TryRemove(message.Key, out _);
                _messageDisplay.RemoveMessage(message.Value.Message.Id);
            }
        }

        private class RemovalMessage
        {
            public RemovalMessage(DisplayableMessage message, Queue<Type> requiredEvents)
            {
                Message = message;
                RequiredEvents = requiredEvents;
            }

            public DisplayableMessage Message { get; }

            public Queue<Type> RequiredEvents { get; }
        }
    }
}