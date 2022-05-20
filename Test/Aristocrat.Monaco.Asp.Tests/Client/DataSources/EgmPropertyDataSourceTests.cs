namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Application.Contracts;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Asp.Client.DataSources;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class EgmPropertyDataSourceTests
    {
        private Mock<IEventBus> _eventBus;
        private Action<GameConnectedEvent> _gameConnectedCallback;
        private Action<GameSelectedEvent> _gameSelectedCallback;
        private Action<DenominationSelectedEvent> _denominatonSelectedCallback;
        private Action<GameExitedNormalEvent> _gameExitedNormalEventCallback;
        private Action<GameProcessExitedEvent> _gameProcessExitedEventCallback;
        private Mock<IPropertiesManager> _propertiesManager;
        private EgmPropertyDataSource _egmPropertyDataSource;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private Mock<IAspGameProvider> _aspGameProvider;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncher;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _persistentStorageManager = new Mock<IPersistentStorageManager>();
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();
            _aspGameProvider = new Mock<IAspGameProvider>(MockBehavior.Strict);
            _operatorMenuLauncher = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<EgmPropertyDataSource>(), It.IsAny<Action<GameConnectedEvent>>()))
                  .Callback<object, Action<GameConnectedEvent>>((subscriber, callback) => _gameConnectedCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<EgmPropertyDataSource>(), It.IsAny<Action<GameSelectedEvent>>()))
                .Callback<object, Action<GameSelectedEvent>>((subscriber, callback) => _gameSelectedCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<EgmPropertyDataSource>(), It.IsAny<Action<DenominationSelectedEvent>>()))
                .Callback<object, Action<DenominationSelectedEvent>>((subscriber, callback) => _denominatonSelectedCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<EgmPropertyDataSource>(), It.IsAny<Action<GameExitedNormalEvent>>()))
                .Callback<object, Action<GameExitedNormalEvent>>((subscriber, callback) => _gameExitedNormalEventCallback = callback);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<EgmPropertyDataSource>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((subscriber, callback) => _gameProcessExitedEventCallback = callback);

            _eventBus.Setup(s => s.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            byte insertedCardId = 0;

            _persistentStorageTransaction.SetupSet(m => m["CardInsertedField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                insertedCardId = (byte)fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(insertedCardId);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageAccessor.SetupGet(g => g["CardInsertedField"]).Returns(insertedCardId);

            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorageManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1)).Returns(_persistentStorageAccessor.Object);

            _persistentStorageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);

            _persistentStorageAccessor.Setup(m => m.StartTransaction()).Returns(_persistentStorageTransaction.Object);
            _persistentStorageTransaction.Setup(m => m.Commit());

            _egmPropertyDataSource = new EgmPropertyDataSource(
                _aspGameProvider.Object,
                _eventBus.Object,
                _propertiesManager.Object,
                _persistentStorageManager.Object,
                _operatorMenuLauncher.Object);

            SetupMockAspGameProvider();
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "EGMProperty";
            Assert.AreEqual(expectedName, _egmPropertyDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Serial_Number",
                "Firmware_ID",
                "Firmware_VerNo",
                "Currency",
                "EGM_Security_Mode",
                "Card_Inserted",
                "Game_Being_Played"
            };

            var actualMembers = _egmPropertyDataSource.Members;

            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMembersTest()
        {
            string serialNumber = "12345678";
            string systemVersion = "1000";
            string firmwareId = systemVersion;
            string firmwareVerNo = firmwareId;
            string currency = "AUD";

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<object>())).Returns(serialNumber);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.SystemVersion, It.IsAny<object>())).Returns(systemVersion);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyId, It.IsAny<object>())).Returns(currency);

            string serialNumberActual = (string)_egmPropertyDataSource.GetMemberValue("Serial_Number");
            string firmwareIdActual = (string)_egmPropertyDataSource.GetMemberValue("Firmware_ID");
            string firmwareVerNoActual = (string)_egmPropertyDataSource.GetMemberValue("Firmware_VerNo");
            string currencyActual = (string)_egmPropertyDataSource.GetMemberValue("Currency");
            byte egmSecurityModeoActual = (byte)_egmPropertyDataSource.GetMemberValue("EGM_Security_Mode");
            byte cardInsertedValueActual = (byte)_egmPropertyDataSource.GetMemberValue("Card_Inserted");

            Assert.AreEqual(serialNumber, serialNumberActual);
            Assert.AreEqual(firmwareId, firmwareIdActual);
            Assert.AreEqual(firmwareVerNo, firmwareVerNoActual);
            Assert.AreEqual(currency, currencyActual);
            Assert.AreEqual(1, egmSecurityModeoActual);
            Assert.AreEqual(0, cardInsertedValueActual);
        }

        [TestMethod]
        public void InsertedCardIdTest()
        {
            for (int i = -5; i <= 35; i++)
            {
                if (i >= 0 && i <= 30)
                {
                    byte expected = (byte)i;
                    _egmPropertyDataSource.SetMemberValue("Card_Inserted", expected);
                    byte actual = (byte)_egmPropertyDataSource.GetMemberValue("Card_Inserted");
                    Assert.AreEqual(expected, actual);
                }
                else
                {
                    if (i<0)
                    {
                        Assert.ThrowsException<OverflowException>(() => { _egmPropertyDataSource.SetMemberValue("Card_Inserted", i); });
                    }
                    else
                    {
                        Assert.ThrowsException<Exception>(() => { _egmPropertyDataSource.SetMemberValue("Card_Inserted", i); });
                    }
                }
            }
        }


        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_egmPropertyDataSource);
        }

        [TestMethod]
        public void NullConsutructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new EgmPropertyDataSource(null, _eventBus.Object, _propertiesManager.Object, _persistentStorageManager.Object, _operatorMenuLauncher.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new EgmPropertyDataSource(_aspGameProvider.Object, null, _propertiesManager.Object, _persistentStorageManager.Object, _operatorMenuLauncher.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new EgmPropertyDataSource(_aspGameProvider.Object, _eventBus.Object, null, _persistentStorageManager.Object, _operatorMenuLauncher.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new EgmPropertyDataSource(_aspGameProvider.Object, _eventBus.Object, _propertiesManager.Object, null, _operatorMenuLauncher.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new EgmPropertyDataSource(_aspGameProvider.Object, _eventBus.Object, _propertiesManager.Object, _persistentStorageManager.Object, null));
        }

        [TestMethod]
        public void GetMemberValueActiveGameIdWhenGameRunningTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>())).Returns(2);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(1000L);

            var activeGameId = _egmPropertyDataSource.GetMemberValue("Game_Being_Played");
            Assert.AreEqual(activeGameId, 3);
        }

        [TestMethod]
        public void GetMemberValueActiveGameIdWhenGameNotRunningTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>())).Returns(0);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(1000L);

            var activeGameId = _egmPropertyDataSource.GetMemberValue("Game_Being_Played");
            Assert.AreEqual(activeGameId, 0);
        }

        [TestMethod]
        public void HandleGameConnectedEventTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>())).Returns(0);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(1000L);

            var memberValueChangedEventsReceived = 0;
            _egmPropertyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_gameConnectedCallback);
            _gameConnectedCallback(new GameConnectedEvent(false));

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void HandleGameProcessExitedEvent()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);

            Assert.IsNotNull(_gameSelectedCallback);
            _gameSelectedCallback(new GameSelectedEvent(3, 2000, "", false, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));

            var memberValueChangedEventsReceived = 0;
            _egmPropertyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_gameExitedNormalEventCallback);
            _gameProcessExitedEventCallback(new GameProcessExitedEvent(0, false));

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void HandleGameExitedNormalEvent()
        {
            _operatorMenuLauncher.Setup(m => m.IsShowing).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);

            var memberValueChangedEventsReceived = 0;
            _egmPropertyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_gameExitedNormalEventCallback);
            _gameExitedNormalEventCallback(new GameExitedNormalEvent());

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void HandleGameSelectedEventTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);

            var memberValueChangedEventsReceived = 0;
            _egmPropertyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_gameSelectedCallback);
            _gameSelectedCallback(new GameSelectedEvent(3, 2000, "", false, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void HandleDenominationSelectedEventTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(false);

            var memberValueChangedEventsReceived = 0;
            _egmPropertyDataSource.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            Assert.IsNotNull(_denominatonSelectedCallback);
            _denominatonSelectedCallback(new DenominationSelectedEvent(1, 1000L));

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        private IGameDetail BuildGameDetail(int id, string theme, string version, string variationId, decimal minimumRTP)
        {
            var currentGame = new Mock<IGameDetail>();
            currentGame.SetupGet(g => g.Id).Returns(() => id);
            currentGame.SetupGet(g => g.ThemeName).Returns(() => theme);
            currentGame.SetupGet(g => g.Version).Returns(() => version);
            currentGame.SetupGet(g => g.VariationId).Returns(() => variationId);
            currentGame.SetupGet(g => g.MinimumPaybackPercent).Returns(() => minimumRTP);

            return currentGame.Object;
        }

        private IDenomination BuildDenominations(bool active, long denomId, long denomValue, int maxWagerCredit)
        {
            var denom = new Mock<IDenomination>();
            denom.SetupGet(g => g.Active).Returns(() => active);
            denom.SetupGet(g => g.Id).Returns(() => denomId);
            denom.SetupGet(g => g.Value).Returns(() => denomValue);
            denom.SetupGet(g => g.MaximumWagerCredits).Returns(() => maxWagerCredit);
            return denom.Object;
        }

        private void SetupMockAspGameProvider()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 11, 2000, 250)),
                (BuildGameDetail(2, "Wild Yukon", "1.02-66394", "01", 92.99M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(2, "Wild Yukon", "1.02-66394", "01", 92.99M), BuildDenominations(true, 11, 2000, 250)),
                (BuildGameDetail(3, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(3, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 11, 2000, 250)),
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        [TestMethod]
		public void Dispose_ShouldUnsubscribeAll()
		{
            //Call dispose twice - should only unsubscribe/deregister from events once
            _egmPropertyDataSource.Dispose();
            _egmPropertyDataSource.Dispose();

			_eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
		}
    }
}