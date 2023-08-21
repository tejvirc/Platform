namespace Aristocrat.Monaco.G2S.Data.Tests.OptionConfig.ChangeOptionConfig
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ChangeOptionConfigRequestTest : OptionConfigTestBase
    {
        [TestMethod]
        public void WhenEqualsWithSameReferenceExpectTrue()
        {
            var item = CreateChangeOptionConfigRequest();
            var itemSameReference = item;

            Assert.IsTrue(item.Equals(itemSameReference));
        }

        [TestMethod]
        public void WhenEqualsWithNullExpectFalse()
        {
            var item = CreateChangeOptionConfigRequest();
            Assert.IsFalse(item.Equals(null));
        }

        [TestMethod]
        public void WhenEqualsWithDifferentTypeExpectFalse()
        {
            var item = CreateChangeOptionConfigRequest();
            Assert.IsFalse(item.Equals(new object()));
        }

        [TestMethod]
        public void WhenEqualsWithSameItemTypeExpectTrue()
        {
            var item = CreateChangeOptionConfigRequest();
            var sameItem = CreateChangeOptionConfigRequest();

            Assert.IsTrue(item.Equals(sameItem));
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectEqualHash()
        {
            var item = CreateChangeOptionConfigRequest();
            var sameItem = CreateChangeOptionConfigRequest();

            Assert.IsTrue(item.GetHashCode() == sameItem.GetHashCode());
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectNotEqualHash()
        {
            var item = CreateChangeOptionConfigRequest();
            var anotherItem = CreateChangeOptionConfigRequest();
            anotherItem.ConfigurationId += 1;

            Assert.IsTrue(item.GetHashCode() == anotherItem.GetHashCode());
        }
    }
}