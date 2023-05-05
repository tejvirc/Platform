namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.Display;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class GpuInfoDialogViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGpuDetailService _gpuDetailService;
        private readonly ITimer _refreshTempTimer;
        private GpuInfo _graphicsCardInfo;
        private string _currentGpuTemp;

        public GpuInfoDialogViewModel()
        {
            _gpuDetailService = ServiceManager.GetInstance()
                .GetService<IGpuDetailService>();
            _graphicsCardInfo = _gpuDetailService.GraphicsCardInfo;
            GpuRam = AppendIfNotNa(_graphicsCardInfo.GpuRam, " GB"); //GigaByte is international and doesn't need translation?
            _refreshTempTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(1) };
            _refreshTempTimer.Tick += RefreshTemperature;
        }

        public string GpuRam { get; set; }

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

        public string CurrentGpuTemp
        {
            get => _currentGpuTemp;

            set
            {
                if (_currentGpuTemp != value)
                {
                    _currentGpuTemp = value;
                    RaisePropertyChanged(nameof(CurrentGpuTemp));
                }
            }
        }

        protected override void OnLoaded()
        {
            _refreshTempTimer.Start();
        }

        protected override void OnUnloaded()
        {
            _refreshTempTimer.Stop();
        }

        private void RefreshTemperature(object sender, EventArgs e)
        {
            CurrentGpuTemp = AppendIfNotNa(_gpuDetailService.GpuTemp, "°C"); //°C is international and doesn't need translation?
        }

        private string AppendIfNotNa(string original, string toAdd)
        {
            return original == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                ? original
                : original + toAdd;
        }
    }
}