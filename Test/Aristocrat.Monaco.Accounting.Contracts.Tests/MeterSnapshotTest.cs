namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System.Collections.Generic;
    using Application.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using MeterSnapshot = Contracts.MeterSnapshot;

    /// <summary>
    ///     Summary description for MeterSnapshotTest
    /// </summary>
    [TestClass]
    public class MeterSnapshotTest
    {
        private Mock<IPropertiesManager> _propertiesManager;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            int persistenceBlockIndexer = 0;
            long lifetime = 456;
            Mock<IPersistentStorageAccessor> block = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            block.Setup(m => m[persistenceBlockIndexer, It.IsAny<string>()]).Returns(lifetime);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            Mock<IMeterProvider> provider = new Mock<IMeterProvider>(MockBehavior.Strict);
            provider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();
            string name = "test";
            MeterClassification classification = new CurrencyMeterClassification();

            IMeter meter = new AtomicMeter(
                name,
                block.Object,
                persistenceBlockIndexer,
                classification,
                provider.Object);
            var target = new MeterSnapshot(meter);

            Assert.IsNotNull(target);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(lifetime, target.Lifetime);
            Assert.AreEqual(lifetime, target.Period);
            Assert.AreEqual(0, target.Session);
        }

        [TestMethod]
        public void CopyConstructorTest()
        {
            long lifetime = 456;
            long period = 123;
            long session = 789;
            string name = "test Copy";

            var values = new Dictionary<MeterValueType, long>
            {
                { MeterValueType.Lifetime, lifetime },
                { MeterValueType.Period, period },
                { MeterValueType.Session, session }
            };

            Application.Contracts.MeterSnapshot source =
                new Application.Contracts.MeterSnapshot { Name = name, Values = values };

            MeterSnapshot target = new MeterSnapshot(source);

            Assert.IsNotNull(target);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(lifetime, target.Lifetime);
            Assert.AreEqual(period, target.Period);
            Assert.AreEqual(session, target.Session);
        }

        [TestMethod]
        public void EqualityTests()
        {
            int persistenceBlockIndexer = 0;
            long lifetime = 456;
            Mock<IPersistentStorageAccessor> block = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            block.Setup(m => m[persistenceBlockIndexer, It.IsAny<string>()]).Returns(lifetime);

            Mock<IMeterProvider> provider = new Mock<IMeterProvider>(MockBehavior.Strict);
            provider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            string name = "test";
            MeterClassification classification = new CurrencyMeterClassification();

            IMeter meter1 = new AtomicMeter(
                name,
                block.Object,
                persistenceBlockIndexer,
                classification,
                provider.Object);
            IMeter meter2 = new AtomicMeter(
                name + "2",
                block.Object,
                persistenceBlockIndexer,
                classification,
                provider.Object);
            var target1 = new MeterSnapshot(meter1);
            var target2 = new MeterSnapshot(meter1);
            var target3 = new MeterSnapshot(meter2);

            // test for ==
            Assert.IsTrue(target1 == target2);
            Assert.IsFalse(target1 == target3);

            // test for !=
            Assert.IsFalse(target1 != target2);
            Assert.IsTrue(target1 != target3);

            // test for target1.Equals(target2)
            Assert.IsTrue(target1.Equals(target2));
        }

        [TestMethod]
        public void ToStringTest()
        {
            int persistenceBlockIndexer = 0;
            long lifetime = 456;
            Mock<IPersistentStorageAccessor> block = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            block.Setup(m => m[persistenceBlockIndexer, It.IsAny<string>()]).Returns(lifetime);

            Mock<IMeterProvider> provider = new Mock<IMeterProvider>(MockBehavior.Strict);
            provider.Setup(m => m.RegisterMeterClearDelegate(It.IsAny<ClearPeriodMeter>())).Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);
            string name = "test";
            MeterClassification classification = new CurrencyMeterClassification();

            IMeter meter = new AtomicMeter(
                name,
                block.Object,
                persistenceBlockIndexer,
                classification,
                provider.Object);
            var target = new MeterSnapshot(meter);

            string expected =
                "Aristocrat.Monaco.Accounting.Contracts.MeterSnapshot [name=test,lifetime=456,period=456,session=0]";
            Assert.AreEqual(expected, target.ToString());
        }
    }
}