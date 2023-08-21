namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Device;
    using Hardware.EdgeLight.Device.Packets;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Hardware.HidLibrary;

    [TestClass]
    public class EdgeLightDeviceTests
    {
        private readonly byte[] _ledConfigData =
        {
            0x61, 0x90, 0x00, 0x90, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private EdgeLightDevice _device;
        private Mock<IHidDevice> _hidDeviceMoc;

        [TestInitialize]
        public void Setup()
        {
            _hidDeviceMoc = new Mock<IHidDevice>(MockBehavior.Strict);
            _hidDeviceMoc.Setup(x => x.CloseDevice());
            _hidDeviceMoc.Setup(x => x.Dispose());
            _device = new EdgeLightDevice(_hidDeviceMoc.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _device.Dispose();
        }

        [TestMethod]
        public void DevicesInfoTest()
        {
            OpenDevice();
            var manufacturer = Encoding.Unicode.GetBytes("TestManufacturer");
            var product = Encoding.Unicode.GetBytes("TestProduct");
            var serial = Encoding.Unicode.GetBytes("TestSerial");
            _hidDeviceMoc.Setup(x => x.ReadManufacturer(out manufacturer)).Returns(true);
            _hidDeviceMoc.Setup(x => x.ReadProduct(out product)).Returns(true);
            _hidDeviceMoc.Setup(x => x.ReadSerialNumber(out serial)).Returns(true);
            var devInfo = _device.DevicesInfo.First();
            Assert.AreEqual("TestManufacturer", devInfo.Manufacturer);
            Assert.AreEqual("TestProduct", devInfo.Product);
            Assert.AreEqual("TestSerial", devInfo.SerialNumber);
        }

        [TestMethod]
        public void LowPowerModeTest()
        {
            OpenDevice();
            var request = (LowPowerMode)CommandFactory.CreateRequest(RequestType.SetLowPowerMode);
            request.Control = true;
            _hidDeviceMoc.Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback<byte[], int>(
                (x, y) =>
                {
                    Assert.IsTrue(request.Data.SequenceEqual(x));
                    Assert.AreEqual(100, y);
                }).Returns(true);
            _device.LowPowerMode = true;
            request.Control = false;
            _device.LowPowerMode = false;
        }

        [TestMethod]
        public void StripBrightnessTest()
        {
            OpenDevice();
            var setBrightness = false;
            var values = new byte[] { 100, 100, 100, 100 };
            _hidDeviceMoc.Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback<byte[], int>(
                (x, y) =>
                {
                    setBrightness = x[0] == (byte)RequestType.SetDeviceLedBrightness;
                    if (setBrightness)
                    {
                        Assert.IsTrue(x.Skip(1).Take(values.Length).SequenceEqual(values));
                    }

                    Assert.AreEqual(100, y);
                }).Returns(true);
            _device.SetSystemBrightness(100);
            _device.RenderAllStripData();
            Assert.IsTrue(setBrightness);

            setBrightness = false;
            _device.SetSystemBrightness(200);
            _device.RenderAllStripData();
            Assert.IsTrue(setBrightness);

            setBrightness = false;
            values = Array.ConvertAll(values, x => x = 10);
            _device.SetSystemBrightness(10);
            _device.RenderAllStripData();
            Assert.IsTrue(setBrightness);

            setBrightness = false;
            values = Array.ConvertAll(values, x => x = 0);
            _device.SetSystemBrightness(-100);
            _device.RenderAllStripData();
            Assert.IsTrue(setBrightness);

            setBrightness = false;
            _device.RenderAllStripData();
            Assert.IsFalse(setBrightness);
        }

        [TestMethod]
        public void EdgeLightDeviceTest()
        {
            Assert.AreEqual(BoardIds.InvalidBoardId, _device.BoardId);
            Assert.IsFalse(_device.IsOpen);
            Assert.AreEqual(0, _device.PhysicalStrips.Count);
        }

        [TestMethod]
        public void RenderAllStripDataTest()
        {
            OpenDevice();
            _hidDeviceMoc.Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Returns(true);
            _device.RenderAllStripData();
            _hidDeviceMoc.Verify(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>()), Times.AtLeast(4));
        }

        [TestMethod]
        public void OpenTest()
        {
            foreach (var readStatus in Enum.GetValues(typeof(HidDeviceData.ReadStatus)))
            {
                Assert.IsFalse(OpenDevice(false, false, (HidDeviceData.ReadStatus)readStatus, false));
                Assert.IsFalse(OpenDevice(false, false, (HidDeviceData.ReadStatus)readStatus, true));
                Assert.IsFalse(OpenDevice(false, true, (HidDeviceData.ReadStatus)readStatus, false));
                Assert.IsFalse(OpenDevice(false, true, (HidDeviceData.ReadStatus)readStatus, true));

                Assert.IsFalse(OpenDevice(true, false, (HidDeviceData.ReadStatus)readStatus, false));
                Assert.IsFalse(OpenDevice(true, false, (HidDeviceData.ReadStatus)readStatus, true));
                Assert.IsFalse(OpenDevice(true, true, (HidDeviceData.ReadStatus)readStatus, false));
                Assert.AreEqual(
                    (HidDeviceData.ReadStatus)readStatus == HidDeviceData.ReadStatus.Success,
                    OpenDevice(true, true, (HidDeviceData.ReadStatus)readStatus, true));
            }
        }

        [TestMethod]
        public void CloseTest()
        {
            _device.Close();
            OpenDevice();
            _device.Close();
            _hidDeviceMoc.Verify(x => x.CloseDevice(), Times.AtLeast(2));
        }

        [TestMethod]
        public void DisposeTest()
        {
            _device.Dispose();
            _device.Dispose();
            _hidDeviceMoc.Verify(x => x.CloseDevice(), Times.AtMostOnce);
        }

        [TestMethod]
        public void CheckForConnectionTest()
        {
            foreach (var readStatus in Enum.GetValues(typeof(HidDeviceData.ReadStatus)))
            {
                SetupOpenDevice(false, false, (HidDeviceData.ReadStatus)readStatus, false);
                Assert.IsFalse(_device.CheckForConnection());
                SetupOpenDevice(false, false, (HidDeviceData.ReadStatus)readStatus, true);
                Assert.IsFalse(_device.CheckForConnection());
                SetupOpenDevice(false, true, (HidDeviceData.ReadStatus)readStatus, false);
                Assert.IsFalse(_device.CheckForConnection());
                SetupOpenDevice(false, true, (HidDeviceData.ReadStatus)readStatus, true);
                Assert.IsFalse(_device.CheckForConnection());

                SetupOpenDevice(true, false, (HidDeviceData.ReadStatus)readStatus, false);
                Assert.IsFalse(_device.CheckForConnection());
                SetupOpenDevice(true, false, (HidDeviceData.ReadStatus)readStatus, true);
                Assert.IsFalse(_device.CheckForConnection());
                SetupOpenDevice(true, true, (HidDeviceData.ReadStatus)readStatus, false);
                Assert.IsFalse(_device.CheckForConnection());

                SetupOpenDevice(true, true, (HidDeviceData.ReadStatus)readStatus, true);
                var isOpen = false;
                _hidDeviceMoc.SetupGet(x => x.IsOpen).Returns(
                    () =>
                    {
                        if (isOpen)
                        {
                            return true;
                        }

                        isOpen = true;
                        return false;
                    });
                Assert.AreEqual(
                    (HidDeviceData.ReadStatus)readStatus == HidDeviceData.ReadStatus.Success,
                    _device.CheckForConnection());
            }
        }

        [TestMethod]
        public void NewStripsDiscoveredTest()
        {
            OpenDevice();
            var stripMocks = SetupStripMocks(StripIDs.BarkeeperStrip1Led);
            var callback = new WriteCallback();
            _hidDeviceMoc.Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback<byte[], int>(
                callback.CallBack
            ).Returns(true);
            callback.Strips.Add((byte)StripIDs.BarkeeperStrip1Led);
            _device.NewStripsDiscovered(stripMocks, false);
            callback.Verify(true, 1);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);

            stripMocks.First().Brightness = 80;
            callback.Verify(true, 1);

            var newStrips = SetupStripMocks(StripIDs.BarkeeperStrip4Led);
            stripMocks.AddRange(newStrips);
            callback.Strips = stripMocks.Select(x => x.StripId).ToList();
            _device.NewStripsDiscovered(newStrips, false);

            callback.Verify(true, 1);

            stripMocks.ForEach(x => x.Brightness = 80);
            callback.Verify(true, stripMocks.Count);

            newStrips = SetupStripMocks(StripIDs.BarkeeperStrip4Led);
            stripMocks.AddRange(newStrips);
            callback.Strips = newStrips.Select(x => x.StripId).ToList();

            _device.NewStripsDiscovered(newStrips, true);

            callback.Verify(true, 1);

            stripMocks.ForEach(x => x.Brightness = 80);
            callback.Verify(true, 1);

            List<IStrip> SetupStripMocks(StripIDs stripId)
            {
                var stripMock = new Mock<IStrip>(MockBehavior.Strict);
                var stripMockList = new List<IStrip> { stripMock.Object };
                stripMock.SetupGet(x => x.StripId).Returns((int)stripId);
                stripMock.SetupGet(x => x.Brightness).Returns(80);
                stripMock.SetupSet(x => x.Brightness = 80).Raises(x => x.BrightnessChanged += null, EventArgs.Empty);
                return stripMockList;
            }
        }

        private void OpenDevice()
        {
            Assert.IsTrue(OpenDevice(true, true, HidDeviceData.ReadStatus.Success, true));
        }

        private bool OpenDevice(bool isConnected, bool isOpen, HidDeviceData.ReadStatus readStatus, bool writeStatus)
        {
            SetupOpenDevice(isConnected, isOpen, readStatus, writeStatus);
            _device.CheckForConnection();
            return _device.IsOpen;
        }

        private void SetupOpenDevice(
            bool isConnected,
            bool isOpen,
            HidDeviceData.ReadStatus readStatus,
            bool writeStatus)
        {
            _hidDeviceMoc.Setup(
                x => x.OpenDevice(
                    DeviceMode.Overlapped,
                    DeviceMode.Overlapped,
                    ShareMode.ShareRead | ShareMode.ShareWrite));
            _hidDeviceMoc.SetupGet(x => x.Capabilities).Returns(
                new HidDeviceCapabilities(new NativeMethods.HIDP_CAPS { OutputReportByteLength = 64 }));
            _hidDeviceMoc.SetupGet(x => x.IsConnected).Returns(isConnected);
            var isOpenFirstCall = true;
            _hidDeviceMoc.SetupGet(x => x.IsOpen).Returns(
                () =>
                {
                    var ret = !isOpenFirstCall && isOpen;
                    isOpenFirstCall = false;
                    return ret;
                });
            _hidDeviceMoc.SetupGet(x => x.DevicePath).Returns("Test");
            var request = CommandFactory.CreateRequest(RequestType.GetLedConfiguration);
            _hidDeviceMoc.Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>())).Callback<byte[], int>(
                (x, y) =>
                {
                    Assert.IsTrue(request.Data.SequenceEqual(x));
                    Assert.AreEqual(100, y);
                }).Returns(writeStatus);
            _hidDeviceMoc.Setup(x => x.Read(250))
                .Returns(new HidDeviceData(_ledConfigData, readStatus));
        }

        private class WriteCallback
        {
            private int _countOfSetBrightness;
            private bool _setBrightness;
            public List<int> Strips { get; set; } = new List<int>();

            private void Reset()
            {
                _countOfSetBrightness = 0;
                _setBrightness = false;
            }

            public void CallBack(byte[] data, int timeout)
            {
                _setBrightness = data[0] == (byte)RequestType.SetBarkeeperLedBrightness;
                if (!_setBrightness)
                {
                    return;

                }

                Assert.IsTrue(Strips.Contains(data[1]));
                _countOfSetBrightness++;
            }

            public void Verify(bool setBrightness, int expectedCount)
            {
                Assert.AreEqual(setBrightness, _setBrightness);
                Assert.AreEqual(expectedCount, _countOfSetBrightness);
                Reset();
            }
        }
    }
}