namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Gaming.Contracts;
    using Mgam.Consumers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CallAttendantButtonOnConsumerTest
    {
        private Mock<INotificationLift> _notificationLift;
        private CallAttendantButtonOnConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new CallAttendantButtonOnConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            var expectedNotifyCode = NotificationCode.CallAttendantOn;
            var actualNotifyCode = -1;

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            _target = new CallAttendantButtonOnConsumer(_notificationLift.Object);
            _target.Consume(new CallAttendantButtonOnEvent());

            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
        }
    }
}