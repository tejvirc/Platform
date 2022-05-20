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
        /// <summary>The default screen height in pixels.</summary>
        public static readonly string DefaultWindowedHeight = "768";

        /// <summary>The default screen width in pixels.</summary>
        public static readonly string DefaultWindowedWidth = "1024";

        /// <summary>
        ///     The property name from command line arguments for the windowed screen width
        /// </summary>
        public static readonly string WindowedScreenWidthPropertyName = "width";

        /// <summary>
        ///     The property name from command line arguments for the windowed screen height
        /// </summary>
        public static readonly string WindowedScreenHeightPropertyName = "height";

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