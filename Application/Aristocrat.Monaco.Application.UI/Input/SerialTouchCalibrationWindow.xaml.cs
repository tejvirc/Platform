﻿namespace Aristocrat.Monaco.Application.UI.Input
{
    using Cabinet.Contracts;
    using Kernel;
    using Views;

    /// <summary>
    ///     Interaction logic for SerialTouchCalibrationWindow.xaml
    /// </summary>
    public partial class SerialTouchCalibrationWindow
    {
        private IDisplayDevice _monitor;

        public SerialTouchCalibrationWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
            Topmost = WindowToScreenMapper.GetFullscreen(ServiceManager.GetInstance().GetService<IPropertiesManager>());
        }

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
    }
}