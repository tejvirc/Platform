
namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Application.Monitors;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This contains the unit tests for the NoteAcceptorMonitor class
    /// </summary>
    [TestClass]
    public class NoteAcceptorMonitorTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IAudio> _audioService;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IMessageDisplay> _messageDisplay;
        private static Mock<ISystemDisableManager> _systemDisableManager;

        private Mock<IDisposable> _disposable;

        /// <summary>
        ///     Tracks how many messages are displayed;
        /// </summary>
        private List<string> _displayedMessages;

        private NoteAcceptorMonitor _target;


        [ClassInitialize()]
        public static void CLassInitialization(TestContext context)
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
        }

        [ClassCleanup()]
        public static void TestClassCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            MockLocalization.Setup(MockBehavior.Strict);
            MockLocalization.Localizer.Setup(m => m.GetString(It.IsAny<string>(), It.IsAny<Action<Exception>>())).Returns(string.Empty);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Loose);
            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Loose);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Loose);
            _persistentStorageAccessor =
                MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Loose);
            _persistentStorageTransaction = MoqServiceManager.CreateAndAddService<IPersistentStorageTransaction>(MockBehavior.Loose);
            _scopedTransaction = MoqServiceManager.CreateAndAddService<IScopedTransaction>(MockBehavior.Loose);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _noteAcceptor.Setup(m => m.Enabled).Returns(true);
            _noteAcceptor.Setup(m => m.LogicalState).Returns(NoteAcceptorLogicalState.InEscrow);
            _propertiesManager.Setup(m => m.AddPropertyProvider(It.IsAny<IPropertyProvider>()));

            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.LockupCulture, It.IsAny<object>()))
                .Returns(CultureFor.Operator);

            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);
            _persistentStorage.Setup(m => m.ScopedTransaction()).Returns(_scopedTransaction.Object);

            _persistentStorageAccessor.Setup(m => m[It.IsAny<String>()]).Returns(true);
            _persistentStorageAccessor.Setup(m => m.StartTransaction()).Returns(_persistentStorageTransaction.Object);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.AlertVolumeKey, It.IsAny<byte>()))
                .Returns((byte)100);

            _displayedMessages = new List<string>();

            _target = new NoteAcceptorMonitor();
        }


        [TestMethod]
        public void ConstructorTest()
        {
            Assert.AreEqual("NoteAcceptor", _target.DeviceName);
            Assert.AreEqual(typeof(NoteAcceptorMonitor).Name, _target.Name);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(INoteAcceptorMonitor)));
        }

        [TestMethod]
        public void NoAudioAlertWhenDisconnected()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.NoteAcceptorErrorSoundKey, It.IsAny<object>()))
                .Returns(string.Empty);

            Action<DisconnectedEvent> handler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisconnectedEvent>>()))
                .Callback<object, Action<DisconnectedEvent>>((subscriber, callback) => handler = callback);

            MockDisableManager(true, ApplicationConstants.NoteAcceptorDisconnectedGuid);

            _target.Initialize();

            Assert.IsNotNull(handler);
            handler(new DisconnectedEvent());

            _audioService.Verify(
                m => m.Play(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Never);
            Assert.AreEqual(1, _displayedMessages.Count);
            _systemDisableManager.Verify();
        }

        [TestMethod]
        public void ExpectAudioAlertWhenDisconnected()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.NoteAcceptorErrorSoundKey, It.IsAny<object>()))
                .Returns("Test.ogg");

            Action<DisconnectedEvent> handler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisconnectedEvent>>()))
                .Callback<object, Action<DisconnectedEvent>>((subscriber, callback) => handler = callback);

            MockDisableManager(true, ApplicationConstants.NoteAcceptorDisconnectedGuid);

            _target.Initialize();

            Assert.IsNotNull(handler);
            handler(new DisconnectedEvent());

            _audioService.Verify(
                m => m.Play("Test.ogg", It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Once);
            Assert.AreEqual(1, _displayedMessages.Count);
            _systemDisableManager.Verify();
        }

        [TestMethod]
        public void NoAudioAlertForDisconnectionWhenAuditMenuOpened()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.NoteAcceptorErrorSoundKey, It.IsAny<object>()))
                .Returns("Test.ogg");

            Action<DisconnectedEvent> handler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisconnectedEvent>>()))
                .Callback<object, Action<DisconnectedEvent>>((subscriber, callback) => handler = callback);

            Action<OperatorMenuEnteredEvent> operatorMenuEnteredEventHandler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>((subscriber, callback) => operatorMenuEnteredEventHandler = callback);

            MockDisableManager(true, ApplicationConstants.NoteAcceptorDisconnectedGuid);

            _target.Initialize();

            Assert.IsNotNull(operatorMenuEnteredEventHandler);
            operatorMenuEnteredEventHandler(new OperatorMenuEnteredEvent());

            Assert.IsNotNull(handler);
            handler(new DisconnectedEvent());

            _audioService.Verify(
                m => m.Play(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Never);
            Assert.AreEqual(1, _displayedMessages.Count);
            _systemDisableManager.Verify();
        }

        [TestMethod]
        public void ExpectAudioAlertForDisconnectionWhenAuditMenuOpened()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.NoteAcceptorErrorSoundKey, It.IsAny<object>()))
                .Returns("Test.ogg");

            Action<DisconnectedEvent> handler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisconnectedEvent>>()))
                .Callback<object, Action<DisconnectedEvent>>((subscriber, callback) => handler = callback);

            Action<OperatorMenuEnteredEvent> operatorMenuEnteredEventHandler = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>((subscriber, callback) => operatorMenuEnteredEventHandler = callback);

            MockDisableManager(true, ApplicationConstants.NoteAcceptorDisconnectedGuid);

            _target.Initialize();
            PrivateObject targetPrivateObject = new PrivateObject(_target);
            targetPrivateObject.SetField("_stopAlarmWhenAuditMenuOpened", false);

            Assert.IsNotNull(operatorMenuEnteredEventHandler);
            operatorMenuEnteredEventHandler(new OperatorMenuEnteredEvent());

            Assert.IsNotNull(handler);
            handler(new DisconnectedEvent());

            _audioService.Verify(
                m => m.Play(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null), Times.Once);
            Assert.AreEqual(1, _displayedMessages.Count);
            _systemDisableManager.Verify();
        }

        private void MockDisableManager(bool disable, Guid disableGuid, string msg = "")
        {
            if (disable)
            {
                _systemDisableManager.Setup(
                        m => m.Disable(
                            disableGuid,
                            It.IsAny<SystemDisablePriority>(),
                            It.Is<Func<string>>(x => x.Invoke() == msg),
                            null))
                    .Callback(
                        (
                            Guid enableKey,
                            SystemDisablePriority priority,
                            Func<string> disableReason,
                            Type type) =>
                        {
                            _displayedMessages.Add(disableReason.Invoke());
                        })
                    .Verifiable();
            }
        }

    }
}
