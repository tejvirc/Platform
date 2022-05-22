namespace Aristocrat.Monaco.Asp.Tests.Client.Comms
{
    using System;
    using System.Threading;
    using Asp.Client.Comms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    [Ignore("Ignored, needs to be reviewed. Test are failing intermittently.")]
    public class DataLinkLayerTests : AspUnitTestBase<TestDataContext>
    {
        [TestMethod]
        public void DataLinkLayerTest()
        {
            var commPortMock = MockRepository.Create<ICommPort>();
            Assert.IsNotNull(new DataLinkLayer(commPortMock.Object));
        }

        [TestMethod]
        public void DataLinkLayerNullTest()
        {
            var dataLinkLayer = new DataLinkLayer(null);
            Assert.IsFalse(dataLinkLayer.IsRunning);
            Assert.IsFalse(dataLinkLayer.Start("Test"));
            Assert.IsFalse(dataLinkLayer.IsRunning);
        }

        [TestMethod]
        public void DataLinkPacketTest()
        {
            var dataLinkPacket = new DataLinkPacket
            {
                DataLength = 8,
                Position = 12,
                Crc = 65535
            };

            Assert.AreEqual(1, dataLinkPacket.Mac);
            Assert.AreEqual(0, dataLinkPacket.Sequence);
            Assert.AreEqual(12, dataLinkPacket.Length);
            Assert.AreEqual((ushort)65535, dataLinkPacket.Crc);
            Assert.IsTrue(dataLinkPacket.Complete);
            Assert.AreEqual(0, dataLinkPacket.NumberOfBytesToRead);
        }

        [TestMethod]
        public void StartTest()
        {
            using (var dataLinkLayer = CreateDataLinkLayer(CreateCommPortMock()))
            {
                var localDataLinkLayer = dataLinkLayer;
                Assert.IsFalse(dataLinkLayer.IsLinkUp);
                Assert.IsFalse(dataLinkLayer.IsRunning);
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(dataLinkLayer.IsRunning);
                Assert.IsFalse(WaitFor(200, () => localDataLinkLayer.IsLinkUp));
                dataLinkLayer.Stop();
                Assert.IsFalse(dataLinkLayer.IsLinkUp);
                Assert.IsFalse(dataLinkLayer.IsRunning);
            }
        }

        [TestMethod]
        public void StopBlockedTest()
        {
            var commPortMock = CreateCommPortMock();
            using (var dataLinkLayer = CreateDataLinkLayer(commPortMock))
            {
                var readCalled = false;
                commPortMock.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<uint>())).Callback(
                    () =>
                    {
                        readCalled = true;
                        using (var resetEvent = new AutoResetEvent(false))
                        {
                            resetEvent.WaitOne();
                        }
                    });
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(dataLinkLayer.IsRunning);
                Assert.IsTrue(WaitFor(500, () => readCalled));
                dataLinkLayer.Stop();
                Assert.IsFalse(dataLinkLayer.IsRunning);
            }
        }

        [TestMethod]
        public void StopSameThreadTest()
        {
            var commPortMock = CreateCommPortMock();
            using (var dataLinkLayer = CreateDataLinkLayer(commPortMock))
            {
                var dl = dataLinkLayer;
                commPortMock.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<uint>())).Returns(
                    () =>
                    {
                        dl?.Stop();
                        return 0;
                    });
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(WaitFor(200, () => !dl.IsRunning));
            }
        }

        [TestMethod]
        public void LinkUpTest()
        {
            var mock = new CommPortMock();

            using (var dataLinkLayer = new DataLinkLayer(mock))
            {
                var localDataLinkLayer = dataLinkLayer;
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(WaitFor(100, () => localDataLinkLayer.IsLinkUp));

                mock.GetDataLinkDataFunc = null;
                Assert.IsTrue(WaitFor(100, () => localDataLinkLayer.IsLinkUp));
            }
        }

        [TestMethod]
        public void LinkUpInvalidMacTest()
        {
            var mock = new CommPortMock { Mac = 0x50 };
            using (var dataLinkLayer = new DataLinkLayer(mock))
            {
                var localDataLinkLayer = dataLinkLayer;
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(WaitFor(500, () => !localDataLinkLayer.IsLinkUp));
            }
        }

        [TestMethod]
        public void LinkUpInvalidSeqTest()
        {
            var mock = new CommPortMock { Sequence = 0x1, UpdateSequence = false };
            using (var dataLinkLayer = new DataLinkLayer(mock))
            {
                var localDataLinkLayer = dataLinkLayer;
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsFalse(WaitFor(500, () => localDataLinkLayer.IsLinkUp));
            }
        }

        [TestMethod]
        public void LinkDownTest()
        {
            var mock = new CommPortMock();

            using (var dataLinkLayer = new DataLinkLayer(mock))
            {
                var localDataLinkLayer = dataLinkLayer;
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(WaitFor(100, () => localDataLinkLayer.IsLinkUp));

                mock.Mac = 0x30;
                var timeoutPeriod = GetWaitTimeFor(1200, () => !localDataLinkLayer.IsLinkUp);
                Assert.IsTrue(timeoutPeriod > 0 && timeoutPeriod < 1100 && timeoutPeriod > 800);
            }
        }

        [TestMethod]
        public void TimeOutExceptionTest()
        {
            var callCount = 0;
            var mock = new CommPortMock
            {
                GetDataLinkDataFunc = () =>
                {
                    callCount++;
                    throw new TimeoutException();
                }
            };
            using (var dataLinkLayer = new DataLinkLayer(mock))
            {
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Thread.Sleep(300);
                Assert.IsTrue(callCount > 0);
                Assert.IsFalse(dataLinkLayer.IsLinkUp);
            }
        }

        [TestMethod]
        public void DisposeTest()
        {
            DataLinkLayer dataLinkLayer;
            using (dataLinkLayer = CreateDataLinkLayer(CreateCommPortMock()))
            {
                Assert.IsFalse(dataLinkLayer.IsRunning);
                Assert.IsTrue(dataLinkLayer.Start("Test"));
                Assert.IsTrue(dataLinkLayer.IsRunning);
                Thread.Sleep(10);
            }

            Assert.IsFalse(dataLinkLayer.IsRunning);
        }

        [TestMethod]
        public void DisposeTwiceTest()
        {
            var commPortMock = new Mock<ICommPort>();
            commPortMock.Setup(x => x.Dispose());

            var dataLinkLayer = CreateDataLinkLayer(commPortMock);

            dataLinkLayer.Dispose();
            dataLinkLayer.Dispose();

            commPortMock.Verify(m => m.Dispose(), Times.Once);
        }

        private static DataLinkLayer CreateDataLinkLayer(IMock<ICommPort> comMock)
        {
            return new DataLinkLayer(comMock.Object);
        }

        private Mock<ICommPort> CreateCommPortMock()
        {
            var commPortMock = MockRepository.Create<ICommPort>();
            commPortMock.Setup(x => x.Close());
            commPortMock.SetupSet<string>(x => x.PortName = "Test").Verifiable();
            commPortMock.SetupGet(x => x.IsOpen).Returns(true);
            commPortMock.Setup(x => x.Open());
            commPortMock.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<uint>())).Returns(0);
            commPortMock.Setup(x => x.Dispose());
            return commPortMock;
        }
    }
}