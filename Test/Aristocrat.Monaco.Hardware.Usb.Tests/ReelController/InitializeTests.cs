namespace Aristocrat.Monaco.Hardware.Usb.Tests.ReelController
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Usb.ReelController.Relm;

    [TestClass]
    public class InitializeTests
    {
        [TestMethod]
        public void UninitializedControllerHasNoCapabilities()
        {
            const int expectedCapabilities = 0;

            var controller = new RelmReelController();
            var capabilities = controller.GetCapabilities();

            Assert.AreEqual(expectedCapabilities, capabilities.Count());
        }

        [TestMethod]
        public async Task InitializationSucceedsWhenCommunicatorIsOpenTest()
        {
            var protocol = "RELM";
            var productId = 0x1234;
            var vendorId = 0x4321;
            var expectedCapabilities = 3;
            var initializedEventRaised = false;
            var initializationFailedEventRaised = false;
            var communicator = new Mock<IRelmCommunicator>();
            var controller = new RelmReelController();

            communicator.Setup(x => x.Protocol).Returns(protocol);
            communicator.Setup(x => x.ProductId).Returns(productId);
            communicator.Setup(x => x.VendorId).Returns(vendorId);
            communicator.Setup(x => x.IsOpen).Returns(true);
            controller.Initialized += delegate { initializedEventRaised = true; };
            controller.InitializationFailed += delegate { initializationFailedEventRaised = true; };

            await controller.Initialize(communicator.Object);
            var capabilities = controller.GetCapabilities();
            
            Assert.IsTrue(initializedEventRaised);
            Assert.IsFalse(initializationFailedEventRaised);
            Assert.IsTrue(controller.IsInitialized);
            Assert.IsTrue(controller.IsConnected);
            Assert.AreEqual(protocol, controller.Protocol);
            Assert.AreEqual(productId, controller.ProductId);
            Assert.AreEqual(vendorId, controller.VendorId);
            Assert.AreEqual(expectedCapabilities, capabilities.Count());
            Assert.IsTrue(controller.HasCapability<IAnimationImplementation>());
            Assert.IsTrue(controller.HasCapability<IReelBrightnessImplementation>());
            Assert.IsTrue(controller.HasCapability<ISynchronizationImplementation>());
        }

        [TestMethod]
        public async Task InitializationFailsWhenCommunicatorIsNotOpenTest()
        {
            var initializedEventRaised = false;
            var initializationFailedEventRaised = false;
            var communicator = new Mock<IRelmCommunicator>();
            var controller = new RelmReelController();

            communicator.Setup(x => x.IsOpen).Returns(false);
            controller.Initialized += delegate { initializedEventRaised = true; };
            controller.InitializationFailed += delegate { initializationFailedEventRaised = true; };

            await controller.Initialize(communicator.Object);
            
            Assert.IsFalse(initializedEventRaised);
            Assert.IsTrue(initializationFailedEventRaised);
            Assert.IsFalse(controller.IsInitialized);
            Assert.IsFalse(controller.IsConnected);
            Assert.AreEqual(string.Empty, controller.Protocol);
            Assert.AreEqual(0, controller.ProductId);
            Assert.AreEqual(0, controller.VendorId);
        }

        [DataTestMethod]
        [DataRow(3)]
        [DataRow(5)]
        public async Task RequestStatusShouldProvideConnectedReels(int connectedReelCount)
        {
            var controller = new RelmReelController();
            var communicator = new Mock<IRelmCommunicator>();
            var connectedEventCount = 0;

            var reelStatuses = new List<ReelStatus>();
            for (var i = 1; i <= connectedReelCount; i++)
            {
                reelStatuses.Add(new ReelStatus { ReelId = i, Connected = true });
            }

            communicator.Setup(x => x.Initialize())
                .Returns(Task.FromResult(default(object)))
                .Raises(x => x.ReelStatusReceived += null, new ReelStatusReceivedEventArgs(reelStatuses));
            communicator.Setup(x => x.IsOpen).Returns(true);
            
            controller.ReelConnected += delegate { connectedEventCount++; };
            await controller.Initialize(communicator.Object);

            Assert.AreEqual(connectedReelCount, connectedEventCount);
        }
    }
}
