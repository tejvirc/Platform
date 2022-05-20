namespace Aristocrat.Monaco.Gaming.UI.Tests.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using UI.Models;

    [TestClass]
    public class PlayInfoDisplayResourcesModelTests
    {
        private PlayInfoDisplayResourcesModel _underTest;

        [TestInitialize]
        public void Setup()
        {
            _underTest = new PlayInfoDisplayResourcesModel();
        }

        [TestMethod]
        public void GivenBackgroundsWhenGetByTagThenResourceReturned()
        {
            _underTest.ScreenBackgrounds = new (HashSet<string> Tags, string FilePath)[] { (new HashSet<string>() {"bla", "bla2", "bla3"}, "blaPath") };
            var result = _underTest.GetScreenBackground(new HashSet<string>() {"bla", "bla2"});

            Assert.AreEqual("blaPath", result);
        }

        [TestMethod]
        public void GivenButtonsWhenGetByTagThenResourceReturned()
        {
            _underTest.Buttons = new (HashSet<string> Tags, string FilePath)[] { (new HashSet<string>() {"bla", "bla2", "bla3"}, "blaPath") };
            var result = _underTest.GetButton(new HashSet<string>() {"bla", "bla2"});

            Assert.AreEqual("blaPath", result);
        }

        [TestMethod]
        public void GivenNoButtonsBackgroundsWhenGetButtonsByTagThenNull()
        {
            _underTest.ScreenBackgrounds = new (HashSet<string> Tags, string FilePath)[] { (new HashSet<string>() {"bla", "bla2", "bla3"}, "blaPath") };
            var result = _underTest.GetButton(new HashSet<string>() {"bla", "bla2"});

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GivenNoBackgroundsButtonsWhenGetByBackgroundTagThenNull()
        {
            _underTest.Buttons = new (HashSet<string> Tags, string FilePath)[] { (new HashSet<string>() {"bla", "bla2", "bla3"}, "blaPath") };
            var result = _underTest.GetScreenBackground(new HashSet<string>() {"bla", "bla2"});

            Assert.IsNull(result);
        }

    }
}