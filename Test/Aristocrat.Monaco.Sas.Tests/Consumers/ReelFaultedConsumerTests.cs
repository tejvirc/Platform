namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Hardware.Contracts.Reel;
    using Aristocrat.Sas.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class ReelFaultedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private ReelFaultedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new ReelFaultedConsumer(_exceptionHandler.Object);
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
            _target = new ReelFaultedConsumer(null);
        }

        [TestMethod]
        public void ConsumeTestNoError()
        {
            _target.Consume(new HardwareReelFaultEvent(ReelFaults.None, 1));
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.ReelTilt)), Times.Never);
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.Reel1Tilt)), Times.Never);
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.Reel2Tilt)), Times.Never);
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.Reel3Tilt)), Times.Never);
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.Reel4Tilt)), Times.Never);
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.Reel5Tilt)), Times.Never);
        }

        [DataRow(0, ReelFaults.ReelStall, GeneralExceptionCode.ReelTilt, DisplayName = "Reel 0, ReelStall, generic reel tilt")]
        [DataRow(1, ReelFaults.Disconnected, GeneralExceptionCode.Reel1Tilt, DisplayName = "Reel 1, Disconnected, reel 1 tilt")]
        [DataTestMethod]
        public void ConsumeTest(int reelId, ReelFaults faults, GeneralExceptionCode exceptionCode)
        {
            _target.Consume(new HardwareReelFaultEvent(faults, reelId));
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == exceptionCode)), Times.AtLeastOnce);
        }
    }
}
