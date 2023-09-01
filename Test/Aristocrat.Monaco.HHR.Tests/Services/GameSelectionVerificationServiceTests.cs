namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using Aristocrat.Monaco.Hhr.Client.Messages;
    using Aristocrat.Monaco.Hhr.Services;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Hhr.Events;

    internal class TestDenomination : IDenomination
    {
        public TestDenomination(long value)
            : this(value, value)
        {
        }

        public TestDenomination(long value, long id, bool active = false)
        {
            Id = id;
            Value = value;
            Active = active;
        }

        public long Id { get; }

        public long Value { get; }

        public bool Active { get; set; }

        public TimeSpan PreviousActiveTime { get; set; }

        public DateTime ActiveDate { get; set; }

        public int MinimumWagerCredits { get; set; }

        public int MaximumWagerCredits { get; set; }

        public int MaximumWagerOutsideCredits { get; set; }

        public string BetOption { get; set; }

        public string LineOption { get; set; }

        public int BonusBet { get; set; }

        public bool SecondaryAllowed { get; set; }

        public bool SecondaryEnabled { get; set; }

        public bool LetItRideAllowed { get; set; }

        public bool LetItRideEnabled { get; set; }

        public bool BetKeeperAllowed { get; set; }

        public long DisplayedValue { get; }
    }

    [TestClass]
    public class GameSelectionVerificationServiceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IGameDataService> _gameDataService;

        private Action<ProtocolInitializationComplete> _sendProtocolInitializationComplete;

        private GameSelectionVerificationService _target;

        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<GameSelectionVerificationService>(), It.IsAny<Action<ProtocolInitializationComplete>>()))
                .Callback<object, Action<ProtocolInitializationComplete>>(
                (tar, act) =>
                {
                    _sendProtocolInitializationComplete = act;
                });

            _target = new GameSelectionVerificationService(_eventBus.Object, _gameProvider.Object, _gameDataService.Object);

            _gameProvider.Setup(m => m.GetEnabledGames())
                .Returns(new List<IGameDetail>());

            _sendProtocolInitializationComplete(new ProtocolInitializationComplete());
        }

        [TestMethod]
        public async Task GameSelectionMade_MatchesServerGames_NoLockup()
        {
            uint gameId = 300100;
            uint denom = 1;

            SetupGameData(gameId, new List<uint> { denom });

            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>(), It.IsAny<IEnumerable<long>>())).Returns(true);

            var gameDetails = new List<IGameDetail>
            {
                // This game was enabled by the operator, and was found on the server
                SetupGameProfile(3, gameId.ToString(), new List<long> { ((long)denom).CentsToMillicents(), 5000, 10000 })
            };

            SetupMockGameProvider(gameDetails);

            _eventBus.Setup(m => m.Publish(It.IsAny<GameSelectionVerificationCompletedEvent>())).Verifiable();

            await _target.Verify();

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GameSelectionMade_OneGameDoesNotExistOnServer_Lockup()
        {
            uint gameId = 300100;
            uint denom = 1;

            SetupGameData(gameId, new List<uint> { denom });

            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>(), It.IsAny<IEnumerable<long>>())).Returns(true);

            var gameDetails = new List<IGameDetail>
            {
                // This game was enabled by the operator, but was not found on the server
                SetupGameProfile(1, "100100", new List<long> { ((long)denom).CentsToMillicents() }),
                // This game was enabled by the operator, and was found on the server
                SetupGameProfile(3, gameId.ToString(), new List<long> { ((long)denom).CentsToMillicents(), 5000, 10000 })
            };

            SetupMockGameProvider(gameDetails);

            _eventBus.Setup(m => m.Publish(It.IsAny<GameSelectionMismatchEvent>())).Verifiable();

            await _target.Verify();

            _eventBus.Verify();
        }

        private void SetupGameData(uint gameId, IList<uint> denominations)
        {
            var gameInfoResponses = new List<GameInfoResponse>();

            foreach (var denom in denominations)
            {
                gameInfoResponses.Add(
                    new GameInfoResponse
                    {
                        GameId = gameId,
                        Denomination = denom
                    });
            }

            _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(() =>
            {
                return Task.FromResult(gameInfoResponses.AsEnumerable());
            });
        }

        private void SetupMockGameProvider(List<IGameDetail> gameDetails)
        {
            _gameProvider.Setup(m => m.EnableGame(It.IsAny<int>(), It.IsAny<GameStatus>()))
                .Callback((int id, GameStatus stat) =>
                {
                    var detail = gameDetails.Single(g => g.Id == id) as MockGameInfo;
                    detail.Status = stat;
                });

            _gameProvider.Setup(m => m.SetActiveDenominations(It.IsAny<int>(), It.IsAny<IEnumerable<long>>()))
                .Callback((int id, IEnumerable<long> denoms) =>
                {
                    var detail = gameDetails.Single(g => g.Id == id) as MockGameInfo;
                    foreach (var denom2 in denoms)
                    {
                        var singleDenom = detail.Denominations.Single(d => d.Value == denom2) as TestDenomination;
                        singleDenom.Active = true;
                    }
                });

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(gameDetails);
        }

        private static IGameDetail SetupGameProfile(
            int id,
            string referenceId,
            IEnumerable<long> denoms)
        {
            var testDenoms = new List<IDenomination>();
            foreach (var denom in denoms)
            {
                testDenoms.Add(new TestDenomination(denom));
            }

            IGameDetail gameDetail = new MockGameInfo
            {
                Id = id,
                Active = true,
                ReferenceId = referenceId,
                Denominations = testDenoms
            };

            return gameDetail;
        }

    }
}
