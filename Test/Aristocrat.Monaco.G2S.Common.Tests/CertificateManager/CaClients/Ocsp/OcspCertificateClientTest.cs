namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CaClients.Ocsp
{
    using System;
    using Common.CertificateManager.CaClients.Ocsp;
    using Common.CertificateManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class OcspCertificateClientTest
    {
        private Mock<IMonacoContextFactory> _contextFactoryMock;
        private Mock<IPkiConfigurationRepository> _pkiConfiguration;
        private Mock<ICertificateRepository> _repositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _repositoryMock = new Mock<ICertificateRepository>();
            _pkiConfiguration = new Mock<IPkiConfigurationRepository>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsructWithNullContextFactoryExpectException()
        {
            var client = new OcspCertificateClient(null, null, null);

            Assert.IsNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsructWithNullCertificateRepositoryExpectException()
        {
            var client = new OcspCertificateClient(_contextFactoryMock.Object, null, null);

            Assert.IsNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConsructWithNullPkiConfigurationRepositoryExpectException()
        {
            var client = new OcspCertificateClient(_contextFactoryMock.Object, _repositoryMock.Object, null);

            Assert.IsNull(client);
        }
    }
}