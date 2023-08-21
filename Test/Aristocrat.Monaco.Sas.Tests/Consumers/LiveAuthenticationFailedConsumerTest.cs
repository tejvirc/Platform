namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class LiveAuthenticationFailedConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _exceptionHandler = new Mock<ISasExceptionHandler>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSasClientIsNullExpectException()
        {
            var consumer = new LiveAuthenticationFailedConsumer(null);
            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectErrorReportedToSas()
        {
            var consumer = new LiveAuthenticationFailedConsumer(_exceptionHandler.Object);
            consumer.Consume(new LiveAuthenticationFailedEvent(string.Empty));

            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.EePromErrorDifferentChecksum)));
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.EePromErrorBadChecksum)));
        }
    }
}
