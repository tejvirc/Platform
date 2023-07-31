using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Hhr.Client.Messages;
using Aristocrat.Monaco.Hhr.Client.WorkFlow;
using Aristocrat.Monaco.Hhr.Services;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Kernel;

namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    
    [TestClass]
    public class PlayerSessionServiceTests
    {
        private const double InactivityInterval = 3000.0;

        private readonly Mock<ICentralManager> _mockManager = new Mock<ICentralManager>(MockBehavior.Strict);
        private readonly Mock<IEventBus> _mockEventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _mockPlayerBank = new Mock<IPlayerBank>(MockBehavior.Strict);

        private Action<TransferOutCompletedEvent> _sendTransferOut;
        private Action<BankBalanceChangedEvent> _sendBalanceChanged;

        private PlayerSessionService _playService;

        private int _requestEventCount = 0;
        private string _nextWaitingId = "PLAYERID01";

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up the central manager so we can monitor for player ID requests being sent over the connection.
            _mockManager.Setup(
                m => m.Send<PlayerIdRequest, PlayerIdResponse>(
                    It.IsAny<PlayerIdRequest>(), It.IsAny<CancellationToken>()))
                    .Callback(() => HandleMessage())
                    .ReturnsAsync(() => GetNextWaitingId());
            _requestEventCount = 0;

            // Set up the event bus so we can capture the handlers it will use to subscribe to events, and pretend the events
            // happened in order to drive the service.
            _mockEventBus.Setup(m => m.Subscribe(It.IsAny<PlayerSessionService>(), It.IsAny<Action<TransferOutCompletedEvent>>()))
                .Callback<object, Action<TransferOutCompletedEvent>>(
                    (tar, act) =>
                    {
                        _sendTransferOut = act;
                    });

            _mockEventBus.Setup(m => m.Subscribe(It.IsAny<PlayerSessionService>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>(
                    (tar, act) =>
                    {
                        _sendBalanceChanged = act;
                    });

            // Now we can create the service which will be tested in each test.
            _playService = new PlayerSessionService(
                _mockManager.Object,
                _mockEventBus.Object,
                _mockPlayerBank.Object);

            _playService.InactivityIntervalMillis = InactivityInterval;
        }

        [TestMethod]
        public async Task Call_WithServer_ShouldFetch()
        {
            Assert.AreEqual(0, _requestEventCount);
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
        }

        [TestMethod]
        public async Task Call_AfterCashout_ShouldRenew()
        {
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
            _nextWaitingId = "PLAYERID02";
            _mockPlayerBank.SetupGet(m => m.Balance).Returns(0);
            _sendTransferOut(null);
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(2, _requestEventCount);
        }

        [TestMethod]
        public async Task Call_CreditZeroed_ShouldWaitAndRenew()
        {
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
            _sendBalanceChanged(new BankBalanceChangedEvent(1, 0, Guid.Empty));
            _mockPlayerBank.SetupGet(m => m.Balance).Returns(0);
            Thread.Sleep((int)(InactivityInterval * 0.6)); // Wait until the timeout is half over.
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
            _nextWaitingId = "PLAYERID02";
            Thread.Sleep((int)(InactivityInterval * 0.6)); // Wait until the timeout is well over.
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(2, _requestEventCount);
        }

        [TestMethod]
        public async Task Call_CreditAdded_ShouldCache()
        {
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
            _sendBalanceChanged(new BankBalanceChangedEvent(1, 0, Guid.Empty));
            Thread.Sleep((int)(InactivityInterval * 0.6)); // Wait until the timeout is half over.
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
            _sendBalanceChanged(new BankBalanceChangedEvent(0, 1, Guid.Empty));
            Thread.Sleep((int)(InactivityInterval * 0.6)); // Wait until the timeout is well over.
            Assert.AreEqual(_nextWaitingId, await _playService.GetCurrentPlayerId());
            Assert.AreEqual(1, _requestEventCount);
        }

        [TestMethod]
        public async Task Call_WithNoServer_ShouldThrow()
        {
            // Set up so that we think we didn't get a response when we asked for a new ID.
            _mockManager.Setup(
                m => m.Send<PlayerIdRequest, PlayerIdResponse>(
                    It.IsAny<PlayerIdRequest>(), It.IsAny<CancellationToken>()))
                    .Callback(() => HandleMessage())
                    .Throws(new UnexpectedResponseException(new EmptyResponse()));

            Func<Task> func = () => _playService.GetCurrentPlayerId();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
        }

        private void HandleMessage() => _requestEventCount++;

        private PlayerIdResponse GetNextWaitingId()
        {
            return new PlayerIdResponse() { PlayerId = _nextWaitingId };
        }
    }
}
