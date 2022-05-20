namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SaveCurrentCertificateHandlerTests : BaseCertificateHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfICertificateRepositoryIsNull()
        {
            var handler = new AddCertificateHandler(ContextFactoryMock.Object, null, CertificateEntityValidator);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfICertificateValidatorEntityIsNull()
        {
            var handler = new AddCertificateHandler(ContextFactoryMock.Object, CertificateRepositoryMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfSaveCertificateArgsIsNull()
        {
            var handler = new AddCertificateHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                CertificateEntityValidator);

            handler.Execute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfCertificateIsNull()
        {
            var handler = new AddCertificateHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                CertificateEntityValidator);

            handler.Execute(null);
        }

        [TestMethod]
        public void AddCertificateTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("a-a-a");

            CertificateRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(new List<Certificate>().AsQueryable());

            var handler = new AddCertificateHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                CertificateEntityValidator);

            var result = handler.Execute(certificate);

            CertificateRepositoryMock.Verify(x => x.Add(It.IsAny<DbContext>(), It.IsAny<Certificate>()), Times.Once);

            Assert.IsNotNull(result);
        }
    }
}