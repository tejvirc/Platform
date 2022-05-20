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
    public class CallAttendantButtonOffConsumerTest
    {
        private Mock<INotificationLift> _notificationLift;
        private CallAttendantButtonOffConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
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
            var expectedNotifyCode = NotificationCode.CallAttendantOff;
            var actualNotifyCode = -1;

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            _target = new CallAttendantButtonOffConsumer(_notificationLift.Object);
            _target.Consume(new CallAttendantButtonOffEvent());

            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
        }
    }
}
