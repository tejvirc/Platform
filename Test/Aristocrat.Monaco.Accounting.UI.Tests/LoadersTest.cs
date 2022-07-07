namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    #region Using

    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using Application.Contracts.Localization;
    using Test.Common;

    #endregion

    /// <summary>
    ///     This class contains the tests for the UI page loaders
    /// </summary>
    [TestClass]
    public class LoadersTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IMeter> _meter;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITime> _time;
        private Mock<ITransactionHistory> _transactionHistory;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meter = MoqServiceManager.CreateAndAddService<IMeter>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void DenominationMetersLoaderTest()
        {
            // set up mocks for constructor
            _propertiesManager.Setup(m => m.GetProperty("Denominations", null)).Returns(new Collection<int>());
            const string expected = "Bills";

            var target = new BillsMetersPageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

        [TestMethod]
        public void VoucherMetersLoaderTest()
        {
            // set up mocks for constructor
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null)).Returns(1.0);
            _propertiesManager.Setup(m => m.GetProperty("System.VoucherIn", true)).Returns(true);
            _meter.Setup(m => m.Lifetime).Returns(0);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            const string expected = "Vouchers";

            var target = new VoucherMetersPageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

       
        [TestMethod]
        public void WatMetersLoaderTest()
        {
            // set up mocks for constructor
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null))
                .Returns(1.0);
            _meter.Setup(m => m.Lifetime).Returns(0);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(_meter.Object);
            const string expected = "Transfers";

            var target = new WatMetersPageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

        [TestMethod]
        public void HandpayMetersLoaderTest()
        {
            const string expected = "Handpay";

            var target = new HandpayMetersPageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

    }
}
