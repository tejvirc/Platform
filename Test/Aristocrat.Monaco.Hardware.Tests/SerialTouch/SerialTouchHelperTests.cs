namespace Aristocrat.Monaco.Hardware.Tests.SerialTouch
{
    using System.Linq;
    using Aristocrat.Monaco.Hardware.SerialTouch;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SerialTouchHelperTests
    {
        private const byte TouchDownByte = 0xD8;
        private const byte TouchUpByte = 0x98;
        private const byte TestByte1 = 0x30;
        private const byte TestByte2 = 0x31;
        private const byte TestByte3 = 0x32;
        private const byte TestByte4 = 0x33;
        private const byte TestByte5 = 0x34;
        private const byte Zero = 0x00;
        private const byte TouchByteSet = 0x7F;
        private const short TouchAllSet = 16383;
        private const short TouchLowSet = 127;
        private const short TouchHighSet = 16256;

        [TestMethod]
        [DataRow(new[] { TouchDownByte }, Zero, Zero)]
        [DataRow(new[] { TouchDownByte, Zero, Zero, Zero, Zero }, Zero, Zero)]
        [DataRow(new[] { TouchDownByte, Zero, Zero, Zero, TouchByteSet }, Zero, TouchHighSet)]
        [DataRow(new[] { TouchDownByte, Zero, Zero, TouchByteSet, Zero }, Zero, TouchLowSet)]
        [DataRow(new[] { TouchDownByte, Zero, Zero, TouchByteSet, TouchByteSet }, Zero, TouchAllSet)]
        [DataRow(new[] { TouchDownByte, Zero, TouchByteSet, Zero, Zero }, TouchHighSet, Zero)]
        [DataRow(new[] { TouchDownByte, TouchByteSet, Zero, Zero, Zero }, TouchLowSet, Zero)]
        [DataRow(new[] { TouchDownByte, TouchByteSet, TouchByteSet, Zero, Zero }, TouchAllSet, Zero)]
        public void GetRawFormatTabletPointTest(byte[] touchPacket, short expectedX, short expectedY)
        {
            var output = SerialTouchHelper.GetRawFormatTabletPoint(touchPacket);

            Assert.AreEqual(expectedX, output.x);
            Assert.AreEqual(expectedY, output.y);
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow(TouchDownByte, true)]
        [DataRow(TouchUpByte, true)]
        [DataRow(M3SerialTouchConstants.Header, false)]
        [DataRow(M3SerialTouchConstants.Terminator, false)]
        public void IsSyncBitSetTest(byte data, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, SerialTouchHelper.IsSyncBitSet(data));
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 }, true)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 }, true)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        public void IsValidTouchDataPacketTest(byte[] data, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, SerialTouchHelper.IsValidTouchDataPacket(data));
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 }, true)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        public void IsDownTouchDataPacketTest(byte[] data, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, SerialTouchHelper.IsDownTouchDataPacket(data));
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, false)]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 }, true)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4, TestByte5 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        public void IsUpTouchDataPacketTest(byte[] data, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, SerialTouchHelper.IsUpTouchDataPacket(data));
        }

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow(new[] { TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2 }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2 }, false)]
        [DataRow(new[] { TestByte1, TestByte2, M3SerialTouchConstants.Header }, false)]
        [DataRow(new[] { TestByte1, TestByte2, M3SerialTouchConstants.Terminator }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, M3SerialTouchConstants.Header }, false)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator }, true)]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, M3SerialTouchConstants.Terminator }, true)]
        public void IsValidCommandResponsePacketTest(byte[] data, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, SerialTouchHelper.IsValidCommandResponsePacket(data));
        }

        [DataTestMethod]
        [DataRow(null, null)]
        [DataRow(new[] { TestByte1, TestByte2, TestByte3, TestByte4 }, new[] { TestByte1, TestByte2, TestByte3, TestByte4 })]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 }, new[] { TouchDownByte, TestByte1, TestByte2, TestByte3, TestByte4 })]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 }, new[] { TouchUpByte, TestByte1, TestByte2, TestByte3, TestByte4 })]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2 }, new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2 })]
        [DataRow(new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2 }, new[] { M3SerialTouchConstants.Terminator, TestByte1, TestByte2 })]
        [DataRow(new[] { TestByte1, TestByte2, M3SerialTouchConstants.Header }, new[] { TestByte1, TestByte2, M3SerialTouchConstants.Header })]
        [DataRow(new[] { TestByte1, TestByte2, M3SerialTouchConstants.Terminator }, new[] { TestByte1, TestByte2, M3SerialTouchConstants.Terminator })]
        [DataRow(new[] { M3SerialTouchConstants.Header, M3SerialTouchConstants.Header }, new[] { M3SerialTouchConstants.Header, M3SerialTouchConstants.Header })]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, M3SerialTouchConstants.Terminator }, new[] { TestByte1 })]
        [DataRow(new[] { M3SerialTouchConstants.Header, TestByte1, TestByte2, M3SerialTouchConstants.Terminator }, new[] { TestByte1, TestByte2 })]
        public void StripHeaderAndTerminatorTest(byte[] data, byte[] expectedData)
        {
            var actualData = SerialTouchHelper.StripHeaderAndTerminator(data);

            if (expectedData is null)
            {
                Assert.IsNull(actualData);
                return;
            }

            Assert.IsTrue(actualData.SequenceEqual(expectedData));
        }
        
        [DataTestMethod]
        [DataRow(null, null)]
        [DataRow(new[] { TouchDownByte, TestByte1, TestByte2, TestByte3 }, new[] { TouchUpByte, TestByte1, TestByte2, TestByte3  })]
        [DataRow(new[] { TouchUpByte, TestByte1, TestByte2, TestByte3 }, new[] { TouchDownByte, TestByte1, TestByte2, TestByte3  })]
        public void ToggleProximityBitTest(byte[] data, byte[] expectedData)
        {
            var actualData = SerialTouchHelper.ToggleProximityBit(data);

            if (expectedData is null)
            {
                Assert.IsNull(actualData);
                return;
            }
            
            Assert.IsTrue(actualData.SequenceEqual(expectedData));
        }
    }
}
