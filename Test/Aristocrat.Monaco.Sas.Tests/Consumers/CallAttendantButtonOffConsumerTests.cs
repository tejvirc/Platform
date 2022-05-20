namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class CallAttendantButtonOffConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private CallAttendantButtonOffConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new CallAttendantButtonOffConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new CallAttendantButtonOffConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.ChangeLampOff)))
                .Verifiable();
            _target.Consume(new CallAttendantButtonOffEvent());
            _exceptionHandler.Verify();
        }
    }
}