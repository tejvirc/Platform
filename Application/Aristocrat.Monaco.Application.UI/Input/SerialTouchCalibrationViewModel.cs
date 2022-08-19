namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Windows;
    using Cabinet.Contracts;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class SerialTouchCalibrationViewModel : BaseViewModel
    {
        private const double CursorWidthCenterOffset = 40.0;
        private const double CursorHeightCenterOffset = 25.0;
        private const double CenterOffsetPercentage = 0.125;

        public SerialTouchCalibrationViewModel(IDisplayDevice displayDevice)
        {
            var heightOffset = displayDevice.WorkingArea.Height * CenterOffsetPercentage - CursorHeightCenterOffset;
            var widthOffset = displayDevice.WorkingArea.Width * CenterOffsetPercentage - CursorWidthCenterOffset;
            LeftCrossHairMargin = new Thickness(widthOffset, 0, 0, heightOffset);
            RightCrossHairMargin = new Thickness(0, heightOffset, widthOffset, 0);
        }

        public Thickness LeftCrossHairMargin { get; }

        public Thickness RightCrossHairMargin { get; }
    }
}