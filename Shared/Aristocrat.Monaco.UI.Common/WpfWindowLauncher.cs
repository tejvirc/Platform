namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Hardware.Contracts.Touch;
    using Kernel;
    using log4net;
    using MahApps.Metro.Controls;

    /// <summary>
    ///     The services for creating windows on a single UI thread.
    /// </summary>
    public sealed class WpfWindowLauncher : IWpfWindowLauncher
    {
        private const string UiThreadName = "UiThread";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();
        private readonly ManualResetEvent _waitForAppCreated = new ManualResetEvent(false);
        private readonly Dictionary<string, WindowInfo> _windows = new Dictionary<string, WindowInfo>();

        private bool _disposed;
        private Thread _uiThread;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Shutdown(false);

            _waitForAppCreated.Close();

            Logger.Debug("Disposed");

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IWpfWindowLauncher) };

        /// <inheritdoc />
        public void Initialize()
        {
            CreateUiThread();
            Application.Current.Dispatcher.Invoke(() => DisableWPFTabletSupport());
        }

        /// <inheritdoc />
        public void CreateWindow<T>(string name, bool isDialog = false)
            where T : Window, new()
        {
            var windowInfo = new WindowInfo
            {
                Name = name,
                IsDialog = isDialog
            };

            lock (_lock)
            {
                if (_disposed)
                {
                    Logger.Error($"WpfWindowLauncher disposed, cannot create window with name: {name}");
                    return;
                }

                if (_windows.ContainsKey(name))
                {
                    throw new ArgumentException($"A window already exists with name: {name}");
                }

                var windowType = typeof(T);
                Logger.Debug($"Creating window {name} of type: {windowType.Name}");

                windowInfo.WindowLoadedResetEvent = new ManualResetEvent(false);
                _windows.Add(name, windowInfo);
            }

            // Invoke blocks until the UI thread executes the action. So when showing
            // a dialog, it blocks until the dialog is closed which is what we want.
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Send,
                new Action(() => InternalCreateWindow<T>(windowInfo)));

            if (isDialog)
            {
                windowInfo.WindowLoadedResetEvent.WaitOne();
            }
        }

        /// <inheritdoc />
        public Window GetWindow(string name)
        {
            lock (_lock)
            {
                _windows.TryGetValue(name, out var windowInfo);
                return windowInfo?.Window;
            }
        }

        /// <inheritdoc />
        public void Close(string name)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    Logger.Info($"Intentionally closing window with name: {name}");
                    windowInfo.Window.Dispatcher.Invoke(DispatcherPriority.Send, new Action(windowInfo.Window.Close));
                    _windows.Remove(name);
                }
            }
        }

        /// <inheritdoc />
        public void Show(string name, bool topmost = false)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    Logger.Error($"WpfWindowLauncher disposed, cannot show window with name: {name}");
                    return;
                }

                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    Logger.Info($"Showing window with name: {name}");
                    windowInfo.Window.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new Action(() => windowInfo.Window.Show()));

                    if (topmost)
                    {
                        windowInfo.Window.Dispatcher.Invoke(
                            DispatcherPriority.Send,
                            new Action(() => windowInfo.Window.Topmost = true));
                    }
                }
                else
                {
                    Logger.Error($"Cannot find window with name: {name}");
                }
            }
        }

        /// <inheritdoc />
        public void Hide(string name, bool topmost = false)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    Logger.Info($"Hiding window with name: {name}");
                    if (topmost)
                    {
                        windowInfo.Window.Dispatcher.Invoke(
                            DispatcherPriority.Send,
                            new Action(
                                () =>
                                {
                                    windowInfo.Window.Topmost = false;
                                    NativeMethods.SendWindowToBack(windowInfo.Window);
                                }));
                    }
                    else
                    {
                        windowInfo.Window.Dispatcher.Invoke(
                            DispatcherPriority.Send,
                            new Action(() => windowInfo.Window.Hide()));
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Activate(string name)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    Logger.Info($"Activating window with name: {name}");
                    windowInfo.Window.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new Action(() => windowInfo.Window.Activate()));
                }
            }
        }

        /// <inheritdoc />
        public Visibility GetWindowVisibility(string name)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    return windowInfo.Window.Visibility;
                }
            }

            return Visibility.Collapsed;
        }

        /// <inheritdoc />
        public WindowState GetWindowState(string name)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    var state = WindowState.Normal;
                    windowInfo.Window.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new Action(() => state = windowInfo.Window.WindowState));
                    return state;
                }
            }

            return WindowState.Normal;
        }

        /// <inheritdoc />
        public void SetWindowState(string name, WindowState state)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(name, out var windowInfo))
                {
                    windowInfo.WindowLoadedResetEvent.WaitOne();
                    Logger.Info($"Setting window {name} to have state {state}");
                    windowInfo.Window.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new Action(() => windowInfo.Window.WindowState = state));
                }
            }
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            Logger.Debug("Shutdown invoked");

            Shutdown(true);
        }

        private void CreateUiThread()
        {
            if (Application.Current != null)
            {
                Logger.Info("Application instance already exists");
                return;
            }

            if (_uiThread == null)
            {
                _uiThread = new Thread(
                    () =>
                    {
                        if (Application.Current == null)
                        {
                            Logger.Info("Creating new Application instance...");

                            var app = new MonacoApplication { ShutdownMode = ShutdownMode.OnExplicitShutdown };

                            _waitForAppCreated.Set();

                            app.Run();
                        }
                    })
                {
                    Name = UiThreadName,
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };

                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();

                _waitForAppCreated.WaitOne();

                Logger.Debug($"Started UI window thread with ID: {_uiThread.ManagedThreadId}");
            }
        }

        private void InternalCreateWindow<T>(WindowInfo windowInfo)
            where T : Window, new()
        {
            Logger.Info($"Creating window of type: {typeof(T).Name}");

            var window = new T();
            windowInfo.Window = window;

            window.Name = windowInfo.Name;

            if (window.Name.Equals("ConfigWizard"))
            {
                var serialTouchService = ServiceManager.GetInstance().TryGetService<ISerialTouchService>();
                if (serialTouchService != null && serialTouchService.Initialized)
                {
                    Logger.Debug($"{window.Name} opened, reconnecting serial touch");
                    serialTouchService.Reconnect();
                }
            }

            window.ShowActivated = true;

            // Turn off stylus/touch graphics.
            DisableStylus(window);
            
            window.Loaded += (sender, e) =>
            {
                windowInfo.WindowLoadedResetEvent.Set();
            };

            window.Closed += WindowOnClosed;

            if (windowInfo.IsDialog)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }
        }

        private void WindowOnClosed(object sender, EventArgs eventArgs)
        {
            if (sender is Window window)
            {
                Logger.Debug($"{window.Name} window closed");

                Logger.Info($"Disposing window of name: {window.Name}");
                var disposable = window as IDisposable;
                disposable?.Dispose();
            }
        }

        private void Shutdown(bool closeApplication)
        {
            lock (_lock)
            {
                var windowsToClose = _windows.ToList();
                foreach (var keyValuePair in windowsToClose)
                {
                    Close(keyValuePair.Key);
                }
            }

            if (closeApplication && Application.Current != null)
            {
                Logger.Info("Shutting down the the WPF Application");
                Application.Current.Invoke(() => Application.Current.Shutdown());
            }
        }

        private class WindowInfo
        {
            /// <summary>Gets or sets the name of the window</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the window object</summary>
            public Window Window { get; set; }

            /// <summary>Gets or sets a value indicating whether or not the window is a dialog</summary>
            public bool IsDialog { get; set; }

            /// <summary>Gets or sets the event that is signaled when the Window is loaded</summary>
            public ManualResetEvent WindowLoadedResetEvent { get; set; }
        }

        /// <summary>
        /// DisableStylus turns off several stylus features for the window
        /// </summary>
        /// <param name="window"></param>
        public static void DisableStylus(Window window)
        {
            Stylus.SetIsTapFeedbackEnabled(window, false);
            Stylus.SetIsTouchFeedbackEnabled(window, false);
            Stylus.SetIsPressAndHoldEnabled(window, false);
            Stylus.SetIsFlicksEnabled(window, false);
        }

        private void DisableWPFTabletSupport()
        {
            // Get a collection of the tablet devices for this window.
            TabletDeviceCollection devices = Tablet.TabletDevices;

            if (devices.Count > 0)
            {
                // Get the Type of InputManager.  
                Type inputManagerType = typeof(InputManager);

                // Call the StylusLogic method on the InputManager.Current instance.  
                object stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, InputManager.Current, null);

                if (stylusLogic != null)
                {
                    //  Get the type of the stylusLogic returned from the call to StylusLogic.  
                    Type stylusLogicType = stylusLogic.GetType();

                    // Loop until there are no more devices to remove.  
                    while (devices.Count > 0)
                    {
                        // Remove the first tablet device in the devices collection.  
                        stylusLogicType.InvokeMember("OnTabletRemoved",
                            BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, stylusLogic, new object[] { (uint)0 });
                    }
                }
            }
        }
    }
}