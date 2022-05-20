namespace Aristocrat.Monaco.Bingo.Handpay.Tests
{
    using System;
    using System.Collections.Generic;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Payment;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;
    using Services.GamePlay;
    using Strategies;
    using JackpotDeterminationFactory = Common.IBingoStrategyFactory<Strategies.IJackpotDeterminationStrategy, Common.Storage.Model.JackpotDetermination>;

    [TestClass()]
    public class HandpayServiceTests
    {
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
        private readonly Mock<ICentralProvider> _centralProvider = new Mock<ICentralProvider>();
        private readonly Mock<JackpotDeterminationFactory> _jackpotDeterminationFactory = new Mock<JackpotDeterminationFactory>();
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>();
        private readonly Mock<ITotalWinValidator> _totalWinValidator = new Mock<ITotalWinValidator>();
        private readonly Mock<IPaymentDeterminationProvider> _largeWinDetermination = new Mock<IPaymentDeterminationProvider>();

        private HandpayService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(
            bool nullUnitOfWOrk,
            bool nullCentralProvider,
            bool nullJackpotFactory,
            bool nullGameHistory,
            bool nullPaymentDeterminationProvider,
            bool nullTotalWinValidator)
        {
            _target = CreateTarget(
                nullUnitOfWOrk,
                nullCentralProvider,
                nullJackpotFactory,
                nullGameHistory,
                nullPaymentDeterminationProvider,
                nullTotalWinValidator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MissingStrategyTest()
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, JackpotDetermination>>()))
                .Returns(JackpotDetermination.Unknown);

            _jackpotDeterminationFactory.Setup(x => x.Create(JackpotDetermination.Unknown))
                .Returns((IJackpotDeterminationStrategy)null);
            _target.GetPaymentResults(0);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void GetPaymentResultsTest(bool isPayResults)
        {
            const long amount = 100;
            const long gameTransactionId = 123;
            var transaction = new CentralTransaction
            {
                AssociatedTransactions = new List<long> { gameTransactionId }
            };

            var expectedResults = new List<PaymentDeterminationResult>
            {
                new PaymentDeterminationResult(amount, 0, Guid.Empty)
            };

            var strategy = new Mock<IJackpotDeterminationStrategy>();
            var historyLog = new Mock<IGameHistoryLog>();
            historyLog.Setup(x => x.TransactionId).Returns(gameTransactionId);
            strategy.Setup(x => x.GetPaymentResults(amount, transaction)).Returns(expectedResults);

            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, JackpotDetermination>>()))
                .Returns(JackpotDetermination.Unknown);
            _jackpotDeterminationFactory.Setup(x => x.Create(JackpotDetermination.Unknown))
                .Returns(strategy.Object);
            _centralProvider.Setup(x => x.Transactions).Returns(new List<CentralTransaction> { transaction });
            _gameHistory.Setup(x => x.CurrentLog).Returns(historyLog.Object);

            var results = _target.GetPaymentResults(amount, isPayResults);
            _totalWinValidator.Verify(
                x => x.ValidateTotalWin(amount, transaction),
                isPayResults ? Times.Once() : Times.Never());
            CollectionAssert.AreEquivalent(expectedResults, results);
        }

        private HandpayService CreateTarget(
            bool nullUnitOfWOrk = false,
            bool nullCentralProvider = false,
            bool nullJackpotFactory = false,
            bool nullGameHistory = false,
            bool nullPaymentDeterminationProvider = false,
            bool nullTotalWinValidator = false)
        {
            return new HandpayService(
                nullUnitOfWOrk ? null : _unitOfWorkFactory.Object,
                nullCentralProvider ? null : _centralProvider.Object,
                nullJackpotFactory ? null : _jackpotDeterminationFactory.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullPaymentDeterminationProvider ? null : _largeWinDetermination.Object,
                nullTotalWinValidator ? null : _totalWinValidator.Object);
        }
    }
}