namespace Aristocrat.Monaco.Hhr.Tests.Services
{
	using Client.Data;
    using Gaming.Contracts.Progressives;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hhr.Services.Progressive;
    using System;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Communication;
    using Client.Messages;
    using Client.WorkFlow;
    using Hhr.Services;
    using Application.Contracts;
    using Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameDataServiceTests
    {
        private readonly BehaviorSubject<ConnectionStatus> _connectionStatusSubject =
            new BehaviorSubject<ConnectionStatus>(new ConnectionStatus
            {
                ConnectionState = ConnectionState.Connected
            });

        private readonly uint serialNumber = 123;
        private Mock<ICentralManager> _centralManager;
        private Mock<IConnectionOpener> _connectionOpener;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IProgressiveAssociation> _progressiveAssociation;
        private Mock<IEventBus> _eventBus;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private readonly ParameterResponse _parameterResponse = new ParameterResponse
        {
            GameIds = new[] { 1, 2 },
            GameIdCount = 2
        };

        private readonly GameInfoResponse _gameInfoResponse = new GameInfoResponse()
        {
            ProgressiveIds = new uint[] { 100, 101 },
            RaceTicketSets = new CRaceTicketSets
            {
                TicketSet = new[]
                {
	                new CRacePatterns
	                {
		                RaceTicketSetId = 200020000,
                        Credits = 10,
                        Pattern = new[]
                        {
                            new CRacePattern
                            {
                                Prize = "W=10~R=1~L=1164~P=102~E=1163"
                            }, 
                        }
	                }, 
                }
            }
        };

        private GameDataService _target;

        [TestInitialize]
        public void Initialize()
        {
            _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
            _connectionOpener = new Mock<IConnectionOpener>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _progressiveAssociation = new Mock<IProgressiveAssociation>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);

            _connectionOpener.Setup(x => x.ConnectionStatus).Returns(_connectionStatusSubject);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(serialNumber);

            _target = new GameDataService(_centralManager.Object, _propertiesManager.Object,
                _progressiveAssociation.Object, _eventBus.Object, _protocolLinkedProgressiveAdapter.Object);

            _centralManager.ResetCalls();
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null CentralManager")]
        [DataRow(false, true, false, false, false, DisplayName = "Null PropertiesManager")]
        [DataRow(false, false, true, false, false, DisplayName = "Null ProgressiveAssociation")]
        [DataRow(false, false, false, true, false, DisplayName = "Null EventBus")]
        [DataRow(false, false, false, false, true, DisplayName = "Null ProtocolLinkedProgressiveAdapter")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(bool nullCentralManager, bool nullPropertiesManager,
            bool nullProgressiveAssociation, bool nullEventBus, bool nullProtocolLinkedProgressiveAdapter)
        {
            _ = ConstructGameDataService(nullCentralManager, nullPropertiesManager, nullProgressiveAssociation,
                nullEventBus, nullProtocolLinkedProgressiveAdapter);
        }

        [TestMethod]
        public async Task GetGameInfoWhileCommunicationIsDownExpectNoGameData()
        {
            _centralManager.Setup(x =>
                    x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                        It.IsAny<CancellationToken>()))
                .Throws(new UnexpectedResponseException(new Response(Command.CmdGameRequest)
                {
                    MessageStatus = MessageStatus.Disconnected
                }));

            var gameInfo = await _target.GetGameInfo();
            Assert.IsFalse(gameInfo.Any());
        }

        [TestMethod]
        public async Task GetGameInfoDuringPowerUpExpectGameInfo()
        {
            _centralManager.Setup(x =>
                    x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_parameterResponse));

            _centralManager.Setup(x =>
                    x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_gameInfoResponse));

            var gameInfoList = (await _target.GetGameInfo()).ToList();
            Assert.IsTrue(gameInfoList.Any());
            Assert.IsTrue(gameInfoList[0].PrizeLocations[0][0] == 1164);

            _centralManager.Verify(
                x => x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                    It.IsAny<CancellationToken>()), Times.Exactly(_parameterResponse.GameIds.Length));
            _centralManager.Verify(
                x => x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetProgressiveInfo_DuringPowerUpAssociationFails_ExpectProgressiveInfo()
        {
            // Server Prog Info
            var progInfo = new List<ProgressiveInfoResponse>
            {
                CreateProgressiveResponse(100, 1, 10, 40, 2000),
                CreateProgressiveResponse(101, 2, 11, 40, 1000)
            };

            _centralManager.Setup(x =>
                    x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_parameterResponse));

            _centralManager.Setup(x =>
                    x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_gameInfoResponse));

            var index = 0;

            _progressiveAssociation.Setup(x =>
                    x.AssociateServerLevelsToGame(It.IsAny<ProgressiveInfoResponse>(),
                        It.IsAny<GameInfoResponse>(),
                        It.IsAny<IList<ProgressiveLevelAssignment>>()))
                .Returns(Task.FromResult(false));

            _eventBus.Setup(x => x.Publish(It.IsAny<ProgressiveInitializationFailed>())).Verifiable();

            _centralManager.Setup(x =>
                    x.Send<ProgressiveInfoRequest, ProgressiveInfoResponse>(It.IsAny<ProgressiveInfoRequest>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ProgressiveInfoRequest, CancellationToken>((i, req) =>
                {
                    if (i.ProgressiveId == 100)
                    {
                        index = 0;
                    }

                    if (i.ProgressiveId == 101)
                    {
                        index = 1;
                    }
                })
                .Returns(Task.FromResult(progInfo[index]));

            var gameInfoList = (await _target.GetGameInfo()).ToList();
            Assert.IsTrue(gameInfoList.Any());
            Assert.IsTrue(gameInfoList[0].PrizeLocations[0][0] == 1164);
            

            var progressiveInfo = await _target.GetProgressiveInfo();
            Assert.IsFalse(progressiveInfo.Any());

            _centralManager.Verify(
                x => x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                    It.IsAny<CancellationToken>()), Times.Exactly(_parameterResponse.GameIds.Length));
            _centralManager.Verify(
                x => x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            _centralManager.Verify(
                x => x.Send<ProgressiveInfoRequest,
                    ProgressiveInfoResponse>(It.IsAny<ProgressiveInfoRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(progInfo.Count * _parameterResponse.GameIdCount));

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetProgressiveInfo_DuringPowerUp_ExpectProgressiveInfo()
        {
            // Server Prog Info
            var progInfo = new List<ProgressiveInfoResponse>
            {
                CreateProgressiveResponse(100, 1, 10, 40, 2000),
                CreateProgressiveResponse(101, 2, 11, 40, 1000)
            };

            _centralManager.Setup(x =>
                    x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_parameterResponse));

            _centralManager.Setup(x =>
                    x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_gameInfoResponse));

            var index = 0;

            _progressiveAssociation.Setup(x =>
                    x.AssociateServerLevelsToGame(It.IsAny<ProgressiveInfoResponse>(),
                        It.IsAny<GameInfoResponse>(),
                        It.IsAny<IList<ProgressiveLevelAssignment>>()))
                .Returns(Task.FromResult(true));
            _protocolLinkedProgressiveAdapter.Setup(x =>
                x.AssignLevelsToGame(It.IsAny<IReadOnlyCollection<ProgressiveLevelAssignment>>(),
                    It.IsAny<string>())).Returns(new List<IViewableProgressiveLevel>());

            _propertiesManager.Setup(x => x.SetProperty(ApplicationConstants.WaitForProgressiveInitialization, true));

            _centralManager.Setup(x =>
                    x.Send<ProgressiveInfoRequest, ProgressiveInfoResponse>(It.IsAny<ProgressiveInfoRequest>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ProgressiveInfoRequest, CancellationToken>((i, req) =>
                {
                    if (i.ProgressiveId == 100)
                    {
                        index = 0;
                    }

                    if (i.ProgressiveId == 101)
                    {
                        index = 1;
                    }
                })
                .Returns(Task.FromResult(progInfo[index]));

            var gameInfoList = (await _target.GetGameInfo()).ToList();
            Assert.IsTrue(gameInfoList.Any());
            Assert.IsTrue(gameInfoList[0].PrizeLocations[0][0] == 1164);

            var progressiveInfo = await _target.GetProgressiveInfo();
            Assert.IsTrue(progressiveInfo.Any());

            _centralManager.Verify(
                x => x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
                    It.IsAny<CancellationToken>()), Times.Exactly(_parameterResponse.GameIds.Length));
            _centralManager.Verify(
                x => x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            _centralManager.Verify(
                x => x.Send<ProgressiveInfoRequest,
                    ProgressiveInfoResponse>(It.IsAny<ProgressiveInfoRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(_parameterResponse.GameIds.Length * _gameInfoResponse.ProgressiveIds.Length));

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetPrizeLocationForAPattern_WithInvalidPatternIndex_ExpectNegativeOne()
        {
            var ret = await _target.GetPrizeLocationForAPattern(_gameInfoResponse.GameId,
	            _gameInfoResponse.RaceTicketSets.TicketSet[0].Credits, MessageLengthConstants.MaxNumPatterns + 1);

            Assert.IsTrue(ret == -1);

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetPrizeLocationForAPattern_WithInvalidCredits_ExpectNegativeOne()
        {
	        _centralManager.Setup(x =>
			        x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
				        It.IsAny<CancellationToken>()))
		        .Returns(Task.FromResult(_parameterResponse));

            _centralManager.Setup(x =>
			        x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
				        It.IsAny<CancellationToken>()))
		        .Returns(() => Task.FromResult(_gameInfoResponse));

	        var ret = await _target.GetPrizeLocationForAPattern(_gameInfoResponse.GameId,
		        _gameInfoResponse.RaceTicketSets.TicketSet[0].Credits + 1, 0);

	        Assert.IsTrue(ret == -1);

	        _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetPrizeLocationForAPattern_WithValid_ExpectValidResponse()
        {
	        _centralManager.Setup(x =>
			        x.Send<ParameterRequest, ParameterResponse>(It.IsAny<ParameterRequest>(),
				        It.IsAny<CancellationToken>()))
		        .Returns(Task.FromResult(_parameterResponse));

            _centralManager.Setup(x =>
			        x.Send<GameInfoRequest, GameInfoResponse>(It.IsAny<GameInfoRequest>(),
				        It.IsAny<CancellationToken>()))
		        .Returns(() => Task.FromResult(_gameInfoResponse));

	        var ret = await _target.GetPrizeLocationForAPattern(_gameInfoResponse.GameId,
		        _gameInfoResponse.RaceTicketSets.TicketSet[0].Credits, 0);

	        Assert.IsTrue(ret == 1164);

	        _eventBus.Verify();
        }


        private ProgressiveInfoResponse CreateProgressiveResponse(uint progId, uint progLevel, uint contribution,
            uint creditBet, uint resetValue)
        {
            return new ProgressiveInfoResponse
            {
                ProgressiveId = progId,
                ProgLevel = progLevel,
                ProgContribPercent = contribution,
                ProgCreditsBet = creditBet,
                ProgResetValue = resetValue
            };
        }

        private GameDataService ConstructGameDataService(bool nullCentralManager, bool nullPropertiesManager,
            bool nullProgressiveAssociation, bool nullEventBus, bool nullProtocolLinkedProgressiveAdapter)
        {
            return new GameDataService(nullCentralManager ? null : _centralManager.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullProgressiveAssociation ? null : _progressiveAssociation.Object,
                nullEventBus ? null : _eventBus.Object,
                nullProtocolLinkedProgressiveAdapter ? null : _protocolLinkedProgressiveAdapter.Object);
        }
    }
}