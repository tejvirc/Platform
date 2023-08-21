namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP83SendCumulativeProgressiveWinHandlerTests
    {
        private Mock<IGameMeterManager> _gameMeterManager;
        private Mock<IGameProvider> _gameProvider;
        private LP83SendCumulativeProgressiveWinHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            _target = new LP83SendCumulativeProgressiveWinHandler(
                _gameMeterManager.Object,
                _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterManagerTest()
        {
            _target = new LP83SendCumulativeProgressiveWinHandler(
                null,
                _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameProviderTest()
        {
            _target = new LP83SendCumulativeProgressiveWinHandler(
                _gameMeterManager.Object,
                null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendCumulativeProgressiveWins));
        }

        [TestMethod]
        public void HandleTestGameEmpty()
        {
            const int gameId = 123;
            var data = new SendCumulativeProgressiveWinData { GameId = gameId, AccountingDenom = 1 };
            _gameProvider.Setup(x => x.GetAllGames())
                .Returns(new List<IGameDetail>());

            var actual = _target.Handle(data);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0UL, actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestRequestAllGames()
        {
            const long expectedAttendantPaid = 1000000;
            const long expectedGamePaid = 3000;
            var data = new SendCumulativeProgressiveWinData { GameId = 0, AccountingDenom = 1 };
            var attendantMeter = new Mock<IMeter>();
            attendantMeter.Setup(x => x.Classification).Returns(new TestMeterClassification());
            attendantMeter.Setup(x => x.Lifetime).Returns(expectedAttendantPaid.CentsToMillicents());

            var gamePaid = new Mock<IMeter>();
            gamePaid.Setup(x => x.Classification).Returns(new TestMeterClassification());
            gamePaid.Setup(x => x.Lifetime).Returns(expectedGamePaid.CentsToMillicents());
            _gameMeterManager.Setup(x => x.GetMeter(GamingMeters.HandPaidProgWonAmount))
                .Returns(attendantMeter.Object);
            _gameMeterManager.Setup(x => x.GetMeter(GamingMeters.EgmPaidProgWonAmount))
                .Returns(gamePaid.Object);
            _gameMeterManager.Setup(x => x.IsMeterProvided(GamingMeters.HandPaidProgWonAmount)).Returns(true);
            _gameMeterManager.Setup(x => x.IsMeterProvided(GamingMeters.EgmPaidProgWonAmount)).Returns(true);

            var actual = _target.Handle(data);

            Assert.IsNotNull(actual);
            Assert.AreEqual((ulong)(expectedAttendantPaid + expectedGamePaid), actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestWithValidGame()
        {
            const int gameId = 123;
            const long denomValue = 1000;
            const long expectedAttendantPaid = 1000000;
            const long expectedGamePaid = 3000;
            var data = new SendCumulativeProgressiveWinData { GameId = gameId, AccountingDenom = 1 };
            _gameProvider.Setup(x => x.GetAllGames())
                .Returns(new List<IGameDetail> { SetupGame(gameId, denomValue, gameId) });
            var attendantMeter = new Mock<IMeter>();
            attendantMeter.Setup(x => x.Classification).Returns(new TestMeterClassification());
            attendantMeter.Setup(x => x.Lifetime).Returns(expectedAttendantPaid.CentsToMillicents());

            var gamePaid = new Mock<IMeter>();
            gamePaid.Setup(x => x.Classification).Returns(new TestMeterClassification());
            gamePaid.Setup(x => x.Lifetime).Returns(expectedGamePaid.CentsToMillicents());
            _gameMeterManager.Setup(x => x.GetMeter(gameId, denomValue, GamingMeters.HandPaidProgWonAmount))
                .Returns(attendantMeter.Object);
            _gameMeterManager.Setup(x => x.GetMeter(gameId, denomValue, GamingMeters.EgmPaidProgWonAmount))
                .Returns(gamePaid.Object);
            _gameMeterManager.Setup(x => x.IsMeterProvided(gameId, denomValue, GamingMeters.HandPaidProgWonAmount)).Returns(true);
            _gameMeterManager.Setup(x => x.IsMeterProvided(gameId, denomValue, GamingMeters.EgmPaidProgWonAmount)).Returns(true);

            var actual = _target.Handle(data);

            Assert.IsNotNull(actual);
            Assert.AreEqual((ulong)(expectedAttendantPaid + expectedGamePaid), actual.MeterValue);
        }

        private IGameDetail SetupGame(int gameId, long denomValue, long denomId)
        {
            return new TestGameProfile
            {
                Active = true,
                Denominations = new List<IDenomination> { new MockDenomination(denomValue, denomId) },
                ActiveDenominations = new List<long> { denomValue },
                Id = gameId
            };
        }
    }
}