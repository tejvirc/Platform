namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Mgam.Services.Attributes;
    using Mgam.Services.DropMode;
    using Kernel;
    using Localization.Properties;
    using Hardware.Contracts.Audio;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Aristocrat.Monaco.Mgam.Common.Events;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Mgam.Services.PlayerTracking;
    using Aristocrat.Monaco.Mgam.Services.CreditValidators;
    using Aristocrat.Monaco.Kernel.Contracts.MessageDisplay;

    [TestClass]
    public class DropModeTest
    {
        private const string MeterName = "TestMeter";

        private DropMode _target;

        private Mock<IEventBus> _eventBus;
        private Mock<IAttributeManager> _attributes;
        private Mock<IAudio> _audio;
        private Mock<ISystemDisableManager> _systemDisable;
        private Mock<IMeterManager> _meterManager;
        private Mock<IEmployeeLogin> _employeeLogin;
        private Mock<IPlayerTracking> _playerTracking;
        private Mock<ICashOut> _bank;
        private Mock<IGamePlayState> _gamePlay;
        private Mock<IPropertiesManager> _properties;
        private Mock<ILogger<DropMode>> _logger;
        private Mock<IMeter> _meter;
        private Mock<ILocalizerFactory> _localizerFactory;

        private Action<AttributeChangedEvent> _subscriptionToAttributeChanged;
        private Dictionary<Guid, string> _disablers = new Dictionary<Guid, string>();
        private long _meterVal;
        private bool _dropMode;
        private bool _muted;
        private string _login;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _subscriptionToAttributeChanged = null;
            _dropMode = false;
            _muted = false;
            _logger = new Mock<ILogger<DropMode>>();

            MockSystemDisable();
            MockMeters();
            MockEventBus();
            MockAudio();
            MockAttributes();
            MockEmployeeLogin();
            MockPlayerTracking();
            MockBank();
            MockGamePlay();
            MockProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(false, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, false, true, true, true, true, true, true, true, true, true, DisplayName = "Null Attribute Manager Object")]
        [DataRow(true, true, false, true, true, true, true, true, true, true, true, DisplayName = "Null Audio Object")]
        [DataRow(true, true, true, false, true, true, true, true, true, true, true, DisplayName = "Null System Disable Manager Object")]
        [DataRow(true, true, true, true, false, true, true, true, true, true, true, DisplayName = "Null Meter Manager Object")]
        [DataRow(true, true, true, true, true, false, true, true, true, true, true, DisplayName = "Null Employee Login Service Object")]
        [DataRow(true, true, true, true, true, true, false, true, true, true, true, DisplayName = "Null Player Tracking Object")]
        [DataRow(true, true, true, true, true, true, true, false, true, true, true, DisplayName = "Null Bank Object")]
        [DataRow(true, true, true, true, true, true, true, true, false, true, true, DisplayName = "Null Game Play State Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, false, true, DisplayName = "Null Properties Manager Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, false, DisplayName = "Null Logger Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool eventBus,
            bool attributeManager,
            bool audio,
            bool systemDisableManager,
            bool meterManager,
            bool employeeLogin,
            bool playerTracking,
            bool bank,
            bool gamePlay,
            bool properties,
            bool logger)
        {
            _target = new DropMode(
                eventBus ? _eventBus.Object : null,
                attributeManager ? _attributes.Object : null,
                audio ? _audio.Object : null,
                systemDisableManager ? _systemDisable.Object : null,
                meterManager ? _meterManager.Object : null,
                employeeLogin ? _employeeLogin.Object : null,
                playerTracking ? _playerTracking.Object : null,
                bank ? _bank.Object : null,
                gamePlay ? _gamePlay.Object : null,
                properties ? _properties.Object : null,
                logger ? _logger.Object : null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var _target = new DropMode(null, null, null, null, null, null, null, null, null, null, null);

            Assert.IsNull(_target);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            CreateNewTarget();

            Assert.IsNotNull(_target);
            Assert.IsInstanceOfType(_target, typeof(IDropMode));
        }

        [TestMethod]
        public void ClearMetersTest()
        {
            CreateNewTarget();

            var meter = _meterManager.Object.GetMeter(MeterName);
            Assert.AreNotEqual(0, meter.Period);

            _target.ClearMeters();

            var meter2 = _meterManager.Object.GetMeter(MeterName);
            Assert.AreEqual(0, meter2.Period);
        }

        [TestMethod]
        public void DropModeOnOffTest()
        {
            CreateNewTarget();

            _dropMode = true;
            _subscriptionToAttributeChanged(new AttributeChangedEvent(AttributeNames.DropMode));

            Assert.AreEqual(true, _muted);
            Assert.AreEqual(true, _target.Active);
            Assert.AreEqual(_login, ResourceKeys.DropMode);
            Assert.AreNotEqual(0, _disablers.Count);

            Task.Delay(100);
            _dropMode = false;
            _subscriptionToAttributeChanged(new AttributeChangedEvent(AttributeNames.DropMode));

            Assert.AreEqual(false, _muted);
            Assert.AreEqual(false, _target.Active);
            Assert.IsNull(_login);
            Assert.AreEqual(0, _disablers.Count);
        }

        [TestMethod]
        public void DropModeDuringGameRoundTest()
        {
            CreateNewTarget();
            _gamePlay.SetupGet(g => g.InGameRound).Returns(true);

            _dropMode = true;
            _subscriptionToAttributeChanged(new AttributeChangedEvent(AttributeNames.DropMode));

            Assert.AreNotEqual(0, _disablers.Count);
        }

        private void CreateNewTarget()
        {
            _target = new DropMode(_eventBus.Object, _attributes.Object, _audio.Object, _systemDisable.Object, _meterManager.Object,
                _employeeLogin.Object, _playerTracking.Object, _bank.Object, _gamePlay.Object, _properties.Object, _logger.Object);
        }

        private void MockSystemDisable()
        {
            _systemDisable = new Mock<ISystemDisableManager>();

            _systemDisable.Setup(d => d.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<string>(), It.IsAny<CultureProviderType>(), It.IsAny<object[]>()))
                .Callback<Guid, SystemDisablePriority, string, CultureProviderType, object[]>((g, p, f, t, o) => _disablers[g] = f);
            _systemDisable.Setup(d => d.Enable(It.IsAny<Guid>()))
                .Callback<Guid>(g => _disablers.Remove(g));
            _systemDisable.SetupGet(d => d.CurrentDisableKeys)
                .Returns(_disablers.Keys.ToList());

            _disablers.Clear();

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("en-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });
        }

        private void MockMeters()
        {
            _meterManager = new Mock<IMeterManager>();
            _meter = new Mock<IMeter>();

            _meterVal = 1;

            _meter.SetupGet(m => m.Period).Returns(() => _meterVal);
            _meter.Setup(m => m.Increment(It.IsAny<long>()))
                .Callback<long>(val => _meterVal += val);

            _meterManager.Setup(m => m.ClearAllPeriodMeters())
                .Callback(() => _meter.Object.Increment(-_meterVal));
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>()))
                .Returns(() => _meter.Object);
        }

        private void MockEventBus()
        {
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<AttributeChangedEvent>>(), It.IsAny<Predicate<AttributeChangedEvent>>()))
                .Callback<object, Action<AttributeChangedEvent>, Predicate<AttributeChangedEvent>>((_, callback, pred) => _subscriptionToAttributeChanged = callback);
        }

        private void MockAudio()
        {
            _audio = new Mock<IAudio>();
            _audio.Setup(a => a.SetSystemMuted(It.IsAny<bool>()))
                .Callback<bool>(b => _muted = b);
        }

        private void MockAttributes()
        {
            _attributes = new Mock<IAttributeManager>();

            _attributes.Setup(p => p.Get(AttributeNames.DropMode, false))
                .Returns(() => _dropMode);
        }

        private void MockEmployeeLogin()
        {
            _employeeLogin = new Mock<IEmployeeLogin>();

            _employeeLogin.Setup(e => e.Login(It.IsAny<string>()))
                .Callback<string>(s => _login = s);
            _employeeLogin.Setup(e => e.Logout(It.IsAny<string>()))
                .Callback<string>(s => _login = null);
        }

        private void MockPlayerTracking()
        {
            _playerTracking = new Mock<IPlayerTracking>();
            _playerTracking.Setup(i => i.EndPlayerSession()).Verifiable();
        }

        private void MockBank()
        {
            _bank = new Mock<ICashOut>();
            _bank.Setup(b => b.CashOut()).Verifiable();
        }

        private void MockGamePlay()
        {
            _gamePlay = new Mock<IGamePlayState>();
        }

        private void MockProperties()
        {
            _properties = new Mock<IPropertiesManager>();
            _properties.Setup(p => p.SetProperty(It.IsAny<string>(), true));
        }
    }
}
