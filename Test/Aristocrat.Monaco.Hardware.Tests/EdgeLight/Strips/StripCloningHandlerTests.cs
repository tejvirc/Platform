namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Strips
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Strips;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StripCloningHandlerTests
    {
        private StripCloningHandler _stripCloningHandler;
        private Mock<IEdgeLightManager> _mockEdgelightManager;
        private List<(int Id, LedColorBuffer Data)> _gameStripData = new List<(int Id, LedColorBuffer Data)>();
        private List<StripData> _mockLogicalStrips = new List<StripData>();

        [TestInitialize]
        public void Initialize()
        {
            _mockEdgelightManager = new Mock<IEdgeLightManager>();
            _mockLogicalStrips.Add(new StripData { StripId = (int)StripIDs.MainCabinetBottom, LedCount = 2 });
            _mockLogicalStrips.Add(new StripData { StripId = (int)StripIDs.MainCabinetTop, LedCount = 2 });

            _mockEdgelightManager.Setup(x => x.LogicalStrips).Returns(_mockLogicalStrips);

            _stripCloningHandler = new StripCloningHandler(_mockEdgelightManager.Object, "./StripCloneMappings.xml");

            _gameStripData.Add(
                (Id: (int)StripIDs.MainCabinetBottom, Data: new LedColorBuffer(new byte[]{
                    0xff,
                    0,
                    0,
                    0xff,
                    0xff,
                    0,
                    0,
                    0xff
                })));
        }

        [TestMethod]
        public void StripCloningHandlerConstructor()
        {
            Assert.IsNotNull(_stripCloningHandler);
        }

        [TestMethod]
        public void TestStripCloning()
        {
            var count = _gameStripData.Count;
            _gameStripData = _stripCloningHandler.Clone(_gameStripData).ToList();
            Assert.AreEqual(_gameStripData.Count, count + 1);
            Assert.IsTrue(_gameStripData.Exists(x => x.Id == (int)StripIDs.MainCabinetTop));
            Assert.IsTrue(
                _gameStripData.First(x => x.Id == (int)StripIDs.MainCabinetBottom).Data.SequenceEqual(
                    _gameStripData.First(x => x.Id == (int)StripIDs.MainCabinetTop).Data));
        }

        [TestMethod]
        public void TestStripDownScale()
        {
            _mockEdgelightManager.Setup(x => x.LogicalStrips).Returns(
                new List<StripData>
                {
                    new StripData { StripId = (int)StripIDs.MainCabinetBottom, LedCount = 2 },
                    new StripData { StripId = (int)StripIDs.MainCabinetTop, LedCount = 1 }
                });

            var count = _gameStripData.Count;
            _gameStripData = _stripCloningHandler.Clone(_gameStripData).ToList();
            Assert.AreEqual(_gameStripData.Count, count + 1);
            Assert.IsTrue(_gameStripData.Exists(x => x.Id == (int)StripIDs.MainCabinetTop));
            Assert.AreEqual(
                _gameStripData.First(x => x.Id == (int)StripIDs.MainCabinetBottom).Data.Count,
                _mockEdgelightManager.Object.LogicalStrips.First(x => x.StripId == (int)StripIDs.MainCabinetBottom)
                    .LedCount);
        }

        [TestMethod]
        public void TestStripUpScale()
        {
            _mockEdgelightManager.Setup(x => x.LogicalStrips).Returns(
                new List<StripData>
                {
                    new StripData { StripId = (int)StripIDs.MainCabinetBottom, LedCount = 2 },
                    new StripData { StripId = (int)StripIDs.VbdBottomStrip, LedCount = 100 }
                });

            var target = new StripCloningHandler(_mockEdgelightManager.Object, "./StripCloneMappings.xml");
            var count = _gameStripData.Count;
            _gameStripData = target.Clone(_gameStripData).ToList();
            Assert.AreEqual(_gameStripData.Count, count + 1);
            Assert.IsTrue(_gameStripData.Exists(x => x.Id == (int)StripIDs.VbdBottomStrip));
            Assert.AreEqual(
                _gameStripData.First(x => x.Id == (int)StripIDs.VbdBottomStrip).Data.Count,
                _mockEdgelightManager.Object.LogicalStrips.First(x => x.StripId == (int)StripIDs.VbdBottomStrip)
                    .LedCount);
        }
    }
}