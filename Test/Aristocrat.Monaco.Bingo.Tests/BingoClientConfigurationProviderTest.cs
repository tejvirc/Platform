namespace Aristocrat.Monaco.Bingo.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Bingo.Services.Security;
    using Common.Storage.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class BingoClientConfigurationProviderTest
    {
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<ICertificateService> _certificateService = new(MockBehavior.Default);
        private BingoClientConfigurationProvider _target;

        private static IEnumerable<object[]> ConfigurationTestData => new List<object[]>
        {
            new object[]
            {
                443,
                "testHost",
                Array.Empty<X509Certificate2>(),
                false
            },
            new object[]
            {
                234,
                "testHost2",
                new X509Certificate2[]
                {
                    new(new byte[1]{ 0 })
                },
                true
            }
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(
            bool nullUnitOfWork,
            bool nullCertificateService)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(nullUnitOfWork, nullCertificateService));
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        public void NullDataTest(bool nullHost, bool nullCertificates)
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, Host>>()))
                .Returns(nullHost ? null : new Host());
            _certificateService.Setup(x => x.GetCertificates())
                .Returns(nullCertificates ? null : Enumerable.Empty<X509Certificate2>());
            Assert.ThrowsException<ArgumentNullException>(() => _ = _target.CreateConfiguration());
        }

        [DynamicData(nameof(ConfigurationTestData))]
        [DataTestMethod]
        public void ConfigurationTest(int port, string hostName, X509Certificate2[] certs, bool isHttps)
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, Host>>()))
                .Returns(new Host { HostName = hostName, Port = port });
            _certificateService.Setup(x => x.GetCertificates())
                .Returns(certs);
            var configuration = _target.CreateConfiguration();
            CollectionAssert.AreEquivalent(configuration.Certificates.ToArray(), certs);
            Assert.AreEqual(configuration.Address.Host.ToLowerInvariant(), hostName.ToLowerInvariant());
            Assert.AreEqual(configuration.Address.Port, port);
            Assert.AreEqual(configuration.Address.Scheme, isHttps ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);
        }

        private BingoClientConfigurationProvider CreateTarget(
            bool nullUnitOfWork = false,
            bool nullCertificateService = false)

        {
            return new BingoClientConfigurationProvider(
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullCertificateService ? null : _certificateService.Object);
        }
    }
}