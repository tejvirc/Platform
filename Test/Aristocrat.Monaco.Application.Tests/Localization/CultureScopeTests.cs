namespace Aristocrat.Monaco.Application.Tests.Localization
{
    using System.Globalization;
    using System.Threading;
    using Contracts.Localization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CultureScopeTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void LocalizeTest()
        {
            const string expected = "Test";
            string actual;

            using (var scope = new CultureScope("Operator"))
            {
                actual = scope.GetString("TestText");
            }

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LocalizeWhenCurrentCultureDifferentTest()
        {
            MockLocalization.Service.SetupGet(x => x.CurrentCulture)
                .Returns(CultureInfo.GetCultureInfo("fr-CA"));

            const string expected = "Test";

            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");

                string actual;
                using (var scope = new CultureScope("Operator"))
                {
                    actual = scope.GetString("TestText");
                }

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void ScopeThreadCultureChangedTest()
        {
            MockLocalization.Service.SetupGet(x => x.CurrentCulture)
                .Returns(CultureInfo.GetCultureInfo("fr-CA"));

            var expected = new CultureInfo("en-US");

            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");

                CultureInfo actual;
                using (var scope = new CultureScope("Operator"))
                {
                    scope.GetString("TestText");
                    actual = Thread.CurrentThread.CurrentCulture;
                }

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = defaultCulture;
            }
        }

        [TestMethod]
        public void CurrentCultureRestoredTest()
        {
            MockLocalization.Service.SetupGet(x => x.CurrentCulture)
                .Returns(CultureInfo.GetCultureInfo("fr-CA"));

            var expected = CultureInfo.GetCultureInfo("fr-CA");

            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                Thread.CurrentThread.CurrentCulture = expected;

                using (var scope = new CultureScope("Operator"))
                {
                    scope.GetString("TestText");
                }

                var actual = Thread.CurrentThread.CurrentCulture;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = defaultCulture;
            }
        }
    }
}
