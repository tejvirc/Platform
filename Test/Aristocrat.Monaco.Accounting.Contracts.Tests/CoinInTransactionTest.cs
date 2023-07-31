namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Accounting.Contracts.CoinAcceptor;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for CoinTransactionTest
    /// </summary>
    [TestClass]
    public class CoinInTransactionTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            int details = (int)CoinInDetails.CoinToCashBox;
            int exception = (int)CurrencyInExceptionCode.None;

            var target = new CoinInTransaction(
                deviceId,
                transactionDateTime,
                details,
                exception);

            Assert.IsNotNull(target);
            Assert.AreEqual(deviceId, target.DeviceId);
            Assert.AreEqual(transactionDateTime, target.TransactionDateTime);
            Assert.AreEqual(details, target.Details);
            Assert.AreEqual(exception, target.Exception);
        }

        [TestMethod]
        public void EqualityTests()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            int details = (int)CoinInDetails.None;
            int exception = (int)CurrencyInExceptionCode.None;

            var target1 = new CoinInTransaction(
                deviceId,
                transactionDateTime,
                details,
                exception);
            var target2 = new CoinInTransaction(
                deviceId,
                transactionDateTime,
                details,
                exception);

            // test == operator
#pragma warning disable CS1718 // Comparison made to same variable; did you mean to compare something else?
            Assert.IsTrue(target1 == target1); // this is testing reference equality
#pragma warning restore CS1718 // Comparison made to same variable; did you mean to compare something else?
            Assert.IsTrue(target1 == target2);
            Assert.IsFalse(target1 == null);
            Assert.IsFalse(null == target1);

            // test != operator
            Assert.IsFalse(target1 != target2);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            int details = (int)CoinInDetails.None;
            int exception = (int)CurrencyInExceptionCode.None;

            var target = new CoinInTransaction(
                deviceId,
                transactionDateTime,
                details,
                exception);

            var expected =
                "Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor.CoinInTransaction [DeviceId=1, LogSequence=0, DateTime=12/31/9999 23:59:59, TransactionId=0, TypeOfAccount=Cashable, Details=None, Exception=Accepted]";
            Assert.AreEqual(expected, target.ToString());
        }

        [TestMethod]
        public void CloneTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            int details = (int)CoinInDetails.None;
            int exception = (int)CurrencyInExceptionCode.None;

            var target = new CoinInTransaction(
                deviceId,
                transactionDateTime,
                details,
                exception);
            var cloned = (CoinInTransaction)target.Clone();

            // use == to test for equality since it checks member variables
            Assert.IsTrue(target == cloned);
        }

        [TestMethod]
        public void ReceivePersistenceHappyPathTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 123;
            long expectedTransactionId = 1;
            int details = (int)CoinInDetails.None;
            int exception = (int)CurrencyInExceptionCode.None;

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "Accepted", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Details", details },
                { "TypeOfAccount", (byte)0 },
                { "Exception", exception }
            };

            var target = new CoinInTransaction(
                expectedDeviceId,
                DateTime.MaxValue,
                details,
                exception);

            Assert.IsTrue(target.SetData(values));
        }

        [TestMethod]
        public void ReceivePersistenceBadLogSequenceTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 0;
            long expectedTransactionId = 1;
            int details = (int)CoinInDetails.None;
            int exception = (int)CurrencyInExceptionCode.None;

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "Accepted", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Details", details },
                { "TypeOfAccount", (byte)0 },
                { "Exception", exception }
            };

            var target = new CoinInTransaction(
                expectedDeviceId,
                DateTime.MaxValue,
                (int)CoinInDetails.CoinToCashBox,
                (int)CurrencyInExceptionCode.None);

            Assert.IsFalse(target.SetData(values));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivePersistenceElementErrorTest()
        {
            var target = new CoinInTransaction(
                1,
                DateTime.MaxValue,
                (int)CoinInDetails.None,
                (int)CurrencyInExceptionCode.None);

            Assert.IsFalse(target.SetData(new Dictionary<string, object>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPersistenceElementErrorTest()
        {
            var element = 10;
            var storageAccessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            storageAccessor.Setup(m => m.Count).Returns(3);

            var target = new CoinInTransaction(
                1,
                DateTime.MaxValue,
                (int)CoinInDetails.CoinToCashBox,
                (int)CurrencyInExceptionCode.None);
            target.SetPersistence(storageAccessor.Object, element);

            storageAccessor.Verify();
        }
    }
}
