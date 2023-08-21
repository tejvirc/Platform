namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Models;
    using Monaco.UI.Common.Controls.Helpers;

    /// <summary>
    /// Interaction logic for GameLevelMappingView.xaml
    /// </summary>
    public partial class ProgressiveSetupView
    {
        public ProgressiveSetupView()
        {
            InitializeComponent();
        }

        private void SelectableLevelNameComboBox_OnDropDownClosed(object sender, EventArgs e)
        {
            UpdateSelectableLevelNameTooLong(sender as ComboBox);
        }

        private void SelectableLevelNameComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSelectableLevelNameTooLong(sender as ComboBox);
        }

        private void SelectableLevelNameComboBox_OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs _)
        {
            UpdateSelectableLevelNameTooLong(sender as ComboBox);
        }

        private static void UpdateSelectableLevelNameTooLong(ComboBox comboBox)
        {
            if (comboBox?.DataContext is not LevelModel context)
            {
                return;
            }

            var selectableLevelName = (
                string.IsNullOrWhiteSpace(comboBox.Text)
                    ? context.SelectableLevel?.Name
                    : comboBox.Text
            ) ?? "";

            var measuredTextWidth = TextMeasurementHelper.MeasureTextWidth(
                selectableLevelName,
                comboBox.FontFamily,
                comboBox.FontStyle,
                comboBox.FontWeight,
                comboBox.FontStretch,
                comboBox.FontSize,
                VisualTreeHelper.GetDpi(comboBox).PixelsPerDip);

            context.SelectableLevelNameTooLong = measuredTextWidth > comboBox.ActualWidth;
        }
    }
}
