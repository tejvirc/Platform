namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Reflection;
    using Application.UI.OperatorMenu;
    using Cabinet.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;

    public class BackgroundPreviewViewModel : OperatorMenuSaveViewModelBase
    {
        private string _backgroundImagePath;

        public BackgroundPreviewViewModel()
        {
            var cabinetService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            var displayDevice = cabinetService.GetDisplayDeviceByItsRole(DisplayRole.Main);

            if (displayDevice != null)
            {
                BackgroundPreviewHeight = displayDevice.Bounds.Height * 0.75;
            }
            if (cabinetService.Type == CabinetType.MarsX)
            {
                BackgroundPreviewHeight /= 2;
            }
        }

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set => SetProperty(ref _backgroundImagePath, value);
        }

        public double BackgroundPreviewHeight { get; }
    }
}