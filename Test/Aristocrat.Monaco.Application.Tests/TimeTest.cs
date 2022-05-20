namespace Aristocrat.Monaco.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using Time;

    /// <summary>
    ///     This is a test class for TimeTest and is intended
    ///     to contain all TimeTest Unit Tests
    /// </summary>
    [TestClass]
    public class TimeTest
    {
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IEventBus> _eventBus;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private Time _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);
            _pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo("."));
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);

            _target = new Time();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(ITime)));
        }

        /// <summary>
        ///     A test for SetProperty
        /// </summary>
        [TestMethod]
        public void SetPropertyTest()
        {
            string timeZoneString = TimeZoneInfo.Local.Id;
            TimeZoneInfo propertyValue = TimeZoneInfo.Local;

            _block.SetupSet(m => m["TimeZone"] = timeZoneString).Verifiable();
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            _eventBus.Setup(m => m.Publish(It.IsAny<TimeZoneOffsetUpdatedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<TimeZoneUpdatedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<TimeUpdatedEvent>()));
            _target.SetProperty(ApplicationConstants.TimeZoneKey, propertyValue);

            Assert.AreEqual(timeZoneString, _target.TimeZoneInformation.Id);
            Assert.AreEqual(propertyValue, _target.TimeZoneInformation);
            _block.Verify();
            _persistentStorage.Verify();
        }

        /// <summary>
        ///     A test for GetLocationTime where the the TimeZoneInfo is null
        /// </summary>
        [TestMethod]
        public void GetLocationTimeNullTest()
        {
            DateTime expected = DateTime.Now;

            Assert.AreEqual(expected, _target.GetLocationTime(expected));
        }
    }
}