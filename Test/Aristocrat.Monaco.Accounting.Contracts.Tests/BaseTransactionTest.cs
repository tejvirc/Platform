namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for BaseTransactionTest
    /// </summary>
    [TestClass]
    public class BaseTransactionTest
    {
        [TestMethod]
        public void EqualityTest()
        {
            const int deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;

            var target1 = new MyTransaction(deviceId, transactionDateTime);
            var target2 = new MyTransaction(deviceId, transactionDateTime);
            var target3 = new MyTransaction(deviceId + 1, transactionDateTime);

            Assert.IsTrue(target1 == target2);
            Assert.IsFalse(target1 == target3);

            Assert.IsTrue(target1.Equals((object)target2));
            Assert.IsFalse(target1.Equals((object)target3));
        }

        [TestMethod]
        public void ToStringTest()
        {
            const int deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;

            var target = new MyTransaction(deviceId, transactionDateTime);

            Assert.IsNotNull(target.ToString());
        }

        public class MyTransaction : BaseTransaction
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="BaseTransaction" /> class.
            /// </summary>
            /// <param name="deviceId">Value to set as DeviceId.</param>
            /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
            public MyTransaction(int deviceId, DateTime transactionDateTime)
                : base(deviceId, transactionDateTime)
            {
            }

            /// <summary>
            ///     Gets the human readable name of the transaction type
            /// </summary>
            public override string Name { get; } = "Test";

            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}