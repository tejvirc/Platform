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

        private static IDenomination SetupMockDenomination(int gameId, long denom)
        {
            var mockDenomination = new Mock<IDenomination>(MockBehavior.Strict);
            mockDenomination.SetupGet(g => g.Id).Returns(() => gameId);
            mockDenomination.SetupGet(g => g.Value).Returns(() => denom);
            mockDenomination.SetupGet(g => g.Active).Returns(() => true);
            return mockDenomination.Object;
        }

        private static IGameDetail SetupMockGameDetail(int gameId, long denom)
        {
            var mockGameProfile = new Mock<IGameDetail>(MockBehavior.Strict);
            mockGameProfile.SetupGet(g => g.Id).Returns(() => gameId);
            mockGameProfile.SetupGet(g => g.ThemeName).Returns(() => "theme");
            mockGameProfile.SetupGet(g => g.Version).Returns(() => "version");
            mockGameProfile.SetupGet(g => g.VariationId).Returns(() => "99");            

            mockGameProfile.SetupGet(g => g.Denominations).Returns(() => new List< IDenomination>{ SetupMockDenomination(gameId, denom) });
            return mockGameProfile.Object;
        }

        [TestMethod]
        [DataRow(1, 10, false, DisplayName = "The GameId is the same, don't send the exception")]
        [DataRow(2, 20, true, DisplayName = "The GameId changed, send the exception")]
        public void ConsumeGameSelectedTests(int gameId, int denom, bool exceptionReported)
        {
            const int lastGameId = 1;
            var expectedResult = new GameSelectedExceptionBuilder(gameId);            

            _propertiesManagerMock.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(lastGameId);

            _gameProviderMock.Setup(g => g.GetGame(gameId)).Returns(SetupMockGameDetail(gameId, denom));

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new GameSelectedEvent(gameId, denom, "", false, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));

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
