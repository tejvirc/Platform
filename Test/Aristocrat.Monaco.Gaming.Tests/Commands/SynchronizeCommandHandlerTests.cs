namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Gaming.Commands;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SynchronizeCommandHandlerTests
    {
        private readonly ReelSynchronizationData _validReelSyncData = new()
        {
            SyncType = SynchronizeType.Regular,
            Duration = 400,
            ReelSyncStepData = new List<ReelSyncStepData>
            {
                new(0, 0),
                new(1, 0)
            }
        };
        private readonly ReelSynchronizationData _invalidReelSyncData = new()
        {
            SyncType = SynchronizeType.Enhanced,
            Duration = 0,
            ReelSyncStepData = new List<ReelSyncStepData>
            {
                new(0, 0)
            }
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
            reelController.Setup(x => x.HasCapability<IReelAnimationCapabilities>()).Returns(false);

            var command = new PrepareSynchronizeReels(_validReelSyncData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);

            Assert.AreEqual(command.Success, false);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HandleValidSynchronizeData(bool capabilityResponse)
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelSynchronizationCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelSynchronizationCapabilities>().Synchronize(It.IsAny<ReelSynchronizationData>(), default))
                .Returns(Task.FromResult(capabilityResponse));
            
            var command = new PrepareSynchronizeReels(_validReelSyncData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelSynchronizationCapabilities>().Synchronize(_validReelSyncData, default), Times.Once);
            Assert.AreEqual(command.Success, capabilityResponse);
        }

        [DataTestMethod]
        public void HandleInvalidSynchronizeData()
        {
            var reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            reelController.Setup(x => x.HasCapability<IReelSynchronizationCapabilities>()).Returns(true);
            reelController.Setup(r => r.GetCapability<IReelSynchronizationCapabilities>().Synchronize(_invalidReelSyncData, default))
                .Returns(Task.FromResult(false));
            
            var command = new PrepareSynchronizeReels(_invalidReelSyncData);
            var handler = Factory_CreateHandler();
            handler.Handle(command);
            
            reelController.Verify(r => r.GetCapability<IReelSynchronizationCapabilities>().Synchronize(_invalidReelSyncData, default), Times.Once);
            Assert.AreEqual(command.Success, false);
        }

        private PrepareSynchronizeCommandHandler Factory_CreateHandler()
        {
            return new PrepareSynchronizeCommandHandler();
        }
    }
}
