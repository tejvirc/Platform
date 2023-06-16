namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.MVVM;
    using Aristocrat.MVVM.ViewModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using ViewModels.OperatorMenu;

    /// <summary>
    /// Interaction logic for ProgressiveMetersPage.xaml
    /// </summary>
    public partial class ProgressiveMetersPage
    {
        public ProgressiveMetersPage()
        {
            InitializeComponent();
            DataContextChanged += ProgressiveMetersPage_DataContextChanged;
        }

        private void ProgressiveMetersPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is ProgressiveMetersPageViewModel oldViewModel)
            {
                oldViewModel.OnShouldRegenerateColumns -= ViewModel_OnCultureChange;
            }

            var viewModel = (ProgressiveMetersPageViewModel) DataContext;
            viewModel.OnShouldRegenerateColumns += ViewModel_OnCultureChange;
            SetupMeters(viewModel);
        }

        private void ViewModel_OnCultureChange(object sender, System.EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                SetupMeters(sender as ProgressiveMetersPageViewModel);
            });
        }

        private void SetupMeters(ProgressiveMetersPageViewModel viewModel)
        {
            if (viewModel is not ProgressiveMetersPageViewModel)
            {
                return;
            }

            ProgressiveDataGrid?.Columns.Clear();
            foreach (var meterNode in viewModel.MeterNodes)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Binding = new Binding($"[{meterNode.Order}].Value"),
                    Header = Localizer.For(CultureFor.Operator).GetString(meterNode.DisplayNameKey),
                    Width = DataGridLength.Auto
                };

                ProgressiveDataGrid?.Columns.Add(textColumn);
            }
        }
    }
}
