
namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aristocrat.Monaco.Application.Contracts;
    using System;

    [TestClass]
    public class MgamCapabilitiesProviderTests
    {
        
        [TestMethod]
        public void MgamCapabilitiesProviderTest()
        {
            var mgamCapabilitiesProvider = (ProtocolCapabilityAttribute)(Attribute.GetCustomAttribute(
                       typeof(MgamBase),
                       typeof(ProtocolCapabilityAttribute)));
            Assert.AreEqual(mgamCapabilitiesProvider.IsValidationSupported, true);
            Assert.AreEqual(mgamCapabilitiesProvider.IsFundTransferSupported, false);
            Assert.AreEqual(mgamCapabilitiesProvider.IsProgressivesSupported, true);
            Assert.AreEqual(mgamCapabilitiesProvider.IsCentralDeterminationSystemSupported, true);
        }
    }
}
