namespace Aristocrat.Monaco.G2S.Common.Tests.CertificateManager
{
    using System.IO;
    using Common.CertificateManager;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CertificateFactoryTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IMonacoContextFactory>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestMethod]
        public void WhenGetCertificateServiceExpectSuccess()
        {
            var certificateFactory = new CertificateFactory();
            certificateFactory.Initialize();

            var service = certificateFactory.GetCertificateService();

            Assert.IsNotNull(service);
        }
    }
}