namespace Aristocrat.Monaco.Gaming.UI.Views
{
    using Contracts;
    using Kernel;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for TestToolView.xaml
    /// </summary>
    public partial class TestToolView
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TestToolView" /> class.
        /// </summary>
        public TestToolView()
        {
            InitializeComponent();

            ViewModel = new TestToolViewModel();

            ServiceManager.GetInstance().GetService<IEventBus>()
                .Subscribe<TestToolPluginEvent>(this, evt =>
                {
                    Execute.OnUIThread(
                        () => TabControl.Items.Insert(TabControl.Items.Count, evt.Tab));
                });
        }

        /// <summary>
        ///     Gets or sets the view model for the view.
        /// </summary>
        public TestToolViewModel ViewModel
        {
            get => DataContext as TestToolViewModel;
            set => DataContext = value;
        }
    }
}
