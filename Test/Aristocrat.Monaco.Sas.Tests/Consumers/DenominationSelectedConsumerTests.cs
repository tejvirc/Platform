﻿namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Sas.Consumers;
    using Aristocrat.Monaco.Sas.Contracts.SASProperties;
    using Aristocrat.Monaco.Sas.Exceptions;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;

    [TestClass]
    public class DenominationSelectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<IGameProvider> _gameProviderMock;

        private DenominationSelectedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _gameProviderMock = new Mock<IGameProvider>(MockBehavior.Strict);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new DenominationSelectedConsumer(_exceptionHandlerMock.Object, _propertiesManagerMock.Object, _gameProviderMock.Object);
        }

        [TestMethod]
        [DataRow(1, false, DisplayName = "The GameId is the same, don't send the exception")]
        [DataRow(2, true, DisplayName = "The GameId changed, send the exception")]
        public void ConsumeDenominationSelectedTests(int gameId, bool exceptionReported)
        {
            const int currentGameId = 1;

            _propertiesManagerMock.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(It.IsAny<int>());
            _propertiesManagerMock.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(It.IsAny<long>());
            _propertiesManagerMock.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));

            var denominationMock = new Mock<IDenomination>(MockBehavior.Strict);
            denominationMock.Setup(x => x.Value).Returns(It.IsAny<int>());
            denominationMock.Setup(x => x.Id).Returns(currentGameId);

            var gameDetailMock = new Mock<IGameDetail>(MockBehavior.Strict);
            gameDetailMock.Setup(x => x.Denominations)
                .Returns(new List<IDenomination> { denominationMock.Object });

            _gameProviderMock.Setup(g => g.GetGame(It.IsAny<int>()))
                .Returns(gameDetailMock.Object);

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new DenominationSelectedEvent(gameId, 10L));

            Assert.AreEqual(exceptionReported, actual != null);

            if (exceptionReported)
            {
                _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()));

                CollectionAssert.AreEquivalent(new GameSelectedExceptionBuilder(gameId), actual);
                _propertiesManagerMock.Verify(m => m.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            }
        }
    }
}
