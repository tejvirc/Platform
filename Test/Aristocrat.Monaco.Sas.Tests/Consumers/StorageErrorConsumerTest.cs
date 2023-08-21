namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class StorageErrorConsumerTest
    {
        private StorageErrorConsumer _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _exceptionHandler.Setup(
                    m => m.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.EePromDataError)))
                .Verifiable();

            _target = new StorageErrorConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConsumeWithNVRamReadFailureTest()
        {
            _target.Consume(new StorageErrorEvent(StorageError.ReadFailure));
            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void ConsumeWithNVRamWriteFailureTest()
        {
            _target.Consume(new StorageErrorEvent(StorageError.WriteFailure));
            _exceptionHandler.Verify();
        }
    }
}
