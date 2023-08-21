namespace Aristocrat.Monaco.Localization.Tests
{
    using System.Globalization;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LocalizationManagerTests
    {
        private const string ResourceDictionary = "Resources";
        private const string OverrideDictionary = "Overrides";
        private Mock<ILocalizationClient> _client;
        private ILocalizationManager _target;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = new Mock<ILocalizationClient>(MockBehavior.Loose);

            _target = new LocalizationManager(_client.Object);

            var assembly = Assembly.GetExecutingAssembly();

            _target.Start(assembly, ResourceDictionary);

            _target.LoadResources(assembly, OverrideDictionary);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void LocalizeTest()
        {
            const string expected = "This is a test";
            var actual = _target.GetObject<string>("Test", CultureInfo.GetCultureInfo("en-US"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LocalizeSetCurrentCultureTest()
        {
            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = CultureInfo.GetCultureInfo("es");

                const string expected = "Esto es una prueba";
                var actual = _target.GetObject<string>("Test");

                Assert.AreEqual(expected, actual);

            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void OverrideTest()
        {
            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

                const string expected = "This is an override";
                var actual = _target.GetObject<string>("Override");

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void SetCurrentCultureTest()
        {
            var expected = CultureInfo.GetCultureInfo("es");

            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = expected;

                var actual = _target.CurrentCulture;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void DifferentLanguageTest()
        {
            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = CultureInfo.GetCultureInfo("es");

                const string expected = "Esto es una prueba";
                var actual = _target.GetObject<string>("Test");

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void DifferentLanguageOverrideTest()
        {
            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");

                const string expected = "Esto es una anulación";
                var actual = _target.GetObject<string>("Override");

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }
    }
}
