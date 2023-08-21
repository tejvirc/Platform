namespace Aristocrat.Monaco.Gaming.UI.Views.OperatorMenu
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels.OperatorMenu;

    public partial class BarkeeperCoinInReward : UserControl
    {
        public BarkeeperCoinInReward()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                var model = DataContext as BarkeeperConfigurationViewModel;
                if (model?.CoinInRewardLevels.Count == 1)
                {
                    LevelsDataGrid.Columns[1].Visibility = Visibility.Collapsed;
                }
                else if (model?.CoinInRewardLevels.Count == 0)
                {
                    LevelsDataGrid.Visibility = Visibility.Collapsed;
                }
            };
        }
    }
}