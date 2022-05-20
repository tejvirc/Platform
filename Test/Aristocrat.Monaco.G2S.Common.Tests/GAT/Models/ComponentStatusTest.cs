namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.Models
{
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ComponentStatusTest
    {
        [TestMethod]
        public void WhenConstructWithArgsExpectValidPropertiesSet()
        {
            var componentId = "componentId";
            var componentVerificationState = ComponentVerificationState.Complete;

            var result = new ComponentStatus(componentId, componentVerificationState);

            Assert.AreEqual(componentId, result.ComponentId);
            Assert.AreEqual(componentVerificationState, result.State);
        }
    }
}