namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Localization;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for VoucherInTransactionTest
    /// </summary>
    [TestClass]
    public class VoucherInTransactionTest
    {
        private Mock<IPropertiesManager> _properties;
        private Mock<IEventBus> _eventBus;
        private LocalizationService _target;

        [TestInitialize]
        public void TestInitialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory);
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Loose);
            MockLocalization.Setup(MockBehavior.Strict);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _properties.Setup(x => x.AddPropertyProvider(It.IsAny<IPropertyProvider>()));
            _properties.Setup(x => x.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<string>()))
                .Returns("");
            _properties.Setup(x => x.SetProperty(It.IsAny<string>(), It.IsAny<object>()));
            _properties.Setup(x => x.GetProperty("Mono.SelectedAddinConfigurationHashCode", It.IsAny<string>()))
                .Returns(It.IsAny<string>());
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<LocalizationConfigurationEvent>>()))
                .Verifiable();
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>()))
                .Verifiable();
            _eventBus.Setup(x => x.Publish(It.IsAny<CurrentCultureChangedEvent>()))
                .Verifiable();
            _eventBus.Setup(x => x.Publish(It.IsAny<OperatorCultureChangedEvent>()))
                .Verifiable();
            _eventBus.Setup(x => x.Publish(It.IsAny<PlayerCultureChangedEvent>()))
                .Verifiable();
            _eventBus.Setup(x => x.Publish(It.IsAny<OperatorCultureAdded>()))
                .Verifiable();
            _target = new LocalizationService();
            _target.Initialize();
        }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new VoucherInTransaction();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 9999;
            var accountType = AccountType.Cashable;
            var barcode = "12345678";

            var target = new VoucherInTransaction(deviceId, transactionDateTime, amount, accountType, barcode);

            Assert.IsNotNull(target);
            Assert.AreEqual(deviceId, target.DeviceId);
            Assert.AreEqual(transactionDateTime, target.TransactionDateTime);
            Assert.AreEqual(amount, target.Amount);
            Assert.AreEqual(accountType, target.TypeOfAccount);
            Assert.AreEqual(barcode, target.Barcode);
        }

        [TestMethod]
        public void NameTest()
        {
            var target = new VoucherInTransaction();

            Assert.AreEqual("Voucher In", target.Name);
        }

        [TestMethod]
        public void EqualityTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 9999;
            var accountType = AccountType.Cashable;
            var barcode = "12345678";

            var target1 = new VoucherInTransaction(deviceId, transactionDateTime, amount, accountType, barcode);
            var target2 = new VoucherInTransaction(deviceId, transactionDateTime, amount, accountType, barcode);
            var target3 = new VoucherInTransaction(deviceId + 1, transactionDateTime, amount, accountType, barcode);

            // tests for ==
#pragma warning disable CS1718 // Comparison made to same variable; did you mean to compare something else?
            Assert.IsTrue(target1 == target1); // need to test references are equal
#pragma warning restore CS1718 // Comparison made to same variable; did you mean to compare something else?
            Assert.IsTrue(target1 == target2);
            Assert.IsFalse(target1 == target3);
            Assert.IsFalse(target1 == null);
            Assert.IsFalse(null == target3);

            // tests for !=
            Assert.IsFalse(target1 != target2);
            Assert.IsTrue(target1 != target3);

            // test for Equals(object)
            Assert.IsTrue(target1.Equals((object)target2));
            Assert.IsFalse(target1.Equals((object)target3));
        }

        [TestMethod]
        public void CloneTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 9999;
            var accountType = AccountType.Cashable;
            var barcode = "12345678";

            var target = new VoucherInTransaction(deviceId, transactionDateTime, amount, accountType, barcode);
            Assert.AreEqual(target, target.Clone());
        }

        [TestMethod]
        public void ToStringTest()
        {
            var deviceId = 1;
            var transactionDateTime = DateTime.MaxValue;
            long amount = 9999;
            var accountType = AccountType.Cashable;
            var barcode = "12345678";

            var target = new VoucherInTransaction(deviceId, transactionDateTime, amount, accountType, barcode);

            Assert.IsFalse(string.IsNullOrEmpty(target.ToString()));
        }

        [TestMethod]
        public void ReceivePersistenceHappyPathTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 123;
            long expectedTransactionId = 1;
            long expectedAmount = 9999;
            var transactionDateTime = DateTime.MaxValue;
            var accountType = AccountType.Cashable;
            var barcode = "012345678";
            var barcodeLength = 9;
            long barcodeNumber = 12345678;

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Amount", expectedAmount },
                { "TypeOfAccount", (byte)0 },
                { "AssociatedTransactions", string.Empty },
                { "BarcodeLength", barcodeLength },
                { "Barcode", barcodeNumber },
                { "VoucherSequence", 1 },
                { "CommitAcknowledged", true },
                { "State", 1 },
                { "Exception", 0 },
                { "TraceId", Guid.Empty },
                { "LogDisplayType", string.Empty }
            };

            var target = new VoucherInTransaction();

            Assert.IsTrue(target.SetData(values));
            Assert.AreEqual(expectedDeviceId, target.DeviceId);
            Assert.AreEqual(transactionDateTime, target.TransactionDateTime);
            Assert.AreEqual(expectedAmount, target.Amount);
            Assert.AreEqual(accountType, target.TypeOfAccount);
            Assert.AreEqual(barcode, target.Barcode);
        }

        [TestMethod]
        public void ReceivePersistenceNullBarcodeTest()
        {
            var expectedDeviceId = 2;
            long expectedLogSequence = 123;
            long expectedTransactionId = 1;
            long expectedAmount = 9999;
            var transactionDateTime = DateTime.MaxValue;
            var accountType = AccountType.Cashable;
            var barcodeLength = 0;

            var values = new Dictionary<string, object>
            {
                { "DeviceId", expectedDeviceId },
                { "LogSequence", expectedLogSequence },
                { "TransactionDateTime", DateTime.MaxValue },
                { "TransactionId", expectedTransactionId },
                { "Amount", expectedAmount },
                { "TypeOfAccount", (byte)0 },
                { "AssociatedTransactions", string.Empty },
                { "BarcodeLength", barcodeLength },
                { "VoucherSequence", 1 },
                { "CommitAcknowledged", true },
                { "State", 1 },
                { "Exception", 0 },
                { "TraceId", Guid.Empty },
                { "LogDisplayType", string.Empty }
            };

            var target = new VoucherInTransaction();

            Assert.IsTrue(target.SetData(values));
            Assert.AreEqual(expectedDeviceId, target.DeviceId);
            Assert.AreEqual(transactionDateTime, target.TransactionDateTime);
            Assert.AreEqual(expectedAmount, target.Amount);
            Assert.AreEqual(accountType, target.TypeOfAccount);
            Assert.IsNull(target.Barcode);
        }

        [TestMethod]
        public void SetPersistenceTest()
        {
            var element = 1;
            var expectedDeviceId = 2;
            long expectedLogSequence = 1;
            long expectedTransactionId = 1;
            var expectedVoucherSequence = 1;
            long expectedAmount = 9999;
            var expectedTransactionDateTime = DateTime.MaxValue;
            var accountType = AccountType.Cashable;
            var barcode = "123456";
            long barcodeValue = 123456;

            var storageTransaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            storageTransaction.SetupSet(m => m[element, "DeviceId"] = expectedDeviceId).Verifiable();
            storageTransaction.SetupSet(m => m[element, "LogSequence"] = expectedLogSequence).Verifiable();
            storageTransaction.SetupSet(m => m[element, "TransactionDateTime"] = expectedTransactionDateTime)
                .Verifiable();
            storageTransaction.SetupSet(m => m[element, "TransactionId"] = expectedTransactionId).Verifiable();
            storageTransaction.SetupSet(m => m[element, "Amount"] = expectedAmount).Verifiable();
            storageTransaction.SetupSet(m => m[element, "TypeOfAccount"] = (byte)AccountType.Cashable).Verifiable();
            storageTransaction.SetupSet(m => m[element, "AssociatedTransactions"] = "[]");
            storageTransaction.SetupSet(m => m[element, "BarcodeLength"] = 6).Verifiable();
            storageTransaction.SetupSet(m => m[element, "Barcode"] = barcodeValue).Verifiable();
            storageTransaction.SetupSet(m => m[element, "VoucherSequence"] = expectedVoucherSequence).Verifiable();
            storageTransaction.SetupSet(m => m[element, "CommitAcknowledged"] = false).Verifiable();
            storageTransaction.SetupSet(m => m[element, "State"] = VoucherState.Issued).Verifiable();
            storageTransaction.SetupSet(m => m[element, "Exception"] = 0).Verifiable();
            storageTransaction.SetupSet(m => m[element, "TraceId"] = Guid.Empty).Verifiable();
            storageTransaction.SetupSet(m => m[element, "LogDisplayType"] = string.Empty).Verifiable();
            storageTransaction.Setup(m => m.Commit()).Verifiable();
            storageTransaction.Setup(m => m.Dispose()).Verifiable();

            var storageAccessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
            storageAccessor.Setup(m => m.Count).Returns(3);
            storageAccessor.Setup(m => m.StartTransaction()).Returns(storageTransaction.Object);

            var target = new VoucherInTransaction(
                expectedDeviceId,
                expectedTransactionDateTime,
                expectedAmount,
                accountType,
                barcode)
            {
                LogSequence = expectedLogSequence,
                TransactionId = expectedTransactionId,
                VoucherSequence = expectedVoucherSequence
            };

            target.SetPersistence(storageAccessor.Object, element);

            storageAccessor.Verify();
        }
    }
}