namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Timers;
    using Hardware.Contracts.Display;
using Aristocrat.Monaco.Kernel;

    [CLSCompliant(false)]
    public class GpuInfoDialogViewModel : OperatorMenuSaveViewModelBase
    {
        private GpuInfo _graphicsCardInfo;
        private string _currentGPUTemp;
        private static System.Timers.Timer getTempTimer;

        protected override void OnLoaded()
        {
            getTempTimer.Start();
        }

        protected override void OnUnloaded()
        {
            getTempTimer.Stop();
        }

        public GpuInfoDialogViewModel()
        {
            _graphicsCardInfo = ServiceManager.GetInstance()
                .GetService<IDisplayService>()
                .GraphicsCardInfo;
            RAM = _graphicsCardInfo.TotalGpuRam + " mB";
            getTempTimer = new Timer(1000);
            getTempTimer.Elapsed += GetTempTimerOnElapsed;
            getTempTimer.AutoReset = true;
            
        }

        private void GetTempTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentGPUTemp = ServiceManager.GetInstance()
                .GetService<IDisplayService>()
                .GpuTemp + "°C";
        }

        public string RAM { get; set; }
        public GpuInfo GraphicsCardInfo
        {
            get => _graphicsCardInfo;

            set
            {
                if (_graphicsCardInfo != value)
                {
                    _graphicsCardInfo = value;
                    RaisePropertyChanged(nameof(GraphicsCardInfo));
                }
            }
        }

        public string CurrentGPUTemp
        {
            get => _currentGPUTemp;

            set
            {
                if (_currentGPUTemp != value)
                {
                    _currentGPUTemp = value;
                    RaisePropertyChanged(nameof(CurrentGPUTemp));
                }
            }
        }
    }
}
