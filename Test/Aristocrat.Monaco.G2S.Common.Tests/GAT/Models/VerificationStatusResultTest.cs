namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.Models
{
    using System.Collections.Generic;
    using Common.GAT.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VerificationStatusResultTest
    {
        [TestMethod]
        public void WhenConstructWithArgsExpectValidPropertiesSet()
        {
            var verificationCompleted = true;
            var componentVerificationResults = new List<ComponentVerificationResult>();
            var verificationStatus = new VerificationStatus(1, 1, new List<ComponentStatus>());

            var result = new VerificationStatusResult(
                verificationCompleted,
                componentVerificationResults,
                verificationStatus);

            Assert.AreEqual(verificationCompleted, result.VerificationCompleted);
            Assert.AreEqual(componentVerificationResults, result.ComponentVerificationResults);
            Assert.AreEqual(verificationStatus, result.VerificationStatus);
        }
    }
}