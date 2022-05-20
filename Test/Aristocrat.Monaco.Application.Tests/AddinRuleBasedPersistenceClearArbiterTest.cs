namespace Aristocrat.Monaco.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Test class for the AddinRuleBasedPersistenceClearArbiter class.
    /// </summary>
    [TestClass]
    public class AddinRuleBasedPersistenceClearArbiterTest
    {
        private dynamic _accessor;
        private Mock<IAddinHelper> _addinHelper;
        private Mock<IEventBus> _eventBus;

        private List<PersistenceClearAuthorizationChangedEvent> _postedEvents =
            new List<PersistenceClearAuthorizationChangedEvent>();

        private AddinRuleBasedPersistenceClearArbiter _target;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _addinHelper = MoqServiceManager.CreateAndAddService<IAddinHelper>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _target = new AddinRuleBasedPersistenceClearArbiter();
            _accessor = new DynamicPrivateObject(_target);
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
        public void ConstructorTest()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_target.Name));
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(IPersistenceClearArbiter)));
            Assert.IsFalse(_target.PartialClearAllowed);
            Assert.IsFalse(_target.FullClearAllowed);
            Assert.AreEqual(0, _target.PartialClearDeniedReasons.Length);
            Assert.AreEqual(0, _target.FullClearDeniedReasons.Length);
        }

        [TestMethod]
        public void InitializeTest()
        {
            MockPersistenceClearAuthorizationChangedEventPost();

            MockRuleExtensions(
                new Mock<IPersistenceClearRule>[]
                {
                    CreateMockRule(true, false, "Rule 1 reason"),
                    CreateMockRule(true, true, "Rule 2 reason")
                });

            _target.Initialize();

            Assert.IsTrue(_target.PartialClearAllowed);
            Assert.IsFalse(_target.FullClearAllowed);
            Assert.AreEqual(0, _target.PartialClearDeniedReasons.Length);
            Assert.AreEqual(1, _target.FullClearDeniedReasons.Length);
            Assert.IsTrue(_target.FullClearDeniedReasons.Contains("Rule 1 reason"));

            Assert.AreEqual(1, _postedEvents.Count);
            Assert.IsTrue(_postedEvents[0].PartialClearAllowed);
            Assert.IsFalse(_postedEvents[0].FullClearAllowed);
            Assert.AreEqual(0, _postedEvents[0].PartialClearDeniedReasons.Length);
            Assert.AreEqual(1, _postedEvents[0].FullClearDeniedReasons.Length);
            Assert.IsTrue(_postedEvents[0].FullClearDeniedReasons.Contains("Rule 1 reason"));
        }

        [TestMethod]
        public void OnRuleChangedTest()
        {
            MockPersistenceClearAuthorizationChangedEventPost();

            var rule1 = CreateMockRule(true, true, "Rule 1 reason");
            var rule2 = CreateMockRule(true, true, "Rule 2 reason");

            MockRuleExtensions(
                new Mock<IPersistenceClearRule>[]
                {
                    rule1,
                    rule2
                });

            _target.Initialize();

            // Clear out the posted events and change rule2 to deny partial clears
            _postedEvents.Clear();
            rule2.SetupGet(m => m.PartialClearAllowed).Returns(false);

            _accessor.OnRuleChanged(null, null);

            Assert.IsFalse(_target.PartialClearAllowed);
            Assert.IsTrue(_target.FullClearAllowed);
            Assert.AreEqual(1, _target.PartialClearDeniedReasons.Length);
            Assert.AreEqual(0, _target.FullClearDeniedReasons.Length);
            Assert.IsTrue(_target.PartialClearDeniedReasons.Contains("Rule 2 reason"));

            Assert.AreEqual(1, _postedEvents.Count);
            Assert.IsFalse(_postedEvents[0].PartialClearAllowed);
            Assert.IsTrue(_postedEvents[0].FullClearAllowed);
            Assert.AreEqual(1, _postedEvents[0].PartialClearDeniedReasons.Length);
            Assert.AreEqual(0, _postedEvents[0].FullClearDeniedReasons.Length);
            Assert.IsTrue(_postedEvents[0].PartialClearDeniedReasons.Contains("Rule 2 reason"));
        }

        [TestMethod]
        public void DisposeTest()
        {
            MockPersistenceClearAuthorizationChangedEventPost();

            var rule1 = CreateMockRule(true, true, "Rule 1 reason");
            var disposableRule1 = rule1.As<IDisposable>();
            disposableRule1.Setup(m => m.Dispose());

            var rule2 = CreateMockRule(true, true, "Rule 2 reason");
            var disposableRule2 = rule2.As<IDisposable>();
            disposableRule2.Setup(m => m.Dispose());

            MockRuleExtensions(
                new Mock<IPersistenceClearRule>[]
                {
                    rule1,
                    rule2
                });

            _target.Initialize();

            _target.Dispose();

            disposableRule1.Verify(m => m.Dispose(), Times.Once);
            disposableRule2.Verify(m => m.Dispose(), Times.Once);

            // A second dispose should do nothing
            _target.Dispose();

            disposableRule1.Verify(m => m.Dispose(), Times.Once);
            disposableRule2.Verify(m => m.Dispose(), Times.Once);
        }

        private void MockPersistenceClearAuthorizationChangedEventPost()
        {
            _postedEvents.Clear();
            _eventBus
                .Setup(m => m.Publish(It.IsAny<PersistenceClearAuthorizationChangedEvent>()))
                .Callback<IEvent>((theEvent) => _postedEvents.Add((PersistenceClearAuthorizationChangedEvent)theEvent));
        }

        private Mock<IPersistenceClearRule> CreateMockRule(bool partial, bool full, string reason)
        {
            var rule = new Mock<IPersistenceClearRule>(MockBehavior.Loose);
            rule.SetupGet(m => m.PartialClearAllowed).Returns(partial);
            rule.SetupGet(m => m.FullClearAllowed).Returns(full);
            rule.SetupGet(m => m.ClearDeniedReason).Returns(reason);
            return rule;
        }

        private void MockRuleExtensions(Mock<IPersistenceClearRule>[] rules)
        {
            var nodeList = new List<TypeExtensionNode>();
            foreach (var mockRule in rules)
            {
                nodeList.Add(new TestTypeExtensionNode(mockRule.Object));
            }

            _addinHelper
                .Setup(m => m.GetSelectedNodes<TypeExtensionNode>("/Application/PersistenceClearRules"))
                .Returns(nodeList);
        }

        private class TestTypeExtensionNode : TypeExtensionNode
        {
            private object _createInstanceReturn;

            public TestTypeExtensionNode(object createInstanceReturn)
            {
                _createInstanceReturn = createInstanceReturn;
            }

            public override object CreateInstance()
            {
                return _createInstanceReturn;
            }
        }
    }
}