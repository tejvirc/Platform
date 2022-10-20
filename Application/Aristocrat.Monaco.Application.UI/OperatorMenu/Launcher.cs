namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using Kernel;
    using Monaco.UI.Common;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Interop;
    using log4net;
    using MVVM;
    using Vgt.Client12.Application.OperatorMenu;
    using Views;

    /// <summary>
    ///     Launcher extends the addin extension point for an operator menu
    ///     implementation, and is responsible for initiating the startup of the
    ///     operator menu.
    /// </summary>
    public sealed class Launcher : IOperatorMenu, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string WindowName = "OperatorMenu";

        private readonly object _lock = new object();

        private readonly IWpfWindowLauncher _windowLauncher;
        private readonly IPropertiesManager _properties;

        private bool _disposed;

        private bool _windowCreated;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Launcher" /> class.
        /// </summary>
        public Launcher()
        {
            _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _windowLauncher = ServiceManager.GetInstance().GetService<IWpfWindowLauncher>();
        }

        /// <summary>
        ///     Disposes the object.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _windowLauncher.Dispose();
            }
        }

        /// <summary>
        ///     Starts the WPF operator menu.
        /// </summary>
        public void Show()
        {
            Logger.Debug("Attempting to launch Operator Menu");

            lock (_lock)
            {
                if (_disposed)
                {
                    Logger.Error("Launcher disposed. Cannot launch Operator Menu.");
                    return;
                }

                if (!_windowCreated)
                {
                    Logger.Debug("Attempting to create new Operator Menu");
                    _windowLauncher.CreateWindow<MenuSelectionWindow>(WindowName);
                    _windowCreated = true;
                }
                else
                {
                    _windowLauncher.Show(WindowName, true);
                    Logger.Debug("Attempting to show existing Operator Menu");
                }

                // TODO: This is temporary code that will go away once SingleWindow is done.
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _windowLauncher.GetWindow(WindowName).Topmost = false;
                        _windowLauncher.GetWindow(WindowName).Topmost = true;
                    });
            }
        }

        /// <summary>
        ///     Hides the WPF operator menu.
        /// </summary>
        public void Hide()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    var topmost = _properties.GetValue("display", string.Empty) != "windowed";
                    _windowLauncher.Hide(WindowName, topmost);
                }
            }
        }

        /// <summary>
        ///     Closes the WPF operator menu.
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _windowLauncher.Close(WindowName);
                    _windowCreated = false;
                }
            }
        }

        /// <inheritdoc />
        public void Activate()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _windowLauncher.Activate(WindowName);
                }
            }
        }
    }
}