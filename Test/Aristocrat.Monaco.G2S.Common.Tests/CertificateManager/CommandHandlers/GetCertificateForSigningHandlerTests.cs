namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Cryptography.X509Certificates;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCertificateForSigningHandlerTests : BaseCertificateHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var handler = new GetCertificateForSigningHandler(null, CertificateRepositoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateRepositoryExpectException()
        {
            var handler = new GetCertificateForSigningHandler(ContextFactoryMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void CertificateNotExistsInDbTest()
        {
            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(Enumerable.Empty<Certificate>().AsQueryable());

            var handler = new GetCertificateForSigningHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object);
            var result = handler.Execute();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CertificateExistsInDbWithGoodStatusAndExistsInStoreTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("a-b-c");

            var data = certificate.Export(X509ContentType.Pkcs12, "test");

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>())).Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = data,
                            Password = "test"
                        }
                    }.AsQueryable());

            var handler = new GetCertificateForSigningHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object);
            var result = handler.Execute();

            Assert.IsNotNull(result);
        }
    }
}