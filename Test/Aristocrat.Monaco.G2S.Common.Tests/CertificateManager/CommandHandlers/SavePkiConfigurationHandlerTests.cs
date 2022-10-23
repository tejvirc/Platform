namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager.CommandHandlers
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Common.CertificateManager.CommandHandlers;
    using Common.CertificateManager.Models;
    using Common.CertificateManager.Validators;
    using FluentValidation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SavePkiConfigurationHandlerTests : BaseCertificateConfigurationHandlerTests
    {
        private static readonly PkiConfiguration[] InvalidCertificateConfigurationEntities =
        {
            //invalid column OcspPeriodForOfflineMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspMinimumPeriodForOffline = -1,
                OcspAcceptPreviouslyGoodCertificatePeriod = 601
            },
            //invalid column OcspPeriodForOfflineMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspMinimumPeriodForOffline = -1,
                OcspAcceptPreviouslyGoodCertificatePeriod = 601
            },
            //invalid column OcspReauthPeriodMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspReAuthenticationPeriod = -1,
                OcspAcceptPreviouslyGoodCertificatePeriod = 601
            },
            //invalid column OcspReauthPeriodMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspReAuthenticationPeriod = -1,
                OcspAcceptPreviouslyGoodCertificatePeriod = 32767
            },
            //invalid column OcspAcceptPrevCertPeriodMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspReAuthenticationPeriod = 1,
                OcspAcceptPreviouslyGoodCertificatePeriod = -1
            },
            //invalid column OcspAcceptPrevCertPeriodMin
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspReAuthenticationPeriod = 1,
                OcspAcceptPreviouslyGoodCertificatePeriod = -1
            },
            //invalid column OcspAcceptPrevCertPeriodMin (Must OcspAcceptPrevCertPeriodMin > OcspReauthPeriodMin)
            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "userName",
                OcspReAuthenticationPeriod = 100,
                OcspAcceptPreviouslyGoodCertificatePeriod = 99
            },

            new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepManualPollingInterval = 2,
                ScepUsername = "userName",
                ScepCaIdent = string.Empty,
                OcspAcceptPreviouslyGoodCertificatePeriod = 601,
                OcspNextUpdate = null
            }
        };

        private IValidator<PkiConfiguration> _pkiConfigurationValidator;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _pkiConfigurationValidator = new PkiConfigurationValidator();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var handler = new SavePkiConfigurationHandler(
                null,
                PkiConfigurationRepositoryMock.Object,
                _pkiConfigurationValidator);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPkiConfigurationRepositoryExpectException()
        {
            var handler = new SavePkiConfigurationHandler(
                ContextFactoryMock.Object,
                null,
                _pkiConfigurationValidator);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPkiConfigurationValidatorExpectException()
        {
            var handler = new SavePkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenExecuteWithNullPkiConfigurationExpectException()
        {
            var handler = new SavePkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object,
                _pkiConfigurationValidator);

            handler.Execute(null);
        }

        [TestMethod]
        public void CheckDefaultValuesPkiConfiguration()
        {
            var pkiConfiguration = new PkiConfiguration();

            Assert.IsTrue(
                (pkiConfiguration.ScepManualPollingInterval == 60)
                && (pkiConfiguration.OcspMinimumPeriodForOffline == 240)
                && (pkiConfiguration.OcspReAuthenticationPeriod == 600)
                && (pkiConfiguration.OcspAcceptPreviouslyGoodCertificatePeriod == 720));
        }

        [TestMethod]
        public void SavePkiConfigurationTest()
        {
            PkiConfigurationRepositoryMock.Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns((PkiConfiguration)null);

            var pkiConfiguration = new PkiConfiguration
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "name",
                OcspAcceptPreviouslyGoodCertificatePeriod = 601
            };

            var handler = new SavePkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object,
                _pkiConfigurationValidator);

            handler.Execute(pkiConfiguration);

            PkiConfigurationRepositoryMock.Verify(
                x => x.Add(It.IsAny<DbContext>(), It.Is<PkiConfiguration>(entity => entity.Id == 0)),
                Times.Once);
        }

        [TestMethod]
        public void UpdatePkiConfigurationTest()
        {
            PkiConfigurationRepositoryMock.Setup(x => x.GetSingle(It.IsAny<DbContext>()))
                .Returns(new PkiConfiguration(5));

            var pkiConfiguration = new PkiConfiguration(5)
            {
                CertificateManagerLocation = "tsu:certmgr.casino.com:8080",
                CertificateStatusLocation = "tsu:certmgr.casino.com:8080",
                ScepUsername = "name",
                OcspAcceptPreviouslyGoodCertificatePeriod = 601
            };

            var handler = new SavePkiConfigurationHandler(
                ContextFactoryMock.Object,
                PkiConfigurationRepositoryMock.Object,
                _pkiConfigurationValidator);

            handler.Execute(pkiConfiguration);

            PkiConfigurationRepositoryMock.Verify(
                x => x.Update(It.IsAny<DbContext>(), It.Is<PkiConfiguration>(entity => entity.Id == 5)),
                Times.Once);
        }
    }
}