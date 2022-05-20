using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Kernel;
using Moq;
using Aristocrat.Monaco.Hardware.Contracts.Audio;
using Aristocrat.Monaco.Test.Common;
using Aristocrat.Monaco.Hardware.Contracts.Button;
using Aristocrat.Monaco.Accounting.Contracts.Wat;
using System.Collections.Generic;

namespace Aristocrat.Monaco.Accounting.Tests
{
    [TestClass()]
    public class MoneyLaunderingMonitorTests
    {
        private Mock<IPropertiesManager> _properties;
        private Mock<IEventBus> _eventBus;
        private Mock<IAudio> _audio;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IBank> _bank;
        private const string AudioFilePath = "dummyPath";
        private Action<CurrencyInCompletedEvent> _currencyInCompletedEventHandler;
        private Action<TransferOutCompletedEvent> _transferOutCompletedEventHandler;
        private Action<TransferOutStartedEvent> _transferOutStartedEventHandler;
        private Action<WatOnCompleteEvent> _watOnCompleteEventHandler;
        private Action<WatTransferCompletedEvent> _watOffCompleteEventHandler;
        private Action<DownEvent> _downEventHandler;
        private Action<PropertyChangedEvent> _propertyChangedEventHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _audio = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Loose);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);

            SetupVoidMethods();

            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterSound, It.IsAny<string>())).Returns(AudioFilePath);
            _properties.Setup(x => x.GetProperty(AccountingConstants.DisabledDueToExcessiveMeter, false)).Returns(true);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false)).Returns(true);
            _properties.Setup(x => x.GetProperty(AccountingConstants.DisabledDueToExcessiveMeter, It.IsAny<bool>())).Returns(false);

            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(200L);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod()]
        public void MoneyLaunderingMonitorConstructor()
        {
            var v = new MoneyLaunderingMonitor();

            Assert.IsNotNull(v);
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullBus,
            bool nullProperties,
            bool nullDisable,
            bool nullAudio,
            bool nullBank
            )
        {
            var target = CreateTarget(nullBus, nullProperties, nullDisable, nullAudio, nullBank);
        }
 
        [TestMethod]
        public void IsThresholdReachedTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(200L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false)).Returns(true);

            var target = CreateTarget();

            Assert.IsTrue(target.IsThresholdReached());

            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false)).Returns(false);
            target = CreateTarget();

            Assert.IsFalse(target.IsThresholdReached());

            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(180L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(10L);

            target = CreateTarget();
            Assert.IsFalse(target.IsThresholdReached());
        }

        [TestMethod()]
        public void DisposeTest()
        {
            var target = CreateTarget();

            target.Dispose();

            _eventBus.Verify(x => x.UnsubscribeAll(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);
        }

        [TestMethod()]
        public void InitializeTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(85L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(1200L);

            var target = CreateTarget();

            Assert.AreEqual(1200, target.ExcessiveMeterValue);
            Assert.IsTrue(target.IsThresholdReached());

            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(150L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(56L);

            target = CreateTarget();

            Assert.AreEqual(56, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());
        }

        [TestMethod()]
        public void NotifyGameStartedTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(200L);

            var target = CreateTarget();

            Assert.IsTrue(target.IsThresholdReached());

            target.NotifyGameStarted();

            Assert.AreEqual(0, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());
        }

        [TestMethod()]
        public void CurrencyInCompletedEventHandlerTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(60L);

            var target = CreateTarget();

            Assert.IsFalse(target.IsThresholdReached());

            var e = new CurrencyInCompletedEvent(20);
            _currencyInCompletedEventHandler(e);

            Assert.AreEqual(80, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());

            e = new CurrencyInCompletedEvent(20);
            _currencyInCompletedEventHandler(e);

            Assert.AreEqual(100, target.ExcessiveMeterValue);
            Assert.IsTrue(target.IsThresholdReached());
        }

        [TestMethod()]
        public void WatOnCompletedEventHandlerTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(60L);

            var target = CreateTarget();

            Assert.IsFalse(target.IsThresholdReached());

            var e = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Committed,
                TransferredCashableAmount = 20
            });

            _watOnCompleteEventHandler(e);

            Assert.AreEqual(80, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());

            e = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Rejected,
                TransferredCashableAmount = 20
            });
            _watOnCompleteEventHandler(e);

            Assert.AreEqual(80, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());

            e = new WatOnCompleteEvent(new WatOnTransaction
            {
                Status = WatStatus.Committed,
                TransferredCashableAmount = 20
            });
            _watOnCompleteEventHandler(e);

            Assert.AreEqual(100, target.ExcessiveMeterValue);
            Assert.IsTrue(target.IsThresholdReached());
        }

        [TestMethod()]
        public void WatOffCompletedEventHandlerTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(60L);
            _bank.Setup(x => x.QueryBalance()).Returns(40);

            var target = CreateTarget();

            Assert.IsFalse(target.IsThresholdReached());

            var e = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredCashableAmount = 20
            });
            _watOffCompleteEventHandler(e);

            Assert.AreEqual(60, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());

            // try a full WatOff now. It should reset the meter if Staus is Complete
            e = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Rejected,
                TransferredCashableAmount = 40
            });

            _watOffCompleteEventHandler(e);
            Assert.AreEqual(60, target.ExcessiveMeterValue);

            e = new WatTransferCompletedEvent(new WatTransaction
            {
                Status = WatStatus.Complete,
                TransferredCashableAmount = 40
            });

            _bank.Setup(x => x.QueryBalance()).Returns(0);

            _watOffCompleteEventHandler(e);

            Assert.AreEqual(0, target.ExcessiveMeterValue);
            Assert.IsFalse(target.IsThresholdReached());

            // Note: WatOff (Full/Partial) with Threshold Reached is redirected in SAS so can not be unit tested
        }

        [TestMethod()]
        public void TransferOutCompletedEventHandlerTest()
        {
            var target = CreateTarget();

            var e = new TransferOutCompletedEvent(0, 0, 0, false, Guid.NewGuid());
            _transferOutCompletedEventHandler(e);

            Assert.AreEqual(0, target.ExcessiveMeterValue);
        }

        [TestMethod()]
        public void TransferOutStartedEventHandlerTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(60L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);

            SetupDisableStatus(false);

            var target = CreateTarget();

            var e = new TransferOutStartedEvent(Guid.NewGuid(), 0, 0, 0);
            _transferOutStartedEventHandler(e);

            Assert.IsFalse(target.IsThresholdReached());
            VerifyDisable(Times.Never());

            // repeat with threshold reached
            _properties.Setup(x => x.GetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>())).Returns(120L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThreshold, It.IsAny<long>())).Returns(100L);
            _properties.Setup(x => x.GetProperty("Application.AlertVolume", It.IsAny<byte>())).Returns((byte)0);

            target = CreateTarget();

            _transferOutStartedEventHandler(e);

            Assert.IsTrue(target.IsThresholdReached());
            VerifyDisable(Times.Once());
            _properties.Verify(x => x.SetProperty(AccountingConstants.DisabledDueToExcessiveMeter, true));

            // Simulate pressing of the jackpot key to re-enable
            SetupDisableStatus();

            var e1 = new DownEvent();
            _downEventHandler(e1);

            VerifyEnable(Times.Once());
            _properties.Verify(x => x.SetProperty(AccountingConstants.DisabledDueToExcessiveMeter, false));
        }

        [TestMethod()]
        public void PropertyChangedEventHandlerTest()
        {
            // Enable the service
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false)).Returns(true);

            var target = CreateTarget();
          
            _eventBus.Verify(x => x.Subscribe<CurrencyInCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<CurrencyInCompletedEvent>>()), Times.Once);
            _eventBus.Verify(x => x.Subscribe<TransferOutCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<TransferOutCompletedEvent>>()), Times.Once);
            _eventBus.Verify(x => x.Subscribe<TransferOutStartedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<TransferOutStartedEvent>>()), Times.Once);
            _eventBus.Verify(x => x.Subscribe<WatOnCompleteEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<WatOnCompleteEvent>>()), Times.Once);
            _eventBus.Verify(x => x.Subscribe<WatTransferCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<WatTransferCompletedEvent>>()), Times.Once);


            // Disable the service
            _properties.Setup(x => x.GetProperty(AccountingConstants.IncrementThresholdIsChecked, false)).Returns(false);

            // invoke PropertyChangedEventHandler and verify all Unsubscribe's are called
            _propertyChangedEventHandler(null);

            _eventBus.Verify(x => x.Unsubscribe<CurrencyInCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<TransferOutCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<TransferOutStartedEvent>(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<WatOnCompleteEvent>(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<WatTransferCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()), Times.Once);

            // verify meter got reset
            Assert.AreEqual(0, target.ExcessiveMeterValue);
        }
        private void SetupDisableStatus(bool disable = true)
        {
            var list = new List<Guid>();

            if (disable)
                list.Add(MoneyLaunderingMonitor.ExcessiveThresholdDisableKey);

            _disableManager.Setup(x => x.CurrentImmediateDisableKeys).Returns(list);
        }
        private void VerifyDisable(Times times)
        {
            _disableManager.Verify(x => x.Disable(MoneyLaunderingMonitor.ExcessiveThresholdDisableKey,
                SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), true, It.IsAny<Func<string>>(), null), times);

            _disableManager.ResetCalls();
        }

        private void VerifyEnable(Times times)
        {
            _disableManager.Verify(x => x.Enable(MoneyLaunderingMonitor.ExcessiveThresholdDisableKey), times);

            _disableManager.ResetCalls();
        }

        private void SetupVoidMethods()
        {
            _eventBus.Setup(x => x.Subscribe<PropertyChangedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<PropertyChangedEvent>>(), It.IsNotNull<Predicate<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>, Predicate<PropertyChangedEvent>>((_, func, f) => _propertyChangedEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<CurrencyInCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<CurrencyInCompletedEvent>>()))
                .Callback<object, Action<CurrencyInCompletedEvent>>((_, func) => _currencyInCompletedEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<TransferOutCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<TransferOutCompletedEvent>>()))
                .Callback<object, Action<TransferOutCompletedEvent>>((_, func) => _transferOutCompletedEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<TransferOutStartedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<TransferOutStartedEvent>>()))
                .Callback<object, Action<TransferOutStartedEvent>>((_, func) => _transferOutStartedEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<WatOnCompleteEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<WatOnCompleteEvent>>()))
                .Callback<object, Action<WatOnCompleteEvent>>((_, func) => _watOnCompleteEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<WatTransferCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<WatTransferCompletedEvent>>()))
                .Callback<object, Action<WatTransferCompletedEvent>>((_, func) => _watOffCompleteEventHandler = func);

            _eventBus.Setup(x => x.Subscribe<DownEvent>(It.IsAny<MoneyLaunderingMonitor>(), It.IsAny<Action<DownEvent>>(), It.IsAny<Predicate<DownEvent>>()))
                .Callback<object, Action<DownEvent>, Predicate<DownEvent>>((_, func, predicate) => _downEventHandler = func);

            _eventBus.Setup(x => x.Unsubscribe<DownEvent>(It.IsAny<MoneyLaunderingMonitor>()));
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<MoneyLaunderingMonitor>()));

            _eventBus.Setup(x => x.Unsubscribe<CurrencyInCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()));
            _eventBus.Setup(x => x.Unsubscribe<TransferOutCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()));
            _eventBus.Setup(x => x.Unsubscribe<TransferOutStartedEvent>(It.IsAny<MoneyLaunderingMonitor>()));
            _eventBus.Setup(x => x.Unsubscribe<WatOnCompleteEvent>(It.IsAny<MoneyLaunderingMonitor>()));
            _eventBus.Setup(x => x.Unsubscribe<WatTransferCompletedEvent>(It.IsAny<MoneyLaunderingMonitor>()));


            _properties.Setup(x => x.SetProperty(AccountingConstants.DisabledDueToExcessiveMeter, true));
            _properties.Setup(x => x.SetProperty(AccountingConstants.ExcessiveMeterValue, It.IsAny<long>()));

            _disableManager.Setup(x => x.Disable(MoneyLaunderingMonitor.ExcessiveThresholdDisableKey,
                SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), true, It.IsAny<Func<string>>(), null));

            _disableManager.Setup(x => x.Enable(MoneyLaunderingMonitor.ExcessiveThresholdDisableKey));
        }

        private MoneyLaunderingMonitor CreateTarget()
        {
            var v = new MoneyLaunderingMonitor();

            v.Initialize();

            return v;
        }

        private MoneyLaunderingMonitor CreateTarget(
            bool nullBus = false,
            bool nullProperties = false,
            bool nullDisable = false,
            bool nullAudio = false,
            bool nullBank = false
            )
        {
            return new MoneyLaunderingMonitor(
                nullBus ? null : _eventBus.Object,
                nullProperties ? null : _properties.Object,
                nullDisable ? null : _disableManager.Object,
                nullAudio ? null : _audio.Object,
                nullBank ? null : _bank.Object
               );
        }
    }
}