namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using OperatorMenu;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for InspectionPage.xaml
    ///     This is the main page that allows users to select
    ///     which wizards to run.
    /// </summary>
    public sealed partial class InspectionPage : OperatorMenuPage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionPage" /> class.
        /// </summary>
        public InspectionPage()
        {
            InitializeComponent();

            DataContext = new InspectionPageViewModel();
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
        }
    }
}