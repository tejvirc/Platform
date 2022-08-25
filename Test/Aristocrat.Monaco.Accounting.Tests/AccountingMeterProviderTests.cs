namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class AccountingMeterProviderTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IBank> _bank;
        private Mock<IDisposable> _disposable;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
         
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(a => a.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
         
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
        }

        [TestCleanup]
        public void CleanUp()
        {
           MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void AccountingMeterProviderConstructorTest()
        {
            var provider = new AccountingMeterProvider();
            var verifyMeters = new Dictionary<string, Type>
            {
                {AccountingMeters.TotalHandpaidCashVoucherAndTickets,  typeof(CurrencyMeterClassification)},
            };
            
            foreach (var meterInfo in verifyMeters)
            {
                var meter = provider.GetMeter(meterInfo.Key);
                Assert.IsNotNull(meter);
                Assert.IsInstanceOfType(meter.Classification, meterInfo.Value);
            }
        }

        [TestMethod]
        public void TestMeterTotalHandpaidCashVoucherAndTickets()
        {
            var provider = new AccountingMeterProvider();
            
            SetupMeters(_meterManager, AccountingMeters.HandpaidCashableAmount, 300, 200, 100);
            SetupMeters(_meterManager, AccountingMeters.HandpaidPromoAmount, 400, 300, 200);
            SetupMeters(_meterManager, AccountingMeters.TotalVouchersOut, 500, 400, 300);

            var handpaidMeter = provider.GetMeter(AccountingMeters.TotalHandpaidCashVoucherAndTickets);

            Assert.AreEqual(1200, handpaidMeter.Lifetime);
            Assert.AreEqual(900, handpaidMeter.Period);
            Assert.AreEqual(600, handpaidMeter.Session);
        }

        [DataRow(100)]
        [DataRow(300)]
        [DataRow(1000)]
        [DataTestMethod]
        public void TestCurrentCreditsMeter(long value)
        {
            _bank.Setup(b => b.QueryBalance()).Returns(value);
            var provider = new AccountingMeterProvider();
            var currentCreditsMeter = provider.GetMeter(AccountingMeters.CurrentCredits);
            Assert.AreEqual(value, currentCreditsMeter.Lifetime);
        }

        private static void SetupMeters(
            Mock<IMeterManager> manager,
            string meterName,
            long lifetimeValue,
            long periodValue,
            long sessionValue)
        {
            var met = new Mock<IMeter>(MockBehavior.Strict);
            met.Setup(m => m.Lifetime).Returns(lifetimeValue);
            met.Setup(m => m.Period).Returns(periodValue);
            met.Setup(m => m.Session).Returns(sessionValue);

            manager.Setup(m => m.GetMeter(meterName)).Returns(met.Object);
        }
    }
}
