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
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using G2S.Handlers.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetProgressiveValueTests
    {
        private Mock<IG2SEgm> _egm;
        private Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>> _commandBuilder;
        private Mock<IProgressiveLevelProvider> _progressiveProvider;

        [TestInitialize]
        public void Initialize()
        {
            _egm = new Mock<IG2SEgm>();
            _commandBuilder = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
            _progressiveProvider = new Mock<IProgressiveLevelProvider>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoEgmExpectException()
        { 
            Assert.IsNull(new GetProgressiveStatus(null, _commandBuilder.Object, _progressiveProvider.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoCommandBuilderExpectException()
        {
            Assert.IsNull(new SetProgressiveValue(_egm.Object, null, _progressiveProvider.Object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNoProgressiveProviderExpectException()
        {
            Assert.IsNull(new GetProgressiveStatus(_egm.Object, _commandBuilder.Object, null));
        }

        [TestMethod]
        public async Task WhenVerifyOwnerExpectSuccess()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IProgressiveService>(MockBehavior.Default);
            var device = new Mock<IProgressiveDevice>();
            var egm = HandlerUtilities.CreateMockEgm(device);
            var handler = CreateHandler(egm, _commandBuilder.Object, _progressiveProvider.Object);

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        private GetProgressiveStatus CreateHandler(IG2SEgm egm = null,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> commandBuilder = null,
            IProgressiveLevelProvider progressiveProvider = null)
        {
            var handler = new GetProgressiveStatus(egm, commandBuilder, progressiveProvider);

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