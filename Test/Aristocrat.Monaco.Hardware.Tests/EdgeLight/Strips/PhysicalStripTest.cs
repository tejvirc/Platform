namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Strips
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PhysicalStripTest
    {
        public int Count { get; private set; }

        [TestMethod]
        public void WhenStripIsCreatedWithDefaultData()
        {
            var strip = new PhysicalStrip();
            Assert.IsTrue(strip.StripId == 0);
            Assert.IsTrue(strip.LedCount == 0);
            Assert.IsTrue(strip.Brightness == 100);
            Assert.IsTrue(strip.BoardId == BoardIds.InvalidBoardId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsRaisesException_1()
        {
            var strip = new PhysicalStrip();
            strip.SetColors(
                new LedColorBuffer(new List<Color>() { Color.Blue, Color.Blue, Color.Blue, Color.Blue }),
                0,
                4,
                0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsRaisesException_2()
        {
            var strip = new PhysicalStrip();
            strip.SetColors(
                new LedColorBuffer(new List<Color>() { Color.Blue, Color.Blue, Color.Blue, Color.Blue }),
                -1,
                4,
                0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenSetColorsRaisesException_3()
        {
            var strip = new PhysicalStrip();
            strip.SetColors(
                new LedColorBuffer(new List<Color>() { Color.Blue, Color.Blue, Color.Blue, Color.Blue }),
                0,
                10,
                0);
        }

        [TestMethod]
        public void WhenSetColorsIsCorrect()
        {
            var strip = new PhysicalStrip(0,4);
            strip.SetColors(
                new LedColorBuffer(new List<Color>() { Color.Blue, Color.Blue, Color.Blue, Color.Blue }),
                0,
                4,
                0);
        }

        [TestMethod]
        public void WhenBrightnessChangeRaisesEvent()
        {
            var stripList = new List<IStrip>
            {
                new PhysicalStrip(0, 10, BoardIds.MainBoardId),
                new PhysicalStrip(0, 10, BoardIds.MainBoardId),
                new PhysicalStrip(0, 10, BoardIds.MainBoardId),
                new PhysicalStrip(0, 10, BoardIds.MainBoardId),
                new PhysicalStrip(0, 10, BoardIds.MainBoardId),
                new PhysicalStrip(0, 10, BoardIds.MainBoardId)
            };

            foreach (var strip in stripList)
            {
                strip.BrightnessChanged += Strip_BrightnessChanged;
            }

            stripList[0].Brightness = 100;
            Assert.IsTrue(Count == 0);
            stripList[1].Brightness = 100;
            Assert.IsTrue(Count == 0);
            stripList[2].Brightness = 100;
            Assert.IsTrue(Count == 0);
            stripList[3].Brightness = 100;
            Assert.IsTrue(Count == 0);
            stripList[0].Brightness = 0;
            Assert.IsTrue(Count == 1);
            stripList[0].Brightness = 10;
            Assert.IsTrue(Count == 2);
            stripList[0].Brightness = 10;
            Assert.IsTrue(Count == 2);
            stripList[0].Brightness = 100;
            Assert.IsTrue(Count == 3);
            Count = 0;
            foreach (var strip in stripList)
            {
                strip.Brightness = 90;
            }
            Assert.IsTrue(Count == 6);
        }

        private void Strip_BrightnessChanged(object sender, EventArgs e)
        {
            Assert.IsTrue(sender is PhysicalStrip _);
            Count++;
        }
    }
}
