namespace Aristocrat.Monaco.Bingo.Tests.GameEndWin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Bingo.GameEndWin;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameEndWinBonusCreditsStrategyTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<IBonusHandler> _bonusHandler = new(MockBehavior.Default);

        private GameEndWinBonusCreditsStrategy _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(bool nullEventBus, bool nullBonusHandler)
        {
            _target = CreateTarget(nullEventBus, nullBonusHandler);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TaskCancelledTest()
        {
            const long winAmount = 100;
            using var source = new CancellationTokenSource();
            _bonusHandler.Setup(x => x.Cancel(It.IsAny<string>())).Returns(true);
            _bonusHandler.Setup(
                    x => x.Award(It.Is<GameWinBonus>(b => b.CashableAmount == winAmount)))
                .Returns<GameWinBonus>(
                    x => new BonusTransaction(
                        0,
                        DateTime.UtcNow,
                        x.BonusId,
                        x.CashableAmount,
                        x.NonCashAmount,
                        x.PromoAmount,
                        1,
                        2,
                        x.PayMethod));

            var resultTask = _target.ProcessWin(100, source.Token);
            source.Cancel();
            await resultTask;
            _eventBus.Verify(x => x.UnsubscribeAll(_target), Times.Once);
        }

        [DataRow(1000, false, false, false, false)]
        [DataRow(1000, false, true, false, true)]
        [DataRow(1000, false, true, true, false)]
        [DataRow(1000, true, true, false, true)]
        [DataTestMethod]
        public async Task OneCentGameEndWinStrategyTest(long winAmount, bool isCancelled, bool bonusCreated, bool bonusFailed, bool expectedResult)
        {
            using var source = new CancellationTokenSource();
            Action<BonusAwardedEvent> awardedAction = (_) => { };
            Action<BonusFailedEvent> failedAction = (_) => { };
            BonusTransaction transaction = null;
            _bonusHandler.Setup(x => x.Cancel(It.IsAny<string>())).Returns(false);
            _bonusHandler.Setup(
                    x => x.Award(It.Is<GameWinBonus>(b => b.CashableAmount == winAmount)))
                .Returns<GameWinBonus>(
                    x => transaction = bonusCreated
                        ? new BonusTransaction(
                            0,
                            DateTime.UtcNow,
                            x.BonusId,
                            x.CashableAmount,
                            x.NonCashAmount,
                            x.PromoAmount,
                            1,
                            2,
                            x.PayMethod)
                        : null);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<BonusAwardedEvent>>(),
                        It.IsAny<Predicate<BonusAwardedEvent>>()))
                .Callback<object, Action<BonusAwardedEvent>, Predicate<BonusAwardedEvent>>(
                    (_, a, _) => awardedAction = a);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<BonusFailedEvent>>(),
                        It.IsAny<Predicate<BonusFailedEvent>>()))
                .Callback<object, Action<BonusFailedEvent>, Predicate<BonusFailedEvent>>((_, a, _) => failedAction = a);
            var resultTask = _target.ProcessWin(winAmount, source.Token);

            if (isCancelled)
            {
                source.Cancel();
            }

            if (bonusCreated)
            {
                if (bonusFailed)
                {
                    failedAction.Invoke(new BonusFailedEvent(transaction));
                }
                else
                {
                    awardedAction.Invoke(new BonusAwardedEvent(transaction));
                }
            }

            Assert.AreEqual(expectedResult, await resultTask);
            _eventBus.Verify(x => x.UnsubscribeAll(_target), Times.Once);
        }

        private GameEndWinBonusCreditsStrategy CreateTarget(
            bool nullEventBus = false,
            bool nullBonusHandler = false)
        {
            return new GameEndWinBonusCreditsStrategy(
                nullEventBus ? null : _eventBus.Object,
                nullBonusHandler ? null : _bonusHandler.Object);
        }
    }
}