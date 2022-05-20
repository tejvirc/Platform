namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class BooleanPropertyConditionTests
    {
        private const string NameOfIdAttribute = "id";
        private const string NameOfCompareToAttribute = "compareTo";
        private const string NameOfDefaultValueAttribute = "defaultValue";
        private const string ValueOfPropertyNameAttribute = "myTestProperty";

        private TestEventBus _eventBusMock;
        private Mock<NodeElement> _nodeElementMock;
        private TestPropertiesManager _propertiesManager;
        private BooleanPropertyCondition _target;

        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Use TestInitialize to run code before running each test
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _target = new BooleanPropertyCondition();

            var currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory, currentDirectory);

            _eventBusMock = new TestEventBus();
            _propertiesManager = new TestPropertiesManager();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService<IPropertiesManager>(_propertiesManager);
            MoqServiceManager.AddService<IEventBus>(_eventBusMock);
        }

        /// <summary>
        ///     Cleans up ServiceManager after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        /// <summary>A test of Evaluate with property value set to true; without, then with a "compareTo" attribute given</summary>
        [TestMethod]
        public void EvaluateTrueTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, true);

            Assert.IsTrue(_target.Evaluate(_nodeElementMock.Object));

            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfCompareToAttribute)).Returns("true");

            Assert.IsTrue(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with property value set to false; without, then with a "compareTo" attribute given</summary>
        [TestMethod]
        public void EvaluateFalseTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, false);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));

            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfCompareToAttribute)).Returns("true");

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with property value set to true and a "compareTo" attribute set to false</summary>
        [TestMethod]
        public void EvaluateTrueVersusFalseTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfCompareToAttribute)).Returns("false");

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, true);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with property value set to false and a "compareTo" attribute set to false</summary>
        [TestMethod]
        public void EvaluateFalseVersusFalseTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfCompareToAttribute)).Returns("false");

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, false);

            Assert.IsTrue(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with default value returned by property manager.</summary>
        [TestMethod]
        public void EvaluateDefaultTest()
        {
            // This tests the use of the defaultValue attribute in the Condition node. It is given to the Properties Manager
            // as a default to return if the property is not found.

            // Test 1 -- No default specified
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));

            // Test 2 -- Default of false specified
            var defaultPropertyValue = "false";

            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfDefaultValueAttribute))
                .Returns(defaultPropertyValue);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));

            // Test 3 -- Default of true specified
            defaultPropertyValue = "true";

            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfDefaultValueAttribute))
                .Returns(defaultPropertyValue);

            Assert.IsTrue(_target.Evaluate(_nodeElementMock.Object));

            // Test 4 -- Default attribute that can't be parsed into a boolean
            defaultPropertyValue = "maybe";

            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfDefaultValueAttribute))
                .Returns(defaultPropertyValue);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with a node with unexpected attribute name</summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EvaluateUnexpectedNodeAttributeTest()
        {
            var badAttributeName = "bogus";
            var badAttributeValue = "who cares";

            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(badAttributeName)).Returns(badAttributeValue);

            Assert.IsFalse(_target.Evaluate(_nodeElementMock.Object));
        }

        /// <summary>A test of Evaluate with a property that is not boolean type</summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void EvaluateWithWrongTypeOfPropertyTest()
        {
            var wrongType = "wrong";

            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, wrongType);

            _target.Evaluate(_nodeElementMock.Object);
        }

        /// <summary>A test of ReceiveEvent before any Evaluate calls are made</summary>
        /// <remarks>Nothing to be checked at the unit test level, except that no exceptions are thrown.</remarks>
        [TestMethod]
        public void ReceiveEventBeforeEvaluateTest()
        {
            _eventBusMock.Publish(new PropertyChangedEvent("isParticipatingInProgressive"));
        }

        /// <summary>A test of ReceiveEvent after an Evaluate call is made</summary>
        /// <remarks>Nothing to be checked at the unit test level, except that no exceptions are thrown.</remarks>
        [TestMethod]
        public void ReceiveEventAfterEvaluateTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, true);

            _target.Evaluate(_nodeElementMock.Object);

            var propertyChanged = new PropertyChangedEvent("isParticipatingInProgressive");
            _eventBusMock.Publish(propertyChanged);
        }

        /// <summary>A test of two calls to Evaluate with different property names.</summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EvaluateWithDifferentPropertyTest()
        {
            _nodeElementMock = new Mock<NodeElement>();
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns(ValueOfPropertyNameAttribute);

            _propertiesManager.SetProperty(ValueOfPropertyNameAttribute, true);

            Assert.IsTrue(_target.Evaluate(_nodeElementMock.Object));

            // Calling Evaluate with a node that has a different property name attribute than the initial call should result in an exception.
            _nodeElementMock.Setup(mock => mock.GetAttribute(NameOfIdAttribute)).Returns("AnotherPropertyName");
            _target.Evaluate(_nodeElementMock.Object);
        }
    }
}