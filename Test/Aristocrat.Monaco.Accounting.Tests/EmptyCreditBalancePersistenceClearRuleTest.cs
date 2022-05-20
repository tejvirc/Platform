namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Test class for the EmptyCreditBalancePersistenceClearRule class.
    /// </summary>
    [TestClass]
    public class EmptyCreditBalancePersistenceClearRuleTest
    {
        private const string CreditsUnavailableDenyReason = "Credit balance is unavailable.";

        private const string CreditsExistDenyReason =
            "Please remove all credits from the machine before clearing all data.";

        private Action<BankBalanceChangedEvent> _bankBalanceChangedCallback;
        private Mock<IEventBus> _eventBus;
        private Action<ServiceAddedEvent> _serviceAddedCallback;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EmptyCreditBalancePersistenceClearRule>(),
                        It.IsAny<Action<ServiceAddedEvent>>()))
                .Callback<object, Action<ServiceAddedEvent>
                >((subscriber, callback) => _serviceAddedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EmptyCreditBalancePersistenceClearRule>(),
                        It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>(
                    (subscriber, callback) => _bankBalanceChangedCallback = callback);
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
        public void ConstructorTestWithNoBank()
        {
            MoqServiceManager.RemoveService<IBank>();

            var target = new EmptyCreditBalancePersistenceClearRule();

            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual(CreditsUnavailableDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void ConstructorTestWithCredits()
        {
            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(1L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var target = new EmptyCreditBalancePersistenceClearRule();

            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void ConstructorTestWithNoCredits()
        {
            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(0L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var target = new EmptyCreditBalancePersistenceClearRule();

            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleServiceAddedEventTestForAnotherService()
        {
            MoqServiceManager.RemoveService<IBank>();

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var target = new EmptyCreditBalancePersistenceClearRule();

            var ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_serviceAddedCallback);
            _serviceAddedCallback(new ServiceAddedEvent(typeof(IEventBus)));

            Assert.AreEqual(0, ruleChangedEventsReceived);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual(CreditsUnavailableDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleServiceAddedEventTestForBankWithCredits()
        {
            MoqServiceManager.RemoveService<IBank>();

            var target = new EmptyCreditBalancePersistenceClearRule();

            var ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(1L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            Assert.IsNotNull(_serviceAddedCallback);
            _serviceAddedCallback(new ServiceAddedEvent(typeof(IBank)));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleServiceAddedEventTestForBankWithNoCredits()
        {
            MoqServiceManager.RemoveService<IBank>();

            var target = new EmptyCreditBalancePersistenceClearRule();

            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(0L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_serviceAddedCallback);
            _serviceAddedCallback(new ServiceAddedEvent(typeof(IBank)));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleBankBalanceChangedEventTestForCreditsGoingToZero()
        {
            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(1L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var target = new EmptyCreditBalancePersistenceClearRule();

            var ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_serviceAddedCallback);
            _bankBalanceChangedCallback(new BankBalanceChangedEvent(1L, 0L, Guid.Empty));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsTrue(target.PartialClearAllowed);
            Assert.IsTrue(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }

        [TestMethod]
        public void HandleBankBalanceChangedEventTestForCreditsAdded()
        {
            var bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            bank.Setup(m => m.QueryBalance()).Returns(0L);

            _eventBus.Setup(m => m.Unsubscribe<ServiceAddedEvent>(It.IsAny<EmptyCreditBalancePersistenceClearRule>()));

            var target = new EmptyCreditBalancePersistenceClearRule();

            var ruleChangedEventsReceived = 0;
            target.RuleChangedEvent += (sender, eventargs) => ++ruleChangedEventsReceived;

            Assert.IsNotNull(_serviceAddedCallback);
            _bankBalanceChangedCallback(new BankBalanceChangedEvent(0L, 1L, Guid.Empty));

            Assert.AreEqual(1, ruleChangedEventsReceived);
            Assert.IsFalse(target.PartialClearAllowed);
            Assert.IsFalse(target.FullClearAllowed);
            Assert.AreEqual(CreditsExistDenyReason, target.ClearDeniedReason);
        }
    }
}