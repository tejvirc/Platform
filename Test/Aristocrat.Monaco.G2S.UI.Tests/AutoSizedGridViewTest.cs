namespace Aristocrat.Monaco.G2S.UI.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Windows.Controls;
    using Test.Common;
    using Views;

    [TestClass]
    public class AutoSizedGridViewTest
    {
        [RequireSTA]
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new AutoSizedGridView();
            Assert.IsNotNull(target);
        }

        [RequireSTA]
        [TestMethod]
        public void PrepareItemTest()
        {
            var target = new AutoSizedGridView();
            var column = new GridViewColumn();
            target.Columns.Add(column);
            var item = new ListViewItem();

            dynamic accessor = new DynamicPrivateObject(target);
            accessor.PrepareItem(item);

            Assert.AreEqual(double.NaN, target.Columns[0].Width);
        }
    }
}