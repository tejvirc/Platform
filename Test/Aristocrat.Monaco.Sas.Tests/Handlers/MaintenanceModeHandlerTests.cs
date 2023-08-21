namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class MaintenanceModeHandlerTests
    {
        private Mock<ISasDisableProvider> _disableProvider;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private MaintenanceModeHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _disableProvider = new Mock<ISasDisableProvider>(MockBehavior.Default);
            _target = new MaintenanceModeHandler(_disableProvider.Object, _eventBus.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(2, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnterMaintenanceMode));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ExitMaintenanceMode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullISystemDisableManagerTest()
        {
            _target = new MaintenanceModeHandler(null, _eventBus.Object);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIEventBusTest()
        {
            _target = new MaintenanceModeHandler(_disableProvider.Object, null);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void EnterMaintenanceModeTest()
        {
            _disableProvider.Setup(
                    m => m.Disable(
                        It.Is<SystemDisablePriority>(priority => priority == SystemDisablePriority.Normal),
                        It.Is<DisableState>(reason => reason == DisableState.MaintenanceMode)))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<MaintenanceModeEnteredEvent>())).Verifiable();

            var longPollResponse = _target.Handle(new LongPollSingleValueData<bool>(true));

            Assert.IsNotNull(longPollResponse);
            _disableProvider.VerifyAll();
            _eventBus.Verify();
        }

        [TestMethod]
        public void ExitMaintenanceModeTest()
        {
            _disableProvider.Setup(m => m.Enable(It.Is<DisableState>(reason => reason == DisableState.MaintenanceMode)))
                .Returns(Task.CompletedTask)
                .Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<MaintenanceModeExitedEvent>())).Verifiable();

            var longPollResponse = _target.Handle(new LongPollSingleValueData<bool>(false));

            Assert.IsNotNull(longPollResponse);
            _disableProvider.VerifyAll();
            _eventBus.Verify();
        }
    }
}