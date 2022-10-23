namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Cryptography;
    using System.Text;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Common.CertificateManager.Storage;
    using Common.CertificateManager.Validators;
    using FluentValidation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CertificateServiceTests
    {
        private readonly Mock<IPkiConfigurationRepository> _certificateConfigurationRepositoryMock =
            new Mock<IPkiConfigurationRepository>();

        private readonly Mock<ICertificateRepository> _certificateRepositoryMock = new Mock<ICertificateRepository>();

        private readonly IValidator<Certificate> _certificateValidator = new CertificateValidator();

        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();

        private IValidator<PkiConfiguration> _pkiConfigurationValidator;

        [TestInitialize]
        public void SetUp()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _pkiConfigurationValidator = new PkiConfigurationValidator();

            _certificateConfigurationRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<PkiConfiguration>
                    {
                        new PkiConfiguration(1)
                        {
                            CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                            CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                            ScepUsername = "name",
                            OcspAcceptPreviouslyGoodCertificatePeriod = 601
                        }
                    }.AsQueryable());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var service = new CertificateService(
                null,
                null,
                null,
                null,
                null);
            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateConfigurationRepositryExpectException()
        {
            var service = new CertificateService(
                _contextFactoryMock.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateRepositryExpectException()
        {
            var service = new CertificateService(
                _contextFactoryMock.Object,
                _certificateConfigurationRepositoryMock.Object,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCertificateValidatorExpectException()
        {
            var service = new CertificateService(
                _contextFactoryMock.Object,
                _certificateConfigurationRepositoryMock.Object,
                _certificateRepositoryMock.Object,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPkiConfigurationValidatorExpectException()
        {
            var service = new CertificateService(
                _contextFactoryMock.Object,
                _certificateConfigurationRepositoryMock.Object,
                _certificateRepositoryMock.Object,
                _certificateValidator,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void GetCaCertificateThumbprintTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("aaa-bbb-ccc");

            var sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
            var hash = sha1CryptoServiceProvider.ComputeHash(certificate.GetRawCertData());

            var hashHexadecimalViewBuilder = new StringBuilder();
            foreach (var b in hash)
            {
                var hex = b.ToString("x2").ToUpper();
                hashHexadecimalViewBuilder.Append(hex);
            }

            Assert.IsTrue(string.CompareOrdinal(certificate.Thumbprint, hashHexadecimalViewBuilder.ToString()) == 0);
        }

        [TestMethod]
        public void RemoveCertificateReturnFalseTest()
        {
            SetReturnEmptyCertificatesList();

            var certificateService = GetCertificateService();

            var certificate = CertificateHelper.GenerateCertificate("aaa-bbb-ccc");

            var result = certificateService.RemoveCertificate(certificate);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RemoveCertificateReturnTrueTest()
        {
            var certificate = CertificateHelper.GenerateCertificate("aaa-bbb-ccc");

            _certificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>())).Returns(
                    new List<Certificate>
                    {
                        new Certificate(1)
                        {
                            Thumbprint = certificate.Thumbprint,
                            VerificationDate = DateTime.UtcNow
                        }
                    }.AsQueryable());

            var certificateService = GetCertificateService();

            var result = certificateService.RemoveCertificate(certificate);

            _certificateRepositoryMock.Verify(
                x => x.Delete(It.IsAny<DbContext>(), It.IsAny<Certificate>()),
                Times.Once);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void InstallCertificateTest()
        {
            SetReturnEmptyCertificatesList();

            _certificateConfigurationRepositoryMock.Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(new PkiConfiguration(1));

            var certificate = CertificateHelper.GenerateCertificate("aaa-bbb-ccc");

            var certificateService = GetCertificateService();

            certificateService.InstallCertificate(certificate);

            _certificateRepositoryMock.Verify(x => x.Add(It.IsAny<DbContext>(), It.IsAny<Certificate>()), Times.Once);
        }

        private void SetReturnEmptyCertificatesList()
        {
            _certificateRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<Certificate, bool>>>()))
                .Returns(Enumerable.Empty<Certificate>().AsQueryable());
        }

        private CertificateService GetCertificateService()
        {
            return new CertificateService(
                _contextFactoryMock.Object,
                _certificateConfigurationRepositoryMock.Object,
                _certificateRepositoryMock.Object,
                _certificateValidator,
                _pkiConfigurationValidator);
        }
    }
}