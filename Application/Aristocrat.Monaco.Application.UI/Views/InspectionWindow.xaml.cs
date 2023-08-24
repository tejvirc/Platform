namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using Application.Contracts.Localization;
    using Cabinet.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;

    /// <summary>
    ///     Interaction logic for InspectionWindow.xaml
    ///     This is just a navigation window that holds the inspection
    ///     wizard pages.
    /// </summary>
    public sealed partial class InspectionWindow : BaseWindow
    {
        private readonly BaseWindow _windowInfo = new BaseWindow();
        private readonly WindowToScreenMapper _screenMapper = new WindowToScreenMapper(DisplayRole.Main, swapRoles: true, showCursor: true);

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionWindow" /> class.
        /// </summary>
        public InspectionWindow()
        {
            InitializeComponent();

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

        private void Window_Initialized(object sender, EventArgs e)
        {
            var propertyManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            SoftwareVersion = (string)propertyManager.GetProperty(KernelConstants.SystemVersion, "0.0.0.0");
            DemoModeText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Inspection);

            WindowState = WindowState.Normal;

            if (_windowInfo.IsWindowed)
            {
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.SingleBorderWindow;

                // check if the user set the display property on the bootstrap command line.
                Height = int.Parse(
                    (string)propertyManager.GetProperty(
                        BaseWindow.WindowedScreenHeightPropertyName,
                        BaseWindow.DefaultWindowedHeight),
                    CultureInfo.InvariantCulture);
                Width = int.Parse(
                    (string)propertyManager.GetProperty(
                        BaseWindow.WindowedScreenWidthPropertyName,
                        BaseWindow.DefaultWindowedWidth),
                    CultureInfo.InvariantCulture);
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;

                _screenMapper.MapWindow(this);
            }

            Activate();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Content != null)
            {
                if (((FrameworkElement)Content).DataContext is IService service)
                {
                    ServiceManager.GetInstance().RemoveService(service);
                }
            }
        }
    }
}
