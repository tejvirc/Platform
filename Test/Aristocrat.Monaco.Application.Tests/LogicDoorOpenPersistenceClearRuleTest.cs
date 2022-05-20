namespace Aristocrat.Monaco.Application.Tests
{
    using System;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Test class for the LogicDoorOpenPersistenceClearRule class.
    /// </summary>
    [TestClass]
    public class LogicDoorOpenPersistenceClearRuleTest
    {
        private Action<ClosedEvent> _doorClosedCallback;
        private Action<OpenEvent> _doorOpenCallback;
        private Mock<IDoorService> _doorService;
        private Mock<IEventBus> _eventBus;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<LogicDoorOpenPersistenceClearRule>(),
                        It.IsAny<Action<OpenEvent>>()))
                .Callback<object, Action<OpenEvent>>((subscriber, callback) => _doorOpenCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<LogicDoorOpenPersistenceClearRule>(),
                        It.IsAny<Action<ClosedEvent>>()))
                .Callback<object, Action<ClosedEvent>>((subscriber, callback) => _doorClosedCallback = callback);

            _doorService = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTestWithLogicDoorClosed()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(true);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }

        [TestMethod]
        public void ConstructorTestWithLogicDoorOpen()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(false);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleDoorOpenEventTestForAnotherDoor()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(true);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            int ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_doorOpenCallback);
            _doorOpenCallback(new OpenEvent((int)DoorLogicalId.Main, string.Empty));

            Assert.AreEqual(0, ruleChangedEventsReceived);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleDoorOpenEventTestForLogicDoor()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(true);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            int ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_doorOpenCallback);
            _doorOpenCallback(new OpenEvent((int)DoorLogicalId.Logic, string.Empty));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleDoorClosedEventTestForAnotherDoor()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(false);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            int ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_doorOpenCallback);
            _doorClosedCallback(new ClosedEvent((int)DoorLogicalId.Main, string.Empty));

            Assert.AreEqual(0, ruleChangedEventsReceived);
            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleDoorClosedEventTestForLogicDoor()
        {
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(false);

            LogicDoorOpenPersistenceClearRule target = new LogicDoorOpenPersistenceClearRule();

            int ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_doorOpenCallback);
            _doorClosedCallback(new ClosedEvent((int)DoorLogicalId.Logic, string.Empty));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual("The Logic Door must be open.", target.ClearDeniedReason);
        }
    }
}