namespace Aristocrat.Monaco.Sas.Tests.Consumers
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
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class GameSelectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<IGameProvider> _gameProviderMock;

        private GameSelectedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProviderMock = new Mock<IGameProvider>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameSelectedConsumer(_exceptionHandlerMock.Object, _propertiesManagerMock.Object, _gameProviderMock.Object);
        }

        [TestMethod]
        [DataRow(1, false, DisplayName = "The GameId is the same, don't send the exception")]
        [DataRow(2, true, DisplayName = "The GameId changed, send the exception")]
        public void ConsumeGameSelectedTests(int gameId, bool exceptionReported)
        {
            const int currentGameId = 1;
            const int denomId = 0;
            var expectedResult = new GameSelectedExceptionBuilder(denomId);

            _propertiesManagerMock.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(currentGameId);

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new GameSelectedEvent(gameId, 1000L, "", false, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));

            Assert.AreEqual(exceptionReported, actual != null);

            if (exceptionReported)
            {
                _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()));
                
                CollectionAssert.AreEquivalent(expectedResult, actual);
                _propertiesManagerMock.Verify(m => m.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            }
        }
    }
}
