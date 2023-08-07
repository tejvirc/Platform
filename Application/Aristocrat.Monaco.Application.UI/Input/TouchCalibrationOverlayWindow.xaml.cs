namespace Aristocrat.Monaco.Application.UI.Input
{
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;

    public partial class TouchCalibrationOverlayWindow
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public TouchCalibrationOverlayWindow()
            : this(ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public TouchCalibrationOverlayWindow(ICabinetDetectionService cabinetDetection)
        {
            InitializeComponent();
            Topmost = true;
            SetOverlay(cabinetDetection);
            AllowsTransparency = true;
        }

        private void SetOverlay(ICabinetDetectionService cabinetDetection)
        {
            var positions = cabinetDetection.ExpectedDisplayDevices
                .Select(x => x.Bounds).ToList();
            Top = positions.Min(x => x.Top);
            Left = positions.Min(x => x.Left);
            Width = positions.Max(x => x.Left + x.Width) - Left;
            Height = positions.Max(x => x.Top + x.Height) - Top;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;

            Logger.Debug($"Setting overlay with the Top={Top}, Left={Left}, Width={Width}, Height={Height}");
        }
    }
}
