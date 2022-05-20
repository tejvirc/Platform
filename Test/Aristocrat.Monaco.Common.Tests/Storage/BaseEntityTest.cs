namespace Aristocrat.Monaco.Common.Tests.Storage
{
    using Common.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BaseEntityTest
    {
        [TestMethod]
        public void WhenGetSetIdExpectSuccess()
        {
            var entity = new BaseEntity();

            Assert.IsNotNull(entity);

            Assert.AreEqual(entity.Id, 0);

            entity.Id = 1;

            Assert.AreEqual(entity.Id, 1);
        }
    }
}