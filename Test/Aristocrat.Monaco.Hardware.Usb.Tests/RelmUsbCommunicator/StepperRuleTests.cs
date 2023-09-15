namespace Aristocrat.Monaco.Hardware.Usb.Tests.RelmUsbCommunicator
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.RelmReels.Communicator;
    using Contracts;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using RelmReels.Messages;
    using RelmReels.Messages.Commands;
    using RelmReels.Messages.Queries;
    using Test.Common;
    using Usb.ReelController.Relm;

    [TestClass]
    public class StepperRuleTests
    {
        private readonly Mock<RelmReels.Communicator.IRelmCommunicator> _driver = new();
        private RelmUsbCommunicator _usbCommunicator;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Initialize()
        {
            _driver.Setup(x => x.IsOpen).Returns(true);
            _driver.Setup(x => x.Configuration).Returns(new DeviceConfiguration());
            _driver.Setup(x => x.SendQueryAsync<DeviceConfiguration>(default)).ReturnsAsync(new RelmResponse<DeviceConfiguration>(true, new DeviceConfiguration()));
            _driver.Setup(x => x.SendQueryAsync<FirmwareSize>(default)).ReturnsAsync(new RelmResponse<FirmwareSize>(true, new FirmwareSize()));
            _driver.Setup(x => x.SendQueryAsync<DeviceStatuses>(default)).ReturnsAsync(new RelmResponse<DeviceStatuses>(true, new DeviceStatuses()));

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HardwareConstants.DoNotResetRelmController, It.IsAny<bool>())).Returns(false);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);

            _usbCommunicator = new RelmUsbCommunicator(_driver.Object, _propertiesManager.Object, _eventBus.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task AnticipationRuleCallsAnticipationCommandReturnsControllerResult(bool expectedResult)
        {
            var rule = new StepperRuleData
            {
                Cycle = 1,
                Delta = 2,
                EventId = 3,
                ReelIndex = 4,
                ReferenceStep = 5,
                StepToFollow = 6,
                RuleType = StepperRuleType.AnticipationRule
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperAnticipationRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedResult));

            var actualResult = await _usbCommunicator.PrepareStepperRule(rule);

            _driver.Verify(x => x.SendCommandAsync(It.Is<PrepareStepperAnticipationRule>(r =>
                r.Cycle == rule.Cycle &&
                r.Delta == rule.Delta &&
                r.EventId == rule.EventId &&
                r.ReelIndex == rule.ReelIndex &&
                r.ReferenceStep == rule.ReferenceStep &&
                r.StepToFollow == rule.StepToFollow &&
                r.RuleType == UserSpecifiedRuleType.Anticipation), default), Times.Once);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task FollowRuleCallsFollowCommandReturnsControllerResult(bool expectedResult)
        {
            var rule = new StepperRuleData
            {
                Cycle = 1,
                EventId = 3,
                ReelIndex = 4,
                ReferenceStep = 5,
                StepToFollow = 6,
                RuleType = StepperRuleType.FollowRule
            };

            _driver.Setup(x => x.SendCommandAsync(It.IsAny<PrepareStepperFollowRule>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedResult));

            var actualResult = await _usbCommunicator.PrepareStepperRule(rule);

            _driver.Verify(x => x.SendCommandAsync(It.Is<PrepareStepperFollowRule>(r =>
                r.Cycle == rule.Cycle &&
                r.EventId == rule.EventId &&
                r.ReelIndex == rule.ReelIndex &&
                r.ReferenceStep == rule.ReferenceStep &&
                r.StepToFollow == rule.StepToFollow &&
                r.RuleType == UserSpecifiedRuleType.Follow), default), Times.Once);

            Assert.AreEqual(expectedResult, actualResult);
        }
    }
}
