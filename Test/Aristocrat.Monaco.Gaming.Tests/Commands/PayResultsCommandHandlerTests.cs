namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Contracts;
    using Gaming.Commands;
    using Gaming.Contracts.Payment;
    using Gaming.Payment;
    using Kernel;
    using Hardware.Contracts.Persistence;
    using Test.Common;
    using Aristocrat.Monaco.Gaming.Progressives;

    /// <summary>
    ///     PayGameResultsCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class PayGameResultsCommandHandlerTests : IPaymentDeterminationHandler
    {
        private const long Jackpot = 120000;
        private const long MaxCredit = 10000000;
        private const long MaxWin = 1000000;
        private const long Denomination = 1000;

        private Mock<IPlayerBank> _bank;
        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IPropertiesManager> _properties;
        private Mock<ICommandHandlerFactory> _commands;
        private Mock<IProgressiveGameProvider> _progressiveGame;
        private Mock<IEventBus> _eventBus;
        private Mock<IGameProvider> _games;
        private PaymentDeterminationProvider _paymentDetermination;
        private Mock<IOutcomeValidatorProvider> _outcomeValidation;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialization()
        {
            _bank = new Mock<IPlayerBank>();
            _persistence = new Mock<IPersistentStorageManager>();
            _gameHistory = new Mock<IGameHistory>();
            _properties = new Mock<IPropertiesManager>();
            _commands = new Mock<ICommandHandlerFactory>();
            _progressiveGame = new Mock<IProgressiveGameProvider>();
            _eventBus = new Mock<IEventBus>();
            _games = new Mock<IGameProvider>();
            _paymentDetermination = new PaymentDeterminationProvider();
            _outcomeValidation = new Mock<IOutcomeValidatorProvider>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);

            var handler = new PayGameResultsCommandHandler(
                _eventBus.Object,
                _bank.Object,
                _persistence.Object,
                _gameHistory.Object,
                _properties.Object,
                _games.Object,
                _commands.Object,
                _progressiveGame.Object,
                _paymentDetermination,
                _outcomeValidation.Object);

            Assert.IsNotNull(handler);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenParamsAreInvalidExpectNullExpectException(
            bool nullBus,
            bool nullBank,
            bool nullPersistence,
            bool nullGameHistory,
            bool nullProperties,
            bool nullGames,
            bool nullCommands,
            bool nullProgressive,
            bool nullDetermination,
            bool nullValidation)
        {
            var handler = new PayGameResultsCommandHandler(
                nullBus ? null : _eventBus.Object,
                nullBank ? null : _bank.Object,
                nullPersistence ? null : _persistence.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullProperties ? null : _properties.Object,
                nullGames ? null : _games.Object,
                nullCommands ? null : _commands.Object,
                nullProgressive ? null : _progressiveGame.Object,
                nullDetermination ? null : _paymentDetermination,
                nullValidation ? null : _outcomeValidation.Object);
        }

        [TestMethod]
        public void WhenJackpotExpectStart()
        {
            const long win = Jackpot + 1000;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), win * GamingConstants.Millicents, TransferOutReason.LargeWin, 0), Times.Once);
            _bank.Verify(b => b.AddWin(It.IsAny<int>()), Times.Never);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenNotJackpotExpectBank()
        {
            const long win = Jackpot - 1000;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _gameHistory.Verify(b => b.PayResults(), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenNotJackpotAndNotMeterFreeGamesIndependentlyExpectBank()
        {
            const long win = Jackpot - 1000;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));

            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _gameHistory.Verify(b => b.PayResults(), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenJackpotAndLargeWinIgnoreExpectBank()
        {
            const long win = Jackpot + 1000;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.None);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _gameHistory.Verify(b => b.PayResults(), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenLossExpectNothing()
        {
            const long win = 0;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _gameHistory.Verify(b => b.PayResults(), Times.Never);
            _bank.Verify(b => b.AddWin(win), Times.Never);
            scope.Verify(m => m.Complete(), Times.Never);
        }

        [Ignore]
        [TestMethod]
        public void WhenMeterFreeGamesIndependentlyExpectNoStart()
        {
            const long win = Jackpot - 1000;

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(true);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _gameHistory.Verify(b => b.PayResults(), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Never);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenCreditLimitExceededExpectHandpay()
        {
            const long win = 10;
            const long balance = MaxCredit * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(Jackpot * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.Win);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<TransferOutReason>(), It.IsAny<long>()), Times.Never);
            _bank.Verify(b => b.AddWin(It.IsAny<int>()), Times.Never);
            scope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenMaxWinExceededAndMaxCreditNotExceeded()
        {
            const long win = MaxWin + 100;
            const long balance = MaxCredit * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(MaxWin * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.CreditLimit);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Voucher);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var winInMillicents = win * GamingConstants.Millicents;

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.CashOut(It.IsAny<Guid>(), winInMillicents, TransferOutReason.CashOut, true, It.IsAny<long>()), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void WhenMaxWinExceededAndMaxCreditExceeded()
        {
            const long win = MaxCredit + 100;
            const long balance = MaxCredit * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(MaxWin * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.CreditLimit);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Voucher);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var winInMillicents = win * GamingConstants.Millicents;

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.CashOut(It.IsAny<Guid>(), winInMillicents, TransferOutReason.CashOut, true, It.IsAny<long>()), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void WhenPaymentDeterminationOverriddenForVoucher()
        {
            const long win = MaxWin + 100;
            const long balance = MaxCredit * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(MaxWin * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.CreditLimit);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Voucher);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);
            _paymentDetermination.Handler = this;
            handler.Handle(command);

            _bank.Verify(b => b.AddWin((large1) / GamingConstants.Millicents), Times.Once);
            _bank.Verify(b => b.CashOut(It.IsAny<Guid>(), large1, TransferOutReason.CashOut, true, It.IsAny<long>()), Times.Once);

            _bank.Verify(b => b.AddWin((large2) / GamingConstants.Millicents), Times.Once);
            _bank.Verify(b => b.CashOut(It.IsAny<Guid>(), large2, TransferOutReason.CashOut, true, It.IsAny<long>()), Times.Once);
            _bank.Verify(b => b.AddWin((wager1 + wager2) / GamingConstants.Millicents), Times.Once);

            scope.Verify(m => m.Complete(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void WhenPaymentDeterminationOverriddenForHandpay()
        {
            const long win = MaxWin + 100;
            const long balance = MaxCredit * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(MaxWin * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.CreditLimit);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var command = new PayGameResults(win);
            _paymentDetermination.Handler = this;
            handler.Handle(command);

            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), large1, TransferOutReason.LargeWin, 0), Times.Once);
            _bank.Verify(b => b.ForceHandpay(It.IsAny<Guid>(), large2, TransferOutReason.LargeWin, 0), Times.Once);
            _bank.Verify(b => b.AddWin((wager1 + wager2) / GamingConstants.Millicents), Times.Once);

            scope.Verify(m => m.Complete(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void WhenMaxWinNotExceededAndMaxCreditExceeded()
        {
            const long win = 100;
            const long balance = (MaxCredit + win) * GamingConstants.Millicents;

            _bank.Setup(b => b.Balance).Returns(balance);

            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(h => h.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(Denomination);
            _properties.Setup(h => h.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
                .Returns(false);
            _properties.Setup(h => h.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<long>()))
                .Returns(long.MaxValue);
            _properties.Setup(h => h.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(MaxCredit * GamingConstants.Millicents);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinMaxCreditCashOutStrategy, It.IsAny<MaxCreditCashOutStrategy>()))
                .Returns(MaxCreditCashOutStrategy.CreditLimit);
            _properties.Setup(h => h.GetProperty(GamingConstants.GameWinLargeWinCashOutStrategy, It.IsAny<LargeWinCashOutStrategy>()))
                .Returns(LargeWinCashOutStrategy.Handpay);
            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));
            var scope = new Mock<IScopedTransaction>();
            _persistence.Setup(m => m.ScopedTransaction()).Returns(scope.Object);

            var log = new Mock<IGameHistoryLog>();
            _gameHistory.SetupGet(m => m.CurrentLog).Returns(log.Object);

            var handler = Factory_CreateHandler();

            var maxCreditInMillicents = MaxCredit * GamingConstants.Millicents;

            var command = new PayGameResults(win);

            handler.Handle(command);
            _bank.Verify(b => b.CashOut(It.IsAny<Guid>(), maxCreditInMillicents, TransferOutReason.CashOut, true, It.IsAny<long>()), Times.Once);
            _bank.Verify(b => b.AddWin(win), Times.Once);
            scope.Verify(m => m.Complete(), Times.AtLeastOnce);
        }

        private PayGameResultsCommandHandler Factory_CreateHandler()
        {
            return new PayGameResultsCommandHandler(
                _eventBus.Object,
                _bank.Object,
                _persistence.Object,
                _gameHistory.Object,
                _properties.Object,
                _games.Object,
                _commands.Object,
                _progressiveGame.Object,
                _paymentDetermination,
                _outcomeValidation.Object);
        }

        private static (IGameDetail game, IDenomination denom) Factory_CreateGame(long denomination)
        {
            var game = new Mock<IGameDetail>();

            game.Setup(h => h.Id).Returns(1);

            var mockDenom = new Mock<IDenomination>();

            mockDenom.Setup(m => m.Value).Returns(denomination);
            mockDenom.Setup(h => h.SecondaryAllowed).Returns(false);
            mockDenom.Setup(h => h.LetItRideAllowed).Returns(false);

            return (game.Object, mockDenom.Object);
        }

        private readonly long wager1 = 12345;
        private readonly long large1 = 23456;
        private readonly long wager2 = 34567;
        private readonly long large2 = 45678;
        public List<PaymentDeterminationResult> GetPaymentResults(long winInMillicents, bool isPayGameResults=true)
        {
            // Return a one off result that's specific to the WhenPaymentDeterminationOverridden tests
            return new List<PaymentDeterminationResult>
            {
                new PaymentDeterminationResult(wager1, large1, Guid.NewGuid()),
                new PaymentDeterminationResult(wager2, large2, Guid.NewGuid())
            };
        }
    }
}