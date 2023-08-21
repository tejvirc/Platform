namespace Aristocrat.Monaco.Kernel.Tests
{
    using System.Linq;
    using Components;
    using Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>This class contains all the unit tests for ComponentRegistry.</summary>
    [TestClass]
    public class ComponentRegistryTests
    {
        private Mock<IEventBus> _eventBus;

        /// <summary>The object under test.</summary>
        private ComponentRegistry _target;

        private const string TestComponentId = "TestComponentId";
        private readonly Component TestComponent = new Component();

        public ComponentRegistryTests()
        {
            TestComponent.ComponentId = TestComponentId;
        }

        /// <summary>Initializes necessary items before tests.</summary>
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);

            _target = new ComponentRegistry();
            _target.Initialize();
        }

        /// <summary>Cleans up after each test.</summary>
        [TestCleanup]
        public void TestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("ComponentRegistry", _target.Name);
        }

        [TestMethod]
        public void DisposeTest()
        {
            _target.Dispose();

            // after Dispose the properties should be cleared
            Assert.AreEqual(0, _target.Components.Count());
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(IComponentRegistry)));
            Assert.AreEqual(1, _target.ServiceTypes.Count);
        }

        [TestMethod]
        public void ComponentsTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentAddedEvent>()));

            _target.Register(TestComponent, false);
            Assert.AreEqual(1, _target.Components.Count());
        }

        [TestMethod]
        public void UnRegisterWithUnknownIdTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentRemovedEvent>()));

            Assert.IsFalse(_target.UnRegister(TestComponentId, false));
            Assert.IsFalse(_target.UnRegister(TestComponentId, true));

            _eventBus.Verify(m => m.Publish(It.IsAny<ComponentRemovedEvent>()), Times.Never);
        }

        [TestMethod]
        public void UnRegisterWithKnownIdTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentAddedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentRemovedEvent>()));

            _target.Register(TestComponent, false);
            _target.UnRegister(TestComponentId, false);

            _eventBus.Verify(m => m.Publish(It.IsAny<ComponentAddedEvent>()), Times.Once);
            _eventBus.Verify(m => m.Publish(It.IsAny<ComponentRemovedEvent>()), Times.Once);
        }

        [TestMethod]
        public void CycleTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentAddedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<ComponentRemovedEvent>()));

            _target.Register(TestComponent, true);
            _target.UnRegister(TestComponentId, true);

            _eventBus.Verify(m => m.Publish(It.IsAny<ComponentAddedEvent>()), Times.Never);
            _eventBus.Verify(m => m.Publish(It.IsAny<ComponentRemovedEvent>()), Times.Never);
        }

        [TestMethod]
        public void GetWithUnknownIdTest()
        {
            Assert.IsNull(_target.Get(TestComponentId));
        }

        [TestMethod]
        public void GetWithKnownIdTest()
        {
            _target.Register(TestComponent, false);
            Assert.IsNotNull(_target.Get(TestComponentId));
        }
    }
}