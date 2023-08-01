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
        public static readonly string DefaultWindowedHeight = "1080";

        /// <summary>The default screen width in pixels.</summary>
        public static readonly string DefaultWindowedWidth = "1920";

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
                var simulatedCabinet = (string)propertiesManager.GetProperty("simulateCabinet", string.Empty);
                if (!string.IsNullOrEmpty(simulatedCabinet))
                {
                    return true;
                }

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
                TargetScreenHeight = (int)(Resources["TargetScreenHeight"] ?? int.Parse(DefaultWindowedHeight));
            }

            base.OnInitialized(e);
        }
    }
}