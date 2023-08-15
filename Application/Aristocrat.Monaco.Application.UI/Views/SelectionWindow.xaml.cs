namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using Cabinet.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Common;
    using Monaco.UI.Common;
    using MVVM;

    /// <summary>
    ///     Interaction logic for SelectionWindow.xaml
    ///     This is just a navigation window that holds the configuration
    ///     wizard pages.
    /// </summary>
    public sealed partial class SelectionWindow : IDisposable
    {
        private readonly AutoConfigurator _autoConfigurator = new AutoConfigurator();
        private readonly BaseWindow _windowInfo = new BaseWindow();
        private readonly WindowToScreenMapper _screenMapper = new WindowToScreenMapper(DisplayRole.Main, showCursor: true);
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectionWindow" /> class.
        /// </summary>
        public SelectionWindow()
        {
            InitializeComponent();

            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            ServiceManager.GetInstance().AddServiceAndInitialize(_autoConfigurator);
            _eventBus.Subscribe<CloseConfigWindowEvent>(this, _ => CloseWindow());

            ContentElement = ConfigPage;
        }

        /// <summary>
        ///     Gets the software version.
        /// </summary>
        public string SoftwareVersion { get; private set; }

        /// <summary>
        ///     Gets the Demo Mode text.
        /// </summary>
        public string DemoModeText { get; private set; }

        /// <summary>
        ///     Gets or sets the window title
        /// </summary>
        public string WindowTitle
        {
            get => MainWindow.Title;
            set => MainWindow.Title = value;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }
        }

        private void CloseWindow()
        {
            MvvmHelper.ExecuteOnUI(Close);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var propertyManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            SoftwareVersion = (string)propertyManager.GetProperty(KernelConstants.SystemVersion, "0.0.0.0");
            DemoModeText = string.Empty;

            WindowState = WindowState.Normal;

            if (_windowInfo.IsWindowed)
            {
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.SingleBorderWindow;

                // check if the user set the display property on the bootstrap command line.
                Height = int.Parse(
                    (string)propertyManager.GetProperty(
                        Constants.WindowedScreenHeightPropertyName,
                        Constants.DefaultWindowedHeight),
                    CultureInfo.InvariantCulture);
                Width = int.Parse(
                    (string)propertyManager.GetProperty(
                        Constants.WindowedScreenWidthPropertyName,
                        Constants.DefaultWindowedWidth),
                    CultureInfo.InvariantCulture);
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
            }

            _screenMapper.MapWindow(this);

            Activate();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ServiceManager.GetInstance().RemoveService(_autoConfigurator);
            if (Content != null)
            {
                if (((FrameworkElement)Content).DataContext is IService service)
                {
                    ServiceManager.GetInstance().RemoveService(service);
                }
            }
            Dispose();
        }
    }
}
