namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Mgam.Services.DropMode;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Commands;
    using Hardware.Contracts.NoteAcceptor;
    using Mgam.Consumers;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NoteAcceptorHardwareFaultConsumerTest
    {
        private Mock<ILockup> _lockup;
        private Mock<ILogger<NoteAcceptorHardwareFaultClearConsumer>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<INotificationLift> _notificationLift;
        private Mock<IDropMode> _dropMode;
        private NoteAcceptorHardwareFaultConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _lockup = new Mock<ILockup>(MockBehavior.Default);
            _logger = new Mock<ILogger<NoteAcceptorHardwareFaultClearConsumer>>(MockBehavior.Default);
            _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
            _dropMode = new Mock<IDropMode>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LockupNullExceptionHandlerTest()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                null,
                _logger.Object,
                _commandFactory.Object,
                _notificationLift.Object,
                _dropMode.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoggerNullExceptionHandlerTest()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                _lockup.Object,
                null,
                _commandFactory.Object,
                _notificationLift.Object,
                _dropMode.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommandFactoryNullExceptionHandlerTest()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                _lockup.Object,
                _logger.Object,
                null,
                _notificationLift.Object,
                _dropMode.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotificationLiftNullExceptionHandlerTest()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                _lockup.Object,
                _logger.Object,
                _commandFactory.Object,
                null,
                _dropMode.Object);
        }

        [DataRow(false, NoteAcceptorFaultTypes.NoteJammed, NotificationCode.LockedBillAcceptorJam, "")]
        [DataRow(false, NoteAcceptorFaultTypes.None, -1, "")]
        [DataRow(false, NoteAcceptorFaultTypes.NoteJammed, NotificationCode.LockedBillAcceptorJam, "")]
        [DataRow(false, NoteAcceptorFaultTypes.StackerJammed, NotificationCode.LockedBillAcceptorJam, "")]
        [DataRow(false, NoteAcceptorFaultTypes.StackerFull, NotificationCode.LockedBillAcceptorFull, "")]
        [DataRow(false, NoteAcceptorFaultTypes.OtherFault, NotificationCode.LockedTilt, "OtherFault")]
        [DataRow(false, NoteAcceptorFaultTypes.StackerDisconnected, NotificationCode.LockedTilt, "StackerDisconnected")]
        [DataRow(true, NoteAcceptorFaultTypes.StackerDisconnected, NotificationCode.LockedTilt, "StackerDisconnected")]
        [DataTestMethod]
        public void ConsumeTest(bool dropMode,
            NoteAcceptorFaultTypes faultType,
            NotificationCode expectedNotificationCode,
            string expectedParams)
        {
            bool doLockupAndNotification = faultType != NoteAcceptorFaultTypes.None;
            bool doLockupForEmployeeCard = doLockupAndNotification && !dropMode;
            int actualNotifyCode = doLockupAndNotification ? -1 : (int)expectedNotificationCode;
            string actualNotifyParam = doLockupAndNotification ? string.Empty : expectedParams;

            _lockup.Setup(x => x.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();

            _dropMode.SetupGet(d => d.Active).Returns(dropMode);

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = string.IsNullOrEmpty(param) ? string.Empty : param;
                    })
                .Returns(Task.FromResult(0))
                .Verifiable();

            _commandFactory
                .Setup(x => x.Execute(It.IsAny<BillAcceptorMeterReport>()))
                .Returns(Task.FromResult(0))
                .Verifiable(); ;

            _target = new NoteAcceptorHardwareFaultConsumer(
                _lockup.Object,
                _logger.Object,
                _commandFactory.Object,
                _notificationLift.Object,
                _dropMode.Object);
            _target.Consume(new HardwareFaultEvent(faultType));

            _lockup.Verify(
                x => x.LockupForEmployeeCard(null, SystemDisablePriority.Normal),
                doLockupForEmployeeCard ? Times.Once() : Times.Never());

            _notificationLift.Verify(
                x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()),
                doLockupAndNotification ? Times.Once() : Times.Never());

            if (faultType == NoteAcceptorFaultTypes.StackerDisconnected)
            {
                if (dropMode)
                {
                    _commandFactory.Verify(x => x.Execute(It.IsAny<BillAcceptorMeterReport>()), Times.Once);
                }
                else
                {
                    _commandFactory.Verify(x => x.Execute(It.IsAny<BillAcceptorMeterReport>()), Times.Never);
                }
            }

            Assert.AreEqual((int)expectedNotificationCode, actualNotifyCode);
            Assert.AreEqual(expectedParams, actualNotifyParam);
        }
    }
}
