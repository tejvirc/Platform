namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using Contracts.SerialPorts;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This file contains the tests for SharedDeviceContracts
    /// </summary>
    [TestClass]
    public class SharedDeviceContractsTests
    {
        private Mock<ISerialPortsService> _ports;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if (AddinManager.IsInitialized)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }
        }

        [TestMethod]
        public void DeviceConfigurationTest()
        {
            var target = new DeviceConfiguration();
            Assert.AreEqual(string.Empty, target.Protocol);
            Assert.AreEqual(string.Empty, target.Manufacturer);
            Assert.AreEqual(string.Empty, target.Model);
            Assert.AreEqual(string.Empty, target.SerialNumber);
            Assert.AreEqual(string.Empty, target.FirmwareBootVersion);
            Assert.AreEqual(string.Empty, target.FirmwareRevision);
            Assert.AreEqual(string.Empty, target.FirmwareCyclicRedundancyCheck);
            Assert.AreEqual(string.Empty, target.VariantName);
            Assert.AreEqual(string.Empty, target.FirmwareId);
            Assert.AreEqual(string.Empty, target.VariantVersion);
            Assert.AreEqual(0, target.PollingFrequency);

            string protocol = "Test protocol";
            string manufacturer = "test manufacturer";
            string model = "test model";
            string serial = "test serial";
            string firmwareVersion = "test version";
            string firmwareRevision = "test revision";
            string firmwareCrc = "0xtest crc";
            string variant = "test variant";
            string firmwareId = "test id";
            string variantVersion = "test variant version";
            int polling = 1234;

            target = new DeviceConfiguration(
                protocol,
                manufacturer,
                model,
                serial,
                firmwareVersion,
                firmwareId,
                firmwareRevision,
                firmwareCrc,
                variant,
                variantVersion,
                polling);
            Assert.AreEqual(protocol, target.Protocol);
            Assert.AreEqual(manufacturer, target.Manufacturer);
            Assert.AreEqual(model, target.Model);
            Assert.AreEqual(serial, target.SerialNumber);
            Assert.AreEqual(firmwareVersion, target.FirmwareBootVersion);
            Assert.AreEqual(firmwareRevision, target.FirmwareRevision);
            Assert.AreEqual(firmwareCrc, target.FirmwareCyclicRedundancyCheck);
            Assert.AreEqual(variant, target.VariantName);
            Assert.AreEqual(firmwareId, target.FirmwareId);
            Assert.AreEqual(variantVersion, target.VariantVersion);
            Assert.AreEqual(polling, target.PollingFrequency);
        }

        [TestMethod]
        public void DeviceTest()
        {
            var target = new Device();
            Assert.AreEqual(string.Empty, target.Protocol);
            Assert.AreEqual(string.Empty, target.Manufacturer);
            Assert.AreEqual(string.Empty, target.Model);
            Assert.AreEqual(string.Empty, target.SerialNumber);
            Assert.AreEqual(string.Empty, target.FirmwareBootVersion);
            Assert.AreEqual(string.Empty, target.FirmwareRevision);
            Assert.AreEqual(string.Empty, target.FirmwareCyclicRedundancyCheck);
            Assert.AreEqual(string.Empty, target.VariantName);
            Assert.AreEqual(string.Empty, target.FirmwareId);
            Assert.AreEqual(string.Empty, target.VariantVersion);
            Assert.AreEqual(0, target.PollingFrequency);

            string mode = "test mode";
            string portName = "test portName";
            int baudRate = 9600;
            Parity parity = Parity.None;
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Handshake handshake = Handshake.None;
            string protocol = "Test protocol";
            string manufacturer = "test manufacturer";
            string model = "test model";
            string serial = "test serial";
            string firmwareVersion = "test version";
            string firmwareRevision = "test revision";
            string firmwareCrc = "test crc";
            string variant = "test variant";
            string firmwareId = "test id";
            string variantVersion = "test variant version";
            int polling = 1234;

            _ports = MoqServiceManager.CreateAndAddService<ISerialPortsService>(MockBehavior.Strict);
            _ports.Setup(m => m.LogicalToPhysicalName(It.IsAny<string>())).Returns<string>(a => a);

            target = new Device(
                protocol,
                mode,
                portName,
                baudRate,
                parity,
                dataBits,
                stopBits,
                handshake,
                manufacturer,
                model,
                serial,
                firmwareVersion,
                firmwareId,
                firmwareRevision,
                firmwareCrc,
                variant,
                variantVersion,
                polling);
            Assert.AreEqual(protocol, target.Protocol);
            Assert.AreEqual(manufacturer, target.Manufacturer);
            Assert.AreEqual(model, target.Model);
            Assert.AreEqual(serial, target.SerialNumber);
            Assert.AreEqual(firmwareVersion, target.FirmwareBootVersion);
            Assert.AreEqual(firmwareRevision, target.FirmwareRevision);
            Assert.AreEqual(firmwareCrc, target.FirmwareCyclicRedundancyCheck);
            Assert.AreEqual(variant, target.VariantName);
            Assert.AreEqual(firmwareId, target.FirmwareId);
            Assert.AreEqual(variantVersion, target.VariantVersion);
            Assert.AreEqual(polling, target.PollingFrequency);
            Assert.AreEqual(mode, target.Mode);
            Assert.AreEqual(portName, target.PortName);
            Assert.AreEqual(baudRate, target.BaudRate);
            Assert.AreEqual(parity, target.Parity);
            Assert.AreEqual(dataBits, target.DataBits);
            Assert.AreEqual(stopBits, target.StopBits);
            Assert.AreEqual(handshake, target.Handshake);
        }

        [TestMethod]
        public void DeviceImplementationExtensionNodeTest()
        {
            string name = "Test Protocol";
            var target = new DeviceImplementationExtensionNode { ProtocolName = name };

            Assert.AreEqual(name, target.ProtocolName);
        }

        [TestMethod]
        public void DeviceAddinHelperGetDeviceImplementationObjectNoExtensionsTest()
        {
            var target = new DeviceAddinHelper();
            Assert.IsNotNull(target);

            var result = target.GetDeviceImplementationObject("UnknownPath", "Protocol1");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DeviceAddinHelperGetDeviceImplementationObjectWithExtensionsTest()
        {
            var target = new DeviceAddinHelper();
            Assert.IsNotNull(target);

            var result = target.GetDeviceImplementationObject(
                "/AddinHelperUnitTest/DeviceImplementations",
                "Protocol2");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DoesDeviceImplementationExistTestWithNoExtensions()
        {
            var target = new DeviceAddinHelper();
            Assert.IsFalse(
                target.DoesDeviceImplementationExist("UnknownPath", "Protocol1"),
                "An unknown path and protocol was found!");
        }

        [TestMethod]
        public void DoesDeviceImplementationExistTestWithExtensions()
        {
            var target = new DeviceAddinHelper();
            Assert.IsTrue(
                target.DoesDeviceImplementationExist("/AddinHelperUnitTest/DeviceImplementations", "Protocol3"));
        }
    }
}