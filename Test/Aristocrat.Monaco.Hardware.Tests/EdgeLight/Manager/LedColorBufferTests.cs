namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Castle.Core.Internal;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LedColorBufferTests
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void WhenLedColorBufferEmpty()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            Assert.IsTrue(tempLedColorBuffer.ArgbBytes.Length == 0);
            Assert.IsTrue(tempLedColorBuffer.Count == 0);
            Assert.IsTrue(tempLedColorBuffer.ArgbBytes.IsNullOrEmpty());
            Assert.IsTrue(tempLedColorBuffer.RgbBytes.Length == 0);
            Assert.IsTrue(tempLedColorBuffer.RgbBytes.IsNullOrEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void WhenIndexOutOfRange()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            var _ = tempLedColorBuffer[1];
        }

        [TestMethod]
        public void WhenCorrectColorIsReturnedForIndex()
        {
            var tempLedColorBuffer = new LedColorBuffer(new List<byte> { 255, 255, 255, 255 });
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.White.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenConstructionIsWrong()
        {
            var _ = new LedColorBuffer(new List<byte> { 255, 255, 255 });
        }

        [TestMethod]
        public void WhenGetSegmentIsCorrect()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var ledColorSegment = tempLedColorBuffer.GetSegment(1, 3);
            Assert.AreEqual(ledColorSegment.Count, 3);
            var tempColor = ledColorSegment[0];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
            tempColor = ledColorSegment[1];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            tempColor = ledColorSegment[2];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            ledColorSegment = tempLedColorBuffer.GetSegment(1, 3, true);
            Assert.AreEqual(ledColorSegment.Count, 3);
            tempColor = ledColorSegment[0];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            tempColor = ledColorSegment[1];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            tempColor = ledColorSegment[2];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsThrowsException_1()
        {
            var tempLedColorBuffer1 = new LedColorBuffer();
            tempLedColorBuffer1.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var tempLedColorBuffer2 = new LedColorBuffer(new List<Color> { Color.White, Color.Black });
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, -1, 3, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsThrowsException_2()
        {
            var tempLedColorBuffer1 = new LedColorBuffer();
            tempLedColorBuffer1.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var tempLedColorBuffer2 = new LedColorBuffer(new List<Color> { Color.White, Color.Black });
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, 0, 3, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsThrowsException_3()
        {
            var tempLedColorBuffer1 = new LedColorBuffer();
            tempLedColorBuffer1.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var tempLedColorBuffer2 = new LedColorBuffer(new List<Color> { Color.White, Color.Black });
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, 0, -1, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsThrowsException_4()
        {
            var tempLedColorBuffer1 = new LedColorBuffer();
            tempLedColorBuffer1.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var tempLedColorBuffer2 = new LedColorBuffer(new List<Color> { Color.White, Color.Black });
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, 0, 2, 10);
        }

        [TestMethod]
        public void WhenSetColorsIsCorrect()
        {
            var tempLedColorBuffer1 = new LedColorBuffer();
            tempLedColorBuffer1.Append(
                new LedColorBuffer(
                    new List<Color>
                    {
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green,
                        Color.Red,
                        Color.Blue,
                        Color.Green
                    }));
            var tempLedColorBuffer2 = new LedColorBuffer(new List<Color> { Color.White, Color.Black });
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, 0, 2, 0);
            var tempColor = tempLedColorBuffer1[0];
            Assert.IsTrue(Color.White.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer1[1];
            Assert.IsTrue(Color.Black.ToArgb() == tempColor.ToArgb());
            tempLedColorBuffer1.SetColors(tempLedColorBuffer2, 0, 2, 4);
            tempColor = tempLedColorBuffer1[0];
            Assert.IsTrue(Color.White.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer1[1];
            Assert.IsTrue(Color.Black.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer1[4];
            Assert.IsTrue(Color.White.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer1[5];
            Assert.IsTrue(Color.Black.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        public void WhenAppendTestIsCorrect()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.Append(new LedColorBuffer(new List<Color> { Color.Red, Color.Blue, Color.Green }));
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer[1];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer[2];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        public void WhenSetColorIsCorrect_1()
        {
            var tempLedColorBuffer = new LedColorBuffer(10);
            tempLedColorBuffer.SetColor(0, Color.Blue, 10);
            foreach (var color in tempLedColorBuffer)
            {
                Assert.IsTrue(Color.Blue.ToArgb() == color.ToArgb());
            }
        }

        [TestMethod]
        public void WhenSetColorIsCorrect_2()
        {
            var tempLedColorBuffer = new LedColorBuffer(
                new List<Color>
                {
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue
                });
            tempLedColorBuffer.SetColor(2, Color.Red, 1);
            var tempColor = tempLedColorBuffer[2];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            tempLedColorBuffer.SetColor(4, Color.Red, 2);
            tempColor = tempLedColorBuffer[4];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer[5];
            Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
            tempColor = tempLedColorBuffer[6];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorThrowsException_1()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.SetColor(5, Color.Blue, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorThrowsException_2()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.SetColor(-1, Color.Blue, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorThrowsException_3()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.SetColor(0, Color.Blue, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorThrowsException_4()
        {
            var tempLedColorBuffer = new LedColorBuffer(
                new List<Color>
                {
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue,
                    Color.Blue
                });
            tempLedColorBuffer.SetColor(1, Color.Red, 10);
        }

        [TestMethod]
        public void WhenSetBgraDataIsCorrect()
        {
            var tempLedColorBuffer = new LedColorBuffer(1);
            tempLedColorBuffer.SetBgraData(new byte[] { 0, 128, 0, 255 });
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            Assert.IsTrue(tempLedColorBuffer.ArgbBytes.SequenceEqual(new byte[] { 255, 0, 128, 0 }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetBgraDataThrowsException()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.SetBgraData(new byte[1]);
        }

        [TestMethod]
        public void WhenDrawIsCorrect_1()
        {
            var tempLedColorBuffer = new LedColorBuffer(1);
            tempLedColorBuffer.SetBgraData(new byte[] { 0, 128, 0, 255 });
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            tempLedColorBuffer.Draw(new LedColorBuffer(new[] { Color.Blue }));
            tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        public void WhenDrawIsCorrect_2()
        {
            var tempLedColorBuffer = new LedColorBuffer(
                new LedColorBuffer(
                    new[]
                    {
                        Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.Blue,
                        Color.Blue, Color.Blue
                    }));
            tempLedColorBuffer.Draw(
                new LedColorBuffer(
                    new[]
                    {
                        Color.Red, Color.FromArgb(0, 255, 255, 255), Color.Red, Color.FromArgb(0, 255, 255, 255),
                        Color.Red, Color.FromArgb(0, 255, 255, 255), Color.Red, Color.FromArgb(0, 255, 255, 255),
                        Color.Red
                    }));
            for (var i = 0; i < tempLedColorBuffer.Count; ++i)
            {
                var tempColor = tempLedColorBuffer[i];
                if (i % 2 == 0)
                {
                    Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
                }
                else
                {
                    Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
                }
            }

            tempLedColorBuffer.Draw(
                new LedColorBuffer(
                    new[]
                    {
                        Color.FromArgb(0, 255, 255, 255), Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(0, 255, 255, 255), Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(0, 255, 255, 255), Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(0, 255, 255, 255), Color.FromArgb(255, 0, 255, 0),
                        Color.FromArgb(0, 255, 255, 255)
                    }));

            for (var i = 0; i < tempLedColorBuffer.Count; ++i)
            {
                var tempColor = tempLedColorBuffer[i];
                if (i % 2 == 0)
                {
                    Assert.IsTrue(Color.Red.ToArgb() == tempColor.ToArgb());
                }
                else
                {
                    Assert.IsTrue(Color.Lime.ToArgb() == tempColor.ToArgb());
                }
            }
        }

        [TestMethod]
        public void WhenDrawIsCorrect_3()
        {
            var tempLedColorBuffer = new LedColorBuffer(1);
            tempLedColorBuffer.SetBgraData(new byte[] { 0, 128, 0, 255 });
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            tempLedColorBuffer.Draw(new LedColorBuffer(new[] { Color.Blue }));
            tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        public void WhenDrawIsCorrect_4()
        {
            var tempLedColorBuffer = new LedColorBuffer(1);
            tempLedColorBuffer.SetBgraData(new byte[] { 0, 128, 0, 255 });
            var tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Green.ToArgb() == tempColor.ToArgb());
            tempLedColorBuffer.Draw(new LedColorBuffer(new[] { Color.Blue }));
            tempColor = tempLedColorBuffer[0];
            Assert.IsTrue(Color.Blue.ToArgb() == tempColor.ToArgb());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenDrawThrowsException()
        {
            var tempLedColorBuffer = new LedColorBuffer();
            tempLedColorBuffer.Draw(new LedColorBuffer(new[] { Color.Blue, Color.Blue }));
        }

        [DataRow(true, 0, 1, 0, 1, DisplayName = "Null source data")]
        [DataRow(false, 0, 2, 0, 20, DisplayName = "Destination led count is greater than that of present")]
        [DataRow(false, 0, 10, 0, 2, DisplayName = "Source led count is greater than present")]
        [DataRow(false, 0, 2, 0, 0, DisplayName = "Zero destination led count")]
        [DataRow(false, 0, 2, 20, 2, DisplayName = "Destination Index out of bound")]
        [DataRow(false, 2, 2, 0, 2, DisplayName = "Source index out of bound")]
        [DataRow(false, -1, 3, 0, 3, DisplayName = "Negative source start index")]
        [DataRow(false, 0, 3, -1, 4, DisplayName = "Negative destination index")]
        [ExpectedException(typeof(ArgumentException))]
        [DataTestMethod]
        public void InvalidArgumentExceptionTests(
            bool nullData,
            int sourceLedStart,
            int sourceLedCount,
            int destinationLedStart,
            int destinationLedCount)
        {
            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var tempLedColorBuffer = new LedColorBuffer(10);

            tempLedColorBuffer.DrawScaled(
                nullData ? null : new LedColorBuffer(sourceData),
                sourceLedStart,
                sourceLedCount,
                destinationLedStart,
                destinationLedCount);
        }

        [TestMethod]
        public void DrawScaledTest()
        {
            var tempLedColorBuffer = new LedColorBuffer(2);
            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            tempLedColorBuffer.DrawScaled(
                new LedColorBuffer(sourceData),
                0,
                sourceData.Length / LedColorBuffer.BytesPerLed,
                0,
                tempLedColorBuffer.Count);
            Assert.IsTrue(tempLedColorBuffer.ArgbBytes.SequenceEqual(sourceData));
        }

        [TestMethod]
        public void DrawScaledWithNonZeroOffsetTest()
        {
            var tempLedColorBuffer = new LedColorBuffer(10);
            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            tempLedColorBuffer.DrawScaled(
                new LedColorBuffer(sourceData),
                0,
                sourceData.Length / LedColorBuffer.BytesPerLed,
                8,
                2);
            Assert.IsTrue(tempLedColorBuffer.GetSegment(8, 2).ArgbBytes.SequenceEqual(sourceData));
        }

        [TestMethod]
        public void UpScaleLedColorBufferTest()
        {
            var tempLedColorBuffer = new LedColorBuffer(10);
            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            tempLedColorBuffer.DrawScaled(
                new LedColorBuffer(sourceData),
                0,
                sourceData.Length / LedColorBuffer.BytesPerLed,
                0,
                tempLedColorBuffer.Count);
            Assert.AreEqual(tempLedColorBuffer.Count, 10);
            Assert.IsTrue(
                tempLedColorBuffer.GetSegment(0, 1).ArgbBytes
                    .SequenceEqual(sourceData.Take(1 * LedColorBuffer.BytesPerLed)));
        }

        [TestMethod]
        public void DownScaleLedColorBufferTest()
        {
            var tempLedColorBuffer = new LedColorBuffer(1);
            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            tempLedColorBuffer.DrawScaled(
                new LedColorBuffer(sourceData),
                0,
                sourceData.Length / LedColorBuffer.BytesPerLed,
                0,
                tempLedColorBuffer.Count);
            Assert.AreEqual(tempLedColorBuffer.Count, 1);
            Assert.IsTrue(
                tempLedColorBuffer.GetSegment(0, 1).ArgbBytes
                    .SequenceEqual(sourceData.Take(1 * LedColorBuffer.BytesPerLed)));
        }
    }
}