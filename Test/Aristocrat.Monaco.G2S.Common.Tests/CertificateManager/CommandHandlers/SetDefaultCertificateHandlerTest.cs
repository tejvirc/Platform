namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SetDefaultCertificateHandlerTest : BaseCertificateHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var handler = new SetDefaultCertificateHandler(
                null,
                CertificateRepositoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateRepositoryExpectException()
        {
            var handler = new SetDefaultCertificateHandler(
                ContextFactoryMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenExecuteThumbprintsEqualExpectCurrentDefault()
        {
            var parameter = CertificateHelper
                .GenerateCertificate("CertificateName");

            var certificate = new Certificate { Thumbprint = parameter.Thumbprint };
            CertificateRepositoryMock
                .Setup(m => m.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(new[] { certificate }.AsQueryable());

            var handler = new SetDefaultCertificateHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object);

            var result = handler.Execute(parameter);

            Assert.AreEqual(parameter.Thumbprint, result.Thumbprint);
        }

        [TestMethod]
        public void WhenExecuteThumbprintsNotEqualExpectUpdates()
        {
            var parameter = CertificateHelper
                .GenerateCertificate("CertificateName");

            var certificate =
                new Certificate { Thumbprint = "699E2CEBC70775B0933B52577KF2C8578BC64D7A", Default = true };
            CertificateRepositoryMock
                .Setup(m => m.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(new[] { certificate }.AsQueryable());

            var handler = new SetDefaultCertificateHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object);

            var result = handler.Execute(parameter);

            Assert.IsTrue(result.Default);
            Assert.AreNotEqual(parameter.Thumbprint, result.Thumbprint);

            CertificateRepositoryMock.Setup(m => m.Update(It.IsAny<DbContext>(), certificate));
            CertificateRepositoryMock.Setup(m => m.Update(It.IsAny<DbContext>(), result));
        }
    }
}