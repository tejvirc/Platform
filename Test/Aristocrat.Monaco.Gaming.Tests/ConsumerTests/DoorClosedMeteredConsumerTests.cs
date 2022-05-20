namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Application.Contracts;
    using Consumers;
    using Contracts;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the DoorClosedMeteredConsumer class
    /// </summary>
    [TestClass]
    public class DoorClosedMeteredConsumerTests
    {
        private DoorClosedMeteredConsumer _target;
        private readonly Mock<IMeterManager> _meterManager = new Mock<IMeterManager>(MockBehavior.Strict);
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.ResetGamesPlayedSinceDoorClosedBelly, true))
                .Returns(true);
            _target = new DoorClosedMeteredConsumer(_meterManager.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterManagerTest()
        {
            _target = new DoorClosedMeteredConsumer(null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new DoorClosedMeteredConsumer(_meterManager.Object, null);
        }


        [DataRow((int)DoorLogicalId.Main, "Main Door", DisplayName = "Main Door Closed")]
        [DataRow((int)DoorLogicalId.Belly, "Belly Door", DisplayName = "Belly Door Closed")]
        [DataRow((int)DoorLogicalId.DropDoor, "Drop Door", DisplayName = "Drop Door Closed")]
        [DataRow((int)DoorLogicalId.MainOptic, "Main Optic Door", DisplayName = "Main Optic Door Closed")]
        [DataRow((int)DoorLogicalId.TopBox, "Top Box Door", DisplayName = "Top Box Door Closed")]
        [DataRow((int)DoorLogicalId.TopBoxOptic, "Top Box Optic Door", DisplayName = "Top Box Optic Door Closed")]
        [DataRow((int)DoorLogicalId.UniversalInterfaceBox, "Universal Interface Box Door", DisplayName = "Universal Interface Box Door Closed")]
        [DataTestMethod]
        public void ConsumeDoorsThatResetMeterTest(int door, string name)
        {
            var (closed, meter) = SetupMocks(door, name);

            _target.Consume(closed);

            meter.Verify();
        }

        [DataRow((int)DoorLogicalId.Logic, "Logic Door", DisplayName = "Logic Door Closed")]
        [DataRow((int)DoorLogicalId.CashBox, "CashBox Door", DisplayName = "CashBox Door Closed")]
        [DataTestMethod]
        public void ConsumeDoorsThatDoNotResetMeterTest(int door, string name)
        {
            var closed = new DoorClosedMeteredEvent(door, name);
            _target.Consume(closed);

            // no mock failures means we pass
        }

        [DataRow((int)DoorLogicalId.Belly, "Belly Door", true, DisplayName = "Belly Door Closed")]
        [DataRow((int)DoorLogicalId.Belly, "Belly Door", false, DisplayName = "Belly Door Closed")]
        [DataTestMethod]
        public void ConsumeDoorsThatResetMeterConditionallyTest(int door, string name, bool allowed)
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.ResetGamesPlayedSinceDoorClosedBelly, true))
                .Returns(allowed);

            _target = new DoorClosedMeteredConsumer(_meterManager.Object, _propertiesManager.Object);

            var (closed, meter) = SetupMocks(door, name, allowed);

            _target.Consume(closed);

            meter.Verify();
        }

        private (DoorClosedMeteredEvent closed, Mock<IMeter> meter) SetupMocks(int door, string name, bool allowed = true)
        {
            var oldGamesPlayedCount = 123;
            var closed = new DoorClosedMeteredEvent(door, name);
            var meter = new Mock<IMeter>(MockBehavior.Strict);
            if (allowed)
            {
                meter.Setup(m => m.Lifetime).Returns(oldGamesPlayedCount).Verifiable();
                meter.Setup(m => m.Increment(-oldGamesPlayedCount)).Verifiable();
            }
            _meterManager.Setup(m => m.GetMeter(GamingMeters.GamesPlayedSinceDoorClosed)).Returns(meter.Object);
            return (closed, meter);
        }
    }
}
