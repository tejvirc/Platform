namespace Aristocrat.Monaco.G2S.UI.Views
{
    /// <summary>
    ///     Interaction logic for HostConfigurationView.xaml
    /// </summary>
    public partial class HostConfigurationView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationView" /> class.
        /// </summary>
        public HostConfigurationView()
        {
            InitializeComponent();
        }

        private void CommsDevice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CommsDevice.SelectedItem = null;
        }
    }
}
