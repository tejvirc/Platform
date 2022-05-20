namespace Aristocrat.SasClient.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Client;

    /// <summary>
    ///     Contains unit tests for the SasAttribute class
    /// </summary>
    [TestClass]
    public class SasAttributeTests
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new SasAttribute(SasGroup.GeneralControl);

            Assert.IsNotNull(target);
            Assert.AreEqual(SasGroup.GeneralControl, target.Group);
        }
    }
}
