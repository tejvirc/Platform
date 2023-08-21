namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP9ASendLegacyBonusMetersHandlerTest
    {
        private LP9ASendLegacyBonusMetersHandler _target;
        private Mock<IGameMeterManager> _meterManager;
        private Mock<IGameProvider> _gameProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            _target = new LP9ASendLegacyBonusMetersHandler(_meterManager.Object, _gameProvider.Object);
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullArgumentsTest(bool nullMeters, bool nullGames)
        {
            _target = new LP9ASendLegacyBonusMetersHandler(
                nullMeters ? null : _meterManager.Object,
                nullGames ? null : _gameProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendLegacyBonusMeters));
        }

        [TestMethod]
        public void HandleGame0PassTest()
        {
            const int gameId = 0;
            var data = new LongPollSendLegacyBonusMetersData { GameId = gameId, AccountingDenom = 1 };

            // mock for the existing meters
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            CreateMockMeter("BonusDeductibleAmount", 12345678000);

            CreateMockMeter("BonusNonDeductibleAmount", 23456789000);

            CreateMockMeter("WagerMatchBonusAmount", 78923456000);

            var expected = new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = 12345678, NonDeductible = 23456789, WagerMatch = 78923456
            };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Deductible, actual.Deductible);
            Assert.AreEqual(expected.NonDeductible, actual.NonDeductible);
            Assert.AreEqual(expected.WagerMatch, actual.WagerMatch);
        }

        [TestMethod]
        public void HandleGameNPassTest()
        {
            const int gameId = 4;
            var data = new LongPollSendLegacyBonusMetersData { GameId = gameId, AccountingDenom = 1 };

            // mock getting a game
            _gameProvider.Setup(g => g.GetAllGames())
                .Returns(new List<IGameDetail> { new TestGameProfile { Id = gameId, Denominations = new List<MockDenomination>()
                {
                    new MockDenomination(1000, gameId)
                }} });

            // mock for the existing meters
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(gameId, 1000, It.IsAny<string>())).Returns(true);

            CreateMockMeter("BonusDeductibleAmount", 12345678000, gameId);

            CreateMockMeter("BonusNonDeductibleAmount", 23456789000, gameId);

            CreateMockMeter("WagerMatchBonusAmount", 0, gameId);

            var expected = new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = 12345678, NonDeductible = 23456789, WagerMatch = 0
            };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Deductible, actual.Deductible);
            Assert.AreEqual(expected.NonDeductible, actual.NonDeductible);
            Assert.AreEqual(expected.WagerMatch, actual.WagerMatch);
        }

        [TestMethod]
        public void HandleInvalidMeterNameTest()
        {
            const int gameId = 4;
            var data = new LongPollSendLegacyBonusMetersData { GameId = gameId, AccountingDenom = 1 };

            // mock getting a game
            _gameProvider.Setup(g => g.GetAllGames())
                .Returns(new List<IGameDetail> { new TestGameProfile { Id = gameId, Denominations = new List<MockDenomination>()
                {
                    new MockDenomination(1000, gameId)
                }} });

            // mock for the existing meters
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(false);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<int>(), It.IsAny<string>())).Returns(false);

            var expected = new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = 0, NonDeductible = 0, WagerMatch = 0
            };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Deductible, actual.Deductible);
            Assert.AreEqual(expected.NonDeductible, actual.NonDeductible);
            Assert.AreEqual(expected.WagerMatch, actual.WagerMatch);
        }

        [TestMethod]
        public void HandleInvalidGameIdTest()
        {
            const int gameId = 4;
            var data = new LongPollSendLegacyBonusMetersData { GameId = int.MaxValue, AccountingDenom = 1 };

            // mock getting a game
            _gameProvider.Setup(g => g.GetAllGames())
                .Returns(new List<IGameDetail> { new TestGameProfile { Id = gameId, Denominations = new List<MockDenomination>()
                {
                    new MockDenomination(1000, gameId)
                }} });

            // mock for the existing meters
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<string>())).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            var expected = new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = 0, NonDeductible = 0, WagerMatch = 0
            };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Deductible, actual.Deductible);
            Assert.AreEqual(expected.NonDeductible, actual.NonDeductible);
            Assert.AreEqual(expected.WagerMatch, actual.WagerMatch);
        }

        private void CreateMockMeter(string meterName, long lifeTimeAmount, int gameId = 0, int denomId = 1000)
        {
            var meter = new Mock<IMeter>();

            meter.Setup(m => m.Lifetime).Returns(lifeTimeAmount);

            if (gameId == 0)
            {
                _meterManager.Setup(m => m.GetMeter(meterName)).Returns(meter.Object);
            }
            else
            {
                _meterManager.Setup(m => m.GetMeter(gameId, denomId, meterName)).Returns(meter.Object);
            }
        }
    }
}