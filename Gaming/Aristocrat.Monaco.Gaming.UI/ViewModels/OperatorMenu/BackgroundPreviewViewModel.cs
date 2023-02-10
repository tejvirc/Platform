namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using System.Windows;

    public class BackgroundPreviewViewModel : OperatorMenuSaveViewModelBase
    {
        private string _backgroundImagePath;
        private readonly double _scaleFactor = 0.75;

        public BackgroundPreviewViewModel()
        {
            BackgroundPreviewHeight = SystemParameters.PrimaryScreenHeight * _scaleFactor;
            BackgroundPreviewWidth = SystemParameters.PrimaryScreenWidth * _scaleFactor;
            Logger.Debug($"Screen Size: {BackgroundPreviewWidth}x{BackgroundPreviewHeight}");
        }

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set => SetProperty(ref _backgroundImagePath, value);
        }

        public double BackgroundPreviewHeight { get; }
        public double BackgroundPreviewWidth { get; }
    }
}