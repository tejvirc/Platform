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
    public class DoorOpenMeteredConsumerTests
    {
        private DoorOpenMeteredConsumer _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _target = new DoorOpenMeteredConsumer(_exceptionHandler.Object);
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
            _target = new DoorOpenMeteredConsumer(null);
        }

        [DataRow((int)DoorLogicalId.Belly, false, false, new[] { GeneralExceptionCode.BellyDoorWasOpened }, DisplayName = "Belly Door Metered Event")]
        [DataRow((int)DoorLogicalId.Belly, true, false, new[] { GeneralExceptionCode.BellyDoorWasOpened }, DisplayName = "Belly Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.Belly, true, true, new GeneralExceptionCode[] { }, DisplayName = "Belly Door Metered Recovery Event")]
        [DataRow((int)DoorLogicalId.CashBox, false, false, new[] { GeneralExceptionCode.CashBoxDoorWasOpened }, DisplayName = "Cash box Door Metered Event")]
        [DataRow((int)DoorLogicalId.CashBox, true, false, new[] { GeneralExceptionCode.CashBoxDoorWasOpened, GeneralExceptionCode.PowerOffCashBoxDoorAccess }, DisplayName = "Cash box Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.CashBox, true, true, new [] { GeneralExceptionCode.PowerOffCashBoxDoorAccess }, DisplayName = "Cash box Door Metered Recovery Event")]
        [DataRow((int)DoorLogicalId.DropDoor, false, false, new[] { GeneralExceptionCode.DropDoorWasOpened }, DisplayName = "Drop Door Metered Event")]
        [DataRow((int)DoorLogicalId.DropDoor, true, false, new[] { GeneralExceptionCode.DropDoorWasOpened, GeneralExceptionCode.PowerOffDropDoorAccess }, DisplayName = "Drop Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.DropDoor, true, true, new [] { GeneralExceptionCode.PowerOffDropDoorAccess }, DisplayName = "Drop Door Metered Recovery Event")]
        [DataRow((int)DoorLogicalId.Logic, false, false, new[] { GeneralExceptionCode.CardCageWasOpened }, DisplayName = "Logic Door Metered Event")]
        [DataRow((int)DoorLogicalId.Logic, true, false, new[] { GeneralExceptionCode.CardCageWasOpened, GeneralExceptionCode.PowerOffCardCageAccess }, DisplayName = "Logic Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.Logic, true, true, new [] { GeneralExceptionCode.PowerOffCardCageAccess }, DisplayName = "Logic Door Metered Recovery Event")]
        [DataRow((int)DoorLogicalId.Main, false, false, new[] { GeneralExceptionCode.SlotDoorWasOpened }, DisplayName = "Main Door Metered Event")]
        [DataRow((int)DoorLogicalId.Main, true, false, new[] { GeneralExceptionCode.SlotDoorWasOpened, GeneralExceptionCode.PowerOffSlotDoorAccess }, DisplayName = "Main Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.Main, true, true, new [] { GeneralExceptionCode.PowerOffSlotDoorAccess }, DisplayName = "Main Door Metered Recovery Event")]
        [DataRow((int)DoorLogicalId.TopBox, false, false, new[] { GeneralExceptionCode.SlotDoorWasOpened }, DisplayName = "Top Door Metered Event")]
        [DataRow((int)DoorLogicalId.TopBox, true, false, new[] { GeneralExceptionCode.SlotDoorWasOpened, GeneralExceptionCode.PowerOffSlotDoorAccess }, DisplayName = "Top Door Metered While Power Off Event")]
        [DataRow((int)DoorLogicalId.TopBox, true, true, new [] { GeneralExceptionCode.PowerOffSlotDoorAccess }, DisplayName = "Top Door Metered Recovery Event")]
        [DataRow(int.MaxValue, true, false, new GeneralExceptionCode[] { }, DisplayName = "Invalid Door test")]
        [DataTestMethod]
        public void ConsumeTest(int logicalId, bool whilePowerOff, bool recovery, GeneralExceptionCode[] expectedExceptions)
        {
            foreach (var exception in expectedExceptions)
            {
                _exceptionHandler.Setup(
                        x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == exception)))
                    .Verifiable();
            }

            _target.Consume(new DoorOpenMeteredEvent(logicalId, whilePowerOff, recovery, string.Empty));
            _exceptionHandler.VerifyAll();
        }
    }
}