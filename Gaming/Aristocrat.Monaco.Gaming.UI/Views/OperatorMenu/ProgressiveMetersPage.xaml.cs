namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
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
            if (e.OldValue is ProgressiveMetersPageViewModel oldViewModel)
            {
                oldViewModel.OnShouldRegenerateColumns -= ViewModel_OnCultureChange;
            }

            var viewModel = (ProgressiveMetersPageViewModel)DataContext;
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
            if (viewModel is null)
            {
                return;
            }

            ProgressiveDataGrid?.Columns.Clear();
            foreach (var meterNode in viewModel.MeterNodes)
            {
                var textColumn = new DataGridTextColumn
                {
                    Binding = new Binding($"[{meterNode.Order}].Value"),
                    Header = GetHeaderFromMeterNode(meterNode),
                    Width = DataGridLength.Auto
                };

                ProgressiveDataGrid?.Columns.Add(textColumn);
            }
        }

        private string GetHeaderFromMeterNode(MeterNode meterNode)
        {
            if (string.IsNullOrEmpty(meterNode.DisplayNameKey))
            {
                return meterNode.DisplayName;
            }

            var header = Localizer.For(CultureFor.Operator).GetString(meterNode.DisplayNameKey, _ => { });
            if (string.IsNullOrEmpty(header))
            {
                header = meterNode.DisplayName;
            }

            return header;
        }
    }
}
