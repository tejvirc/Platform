namespace Aristocrat.Monaco.Hhr.UI.Views
{
    using System.Reflection;
    using System.Windows;
    using ViewModels;
    using log4net;

    /// <summary>
    ///     Interaction logic for VenueRaceCollection.xaml
    /// </summary>
    public partial class VenueRaceCollection
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public VenueRaceCollection(VenueRaceCollectionViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void Root_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Logger.Debug($"Root_OnIsVisibleChanged: {e.OldValue}--->{e.NewValue}");
        }
    }
}