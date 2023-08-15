namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using Kernel;
    using Monaco.Common;

    /// <summary>
    ///     Definition of the BaseWindow class.
    /// </summary>
    public class BaseWindow : Window
    {
        /// <summary>
        ///     Gets the release target screen size in pixels. Modify this value in the ResourceLibrary.xaml file.
        /// </summary>
        public int TargetScreenHeight { get; private set; }

        /// <summary>
        ///     The FrameworkElement containing the window's main content
        /// </summary>
        public FrameworkElement ContentElement { get; set; }

        /// <summary>
        ///     Gets a value indicating whether we are in windowed mode or running full screen.
        /// </summary>
        public bool IsWindowed
        {
            get
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var display = ((string)propertiesManager.GetProperty(
                    Constants.DisplayPropertyKey,
                    Constants.DisplayPropertyFullScreen))
                    .ToUpperInvariant();

                return display != Constants.DisplayPropertyFullScreen;
            }
        }

        /// <inheritdoc />
        protected override void OnInitialized(EventArgs e)
        {
            if (Resources != null)
            {
                TargetScreenHeight = (int)(Resources["TargetScreenHeight"] ?? 1080);
            }

            base.OnInitialized(e);
        }
    }
}