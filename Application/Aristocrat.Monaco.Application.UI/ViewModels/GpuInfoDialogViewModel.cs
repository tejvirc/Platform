namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Hardware.Contracts.Display;
    using Kernel;
    using Monaco.UI.Common;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class GpuInfoDialogViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGpuDetailService _gpuDetailService;
        private readonly ITimer getTempTimer;
        private GpuInfo _graphicsCardInfo;
        private string _currentGPUTemp;

        public GpuInfoDialogViewModel()
        {
            _gpuDetailService = ServiceManager.GetInstance()
                .GetService<IGpuDetailService>();
            _graphicsCardInfo = _gpuDetailService.GraphicsCardInfo;
            TotalGpuRam = AppendIfNotNa(_graphicsCardInfo.TotalGpuRam, " mB");
            getTempTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(1) };
            getTempTimer.Tick += GetTempTimerOnElapsed;
            UsingIGpu = CheckIfGpuIsBeingUsed();
        }

        public string WarningText => "The IGPU is currently running instead of the discrete GPU.";

        public bool UsingIGpu { get; set; }

        public string TotalGpuRam { get; set; }

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
            get => _currentGPUTemp;

            set
            {
                if (_currentGPUTemp != value)
                {
                    _currentGPUTemp = value;
                    RaisePropertyChanged(nameof(CurrentGpuTemp));
                }
            }
        }

        protected override void OnLoaded()
        {
            getTempTimer.Start();
        }

        protected override void OnUnloaded()
        {
            getTempTimer.Stop();
        }

        private void GetTempTimerOnElapsed(object sender, EventArgs e)
        {
            CurrentGpuTemp = AppendIfNotNa(_gpuDetailService.GpuTemp, "°C");
        }

        private string AppendIfNotNa(string original, string toAdd)
        {
            return original == "N/A" ? original : original + toAdd;
        }

        private bool CheckIfGpuIsBeingUsed()
        {
            return GraphicsCardInfo.GpuFullName == _gpuDetailService.ActiveGpuName;
        }
    }
}