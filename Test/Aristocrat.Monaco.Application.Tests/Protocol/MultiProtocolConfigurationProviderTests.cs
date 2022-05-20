namespace Aristocrat.Monaco.Application.Tests.Protocol
{
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Application.Protocol;
    using Aristocrat.Monaco.Test.Common;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class MultiProtocolConfigurationProviderTests
    {
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageTransaction> _storageTransaction;

        private readonly IDictionary<int, Dictionary<string, object>> _keyValuePairs =
            new Dictionary<int, Dictionary<string, object>>();

        private MultiProtocolConfigurationProvider _target;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _storageTransaction = MoqServiceManager.CreateAndAddService<IPersistentStorageTransaction>(MockBehavior.Default);
            _storageTransaction.Setup(st => st.Commit());

            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            _block.Setup(b => b.GetAll()).Returns(_keyValuePairs);
            _block.Setup(b => b.Count).Returns(_keyValuePairs.Count);
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);

            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _storageManager.Setup(s => s.ResizeBlock(It.IsAny<string>(), It.IsAny<int>()));

            _target = new MultiProtocolConfigurationProvider();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void MultiProtocolConfiguration_OnStartup_ShouldHaveEmptyConfig()
        {
            var multiProtocolConfig = _target.MultiProtocolConfiguration;

            Assert.IsFalse(multiProtocolConfig.Any());
        }

        [DataRow(CommsProtocol.SAS, CommsProtocol.SAS, CommsProtocol.SAS)]
        [DataRow(CommsProtocol.MGAM, CommsProtocol.MGAM, CommsProtocol.MGAM)]
        [DataRow(CommsProtocol.G2S, CommsProtocol.G2S, CommsProtocol.G2S)]
        [DataRow(CommsProtocol.Test, CommsProtocol.Test, CommsProtocol.Test)]
        [DataRow(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.G2S)]
        [TestMethod]
        public void MultiProtocolConfiguration_AfterSavingConfiguration_ShouldReturnSameConfiguration(
            CommsProtocol validation,
            CommsProtocol fundTransfer,
            CommsProtocol progressive)
        {
            var expectedMultiProtocolConfiguration = new List<ProtocolConfiguration>
            {
                new ProtocolConfiguration(CommsProtocol.SAS),
                new ProtocolConfiguration(CommsProtocol.MGAM),
                new ProtocolConfiguration(CommsProtocol.G2S),
                new ProtocolConfiguration(CommsProtocol.Test)
            };

            foreach (var config in expectedMultiProtocolConfiguration)
            {
                config.IsValidationHandled = config.Protocol == validation;
                config.IsFundTransferHandled = config.Protocol == fundTransfer;
                config.IsProgressiveHandled = config.Protocol == progressive;
                config.IsCentralDeterminationHandled = config.Protocol == progressive;
            }

            _target.MultiProtocolConfiguration = expectedMultiProtocolConfiguration;

            for (int i = 0; i < expectedMultiProtocolConfiguration.Count; i++)
            {
                var protocolConfig = expectedMultiProtocolConfiguration[i];

                _keyValuePairs.Add(i, new Dictionary<string, object>
                {
                    { @"MultiProtocol.ProtocolId", protocolConfig.Protocol },
                    { @"MultiProtocol.Validation", protocolConfig.IsValidationHandled },
                    { @"MultiProtocol.FundTransfer", protocolConfig.IsFundTransferHandled },
                    { @"MultiProtocol.Progressive", protocolConfig.IsProgressiveHandled },
                    { @"MultiProtocol.CentralDetermination", protocolConfig.IsCentralDeterminationHandled }
                });
            }

            var actualMultiProtocolConfiguration = _target.MultiProtocolConfiguration.ToList();

            for (int i = 0; i < expectedMultiProtocolConfiguration.Count; i++)
            {
                var expectedProtocolConfiguration = expectedMultiProtocolConfiguration[i];
                var actualProtocolConfiguration = actualMultiProtocolConfiguration[i];

                Assert.AreEqual(expectedProtocolConfiguration.Protocol, actualProtocolConfiguration.Protocol);
                Assert.AreEqual(expectedProtocolConfiguration.IsValidationHandled, actualProtocolConfiguration.IsValidationHandled);
                Assert.AreEqual(expectedProtocolConfiguration.IsFundTransferHandled, actualProtocolConfiguration.IsFundTransferHandled);
                Assert.AreEqual(expectedProtocolConfiguration.IsProgressiveHandled, actualProtocolConfiguration.IsProgressiveHandled);
                Assert.AreEqual(expectedProtocolConfiguration.IsCentralDeterminationHandled, actualProtocolConfiguration.IsCentralDeterminationHandled);
            }
        }
    }
}
