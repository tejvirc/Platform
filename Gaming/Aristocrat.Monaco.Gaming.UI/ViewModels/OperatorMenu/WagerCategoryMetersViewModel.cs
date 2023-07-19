namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Meters;
    using Kernel;

    [CLSCompliant(false)]
    public class WagerCategoryMetersViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGameDetail _selectedGame;
        public WagerCategoryMetersViewModel(IGameDetail selectedGame, bool showLifetime)
        {
            _selectedGame = selectedGame;
            LoadCategories(showLifetime);
        }

        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        
        public decimal TheoPayback { get; set; }

        public string GameName => $"{_selectedGame.ThemeName} ({_selectedGame.PaytableName} - {_selectedGame.Version})";

        private void LoadCategories(bool showLifetime)
        {
            var meterManager = ServiceManager.GetInstance().GetService<IGameMeterManager>();

            Categories = new ObservableCollection<Category>(
                _selectedGame.WagerCategories.Select(
                    w => new Category
                    {
                        WagerCategoryId = w.Id,
                        Rtp = w.MinBaseRtpPercent,
                        WageredMillicents = showLifetime
                            ? meterManager.GetMeter(
                                _selectedGame.Id,
                                w.Id,
                                GamingMeters.WagerCategoryWageredAmount).Lifetime
                            : meterManager.GetMeter(
                                _selectedGame.Id,
                                w.Id,
                                GamingMeters.WagerCategoryWageredAmount).Period,
                        GamesPlayed = showLifetime
                            ? meterManager.GetMeter(
                                _selectedGame.Id,
                                w.Id,
                                GamingMeters.WagerCategoryPlayedCount).Lifetime
                            : meterManager.GetMeter(
                                _selectedGame.Id,
                                w.Id,
                                GamingMeters.WagerCategoryPlayedCount).Period
                    }
                ));

            RaisePropertyChanged(nameof(Categories));
            var totalAmountIn = Categories.Sum(d => d.WageredMillicents);
            if (totalAmountIn > 0)
            {
                TheoPayback =
                    Categories.Sum(d => d.Rtp * d.WageredMillicents) / totalAmountIn;
            }
            else
            {
                TheoPayback = 0;
            }
            RaisePropertyChanged(nameof(TheoPayback));
        }
    }

    public class Category
    {
        public string WagerCategoryId { get; set; }

        public decimal Rtp { get; set; }

        public long WageredMillicents { get; set; }

        /// <summary> Gets or sets the credits in.</summary>
        public decimal Wagered => WageredMillicents.MillicentsToDollars();

        public long GamesPlayed { get; set; }
    }

}
