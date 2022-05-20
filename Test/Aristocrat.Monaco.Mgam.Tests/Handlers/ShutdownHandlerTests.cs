namespace Aristocrat.Monaco.Mgam.Tests.Handlers
{
    using System;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Mgam.Handlers;
    using Mgam.Services.Lockup;
    using Mgam.Services.PlayerTracking;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ShutdownHandlerTests
    {
        private Mock<ILogger<ShutdownHandler>> _logger;
        private Mock<IEventBus> _eventBus;
        private Mock<IPlayerTracking> _playerTracking;
        private Mock<ILockup> _lockup;
        private ShutdownHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _logger = new Mock<ILogger<ShutdownHandler>>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _playerTracking = new Mock<IPlayerTracking>(MockBehavior.Default);
            _lockup = new Mock<ILockup>(MockBehavior.Default);
        }

        [DataRow(false, true, true, true, DisplayName = "Null Logger Object")]
        [DataRow(true, false, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, true, false, true, DisplayName = "Null Player Tracking Object")]
        [DataRow(true, true, true, false, DisplayName = "Null Lockup Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool logger,
            bool eventBus,
            bool playerTracking,
            bool lockup)
        {
            _target = new ShutdownHandler(
                logger ? _logger.Object : null,
                eventBus ? _eventBus.Object : null,
                playerTracking ? _playerTracking.Object : null,
                lockup ? _lockup.Object : null);
        }

        [TestMethod]
        public void SuccessfulConstructorTest()
        {
            CreateNewTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TestHandlerPublishesExitEventOnLoggedOffAndLocked()
        {
            CreateNewTarget();
            _playerTracking.SetupGet(p => p.IsSessionActive).Returns(false);
            _lockup.SetupGet(l => l.IsLockedByHost).Returns(true);
            _eventBus.Setup(e => e.Publish(It.IsAny<ExitRequestedEvent>())).Verifiable();

            var result = _target.Handle(new Shutdown());
            _eventBus.Verify();
            Assert.AreEqual(result.Result.ResponseCode, ServerResponseCode.Ok);
        }

        [TestMethod]
        public void TestHandlerLocksUpOnLoggedOnAndLocked()
        {
            CreateNewTarget();
            _playerTracking.SetupGet(p => p.IsSessionActive).Returns(true);
            _lockup.SetupGet(l => l.IsLockedByHost).Returns(true);
            _lockup.Setup(l => l.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();

            var result = _target.Handle(new Shutdown());
            _lockup.Verify();
            Assert.AreEqual(result.Result.ResponseCode, ServerResponseCode.Ok);
        }

        [TestMethod]
        public void TestHandlerLocksUpOnLoggedOffAndUnlocked()
        {
            CreateNewTarget();
            _playerTracking.SetupGet(p => p.IsSessionActive).Returns(false);
            _lockup.SetupGet(l => l.IsLockedByHost).Returns(false);
            _lockup.Setup(l => l.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();

            var result = _target.Handle(new Shutdown());
            _lockup.Verify();
            Assert.AreEqual(result.Result.ResponseCode, ServerResponseCode.Ok);
        }

        [TestMethod]
        public void TestHandlerLocksUpOnLoggedOnAndUnlocked()
        {
            CreateNewTarget();
            _playerTracking.SetupGet(p => p.IsSessionActive).Returns(true);
            _lockup.SetupGet(l => l.IsLockedByHost).Returns(false);
            _lockup.Setup(l => l.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();

            var result = _target.Handle(new Shutdown());
            _lockup.Verify();
            Assert.AreEqual(result.Result.ResponseCode, ServerResponseCode.Ok);
        }

        private void CreateNewTarget()
        {
            _target = new ShutdownHandler(_logger.Object, _eventBus.Object, _playerTracking.Object, _lockup.Object);
        }
    }
}
