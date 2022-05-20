namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Windows;
    using ViewModels.OperatorMenu;

    public partial class BarkeeperCashInReward
    {
        public BarkeeperCashInReward()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                var model = DataContext as BarkeeperConfigurationViewModel;
                if (model?.CashInRewardLevels.Count == 1)
                {
                    LevelsDataGrid.Columns[1].Visibility = Visibility.Collapsed;
                }
                else if (model?.CashInRewardLevels.Count == 0)
                {
                    LevelsDataGrid.Visibility = Visibility.Collapsed;
                }
            };
        }
    }
}