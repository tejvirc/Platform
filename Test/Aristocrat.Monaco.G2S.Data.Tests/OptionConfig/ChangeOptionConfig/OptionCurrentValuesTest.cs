namespace Aristocrat.Monaco.G2S.Data.Tests.OptionConfig.ChangeOptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.OptionConfig.ChangeOptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OptionCurrentValuesTest : OptionConfigTestBase
    {
        [TestMethod]
        public void WhenEqualsWithSameReferenceExpectTrue()
        {
            var item = CreateOptionCurrentValue();
            var itemSameReference = item;

            Assert.IsTrue(item.Equals(itemSameReference));
        }

        [TestMethod]
        public void WhenEqualsWithNullExpectFalse()
        {
            var item = CreateOptionCurrentValue();
            Assert.IsFalse(item.Equals(null));
        }

        [TestMethod]
        public void WhenEqualsWithDifferentTypeExpectFalse()
        {
            var item = CreateOptionCurrentValue();
            Assert.IsFalse(item.Equals(new object()));
        }

        [TestMethod]
        public void WhenEqualsWithSameItemTypeExpectTrue()
        {
            var item = CreateOptionCurrentValue();
            var sameItem = CreateOptionCurrentValue();

            Assert.IsTrue(item.Equals(sameItem));
        }

        [TestMethod]
        public void WhenEqualsWithChildValuesExpectValidResults()
        {
            var firstItem = CreateOptionCurrentValue();
            var secondItem = CreateOptionCurrentValue();

            firstItem.ChildValues = null;
            secondItem.ChildValues = new List<OptionCurrentValue>();

            Assert.IsFalse(firstItem.Equals(secondItem));

            firstItem.ChildValues = new List<OptionCurrentValue>();
            secondItem.ChildValues = null;

            Assert.IsFalse(firstItem.Equals(secondItem));

            firstItem.ChildValues = new List<OptionCurrentValue>
            {
                CreateOptionCurrentValue(),
                CreateOptionCurrentValue()
            };
            secondItem.ChildValues = new List<OptionCurrentValue>
            {
                CreateOptionCurrentValue(),
                CreateOptionCurrentValue()
            };

            Assert.IsTrue(firstItem.Equals(secondItem));

            secondItem.ChildValues.First().Value += 1;

            Assert.IsFalse(firstItem.Equals(secondItem));
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectEqualHash()
        {
            var item = CreateOptionCurrentValue();
            var sameItem = CreateOptionCurrentValue();

            Assert.IsTrue(item.GetHashCode() == sameItem.GetHashCode());
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectNotEqualHash()
        {
            var item = CreateOptionCurrentValue();
            var anotherItem = CreateOptionCurrentValue();
            anotherItem.Value += 1;

            Assert.IsTrue(item.GetHashCode() == anotherItem.GetHashCode());
        }
    }
}