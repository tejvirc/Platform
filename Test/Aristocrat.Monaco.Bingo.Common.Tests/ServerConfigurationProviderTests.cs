namespace Aristocrat.Monaco.Bingo.Common.Tests
{
    using System;
    using System.Data.Entity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Storage.Model;

    [TestClass]
    public class ServerConfigurationProviderTests
    {
        private ServerConfigurationProvider _target;
        private Mock<IMonacoContextFactory> _factory;
        private Mock<IRepository<BingoServerSettingsModel>> _modelRepo;

        [TestInitialize]
        public void Initialize()
        {
            _factory = new Mock<IMonacoContextFactory>(MockBehavior.Strict);
            _modelRepo = new Mock<IRepository<BingoServerSettingsModel>>(MockBehavior.Strict);
        }

        [DataTestMethod]
        [DataRow(true, false, DisplayName = "No factory")]
        [DataRow(false, true, DisplayName = "No model repository")]
        [DataRow(true, true, DisplayName = "No factory or model repository")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail(bool noFactory, bool noModel)
        {
            _target = new ServerConfigurationProvider(
                noFactory ? null : _factory.Object,
                noModel ? null : _modelRepo.Object);
        }

        [TestMethod]
        public void Constructor()
        {
            _target = new ServerConfigurationProvider(_factory.Object, _modelRepo.Object);
        }

        [DataTestMethod]
        [DataRow(false, DisplayName = "No database values found")]
        [DataRow(true, DisplayName = "Found database values")]
        public void GetServerConfiguration(bool foundValues)
        {
            // Setup
            var model = foundValues ? new BingoServerSettingsModel { VoucherInLimit = 1234 } : null;

            _target = new ServerConfigurationProvider(_factory.Object, _modelRepo.Object);
            _factory.Setup(m => m.Create()).Returns(It.IsAny<DbContext>());
            _modelRepo.Setup(m => m.GetSingle(It.IsAny<DbContext>())).Returns(model);

            // Test
            var resultModel = _target.GetServerConfiguration();

            // Verify
            Assert.IsNotNull(resultModel);
            if (foundValues)
            {
                Assert.IsNotNull(resultModel.VoucherInLimit);
                Assert.AreEqual(model.VoucherInLimit, resultModel.VoucherInLimit);
            }
            else
            {
                Assert.IsNull(resultModel.VoucherInLimit);
            }
        }
    }
}
