namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Localization.Properties;
    using Aristocrat.Monaco.Mgam.Consumers;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Aristocrat.Monaco.Test.Common;
    using Hardware.Contracts.Battery;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;

    [TestClass]
    public class BatteryLowConsumerTest
    {
        private Mock<ILockup> _lockup;
        private Mock<INotificationLift> _notificationLift;
        private BatteryLowConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _lockup = new Mock<ILockup>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LockupNullExceptionHandlerTest()
        {
            _target = new BatteryLowConsumer(null, _notificationLift.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotificationLiftNullExceptionHandlerTest()
        {
            _target = new BatteryLowConsumer(_lockup.Object, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const int batteryId = 1;
            var expectedNotifyCode = NotificationCode.LowRamBattery;

            int actualNotifyCode = -1;
            string actualNotifyParam = string.Empty;

            var text = Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.BatteryLowTilt);
            _lockup.Setup(x => x.LockupForEmployeeCard(text, SystemDisablePriority.Immediate)).Verifiable();

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            _target = new BatteryLowConsumer(_lockup.Object, _notificationLift.Object);
            _target.Consume(new BatteryLowEvent(batteryId));

            _lockup.Verify();
            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
            Assert.AreEqual(batteryId.ToString(), actualNotifyParam);
        }
    }
}
