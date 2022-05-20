namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Cabinet.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    public class BackgroundPreviewViewModel : OperatorMenuSaveViewModelBase
    {
        private string _backgroundImagePath;

        public BackgroundPreviewViewModel()
        {
            var displayDevice = ServiceManager.GetInstance().GetService<ICabinetDetectionService>().GetDisplayDeviceByItsRole(DisplayRole.Main);

            if (displayDevice != null)
            {
                BackgroundPreviewHeight = displayDevice.Bounds.Height * 0.75;
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