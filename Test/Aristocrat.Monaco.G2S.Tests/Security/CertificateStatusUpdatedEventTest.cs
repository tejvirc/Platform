namespace Aristocrat.Monaco.G2S.Tests.Security
{
    using Common.CertificateManager;
    using G2S.Security;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CertificateStatusUpdatedEventTest
    {
        [TestMethod]
        public void WhenConstructWithParamsExpectValidPropertiesSet()
        {
            var certificateStatus = (CertificateRequestStatus)1000;
            var evt = new CertificateStatusUpdatedEvent(certificateStatus);

            Assert.AreEqual(certificateStatus, evt.Status);
        }
    }
}