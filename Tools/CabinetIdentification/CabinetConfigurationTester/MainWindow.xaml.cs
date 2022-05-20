namespace CabinetConfigurationTester
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using Aristocrat.Cabinet;
    using Microsoft.Win32;
    using TouchDevice = Aristocrat.Cabinet.TouchDevice;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DeleteDetectedDisplay = new ActionCommand(() =>
            {
                ViewModel.DetectedDisplayDevices.Remove(DetectedDisplays.SelectedItem as DisplayDevice);
                DetectedDisplays.SelectedItem = null;
            });
            DeleteDetectedTouchDevices = new ActionCommand(() =>
            {
                ViewModel.DetectedTouchDevices.Remove(DetectedTouchDevices.SelectedItem as TouchDevice);
                DetectedTouchDevices.SelectedItem = null;
            });
        }

        public ActionCommand DeleteDetectedDisplay { get; }
        public ActionCommand DeleteDetectedTouchDevices { get; }

        public CabinetViewModel ViewModel => DataContext as CabinetViewModel;

        private void AddDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddToDetectedDisplays();
        }

        private void AddTouchButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddToDetectedTouch();
        }

        private void DetectDevices_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DetectDevices();
        }

        private void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.Identify();
        }

        private void DetectAndApplySettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DetectAndApplySettings();
        }

        private void SaveCabinetFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, ViewModel.CabinetXml);
            }
        }

        private void ClearDetectedDevices_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DetectedDisplayDevices.Clear();
            ViewModel?.DetectedTouchDevices.Clear();
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshStatus();
        }

        public class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            protected virtual void OnCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}