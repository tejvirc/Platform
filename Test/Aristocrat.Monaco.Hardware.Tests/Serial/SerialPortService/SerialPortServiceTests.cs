namespace Aristocrat.Monaco.Hardware.Tests.Serial.SerialPortService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.Services;
    using Cabinet.Contracts;
    using Contracts.Cabinet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SerialPortServiceTests
    {
        private const string Com3PortName = "COM3";
        private const string NonExistingPortName = "COM1";
        private static readonly Random Random = new Random();

        private readonly IList<SerialPortInfo> _listOfSerialPorts =
            new List<SerialPortInfo>();

        private Mock<SerialPortEnumerator> _moqSerialPortEnumerator;
        private Mock<ICabinetDetectionService> _cabinetDetectionServiceMoq;

        /*
            These are serial port expected values, taken from Gen8 hardware.
            {SerialPort Name, Address}
            { "COM3", 0 },
            { "COM4", 1 },
            { "COM5", 2 },
            { "COM6", 3 },
            { "COM7", 4 },
            { "COM8", 5 },
            { "COM9", 6 },
            { "COM10", 7 },
            { "COM11", 8 },
            { "COM12", 9 },
            { "COM13", 10 },
            { "COM14", 11 },
            { "COM15", 12 }
        */

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _cabinetDetectionServiceMoq =
                MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);
            _cabinetDetectionServiceMoq.Setup(x => x.Family).Returns(HardwareFamily.Gen8);

            _moqSerialPortEnumerator = new Mock<SerialPortEnumerator>();
            _moqSerialPortEnumerator.Setup(x => x.EnumerateSerialPorts()).Returns(_listOfSerialPorts);
        }

        [TestMethod]
        public void RegisterASerialPortAndExpectCorrectPhysicalandLogicalName()
        {
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 1);

            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            Assert.AreEqual(GetPhysicalPortString(0), serialPortService.LogicalToPhysicalName(Com3PortName));
            Assert.AreEqual(NonExistingPortName, serialPortService.LogicalToPhysicalName(NonExistingPortName));

            Assert.AreEqual(Com3PortName, serialPortService.PhysicalToLogicalName(GetPhysicalPortString(0)));
            Assert.AreEqual(NonExistingPortName, serialPortService.PhysicalToLogicalName(NonExistingPortName));
        }

        [TestMethod]
        public void RunningWithNoSerialPorts()
        {
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            Assert.AreEqual(Com3PortName, serialPortService.LogicalToPhysicalName(Com3PortName));
            Assert.AreEqual(Com3PortName, serialPortService.PhysicalToLogicalName(Com3PortName));
        }

        [TestMethod]
        public void ExtraComPortsShouldReturnLogicalPortNamesAsPhysicalPortNames()
        {
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 20, 20);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);

            var allLogicalPortNames = serialPortService.GetAllLogicalPortNames();
            foreach (var port in allLogicalPortNames)
            {
                Assert.AreEqual(port, serialPortService.LogicalToPhysicalName(port));
                Assert.AreEqual(port, serialPortService.PhysicalToLogicalName(port));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RegisterNonExistingPort()
        {
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(NonExistingPortName));
        }

        [TestMethod]
        public void RegisterAValidPort()
        {
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 1);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            try
            {
                serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TryToRegisterAnAlreadyRegisteredPort()
        {
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 1);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);

            serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
            serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void UnRegisterAnNonExistingPort()
        {
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            serialPortService.UnRegisterPort(serialPortService.LogicalToPhysicalName(NonExistingPortName));
        }

        [TestMethod]
        public void UnRegisterPort()
        {
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 1);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);

            serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
            serialPortService.UnRegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
            // This should be successful without exception since we unregistered it.
            serialPortService.RegisterPort(serialPortService.LogicalToPhysicalName(Com3PortName));
        }

        [TestMethod]
        public void TestUsbSerialPort()
        {
            AddSerialPortsOfGivenType(SerialPortType.Usb, 5);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);
            // Since we do not map them, we should get same logical ports as physical ports
            foreach (var serialPort in _listOfSerialPorts)
            {
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, SerialPortType.Usb),
                    serialPortService.LogicalToPhysicalName(
                        GetPhysicalPortString(serialPort.Address, SerialPortType.Usb)));
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, SerialPortType.Usb),
                    serialPortService.PhysicalToLogicalName(
                        GetPhysicalPortString(serialPort.Address, SerialPortType.Usb)));
            }
        }

        [TestMethod]
        public void TestOnUnknownDevice()
        {
            // No mapping happens on Unknown devices, hence logical ports to be returned as is.
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 4);
            AddSerialPortsOfGivenType(SerialPortType.Usb, 4);
            _cabinetDetectionServiceMoq.Setup(x => x.Family).Returns(HardwareFamily.Unknown);
            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);

            foreach (var serialPort in _listOfSerialPorts)
            {
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType),
                    serialPortService.LogicalToPhysicalName(
                        GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType)));
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType),
                    serialPortService.PhysicalToLogicalName(
                        GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType)));
            }

            Assert.AreEqual(Com3PortName, serialPortService.LogicalToPhysicalName(Com3PortName));
            Assert.AreEqual(Com3PortName, serialPortService.PhysicalToLogicalName(Com3PortName));
        }

        [TestMethod]
        public void TestOnNullCabinetDetectionService()
        {
            MoqServiceManager.RemoveService<ICabinetDetectionService>();
            _cabinetDetectionServiceMoq = null;

            // No mapping happens on Unknown devices, hence logical ports to be returned as is.
            AddSerialPortsOfGivenType(SerialPortType.Rs232, 4);
            AddSerialPortsOfGivenType(SerialPortType.Usb, 4);

            var serialPortService = new SerialPortsService(_moqSerialPortEnumerator.Object);

           foreach (var serialPort in _listOfSerialPorts)
            {
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType),
                    serialPortService.LogicalToPhysicalName(
                        GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType)));
                Assert.AreEqual(
                    GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType),
                    serialPortService.PhysicalToLogicalName(
                        GetPhysicalPortString(serialPort.Address, serialPort.SerialPortType)));
            }

            Assert.AreEqual(Com3PortName, serialPortService.LogicalToPhysicalName(Com3PortName));
            Assert.AreEqual(Com3PortName, serialPortService.PhysicalToLogicalName(Com3PortName));
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private string GetPhysicalPortString(int address, SerialPortType type = SerialPortType.Rs232)
        {
            return _listOfSerialPorts.Where(x => x.SerialPortType == type && x.Address == address)
                .Select(x => x.PhysicalPortName).First();
        }

        private void AddSerialPortsOfGivenType(SerialPortType type, int count, int startingAddress = 0)
        {
            for (var i = startingAddress; i < count + startingAddress; i++)
            {
                _listOfSerialPorts.Add(
                    new SerialPortInfo { SerialPortType = type, Address = i, PhysicalPortName = RandomString(4) });
            }
        }
    }
}