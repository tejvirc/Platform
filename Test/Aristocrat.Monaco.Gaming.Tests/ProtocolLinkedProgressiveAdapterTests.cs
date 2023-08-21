namespace Aristocrat.Monaco.Gaming.Tests
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Gaming.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    public class ProtocolLinkedProgressiveAdapterTests
    {
        ProtocolLinkedProgressiveAdapter _target;
        private const CommsProtocol ProgressiveProtocol = CommsProtocol.SAS;

        private Mock<ILinkedProgressiveProvider> _linkedProgressiveProvider;
        private Mock<IProgressiveConfigurationProvider> _progressiveConfigurationProvider;
        private Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider;
        private Mock<IProgressiveGameProvider> _progressiveGameProvider;

        [TestInitialize]
        public void Initialize()
        {
            _multiProtocolConfigurationProvider = new Mock<IMultiProtocolConfigurationProvider>(MockBehavior.Strict);
            _multiProtocolConfigurationProvider.Setup(m => m.MultiProtocolConfiguration)
                .Returns(new List<ProtocolConfiguration>() { new ProtocolConfiguration(ProgressiveProtocol, isProgressiveHandled: true) });

            _linkedProgressiveProvider = new Mock<ILinkedProgressiveProvider>(MockBehavior.Strict);
            _progressiveConfigurationProvider = new Mock<IProgressiveConfigurationProvider>(MockBehavior.Default);
            _progressiveGameProvider = new Mock<IProgressiveGameProvider>(MockBehavior.Default);

            _target = new ProtocolLinkedProgressiveAdapter(
                _linkedProgressiveProvider.Object,
                _progressiveConfigurationProvider.Object,
                _multiProtocolConfigurationProvider.Object,
                _progressiveGameProvider.Object);

            _target.Initialize();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_ReportLinkDown_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.ReportLinkDown(It.IsAny<string>()));

            _target.ReportLinkDown(EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.ReportLinkDown(It.IsAny<string>()), times);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_ReportLinkUp_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.ReportLinkUp(It.IsAny<string>()));

            _target.ReportLinkUp(EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.ReportLinkUp(It.IsAny<string>()), times);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_UpdateLinkedProgressiveLevels_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>()));

            _target.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.UpdateLinkedProgressiveLevels(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>()), times);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_UpdateLinkedProgressiveLevelsAsync_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>()))
                .Returns(It.IsAny<Task>());

            _target.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>(), EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<IViewableLinkedProgressiveLevel>>()), times);
        }

        [TestMethod]
        public void LinkedProgressiveProvider_ViewLinkedProgressiveLevels_AlwaysCalled()
        {
            _linkedProgressiveProvider.Setup(m => m.ViewLinkedProgressiveLevels()).Returns(Array.Empty<IViewableLinkedProgressiveLevel>());

            var returnValue = _target.ViewLinkedProgressiveLevels();

            _linkedProgressiveProvider.Verify(x => x.ViewLinkedProgressiveLevels(), Times.Once);
            Assert.AreEqual(returnValue, Array.Empty<IViewableLinkedProgressiveLevel>());
        }

        [TestMethod]
        public void LinkedProgressiveProvider_ViewLinkedProgressiveLevel_AlwaysCalled()
        {
            IViewableLinkedProgressiveLevel level;
            _linkedProgressiveProvider.Setup(m => m.ViewLinkedProgressiveLevel(It.IsAny<string>(), out level)).Returns(true);

            var returnValue = _target.ViewLinkedProgressiveLevel(It.IsAny<string>(), out level);

            _linkedProgressiveProvider.Verify(x => x.ViewLinkedProgressiveLevel(It.IsAny<string>(), out level), Times.Once);
            Assert.AreEqual(returnValue, true);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_ClaimLinkedProgressiveLevel_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.ClaimLinkedProgressiveLevel(It.IsAny<string>()));

            _target.ClaimLinkedProgressiveLevel(It.IsAny<string>(), EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.ClaimLinkedProgressiveLevel(It.IsAny<string>()), times);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_AwardLinkedProgressiveLevel1_ChecksProtocol(CommsProtocol protocol)
        {
            _linkedProgressiveProvider.Setup(m => m.AwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<long>()));

            _target.AwardLinkedProgressiveLevel(It.IsAny<string>(), EnumParser.ToName(protocol));
            _target.AwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<long>(), EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Exactly(2) : Times.Never();
            _linkedProgressiveProvider.Verify(x => x.AwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<long>()), times);
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void LinkedProgressiveProvider_ClaimAndAwardLinkedProgressiveLayer_ChecksProtocol(CommsProtocol protocolName)
        {
            _linkedProgressiveProvider.Setup(m =>
                m.ClaimAndAwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<long>()));

            _target.ClaimAndAwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>());

            var times = protocolName == ProgressiveProtocol ? Times.Once() : Times.Never();

            _linkedProgressiveProvider.Verify(
                x => x.ClaimAndAwardLinkedProgressiveLevel(It.IsAny<string>(), It.IsAny<long>()), times);
        }

        [TestMethod]
        public void ProgressiveConfigurationProvider_ViewProgressiveLevels_AlwaysCalled()
        {
            _progressiveConfigurationProvider.Setup(m => m.ViewProgressiveLevels()).Returns(Array.Empty<IViewableProgressiveLevel>());

            var returnValue = _target.ViewProgressiveLevels();

            _progressiveConfigurationProvider.Verify(x => x.ViewProgressiveLevels(), Times.Once);
            Assert.AreEqual(returnValue, Array.Empty<IViewableProgressiveLevel>());
        }

        [DataRow(CommsProtocol.SAS)]
        [DataRow(CommsProtocol.G2S)]
        [DataTestMethod]
        public void ProgressiveConfigurationProvider_AssignLevelsToGame_ChecksProtocol(CommsProtocol protocol)
        {
            _progressiveConfigurationProvider.Setup(m => m.AssignLevelsToGame(It.IsAny<IReadOnlyCollection<ProgressiveLevelAssignment>>()))
                .Returns(Array.Empty<IViewableProgressiveLevel>());

            var returnValue = _target.AssignLevelsToGame(It.IsAny<IReadOnlyCollection<ProgressiveLevelAssignment>>(), EnumParser.ToName(protocol));

            Times times = protocol == ProgressiveProtocol ? Times.Once() : Times.Never();
            _progressiveConfigurationProvider.Verify(x => x.AssignLevelsToGame(It.IsAny<IReadOnlyCollection<ProgressiveLevelAssignment>>()), times);

            var expectedValue = protocol == ProgressiveProtocol ? Array.Empty<IViewableProgressiveLevel>() : null;
            Assert.AreEqual(returnValue, expectedValue);
        }

        [TestMethod]
        public void ProgressiveConfigurationProvider_ViewConfiguredProgressiveLevels1_AlwaysCalled()
        {
            _progressiveConfigurationProvider.Setup(m => m.ViewConfiguredProgressiveLevels())
                .Returns(Array.Empty<IViewableProgressiveLevel>());

            var returnValue = _target.ViewConfiguredProgressiveLevels();

            _progressiveConfigurationProvider.Verify(x => x.ViewConfiguredProgressiveLevels(), Times.Once);
            Assert.AreEqual(returnValue, Array.Empty<IViewableProgressiveLevel>());
        }

        [TestMethod]
        public void ProgressiveConfigurationProvider_ViewConfiguredProgressiveLevels2_AlwaysCalled()
        {
            _progressiveConfigurationProvider.Setup(m => m.ViewConfiguredProgressiveLevels(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(Array.Empty<IViewableProgressiveLevel>());

            var returnValue = _target.ViewConfiguredProgressiveLevels(It.IsAny<int>(), It.IsAny<long>());

            _progressiveConfigurationProvider.Verify(x => x.ViewConfiguredProgressiveLevels(It.IsAny<int>(), It.IsAny<long>()), Times.Once);
            Assert.AreEqual(returnValue, Array.Empty<IViewableProgressiveLevel>());
        }

        [TestMethod]
        public void ProgressiveGameProvider_GetActiveProgressiveLevels_AlwaysCalled()
        {
            _progressiveGameProvider.Setup(m => m.GetActiveProgressiveLevels()).Returns(Array.Empty<IViewableProgressiveLevel>());

            var returnValue = _target.GetActiveProgressiveLevels();

            _progressiveGameProvider.Verify(x => x.GetActiveProgressiveLevels(), Times.Once);
            Assert.AreEqual(returnValue, Array.Empty<IViewableProgressiveLevel>());
        }
    }
}
