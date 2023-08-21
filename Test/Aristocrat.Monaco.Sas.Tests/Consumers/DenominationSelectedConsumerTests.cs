namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Sas.Consumers;
    using Sas.Contracts.SASProperties;
    using Sas.Exceptions;
    using Test.Common;
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
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProviderMock = new Mock<IGameProvider>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new DenominationSelectedConsumer(_exceptionHandlerMock.Object, _propertiesManagerMock.Object, _gameProviderMock.Object);
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

            mockGameProfile.SetupGet(g => g.Denominations).Returns(() => new List<IDenomination> { SetupMockDenomination(gameId, denom) });
            return mockGameProfile.Object;
        }

        [TestMethod]
        public void WhenNewId_SendException()
        {
            int lastGameId = 100;
            int gameId = 123;
            int denom = 1000;
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                 .Returns(lastGameId);
            _propertiesManagerMock.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, denom)).Returns(denom);

            _gameProviderMock.Setup(g => g.GetGame(gameId)).Returns(SetupMockGameDetail(gameId, denom));

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new DenominationSelectedEvent(gameId, denom));

            Assert.IsNotNull(actual);

            _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()));

            CollectionAssert.AreEquivalent(new GameSelectedExceptionBuilder(gameId), actual);
        }

        [TestMethod]
        public void WhenSameId_NoException()
        {
            int lastGameId = 123;
            int gameId = 123;
            int denom = 1000;

            _propertiesManagerMock.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, denom)).Returns(denom);
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                 .Returns(lastGameId);

            _gameProviderMock.Setup(g => g.GetGame(gameId)).Returns(SetupMockGameDetail(gameId, denom));

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new DenominationSelectedEvent(gameId, denom));

            Assert.IsNull(actual);
        }
    }
}
