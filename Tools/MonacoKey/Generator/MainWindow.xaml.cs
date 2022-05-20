namespace Generator
{
    using Common.Models;
    using Generator.VMs;
    using MahApps.Metro.Controls;
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;

    // https://www.codeproject.com/Articles/60579/A-USB-Library-to-Detect-USB-Devices
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Title = "Platform Key Generator " + version;
            InitializeComponent();
            DataContext = new GeneratorVM(version);
            InitializeUsbInsertRemoveDetection();
        }

        private void LogBox_ScrollToEnd(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
                return;

            GeneratorVM vm = ((GeneratorVM)DataContext);
            foreach (USBKey key in vm.USBKeys)
                key.Enable = true;
        }
        private void UnselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
                return;

            GeneratorVM vm = ((GeneratorVM)DataContext);
            foreach (USBKey key in vm.USBKeys)
            {
                key.Enable = false;
            }
        }

        #region USB Insert/Remove Detection
        private USBClass USBPort;

        private void InitializeUsbInsertRemoveDetection()
        {
            USBPort = new USBClass();
            USBPort.USBDeviceAttached += new USBClass.USBDeviceEventHandler(USBPort_USBDeviceAttached);
            USBPort.USBDeviceRemoved += new USBClass.USBDeviceEventHandler(USBPort_USBDeviceRemoved);
        }

        private void USBPort_USBDeviceAttached(object sender, USBClass.USBDeviceEventArgs e)
        {
            ((GeneratorVM)DataContext).UpdateAsync();
        }

        private void USBPort_USBDeviceRemoved(object sender, USBClass.USBDeviceEventArgs e)
        {
            ((GeneratorVM)DataContext).UpdateAsync();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            USBPort.ProcessWindowsMessage(msg, wParam, lParam, ref handled);
            return IntPtr.Zero;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
            USBPort.RegisterForDeviceChange(true, source.Handle);
        }
        #endregion
    }
}
