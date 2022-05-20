namespace Aristocrat.Monaco.Application.UI.Views
{
    using ViewModels;

    /// <summary>
    ///     Interaction logic for ConfigSelectionPage.xaml
    ///     This is the main page that allows users to select
    ///     which wizards to run.
    /// </summary>
    public sealed partial class ConfigSelectionPage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigSelectionPage" /> class.
        /// </summary>
        public ConfigSelectionPage()
        {
            InitializeComponent();
            DataContext = new ConfigSelectionPageViewModel();
        }
    }
}