namespace Aristocrat.Monaco.G2S.UI.Views
{
    /// <summary>
    ///     Interaction logic for Host Transcript View
    /// </summary>
    public partial class HostTranscriptsView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTranscriptsView" /> class.
        /// </summary>
        public HostTranscriptsView()
        {
            InitializeComponent();
        }

        private void CommsDevice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CommsDevice.SelectedItem = null;
        }
    }
}