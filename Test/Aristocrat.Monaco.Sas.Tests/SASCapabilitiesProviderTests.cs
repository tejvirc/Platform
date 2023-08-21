namespace Aristocrat.Monaco.Sas.Tests
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Sas.Base;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class SASCapabilitiesProviderTests
    {
        [TestMethod]
        public void SASCapabilitiesProviderTest()
        {
            var sasCapabilitiesProvider = (ProtocolCapabilityAttribute)(Attribute.GetCustomAttribute(
                       typeof(SasBase),
                       typeof(ProtocolCapabilityAttribute)));
            Assert.AreEqual(sasCapabilitiesProvider.IsValidationSupported, true);
            Assert.AreEqual(sasCapabilitiesProvider.IsFundTransferSupported, true);
            Assert.AreEqual(sasCapabilitiesProvider.IsProgressivesSupported, true);
            Assert.AreEqual(sasCapabilitiesProvider.IsCentralDeterminationSystemSupported, false);
        }
    }
}
