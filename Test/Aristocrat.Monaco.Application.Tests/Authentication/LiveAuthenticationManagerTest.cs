namespace Aristocrat.Monaco.Application.Tests.Authentication
{
    using Application.Authentication;
    using Kernel.Contracts.MessageDisplay;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using System;
    using System.IO;
    using Test.Common;

    [TestClass]
    public class LiveAuthenticationManagerTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _properties;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IAudio> _audioService;
        private Mock<ISystemDisableManager> _systemDisableManager;

        private LiveAuthenticationManager _target;

        private Action<PlatformBootedEvent> _onPlatformBootedEvent;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Default);
            _audioService = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Default);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);

            SetDevelopmentKey();

            MockLocalization.Setup(MockBehavior.Default);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(true, DisplayName = "Verify Signature runs after reboot")]
        [DataRow(false, DisplayName = "No Signature verification done and event is not subscribed")]
        public void PlatformRebootInitiatesSignatureVerificationWhenFlagSet(bool runSignatureVerificationAfterReboot)
        {
            _properties.Setup(
                    m => m.GetProperty(ApplicationConstants.RunSignatureVerificationAfterReboot, It.IsAny<object>()))
                .Returns(runSignatureVerificationAfterReboot);

            _target = GetTarget();
            _target.Initialize();

            if (runSignatureVerificationAfterReboot)
            {
                _systemDisableManager.Setup(
                    m => m.Disable(
                        ApplicationConstants.LiveAuthenticationDisableKey,
                        SystemDisablePriority.Immediate,
                        It.IsAny<string>(),
                        It.IsAny<CultureProviderType>(),
                        It.IsAny<object[]>())).Verifiable();

                _eventBus.Setup(evt => evt.Unsubscribe<PlatformBootedEvent>(It.IsAny<object>())).Verifiable();
                Assert.IsNotNull(_onPlatformBootedEvent);
                _onPlatformBootedEvent(new PlatformBootedEvent());
                _systemDisableManager.Verify();
                _eventBus.Verify();
            }
            else
            {
                Assert.IsNull(_onPlatformBootedEvent);
            }
        }

        private LiveAuthenticationManager GetTarget()
        {
            var target = new LiveAuthenticationManager(
                _eventBus.Object,
                _systemDisableManager.Object,
                _properties.Object,
                _pathMapper.Object,
                _audioService.Object);

            _eventBus.Setup(m => m.Subscribe(target, It.IsAny<Action<PlatformBootedEvent>>()))
                .Callback<object, Action<PlatformBootedEvent>>(
                    (tar, func) => { _onPlatformBootedEvent = func; });

            return target;
        }

        private void SetDevelopmentKey()
        {
            DsaKeyParameters developmentKey;
            var privateType = new PrivateType(typeof(LiveAuthenticationManager));
            using (var dummyReader = new StringReader(
                "-----BEGIN PUBLIC KEY-----\n\r" +
                "MIIBuDCCASwGByqGSM44BAEwggEfAoGBAO3pESBopuhI6ZfQL/aErIhP9cbha7V3\n3Lc+wm+/wz6/sB07CXJzdCA+hTuSzh4nEIMC05v9h/WBgMlC9uN4LAuEPvqWYjo5\n7Ms0hM2xiJA2GHu44SammeRXKn4spF5rxYOhTOZprzS/Z13Qx6GdUvGGnkLeUHL2\n5mN7VkpzcMElAhUAhhRWaBTe2b86D7LWrW579zlDdJ0CgYEA42oLebcavFTSshuC\nf2PVnzAYDpUu+vu4qEdl4u5yyaqooPvjqii12YNXAxj+AzTmVWOPwVd1JnBQCc2b\neo/HmVlvb5PWRthf3WS64JGPtv2bnUO9hj+drk94X6vbSOCTV9uC8Be07vKZ+DUh\neWTM6aX4kWaOfLDVvsCINZokOYsDgYUAAoGBALkkquiTj9OuKuQDtsp2Z57h70re\n2NtF/aFLEMQd/44b7O2HxUu+ankaWnScN4vU4nl1SX+lHl8FfL1GsueT1NofidYU\n6obdagfSyca9E4xUQwi4x5rgAiLGt6DWhRFAQ1Mrt9JbB0PgE2RWV38iBEvtNrr9\nECJpA5hEUzBJ01n8\n\r" +
                "-----END PUBLIC KEY-----"))
            {
                developmentKey = (DsaKeyParameters)new PemReader(dummyReader).ReadObject();
            }

            privateType.SetStaticFieldOrProperty("_developmentKey", developmentKey);
        }
    }
}