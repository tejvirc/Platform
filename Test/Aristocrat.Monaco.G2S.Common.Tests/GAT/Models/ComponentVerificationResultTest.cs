namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.Models
{
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ComponentVerificationResultTest
    {
        [TestMethod]
        public void WhenConstructWithArgsExpectValidPropertiesSet()
        {
            var componentId = "componentId";
            var componentVerificationState = ComponentVerificationState.Complete;
            var gatExec = "gatExec";
            var verifyResult = "verifyResult";

            var result = new ComponentVerificationResult(
                componentId,
                componentVerificationState,
                gatExec,
                verifyResult);

            Assert.AreEqual(componentId, result.ComponentId);
            Assert.AreEqual(componentVerificationState, result.State);
            Assert.AreEqual(gatExec, result.GatExec);
            Assert.AreEqual(verifyResult, result.VerifyResult);
        }
    }
}