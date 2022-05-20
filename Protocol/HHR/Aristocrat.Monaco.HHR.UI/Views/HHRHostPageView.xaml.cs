namespace Aristocrat.Monaco.Hhr.UI.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using Menu;
    using Services;

    /// <summary>
    ///     Interaction logic for HHRBasePage.xaml
    /// </summary>
    public partial class HHRHostPageView : IHhrHostPageView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRHostPageView" /> class.
        /// </summary>
        public HHRHostPageView(IMenuAccessService dataContext)
        {
            DataContext = dataContext;

            InitializeComponent();

            OuterGrid.SetValue(Grid.RowProperty, 1);

            HeaderGrid.SetValue(Grid.RowProperty, 0);
            MainGrid.SetValue(Grid.RowProperty, 1);
            FooterGrid.SetValue(Grid.RowProperty, 2);

            OuterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Landscape
            if (Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height)
            {
                OuterGrid.Width = 1920;
                InnerGrid.Height = OuterGrid.Height = 1080;

                InnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(110) });
                InnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(590) });
                InnerGrid.RowDefinitions.Add(new RowDefinition());

                ButtonAndTimerGrid.Width = OuterGrid.Width;
                ButtonAndTimerGrid.Margin = new Thickness(0, 10, 0, 0);

                MyTimerControl.Margin = new Thickness(0, 0, 90, 70);
            }

            // Portrait
            else
            {
                RootViewBox.Margin = new Thickness(0, 0, 0, 15);

                OuterGrid.Width = 1080;
                InnerGrid.Height = OuterGrid.Height = 880;

                InnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
                InnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(530) });
                InnerGrid.RowDefinitions.Add(new RowDefinition());

                ButtonAndTimerGrid.Width = OuterGrid.Width;

                MyTimerControl.Margin = new Thickness(0, 0, -200, 60);
            }

            ((IMenuAccessService)DataContext).SetView(this);
        }

        public void Show(Command command)
        {
            ((IMenuAccessService)DataContext).Show(command);
        }

        public void Hide()
        {
            ((IMenuAccessService)DataContext).Hide();
        }

        public void Unhide()
        {
            ((IMenuAccessService)DataContext).Unhide();
        }
    }
}