namespace Aristocrat.Monaco.UI.Common.Behaviors
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Interactivity;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Application.Contracts;
    using Common;
    using Kernel;
    using log4net;
    using Monaco.Common;
    using DpiChangedEventArgs = System.Windows.DpiChangedEventArgs;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using Size = System.Drawing.Size;

    /// <summary>
    ///     Attaching this behavior to a Window will make the window automatically adjust its DPI scaling for the display it is
    ///     contained in.
    ///     <remarks>
    ///         This Behavior works in both windowed (with the allowWindowedScaling override) and fullscreen modes.
    ///         This behavior will also ensure scaling is consistent, no matter what the System DPI scaling is set to.
    ///     </remarks>
    /// </summary>
    /// <seealso cref="Behavior" />
    public class HighDpiWindowBehavior : Behavior<Window>
    {
        private const int TargetResolutionWidth = ApplicationConstants.TargetResolutionWidth;
        private const int TargetResolutionHeight = ApplicationConstants.TargetResolutionHeight;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isWindowedMode;
        private double _scale;
        private double _systemDpiScale;
        private int _screenWidth;
        private int _screenHeight;

        private int? ViewportWidth
        {
            get
            {
                var contentPresenter = TreeHelper.FindChild<ContentPresenter>(AssociatedObject);
                if (contentPresenter == null)
                {
                    return null;
                }

                return (int?)Math.Round(contentPresenter.ActualWidth);
            }
        }

        private int? ViewportHeight
        {
            get
            {
                var contentPresenter = TreeHelper.FindChild<ContentPresenter>(AssociatedObject);
                if (contentPresenter == null)
                {
                    return null;
                }

                return (int?)Math.Round(contentPresenter.ActualHeight);
            }
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            var screenBounds = GetContainingScreenSize();
            _screenWidth = screenBounds.Width;
            _screenHeight = screenBounds.Height;

            _systemDpiScale = GetSystemDpiScale();

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var display = (string)propertiesManager.GetProperty(
                Constants.DisplayPropertyKey,
                Constants.DisplayPropertyFullScreen);
            _isWindowedMode = display.ToUpperInvariant() != Constants.DisplayPropertyFullScreen;

            ProcessWindowDpi(AssociatedObject);

            AssociatedObject.ContentRendered += OnContentRendered;
            AssociatedObject.SizeChanged += OnSizeChanged;

            AssociatedObject.LocationChanged += OnLocationChanged;
            AssociatedObject.DpiChanged += OnDpiChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.ContentRendered -= OnContentRendered;
            AssociatedObject.SizeChanged -= OnSizeChanged;
            AssociatedObject.LocationChanged -= OnLocationChanged;
            AssociatedObject.DpiChanged -= OnDpiChanged;
        }

        /// <summary>
        /// The current DPI-aware scale.
        /// </summary>
        public double Scale { get; private set; } = 1.0;

        private static double CalculateScale(double width1, double height1, double width2, double height2)
        {
            var xScale = width2 / width1;
            var yScale = height2 / height1;

            // "Scale to fit" algorithm
            var scale = Math.Min(xScale, yScale);
            if (scale.Equals(0))
            {
                scale = 1.0;
            }

            return scale;
        }

        private void ProcessWindowDpi(Window window)
        {
            _scale = CalculateScale(TargetResolutionWidth, TargetResolutionHeight, _screenWidth, _screenHeight);

            // Adjust scale to compensate for any DPI scaling the operating system is doing
            var adjustedScale = _scale / _systemDpiScale;

            if (window.Content is FrameworkElement content)
            {
                Scale = adjustedScale = _isWindowedMode && adjustedScale >= 1.0
                    ? 1.0               // Leave scaling as-is for developers working in windowed mode, unless specifically overridden
                    : adjustedScale;    // In fullscreen mode or windowed mode when the TargetResolution can't fit on screen, use adjusted scaling
                UpdateApplicationSurfaceLayout(content, adjustedScale);
            }
        }

        private void UpdateApplicationSurfaceLayout(FrameworkElement appSurface, double scale)
        {
            if (!ViewportWidth.HasValue || !ViewportHeight.HasValue)
            {
                return;
            }

            // Applying a transform to the ContentPresenter will scale the content up or down.
            var contentPresenter = TreeHelper.FindChild<ContentPresenter>(AssociatedObject);
            if (contentPresenter == null)
            {
                return;
            }

            contentPresenter.LayoutTransform = new ScaleTransform
            {
                CenterX = 0.0, CenterY = 0.0, ScaleX = scale, ScaleY = scale
            };

            // If the content is set to resize to content don't set the height and width it is already handled
            if (AssociatedObject.SizeToContent != SizeToContent.WidthAndHeight)
            {
                // Then the actual content inside the presenter is resized (not scaled) to fill the Window (Viewport)
                appSurface.Width = ViewportWidth.Value;
                appSurface.Height = ViewportHeight.Value;
                appSurface.VerticalAlignment = VerticalAlignment.Top;
                appSurface.HorizontalAlignment = HorizontalAlignment.Left;
            }

            // Trigger recenter of dialog window
            if (AssociatedObject is OperatorMenuDialogWindow)
            {
                AssociatedObject.BringIntoView();
            }

            Logger.Info($"[{AssociatedObject.Title}]: Calculated Scale={_scale}, " +
                        $"System DPI Scale={_systemDpiScale}, " +
                        $"Adjusted Scale={scale}, " +
                        $"Content Size=({appSurface.Width}x{appSurface.Height}), " +
                        $"AssociatedObject Size=({AssociatedObject.Width}x{AssociatedObject.Height}), " +
                        $"SizeToContent={AssociatedObject.SizeToContent}, " +
                        $"Screen Size=({_screenWidth}x{_screenHeight})");
        }

        /// <summary>
        ///     Gets the system dpi for the particular display that is containing the Window this behavior is attached to.
        /// </summary>
        private double GetSystemDpiScale()
        {
            return PresentationSource.FromVisual(AssociatedObject)
                ?.CompositionTarget
                ?.TransformToDevice.M11 ?? 1.0;
        }

        private Size GetContainingScreenSize()
        {
            if (_isWindowedMode &&
                AssociatedObject is not OperatorMenuDialogWindow &&
                (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
                    .GetProperty(Constants.AllowWindowedScalingKey, false))
            {
                return new Size((int)AssociatedObject.Width, (int)AssociatedObject.Height);
            }

            var containingScreen = Screen.FromHandle(new WindowInteropHelper(AssociatedObject).Handle);
            var screenSize = new Size(containingScreen.Bounds.Width, containingScreen.Bounds.Height);
            return screenSize;
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            if (AssociatedObject == null)
            {
                return;
            }

            // This forces the window to re-valuate its layout. It covers certain edge cases
            // where the Window does not internally re-valuate layout like it normally does.
            // Visually, the resizing of the window by 1px is virtually unnoticeable. If a
            // better/proper fix is found, this can be removed. 
            AssociatedObject.Width -= 1;
            AssociatedObject.Width += 1;
        }

        /// <summary>
        ///     "Occurs when either the ActualHeight or the ActualWidth properties change value on this element."
        ///     <para>https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.sizechanged</para>
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProcessWindowDpi(AssociatedObject);
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            var screenSize = GetContainingScreenSize();

            _screenWidth = screenSize.Width;
            _screenHeight = screenSize.Height;
            _systemDpiScale = GetSystemDpiScale();

            ProcessWindowDpi(AssociatedObject);
        }

        private void OnDpiChanged(object sender, DpiChangedEventArgs e)
        {
            ProcessWindowDpi(AssociatedObject);
        }
    }
}