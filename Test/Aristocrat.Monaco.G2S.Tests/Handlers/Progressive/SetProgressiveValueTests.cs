namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.G2S.Services;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using G2S.Handlers.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetProgressiveValueTests
    {
        private Mock<IG2SEgm> _egm;
        private Mock<ICommandBuilder<IProgressiveDevice, progressiveValueAck>> _commandBuilder;
        private Mock<IProgressiveLevelProvider> _progressiveProvider;
        private Mock<IProgressiveLevelManager> _progressiveLevelManager;
        private Mock<IProgressiveDeviceManager> _progressiveDeviceManager;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
            _commandBuilder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveValueAck>>();
            _progressiveProvider = new Mock<IProgressiveLevelProvider>();
            _progressiveLevelManager = new Mock<IProgressiveLevelManager>();
            _progressiveDeviceManager = new Mock<IProgressiveDeviceManager>();
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoEgmExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(null, _commandBuilder.Object, _progressiveProvider.Object, _progressiveLevelManager.Object, _progressiveDeviceManager.Object, _protocolLinkedProgressiveAdapter.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoCommandBuilderExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, null, _progressiveProvider.Object, _progressiveLevelManager.Object, _progressiveDeviceManager.Object, _protocolLinkedProgressiveAdapter.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoProgressiveProviderExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, _commandBuilder.Object, null, _progressiveLevelManager.Object, _progressiveDeviceManager.Object, _protocolLinkedProgressiveAdapter.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoProgressiveServiceExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, _commandBuilder.Object, _progressiveProvider.Object, null, _progressiveDeviceManager.Object, _protocolLinkedProgressiveAdapter.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoProgressiveDeviceManagerExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, _commandBuilder.Object, _progressiveProvider.Object, _progressiveLevelManager.Object, null, _protocolLinkedProgressiveAdapter.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoProtocolLinkedProgressiveAdapterExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, _commandBuilder.Object, _progressiveProvider.Object, _progressiveLevelManager.Object, _progressiveDeviceManager.Object, null));
        }

        [TestMethod]
        public void WhenConstructWithEgmExpectException()
        {
            //var progressiveDataService = new Mock<IProgressiveService>();
            //var jackpotProvider = new Mock<G2SJackpotProvider>();


            var handler = new SetProgressiveValue(
                _egm.Object,
                _commandBuilder.Object,
                _progressiveProvider.Object,
                _progressiveLevelManager.Object,
                _progressiveDeviceManager.Object,
                _protocolLinkedProgressiveAdapter.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyOwnerExpectSuccess()
        {
            var device = new Mock<IProgressiveDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);
            var handler = CreateHandler(egm, _commandBuilder.Object, _progressiveProvider.Object, _progressiveLevelManager.Object, _progressiveDeviceManager.Object, _protocolLinkedProgressiveAdapter.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        private SetProgressiveValue CreateHandler(IG2SEgm egm = null,
            ICommandBuilder<IProgressiveDevice, progressiveValueAck> commandBuilder = null,
            IProgressiveLevelProvider progressiveProvider = null,
            IProgressiveLevelManager progressiveLevelManager = null,
            IProgressiveDeviceManager progressiveDeviceManager = null,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter = null)
        {
            var handler = new SetProgressiveValue(egm, commandBuilder, progressiveProvider, progressiveLevelManager, progressiveDeviceManager, _protocolLinkedProgressiveAdapter.Object);

            return handler;
        }

        private ClassCommand<progressive, setProgressiveValue> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<progressive, setProgressiveValue>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return command;
        }
    }
}