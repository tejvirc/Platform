namespace Aristocrat.Monaco.Bingo.Handpay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;
    using Protocol.Common.Storage.Entity;
    using Services.GamePlay;
    using JackpotDeterminationFactory = Common.IBingoStrategyFactory<Strategies.IJackpotDeterminationStrategy, Common.Storage.Model.JackpotDetermination>;

    public class HandpayService : IPaymentDeterminationHandler, IDisposable
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICentralProvider _centralProvider;
        private readonly JackpotDeterminationFactory _jackpotDeterminationFactory;
        private readonly IGameHistory _gameHistory;
        private readonly IPaymentDeterminationProvider _largeWinDetermination;
        private readonly ITotalWinValidator _totalWinValidator;

        private bool _disposed;

        public HandpayService(
            IUnitOfWorkFactory unitOfWorkFactory,
            ICentralProvider centralProvider,
            JackpotDeterminationFactory jackpotDeterminationFactory,
            IGameHistory gameHistory,
            IPaymentDeterminationProvider largeWinDetermination,
            ITotalWinValidator totalWinValidator)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _jackpotDeterminationFactory = jackpotDeterminationFactory ??
                                           throw new ArgumentNullException(nameof(jackpotDeterminationFactory));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _largeWinDetermination =
                largeWinDetermination ?? throw new ArgumentNullException(nameof(largeWinDetermination));
            _totalWinValidator = totalWinValidator ?? throw new ArgumentNullException(nameof(totalWinValidator));

            _largeWinDetermination.Handler = this;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<PaymentDeterminationResult> GetPaymentResults(long winInMillicents, bool isPayGameResults = true)
        {
            var transaction = _centralProvider.Transactions.First(
                t => t.AssociatedTransactions.Contains(_gameHistory.CurrentLog.TransactionId));
            if (isPayGameResults)
            {
                _totalWinValidator.ValidateTotalWin(winInMillicents, transaction);
            }

            var jackpotDetermination = _unitOfWorkFactory.Invoke(
                x => x.Repository<BingoServerSettingsModel>().Queryable().SingleOrDefault()
                    ?.JackpotAmountDetermination ?? JackpotDetermination.Unknown);
            var strategy = _jackpotDeterminationFactory.Create(jackpotDetermination) ??
                           throw new InvalidOperationException("Unable to find a valid jackpot strategy handler");
            return strategy.GetPaymentResults(winInMillicents, transaction).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _largeWinDetermination.Handler = null;
            }

            _disposed = true;
        }
    }
}