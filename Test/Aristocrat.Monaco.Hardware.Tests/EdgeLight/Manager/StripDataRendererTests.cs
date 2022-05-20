namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Manager;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StripDataRendererTests
    {
        private StripDataRenderer _dataRenderer;
        private PriorityComparer _priorityComparer;

        [TestInitialize]
        public void Setup()
        {
            _priorityComparer = new PriorityComparer();
            _dataRenderer = new StripDataRenderer(_priorityComparer);
        }

        [TestMethod]
        public void StripDataRendererTest()
        {
            Assert.IsFalse(_dataRenderer.RemovePriority(0, StripPriority.Absolute));
            Assert.IsTrue(_dataRenderer.RenderedData(0).Count == EdgeLightConstants.MaxLedPerStrip);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == 0));
        }

        [TestMethod]
        public void SetColorTest()
        {
            _dataRenderer.SetColor(0, Color.AliceBlue, StripPriority.AuditMenu);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.AliceBlue.ToArgb()));
            _dataRenderer.SetColor(0, Color.Green, StripPriority.GamePriority);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.AliceBlue.ToArgb()));
            _dataRenderer.RemovePriority(0, StripPriority.AuditMenu);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.Green.ToArgb()));
            _dataRenderer.SetColor(0, Color.AliceBlue, StripPriority.AuditMenu);
            _dataRenderer.SetColor(1, Color.Red, StripPriority.AuditMenu);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.AliceBlue.ToArgb()));
            Assert.IsTrue(_dataRenderer.RenderedData(1).All(x => x.ToArgb() == Color.Red.ToArgb()));

            //Transparent color test
            _dataRenderer.SetColor(0, Color.FromArgb(0, 255, 0, 0), StripPriority.AuditMenu);
            _dataRenderer.SetColor(0, Color.Green, StripPriority.GamePriority);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.Green.ToArgb()));
        }

        [TestMethod]
        public void SetColorBufferTest()
        {
            var colorBuffer = new LedColorBuffer(EdgeLightConstants.MaxLedPerStrip);
            colorBuffer.SetColor(10, Color.Red, colorBuffer.Count - 10);
            colorBuffer.SetColor(0, Color.Green, 10);
            _dataRenderer.SetColorBuffer(0, colorBuffer, 10, 10, colorBuffer.Count - 10, StripPriority.AuditMenu);
            var data = _dataRenderer.RenderedData(0);
            Assert.IsTrue(data.Take(10).Select(x => x.ToArgb()).All(x => x == 0));
            Assert.IsTrue(data.Skip(10).Select(x => x.ToArgb()).All(x => x == Color.Red.ToArgb()));
            _dataRenderer.SetColorBuffer(0, colorBuffer, 0, 0, 10, StripPriority.GamePriority);
            Assert.IsTrue(
                _dataRenderer.RenderedData(0).Select(x => x.ToArgb())
                    .SequenceEqual(colorBuffer.Select(x => x.ToArgb())));
            _dataRenderer.RemovePriority(0, StripPriority.AuditMenu);
            data = _dataRenderer.RenderedData(0);
            Assert.IsTrue(data.Take(10).Select(x => x.ToArgb()).All(x => x == Color.Green.ToArgb()));
            Assert.IsTrue(data.Skip(10).Select(x => x.ToArgb()).All(x => x == 0));
        }

        [TestMethod]
        public void SetColorBufferBoundaryTest()
        {
            var colorBuffer = new LedColorBuffer(EdgeLightConstants.MaxLedPerStrip);
            // Boundary conditions
            Helper.AssertThrow<ArgumentOutOfRangeException>(
                () =>
                    _dataRenderer.SetColorBuffer(
                        0,
                        colorBuffer,
                        int.MinValue,
                        10,
                        colorBuffer.Count - 10,
                        StripPriority.AuditMenu)
            );

            Helper.AssertThrow<ArgumentOutOfRangeException>(
                () =>
                    _dataRenderer.SetColorBuffer(
                        0,
                        colorBuffer,
                        0,
                        int.MinValue,
                        colorBuffer.Count - 10,
                        StripPriority.AuditMenu)
            );

            Helper.AssertThrow<ArgumentOutOfRangeException>(
                () => _dataRenderer.SetColorBuffer(
                    0,
                    colorBuffer,
                    0,
                    0,
                    int.MaxValue,
                    StripPriority.AuditMenu)
            );
        }

        [TestMethod]
        public void ChangeComparerTest()
        {
            _dataRenderer.SetColor(0, Color.AliceBlue, StripPriority.AuditMenu);
            _dataRenderer.SetColor(0, Color.Green, StripPriority.GamePriority);
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.AliceBlue.ToArgb()));
            _priorityComparer.OverridenComparer = new TestPriorityComparer(
                new List<StripPriority> { StripPriority.GamePriority, StripPriority.AuditMenu });
            Assert.IsTrue(_dataRenderer.RenderedData(0).All(x => x.ToArgb() == Color.Green.ToArgb()));
        }
    }
}