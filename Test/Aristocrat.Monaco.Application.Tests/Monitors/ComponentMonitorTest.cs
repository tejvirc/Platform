namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Application.Monitors;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using Kernel.Contracts.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the ComponentMonitor class
    /// </summary>
    [TestClass]
    public class ComponentMonitorTest
    {
        private ComponentMonitor _target;
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Mock<IComponentRegistry> _componentRegistry;
        private Mock<IPersistentStorageManager> _storage;
        private readonly Mock<IPersistentStorageAccessor> _storageAccessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
        private const string PersistenceBlockName = "Aristocrat.Monaco.Application.ComponentMonitor";

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _componentRegistry = MoqServiceManager.CreateAndAddService<IComponentRegistry>(MockBehavior.Strict);
            _storage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _eventBus.Setup(m => m.Subscribe<ComponentAddedEvent>(It.IsAny<ComponentMonitor>(), It.IsAny<Action<ComponentAddedEvent>>()));
            _eventBus.Setup(m => m.Subscribe<ComponentRemovedEvent>(It.IsAny<ComponentMonitor>(), It.IsAny<Action<ComponentRemovedEvent>>()));
            _eventBus.Setup(m => m.Subscribe<InitializationCompletedEvent>(It.IsAny<ComponentMonitor>(), It.IsAny<Action<InitializationCompletedEvent>>()));
            _storage.Setup(m => m.BlockExists(PersistenceBlockName)).Returns(true);
            _storage.Setup(m => m.GetBlock(PersistenceBlockName)).Returns(_storageAccessor.Object);
            _storageAccessor.Setup(m => m.Level).Returns(PersistenceLevel.Transient);

            _target = new ComponentMonitor(_eventBus.Object, _storage.Object, _componentRegistry.Object);
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void DefaultConstructorTest()
        {
            _target = new ComponentMonitor();
            Assert.IsNotNull(_target);
            _target.Initialize();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventBusTest()
        {
            _target = new ComponentMonitor(null, _storage.Object, _componentRegistry.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullStorageManagerTest()
        {
            _target = new ComponentMonitor(_eventBus.Object, null, _componentRegistry.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullComponentRegistryTest()
        {
            _target = new ComponentMonitor(_eventBus.Object, _storage.Object, null);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("ComponentMonitor", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var types = _target.ServiceTypes;
            Assert.IsTrue(types.Contains(typeof(IComponentMonitor)));
            Assert.AreEqual(1, types.Count);
        }

        [TestMethod]
        public void HaveComponentsChangedFirstBootTest()
        {
            var components = new List<Component>
            {
                new Component { ComponentId = "Printer"},
                new Component { ComponentId = "NoteAcceptor"},
                new Component { ComponentId = "Game1"},
                new Component { ComponentId = "Game2"}
            };

            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);
            transaction.Setup(m => m.Commit()).Verifiable();
            transaction.SetupSet(m => m["Components"] = It.IsAny<byte[]>()).Verifiable();

            _storageAccessor.Setup(m => m["Components"]).Returns(new byte[0]);
            _storageAccessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            _componentRegistry.Setup(m => m.Components).Returns(components);
            _accessor.SafeToCheckForComponentChanges();

            var actual = _target.HaveComponentsChangedWhilePoweredOff("test");

            Assert.IsFalse(actual);
            transaction.Verify();
        }

        [TestMethod]
        public void HaveComponentsChangedNoChangeTest()
        {
            var components = new List<Component>
            {
                new Component { ComponentId = "Printer"},
                new Component { ComponentId = "NoteAcceptor"},
                new Component { ComponentId = "Game1"},
                new Component { ComponentId = "Game2"}
            };

            var componentNames = new List<string> { "Game1", "Game2", "Printer", "NoteAcceptor" };
            _storageAccessor.Setup(m => m["Components"]).Returns(StorageUtilities.ToByteArray(componentNames));
            _componentRegistry.Setup(m => m.Components).Returns(components);
            _accessor.SafeToCheckForComponentChanges();

            var actual = _target.HaveComponentsChangedWhilePoweredOff("test");

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void HaveComponentsChangedTrueTest()
        {
            var components = new List<Component>
            {
                new Component { ComponentId = "Printer"},
                new Component { ComponentId = "NoteAcceptor"},
                new Component { ComponentId = "Game1"},
                new Component { ComponentId = "Game2"}
            };

            var componentNames = new List<string> { "Game1", "NoteAcceptor" };
            var componentBytes = StorageUtilities.ToByteArray(componentNames);
            _storageAccessor.Setup(m => m["Components"]).Returns(componentBytes);
            _componentRegistry.Setup(m => m.Components).Returns(components);
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);
            transaction.Setup(m => m.Commit()).Verifiable();
            transaction.SetupSet(m => m["Components"] = It.IsAny<byte[]>()).Verifiable();

            _storageAccessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            _accessor.SafeToCheckForComponentChanges();

            var actual = _target.HaveComponentsChangedWhilePoweredOff("test");
            Assert.IsTrue(actual);

            // ask second time with same id
            actual = _target.HaveComponentsChangedWhilePoweredOff("test");
            Assert.IsFalse(actual);

            // ask with a different id
            actual = _target.HaveComponentsChangedWhilePoweredOff("test1");
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void UpdateComponentDataBeforeDoneBootingTest()
        {
            _componentRegistry.Setup(m => m.Components).Returns((IEnumerable<Component>)null).Verifiable();
            _accessor.UpdateComponentData();

            _componentRegistry.Verify(m => m.Components, Times.Never);
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<ComponentMonitor>())).Verifiable();

            // 2nd dispose call shouldn't do anything
            _target.Dispose();
            _target.Dispose();

            _eventBus.Verify(m => m.UnsubscribeAll(It.IsAny<ComponentMonitor>()), Times.Once);
        }
    }
}
