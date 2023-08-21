namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts.Configuration;
    using Contracts;
    using Contracts.Models;
    using Contracts.Process;
    using Gaming.Runtime;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameServiceTests
    {
        private const int GameId = 1;
        private const long Denom = 5;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            var service = new GameService(null, null, null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameProcessIsNullExpectException()
        {
            var eventbus = new Mock<IEventBus>();
            var service = new GameService(eventbus.Object, null, null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenIpcIsNullExpectException()
        {
            var process = new Mock<IGameProcess>();
            var eventbus = new Mock<IEventBus>();
            var service = new GameService(eventbus.Object, process.Object, null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var eventbus = new Mock<IEventBus>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameConfigurationIsNullExpectException()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var eventbus = new Mock<IEventBus>();
            var audio = new Mock<IAudio>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameProviderIsNullExpectException()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var eventbus = new Mock<IEventBus>();
            var audio = new Mock<IAudio>();
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var eventbus = new Mock<IEventBus>();
            var audio = new Mock<IAudio>();
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var gameProvider = new Mock<IGameProvider>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, gameProvider.Object);

            Assert.IsNotNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidGameException))]
        public void WhenGameIdDoesNotExistExpectException()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();

            propertiesManager.Setup(p => p.GetProperty(GamingConstants.Games, null))
                .Returns(Enumerable.Empty<IGameDetail>());
            var eventbus = new Mock<IEventBus>();
            var audio = new Mock<IAudio>();
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var gameProvider = new Mock<IGameProvider>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, gameProvider.Object);

            var request = new GameInitRequest
            {
                GameId = GameId,
                Denomination = Denom,
                GameBottomHwnd = IntPtr.Zero,
                GameTopHwnd = IntPtr.Zero,
                GameVirtualButtonDeckHwnd = IntPtr.Zero,
                GameTopperHwnd = IntPtr.Zero
            };

            service.Initialize(request);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidGameException))]
        public void WhenDenomDoesNotExistExpectException()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var audio = new Mock<IAudio>();

            propertiesManager.Setup(p => p.GetProperty(GamingConstants.GameCombos, null))
                .Returns(new List<IGameCombo> { Factory_CreateMockGameCombo() });

            var eventbus = new Mock<IEventBus>();
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var gameProvider = new Mock<IGameProvider>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, gameProvider.Object);

            var request = new GameInitRequest
            {
                GameId = 1,
                Denomination = 1,
                GameBottomHwnd = IntPtr.Zero,
                GameTopHwnd = IntPtr.Zero,
                GameVirtualButtonDeckHwnd = IntPtr.Zero,
                GameTopperHwnd = IntPtr.Zero
            };

            service.Initialize(request);
        }

        [TestMethod]
        public void WhenRequestIsValidExpectSuccess()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var audio = new Mock<IAudio>();

            propertiesManager.Setup(p => p.GetProperty(GamingConstants.GameCombos, null))
                .Returns(new List<IGameCombo> { Factory_CreateMockGameCombo() });
            propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(0);
            var eventbus = new Mock<IEventBus>();
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var gameProvider = new Mock<IGameProvider>();
            var game = new Mock<IGameDetail>();
            game.SetupGet(g => g.ThemeId).Returns("Theme 1");

            gameProvider.Setup(g => g.GetGame(It.IsAny<int>())).Returns(game.Object);
            gameConfiguration.Setup(c => c.GetActive(It.IsAny<string>())).Returns(default(IConfigurationRestriction));
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, gameProvider.Object);

            var request = new GameInitRequest
            {
                GameId = GameId,
                Denomination = Denom,
                GameBottomHwnd = IntPtr.Zero,
                GameTopHwnd = IntPtr.Zero,
                GameVirtualButtonDeckHwnd = IntPtr.Zero,
                GameTopperHwnd = IntPtr.Zero
            };

            service.Initialize(request);

            propertiesManager.Verify(
                p =>
                    p.SetProperty(
                        It.Is<string>(prop => prop == GamingConstants.SelectedGameId),
                        It.Is<object>(v => (int)v == GameId)));
            propertiesManager.Verify(
                p =>
                    p.SetProperty(
                        It.Is<string>(prop => prop == GamingConstants.SelectedDenom),
                        It.Is<object>(v => (long)v == Denom)));

            process.Verify(p => p.EndGameProcess(false, true));
            ipc.Verify(i => i.StartComms());
            process.Verify(p => p.StartGameProcess(It.Is<GameInitRequest>(req => req == request)));
        }

        [TestMethod]
        public void TerminateTest()
        {
            var process = new Mock<IGameProcess>();
            var ipc = new Mock<IProcessCommunication>();
            var propertiesManager = new Mock<IPropertiesManager>();
            var eventbus = new Mock<IEventBus>();
            var audio = new Mock<IAudio>();

            process.Setup(m => m.IsRunning(1)).Returns(true);
            var gameConfiguration = new Mock<IGameConfigurationProvider>();
            var gameProvider = new Mock<IGameProvider>();
            var service = new GameService(eventbus.Object, process.Object, ipc.Object, propertiesManager.Object, audio.Object, gameConfiguration.Object, gameProvider.Object);
            service.Terminate(1);

            process.Verify(p => p.EndGameProcess(1, true, true));
        }

        private static IGameCombo Factory_CreateMockGameCombo()
        {
            var gameDetail = new Mock<IGameCombo>();
            gameDetail.SetupGet(g => g.GameId).Returns(GameId);
            gameDetail.SetupGet(g => g.ThemeId).Returns("Test");
            gameDetail.SetupGet(g => g.Denomination).Returns(Denom);

            return gameDetail.Object;
        }
    }
}
