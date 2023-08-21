namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Windows;
    using Cabinet.Contracts;
    using Kernel;
    using Views;

    /// <summary>
    ///     Interaction logic for TouchCalibrationWindow.xaml
    /// </summary>
    public partial class TouchCalibrationWindow
    {
        public delegate void CalibrationCompleteHandler(object sender, EventArgs e);

        private IDisplayDevice _monitor;

        public TouchCalibrationWindow()
        {
            InitializeComponent();
            Topmost = WindowToScreenMapper.GetFullscreen(ServiceManager.GetInstance().GetService<IPropertiesManager>());
            ContentView.Visibility = Visibility.Hidden;
        }

        public TouchCalibrationWindow NextDevice { get; set; }

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

        private bool CalibrationShow
        {
            get => ContentView.Visibility == Visibility.Visible;
            set => ContentView.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        public void BeginCalibrationTest()
        {
            if (!CalibrationShow)
            {
                CalibrationShow = true;
            }
        }

        public TouchCalibrationWindow NextCalibrationTest()
        {
            if (CalibrationShow)
            {
                CalibrationShow = false;
            }

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

        public event CalibrationCompleteHandler CalibrationComplete;
    }
}