namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.Manifest
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PackageManifest.Ati;

    [TestClass]
    public class ManifestServiceTests
    {
        private string _manifest;
        private ImageManifest _subject;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            _manifest = Path.Combine(
                AppContext.BaseDirectory,
                "PackageManager/TestData/ATI_Wild_Panda_Gold.manifest");

            _subject = new ImageManifest();
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        public void WhenReadExpectSuccess()
        {
            var result = _subject.Read(_manifest);

            Assert.IsNotNull(result);
        }
    }
}
