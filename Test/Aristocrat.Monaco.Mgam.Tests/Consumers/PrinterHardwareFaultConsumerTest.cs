namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Hardware.Contracts.Printer;
    using Mgam.Consumers;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrinterHardwareFaultConsumerTest
    {
        private Mock<ILockup> _lockup;
        private Mock<INotificationLift> _notificationLift;
        private PrinterHardwareFaultConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _lockup = new Mock<ILockup>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LockupNullExceptionHandlerTest()
        {
            _target = new PrinterHardwareFaultConsumer(null, _notificationLift.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotificationLiftNullExceptionHandlerTest()
        {
            _target = new PrinterHardwareFaultConsumer(_lockup.Object, null);
        }

        [DataRow(true, PrinterFaultTypes.PaperJam, NotificationCode.LockedPrinterJammed, "")]
        [DataRow(false, PrinterFaultTypes.None, -1, "")]
        [DataRow(false, PrinterFaultTypes.PaperJam, NotificationCode.LockedPrinterJammed, "")]
        [DataRow(false, PrinterFaultTypes.PaperEmpty, NotificationCode.LockedPrinterOutOfPaper, "")]
        [DataRow(false, PrinterFaultTypes.ChassisOpen, NotificationCode.LockedTilt, "ChassisOpen")]
        [DataTestMethod]
        public void ConsumeTest(bool employeeLoggedIn,
            PrinterFaultTypes faultType,
            NotificationCode expectedNotificationCode,
            string expectedParams)
        {
            bool doLockupAndNotification = faultType != PrinterFaultTypes.None;
            int actualNotifyCode = doLockupAndNotification ? -1 : (int)expectedNotificationCode;
            string actualNotifyParam = doLockupAndNotification ? string.Empty : expectedParams;

            _lockup.Setup(x => x.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = string.IsNullOrEmpty(param) ? string.Empty : param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            _target = new PrinterHardwareFaultConsumer(_lockup.Object, _notificationLift.Object);
            _target.Consume(new HardwareFaultEvent(faultType));

            _lockup.Verify(
                x => x.LockupForEmployeeCard(null, SystemDisablePriority.Immediate),
                doLockupAndNotification ? Times.Once() : Times.Never());

            _notificationLift.Verify(
                x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()),
                doLockupAndNotification ? Times.Once() : Times.Never());

            Assert.AreEqual((int)expectedNotificationCode, actualNotifyCode);
            Assert.AreEqual(expectedParams, actualNotifyParam);
        }
    }
}
