namespace Aristocrat.Monaco.G2S.Data.Tests.OptionConfig.ChangeOptionConfig
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OptionTest : OptionConfigTestBase
    {
        [TestMethod]
        public void WhenEqualsWithSameReferenceExpectTrue()
        {
            var item = CreateOption();
            var itemSameReference = item;

            Assert.IsTrue(item.Equals(itemSameReference));
        }

        [TestMethod]
        public void WhenEqualsWithNullExpectFalse()
        {
            var item = CreateOption();
            Assert.IsFalse(item.Equals(null));
        }

        [TestMethod]
        public void WhenEqualsWithDifferentTypeExpectFalse()
        {
            var item = CreateOption();
            Assert.IsFalse(item.Equals(new object()));
        }

        [TestMethod]
        public void WhenEqualsWithSameItemTypeExpectTrue()
        {
            var item = CreateOption();
            var sameItem = CreateOption();

            Assert.IsTrue(item.Equals(sameItem));
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectEqualHash()
        {
            var item = CreateOption();
            var sameItem = CreateOption();

            Assert.IsTrue(item.GetHashCode() == sameItem.GetHashCode());
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectNotEqualHash()
        {
            var item = CreateOption();
            var anotherItem = CreateOption();
            anotherItem.OptionValues.First().Value += 1;

            Assert.IsTrue(item.GetHashCode() != anotherItem.GetHashCode());
        }
    }
}