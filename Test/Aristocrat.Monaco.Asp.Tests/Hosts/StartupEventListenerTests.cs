namespace Aristocrat.Monaco.Asp.Tests.Hosts
{
    using System;
    using Application.Contracts;
    using Asp.Hosts.CompositionRoot;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class StartupEventListenerTests
    {
        private Mock<IEventBus> _eventBus;
        private StartupEventListener _underTest;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _underTest = new StartupEventListener();
            _underTest.EventBus = _eventBus.Object;
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void GivenStartupEventListenerWhenSubscribeThenSubscribeAllEvents()
        {
            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<InitializationCompletedEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<SystemDisableAddedEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<SystemDisableRemovedEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<ClosedEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<OpenEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<PersistentStorageIntegrityCheckFailedEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<StorageErrorEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<DoorOpenMeteredEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<SystemDisabledByOperatorEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.Subscribe(
                _underTest,
                It.IsAny<Action<SystemEnabledByOperatorEvent>>()))
                .Verifiable();

            _underTest.Initialize();

            _eventBus.Verify();
        }
    }
}