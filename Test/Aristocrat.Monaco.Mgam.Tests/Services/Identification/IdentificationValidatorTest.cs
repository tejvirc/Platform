namespace Aristocrat.Monaco.Mgam.Tests.Services.Identification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Action;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Services.Identification;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Common.Events;
    using Gaming.Contracts.InfoBar;
    using Hardware.Contracts.CardReader;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Mgam.Services.Identification;
    using Mgam.Services.PlayerTracking;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class IdentificationValidatorTest
    {
        private const int ReaderId = 1;
        private const int OperatorKeySwitchId = 0;
        private const string OperatorKeySwitchName = "Operator";
        private const string CardRemovedMessage = "Card Removed";
        private const string ReadErrorMessage = "Please re-insert card";
        private const string InvalidCardMessage = "Invalid Card";

        private readonly ActionInfo _operatorAction = new ActionInfo(
            new Guid("{EBB8E262-C96E-4B28-8A9A-3C0CF3189339}"),
            "ATIOperator",
            "Can do Operator operations");

        private readonly ActionInfo _technicianAction = new ActionInfo(
            new Guid("{5F5E2E27-4E31-4FEE-BF0D-03719F4D04C8}"),
            "ATITechnician",
            "Can do Technician operations");

        private Mock<IIdentificationProvider> _identificationProvider;
        private Mock<ILogger<IdentificationValidator>> _logger;
        private Mock<IEventBus> _eventBus;
        private Mock<IEgm> _egm;
        private Mock<IEmployeeLogin> _employeeLogin;
        private Mock<IOperatorMenuLauncher> _operatorMenu;
        private Mock<IPlayerTracking> _playerTracking;
        private Mock<INotificationLift> _notificationLift;
        private Mock<IIdentification> _identification;
        private Mock<IIdReaderProvider> _idReaderProvider;
        private Mock<IIdReader> _idReader;
        private IdentificationValidator _target;

        private Action<OffEvent> _keyOffHandler;
        private Action<OnEvent> _keyOnHandler;
        private Action<HostOnlineEvent> _onlineHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            _identificationProvider = new Mock<IIdentificationProvider>(MockBehavior.Default);
            _logger = new Mock<ILogger<IdentificationValidator>>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _egm = new Mock<IEgm>(MockBehavior.Default);
            _employeeLogin = new Mock<IEmployeeLogin>(MockBehavior.Default);
            _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Default);
            _playerTracking = new Mock<IPlayerTracking>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
            _identification = new Mock<IIdentification>(MockBehavior.Default);
            _idReader = new Mock<IIdReader>(MockBehavior.Default);
            _idReader.SetupGet(i => i.Enabled).Returns(true);
            _idReader.SetupGet(i => i.Connected).Returns(true);
            _idReaderProvider = MoqServiceManager.CreateAndAddService<IIdReaderProvider>(MockBehavior.Default);
            _idReaderProvider.SetupGet(i => i.Adapters).Returns(new List<IIdReader> { _idReader.Object });

            _eventBus
                .Setup(x => x.Subscribe(It.IsAny<IdentificationValidator>(), It.IsAny<Action<OffEvent>>()))
                .Callback<object, Action<OffEvent>>((o, action) => _keyOffHandler = action);
            _eventBus
                .Setup(x => x.Subscribe(It.IsAny<IdentificationValidator>(), It.IsAny<Action<OnEvent>>()))
                .Callback<object, Action<OnEvent>>((o, action) => _keyOnHandler = action);
            _eventBus
                .Setup(x => x.Subscribe(It.IsAny<IdentificationValidator>(), It.IsAny<Action<HostOnlineEvent>>()))
                .Callback<object, Action<HostOnlineEvent>>((o, action) => _onlineHandler = action);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            if (AddinManager.IsInitialized)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }
        }

        [DataRow(false, true, true, true, true, true, true, true, DisplayName = "Null Identification Provider Object")]
        [DataRow(true, false, true, true, true, true, true, true, DisplayName = "Null Logger Object")]
        [DataRow(true, true, false, true, true, true, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, true, true, false, true, true, true, true, DisplayName = "Null EGM Object")]
        [DataRow(true, true, true, true, false, true, true, true, DisplayName = "Null Employee Login Object")]
        [DataRow(true, true, true, true, true, false, true, true, DisplayName = "Null Operator Menu Object")]
        [DataRow(true, true, true, true, true, true, false, true, DisplayName = "Null Player Tracking Object")]
        [DataRow(true, true, true, true, true, true, true, false, DisplayName = "Null Notification Lift Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool identificationProvider,
            bool logger,
            bool eventBus,
            bool egm,
            bool employeeLogin,
            bool operatorMenu,
            bool playerTracking,
            bool notificationLift)
        {
            _target = new IdentificationValidator(
                identificationProvider ? _identificationProvider.Object : null,
                logger ? _logger.Object : null,
                eventBus ? _eventBus.Object : null,
                egm ? _egm.Object : null,
                employeeLogin ? _employeeLogin.Object : null,
                operatorMenu ? _operatorMenu.Object : null,
                playerTracking ? _playerTracking.Object : null,
                notificationLift ? _notificationLift.Object : null);
        }

        [TestMethod]
        public void EmployeeClearValidationTest()
        {
            const bool isHostOnline = true;
            const string cardType = "EMP";
            const string track1Data = "EMP123";
            const int employeeId = 123;
            const string employeeName = "Test Employee";
            const InfoBarRegion expectedRegion = InfoBarRegion.Center;

            ActionInfo[] actions = new[] { _technicianAction };

            InfoBarDisplayTransientMessageEvent actualInfoBarEvent = null;

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<EmployeeLoginResponse> employeeLoginResult =
                MessageResult<EmployeeLoginResponse>.Create(
                    MessageStatus.Success,
                    new EmployeeLoginResponse
                    {
                        Actions = actions,
                        CardString = track1Data,
                        EmployeeId = employeeId,
                        EmployeeName = employeeName,
                        ResponseCode = ServerResponseCode.Ok
                    });

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification.Setup(x => x.EmployeeLogin(track1Data, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(employeeLoginResult))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>((i) => actualInfoBarEvent = i)
                .Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            // Employee needs to be logged on to logoff
            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);
            var result2 = _target.ClearValidation(ReaderId, CancellationToken.None);

            _operatorMenu.Verify(x => x.Close(), Times.Once);
            _eventBus.Verify();
            Assert.AreEqual(CardRemovedMessage, actualInfoBarEvent.Message);
            Assert.AreEqual(expectedRegion, actualInfoBarEvent.Region);
        }

        [TestMethod]
        public void HandleReadErrorTest()
        {
            const bool isHostOnline = false;
            const InfoBarRegion expectedRegion = InfoBarRegion.Center;
            InfoBarDisplayTransientMessageEvent actualInfoBarEvent = null;

            _eventBus.Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>((i) => actualInfoBarEvent = i)
                .Verifiable();

            InitializeTarget(isHostOnline);

            _target.HandleReadError(ReaderId);

            _eventBus.Verify();
            Assert.AreEqual(ReadErrorMessage, actualInfoBarEvent.Message);
            Assert.AreEqual(expectedRegion, actualInfoBarEvent.Region);
        }

        [TestMethod]
        public void KeyTurnedOffCloseMenuTest()
        {
            const bool isHostOnline = false;

            InitializeTarget(isHostOnline);

            _keyOffHandler.Invoke(new OffEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            _operatorMenu.Verify(x => x.Close(), Times.Once);
        }

        [TestMethod]
        public void OfflineEmployeeKeyNotTurnedValidateIdentificationTest()
        {
            const bool isHostOnline = false;
            const string track1Data = "EMP123";

            TrackData trackData = new TrackData { Track1 = track1Data };

            InitializeTarget(isHostOnline);

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _operatorMenu.Verify(x => x.Show(), Times.Never);
        }

        [TestMethod]
        public void OfflineEmployeeKeyTurnedOnAfterValidateIdentificationTest()
        {
            const bool isHostOnline = false;
            const string track1Data = "EMP123";

            TrackData trackData = new TrackData { Track1 = track1Data };

            InitializeTarget(isHostOnline);
            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));
            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);
            _operatorMenu.Verify(x => x.Show(), Times.Once);
        }

        [TestMethod]
        public void OfflineEmployeeKeyTurnedOnWithDisconnectedCardReaderTest()
        {
            const bool isHostOnline = false;
            const string track1Data = "EMP123";

            TrackData trackData = new TrackData { Track1 = track1Data };

            _idReader.SetupGet(i => i.Connected).Returns(false);

            InitializeTarget(isHostOnline);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));
            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);
            _operatorMenu.Verify(x => x.Show(), Times.Never);
        }

        [TestMethod]
        public void OfflineEmployeeKeyTurnedOnBeforeValidateIdentificationTest()
        {
            const bool isHostOnline = false;
            const string track1Data = "EMP123";
            const string expectedEmployeeId = "123";
            const bool expectedIsTechnician = false;

            TrackData trackData = new TrackData { Track1 = track1Data };

            OperatorMenuAccessRequestedEvent actualMenuEvent = null;

            InitializeTarget(isHostOnline);

            _eventBus
                .Setup(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()))
                .Callback<OperatorMenuAccessRequestedEvent>((e) => actualMenuEvent = e)
                .Verifiable();

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            _operatorMenu.Verify(x => x.Show(), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()), Times.Once);

            Assert.AreEqual(expectedIsTechnician, actualMenuEvent.IsTechnician);
            Assert.AreEqual(expectedEmployeeId, actualMenuEvent.EmployeeId);
        }

        [TestMethod]
        public void OnlineOperatorRoleValidateIdentification()
        {
            const bool isHostOnline = true;
            const string cardType = "EMP";
            const string track1Data = "EMP123";
            const int employeeId = 123;
            const string employeeName = "Test Employee";
            const bool expectedIsTechnician = false;

            ActionInfo[] actions = new[] { _operatorAction };
            int expectedNotifyCode = (int)NotificationCode.EmployeeLoggedIn;

            OperatorMenuAccessRequestedEvent actualMenuEvent = null;

            int actualNotifyCode = -1;
            string actualNotifyParam = string.Empty;

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<EmployeeLoginResponse> employeeLoginResult =
                MessageResult<EmployeeLoginResponse>.Create(
                    MessageStatus.Success,
                    new EmployeeLoginResponse
                    {
                        Actions = actions,
                        CardString = track1Data,
                        EmployeeId = employeeId,
                        EmployeeName = employeeName,
                        ResponseCode = ServerResponseCode.Ok
                    });

            _eventBus
                .Setup(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()))
                .Callback<OperatorMenuAccessRequestedEvent>((e) => actualMenuEvent = e)
                .Verifiable();

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification.Setup(x => x.EmployeeLogin(track1Data, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(employeeLoginResult))
                .Verifiable();

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _identification.Verify();
            _operatorMenu.Verify(x => x.Show(), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()), Times.Once);
            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
            Assert.AreEqual(track1Data, actualNotifyParam);
            Assert.AreEqual(expectedIsTechnician, actualMenuEvent.IsTechnician);
            Assert.AreEqual(employeeId.ToString(), actualMenuEvent.EmployeeId);
        }

        [TestMethod]
        public void OnlinePlayerValidateIdentification()
        {
            const bool isHostOnline = true;
            const string cardType = "PLY";
            const string track1Data = "PLY123";
            const int playerPoints = 123;
            const string playerName = "Test Player";
            const string playerInfo = "Welcome";

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<PlayerTrackingLoginResponse> playerTrackingLoginResult =
                MessageResult<PlayerTrackingLoginResponse>.Create(
                    MessageStatus.Success,
                    new PlayerTrackingLoginResponse
                    {
                        PlayerName = playerName,
                        PlayerPoints = playerPoints,
                        PromotionalInfo = playerInfo,
                        ResponseCode = ServerResponseCode.Ok
                    });

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification
                .Setup(x => x.PlayerTrackingLogin(track1Data, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(playerTrackingLoginResult))
                .Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _identification.Verify();
            _playerTracking.Verify(x => x.StartPlayerSession(playerName, playerPoints, playerInfo), Times.Once);
        }

        [TestMethod]
        public void OnlineTechnicianRoleValidateIdentification()
        {
            const bool isHostOnline = true;
            const string cardType = "EMP";
            const string track1Data = "EMP123";
            const int employeeId = 123;
            const string employeeName = "Test Employee";
            const bool expectedIsTechnician = true;

            ActionInfo[] actions = new[] { _technicianAction };
            int expectedNotifyCode = (int)NotificationCode.EmployeeLoggedIn;

            OperatorMenuAccessRequestedEvent actualMenuEvent = null;

            int actualNotifyCode = -1;
            string actualNotifyParam = string.Empty;

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<EmployeeLoginResponse> employeeLoginResult =
                MessageResult<EmployeeLoginResponse>.Create(
                    MessageStatus.Success,
                    new EmployeeLoginResponse
                    {
                        Actions = actions,
                        CardString = track1Data,
                        EmployeeId = employeeId,
                        EmployeeName = employeeName,
                        ResponseCode = ServerResponseCode.Ok
                    });

            _eventBus
                .Setup(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()))
                .Callback<OperatorMenuAccessRequestedEvent>((e) => actualMenuEvent = e)
                .Verifiable();

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification.Setup(x => x.EmployeeLogin(track1Data, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(employeeLoginResult))
                .Verifiable();

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _identification.Verify();
            _operatorMenu.Verify(x => x.Show(), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<OperatorMenuAccessRequestedEvent>()), Times.Once);
            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
            Assert.AreEqual(track1Data, actualNotifyParam);
            Assert.AreEqual(expectedIsTechnician, actualMenuEvent.IsTechnician);
            Assert.AreEqual(employeeId.ToString(), actualMenuEvent.EmployeeId);
        }

        [TestMethod]
        public void OnlineUnknownRoleValidateIdentification()
        {
            const bool isHostOnline = true;
            const string cardType = "EMP";
            const string track1Data = "EMP123";
            const int employeeId = 123;
            const string employeeName = "Test Employee";

            ActionInfo[] actions = new ActionInfo[0];
            var expectedNotifyCode = NotificationCode.EmployeeLoggedIn;

            int actualNotifyCode = -1;
            string actualNotifyParam = string.Empty;

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<EmployeeLoginResponse> employeeLoginResult =
                MessageResult<EmployeeLoginResponse>.Create(
                    MessageStatus.Success,
                    new EmployeeLoginResponse
                    {
                        Actions = actions,
                        CardString = track1Data,
                        EmployeeId = employeeId,
                        EmployeeName = employeeName,
                        ResponseCode = ServerResponseCode.Ok
                    });

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification.Setup(x => x.EmployeeLogin(track1Data, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(employeeLoginResult))
                .Verifiable();

            _notificationLift
                .Setup(x => x.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>()))
                .Callback<NotificationCode, string>(
                    (code, param) =>
                    {
                        actualNotifyCode = (int)code;
                        actualNotifyParam = param;
                    })
                .Returns(Task.FromResult(0)).Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _identification.Verify();
            _operatorMenu.Verify(x => x.Show(), Times.Never);
            _notificationLift.Verify();

            Assert.AreEqual((int)expectedNotifyCode, actualNotifyCode);
            Assert.AreEqual(track1Data, actualNotifyParam);
        }

        [TestMethod]
        public void OnlineUnknownCardTypeValidateIdentification()
        {
            const bool isHostOnline = true;
            const string cardType = "";
            const string track1Data = "123";
            const InfoBarRegion expectedRegion = InfoBarRegion.Center;
            InfoBarDisplayTransientMessageEvent actualInfoBarEvent = null;

            TrackData trackData = new TrackData { Track1 = track1Data };
            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.InvalidCardString
                    });

            _eventBus.Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>((i) => actualInfoBarEvent = i)
                .Verifiable();

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);

            _identification.Verify();
            _operatorMenu.Verify(x => x.Show(), Times.Never);
            _eventBus.Verify();

            Assert.AreEqual(InvalidCardMessage, actualInfoBarEvent.Message);
            Assert.AreEqual(expectedRegion, actualInfoBarEvent.Region);
        }

        [TestMethod]
        public void PlayerClearValidationTest()
        {
            const bool isHostOnline = true;
            const string cardType = "PLY";
            const string track1Data = "PLY123";
            const int playerPoints = 123;
            const string playerName = "Test Player";
            const string playerInfo = "Welcome";
            const InfoBarRegion expectedRegion = InfoBarRegion.Center;

            TrackData trackData = new TrackData { Track1 = track1Data };
            InfoBarDisplayTransientMessageEvent actualInfoBarEvent = null;

            MessageResult<GetCardTypeResponse> getCardTypeResult =
                MessageResult<GetCardTypeResponse>.Create(
                    MessageStatus.Success,
                    new GetCardTypeResponse
                    {
                        CardString = track1Data,
                        CardType = cardType,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<PlayerTrackingLoginResponse> playerTrackingLoginResult =
                MessageResult<PlayerTrackingLoginResponse>.Create(
                    MessageStatus.Success,
                    new PlayerTrackingLoginResponse
                    {
                        PlayerName = playerName,
                        PlayerPoints = playerPoints,
                        PromotionalInfo = playerInfo,
                        ResponseCode = ServerResponseCode.Ok
                    });

            MessageResult<PlayerTrackingLogoffResponse> playerTrackingLogoffResult =
                MessageResult<PlayerTrackingLogoffResponse>.Create(
                    MessageStatus.Success,
                    new PlayerTrackingLogoffResponse
                    {
                        ResponseCode = ServerResponseCode.Ok
                    });

            _identification
                .Setup(x => x.GetCardType(track1Data, null, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getCardTypeResult))
                .Verifiable();

            _identification
                .Setup(x => x.PlayerTrackingLogin(track1Data, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(playerTrackingLoginResult))
                .Verifiable();

            _identification
                .Setup(x => x.PlayerTrackingLogoff(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(playerTrackingLogoffResult))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>(i => actualInfoBarEvent = i)
                .Verifiable();

            InitializeTarget(isHostOnline, _identification.Object);

            // Player needs to be logged on to logoff
            var result = _target.ValidateIdentification(ReaderId, trackData, CancellationToken.None);
            var result2 = _target.ClearValidation(ReaderId, CancellationToken.None);

            _eventBus.Verify();
            _playerTracking.Verify(x => x.EndPlayerSession(), Times.Once);
            _identification.Verify(x => x.PlayerTrackingLogoff(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(CardRemovedMessage, actualInfoBarEvent.Message);
            Assert.AreEqual(expectedRegion, actualInfoBarEvent.Region);
        }

        [TestMethod]
        public void CanOpenOperatorMenuWhenIdReaderDisconnected()
        {
            const bool isHostOnline = false;
            _idReaderProvider.SetupGet(i => i.Adapters).Returns(new List<IIdReader>());

            InitializeTarget(isHostOnline);
            _keyOnHandler.Invoke(new OnEvent(OperatorKeySwitchId, OperatorKeySwitchName));
            _operatorMenu.Verify(x => x.Show(), Times.Once);
        }

        private void InitializeTarget(bool isHostOnline, IIdentification identification = null)
        {
            _target = new IdentificationValidator(
                _identificationProvider.Object,
                _logger.Object,
                _eventBus.Object,
                _egm.Object,
                _employeeLogin.Object,
                _operatorMenu.Object,
                _playerTracking.Object,
                _notificationLift.Object);

            if (isHostOnline)
            {
                _onlineHandler.Invoke(new HostOnlineEvent("127.0.0.1"));
            }

            _egm.Setup(x => x.GetService<IIdentification>()).Returns(identification);

            _target.Initialize();
        }
    }
}
