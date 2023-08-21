namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for BillTransactionTest
    /// </summary>
    [TestClass]
    public class BillTransactionTest
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
            char[] currencyId = { 'U', 'S', 'D' };
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 12345;

            var target = new BillTransaction(
                currencyId,
                deviceId,
                transactionDateTime,
                amount);

            Assert.IsNotNull(target);
            Assert.AreEqual(deviceId, target.DeviceId);
            Assert.AreEqual(transactionDateTime, target.TransactionDateTime);
            Assert.AreEqual(amount, target.Amount);
            Assert.AreEqual("USD", target.CurrencyId);
            Assert.AreEqual("Cash In", target.Name);
        }

        [TestMethod]
        public void EqualityTests()
        {
            char[] currencyId = { 'U', 'S', 'D' };
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 12345;

            var target1 = new BillTransaction(
                currencyId,
                deviceId,
                transactionDateTime,
                amount);
            var target2 = new BillTransaction(
                currencyId,
                deviceId,
                transactionDateTime,
                amount);
            var target3 = new BillTransaction(
                new[] { 'A', 'B', 'C' },
                deviceId,
                transactionDateTime,
                amount);

            // test == operator
            Assert.IsTrue(target1 == target2);
            Assert.IsFalse(target1 == target3);
            Assert.IsFalse(target1 == null);
            Assert.IsFalse(null == target1);

            // test != operator
            Assert.IsFalse(target1 != target2);
            Assert.IsTrue(target1 != target3);

            // Equal operator
            Assert.IsFalse(target3.Equals("Test"));
        }

        [TestMethod]
        public void ToStringTest()
        {
            char[] currencyId = { 'U', 'S', 'D' };
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 12345;

            var target = new BillTransaction(
                currencyId,
                deviceId,
                transactionDateTime,
                amount);

            var expected =
                "Aristocrat.Monaco.Accounting.Contracts.BillTransaction [DeviceId=1, LogSequence=0, DateTime=12/31/9999 23:59:59, TransactionId=0, Amount=12345, TypeOfAccount=Cashable, CurrencyId=USD, State=Pending, Exception=Accepted]";
            Assert.AreEqual(expected, target.ToString());
        }

        [TestMethod]
        public void CloneTest()
        {
            char[] currencyId = { 'U', 'S', 'D' };
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 12345;

            var target = new BillTransaction(
                currencyId,
                deviceId,
                transactionDateTime,
                amount);
            var cloned = (BillTransaction)target.Clone();

            // use == to test for equality since it checks member variables
            Assert.IsTrue(target == cloned);
        }

        [TestMethod]
        public void ReceivePersistenceHappyPathTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 123;
            long expectedTransactionId = 1;
            long expectedAmount = 9999;
            byte[] currencyId = { (byte)'U', (byte)'S', (byte)'D' };

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "Accepted", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Amount", expectedAmount },
                { "TypeOfAccount", (byte)0 },
                { "CurrencyId", currencyId },
                { "Denomination", 0L },
                { "BankTransactionId", Guid.Empty },
                { "State", 0 },
                { "Exception", 0 }
            };

            var target = new BillTransaction(
                new[] { 'A', 'B', 'C' },
                expectedDeviceId,
                DateTime.MaxValue,
                expectedAmount);

            Assert.IsTrue(target.SetData(values));
            Assert.AreEqual("USD", target.CurrencyId);
        }

        [TestMethod]
        public void ReceivePersistenceBadLogSequenceTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 0;
            long expectedTransactionId = 1;
            long expectedAmount = 9999;
            byte[] currencyId = { (byte)'U', (byte)'S', (byte)'D' };

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "Accepted", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Amount", expectedAmount },
                { "TypeOfAccount", (byte)0 },
                { "CurrencyId", currencyId },
                { "Denomination", 0L },
                { "BankTransactionId", Guid.Empty }
            };

            var target = new BillTransaction(
                new[] { 'A', 'B', 'C' },
                expectedDeviceId,
                DateTime.MaxValue,
                expectedAmount);

            Assert.IsFalse(target.SetData(values));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReceivePersistenceElementErrorTest()
        {
            var target = new BillTransaction(
                new[] { 'A', 'B', 'C' },
                1,
                DateTime.MaxValue,
                9999);

            Assert.IsFalse(target.SetData(new Dictionary<string, object>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetPersistenceElementErrorTest()
        {
            var element = 10;
            var
                storageAccessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            storageAccessor.Setup(m => m.Count).Returns(3);

            var target = new BillTransaction(
                new[] { 'A', 'B', 'C' },
                1,
                DateTime.MaxValue,
                9999);
            target.SetPersistence(storageAccessor.Object, element);

            storageAccessor.Verify();
        }
    }
}