namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for TransactionHistoryProviderExtensionNodeTest
    /// </summary>
    [TestClass]
    public class TransactionHistoryProviderExtensionNodeTest
    {
        private dynamic _accessor;
        private TransactionHistoryProviderExtensionNode _target;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new TransactionHistoryProviderExtensionNode();
            _accessor = new DynamicPrivateObject(_target);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void MaxTransactionsTest()
        {
            _accessor._maxTransactions = "123";

            Assert.AreEqual(123, _target.MaxTransactions);
        }

        [TestMethod]
        public void LevelTest()
        {
            _accessor._persistenceLevel = "Critical";

            Assert.AreEqual(PersistenceLevel.Critical, _target.Level);
        }

        [TestMethod]
        public void IsPrintableTest()
        {
            _accessor._isPrintable = true;
            Assert.IsTrue(_target.IsPrintable);

            _accessor._isPrintable = false;
            Assert.IsFalse(_target.IsPrintable);
        }
    }
}