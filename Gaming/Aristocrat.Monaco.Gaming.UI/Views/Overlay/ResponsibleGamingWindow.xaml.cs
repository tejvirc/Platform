namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using ViewModels;

    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class ResponsibleGamingWindow
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="ResponsibleGamingWindow" /> class.
        /// </summary>
        public ResponsibleGamingWindow()
        {
            InitializeComponent();

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;
        }

        /// <summary>
        ///  Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;
            set => DataContext = value;
        }
    }
}
