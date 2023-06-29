namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LogicalStripFactoryTests
    {
        private LogicalStripFactory _logicalStripFactory;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPathToXmlIsNull()
        {
            _logicalStripFactory = new LogicalStripFactory(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenPathToXmlIsEmpty()
        {
            _logicalStripFactory =
                new LogicalStripFactory(new LogicalStripInformation { LogicalStripCreationRuleXmlPath = "" });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void WhenPathToXmlIsNotSupported()
        {
            _logicalStripFactory =
                new LogicalStripFactory(new LogicalStripInformation { LogicalStripCreationRuleXmlPath = "com1:" });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void WhenPathToXmlDoesNotExist()
        {
            _logicalStripFactory = new LogicalStripFactory(
                new LogicalStripInformation { LogicalStripCreationRuleXmlPath = "MyPath.xml" });
        }

        [TestMethod]
        public void WhenXmlDoesNotMatch()
        {
            var physicalStrips = new List<Mock<IStrip>>
            {
                new(),
                new(),
                new(),
                new(),
                new()
            };
            var stripId = 0x1200;
            foreach (var strip in physicalStrips)
            {
                strip.SetupGet(x => x.StripId).Returns(stripId++);
                strip.SetupGet(x => x.LedCount).Returns(24);
            }

            _logicalStripFactory = new LogicalStripFactory(
                new LogicalStripInformation
                {
                    LogicalStripCreationRuleXmlPath = @"EdgeLight\Manager\LogicalStripsCreationRuleTest.xml"
                });

            var logicalStrips =
                _logicalStripFactory.GetLogicalStrips(physicalStrips.Select(x => x.Object).ToList());

            Assert.IsTrue(logicalStrips.Count == 5);
            Assert.IsTrue(logicalStrips[0].StripId == 0x0);
            Assert.IsTrue(logicalStrips[1].StripId == 0x1);
            Assert.IsTrue(logicalStrips[2].StripId == 0x2);
            Assert.IsTrue(logicalStrips[3].StripId == 0x3);
            Assert.IsTrue(logicalStrips[4].StripId == 0x4);
        }

        [TestMethod]
        public void WhenXmlIsCorrectlyMatched_1()
        {
            var physicalStrips = new List<Mock<IStrip>>
            {
                new(),
                new(),
                new(),
                new(),
                new()
            };
            var stripId = 0x130000;
            foreach (var strip in physicalStrips)
            {
                strip.SetupGet(x => x.StripId).Returns(stripId++);
                strip.SetupGet(x => x.LedCount).Returns(24);
            }

            _logicalStripFactory = new LogicalStripFactory(
                new LogicalStripInformation
                {
                    LogicalStripCreationRuleXmlPath = @"EdgeLight\Manager\LogicalStripsCreationRuleTest.xml"
                });

            var logicalStrips =
                _logicalStripFactory.GetLogicalStrips(physicalStrips.Select(x => x.Object).ToList());

            Assert.IsTrue(logicalStrips.Count == 4);
            Assert.IsTrue(
                logicalStrips[0].StripId == 0x31
                && logicalStrips[0].LedCount == 24);
            Assert.IsTrue(
                logicalStrips[1].StripId == 0x32
                && logicalStrips[0].LedCount == 24);
            Assert.IsTrue(
                logicalStrips[2].StripId == 0x33
                && logicalStrips[0].LedCount == 24);
            Assert.IsTrue(
                logicalStrips[3].StripId == 0x4
                && logicalStrips[0].LedCount == 24);
        }

        [TestMethod]
        public void WhenXmlIsCorrectlyMatched_2()
        {
            var physicalStrips = new List<Mock<IStrip>>
            {
                new(),
                new(),
                new(),
                new(),
                new()
            };
            var stripId = 0x120000;
            foreach (var strip in physicalStrips)
            {
                strip.SetupGet(x => x.StripId).Returns(stripId++);
                strip.SetupGet(x => x.LedCount).Returns(24);
            }

            physicalStrips.Add(new Mock<IStrip>());
            physicalStrips[5].SetupGet(x => x.StripId).Returns(0xC9);
            physicalStrips[5].SetupGet(x => x.LedCount).Returns(24);
            physicalStrips.Add(new Mock<IStrip>());
            physicalStrips[6].SetupGet(x => x.StripId).Returns(0xC8);
            physicalStrips[6].SetupGet(x => x.LedCount).Returns(24);
            physicalStrips.Add(new Mock<IStrip>());
            physicalStrips[7].SetupGet(x => x.StripId).Returns(0x20);
            physicalStrips[7].SetupGet(x => x.LedCount).Returns(24);

            _logicalStripFactory = new LogicalStripFactory(
                new LogicalStripInformation
                {
                    LogicalStripCreationRuleXmlPath = @"EdgeLight\Manager\LogicalStripsCreationRuleTest.xml"
                });

            var logicalStrips =
                _logicalStripFactory.GetLogicalStrips(physicalStrips.Select(x => x.Object).ToList());

            Assert.IsTrue(logicalStrips.Count == 13);
            Assert.IsTrue(
                logicalStrips[0].StripId == 0x31
                && logicalStrips[0].LedCount == 12
                && logicalStrips[0] is LogicalStrip logical0
                && logical0.LedSegments.Count == 1
                && logical0.LedSegments[0].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[1].StripId == 0x32
                && logicalStrips[1].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[2].StripId == 0x33
                && logicalStrips[2].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[3].StripId == 0x34
                && logicalStrips[3].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[4].StripId == 0x35
                && logicalStrips[4].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[5].StripId == 0x36
                && logicalStrips[5].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[6].StripId == 0x37
                && logicalStrips[6].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[7].StripId == 0x38
                && logicalStrips[7].LedCount == 12);
            Assert.IsTrue(
                logicalStrips[8].StripId == 0x20
                && logicalStrips[8].LedCount == 10
                && logicalStrips[8] is LogicalStrip logical8
                && logical8.LedSegments.Count == 2
                && logical8.LedSegments[0].LedCount == 5
                && logical8.LedSegments[1].LedCount == 5);
            Assert.IsTrue(
                logicalStrips[9].StripId == 0x21
                && logicalStrips[9].LedCount == 14
                && logicalStrips[9] is LogicalStrip logical9
                && logical9.LedSegments.Count == 4
                && logical9.LedSegments[0].LedCount == 4
                && logical9.LedSegments[1].LedCount == 3
                && logical9.LedSegments[2].LedCount == 4
                && logical9.LedSegments[3].LedCount == 3);
            Assert.IsTrue(
                logicalStrips[10].StripId == 0x04
                && logicalStrips[10].LedCount == physicalStrips[4].Object.LedCount);
            Assert.IsTrue(
                logicalStrips[11].StripId == 0xC9
                && logicalStrips[11].LedCount == physicalStrips[5].Object.LedCount
                && logicalStrips[11].IsBarKeeper());
            Assert.IsTrue(
                logicalStrips[12].StripId == 0xC8
                && logicalStrips[12].LedCount == physicalStrips[6].Object.LedCount
                && logicalStrips[12].IsBarKeeper());
        }
    }
}