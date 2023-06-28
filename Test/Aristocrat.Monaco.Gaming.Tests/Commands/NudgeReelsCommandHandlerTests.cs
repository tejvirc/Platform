namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     NudgeReelsCommandHandler unit tests
    /// </summary>
    [TestClass]
    public class NudgeReelsCommandHandlerTests
    {
        private readonly NudgeReelData[] _nudgeData =
        {
            new(1, SpinDirection.Forward, 50, 1, 10),
            new(2, SpinDirection.Backwards, 100, 2, 20),
            new(3, SpinDirection.Forward, 200, 3, 30)
        };

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void NullControllerIsHandledTest()
        {
            Factory_CreateHandler();
        }

        [TestMethod]
        public void HandleTestForNoCapabilitiesShouldFail()
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelSpinCapabilities>()).Returns(false);
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(false);

            var command = new NudgeReels(_nudgeData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);

            Assert.AreEqual(command.Success, false);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HandleTestForSpinCapabilities(bool capabilityResponse)
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelSpinCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelSpinCapabilities>().NudgeReels(It.IsAny<NudgeReelData[]>())).Returns(Task.FromResult(capabilityResponse));

            var command = new NudgeReels(_nudgeData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);

            reelController.Verify(r => r.GetCapability<IReelSpinCapabilities>().NudgeReels(command.NudgeSpinData), Times.Once);
            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PrepareNudgeReels(command.NudgeSpinData, default), Times.Never);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HandleTestForAnimationCapabilities(bool capabilityResponse)
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelAnimationCapabilities>().PrepareNudgeReels(It.IsAny<NudgeReelData[]>(), default)).Returns(Task.FromResult(capabilityResponse));

            var command = new NudgeReels(_nudgeData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelSpinCapabilities>().NudgeReels(command.NudgeSpinData), Times.Never);
            reelController.Verify(r => r.GetCapability<IReelAnimationCapabilities>().PrepareNudgeReels(command.NudgeSpinData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        private NudgeReelsCommandHandler Factory_CreateHandler()
        {
            return new NudgeReelsCommandHandler();
        }
    }
}