namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LPB7SetMachineNumberHandlerTest
    {
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private LPB7SetMachineNumberHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LPB7SetMachineNumberHandler(_propertiesManager.Object, _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LPB7SetMachineNumberHandler(null, _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBankTest()
        {
            _target = new LPB7SetMachineNumberHandler(_propertiesManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SetMachineNumbers));
        }

        [DataRow(
            "",
            (uint)0,
            "Test",
            (uint)1234,
            true,
            true,
            MachineNumbersControlFlags.AssetNumberConfigurable | MachineNumbersControlFlags.FloorLocationConfigurable,
            DisplayName = "Query results asset and floor location configurable")]
        [DataRow(
            "",
            (uint)0,
            "Test",
            (uint)1234,
            false,
            true,
            MachineNumbersControlFlags.AssetNumberConfigurable,
            DisplayName = "Query results asset configurable")]
        [DataRow(
            "",
            (uint)0,
            "Test",
            (uint)1234,
            true,
            false,
            MachineNumbersControlFlags.FloorLocationConfigurable,
            DisplayName = "Query results floor location configurable")]
        [DataRow(
            "",
            (uint)0,
            "Test",
            (uint)1234,
            false,
            false,
            MachineNumbersControlFlags.None,
            DisplayName = "Query results none configurable")]
        [DataRow(
            "NewLoc",
            (uint)4567,
            "NewLoc",
            (uint)4567,
            true,
            true,
            MachineNumbersControlFlags.AssetNumberConfigurable | MachineNumbersControlFlags.FloorLocationConfigurable,
            DisplayName = "Set data asset and floor location configurable")]
        [DataRow(
            "NewLoc",
            (uint)4567,
            "Test",
            (uint)4567,
            false,
            true,
            MachineNumbersControlFlags.AssetNumberConfigurable,
            DisplayName = "Set data asset configurable")]
        [DataRow(
            "NewLoc",
            (uint)4567,
            "NewLoc",
            (uint)1234,
            true,
            false,
            MachineNumbersControlFlags.FloorLocationConfigurable,
            DisplayName = "Set data floor location configurable")]
        [DataRow(
            "NewLoc",
            (uint)4567,
            "Test",
            (uint)1234,
            false,
            false,
            MachineNumbersControlFlags.None,
            DisplayName = "Set data none configurable")]
        [DataTestMethod]
        public void HandleCommandTest(
            string floorLocation,
            uint assetNumber,
            string expectedFloorLocation,
            uint expectedAssetNumber,
            bool floorLocationConfigurable,
            bool assetNumberConfigurable,
            MachineNumbersControlFlags expectedControlFlags)
        {
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.ChangeAssetNumberSupportedKey, It.IsAny<bool>()))
                .Returns(assetNumberConfigurable);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.ChangeFloorLocationSupportedKey, It.IsAny<bool>()))
                .Returns(floorLocationConfigurable);
            _propertiesManager.Setup(c => c.GetProperty(ApplicationConstants.Location, It.IsAny<string>()))
                .Returns(expectedFloorLocation);
            _propertiesManager.Setup(c => c.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(expectedAssetNumber);
            _bank.Setup(x => x.QueryBalance()).Returns(0L);
            var actual = _target.Handle(new LongPollSetMachineNumbersData(assetNumber, floorLocation));

            Assert.AreEqual(expectedAssetNumber, actual.AssetNumber);
            Assert.AreEqual(expectedFloorLocation, actual.FloorLocation);
            Assert.AreEqual(expectedControlFlags, actual.ControlFlags);
            _propertiesManager.Verify(
                c => c.SetProperty(ApplicationConstants.Location, floorLocation),
                floorLocationConfigurable && !string.IsNullOrEmpty(floorLocation) ? Times.Once() : Times.Never());
            _propertiesManager.Verify(
                c => c.SetProperty(ApplicationConstants.MachineId, assetNumber),
                assetNumberConfigurable && assetNumber != 0 ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public void SettingValuesWithNonZeroBalanceTest()
        {
            const string floorLocation = "TestLocation";
            const uint assetNumber = 1234;
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.ChangeAssetNumberSupportedKey, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.ChangeFloorLocationSupportedKey, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(ApplicationConstants.Location, It.IsAny<string>()))
                .Returns(floorLocation);
            _propertiesManager.Setup(c => c.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(assetNumber);
            _bank.Setup(x => x.QueryBalance()).Returns(1234L);
            var actual = _target.Handle(new LongPollSetMachineNumbersData(1, "Changed"));

            Assert.AreEqual(assetNumber, actual.AssetNumber);
            Assert.AreEqual(floorLocation, actual.FloorLocation);
            _propertiesManager.Verify(
                c => c.SetProperty(ApplicationConstants.Location, It.IsAny<string>()), Times.Never());
            _propertiesManager.Verify(
                c => c.SetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()), Times.Never());
        }
    }
}