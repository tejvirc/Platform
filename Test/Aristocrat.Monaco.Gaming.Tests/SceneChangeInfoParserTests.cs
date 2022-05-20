namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Contracts.Events;
    using Aristocrat.Monaco.Gaming.GameRound;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SceneChangeInfoParserTests
    {
        private SceneChangeInfoParser _target;
        private Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);

        [TestInitialize]
        public void Initialize()
        {
            _target = new SceneChangeInfoParser(_eventBus.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParametersTest()
        {
            _ = new SceneChangeInfoParser(null);
        }

        [TestMethod]
        public void UpdateGameRoundInfoToFewItemsTest()
        {
            _eventBus.Setup(x => x.Publish(It.IsAny<SceneChangedEvent>())).Verifiable();
            var sceneChangeInformation = new List<string> { _target.GameType, _target.Version };

            _target.UpdateGameRoundInfo(sceneChangeInformation);

            _eventBus.Verify(x => x.Publish(It.IsAny<SceneChangedEvent>()), Times.Never());
        }

        [TestMethod]
        public void UpdateGameRoundInfoTest()
        {
            var sceneName = "RSFS";
            _eventBus.Setup(x => x.Publish(It.IsAny<SceneChangedEvent>())).Verifiable();
            var sceneChangeInformation = new List<string> { _target.GameType, _target.Version, sceneName };

            _target.UpdateGameRoundInfo(sceneChangeInformation);

            _eventBus.Verify(x => x.Publish(It.IsAny<SceneChangedEvent>()), Times.Once());
        }
    }
}