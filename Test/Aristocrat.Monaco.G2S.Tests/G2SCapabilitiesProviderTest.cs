using System;
using Aristocrat.Monaco.Application.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.G2S.Tests
{
    [TestClass]
    public class G2SCapabilitiesProviderTests
    {
        [TestMethod]
        public void G2SCapabilitiesProviderTest()
        {
            var g2sCapabilitiesProvider =(ProtocolCapabilityAttribute)(Attribute.GetCustomAttribute(
            typeof(G2SBase),
                       typeof(ProtocolCapabilityAttribute)));
            Assert.AreEqual(g2sCapabilitiesProvider.IsValidationSupported, true);
            Assert.AreEqual(g2sCapabilitiesProvider.IsFundTransferSupported, false);
            Assert.AreEqual(g2sCapabilitiesProvider.IsProgressivesSupported, false);
            Assert.AreEqual(g2sCapabilitiesProvider.IsCentralDeterminationSystemSupported, true);
        }
    }
}
