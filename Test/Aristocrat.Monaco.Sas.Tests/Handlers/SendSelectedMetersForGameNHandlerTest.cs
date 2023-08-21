namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Contains the tests for the SendSelectedMetersForGameNHandler class
    /// </summary>
    [TestClass]
    public class SendSelectedMetersForGameNHandlerTest
    {
        private const int OccurenceMeterLength = 13;
        private const int CurrencyMeterLength = 10;
        private const long BankBalance = 1234000L;
        private const long NonCashBalance = 567000L;
        private const long LifetimePlayCount = 89;
        private const long LifetimeGamesWonCount = 89;
        private const long LifetimeOneCenDenomPlayCount = 50;
        private const long TotalCashableTicketIn = 6789_000;
        private const long EgmPaidGameWonAmount = 1235000L;
        private const int ValidGameId = 3;
        private const int AllGameId = 0;
        private const long OneCentDenom = 1000L;
        private const long FiveCentDenom = 5000L;
        private const int FourBcdMeterLength = 4;
        private const int FiveBcdMeterLength = 5;

        private SendSelectedMetersForGameNHandler _target;
        private Mock<IMeterManager> _meterManager;
        private Mock<IBank> _bank;
        private Mock<IGameMeterManager> _gameMeterManager;
        private Mock<IMeter> _playCountMeter;
        private Mock<IMeter> _gamesWonCountMeter;
        private Mock<IMeter> _oneCentPlayCountMeter;
        private Mock<IMeter> _totalSasCashableTicketInCentsMeter;
        private Mock<IMeter> _emgPaidGameWonMeter;
        private Mock<IGameProvider> _gameProvider;
        private TestMeterClassification _meterClassification;
        private LongPollSelectedMetersForGameNData _data;

        [TestInitialize]
        public void MyTestInitialize()
        {
            const long meterUpperBounds = 10000000000000L;

            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _target = new SendSelectedMetersForGameNHandler(_meterManager.Object, _bank.Object, _gameMeterManager.Object, _gameProvider.Object);
            _playCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _gamesWonCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _oneCentPlayCountMeter = new Mock<IMeter>(MockBehavior.Default);
            _totalSasCashableTicketInCentsMeter = new Mock<IMeter>(MockBehavior.Default);
            _emgPaidGameWonMeter = new Mock<IMeter>(MockBehavior.Default);
            _meterClassification = new TestMeterClassification("Testing", meterUpperBounds);

            _data = new LongPollSelectedMetersForGameNData
            {
                GameNumber = AllGameId,
                AccountingDenom = 1,
                RequestedMeters =
                {
                    SasMeterId.CurrentCredits,
                    SasMeterId.CurrentRestrictedCredits,
                    SasMeterId.GamesPlayed,
                    SasMeterId.GamesWon,
                    SasMeterId.TotalSasCashableTicketInCents,
                    SasMeterId.TotalMachinePaidPaytableWin
                }
            };

            _bank.Setup(m => m.QueryBalance()).Returns(BankBalance);
            _bank.Setup(m => m.QueryBalance(AccountType.NonCash)).Returns(NonCashBalance);
            _playCountMeter.Setup(m => m.Lifetime).Returns(LifetimePlayCount);
            _playCountMeter.Setup(m => m.Classification).Returns(_meterClassification);
            _emgPaidGameWonMeter.Setup(m => m.Lifetime).Returns(EgmPaidGameWonAmount);
            _emgPaidGameWonMeter.Setup(m => m.Classification).Returns(_meterClassification);
            _gamesWonCountMeter.Setup(m => m.Lifetime).Returns(LifetimeGamesWonCount);
            _gamesWonCountMeter.Setup(m => m.Classification).Returns(_meterClassification);
            _oneCentPlayCountMeter.Setup(m => m.Lifetime).Returns(LifetimeOneCenDenomPlayCount);
            _oneCentPlayCountMeter.Setup(m => m.Classification).Returns(_meterClassification);
            _totalSasCashableTicketInCentsMeter.Setup(m => m.Lifetime).Returns(TotalCashableTicketIn);
            _totalSasCashableTicketInCentsMeter.Setup(m => m.Classification).Returns(_meterClassification);
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.PlayedCount)).Returns(true);
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.TotalVoucherInCashableAndPromoAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, OneCentDenom, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, FiveCentDenom, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, GamingMeters.PlayedCount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, OneCentDenom, GamingMeters.PlayedCount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ValidGameId, FiveCentDenom, GamingMeters.PlayedCount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(OneCentDenom, GamingMeters.WonCount)).Returns(true);
            _gameMeterManager.Setup(m => m.IsMeterProvided(OneCentDenom, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.GetMeter(OneCentDenom, GamingMeters.WonCount)).Returns(_gamesWonCountMeter.Object);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.PlayedCount)).Returns(_playCountMeter.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.TotalVoucherInCashableAndPromoAmount)).Returns(_totalSasCashableTicketInCentsMeter.Object);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.TotalEgmPaidGameWonAmount)).Returns(_emgPaidGameWonMeter.Object);
            _gameMeterManager.Setup(m => m.GetMeter(ValidGameId, GamingMeters.PlayedCount)).Returns(_playCountMeter.Object);
            _gameMeterManager.Setup(m => m.GetMeter(ValidGameId, OneCentDenom, GamingMeters.PlayedCount)).Returns(_oneCentPlayCountMeter.Object);
            _gameMeterManager.Setup(m => m.GetMeter(ValidGameId, OneCentDenom, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(_emgPaidGameWonMeter.Object);
            _gameMeterManager.Setup(m => m.GetMeter(ValidGameId, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(_emgPaidGameWonMeter.Object);
            _gameMeterManager.Setup(m => m.GetMeter(OneCentDenom, GamingMeters.TotalEgmPaidGameWonAmount)).Returns(_emgPaidGameWonMeter.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterManagerTest()
        {
            _target = new SendSelectedMetersForGameNHandler(null, _bank.Object, _gameMeterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBankTest()
        {
            _target = new SendSelectedMetersForGameNHandler(_meterManager.Object, null, _gameMeterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameMeterManagerTest()
        {
            _target = new SendSelectedMetersForGameNHandler(_meterManager.Object, _bank.Object, null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameProviderTest()
        {
            _target = new SendSelectedMetersForGameNHandler(_meterManager.Object, _bank.Object, _gameMeterManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            var expectedCommands = new List<LongPoll>
            {
                LongPoll.SendSelectedMetersForGameN,
                LongPoll.SendExtendedMetersForGameN,
                LongPoll.SendExtendedMetersForGameNAlternate
            };

            CollectionAssert.AreEquivalent(expectedCommands, _target.Commands);
        }

        [TestMethod]
        public void HandleMachineMetersTest()
        {
            var expected = new SendSelectedMetersForGameNResponse(
                new List<SelectedMeterForGameNResponse>
                {
                    new SelectedMeterForGameNResponse(SasMeterId.CurrentCredits, (ulong)BankBalance.MillicentsToCents(), FourBcdMeterLength, SasConstants.MaxMeterLength),
                    new SelectedMeterForGameNResponse(SasMeterId.CurrentRestrictedCredits, (ulong)NonCashBalance.MillicentsToCents(), FourBcdMeterLength, SasConstants.MaxMeterLength),
                    new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, LifetimePlayCount, FourBcdMeterLength, OccurenceMeterLength),
                    new SelectedMeterForGameNResponse(SasMeterId.TotalSasCashableTicketInCents, (ulong)TotalCashableTicketIn.MillicentsToCents(), FiveBcdMeterLength, CurrencyMeterLength)
                });

            _data.GameNumber = AllGameId;
            var actual = _target.Handle(_data);

            ValidateMeters(expected.SelectedMeters, actual.SelectedMeters);
        }

        [TestMethod]
        public void HandleGameMetersTest()
        {
            const long denomId = 34;
            var expected = new SendSelectedMetersForGameNResponse(
                new List<SelectedMeterForGameNResponse>
                {
                    new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, LifetimeOneCenDenomPlayCount, FourBcdMeterLength, OccurenceMeterLength),
                    new SelectedMeterForGameNResponse(SasMeterId.TotalMachinePaidPaytableWin, (ulong)EgmPaidGameWonAmount.MillicentsToCents(), FourBcdMeterLength, CurrencyMeterLength)
                });

            _data.GameNumber = denomId;

            var testGame = new TestGameProfile
            {
                Id = ValidGameId,
                Denominations = new List<IDenomination> { new MockDenomination(OneCentDenom, denomId) }
            };

            _gameProvider.Setup(x => x.GetGame((int)_data.GameNumber)).Returns(testGame);
            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<IGameDetail> { testGame });
            var actual = _target.Handle(_data);
            ValidateMeters(expected.SelectedMeters, actual.SelectedMeters);
        }

        [TestMethod]
        public void HandleDenomMetersTest()
        {
            var expected = new SendSelectedMetersForGameNResponse(
                new List<SelectedMeterForGameNResponse>
                {
                    new SelectedMeterForGameNResponse(SasMeterId.GamesWon, LifetimeGamesWonCount, FourBcdMeterLength, OccurenceMeterLength),
                    new SelectedMeterForGameNResponse(SasMeterId.TotalMachinePaidPaytableWin, (ulong)EgmPaidGameWonAmount.MillicentsToCents(), FourBcdMeterLength, CurrencyMeterLength)
                });

            _data.GameNumber = AllGameId;
            _data.TargetDenomination = OneCentDenom.MillicentsToCents();

            var testGame = new TestGameProfile() { Denominations = new List<IDenomination> { new MockDenomination(OneCentDenom) } };
            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<TestGameProfile> { testGame });
            _gameProvider.Setup(x => x.GetGame((int)_data.GameNumber)).Returns(testGame);

            var actual = _target.Handle(_data);
            ValidateMeters(expected.SelectedMeters, actual.SelectedMeters);
        }

        [TestMethod]
        public void HandleInvalidGameIdTest()
        {
            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.InvalidMeter);
            _data.GameNumber = ValidGameId;

            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<IGameDetail>());
            var actual = _target.Handle(_data);

            Assert.AreEqual(0, actual.SelectedMeters.Count);
        }

        [TestMethod]
        public void HandleUnsupportedMeterTest()
        {
            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.InvalidMeter);

            var actual = _target.Handle(_data);

            Assert.AreEqual(0, actual.SelectedMeters.Count);
        }

        [TestMethod]
        public void HandleGameMetersWhenMultiDenomPollTest()
        {
            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.GamesPlayed);
            _data.GameNumber = ValidGameId;
            _data.MultiDenomPoll = true;
            _data.TargetDenomination = OneCentDenom.MillicentsToCents();

            var testGame = new TestGameProfile() { Denominations = new List<IDenomination> { new MockDenomination(OneCentDenom) } };
            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<TestGameProfile> { testGame });
            _gameProvider.Setup(x => x.GetGame((int)_data.GameNumber)).Returns(testGame);

            var actual = _target.Handle(_data);
            Assert.AreEqual(MultiDenomAwareErrorCode.SpecificDenomNotSupported, actual.ErrorCode);
            Assert.AreEqual(0, actual.SelectedMeters.Count);
        }

        [TestMethod]
        public void HandleGameMetersWhenInvalidDenomPollTest()
        {
            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.GamesPlayed);
            _data.GameNumber = ValidGameId;
            _data.MultiDenomPoll = true;
            _data.TargetDenomination = OneCentDenom.MillicentsToCents();

            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<TestGameProfile>());
            _gameProvider.Setup(x => x.GetGame((int)_data.GameNumber)).Returns(new TestGameProfile());
            var actual = _target.Handle(_data);
            Assert.AreEqual(MultiDenomAwareErrorCode.NotValidPlayerDenom, actual.ErrorCode);
            Assert.AreEqual(0, actual.SelectedMeters.Count);
        }

        [TestMethod]
        public void HandleGameDenomMetersTest()
        {
            const long denomId = 43;
            var expected = new SendSelectedMetersForGameNResponse(
                new List<SelectedMeterForGameNResponse>
                {
                    new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, LifetimeOneCenDenomPlayCount, FourBcdMeterLength, OccurenceMeterLength)
                });

            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.GamesPlayed);
            _data.GameNumber = denomId;
            _data.TargetDenomination = OneCentDenom.MillicentsToCents();

            var testGame = new TestGameProfile
            {
                Id = ValidGameId,
                Denominations = new List<IDenomination> { new MockDenomination(OneCentDenom, denomId) }
            };
            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<TestGameProfile> { testGame });
            _gameProvider.Setup(x => x.GetGame((int)_data.GameNumber)).Returns(testGame);

            var actual = _target.Handle(_data);
            ValidateMeters(expected.SelectedMeters, actual.SelectedMeters);
        }

        [TestMethod]
        public void HandleUnsupportedGameDenomMetersTest()
        {
            _data.RequestedMeters.Clear();
            _data.RequestedMeters.Add(SasMeterId.GamesPlayed);
            _data.GameNumber = ValidGameId;
            _data.TargetDenomination = FiveCentDenom.MillicentsToCents();

            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<TestGameProfile>());

            var actual = _target.Handle(_data);

            Assert.AreEqual(0, actual.SelectedMeters.Count);
        }

        private static void ValidateMeters(
            IReadOnlyCollection<SelectedMeterForGameNResponse> expected,
            IReadOnlyCollection<SelectedMeterForGameNResponse> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var response in actual)
            {
                var expectedResponse = expected.FirstOrDefault(x => x.MeterCode == response.MeterCode);
                Assert.IsNotNull(expectedResponse);
                Assert.AreEqual(expectedResponse.MeterLength, response.MeterLength);
                Assert.AreEqual(expectedResponse.MeterValue, response.MeterValue);
                Assert.AreEqual(expectedResponse.MinMeterLength, response.MinMeterLength);
            }
        }
    }
}
