namespace Aristocrat.Monaco.Asp.Tests.Progressive
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events.OperatorMenu;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class LinkProgressiveLevelConfigurationServiceTest
    {
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IEventBus> _eventBus;
        private Mock<ProgressiveLevel> _viewableProgressiveLevel;
        private Mock<IGameDetail> _gameDetail;
        private Mock<IViewableLinkedProgressiveLevel> _viewableLinkedProgressiveLevel;

        private Action<GameConfigurationSaveCompleteEvent> _GameConfigurationSaveCompleteEvent;

        private LinkProgressiveLevelConfigurationService _source;

        private int _gameId = 1;
        private int _progressiveId = 1;
        private int _levelId = 1;
        private string _allVariation = "ALL";
        private string _doubleVariation = "One,Two";
        private string _ninetyNineVariation = "99";
        private long _denomination = 1;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>();
            _gameProvider = new Mock<IGameProvider>();
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _viewableProgressiveLevel = new Mock<ProgressiveLevel>();
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>();
            _viewableLinkedProgressiveLevel = new Mock<IViewableLinkedProgressiveLevel>();
            _gameDetail = new Mock<IGameDetail>();

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameConfigurationSaveCompleteEvent>>()))
                .Callback<object, Action<GameConfigurationSaveCompleteEvent>>((subscriber, callback) => _GameConfigurationSaveCompleteEvent = callback);

            SetupGameDetails();
            SetupViewableProgressiveLevel(_allVariation);
            SetupProtocolLinkedProgressiveAdapter();

            _gameProvider.Setup(m => m.GetGame(_gameId)).Returns(_gameDetail.Object);
            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(new List<IGameDetail>(new IGameDetail[] { _gameDetail.Object }));
        }

        private void SetupProtocolLinkedProgressiveAdapter()
        {
            _protocolLinkedProgressiveAdapter.Setup(m => m.ViewProgressiveLevels()).Returns(new List<IViewableProgressiveLevel>(new IViewableProgressiveLevel[] { _viewableProgressiveLevel.Object, _viewableProgressiveLevel.Object }));
            _protocolLinkedProgressiveAdapter.Setup(m => m.ViewLinkedProgressiveLevels()).Returns(new List<IViewableLinkedProgressiveLevel>(new IViewableLinkedProgressiveLevel[] { _viewableLinkedProgressiveLevel.Object }));
            _protocolLinkedProgressiveAdapter.Setup(m => m.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM));
            _protocolLinkedProgressiveAdapter.Setup(o => o.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.Is<string>(i => i == ProtocolNames.DACOM)));
            _protocolLinkedProgressiveAdapter.Setup(o => o.AssignLevelsToGame(It.IsAny<List<ProgressiveLevelAssignment>>(), ProtocolNames.DACOM));

        }
        private void SetupGameDetails()
        {
            _gameDetail.SetupGet(m => m.Enabled).Returns(true);
            _gameDetail.SetupGet(m => m.Active).Returns(true);
            _gameDetail.SetupGet(m => m.VariationId).Returns(_ninetyNineVariation);
            _gameDetail.SetupGet(m => m.Id).Returns(_gameId);
            _gameDetail.SetupGet(m => m.ActiveDenominations).Returns(new List<long>(new long[] { _denomination }));
        }

        private void SetupViewableProgressiveLevel(string variation)
        {
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.LevelType).Returns(ProgressiveLevelType.LP);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.GameId).Returns(_gameId);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.ProgressivePackName).Returns("BF_LP");
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.ProgressiveId).Returns(_progressiveId);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.LevelId).Returns(_levelId);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.Variation).Returns(variation);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.Denomination).Returns(new List<long>(new long[] { _denomination }));
            string levelName = $"{ProtocolNames.DACOM}, Level Id: {_levelId}, Progressive Group Id: {_progressiveId}";
            _viewableLinkedProgressiveLevel.SetupGet(m => m.LevelName).Returns(levelName);
            _viewableProgressiveLevel.As<IViewableProgressiveLevel>().SetupGet(m => m.ResetValue).Returns(5000);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            _source = new LinkProgressiveLevelConfigurationService(_eventBus.Object, _gameProvider.Object, _protocolLinkedProgressiveAdapter.Object);
            Assert.ThrowsException<ArgumentNullException>(() => new LinkProgressiveLevelConfigurationService(null, _gameProvider.Object, _protocolLinkedProgressiveAdapter.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinkProgressiveLevelConfigurationService(_eventBus.Object, null, _protocolLinkedProgressiveAdapter.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinkProgressiveLevelConfigurationService(_eventBus.Object, _gameProvider.Object, null));
        }

        [TestMethod]
        public void ConfigurationInitializeTest()
        {
            _source = new LinkProgressiveLevelConfigurationService(_eventBus.Object, _gameProvider.Object, _protocolLinkedProgressiveAdapter.Object);
            Assert.IsNotNull(_source);
            _gameProvider.Verify(o => o.GetEnabledGames(), Times.Once());
            _protocolLinkedProgressiveAdapter.Verify(o => o.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), It.Is<string>(i => i == ProtocolNames.DACOM)), Times.Once());
            _protocolLinkedProgressiveAdapter.Verify(o => o.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), ProtocolNames.DACOM), Times.Once());
            _protocolLinkedProgressiveAdapter.Verify(o => o.AssignLevelsToGame(It.IsAny<List<ProgressiveLevelAssignment>>(), ProtocolNames.DACOM), Times.Once());

            _eventBus.Verify(v => v.Publish(It.IsAny<LinkProgressiveLevelConfigurationAppliedEvent>()), Times.Once);
        }

        [TestMethod]
        public void ConfigureGameCompleteEventTest()
        {
            SetupViewableProgressiveLevel(_doubleVariation);
            _source = new LinkProgressiveLevelConfigurationService(_eventBus.Object, _gameProvider.Object, _protocolLinkedProgressiveAdapter.Object);
            GameConfigurationSaveCompleteEvent @event = new GameConfigurationSaveCompleteEvent();
            _GameConfigurationSaveCompleteEvent(@event);

            _gameProvider.Verify(o => o.GetEnabledGames(), Times.Exactly(2));
            _eventBus.Verify(v => v.Publish(It.IsAny<LinkProgressiveLevelConfigurationAppliedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ThrowExceptionInIntializationTest()
        {
            _protocolLinkedProgressiveAdapter.Setup(m => m.ViewLinkedProgressiveLevels()).Callback(() => throw new Exception());
            _source = new LinkProgressiveLevelConfigurationService(_eventBus.Object, _gameProvider.Object, _protocolLinkedProgressiveAdapter.Object);
            Assert.IsNotNull(_source);
            _gameProvider.Verify(o => o.GetEnabledGames(), Times.Never);
            _eventBus.Verify(v => v.Publish(It.IsAny<LinkProgressiveLevelConfigurationAppliedEvent>()), Times.Never);
        }
    }
}
