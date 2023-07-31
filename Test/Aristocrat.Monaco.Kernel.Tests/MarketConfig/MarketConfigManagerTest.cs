namespace Aristocrat.Monaco.Kernel.Tests.MarketConfig
{
    using System.Reflection;
    using Application.Contracts;
    using Kernel.MarketConfig;
    using Kernel.MarketConfig.Models.BootExtender;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Unit tests covering the MarketConfigManager service.
    /// </summary>
    [TestClass]
    public class MarketConfigManagerTest {
        /// <summary>
        ///     A test for covering the initialization of the market config manager from a directory and de-serializing
        ///     of a config file.
        /// </summary>
        [TestMethod]
        public void GetConfigByJurisdictionTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.ModelObjectAssembly = Assembly.GetExecutingAssembly();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/Default");
            var config = marketConfigManager.GetMarketConfiguration<DefaultTestConfigSegment>("Test Jurisdiction");

            Assert.AreEqual("Reprint", config.RebootWhilePrintingBehavior);
            Assert.AreEqual(CheckCreditsInEnum.None, config.TenderIn.CheckCreditsIn);
        }

        /// <summary>
        ///     A test for covering the error handling for invalid jurisdiction name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void InvalidJurisdictionTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/Default");
            var config = marketConfigManager.GetMarketConfiguration<DefaultTestConfigSegment>("Does Not Exist");
        }

        /// <summary>
        ///     A test for covering the error handling for an invalid segment class
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void InvalidSegmentTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/Default");
            var config = marketConfigManager.GetMarketConfiguration<string>("Test Jurisdiction");
        }

        /// <summary>
        ///     A test for covering the error handling for a missing manifest.json file.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void NoManifestTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/NoManifest");
        }

        /// <summary>
        ///     A test for covering the error handling for invalid manifest.json files.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void InvalidManifestTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/InvalidManifest");
        }

        /// <summary>
        ///     A test for covering the error handling of invalid segment json files.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void InvalidSegmentJsonTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.ModelObjectAssembly = Assembly.GetExecutingAssembly();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/InvalidSegment");
            var config = marketConfigManager.GetMarketConfiguration<SimpleTestConfigSegment>("Test Jurisdiction");
        }

        /// <summary>
        ///     A test for covering the error handling of a missing segment json file.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void InvalidSegmentJsonNotFoundTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.ModelObjectAssembly = Assembly.GetExecutingAssembly();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/SegmentNotFound");
            var accounting = marketConfigManager.GetMarketConfiguration<SimpleTestConfigSegment>("Test Jurisdiction");
        }

        /// <summary>
        ///     A test for covering the error handling of an unknown segment json file.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void UnknownSegmentJsonTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.ModelObjectAssembly = Assembly.GetExecutingAssembly();
            marketConfigManager.InitializeFromDirectory("MarketConfig/TestFixtures/UnknownSegment");
            var accounting = marketConfigManager.GetMarketConfiguration<SimpleTestConfigSegment>("Test Jurisdiction");
        }

        /// <summary>
        ///     A test for covering the error handling of using the service uninitialized.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(MarketConfigException))]
        public void ServiceUninitializedTest()
        {
            var marketConfigManager = new MarketConfigManager();
            marketConfigManager.ModelObjectAssembly = Assembly.GetExecutingAssembly();
            var accounting = marketConfigManager.GetMarketConfiguration<SimpleTestConfigSegment>("Test Jurisdiction");
        }
    }
}