namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Hardware.Contracts.Persistence;
    using Mgam.Consumers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StorageErrorConsumerTest
    {
        private Mock<INotificationLift> _notificationLift;
        private StorageErrorConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new StorageErrorConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const StorageError errorType = StorageError.ReadFailure;
            var expectedNotifyCode = NotificationCode.RamCorruption;

            int actualNotifyCode = -1;
            string actualParam = string.Empty;

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualParam = param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            _target = new StorageErrorConsumer(_notificationLift.Object);
            _target.Consume(new StorageErrorEvent(errorType));
            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
            Assert.AreEqual(errorType.ToString(), actualParam);
        }
    }
}
