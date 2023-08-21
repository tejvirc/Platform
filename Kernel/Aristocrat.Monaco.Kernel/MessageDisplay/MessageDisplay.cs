namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.ErrorMessage;
    using log4net;

    /// <summary>
    ///     Manages message display handlers and routes commands to display and
    ///     remove messages to the registered handler.
    /// </summary>
    public class MessageDisplay : IService, IMessageDisplay, IDisposable
    {
        private const string AddinMessageDisplayRemoveExtensionPoint = "/Kernel/MessageDisplayRemove";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private List<MessageDisplayReasonNode> _configuredDisplayNodes = new List<MessageDisplayReasonNode>();
        private ConcurrentStack<IMessageDisplayHandler> _handlers = new ConcurrentStack<IMessageDisplayHandler>();
        private Collection<DisplayableMessage> _messages = new Collection<DisplayableMessage>();
        private List<ObservedMessage> _observedMessages = new List<ObservedMessage>();
        private IEventBus _eventBus;
        private IErrorMessageMapping _mapping;
        private readonly object _messageLock = new object();

        private bool _disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void AddMessageDisplayHandler(IMessageDisplayHandler handler, bool displayPreviousMessages = true)
        {
            if (!_handlers.Contains(handler))
            {
                Logger.Debug("Adding new message display handler...");

                _handlers.Push(handler);

                lock (_messageLock)
                {
                    foreach (var message in _messages)
                    {
                        // Hard error messages are always displayed
                        // Soft error and Info messages are displayed only if requested
                        if (message.Classification == DisplayableMessageClassification.HardError ||
                            displayPreviousMessages)
                        {
                            handler?.DisplayMessage(message);
                        }
                    }
                }
                Logger.Debug("Added new message display handler...");
            }
            else
            {
                Logger.WarnFormat("MessageDisplay already contains message display handler: {0}", handler);
            }
        }

        /// <inheritdoc />
        public void RemoveMessageDisplayHandler(IMessageDisplayHandler handler)
        {
            Logger.Debug("Try to remove message display handler and clearing its messages...");
            handler?.ClearMessages();
            if (_handlers.TryPeek(out var i) && i.Equals(handler) && _handlers.TryPop(out i))
            {
                return;
            }

            var items = new IMessageDisplayHandler[_handlers.Count];
            if (_handlers.TryPopRange(items) > 0)
            {
                bool found = false;
                foreach (var item in items)
                {
                    if (!item.Equals(handler))
                    {
                        _handlers.Push(item);
                    }
                    else
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    Logger.WarnFormat(
                        "MessageDisplay has no handlers to remove\nMessage display handler: {0}",
                        handler);
                }
            }
            else
            {
                Logger.WarnFormat(
                    "MessageDisplay has no handlers to remove\nMessage display handler: {0}",
                    handler);
            }
        }

        /// <inheritdoc />
        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            var mappedMessage = MapMessage(displayableMessage);

            Logger.Info($"Adding message to MessageDisplay: {mappedMessage}");

            lock (_messageLock)
            {
                if (!_messages.Any(o => mappedMessage.IsMessageEquivalent(o)))
                {
                    var sameId = _messages.Where(o => o.Id == mappedMessage.Id && !mappedMessage.IsMessageEquivalent(o)).ToList();
                    foreach (var message in sameId)
                    {
                        // We are already displaying a message on this Guid, remove it for the new message if anything has changed
                        RemoveMessage(message);
                    }

                    if (mappedMessage.MessageHasDynamicGuid)
                    {
                        // Messages with dynamic Guids have to be equated via the text of the message
                        var sameText = _messages.Where(o => o.MessageHasDynamicGuid && o.Message == mappedMessage.Message
                                                        && !mappedMessage.IsMessageEquivalent(o)).ToList();
                        foreach (var message in sameText)
                        {
                            // We are already displaying a message with the same text.  Remove it for the new message if anything has changed
                            RemoveMessage(message);
                        }
                    }

                    _messages.Add(mappedMessage);

                    var displayNode = _configuredDisplayNodes.Find(node => node.EventType == mappedMessage.ReasonEvent);
                    if (displayNode != null)
                    {
                        _observedMessages.Add(
                            new ObservedMessage
                            {
                                Message = mappedMessage,
                                DisplayReasonNode = displayNode
                            });
                    }

                    Logger.Debug($"Displaying new message: {mappedMessage}");

                    foreach (var handler in _handlers)
                    {
                        if (handler == null)
                        {
                            continue;
                        }

                        Logger.Debug($"Sending message to display handler: {handler.GetType().Name}");
                        handler.DisplayMessage(mappedMessage);
                    }

                    _eventBus.Publish(new MessageAddedEvent(displayableMessage));
                }
                else
                {
                    Logger.Warn("Duplicate message add attempted...");
                }
            }
        }

        /// <inheritdoc />
        public void DisplayMessage(DisplayableMessage displayableMessage, int timeout)
        {
            DisplayMessage(displayableMessage);

            Task.Delay(timeout).ContinueWith(_ => RemoveMessage(displayableMessage));
        }

        private void RemoveMessageInternal(DisplayableMessage displayableMessage, bool explicitRemove = false)
        {
            DisplayableMessage mappedMessage = null;
            if (!explicitRemove)
            {
                mappedMessage = MapMessage(displayableMessage);
            }
            Logger.Debug($"Removing Internal - {displayableMessage.Message}");
            lock (_messageLock)
            {
                List<ObservedMessage> observedMessagesToRemove;
                if (explicitRemove)
                {
                    // explicit remove is referring to the _messages object.  We still need to search _observedMessages by id
                    observedMessagesToRemove =
                        _observedMessages.Where(o => o.Message.Id == displayableMessage.Id).ToList();
                }
                else
                {
                    observedMessagesToRemove = _observedMessages
                        .Where(o => mappedMessage?.IsMessageEquivalent(o.Message) ?? false).ToList();
                }

                foreach (var message in observedMessagesToRemove)
                {
                    _observedMessages.Remove(message);
                }

                var messagesToRemove = explicitRemove
                    ? new List<DisplayableMessage> { displayableMessage }
                    : _messages.Where(o => mappedMessage?.IsMessageEquivalent(o) ?? false).ToList();

                foreach (var message in messagesToRemove)
                {
                    if (_messages.Remove(message))
                    {
                        Logger.Info($"Removing message {message} from MessageDisplay");

                        foreach (var handler in _handlers)
                        {
                            Logger.Debug($"Removing message {message} from display handler {handler}");
                            handler?.RemoveMessage(message);
                        }
                    }
                }
            }

            Logger.Debug($"Removed Internal - {displayableMessage.Message}");

            _eventBus.Publish(new MessageRemovedEvent(displayableMessage));
        }

        /// <inheritdoc />
        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            Logger.Debug($"Removing message - {displayableMessage?.Message ?? string.Empty}");

            RemoveMessageInternal(displayableMessage);

            Logger.Debug($"Removed message - {displayableMessage?.Message ?? string.Empty}");
        }

        /// <summary>
        /// Same as RemoveMessage(DisplayableMessage) but removes a message added with the supplied messageId
        /// </summary>
        /// <param name="messageId"></param>
        public void RemoveMessage(Guid messageId)
        {
            Logger.Debug($"Removing message Id - {messageId}");

            lock (_messageLock)
            {
                if (_messages.Any(o => o.Id == messageId))
                {
                    // There should only ever be one of these, but remove them all to be safe.
                    var remove = _messages.Where(o => o.Id == messageId).ToList();
                    foreach (var message in remove)
                    {
                        RemoveMessageInternal(message, true);
                    }
                }
            }

            Logger.Debug($"Removed message Id - {messageId}");
        }

        /// <inheritdoc />
        public void DisplayStatus(string statusMessage)
        {
            foreach (var handler in _handlers)
            {
                handler?.DisplayStatus(statusMessage);
            }
        }

        /// <summary>
        /// Use AddErrorMessageMapping to inject the ErrorMessageMapping object into the MessageDisplay Service
        /// </summary>
        /// <param name="mapping"></param>
        public void AddErrorMessageMapping(IErrorMessageMapping mapping)
        {
            _mapping = mapping;
            Logger.Debug("Adding error message mapping");
            // I believe this is called early enough that we won't ever have anything in _messages that needs to be back-translated.
            // but adding in this code to be safe
            lock (_messageLock)
            {
                foreach (var message in _messages.ToList())
                {
                    var mappingResult = _mapping.MapError(message.Id, message.Message);
                    if (mappingResult.errorMapped)
                    {
                        var newMessage = new DisplayableMessage(() => mappingResult.mappedText, message.Classification, message.Priority, message.ReasonEvent, message.Id);
                        RemoveMessage(message);
                        DisplayMessage(newMessage);
                    }
                }
            }

            Logger.Debug("Added error message mapping");
        }

        /// <inheritdoc />
        public string Name => "Message Display Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMessageDisplay) };

        /// <inheritdoc />
        public void Initialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            var displayEventTypes = new List<Type>();

            // Gather all configured display nodes and subscribe to the remove events.
            var removeTypes = new HashSet<Type>();
            _configuredDisplayNodes = new List<MessageDisplayReasonNode>(
                MonoAddinsHelper.GetSelectedNodes<MessageDisplayReasonNode>(AddinMessageDisplayRemoveExtensionPoint));
            foreach (var displayNode in _configuredDisplayNodes)
            {
                string logMessage;
                if (displayNode.EventType == null)
                {
                    logMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        "No valid display event type defined in AddinId {0}",
                        displayNode.Addin.Id);
                    Logger.Error(logMessage);
                    throw new MessageDisplayException(logMessage);
                }

                if (displayEventTypes.Contains(displayNode.EventType))
                {
                    logMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        "The display event type {0} defined in AddinId {1} has been used before",
                        displayNode.EventType,
                        displayNode.Addin.Id);
                    Logger.Error(logMessage);
                    throw new MessageDisplayException(logMessage);
                }

                displayEventTypes.Add(displayNode.EventType);
                if (displayNode.RemoveNodes.Count == 0)
                {
                    logMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        "There are no remove nodes defined in the display event {0}.",
                        displayNode.EventType);
                    Logger.Error(logMessage);
                    throw new MessageDisplayException(logMessage);
                }

                logMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    "Display event type: {0} / Remove event type(s):  ",
                    displayNode.EventType);
                foreach (var eventType in displayNode.RemoveNodes)
                {
                    removeTypes.Add(eventType);
                    logMessage += string.Format(CultureInfo.CurrentCulture, "{0}; ", eventType);
                }

                Logger.Info(logMessage);
            }

            foreach (var removeType in removeTypes)
            {
                _eventBus.Subscribe(this, removeType, ReceiveEvent);
            }

            Logger.Debug("Initialized");
        }

        /// <summary>
        ///     Dispose of managed resources.
        /// </summary>
        /// <param name="disposing">true if Dispose() has been called.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventBus?.UnsubscribeAll(this);

                    _handlers.Clear();
                    _handlers = null;

                    _messages.Clear();
                    _messages = null;

                    _configuredDisplayNodes.Clear();
                    _configuredDisplayNodes = null;

                    _observedMessages.Clear();
                    _observedMessages = null;
                }

                _disposed = true;
            }
        }

        private void ReceiveEvent(IEvent data)
        {
            var removeMessages = new List<DisplayableMessage>();
            List<ObservedMessage> observedMessages;

            lock (_messageLock)
            {
                // Find out all observed message related to this event type for removal.             
                observedMessages =
                    _observedMessages.FindAll(
                        trace =>
                        trace.DisplayReasonNode.RemoveNodes.Contains(data.GetType()));
            }

            foreach (var observedMessage in observedMessages)
            {
                var removeNodes = observedMessage.DisplayReasonNode.RemoveNodes.ToList();
                var index = removeNodes.FindIndex(type => type == data.GetType());
                var expectedEventTypes = new List<Type>();
                removeNodes.GetRange(0, index).ForEach(item => expectedEventTypes.Add(item));

                if (expectedEventTypes.SequenceEqual(observedMessage.ReceivedEventTypes))
                {
                    if (removeNodes.Last() == data.GetType())
                    {
                        // It's the last event so ready to remove the message.
                        removeMessages.Add(observedMessage.Message);
                    }
                    else
                    {
                        observedMessage.ReceivedEventTypes.Add(data.GetType());
                    }
                }
                else
                {
                    // Out of the sequence. Reset.
                    observedMessage.ReceivedEventTypes.Clear();
                }
            }

            // Now actually remove the messages.
            foreach (var message in removeMessages)
            {
                RemoveMessage(message);
            }
        }

        private DisplayableMessage MapMessage(DisplayableMessage message)
        {
            if (_mapping == null)
            {
                return message;
            }

            var errorMapping = _mapping.MapError(message.Id, message.Message);
            if (errorMapping.errorMapped)
            {
                return new DisplayableMessage(
                    () => errorMapping.mappedText,
                    message.Classification,
                    message.Priority,
                    message.ReasonEvent,
                    message.Id);
            }

            return message;
        }

        /// <summary>
        ///     Class used to check when a displayed message needs to be removed.
        /// </summary>
        internal class ObservedMessage
        {
            /// <summary>
            ///     Gets or sets a message being displayed.
            /// </summary>
            public DisplayableMessage Message { get; set; }

            /// <summary>
            ///     Gets or sets the configured node associated with this message.
            /// </summary>
            public MessageDisplayReasonNode DisplayReasonNode { get; set; }

            /// <summary>
            ///     Gets the list of received event types.
            /// </summary>
            public List<Type> ReceivedEventTypes { get; } = new List<Type>();
        }
    }
}