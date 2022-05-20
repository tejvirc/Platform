namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class DoorClosedMeteredConsumerTests
    {
        private DoorClosedMeteredConsumer _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _target = new DoorClosedMeteredConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new DoorClosedMeteredConsumer(null);
        }

        [DataRow((int)DoorLogicalId.Belly, new[] { GeneralExceptionCode.BellyDoorWasClosed }, DisplayName = "Belly Door Metered Event")]
        [DataRow((int)DoorLogicalId.CashBox, new[] { GeneralExceptionCode.CashBoxDoorWasClosed }, DisplayName = "Cash box Door Metered Event")]
        [DataRow((int)DoorLogicalId.DropDoor, new[] { GeneralExceptionCode.DropDoorWasClosed }, DisplayName = "Drop Door Metered Event")]
        [DataRow((int)DoorLogicalId.Logic, new[] { GeneralExceptionCode.CardCageWasClosed }, DisplayName = "Logic Door Metered Event")]
        [DataRow((int)DoorLogicalId.Main, new[] { GeneralExceptionCode.SlotDoorWasClosed }, DisplayName = "Main Door Metered Event")]
        [DataRow((int)DoorLogicalId.TopBox, new[] { GeneralExceptionCode.SlotDoorWasClosed }, DisplayName = "Top Door Metered Event")]
        [DataRow(int.MaxValue, new GeneralExceptionCode[] { }, DisplayName = "Invalid Door test")]
        [DataTestMethod]
        public void ConsumeTest(int logicalId, GeneralExceptionCode[] expectedExceptions)
        {
            foreach (var exception in expectedExceptions)
            {
                _exceptionHandler.Setup(
                        x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == exception)))
                    .Verifiable();
            }

            _target.Consume(new DoorClosedMeteredEvent(logicalId, false, string.Empty));
            _exceptionHandler.VerifyAll();
        }
    }
}