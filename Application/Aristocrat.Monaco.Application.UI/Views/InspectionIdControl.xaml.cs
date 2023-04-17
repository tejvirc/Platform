namespace Aristocrat.Monaco.Application.UI.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using Kernel;
    using Kernel.Contracts;
    using ViewModels;

    /// <summary>
    /// Interaction logic for InspectionIdControl.xaml
    /// </summary>
    public partial class InspectionIdControl : UserControl
    {
        public InspectionIdControl()
        {
            InitializeComponent();

            DataContext = new InspectionIdViewModel();

            Visibility = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(KernelConstants.IsInspectionOnly, false)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
