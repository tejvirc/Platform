namespace Aristocrat.Monaco.Hardware.Tests.SerialTouch
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Hardware.SerialTouch;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TouchPacketBuilderTests
    {
        private const byte TestByte1 = 0x30;
        private const byte TestByte2 = 0x31;
        private const byte TestByte3 = 0x32;
        private const byte TestByte4 = 0x33;

        [TestMethod]
        public void TryTakeFalseAfterInitializationTest()
        {
            var packetBuilder = new TouchPacketBuilder();

            var success = packetBuilder.TryTakePackets(out var actualPackets);

            Assert.IsFalse(success);
            Assert.IsNull(actualPackets);
        }

        [TestMethod]
        public void TryTakeFalseAfterPartialDataTest()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, TestByte1 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            var success = packetBuilder.TryTakePackets(out var actualPackets);

            Assert.IsFalse(success);
            Assert.IsNull(actualPackets);
        }

        [TestMethod]
        public void TryTakeFalseAfterTryTakeTest()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            packetBuilder.TryTakePackets(out _);
            var success = packetBuilder.TryTakePackets(out _);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void TryTakeFalseAfterReset()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            packetBuilder.Reset();
            var success = packetBuilder.TryTakePackets(out _);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void ValidCommandPacketFromFullDataTest()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator };
            var expectedPackets = new[] { originalData };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        public void ValidCommandPacketsFromFullDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator };
            var originalData2 = new[] { M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator };
            var expectedPackets = new[] { originalData1, originalData2 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);
            packetBuilder.Append(originalData2);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
        }

        [TestMethod]
        public void ValidCommandPacketsFromPartialDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.Header, TestByte1  };
            var originalData2 = new[] { M3SerialTouchConstants.Terminator, M3SerialTouchConstants.Header };
            var originalData3 = new[] { TestByte2, M3SerialTouchConstants.Terminator };
            var expectedPackets = new[] {
                new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator },
                new[] { M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator } };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);
            packetBuilder.Append(originalData2);
            packetBuilder.Append(originalData3);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
        }
        
        [TestMethod]
        public void ValidCommandPacketFromBadDataBeforeGoodDataTest()
        {
            var originalData = new[] { TestByte1, M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator };
            var expectedPackets = new[] { new[] { M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator } };
            var expectedExceptionCount = 1;

            var packetBuilder = new TouchPacketBuilder();
            var exceptionCount = 0;

            try
            {
                packetBuilder.Append(originalData);
            }
            catch(AggregateException ex)
            {
                exceptionCount = ex.InnerExceptions.Count;
            }

            packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.AreEqual(expectedExceptionCount, exceptionCount);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        public void ValidTouchPacketFromFullDataTest()
        {
            var originalData = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 };
            var expectedPackets = new[] { originalData };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        public void ValidTouchPacketsFromFullDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 };
            var originalData2 = new[] { M3SerialTouchConstants.SyncBit, TestByte4, TestByte3, TestByte2, TestByte1 };
            var expectedPackets = new[] { originalData1, originalData2 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);
            packetBuilder.Append(originalData2);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
        }

        [TestMethod]
        public void ValidTouchPacketsFromPartialDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2  };
            var originalData2 = new[] { TestByte3, TestByte4, M3SerialTouchConstants.SyncBit };
            var originalData3 = new[] { TestByte4, TestByte3, TestByte2, TestByte1 };
            var expectedPackets = new[] {
                new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 },
                new[] { M3SerialTouchConstants.SyncBit, TestByte4, TestByte3, TestByte2, TestByte1 } };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);
            packetBuilder.Append(originalData2);
            packetBuilder.Append(originalData3);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
        }
        
        [TestMethod]
        public void ValidTouchPacketFromBadDataBeforeGoodDataTest()
        {
            var originalData = new[] { TestByte1, M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 };
            var expectedPackets = new[] { new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 } };
            var expectedExceptionCount = 1;

            var packetBuilder = new TouchPacketBuilder();
            var exceptionCount = 0;

            try
            {
                packetBuilder.Append(originalData);
            }
            catch(AggregateException ex)
            {
                exceptionCount = ex.InnerExceptions.Count;
            }

            packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.AreEqual(expectedExceptionCount, exceptionCount);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        public void ValidTouchCommandTouchPacketsTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 };
            var originalData2 = new[] { M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator };
            var originalData3 = new[] { M3SerialTouchConstants.SyncBit, TestByte4, TestByte3, TestByte2, TestByte1 };
            var expectedPackets = new[] { originalData1, originalData2, originalData3 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);
            packetBuilder.Append(originalData2);
            packetBuilder.Append(originalData3);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
            Assert.IsTrue(expectedPackets[2].SequenceEqual(actualPacketsArray[2]));
        }

        [TestMethod]
        public void ValidTouchCommandTouchPacketsWithBadDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, TestByte4 };
            var originalData2 = new[] { TestByte1, M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator };
            var originalData3 = new[] { M3SerialTouchConstants.SyncBit, TestByte4, TestByte3, TestByte2, TestByte1 };
            var expectedPackets = new[] {
                originalData1,
                new[] { M3SerialTouchConstants.Header, TestByte2, M3SerialTouchConstants.Terminator },
                originalData3 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);

            try
            {
                packetBuilder.Append(originalData2);
            }
            catch (AggregateException)
            {
            }

            packetBuilder.Append(originalData3);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
            Assert.IsTrue(expectedPackets[1].SequenceEqual(actualPacketsArray[1]));
            Assert.IsTrue(expectedPackets[2].SequenceEqual(actualPacketsArray[2]));
        }

        [TestMethod]
        public void ValidCommandPacketAfterBadDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, TestByte3 };
            var originalData2 = new[] { M3SerialTouchConstants.Header, TestByte4, TestByte3, TestByte2, TestByte1, M3SerialTouchConstants.Terminator };
            var expectedPackets = new[] { originalData2 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);

            try
            {
                packetBuilder.Append(originalData2);
            }
            catch (AggregateException)
            {
            }

            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        public void ValidTouchPacketAfterBadDataTest()
        {
            var originalData1 = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3 };
            var originalData2 = new[] { M3SerialTouchConstants.SyncBit, TestByte4, TestByte3, TestByte2, TestByte1 };
            var expectedPackets = new[] { originalData2 };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData1);

            try
            {
                packetBuilder.Append(originalData2);
            }
            catch (AggregateException)
            {
            }

            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void SyncBitInTouchDataTest()
        {
            var originalData = new[] { M3SerialTouchConstants.SyncBit, TestByte1, TestByte2, TestByte3, M3SerialTouchConstants.SyncBit };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void SyncBitInCommandResponseTest()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, M3SerialTouchConstants.SyncBit, M3SerialTouchConstants.Terminator };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void HeaderInCommandResponseTest()
        {
            var originalData = new[] { M3SerialTouchConstants.Header, M3SerialTouchConstants.Header, M3SerialTouchConstants.Terminator };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
        }

        [TestMethod]
        public void HeaderInTouchDataTest()
        {
            var originalData = new[] { M3SerialTouchConstants.SyncBit, M3SerialTouchConstants.Header, M3SerialTouchConstants.Terminator, TestByte3, TestByte4 };
            var expectedPackets = new[] { originalData };

            var packetBuilder = new TouchPacketBuilder();
            packetBuilder.Append(originalData);
            var success = packetBuilder.TryTakePackets(out var actualPackets);
            var actualPacketsArray = actualPackets.ToArray();

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPackets.Length, actualPacketsArray.Length);
            Assert.IsTrue(expectedPackets[0].SequenceEqual(actualPacketsArray[0]));
        }
    }
}
