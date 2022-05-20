namespace Aristocrat.Monaco.Hhr.UI.Views
{
    using ViewModels;

    /// <summary>
    ///     Interaction logic for VenueRaceCollection.xaml
    /// </summary>
    public partial class VenueRaceCollection
    {
        public VenueRaceCollection(VenueRaceCollectionViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}