namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Cabinet.Contracts;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Localization;
    using Hardware.Contracts.Touch;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class SerialTouchCalibrationViewModel : ObservableObject
    {
        private readonly string _modelName;
        private const double CursorWidthCenterOffset = 40.0;
        private const double CursorHeightCenterOffset = 25.0;
        private const double CenterOffsetPercentage = 0.125;

        private string _status;
        private bool _visibility;
        private SolidColorBrush _lowerLeftOuterEllipseStroke = Brushes.Transparent;
        private SolidColorBrush _lowerLeftInnerEllipseStroke = Brushes.Transparent;
        private SolidColorBrush _lowerLeftCrosshair1Stroke = Brushes.Transparent;
        private SolidColorBrush _lowerLeftCrosshair2Stroke = Brushes.Transparent;
        private SolidColorBrush _upperRightOuterEllipseStroke = Brushes.Transparent;
        private SolidColorBrush _upperRightInnerEllipseStroke = Brushes.Transparent;
        private SolidColorBrush _upperRightCrosshair1Stroke = Brushes.Transparent;
        private SolidColorBrush _upperRightCrosshair2Stroke = Brushes.Transparent;
        private string _error;

        public SerialTouchCalibrationViewModel(IDisplayDevice displayDevice, string modelName)
        {
            _modelName = modelName;
            var heightOffset = displayDevice.WorkingArea.Height * CenterOffsetPercentage - CursorHeightCenterOffset;
            var widthOffset = displayDevice.WorkingArea.Width * CenterOffsetPercentage - CursorWidthCenterOffset;
            LeftCrossHairMargin = new Thickness(widthOffset, 0, 0, heightOffset);
            RightCrossHairMargin = new Thickness(0, heightOffset, widthOffset, 0);
        }

        public Thickness LeftCrossHairMargin { get; }

        public Thickness RightCrossHairMargin { get; }

        public string Status

        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public bool Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        public SolidColorBrush LowerLeftOuterEllipseStroke
        {
            get => _lowerLeftOuterEllipseStroke;
            set => SetProperty(ref _lowerLeftOuterEllipseStroke, value);
        }

        public SolidColorBrush LowerLeftInnerEllipseStroke
        {
            get => _lowerLeftInnerEllipseStroke;
            set => SetProperty(ref _lowerLeftInnerEllipseStroke, value);
        }

        public SolidColorBrush LowerLeftCrosshair1Stroke
        {
            get => _lowerLeftCrosshair1Stroke;
            set => SetProperty(ref _lowerLeftCrosshair1Stroke, value);
        }

        public SolidColorBrush LowerLeftCrosshair2Stroke
        {
            get => _lowerLeftCrosshair2Stroke;
            set => SetProperty(ref _lowerLeftCrosshair2Stroke, value);
        }

        public SolidColorBrush UpperRightOuterEllipseStroke
        {
            get => _upperRightOuterEllipseStroke;
            set => SetProperty(ref _upperRightOuterEllipseStroke, value);
        }

        public SolidColorBrush UpperRightInnerEllipseStroke
        {
            get => _upperRightInnerEllipseStroke;
            set => SetProperty(ref _upperRightInnerEllipseStroke, value);
        }

        public SolidColorBrush UpperRightCrosshair1Stroke
        {
            get => _upperRightCrosshair1Stroke;
            set => SetProperty(ref _upperRightCrosshair1Stroke, value);
        }

        public SolidColorBrush UpperRightCrosshair2Stroke
        {
            get => _upperRightCrosshair2Stroke;
            set => SetProperty(ref _upperRightCrosshair2Stroke, value);
        }

        public void UpdateCalibration(SerialTouchCalibrationStatusEvent e)
        {
            if (!string.IsNullOrEmpty(e.ResourceKey))
            {
                Status = !string.IsNullOrEmpty(e.Error)
                    ? string.Format(Localizer.For(CultureFor.Operator).GetString(e.ResourceKey), e.Error)
                    : Localizer.For(CultureFor.Operator).GetString(e.ResourceKey);
            }

            SetLowerLeftCrosshair(e.CrosshairColorLowerLeft);
            SetUpperRightCrosshair(e.CrosshairColorUpperRight);
        }

        public void BeginCalibrationTest()
        {
            Visibility = true;
            Status = string.Format(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrateModel),
                _modelName);
        }

        public void CompleteCalibrationTest()
        {
            Visibility = false;
        }

        public void UpdateError(string error) => Error = error;

        private static SolidColorBrush GetCrosshairColor(CalibrationCrosshairColors crosshair)
        {
            return crosshair switch
            {
                CalibrationCrosshairColors.Active => Brushes.Black,
                CalibrationCrosshairColors.Acknowledged => Brushes.Green,
                CalibrationCrosshairColors.Error => Brushes.Red,
                _ => Brushes.Transparent
            };
        }

        private void SetLowerLeftCrosshair(CalibrationCrosshairColors crosshair)
        {
            var brush = GetCrosshairColor(crosshair);
            LowerLeftOuterEllipseStroke = brush;
            LowerLeftInnerEllipseStroke = brush;
            LowerLeftCrosshair1Stroke = brush;
            LowerLeftCrosshair2Stroke = brush;
        }

        private void SetUpperRightCrosshair(CalibrationCrosshairColors crosshair)
        {
            var brush = GetCrosshairColor(crosshair);
            UpperRightOuterEllipseStroke = brush;
            UpperRightInnerEllipseStroke = brush;
            UpperRightCrosshair1Stroke = brush;
            UpperRightCrosshair2Stroke = brush;
        }
    }
}