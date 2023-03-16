namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Hardware.Contracts.Display;
using Aristocrat.Monaco.Kernel;

    [CLSCompliant(false)]
    public class GpuInfoDialogViewModel : OperatorMenuSaveViewModelBase
    {
        private GpuInfo _graphicsCardInfo;

        public GpuInfoDialogViewModel()
        {
            _graphicsCardInfo = ServiceManager.GetInstance()
                .GetService<IDisplayService>()
                .GraphicsCardInfo;
            RAM = _graphicsCardInfo.TotalGpuRam + " mB";
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
        

        private void GetStats()
        {
           
        }
    }
}
