namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using Models;

    /// <summary>
    ///     Interaction logic for HorseRaceResultSetEntryControl.xaml
    /// </summary>
    public partial class PreviousResultSet
    {
        /// <summary>
        ///     Dependency property for RaceName
        /// </summary>
        public static readonly DependencyProperty RaceNameProperty =
            DependencyProperty.Register(
                "RaceName",
                typeof(string),
                typeof(PreviousResultSet),
                new PropertyMetadata(""));

        /// <summary>
        ///     Dependency property for RaceDate
        /// </summary>
        public static readonly DependencyProperty RaceDateProperty =
            DependencyProperty.Register(
                "RaceDate",
                typeof(string),
                typeof(PreviousResultSet),
                new PropertyMetadata(null));

        /// <summary>
        ///     Dependency property for HorseCollection
        /// </summary>
        public static readonly DependencyProperty HorseCollectionProperty =
            DependencyProperty.Register(
                "HorseCollection",
                typeof(ObservableCollection<HorseModel>),
                typeof(PreviousResultSet),
                new PropertyMetadata(null));

        public PreviousResultSet()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Race Name to be displayed
        /// </summary>
        public string RaceName
        {
            get => (string)GetValue(RaceNameProperty);
            set => SetValue(RaceNameProperty, value);
        }

        /// <summary>
        ///     Race Date to be displayed
        /// </summary>
        public string RaceDate
        {
            get => (string)GetValue(RaceDateProperty);
            set => SetValue(RaceDateProperty, value);
        }

        /// <summary>
        ///     Horse collection to be displayed
        /// </summary>
        public ObservableCollection<HorseModel> HorseCollection
        {
            get => (ObservableCollection<HorseModel>)GetValue(HorseCollectionProperty);
            set => SetValue(HorseCollectionProperty, value);
        }
    }
}