namespace Aristocrat.Monaco.Hhr.UI.Views
{
    using Menu;

    /// <summary>
    /// Interaction logic for PlacardView.xaml
    /// </summary>
    public partial class PlacardView
    {
        public PlacardView(Placard placard)
        {
            Placard = placard;
            InitializeComponent();
        }

        public Placard Placard { get; set; }
    }
}