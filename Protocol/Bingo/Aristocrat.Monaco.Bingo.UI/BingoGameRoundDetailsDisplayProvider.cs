namespace Aristocrat.Monaco.Bingo.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Application.Contracts.OperatorMenu;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    /// <summary>
    ///     Provides the display for bingo details for a given game round.
    /// </summary>
    public class BingoGameRoundDetailsDisplayProvider : IGameRoundDetailsDisplayProvider
    {
        private readonly ICentralProvider _centralProvider;

        public BingoGameRoundDetailsDisplayProvider(ICentralProvider centralProvider)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameRoundDetailsDisplayProvider)};

        public void Initialize()
        {
        }

        public void Display(
            INotifyPropertyChanged ownerViewModel,
            IDialogService dialogService,
            string windowTitle,
            long centralTransactionId)
        {
            if (ownerViewModel is null)
            {
                throw new ArgumentNullException(nameof(ownerViewModel));
            }
            if (dialogService is null)
            {
                throw new ArgumentNullException(nameof(dialogService));
            }
            if (windowTitle is null)
            {
                throw new ArgumentNullException(nameof(windowTitle));
            }

            var bingoGameDescription = _centralProvider.Transactions
                .SingleOrDefault(x => x.TransactionId == centralTransactionId)?.Descriptions
                .OfType<BingoGameDescription>()
                .FirstOrDefault();
            if (bingoGameDescription is null)
            {
                return;
            }

            dialogService.ShowInfoDialog<BingoGameHistoryDetailsView>(
                ownerViewModel,
                new BingoGameHistoryDetailsViewModel(bingoGameDescription),
                windowTitle);
        }
    }
}
