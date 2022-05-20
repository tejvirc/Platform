namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Strips
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LogicalStripTest
    {
        public int Count { get; private set; }
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Strict);

        private void Strip_BrightnessChanged(object sender, EventArgs e)
        {
            Assert.IsTrue(sender is LogicalStrip _);
            Count++;
        }

        private void PhysicalStrip_BrightnessChanged(object sender, EventArgs e)
        {
            Count++;
        }

        [TestMethod]
        public void CreatingDefault()
        {
            var strip = new LogicalStrip();
            Assert.IsTrue(strip.Id == int.MinValue);
            Assert.IsTrue(strip.StripId == int.MinValue);
            Assert.IsTrue(strip.Brightness == 100);
            Assert.IsTrue(strip.LedSegments.Count == 0);
            Assert.IsTrue(strip.LedCount == 0);
            Assert.IsTrue(strip.ColorBuffer.Count == 0);
            strip.BrightnessChanged += Strip_BrightnessChanged;
            strip.Brightness = 90;
            Assert.IsTrue(Count == 1);
        }

        [TestMethod]
        public void CreatingWithMockedPhysical()
        {
            var colorList = new List<Color>
            {
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
                Color.Blue,
                Color.Black,
                Color.Lime,
            };

            var physicalStripMocks = new List<Mock<IStrip>>()
            {
                _mockRepository.Create<IStrip>(), _mockRepository.Create<IStrip>()
            };
            var stripId = 0;
            foreach (var physicalStripMock in physicalStripMocks)
            {
                var id = stripId++;
                physicalStripMock.SetupGet(x => x.StripId).Returns(id);
                physicalStripMock.SetupGet(x => x.LedCount).Returns(24);
                physicalStripMock.SetupGet(x => x.ColorBuffer).Returns(new LedColorBuffer(24));
                physicalStripMock.SetupGet(x => x.Brightness).Returns(90);
            }

            var strip = new LogicalStrip
            {
                LedSegments = new List<LedSegment>
                {
                    new LedSegment { From = 0, To = 20, PhysicalStrip = physicalStripMocks[0].Object, Id = 0 },
                    new LedSegment { From = 0, To = 20, PhysicalStrip = physicalStripMocks[1].Object, Id = 1 }
                }
            };
            
            Assert.IsTrue(strip.LedCount == 42);
            Assert.IsTrue(strip.Id == int.MinValue);
            Assert.IsTrue(strip.StripId == int.MinValue);
            Assert.IsTrue(strip.Brightness == 90);
            Assert.IsTrue(strip.ColorBuffer.Count == 42);
            Assert.IsTrue(physicalStripMocks[0].Object.StripId == 0);
            Assert.IsTrue(physicalStripMocks[0].Object.LedCount == 24);
            Assert.IsTrue(physicalStripMocks[0].Object.Brightness == 90);
            Assert.IsTrue(physicalStripMocks[1].Object.StripId == 1);
            Assert.IsTrue(physicalStripMocks[1].Object.LedCount == 24);
            Assert.IsTrue(physicalStripMocks[1].Object.Brightness == 90);

            Count = 0;

            foreach (var physicalStripMock in physicalStripMocks)
            {
                physicalStripMock.SetupSet(x => x.Brightness = 70).Raises(
                    x => x.BrightnessChanged += null,
                    EventArgs.Empty);
                physicalStripMock.Object.BrightnessChanged += PhysicalStrip_BrightnessChanged;
            }

            strip.Brightness = 70;
            Assert.IsTrue(Count == 2);

            physicalStripMocks[0].Setup(x => x.SetColors(new LedColorBuffer(colorList), 0, 21, 0));
            physicalStripMocks[0].SetupGet(x => x.ColorBuffer).Returns(new LedColorBuffer(colorList));
            physicalStripMocks[1].Setup(x => x.SetColors(new LedColorBuffer(new List<Color>()), 0, 0, 0));

            strip.SetColors(
                new LedColorBuffer(
                    colorList),
                0,
                21,
                0);

            Assert.IsTrue(strip.LedSegments[0].ColorBuffer.SequenceEqual(new LedColorBuffer(colorList)));
            Assert.IsTrue(strip.LedSegments[1].ColorBuffer[0].ToArgb() == Color.FromArgb(0,0,0,0).ToArgb());
            physicalStripMocks[0].VerifyAll();
            physicalStripMocks[1].VerifyAll();
        }
    }
}
