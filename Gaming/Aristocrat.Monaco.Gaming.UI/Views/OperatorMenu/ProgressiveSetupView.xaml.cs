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

        private void UpdateSelectableLevelNameTooLong(ComboBox comboBox)
        {
            var context = comboBox?.DataContext as LevelModel;
            if (context is null)
            {
                return;
            }

            var measuredTextWidth = TextMeasurementHelper.MeasureTextWidth(
                comboBox.Text,
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
