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
    using Monaco.Sas.Handlers;
    using Moq;

    [TestClass]
    public class LP52SendGameNMetersHandlerTest
    {
        private LP52SendGameNMetersHandler _target;
        private Mock<IGameMeterManager> _meterManager;
        private Mock<IGameProvider> _gameProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            _target = new LP52SendGameNMetersHandler(_meterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameMeterProviderTest()
        {
            _target = new LP52SendGameNMetersHandler(null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            _target = new LP52SendGameNMetersHandler(_meterManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendGameNMeters));
        }

        [TestMethod]
        public void HandleGame0PassTest()
        {
            var data = new LongPollReadMultipleMetersGameNData
            {
                GameNumber = 0
            };
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            data.AccountingDenom = 1;

            var testClassification = new TestMeterClassification();

            // mocks for canceled credits meter with value 12345678000
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.TotalEgmPaidAmt)).Returns(true);
            var coinOutMeter = new Mock<IMeter>();
            coinOutMeter.Setup(m => m.Lifetime).Returns(12345678000);
            coinOutMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.TotalEgmPaidAmt)).Returns(coinOutMeter.Object);

            // mocks for coin in meter with value 23456789000
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.WageredAmount)).Returns(true);
            var coinInMeter = new Mock<IMeter>();
            coinInMeter.Setup(m => m.Lifetime).Returns(23456789000);
            coinInMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.WageredAmount)).Returns(coinInMeter.Object);

            // mocks for jackpot meter with value 78923456000
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.TotalHandPaidAmt)).Returns(true);
            var jackpotMeter = new Mock<IMeter>();
            jackpotMeter.Setup(m => m.Lifetime).Returns(78923456000);
            jackpotMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.TotalHandPaidAmt)).Returns(jackpotMeter.Object);

            // mocks for games played meter with value 00000005678
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.PlayedCount)).Returns(true);
            var gamesPlayed = new Mock<IMeter>();
            gamesPlayed.Setup(m => m.Lifetime).Returns(00000005678);
            gamesPlayed.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.PlayedCount)).Returns(gamesPlayed.Object);

            var expected = new LongPollReadMultipleMetersResponse();
            expected.Meters.Add(SasMeters.TotalCoinOut, new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 12345678));
            expected.Meters.Add(SasMeters.TotalCoinIn, new LongPollReadMeterResponse(SasMeters.TotalCoinIn, 23456789));
            expected.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 78923456));
            expected.Meters.Add(SasMeters.GamesPlayed, new LongPollReadMeterResponse(SasMeters.GamesPlayed, 00000005678));

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meters[SasMeters.TotalCoinOut].MeterValue, actual.Meters[SasMeters.TotalCoinOut].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalCoinIn].MeterValue, actual.Meters[SasMeters.TotalCoinIn].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalJackpot].MeterValue, actual.Meters[SasMeters.TotalJackpot].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.GamesPlayed].MeterValue, actual.Meters[SasMeters.GamesPlayed].MeterValue);
            Assert.AreEqual(SasMeters.TotalCoinOut, actual.Meters[SasMeters.TotalCoinOut].Meter);
        }

        [TestMethod]
        public void HandleGameNPassTest()
        {
            const int gameId = 3;
            const long denomValue = 2000;
            var data = new LongPollReadMultipleMetersGameNData
            {
                GameNumber = 4
            };
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinOut, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.GamesPlayed, MeterType.Lifetime));
            data.AccountingDenom = 1;

            var testClassification = new TestMeterClassification();

            // mocks for canceled credits meter with value 12345678000
            _meterManager.Setup(m => m.IsMeterProvided(gameId, denomValue, GamingMeters.TotalEgmPaidAmt)).Returns(true);
            var coinOutMeter = new Mock<IMeter>();
            coinOutMeter.Setup(m => m.Lifetime).Returns(12345678000);
            coinOutMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(gameId, denomValue, GamingMeters.TotalEgmPaidAmt)).Returns(coinOutMeter.Object);

            // mocks for coin in meter with value 23456789000
            _meterManager.Setup(m => m.IsMeterProvided(gameId, denomValue, GamingMeters.WageredAmount)).Returns(true);
            var coinInMeter = new Mock<IMeter>();
            coinInMeter.Setup(m => m.Lifetime).Returns(23456789000);
            coinInMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(gameId, denomValue, GamingMeters.WageredAmount)).Returns(coinInMeter.Object);

            // mocks for jackpot meter with value 0
            _meterManager.Setup(m => m.IsMeterProvided(gameId, denomValue, GamingMeters.HandPaidTotalWonAmount )).Returns(false);

            // mocks for games played meter with value 00000005678
            _meterManager.Setup(m => m.IsMeterProvided(gameId, denomValue, GamingMeters.PlayedCount)).Returns(true);
            var gamesPlayed = new Mock<IMeter>();
            gamesPlayed.Setup(m => m.Lifetime).Returns(00000005678);
            gamesPlayed.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(gameId, denomValue, GamingMeters.PlayedCount)).Returns(gamesPlayed.Object);

            var expected = new LongPollReadMultipleMetersResponse();
            expected.Meters.Add(SasMeters.TotalCoinOut, new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 12345678));
            expected.Meters.Add(SasMeters.TotalCoinIn, new LongPollReadMeterResponse(SasMeters.TotalCoinIn, 23456789));
            expected.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 0));
            expected.Meters.Add(SasMeters.GamesPlayed, new LongPollReadMeterResponse(SasMeters.GamesPlayed, 00000005678));

            _gameProvider.Setup(x => x.GetAllGames()).Returns(
                new List<IGameDetail>
                {
                    new TestGameProfile
                    {
                        Id = gameId,
                        Denominations =
                            new List<IDenomination> { new MockDenomination(denomValue, data.GameNumber) }
                    }
                });

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meters[SasMeters.TotalCoinOut].MeterValue, actual.Meters[SasMeters.TotalCoinOut].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalCoinIn].MeterValue, actual.Meters[SasMeters.TotalCoinIn].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalJackpot].MeterValue, actual.Meters[SasMeters.TotalJackpot].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.GamesPlayed].MeterValue, actual.Meters[SasMeters.GamesPlayed].MeterValue);
            Assert.AreEqual(SasMeters.TotalCoinOut, actual.Meters[SasMeters.TotalCoinOut].Meter);
        }
    }
}
