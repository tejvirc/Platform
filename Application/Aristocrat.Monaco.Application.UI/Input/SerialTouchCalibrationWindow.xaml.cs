namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Windows;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.Touch;
    using Aristocrat.Monaco.Localization.Properties;
    using Cabinet.Contracts;
    using Kernel;
    using Views;

    /// <summary>
    ///     Interaction logic for SerialTouchCalibrationWindow.xaml
    /// </summary>
    public partial class SerialTouchCalibrationWindow
    {
        public delegate void SerialTouchCalibrationCompleteHandler(object sender, EventArgs e);

        private IDisplayDevice _monitor;

        public SerialTouchCalibrationWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
            Topmost = WindowToScreenMapper.GetFullscreen(ServiceManager.GetInstance().GetService<IPropertiesManager>());
            ContentView.Visibility = Visibility.Hidden;
        }

        public void BeginCalibrationTest()
        {
            SetCalibrationShow(true);

            var serialTouchService = ServiceManager.GetInstance().GetService<ISerialTouchService>();
            Status.Content = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrateModel), serialTouchService.Model);
            SetLowerLeftCrosshair(CalibrationCrosshairColors.Inactive);
            SetUpperRightCrosshair(CalibrationCrosshairColors.Inactive);

            serialTouchService.SendResetCommand(true);
        }

        public event SerialTouchCalibrationCompleteHandler CalibrationComplete;

        public SerialTouchCalibrationWindow NextCalibrationTest()
        {
            SetCalibrationShow(false);
            if (NextDevice != null)
            {
                NextDevice.BeginCalibrationTest();
            }
            else
            {
                CalibrationComplete?.Invoke(this, EventArgs.Empty);
            }

            return NextDevice;
        }

        public SerialTouchCalibrationWindow NextDevice { get; set; }

        public IDisplayDevice Monitor
        {
            get => _monitor;
            set
            {
                _monitor = value;
                var w = new WindowToScreenMapper(_monitor.Role, true);
                w.MapWindow(this);
            }
        }

        public void UpdateCalibration(SerialTouchCalibrationStatusEvent e)
        {
            if (!string.IsNullOrEmpty(e.ResourceKey))
            {
                if (!string.IsNullOrEmpty(e.Error))
                {
                    Status.Content = string.Format(Localizer.For(CultureFor.Operator).GetString(e.ResourceKey), e.Error);
                }
                else
                {
                    Status.Content = Localizer.For(CultureFor.Operator).GetString(e.ResourceKey);
                }
            }
            else
            {
                Status.Content = string.Empty;
            }

            SetLowerLeftCrosshair(e.CrosshairColorLowerLeft);
            SetUpperRightCrosshair(e.CrosshairColorUpperRight);
        }

        public void UpdateError(string error)
        {
            Error.Content = error;
        }

        private System.Windows.Media.SolidColorBrush GetCrosshairColor(CalibrationCrosshairColors crosshair)
        {
            var crosshairColor = System.Windows.Media.Brushes.Transparent;
            switch (crosshair)
            {
                case CalibrationCrosshairColors.Active:
                    crosshairColor = System.Windows.Media.Brushes.Black;
                    break;
                case CalibrationCrosshairColors.Acknowledged:
                    crosshairColor = System.Windows.Media.Brushes.Green;
                    break;
                case CalibrationCrosshairColors.Error:
                    crosshairColor = System.Windows.Media.Brushes.Red;
                    break;
            }

            return crosshairColor;
        }

        private void SetCalibrationShow(bool value)
        {
            ContentView.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetLowerLeftCrosshair(CalibrationCrosshairColors crosshair)
        {
            var brush = GetCrosshairColor(crosshair);
            CrosshairLowerLeftOuterEllipse.Stroke = brush;
            CrosshairLowerLeftInnerEllipse.Stroke = brush;
            CrosshairLowerLeftCrosshair1.Stroke = brush;
            CrosshairLowerLeftCrosshair2.Stroke = brush;
        }

        private void SetUpperRightCrosshair(CalibrationCrosshairColors crosshair)
        {
            var brush = GetCrosshairColor(crosshair);
            CrosshairUpperRightOuterEllipse.Stroke = brush;
            CrosshairUpperRightInnerEllipse.Stroke = brush;
            CrosshairUpperRightCrosshair1.Stroke = brush;
            CrosshairUpperRightCrosshair2.Stroke = brush;
        }
    }
}