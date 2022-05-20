using System;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Hardware.Contracts.Button;
using Aristocrat.Monaco.Hhr.Client.Data;
using Aristocrat.Monaco.Hhr.Client.Messages;
using Aristocrat.Monaco.Hhr.Client.WorkFlow;
using Aristocrat.Monaco.Hhr.Events;
using Aristocrat.Monaco.Hhr.Exceptions;
using Aristocrat.Monaco.Hhr.Services;
using Aristocrat.Monaco.Hhr.Storage.Helpers;
using Aristocrat.Monaco.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Timers;

namespace Aristocrat.Monaco.Hhr.Tests.Services
{
	[TestClass]
	public class GameRecoveryServiceTests
	{
		private readonly Mock<ICentralManager> _centralManager = new Mock<ICentralManager>(MockBehavior.Default);
		private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
		private readonly Mock<IGameDataService> _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);
		private readonly uint _gameId = 1, _default = 10;

		private readonly Mock<IGamePlayEntityHelper> _gamePlayEntityHelper =
			new Mock<IGamePlayEntityHelper>(MockBehavior.Default);

		private readonly ushort _numberOfCredits = 40, _linesPlayed = 40;

		private Action<DownEvent> _jackpotKeyAction;

		private GameRecoveryService _target;
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);

        [TestInitialize]
		public void TestInitialize()
		{
			_centralManager.Setup(x =>
				x.Send<GameRecoveryRequest, GameRecoveryResponse>(It.IsAny<GameRecoveryRequest>(),
					It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(new GameRecoveryResponse
					{
						GameId = _default,
						PrizeLoc1 = 0,
						PrizeLoc2 = 0,
						RaceTicketSetId = _default,
						RaceTicketId = _default,
						LastGamePlayTime = _default
					}
				));

			_gameDataService.Setup(x => x.GetRacePatterns(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<uint>()))
				.Returns(Task.FromResult(new CRacePatterns()));

			_eventBus.Setup(x =>
					x.Subscribe(It.IsAny<object>(), It.IsAny<Action<DownEvent>>(), It.IsAny<Predicate<DownEvent>>()))
				.Callback<object, Action<DownEvent>, Predicate<DownEvent>>((c, a, p) => { _jackpotKeyAction = a; });

            _eventBus.Setup(x => x.Publish(It.IsAny<GamePlayRequestFailedEvent>()))
                .Callback<GamePlayRequestFailedEvent>(evt =>
                {
                    _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>
                    {
                        HhrConstants.GamePlayRequestFailedKey
                    });
                });

            _target = new GameRecoveryService(_centralManager.Object,
				_gameDataService.Object, _gamePlayEntityHelper.Object, _eventBus.Object, _systemDisableManager.Object);
		}

		[DataTestMethod]
		[DataRow(true, false, false, false, false, true, DisplayName = "Null CentralManager")]
		[DataRow(false, true, false, false, false, true, DisplayName = "Null GameDataService")]
		[DataRow(false, false, true, false, false, true, DisplayName = "Null GamePlayEntiryHelper")]
		[DataRow(false, false, false, true, false, true, DisplayName = "Null EventBus")]
		[DataRow(false, false, false, false, true, true, DisplayName = "Null SystemDisableManager")]
		[DataRow(false, false, false, false, false, false, DisplayName = "All non-null services, expect success.")]
		public void GameRecoveryServiceConstructorTestExpectException(bool nullCentralManager,
			bool nullGameDataService, bool nullGamePlayEntiryHelper, bool nullEventBus, bool nullSystemDisableManager, bool throwException)
		{
			if (throwException)
				Assert.ThrowsException<ArgumentNullException>(() =>
					GetGameRecoveryService(nullCentralManager, nullGameDataService, nullGamePlayEntiryHelper,
						nullEventBus, nullSystemDisableManager));
			else
				Assert.IsNotNull(GetGameRecoveryService(nullCentralManager, nullGameDataService,
					nullGamePlayEntiryHelper, nullEventBus, nullSystemDisableManager));
		}

		[TestMethod]
		public void NoGamePlayRequestPendingStartRecoveryExpectFailure()
		{
			_gamePlayEntityHelper.Setup(x => x.GamePlayRequest).Returns(() => null);
			_gamePlayEntityHelper.Setup(x => x.GamePlayResponse).Returns(() => null);

			Assert.ThrowsExceptionAsync<GameRecoveryFailedException>(async () => await _target.Recover());
		}

		[DataRow(1u, 1u,
			DisplayName = "Request with SequenceId - 1 and Response with Reply Id - 1. Response already received.")]
		[DataRow(1u, 2u,
			DisplayName = "Request with SequenceId - 1 and Response with Reply Id - 2. Response not received.")]
		[DataTestMethod]
		public async Task GamePlayRequestPendingStartRecoveryExpectGamePlayResponse(uint sequenceId, uint replyId)
		{
			_gamePlayEntityHelper.Setup(x => x.GamePlayRequest).Returns(PopulateGamePlayRequest(sequenceId));
			_gamePlayEntityHelper.Setup(x => x.GamePlayResponse).Returns(PopulateGamePlayResponse(replyId));

			var response = await _target.Recover();
			// When we already have a response, we dont request for Recovery message.
			if (sequenceId == replyId)
			{
				Assert.AreEqual(replyId, response.ReplyId);
				_centralManager.Verify(
					x => x.Send<GameRecoveryRequest, GameRecoveryResponse>(It.IsAny<GameRecoveryRequest>(),
						It.IsAny<CancellationToken>()), Times.Never);
			}
			else
			{
				_centralManager.Verify(
					x => x.Send<GameRecoveryRequest, GameRecoveryResponse>(It.IsAny<GameRecoveryRequest>(),
						It.IsAny<CancellationToken>()), Times.Once);

				_eventBus.Verify(x => x.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Never);

				Assert.AreEqual(response.ScratchTicketId, _default);
				Assert.AreEqual(response.ScratchTicketSetId, _default);
				Assert.AreEqual(response.LastGamePlayTime, _default);
				Assert.AreEqual(response.Prize, "P=0");
			}
		}

		[TestMethod]
        [ExpectedException(typeof(UnexpectedResponseException))]
		public async Task GamePlayRequestPendingServerSendsInvalidResponseExpectFailure()
		{
			_gamePlayEntityHelper.Setup(x => x.GamePlayRequest).Returns(PopulateGamePlayRequest(1));
			_gamePlayEntityHelper.Setup(x => x.GamePlayResponse).Returns(() => null);

			_centralManager
				.Setup(x => x.Send<GameRecoveryRequest, GameRecoveryResponse>(It.IsAny<GameRecoveryRequest>(),
					It.IsAny<CancellationToken>())).Throws(new UnexpectedResponseException(new Response()));

			var token = new CancellationTokenSource();

			await _target.Recover(token.Token);
			_centralManager.Verify(x => x.Send<GameRecoveryRequest, GameRecoveryResponse>(
					It.IsAny<GameRecoveryRequest>(),
					It.IsAny<CancellationToken>()), Times.AtLeastOnce);

			_eventBus.Verify(x => x.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Once);

            // Toggle jackpot key
            _jackpotKeyAction.Invoke(new DownEvent());
            _systemDisableManager.Verify(x => x.Enable(HhrConstants.GamePlayRequestFailedKey), Times.Once);
        }

		[TestMethod]
        [ExpectedException(typeof(GameRecoveryFailedException))]
		public async Task GamePlayRequestPendingServerUnableToRecogniseGameExpectFailure()
		{
			_gamePlayEntityHelper.Setup(x => x.GamePlayRequest).Returns(PopulateGamePlayRequest(1));
			_gamePlayEntityHelper.Setup(x => x.GamePlayResponse).Returns(() => null);

			_centralManager.Setup(x =>
				x.Send<GameRecoveryRequest, GameRecoveryResponse>(It.IsAny<GameRecoveryRequest>(),
					It.IsAny<CancellationToken>())).Returns(
				Task.FromResult(new GameRecoveryResponse
					{
						GameId = 0
					}
				));

			await _target.Recover();
			_eventBus.Verify(x => x.Publish(It.IsAny<GamePlayRequestFailedEvent>()), Times.Once);
			_centralManager.Verify(x => x.Send<GameRecoveryRequest, GameRecoveryResponse>(
				It.IsAny<GameRecoveryRequest>(),
				It.IsAny<CancellationToken>()), Times.Once);

            // Toggle jackpot key
            _jackpotKeyAction.Invoke(new DownEvent());
            _systemDisableManager.Verify(x => x.Enable(HhrConstants.GamePlayRequestFailedKey), Times.Once);
        }

		private GameRecoveryService GetGameRecoveryService(bool nullCentralManager, bool nullGameDataService,
			bool nullGamePlayEntityHelper, bool nullEventBus, bool nullSystemDisableManager)
		{
            return new GameRecoveryService(
                nullCentralManager ? null : _centralManager.Object,
                nullGameDataService ? null : _gameDataService.Object,
                nullGamePlayEntityHelper ? null : _gamePlayEntityHelper.Object,
                nullEventBus ? null : _eventBus.Object,
                nullSystemDisableManager ? null : _systemDisableManager.Object);
		}

		private GamePlayResponse PopulateGamePlayResponse(uint replyId)
		{
			return new GamePlayResponse
			{
				ReplyId = replyId
			};
		}

        private void ToggleJackpotKey(object sender, ElapsedEventArgs e)
        {
            _jackpotKeyAction.Invoke(new DownEvent());
        }

        private GamePlayRequest PopulateGamePlayRequest(uint sequenceId)
		{
			return new GamePlayRequest
			{
				GameId = _gameId,
				CreditsPlayed = _numberOfCredits,
				LinesPlayed = _linesPlayed,
				SequenceId = sequenceId
			};
		}
	}
}