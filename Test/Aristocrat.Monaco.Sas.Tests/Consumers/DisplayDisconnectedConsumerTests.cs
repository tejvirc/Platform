namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Display;
    using Kernel;
    using Sas.Consumers;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DisplayDisconnectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManager;
        private DisplayDisconnectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new DisplayDisconnectedConsumer(_exceptionHandler.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new DisplayDisconnectedConsumer(null, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.IsTopperOverlayRedirecting, false)).Returns(false);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.GeneralTilt)))
                .Verifiable();
            _target.Consume(new DisplayDisconnectedEvent());
            _exceptionHandler.Verify();
        }
    }
}