namespace Aristocrat.Monaco.G2S.Data.Tests.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HashHelperTest
    {
        [TestMethod]
        public void WhenCollectionInNullExpectSeed()
        {
            var seed = 99;
            var hash = HashHelper.GetCollectionHash((IEnumerable<int>)null, seed);

            Assert.AreEqual(seed, hash);
        }

        [TestMethod]
        public void WhenCollectionsTheSameTest()
        {
            var collection = Enumerable.Range(0, 1000);
            var collectionClone = Enumerable.Range(0, 1000);

            Assert.AreEqual(
                HashHelper.GetCollectionHash(collection, 100),
                HashHelper.GetCollectionHash(collectionClone, 100));
            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 101),
                HashHelper.GetCollectionHash(collectionClone, 100));
        }

        [TestMethod]
        public void WhenCollectionsDifferentTest()
        {
            var collection = Enumerable.Range(0, 1000);
            var collectionClone = Enumerable.Range(0, 1001);

            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 100),
                HashHelper.GetCollectionHash(collectionClone, 100));
            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 101),
                HashHelper.GetCollectionHash(collectionClone, 100));
        }

        [TestMethod]
        public void WhenPrimitiveTypeCollectionsTheSameTest()
        {
            var collection = Enumerable.Range(0, 1000);
            var collectionClone = Enumerable.Range(0, 1000);

            Assert.AreEqual(
                HashHelper.GetCollectionHash(collection, 100),
                HashHelper.GetCollectionHash(collectionClone, 100));
            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 101),
                HashHelper.GetCollectionHash(collectionClone, 100));
        }

        [TestMethod]
        public void WhenPrimitiveTypeCollectionsDifferentTest()
        {
            var collection = Enumerable.Range(0, 1000);
            var collectionClone = Enumerable.Range(0, 1001);

            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 100),
                HashHelper.GetCollectionHash(collectionClone, 100));
            Assert.AreNotEqual(
                HashHelper.GetCollectionHash(collection, 101),
                HashHelper.GetCollectionHash(collectionClone, 100));
        }
    }
}