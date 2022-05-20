namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Windows;

    /// <summary>
    ///     Interaction logic for ReplayRecoveryView.xaml
    /// </summary>
    public partial class ReplayRecoveryView
    {
        /// <summary>
        /// Notifies listeners that the replay navigation bar has resized.
        /// </summary>
        public event SizeChangedEventHandler NavigationBarSizeChanged;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReplayRecoveryView" /> class.
        /// </summary>
        public ReplayRecoveryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Relays navigation bar changes to <see cref="NavigationBarSizeChanged"/> listeners.
        /// </summary>
        private void OnNavigationBarSizeChanged(object sender, SizeChangedEventArgs e)
        {
            NavigationBarSizeChanged?.Invoke(sender, e);
        }
    }
}
