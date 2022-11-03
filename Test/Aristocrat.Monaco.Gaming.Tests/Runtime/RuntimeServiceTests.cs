/*namespace Aristocrat.Monaco.Gaming.Tests.Runtime
{
    using System;
    using Contracts;
    using Contracts.Process;
    using Gaming.Commands;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Gaming.Runtime.Server;
    using GDKRuntime.Contract;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RuntimeServiceTests
    {
        private Mock<IEventBus> _bus;
        private Mock<ICommandHandlerFactory> _factory;
        private Mock<IGameDiagnostics> _gameReplay;
        private Mock<IClientEndpointProvider<IRuntime>> _runtimeProvider;
        private Mock<IClientEndpointProvider<IReelService>> _reelsProvider;
        private Mock<IClientEndpointProvider<IPresentationService>> _presentationProvider;
        private Mock<IProcessManager> _process;
        private WcfService _service;

        [TestInitialize]
        public void Initialize()
        {
            _bus = new Mock<IEventBus>();
            _factory = new Mock<ICommandHandlerFactory>();
            _gameReplay = new Mock<IGameDiagnostics>();
            _runtimeProvider = new Mock<IClientEndpointProvider<IRuntime>>();
            _reelsProvider = new Mock<IClientEndpointProvider<IReelService>>();
            _presentationProvider = new Mock<IClientEndpointProvider<IPresentationService>>();
            _process = new Mock<IProcessManager>();

            InitService();
        }

        private void InitService()
        {
            _service = new WcfService(_bus.Object, _factory.Object, _gameReplay.Object, _runtimeProvider.Object, _reelsProvider.Object, _presentationProvider.Object, _process.Object);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(
            bool nullBus,
            bool nullFactory,
            bool nullDiagnostics,
            bool nullRuntime,
            bool nullReelsProvider,
            bool nullPresentationProvider,
            bool nullProcess)
        {
            var instance = CreateTarget(nullBus, nullFactory, nullDiagnostics, nullRuntime, nullReelsProvider, nullPresentationProvider, nullProcess);
            Assert.IsNull(instance);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            Assert.IsNotNull(_service);
        }

        [TestMethod]
        public void WhenJoinExpectSuccess()
        {
            _service.Join();

            _bus.Verify(b => b.Publish(It.IsAny<GameConnectedEvent>()));
        }

        [TestMethod]
        public void WhenLeaveExpectSuccess()
        {
            _service.Leave();

            _bus.Verify(b => b.Publish(It.IsAny<GameExitedNormalEvent>()));
        }

        [TestMethod]
        public void WhenBeginGameRoundExpectEvent()
        {
            var command = new Mock<ICommandHandler<BeginGameRound>>();

            _factory.Setup(m => m.Create<BeginGameRound>()).Returns(command.Object);

            var actual = _service.BeginGameRound(1);

            Assert.IsFalse(actual);

            command.Verify(m => m.Handle(It.IsAny<BeginGameRound>()));
        }

        [TestMethod]
        public void WhenEndGameRoundExpectEvent()
        {
            const long betAmount = 5;
            const long winAmount = 100;

            var command = new Mock<ICommandHandler<SetGameResult>>();
            _factory.Setup(m => m.Create<SetGameResult>()).Returns(command.Object);

            _service.EndGameRound(betAmount, winAmount);

            command.Verify(m => m.Handle(It.IsAny<SetGameResult>()));
        }

        [TestMethod]
        public void WhenGameStartedExpectEvent()
        {
            var command = new Mock<ICommandHandler<GameLoadedEvent>>();
            _factory.Setup(f => f.Create<GameLoadedEvent>()).Returns(command.Object);

            _service.OnRuntimeEvent(RuntimeEvent.NotifyGameReady);

            _bus.Verify(b => b.Publish(It.IsAny<GameLoadedEvent>()));
        }

        [TestMethod]
        public void WhenGameRequestExitExpectSuccess()
        {
            _service.OnRuntimeEvent(RuntimeEvent.RequestGameExit);
        }

        [TestMethod]
        public void WhenGetRandomNumberU64ExpectSuccess()
        {
            const ulong range = 1000;
            const ulong notSoRandom = 500;

            var command = new Mock<ICommandHandler<GetRandomNumber>>();
            command.Setup(c => c.Handle(It.IsAny<GetRandomNumber>()))
                .Callback<GetRandomNumber>(c => c.Value = notSoRandom);

            _factory.Setup(f => f.Create<GetRandomNumber>()).Returns(command.Object);

            InitService();

            var expected = _service.GetRandomNumberU64(range);

            Assert.AreEqual(expected, notSoRandom);
        }

        [TestMethod]
        public void WhenGetRandomNumberU32ExpectSuccess()
        {
            const uint range = 1000;
            const uint notSoRandom = 500;

            var command = new Mock<ICommandHandler<GetRandomNumber>>();
            command.Setup(c => c.Handle(It.IsAny<GetRandomNumber>()))
                .Callback<GetRandomNumber>(c => c.Value = notSoRandom);

            _factory.Setup(f => f.Create<GetRandomNumber>()).Returns(command.Object);

            InitService();

            var expected = _service.GetRandomNumberU32(range);

            Assert.AreEqual(expected, notSoRandom);
        }

        [TestMethod]
        public void WhenConfigureWithFailedLoadExpectNotSaved()
        {
            var command = new Mock<ICommandHandler<ConfigureClient>>();
            _factory.Setup(f => f.Create<ConfigureClient>()).Returns(command.Object);

            _service.OnRuntimeEvent(RuntimeEvent.RequestConfiguration);
        }

        [TestMethod]
        public void WhenReceiveRecoveryExpectSave()
        {
            var command = new Mock<ICommandHandler<AddRecoveryDataPoint>>();
            _factory.Setup(f => f.Create<AddRecoveryDataPoint>()).Returns(command.Object);

            var data = new byte[] { 0x0A };

            _service.OnRecoveryPoint(data);
        }

        private WcfService CreateTarget(
            bool nullEventBus = false,
            bool nullFactory = false,
            bool nullDiagnostics = false,
            bool nullRuntime = false,
            bool nullReelService = false,
            bool nullPresentationService = false,
            bool nullProcess = false)
        {

            return new WcfService(
                nullEventBus ? null : _bus.Object,
                nullFactory ? null : _factory.Object,
                nullDiagnostics ? null : _gameReplay.Object,
                nullRuntime ? null : _runtimeProvider.Object,
                nullReelService ? null : _reelsProvider.Object,
                nullPresentationService ? null : _presentationProvider.Object,
                nullProcess ? null : _process.Object);
        }
    }
}*/