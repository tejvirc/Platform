namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RemoveCertificateHandlerTests : BaseCertificateHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfCertificateRepositoryIsNull()
        {
            var handler = new DeleteCertificateHandler(ContextFactoryMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void RemoveCertificateEntityTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("aaa-bbb-ccc");

            CertificateRepositoryMock.Setup(x => x.Delete(It.IsAny<DbContext>(), It.IsAny<Certificate>()));
            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>())).Returns(
                    new List<Certificate>
                    {
                        new Certificate(1)
                        {
                            Thumbprint = certificate.Thumbprint,
                            VerificationDate = DateTime.UtcNow
                        }
                    }.AsQueryable());

            var handler = new DeleteCertificateHandler(ContextFactoryMock.Object, CertificateRepositoryMock.Object);
            var result = handler.Execute(certificate);

            CertificateRepositoryMock.Verify(x => x.Delete(It.IsAny<DbContext>(), It.IsAny<Certificate>()), Times.Once);

            Assert.IsTrue(result);
        }
    }
}