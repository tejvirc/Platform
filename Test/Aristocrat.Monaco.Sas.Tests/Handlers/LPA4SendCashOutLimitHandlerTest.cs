namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Tests for the LPA4SendCashOutLimitHandlerTest class
    /// </summary>
    [TestClass]
    public class LPA4SendCashOutLimitHandlerTest
    {
        private const long ExpectedAmountMilliCents = 123456780000;
        private const int ExpectedGameId = 12345;
        private const int ExpectedBadGameId = 666;
        private LPA4SendCashOutLimitHandler _target;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IMeterManager> _meterManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var games = SetupDummyGames();
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _gameProvider.Setup(m => m.GetAllGames()).Returns(games);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.CurrentHopperLevel)).Returns(true);

            var currentHopperLevel = new Mock<IMeter>();
            currentHopperLevel.Setup(m => m.Lifetime).Returns(ExpectedAmountMilliCents);
            currentHopperLevel.Setup(m => m.Classification).Returns(new TestMeterClassification());
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.CurrentHopperLevel)).Returns(currentHopperLevel.Object);

            _target = new LPA4SendCashOutLimitHandler(_meterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendCashOutLimit));
        }

        [DataRow(ExpectedGameId, ExpectedAmountMilliCents)]
        [DataRow(ExpectedBadGameId, 0L)]
        [DataTestMethod]
        public void HandleTest(int gameId, long expectedAmountMillicents)
        {
            var expected = _target.Handle(new SendCashOutLimitData { GameId = gameId, AccountingDenom = 1 } );
            Assert.IsNotNull(expected);
            Assert.AreEqual((ulong)expectedAmountMillicents.MillicentsToCents(), expected.Data);
        }

        private static List<IGameDetail> SetupDummyGames()
        {
            return new List<IGameDetail>
            {
                new TestGameProfile { Id = ExpectedGameId },
                new TestGameProfile { Id = 565 },
                new TestGameProfile { Id = 2 }
            };
        }
    }
}
