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

    [TestClass]
    public class DenominationSelectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;
        private Mock<IPropertiesManager> _propertiesManagerMock;

        private DenominationSelectedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Strict);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new DenominationSelectedConsumer(_exceptionHandlerMock.Object, _propertiesManagerMock.Object);
        }

        [TestMethod]
        public void WhenNewId_SendException()
        {
            _propertiesManagerMock.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>())).Returns(1);

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new DenominationSelectedEvent(123, 10L));

            Assert.IsNotNull(actual);

            _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()));

            CollectionAssert.AreEquivalent(new GameSelectedExceptionBuilder(123), actual);
            _propertiesManagerMock.Verify(m => m.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
        }

        [TestMethod]
        public void WhenSameId_NoException()
        {
            _propertiesManagerMock.Setup(p => p.SetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()));
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>())).Returns(123);

            GameSelectedExceptionBuilder actual = null;
            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection a) => actual = a as GameSelectedExceptionBuilder)
                .Verifiable();

            _target.Consume(new DenominationSelectedEvent(123, 10L));

            Assert.IsNull(actual);
        }
    }
}
