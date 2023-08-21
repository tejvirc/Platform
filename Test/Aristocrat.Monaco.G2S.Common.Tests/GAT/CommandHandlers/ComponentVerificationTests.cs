namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Common.GAT.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ComponentVerificationTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullComponentIdExpectException()
        {
            var componentVerification = new ComponentVerification(null, true);

            Assert.IsNull(componentVerification);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithEmptyComponentIdExpectException()
        {
            var componentVerification = new ComponentVerification(string.Empty, true);

            Assert.IsNull(componentVerification);
        }

        [TestMethod]
        public void WhenConstructWithArgsExpectValidPropertiesSet()
        {
            var componentId = "componentId";
            var passed = true;

            var componentVerification = new ComponentVerification(componentId, passed);

            Assert.AreEqual(componentId, componentVerification.ComponentId);
            Assert.AreEqual(passed, componentVerification.Passed);
        }
    }
}