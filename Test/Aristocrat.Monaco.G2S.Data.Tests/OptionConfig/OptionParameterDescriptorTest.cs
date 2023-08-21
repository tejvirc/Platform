namespace Aristocrat.Monaco.G2S.Data.Tests.OptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Data.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OptionParameterDescriptorTest : OptionConfigTestBase
    {
        [TestMethod]
        public void WhenEqualsWithSameReferenceExpectTrue()
        {
            var item = CreateOptionParameterDescriptor();
            var itemSameReference = item;

            Assert.IsTrue(item.Equals(itemSameReference));
        }

        [TestMethod]
        public void WhenEqualsWithNullExpectFalse()
        {
            var item = CreateOptionParameterDescriptor();
            Assert.IsFalse(item.Equals(null));
        }

        [TestMethod]
        public void WhenEqualsWithDifferentTypeExpectFalse()
        {
            var item = CreateOptionParameterDescriptor();
            Assert.IsFalse(item.Equals(new object()));
        }

        [TestMethod]
        public void WhenEqualsWithSameItemTypeExpectTrue()
        {
            var item = CreateOptionParameterDescriptor();
            var sameItem = CreateOptionParameterDescriptor();

            Assert.IsTrue(item.Equals(sameItem));
        }

        [TestMethod]
        public void WhenEqualsWithChildValuesExpectValidResults()
        {
            var firstItem = CreateOptionParameterDescriptor();
            var secondItem = CreateOptionParameterDescriptor();

            firstItem.ChildItems = null;
            secondItem.ChildItems = new List<OptionParameterDescriptor>();

            Assert.IsFalse(firstItem.Equals(secondItem));

            firstItem.ChildItems = new List<OptionParameterDescriptor>();
            secondItem.ChildItems = null;

            Assert.IsFalse(firstItem.Equals(secondItem));

            firstItem.ChildItems = null;
            secondItem.ChildItems = null;

            Assert.IsTrue(firstItem.Equals(secondItem));

            firstItem.ChildItems = new List<OptionParameterDescriptor>
            {
                CreateOptionParameterDescriptor(),
                CreateOptionParameterDescriptor()
            };
            secondItem.ChildItems = new List<OptionParameterDescriptor>
            {
                CreateOptionParameterDescriptor(),
                CreateOptionParameterDescriptor()
            };

            Assert.IsTrue(firstItem.Equals(secondItem));

            secondItem.ChildItems.First().ParameterId += 1;

            Assert.IsFalse(firstItem.Equals(secondItem));
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectEqualHash()
        {
            var item = CreateOptionParameterDescriptor();
            var sameItem = CreateOptionParameterDescriptor();

            Assert.IsTrue(item.GetHashCode() == sameItem.GetHashCode());
        }

        [TestMethod]
        public void WhenGetHashCodeWithSameItemsExpectNotEqualHash()
        {
            var item = CreateOptionParameterDescriptor();
            var anotherItem = CreateOptionParameterDescriptor();
            anotherItem.ParameterId += 1;

            Assert.IsTrue(item.GetHashCode() == anotherItem.GetHashCode());
        }
    }
}