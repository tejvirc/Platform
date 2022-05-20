namespace Aristocrat.Monaco.Application.Tests
{
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using Contracts.OperatorMenu;
    using Test.Common;

    /// <summary>
    ///     This is a test class for DisableByOperatorManager and is intended
    ///     to contain all DisableByOperatorManager Unit Tests
    /// </summary>
    [TestClass]
    public class DisableByOperatorManagerTest
    {
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        //private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IPersistentStorageAccessor> _persistenceAccessor;

        private Mock<IPersistentStorageManager> _persistenceManager;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IPropertiesManager> _propertiesManager;

        private DisableByOperatorManager _target;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _persistenceManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _persistenceAccessor =
                MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);

            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _target = new DisableByOperatorManager();
            _accessor = new DynamicPrivateObject(_target);
        }

        /// <summary>
        ///     Method to release objects used in the test run.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsFalse(_target.DisabledByOperator);
        }

        [TestMethod]
        public void InitializeTestWithNoBlock()
        {
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuEnteredEvent>>())).Verifiable();

            string blockName = typeof(DisableByOperatorManager).ToString();

            _persistenceManager.Setup(m => m.BlockExists(blockName)).Returns(false);
            _persistenceManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, blockName, 1))
                .Returns(_persistenceAccessor.Object);

            _target.Initialize();

            _persistenceManager.Verify(m => m.BlockExists(blockName), Times.Once());
            _persistenceManager.Verify(m => m.CreateBlock(PersistenceLevel.Critical, blockName, 1), Times.Once());

            Assert.IsFalse(_target.DisabledByOperator);
        }

        [TestMethod]
        public void InitializeTestWithBlock()
        {
             string blockName = typeof(DisableByOperatorManager).ToString();

             _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuEnteredEvent>>())).Verifiable();

            _persistenceManager.Setup(m => m.BlockExists(blockName)).Returns(true);
            _persistenceManager.Setup(m => m.GetBlock(blockName)).Returns(_persistenceAccessor.Object);
            _persistenceAccessor.Setup(m => m["DisabledByOperator"]).Returns(true);

            // string reasonText = Resources.OutOfService;
            string reasonText = "Out Of Service";
            Guid expectedGuid = (Guid)_accessor.DisableGuid;

            _systemDisableManager.Setup(m => m.Disable(expectedGuid, SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == reasonText), null));
            
            _eventBus.Setup(m => m.Publish(It.IsAny<SystemDisabledByOperatorEvent>()));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.DisabledByOperatorText, string.Empty))
                .Returns(string.Empty);

            _target.Initialize();

            Assert.IsTrue(_target.DisabledByOperator);

            _systemDisableManager.Verify(
                m => m.Disable(expectedGuid, SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == reasonText), null),
                Times.Once());

            _eventBus.Verify(m => m.Publish(It.IsAny<SystemDisabledByOperatorEvent>()), Times.Once());

            _persistenceManager.Verify(m => m.BlockExists(blockName), Times.Once());
            _persistenceManager.Verify(m => m.GetBlock(blockName), Times.Once());
            _persistenceAccessor.Verify(m => m["DisabledByOperator"], Times.Once());
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Disable-By-Operator Manager", _target.Name);
            Assert.IsFalse(_target.DisabledByOperator);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual((new[] { typeof(IDisableByOperatorManager) }).GetType(), _target.ServiceTypes.GetType());
        }

        [TestMethod]
        public void DisabledByOperatorTest()
        {
            Assert.IsFalse(_target.DisabledByOperator);
            _accessor.DisabledByOperator = true;
            Assert.IsTrue(_target.DisabledByOperator);
        }

        [TestMethod]
        public void DisableTestWhenAlreadyDisabledByOperator()
        {
            _accessor.DisabledByOperator = true;

            _target.Disable(() => "foo");

            Assert.IsTrue(_target.DisabledByOperator);
        }

        [TestMethod]
        public void DisableTestWhenNotDisabledByOperator()
        {
            var blockName = typeof(DisableByOperatorManager).ToString();
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _persistenceManager.Setup(m => m.GetBlock(blockName)).Returns(_persistenceAccessor.Object);
            _persistenceAccessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            var disabled = false;
            transaction.SetupSet(m => m["DisabledByOperator"] = true).Callback(() => { disabled = true; });
            transaction.Setup(m => m.Commit());
            transaction.Setup(m => m.Dispose());

            const string reasonText = "For unit test";
            var expectedGuid = (Guid)_accessor.DisableGuid;

            _systemDisableManager.Setup(m => m.Disable(expectedGuid, SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == reasonText), null));

            _eventBus.Setup(m => m.Publish(It.IsAny<SystemDisabledByOperatorEvent>()));
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.DisabledByOperatorText, string.Empty))
                .Returns(string.Empty);
            
            _target.Disable(() => reasonText);

            Assert.IsTrue(_target.DisabledByOperator);

            _systemDisableManager.Verify(
                m => m.Disable(expectedGuid, SystemDisablePriority.Immediate, It.Is<Func<string>>(x => x.Invoke() == reasonText), null),
                Times.Once());

            _eventBus.Verify(m => m.Publish(It.IsAny<SystemDisabledByOperatorEvent>()), Times.Once());

            _persistenceManager.Verify(m => m.GetBlock(blockName), Times.Once());
            Assert.IsTrue(disabled);
            transaction.Verify(m => m.Commit(), Times.Once());
        }

        [TestMethod]
        public void EnableTestWhenDisabledByOperator()
        {
            _accessor.DisabledByOperator = true;

            var blockName = typeof(DisableByOperatorManager).ToString();

            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _persistenceManager.Setup(m => m.GetBlock(blockName)).Returns(_persistenceAccessor.Object);
            _persistenceAccessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            var disabled = true;
            transaction.SetupSet(m => m["DisabledByOperator"] = false).Callback(() => { disabled = false; });
            transaction.Setup(m => m.Commit());
            transaction.Setup(m => m.Dispose());
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.DisabledByOperatorText, string.Empty))
                .Returns(string.Empty);

            var expectedGuid = (Guid)_accessor.DisableGuid;
            _systemDisableManager.Setup(m => m.Enable(expectedGuid));
            _systemDisableManager.Setup(
                m => m.Disable(expectedGuid, SystemDisablePriority.Immediate, () => "Out Of Service", null));

            _eventBus.Setup(m => m.Publish(It.IsAny<SystemDisabledByOperatorEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<SystemEnabledByOperatorEvent>()));

            _target.Enable();

            Assert.IsFalse(_target.DisabledByOperator);

            _systemDisableManager.Verify(m => m.Enable(expectedGuid), Times.Once());

            _eventBus.Verify(m => m.Publish(It.IsAny<SystemEnabledByOperatorEvent>()), Times.Once());

            _persistenceManager.Verify(m => m.GetBlock(blockName), Times.Once());
            Assert.IsFalse(disabled);
            transaction.Verify(m => m.Commit(), Times.Once());
        }

        [TestMethod]
        public void EnableTestWhenNotDisabledByOperator()
        {
            _target.Enable();

            Assert.IsFalse(_target.DisabledByOperator);
        }
    }
}