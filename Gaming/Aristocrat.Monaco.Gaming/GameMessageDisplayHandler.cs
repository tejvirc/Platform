namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Kernel;
    using log4net;
    using Runtime;

    /// <summary>
    ///     Implementation of IMessageDisplayHandler
    /// </summary>
    /// <remarks>
    ///     GameMessageDisplayHandler listens for messages coming from the MessageDisplay and forwards then to the GDK. It only
    ///     does so while
    ///     the game is loaded, hooking and unhooking from the MessageDisplay as appropriate.
    /// </remarks>
    public class GameMessageDisplayHandler : IMessageDisplayHandler, IDisposable
    {
        private static readonly TimeSpan MaxDisplayLimit = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IRuntime _runtime;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IEventBus _eventBus;
        private readonly IMessageDisplay _messageDisplay;
        private readonly List<DisplayableMessage> _displayMessages = new List<DisplayableMessage>();
        private readonly bool _showMessages;
        private readonly object _messageLock = new object();

        private Timer _changePropagationTimer;

        private string _lastMessage = string.Empty;
        private TimeSpan _currentMessageDisplayTime;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMessageDisplayHandler" /> class.
        /// </summary>
        /// <param name="runtimeService">The runtime service</param>
        /// <param name="eventBus">The event bus</param>
        /// <param name="messageDisplay">The message display</param>
        /// <param name="properties">The property manager.</param>
        /// <param name="gameDiagnostics">The game replay service.</param>
        public GameMessageDisplayHandler(
            IRuntime runtimeService,
            IEventBus eventBus,
            IMessageDisplay messageDisplay,
            IPropertiesManager properties,
            IGameDiagnostics gameDiagnostics)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));

            _changePropagationTimer = new Timer(UpdateMessages, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _showMessages = properties.GetValue(GamingConstants.ShowMessages, false);

            _messageDisplay.AddMessageDisplayHandler(this);

            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, HandleEvent);
        }

        /// <inheritdoc />
        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            if (!_showMessages || _gameDiagnostics.IsActive || displayableMessage == null ||
                (displayableMessage.Classification != DisplayableMessageClassification.SoftError &&
                 displayableMessage.Classification != DisplayableMessageClassification.Informative))
            {
                return;
            }

            Logger.Debug("Displaying messages");

            lock (_messageLock)
            {
                _displayMessages.Add(displayableMessage);
            }

            if (_displayMessages.Count == 1)
            {
                _changePropagationTimer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }

            Logger.Debug("Displayed messages");
        }

        /// <inheritdoc />
        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            Logger.Debug("Removing messages");

            lock (_messageLock)
            {
                if (_displayMessages.Count > 0 && _displayMessages[0] == displayableMessage)
                {
                    _currentMessageDisplayTime = TimeSpan.Zero;
                }

                _displayMessages.Remove(displayableMessage);
            }

            Logger.Debug("Removed messages");
        }

        /// <inheritdoc />
        public void DisplayStatus(string message)
        {
        }

        /// <inheritdoc />
        public void ClearMessages()
        {
            Logger.Debug("Clearing messages");

            lock (_messageLock)
            {
                _displayMessages.Clear();
            }

            Logger.Debug("Cleared messages");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_changePropagationTimer != null)
                {
                    _changePropagationTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                    using (var handle = new ManualResetEvent(false))
                    {
                        if (_changePropagationTimer.Dispose(handle))
                        {
                            if (!handle.WaitOne(ShutdownTimeout))
                            {
                                Logger.Warn("Failed waiting for Dispose of timer");
                            }
                        }
                    }

                    _changePropagationTimer.Dispose();
                }

                _eventBus.UnsubscribeAll(this);
            }
            _changePropagationTimer = null;

            _disposed = true;
        }

        private void UpdateMessages(object state)
        {
            if (!_runtime.Connected || _changePropagationTimer == null)
            {
                _lastMessage = string.Empty;
                return;
            }

            lock (_messageLock)
            {
                if (_displayMessages.Count == 0)
                {
                    _lastMessage = string.Empty;
                    _runtime.UpdatePlatformMessages(null);
                }
                else
                {
                    if (_currentMessageDisplayTime < MaxDisplayLimit)
                    {
                        _currentMessageDisplayTime += UpdateInterval;
                    }

                    if (_displayMessages.Count > 1 && _currentMessageDisplayTime >= MaxDisplayLimit)
                    {
                        var current = _displayMessages[0];
                        _displayMessages.Remove(current);
                        _displayMessages.Add(current);
                        _currentMessageDisplayTime = TimeSpan.Zero;
                    }

                    UpdateMessage(_displayMessages[0].Message);

                    _changePropagationTimer?.Change(UpdateInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void UpdateMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || _lastMessage.Equals(message))
            {
                return;
            }

            _lastMessage = message;

            _runtime.UpdatePlatformMessages(new[] { message });
        }

        private void HandleEvent(GameInitializationCompletedEvent evt)
        {
            // Once a game has initialized, re-broadcast platform messages by registering for messages and setting the pending flag on
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            _messageDisplay.AddMessageDisplayHandler(this);

            _changePropagationTimer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        private void HandleEvent(GameProcessExitedEvent evt)
        {
            // un-register for messages, the game has been unloaded
            _messageDisplay.RemoveMessageDisplayHandler(this);

            _changePropagationTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}