namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Cryptography.X509Certificates;
    using Common.CertificateManager.CaClients.Ocsp;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Common.CertificateManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Org.BouncyCastle.Security.Certificates;

    [TestClass]
    public class GetCertificateStatusHandlerTests : BaseCertificateHandlerTests
    {
        private const string CertificatePassword = "test";

        private readonly Mock<OcspCertificateClient> _ocspCertificateServiceMock =
            new Mock<OcspCertificateClient>(
                new Mock<IMonacoContextFactory>().Object,
                new Mock<ICertificateRepository>().Object,
                new Mock<IPkiConfigurationRepository>().Object);

        private readonly Mock<IPkiConfigurationRepository> _pkiConfigurationRepositoryMock =
            new Mock<IPkiConfigurationRepository>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var handler = new GetCertificateStatusHandler(
                null,
                CertificateRepositoryMock.Object,
                _pkiConfigurationRepositoryMock.Object,
                _ocspCertificateServiceMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateRepositoryExpectException()
        {
            var handler = new GetCertificateStatusHandler(
                ContextFactoryMock.Object,
                null,
                _pkiConfigurationRepositoryMock.Object,
                _ocspCertificateServiceMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateConfigurationRepositoryExpectException()
        {
            var handler = new GetCertificateStatusHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                null,
                _ocspCertificateServiceMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPkiConfigurationRepositoryExpectException()
        {
            var handler = new GetCertificateStatusHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                _pkiConfigurationRepositoryMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(CertificateException))]
        public void ReturnNotExistsStatusIfCertificateNotExistsInDataBase()
        {
            CertificateRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(new List<Certificate>().AsQueryable());

            var handler = CreateGetCertificateStatusHandler();

            handler.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CertificateException))]
        public void ReturnNotExistsStatusIfCertificateNotExistsInLocalStore()
        {
            CertificateRepositoryMock
                .Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate(1)
                        {
                            Thumbprint = "123"
                        }
                    }.AsQueryable());

            var handler = CreateGetCertificateStatusHandler();

            handler.Execute();
        }

        [TestMethod]
        public void ReturnExpiredStatusTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("a-b-c", DateTime.UtcNow.AddDays(-1));

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword
                        }
                    }.AsQueryable());

            var handler = CreateGetCertificateStatusHandler();

            var result = handler.Execute();

            Assert.IsTrue((result.Status == CertificateStatus.Expired));
        }

        [TestMethod]
        public void Return_Good_If_OcspOffline_And_OfflineTimePassed_Less_OcspPeriodForOfflineMin()
        {
            var certificate = InstallNotExpiredCertificate();

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword,
                            VerificationDate = DateTime.UtcNow.AddDays(-1)
                        }
                    }.AsQueryable());

            ConfigureCertificateConfigurationRepository();

            _ocspCertificateServiceMock.Setup(x => x.GetStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>()))
                .Returns(new GetOcspCertificateStatusResult(OcspServiceStatus.Offline, OcspCertificateStatus.Good));

            var handler = CreateGetCertificateStatusHandler();
            var result = handler.Execute();

            //check save SaveOcspOfflineDate
            CertificateRepositoryMock.Verify(x => x.Update(It.IsAny<DbContext>(), It.IsAny<Certificate>()));

            Assert.IsTrue(result.Status == CertificateStatus.Good);
        }

        [TestMethod]
        public void ReturnRevoked_If_OcspOffline_And_OfflineTimePassed_NotLess_OcspPeriodForOfflineMin()
        {
            var certificate = InstallNotExpiredCertificate();

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword,
                            VerificationDate =
                                DateTime.UtcNow.AddDays(-1),
                            OcspOfflineDate = DateTime.UtcNow.AddDays(-1)
                        }
                    }.AsQueryable());

            ConfigureCertificateConfigurationRepository();

            _ocspCertificateServiceMock.Setup(x => x.GetStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>()))
                .Returns(new GetOcspCertificateStatusResult(OcspServiceStatus.Offline, OcspCertificateStatus.Revoked));

            var handler = CreateGetCertificateStatusHandler();
            var result = handler.Execute();

            Assert.IsTrue(result.Status == CertificateStatus.Revoked);
        }

        [TestMethod]
        public void ReturnRevoked_If_OcspOffline_And_OfflineTimePassed_NotLess_OcspOfflineDate_And_OptionB()
        {
            var certificate = InstallNotExpiredCertificate();

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword,
                            VerificationDate =
                                DateTime.UtcNow.AddDays(-1),
                            OcspOfflineDate = DateTime.UtcNow.AddDays(-1)
                        }
                    }.AsQueryable());

            _pkiConfigurationRepositoryMock
                .Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(new PkiConfiguration(1) { OfflineMethod = OfflineMethodType.OptionB, OcspEnabled = true});

            _ocspCertificateServiceMock.Setup(x => x.GetStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>()))
                .Returns(new GetOcspCertificateStatusResult(OcspServiceStatus.Offline, OcspCertificateStatus.Revoked));

            var handler = CreateGetCertificateStatusHandler();
            var result = handler.Execute();

            Assert.IsTrue((result.Status == CertificateStatus.Revoked));
        }

        [TestMethod]
        public void ReturnGood_If_OcspOffline_And_OfflineTimePassed_NotLess_OcspOfflineDate_And_OptionB()
        {
            var certificate = InstallNotExpiredCertificate();

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword,
                            VerificationDate =
                                DateTime.UtcNow.AddDays(-1),
                            OcspOfflineDate =
                                DateTime.UtcNow.AddMinutes(-100)
                        }
                    }.AsQueryable());

            _pkiConfigurationRepositoryMock
                .Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(
                    new PkiConfiguration(1)
                    {
                        OfflineMethod = OfflineMethodType.OptionB
                    });

            _ocspCertificateServiceMock.Setup(x => x.GetStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>()))
                .Returns(new GetOcspCertificateStatusResult(OcspServiceStatus.Offline, OcspCertificateStatus.Good));

            var handler = CreateGetCertificateStatusHandler();
            var result = handler.Execute();

            Assert.IsTrue(
                (result.Status == CertificateStatus.Good));
        }

        [TestMethod]
        public void ReturnReceivedStatus_If_OcspOnline()
        {
            var certificate = InstallNotExpiredCertificate();

            CertificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(
                    new List<Certificate>
                    {
                        new Certificate
                        {
                            Thumbprint = certificate.Thumbprint,
                            Data = certificate.Export(X509ContentType.Pkcs12, CertificatePassword),
                            Password = CertificatePassword,
                            VerificationDate =
                                DateTime.UtcNow.AddDays(-1)
                        }
                    }.AsQueryable());

            ConfigureCertificateConfigurationRepository();

            _ocspCertificateServiceMock.Setup(x => x.GetStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<X509Certificate2>()))
                .Returns(new GetOcspCertificateStatusResult(OcspServiceStatus.Online, OcspCertificateStatus.Good));

            var handler = CreateGetCertificateStatusHandler();
            var result = handler.Execute();

            CertificateRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.IsAny<DbContext>(),
                        It.Is<Certificate>(
                            cer => (cer.OcspOfflineDate == null) &&
                                   (cer.VerificationDate.Date == DateTime.UtcNow.Date))));

            Assert.IsTrue((result.Status == CertificateStatus.Good));
        }

        private void ConfigureCertificateConfigurationRepository()
        {
            _pkiConfigurationRepositoryMock.Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(new PkiConfiguration(1) { OcspEnabled = true});
        }

        private X509Certificate2 InstallNotExpiredCertificate()
        {
            var certificate = CertificateHelper.GenerateCertificate("a-b-c", DateTime.UtcNow.AddDays(5));

            return certificate;
        }

        private GetCertificateStatusHandler CreateGetCertificateStatusHandler()
        {
            var handler = new GetCertificateStatusHandler(
                ContextFactoryMock.Object,
                CertificateRepositoryMock.Object,
                _pkiConfigurationRepositoryMock.Object,
                _ocspCertificateServiceMock.Object);

            return handler;
        }
    }
}