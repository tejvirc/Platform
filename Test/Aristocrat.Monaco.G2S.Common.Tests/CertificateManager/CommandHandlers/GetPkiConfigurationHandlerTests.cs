namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetPkiConfigurationHandlerTests : BaseCertificateConfigurationHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var handler = new GetPkiConfigurationHandler(null, PkiConfigurationRepositoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPkiConfigurationRepositoryExpectException()
        {
            var handler = new GetPkiConfigurationHandler(ContextFactoryMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void GetCertificateConfigurationEntityTest()
        {
            SetPkiConfigurationRepositoryReturnValue();

            var handler = new GetPkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object);
            handler.Execute();

            PkiConfigurationRepositoryMock.Verify(x => x.GetSingle(It.IsAny<DbContext>()), Times.Once);
        }

        [TestMethod]
        public void SetPkiConfigurationEntityPropertyTest()
        {
            SetPkiConfigurationRepositoryReturnValue();
            var handler = new GetPkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object);
            var entity = handler.Execute();

            Assert.IsTrue(entity != null);
        }

        private void SetPkiConfigurationRepositoryReturnValue()
        {
            PkiConfigurationRepositoryMock
                .Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(new PkiConfiguration());
        }
    }
}