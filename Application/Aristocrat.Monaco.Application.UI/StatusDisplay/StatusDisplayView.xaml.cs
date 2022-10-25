namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Forms;
    using Hardware.Contracts.Display;

    public sealed partial class StatusDisplayView : IDisposable
    {
        private readonly IDictionary<DisplayableMessageClassification, DisplayBox> _displayBoxes;
        private readonly object _mutex = new object();

        private bool _disposed;

        private StatusDisplayMessageHandler _statusDisplayMessageHandler;
        private IEventBus _eventBus;

        public StatusDisplayView()
        {
            InitializeComponent();

            _displayBoxes = new Dictionary<DisplayableMessageClassification, DisplayBox>
            {
                { DisplayableMessageClassification.HardError, HardErrorBox },
                { DisplayableMessageClassification.SoftError, SoftErrorBox },
                { DisplayableMessageClassification.Informative, InformativeBox },
                { DisplayableMessageClassification.Diagnostic, InformativeBox }
            };

            _statusDisplayMessageHandler = new StatusDisplayMessageHandler(this);
        }

        public void Dispose()
        {
            lock (_mutex)
            {
                if (_disposed)
                {
                    return;
                }

                if (_statusDisplayMessageHandler != null)
                {
                    _statusDisplayMessageHandler.Dispose();
                    _statusDisplayMessageHandler = null;
                }

                _eventBus?.UnsubscribeAll(this);
                _eventBus = null;

                _disposed = true;
            }
        }

        public void DisplayStatus(string message)
        {
            lock (_mutex)
            {
                StatusBox.AddToDisplay(message);
            }
        }

        public void DisplayMessage(DisplayableMessage message)
        {
            lock (_mutex)
            {
                if (_displayBoxes.TryGetValue(message.Classification, out var displayBox))
                {
                    displayBox.AddToDisplay(message.Message);
                }
            }
        }

        public void ClearMessages()
        {
            lock (_mutex)
            {
                foreach (var displayBox in _displayBoxes.Values)
                {
                    displayBox.RemoveAll();
                }
            }
        }

        public void RemoveMessage(DisplayableMessage message)
        {
            lock (_mutex)
            {
                if (_displayBoxes.TryGetValue(message.Classification, out var displayBox))
                {
                    displayBox.RemoveFromDisplay(message.Message);
                }
            }
        }

        private void RestoreWindowPlacement()
        {
            WindowState = WindowState.Normal;

            if (IsWindowed)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;

                // check if the user set the display property on the bootstrap command line.
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                Width = int.Parse(
                    (string)propertiesManager.GetProperty(WindowedScreenWidthPropertyName, DefaultWindowedWidth),
                    CultureInfo.CurrentCulture);
                Height = int.Parse(
                    (string)propertiesManager.GetProperty(WindowedScreenHeightPropertyName, DefaultWindowedHeight),
                    CultureInfo.CurrentCulture);
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;

                if (SystemParameters.PrimaryScreenHeight > SystemParameters.PrimaryScreenWidth)
                {
                    Top = SystemParameters.PrimaryScreenHeight / 2;
                    Left = -1;
                    Height = SystemParameters.PrimaryScreenHeight / 2;
                    Width = SystemParameters.PrimaryScreenWidth;
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.None;
                }
                else
                {
                    WindowState = WindowState.Normal;

                    // Needed for Windows to re-evaluate what screen now has the origin point
                    // http://www.codewrecks.com/blog/index.php/2013/01/05/open-a-window-in-fullscreen-on-a-specific-monitor-in-wpf/
                    Top += 2;
                    Left += 2;

                    Top = Screen.PrimaryScreen.WorkingArea.Top;
                    Left = Screen.PrimaryScreen.WorkingArea.Left;
                    Width = Screen.PrimaryScreen.WorkingArea.Width;
                    Height = Screen.PrimaryScreen.WorkingArea.Height;

                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            RestoreWindowPlacement();
            Activate();

            _eventBus = ServiceManager.GetInstance()?.TryGetService<IEventBus>();
            _eventBus?.Subscribe<DisplayConnectedEvent>(this, HandleEvent);
            _eventBus?.Subscribe<DisplayDisconnectedEvent>(this, HandleEvent);
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            lock (_mutex)
            {
                if (_statusDisplayMessageHandler != null)
                {
                    _statusDisplayMessageHandler.Dispose();
                    _statusDisplayMessageHandler = null;
                }
            }
        }

        private void HandleEvent(DisplayConnectedEvent evt)
        {
            Dispatcher?.Invoke(RestoreWindowPlacement);
        }

        private void HandleEvent(DisplayDisconnectedEvent evt)
        {
            Dispatcher?.Invoke(RestoreWindowPlacement);
        }
    }
}