namespace Aristocrat.Monaco.Bingo.UI.Views.TestTool
{
    using ViewModels.TestTool;

    /// <summary>
    /// Interaction logic for BingoInfoTestToolTab.xaml
    /// </summary>
    public partial class BingoInfoTestToolTab
    {
        public BingoInfoTestToolTab()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public BingoInfoTestToolViewModel ViewModel
        {
            get => DataContext as BingoInfoTestToolViewModel;
            set => DataContext = value;
        }
    }
}