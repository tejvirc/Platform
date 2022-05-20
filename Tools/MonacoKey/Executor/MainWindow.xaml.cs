namespace Executor
{
    using Executor.VMs;
    using MahApps.Metro.Controls;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Visibility = Visibility.Hidden; // fixes WPF bug with window visibility binding
            DataContext = new ExecutorVM("Platform Key Executor " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            InitializeComponent();

            ((ExecutorVM)DataContext).Start();
        }

        private void LogBox_ScrollToEnd(object sender, TextChangedEventArgs e)
        {
            LogBox.ScrollToEnd();
        }

    }

    public class DebugReleaseStylePicker
    {
        #if DEBUG
        internal static readonly bool debug = true;
        #else
        internal static readonly bool debug=false;
        #endif

        public Style ReleaseStyle
        {
            get; set;
        }

        public Style DebugStyle
        {
            get; set;
        }

        public Style CurrentStyle
        {
            get
            {
                return debug ? DebugStyle : ReleaseStyle;
            }
        }
    }
}
