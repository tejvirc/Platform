namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Reflection;
    using Aristocrat.Extensions.CommunityToolkit;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handles registration as the IMessageDisplayHandler as well as dispatching of incomming messages to the GUI thread.
    /// </summary>
    public sealed class StatusDisplay : IMessageDisplayHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();
        private bool _disposed;

        private StatusDisplayView _statusDisplay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusDisplay" /> class.
        /// </summary>
        /// <param name="display">The display to which to forward messages.</param>
        public StatusDisplay(StatusDisplayView display)
        {
            _statusDisplay = display;
            ServiceManager.GetInstance().GetService<IMessageDisplay>().AddMessageDisplayHandler(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void ClearMessages()
        {
            lock (_lock)
            {
                Logger.Debug("Clearing message");
                Execute.OnUIThread(() => _statusDisplay.ClearMessages());
                Logger.Debug("Cleared message");
            }
        }

        /// <inheritdoc />
        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            lock (_lock)
            {
                Logger.Debug("Displaying message");
                Execute.OnUIThread(() => _statusDisplay.DisplayMessage(displayableMessage));
                Logger.Debug("Displayed message");
            }
        }

        /// <inheritdoc />
        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            lock (_lock)
            {
                Logger.Debug("Removing message");
                Execute.OnUIThread(() => _statusDisplay.RemoveMessage(displayableMessage));
                Logger.Debug("Removed message");
            }
        }

        /// <inheritdoc />
        public void DisplayStatus(DisplayableMessage message)
        {
<<<<<<< HEAD
            MvvmHelper.ExecuteOnUI(() => _statusDisplay.DisplayStatus(message));   
        }

        public void UpdateMessages()
        {
            MvvmHelper.ExecuteOnUI(() => _statusDisplay.UpdateMessages());
=======
            lock (_lock)
            {
                Execute.OnUIThread(() => _statusDisplay.DisplayStatus(message));
            }
>>>>>>> refs/remotes/origin/main
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                lock (_lock)
                {
                    if (_statusDisplay != null)
                    {
                        var messageDisplay = ServiceManager.GetInstance().TryGetService<IMessageDisplay>();
                        messageDisplay?.RemoveMessageDisplayHandler(this);
                    }
                }
            }

            _statusDisplay = null;

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="StatusDisplay" /> class.
        /// </summary>
        ~StatusDisplay()
        {
            Dispose(false);
        }
    }
}
