namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Gaming.Contracts;
    using Client.Data;
    using Client.Messages;
    using Hardware.Contracts.Button;
    using Hhr.Services;
    using Hhr.UI.ViewModels;
    using Kernel;
    using Test.Common;
    using Moq;
    using Command = Menu.Command;

    [TestClass]
    public class WinningCombinationPageViewModelTest
    {
        private const int NumberOfCredits = 10;
        private const int RaceTicketSetId = 1;
        private const string Prize = "W=40~R=2~L=186~P=167~E=56A ~PW=0001";
        private int _buttonId = -1;
        private int _currentBetIndex;
        private int _currentPatternIndex;
        private Mock<IEventBus> _eventBus;

        private Mock<IGameDataService> _gameDataService;
        private readonly int _gameId = 1;
        private GameInfoResponse _gameInfoResponse;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _systemDisable;
        private RacePariResponse _racePariResponse;
        private WinningCombinationPageViewModel _target;
        private int _indexToReturn;

        [TestInitialize]
        public void TestInitialization()
        {
            _systemDisable = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _eventBus.Setup(m => m.Publish(It.IsAny<DownEvent>()))
                .Callback((DownEvent ev) => { _buttonId = ev.LogicalId; });

            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(It.IsAny<int>());
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedBetCredits, It.IsAny<long>()))
                .Returns((long)NumberOfCredits);

            _gameDataService
                .Setup(x => x.GetPrizeLocationForAPattern(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<int>()))
                .Callback((uint gameId, uint credits, int index) =>
                {
                    _indexToReturn = index;
                })
                .Returns(() => Task.FromResult(_indexToReturn));


            if (System.Windows.Application.Current == null)
                // ReSharper disable once ObjectCreationAsStatement
                new System.Windows.Application();

            _target = new WinningCombinationPageViewModel(
                _gameDataService.Object,
                _gameProvider.Object,
                _propertiesManager.Object,
                _eventBus.Object,
                _systemDisable.Object);

            _gameInfoResponse = GetSampleGameInfoResponse(1);
            _racePariResponse = GetSampleRacePariResponse();

            var gameDetails = new List<IGameDetail>
            {
                // This game was enabled by the operator, and was found on the server
                SetupGameProfile(_gameId, _gameId.ToString())
            };

            _gameProvider.Setup(x => x.GetGame(It.IsAny<int>())).Returns(gameDetails[0]);

            _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(() =>
                Task.FromResult(new List<GameInfoResponse>
                {
                    _gameInfoResponse
                }.AsEnumerable()));

            _gameDataService.Setup(x => x.GetRaceInformation(It.IsAny<uint>(), 0, 0))
                .Returns(() => Task.FromResult(_racePariResponse));

            _target.Init(Command.WinningCombination);

            // Reset the bet up/down delay counter so that we don't hit the limit while testing.
            HostPageViewModelManager.ResetBetButtonDelayLimit(
                DateTime.UtcNow.Ticks - HostPageViewModelManager.BetUpDownDelayTimeTicks - 1);
        }

        private static IGameDetail SetupGameProfile(
            int id,
            string referenceId)
        {
            IGameDetail gameDetail = new MockGameInfo
            {
                Id = id,
                Active = true,
                ReferenceId = referenceId
            };

            return gameDetail;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }


        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyCurrentPattern()
        {
            Assert.AreEqual(_target.CurrentWinningPattern.Patterns[0],
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].Pattern1);
            Assert.AreEqual(_target.CurrentWinningPattern.Patterns[1],
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].Pattern2);
            Assert.AreEqual(_target.CurrentWinningPattern.Patterns[2],
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].Pattern3);
            Assert.AreEqual(_target.CurrentWinningPattern.Patterns[3],
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].Pattern4);
            Assert.AreEqual(_target.CurrentWinningPattern.Patterns[4],
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].Pattern5);
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyRaceGroup()
        {
            Assert.AreEqual(_target.CurrentWinningPattern.RaceSet,
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].RaceGroup);
        }

        [TestMethod]
        public async Task WhenWeCannotFetchGameInfoResponse_Init_VerifyRaceInfoCalledOnce()
        {
            const int gameId = 2;
            var gameDetails = new List<IGameDetail>
            {
                // This game was enabled by the operator, and was found on the server
                SetupGameProfile(gameId, gameId.ToString())
            };

            _gameProvider.Setup(x => x.GetGame(It.IsAny<int>())).Returns(gameDetails[0]);

            _gameDataService.Setup(x => x.GetGameInfo(It.IsAny<bool>())).Returns(() =>
                Task.FromResult(new List<GameInfoResponse>
                {
                    _gameInfoResponse
                }.AsEnumerable()));

            _gameDataService.Setup(x => x.GetRaceInformation(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(_racePariResponse)).Verifiable();

            await _target.Init(Command.WinningCombination);

            //The first time it will be called from the initializer
            _gameDataService.Verify(x => x.GetRaceInformation(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<uint>()),
                Times.Once);
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyGuaranteedCredits()
        {
            Assert.AreEqual(_target.CurrentWinningPattern.GuaranteedCredits,
                _gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex].PrizeValue / _gameInfoResponse.Denomination);
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyExtraWinnings()
        {
            Assert.AreEqual(_target.CurrentWinningPattern.ExtraWinnings,
                (double)((long)(_racePariResponse.TemplatePool[_currentBetIndex].PrizeDataRace[_currentPatternIndex])).CentsToDollars());
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyExpectedProgressive()
        {
            Assert.AreEqual(_target.CurrentWinningPattern.IncludesProgressiveResetValues,
                !string.IsNullOrEmpty(_gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex]
                    .Pattern[_currentPatternIndex].Prize
                    .GetPrizeString(HhrConstants.ProgressiveInformation)));
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyExpectWager()
        {
            // int index = 0; 
            Assert.AreEqual(_target.CurrentWinningPattern.Wager,
                long.Parse(_gameInfoResponse.RaceTicketSets.TicketSet[_currentBetIndex].Pattern[_currentPatternIndex]
                        .Prize.GetPrizeString(HhrConstants.Wager)).CentsToDollars()
                    .FormattedCurrencyString());
        }

        [TestMethod]
        public void WinningCombinationPageLoaded_Init_VerifyAllButtonsPresent()
        {
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.Bet) != 0);
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.ExitHelp) != 0);
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.Next) != 0);
        }

        [TestMethod]
        public void ClickButton_Next_SetCurrentPattern()
        {
            var cmd = _target.Commands.FirstOrDefault(x => x.Command == Command.Next);
            cmd?.Execute(Command.Next);
            _currentPatternIndex++;
            VerifyAllCombinationItems();
        }

        [TestMethod]
        public void ClickButton_IncrementBet_SetCurrentPattern()
        {
            var cmd = _target.Commands.FirstOrDefault(x => x.Command == Command.Bet);
            cmd?.Execute(Command.BetUp);
            _currentBetIndex++;
            VerifyAllCombinationItems();
        }

        [TestMethod]
        public void ClickButton_DecrementBet_SetCurrentPattern()
        {
            var cmd = _target.Commands.FirstOrDefault(x => x.Command == Command.Bet);
            cmd?.Execute(Command.BetDown);
            VerifyAllCombinationItems();
        }

        [TestMethod]
        public void ClickButton_BetUp_ExpectDownEventWithBetUP()
        {
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetUp);
            Assert.AreEqual((int)ButtonLogicalId.BetUp, _buttonId);
        }

        [TestMethod]
        public void ClickButton_BetDown_ExpectDownEventWithBetDown()
        {
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetDown);
            Assert.AreEqual((int)ButtonLogicalId.BetDown, _buttonId);
        }

        private void VerifyAllCombinationItems()
        {
            WinningCombinationPageLoaded_Init_VerifyExpectWager();
            WinningCombinationPageLoaded_Init_VerifyExpectedProgressive();
            WinningCombinationPageLoaded_Init_VerifyGuaranteedCredits();
            WinningCombinationPageLoaded_Init_VerifyExtraWinnings();
            WinningCombinationPageLoaded_Init_VerifyRaceGroup();
            WinningCombinationPageLoaded_Init_VerifyCurrentPattern();
        }

        private RacePariResponse GetSampleRacePariResponse()
        {
            return new RacePariResponse
            {
                TemplatePool = new[]
                {
                    new CTemplatePool
                    {
                        PrizeDataRace = new[] {1, 2, 3, 4, 5}
                    },
                    new CTemplatePool
                    {
                        PrizeDataRace = new[] {6, 7, 8, 9, 10}
                    }
                }
            };
        }

        private GameInfoResponse GetSampleGameInfoResponse(uint gameId)
        {
            return new GameInfoResponse
            {
                GameId = gameId,
                MaxLines = 40,
                Denomination = 2,
                RaceTicketSets = new CRaceTicketSets
                {
                    TicketSet = new[]
                    {
                        new CRacePatterns
                        {
                            Credits = NumberOfCredits,
                            Line = 40,
                            RaceTicketSetId = RaceTicketSetId,
                            Pattern = new[]
                            {
                                new CRacePattern
                                {
                                    Pattern1 = 1,
                                    Pattern2 = 2,
                                    Pattern3 = 3,
                                    Pattern4 = 4,
                                    Pattern5 = 5,
                                    RaceGroup = 1,
                                    Prize = Prize,
                                    PrizeValue = 100
                                },
                                new CRacePattern
                                {
                                    Pattern5 = 32,
                                    Pattern4 = 35,
                                    Pattern3 = 123,
                                    Pattern2 = 178,
                                    Pattern1 = 56,
                                    RaceGroup = 2,
                                    Prize = Prize,
                                    PrizeValue = 200
                                }
                            }
                        },
                        new CRacePatterns
                        {
                            Credits = 400,
                            Line = 40,
                            RaceTicketSetId = RaceTicketSetId + 1,
                            Pattern = new[]
                            {
                                new CRacePattern
                                {
                                    Pattern1 = 3,
                                    Pattern2 = 6,
                                    Pattern3 = 8,
                                    Pattern4 = 9,
                                    Pattern5 = 3,
                                    RaceGroup = 1,
                                    Prize = Prize,
                                    PrizeValue = 300
                                },
                                new CRacePattern
                                {
                                    Pattern5 = 23,
                                    Pattern4 = 45,
                                    Pattern3 = 18,
                                    Pattern2 = 108,
                                    Pattern1 = 65,
                                    RaceGroup = 2,
                                    Prize = Prize,
                                    PrizeValue = 400
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}