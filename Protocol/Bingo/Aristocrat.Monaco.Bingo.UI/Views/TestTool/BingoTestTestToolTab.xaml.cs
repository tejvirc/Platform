namespace Aristocrat.Monaco.Bingo.UI.Views.TestTool
{
    using ViewModels.TestTool;

    /// <summary>
    /// Interaction logic for BingoTestTestToolTab.xaml
    /// </summary>
    public partial class BingoTestTestToolTab
    {
        public BingoTestTestToolTab()
        {
            InitializeComponent();
        }


        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public BingoTestTestToolViewModel ViewModel
        {
            get => DataContext as BingoTestTestToolViewModel;
            set => DataContext = value;
        }
    }
}
