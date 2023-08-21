namespace Aristocrat.Monaco.Gaming.VideoLottery.ScreenSaver
{
    using System;
    using Kernel;
    using Monaco.UI.Common;

    public sealed class ScreenSaverWindowLauncher : IDisposable
    {
        private const string ScreensaverWindowName = "ScreensaverWindow";

        private readonly IEventBus _eventBus;
        private readonly object _lock = new object();
        private readonly IWpfWindowLauncher _windowLauncher;

        private bool _disposed;
        private bool _windowCreated;

        public ScreenSaverWindowLauncher(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
            _eventBus.Subscribe<ScreenSaverVisibilityEvent>(
                this,
                evt =>
                {
                    if (evt.Show)
                    {
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                });
        }

        ~ScreenSaverWindowLauncher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Hide();
                    _windowLauncher.Dispose();

                    _eventBus.UnsubscribeAll(this);
                }

                _disposed = true;
            }

        }

        private void Show()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    if (!_windowCreated)
                    {
                        _windowLauncher.CreateWindow<ScreenSaverWindow>(ScreensaverWindowName);
                        _windowCreated = true;
                    }

                    _windowLauncher.Show(ScreensaverWindowName);
                }
            }
        }

        private void Hide()
        {
            lock (_lock)
            {
                if (!_disposed && _windowCreated)
                {
                    _windowLauncher.Close(ScreensaverWindowName);
                    _windowCreated = false;
                }
            }
        }
    }
}