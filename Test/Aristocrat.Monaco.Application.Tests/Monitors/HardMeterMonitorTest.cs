namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using Application.Monitors;
    using Hardware.Contracts.HardMeter;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Test.Common;

    /// <summary>
    ///     Summary description for HardMeterMonitorTest
    /// </summary>
    [TestClass]
    public class HardMeterMonitorTest
    {
        private const string HardMetersEnabledKey = "Hardware.HardMetersEnabled";
        private const string NoHardMeterPropertyKey = "nohardmeter";
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _properties;
        private HardMeterMonitor _target;
        private Mock<IHardMeter> _hardMeters;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Loose);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _hardMeters = MoqServiceManager.CreateAndAddService<IHardMeter>(MockBehavior.Loose);
            _target = new HardMeterMonitor();

            var selectedConfiguration = new Dictionary<int, int>
            {
                { "Jurisdiction".GetHashCode(), "Quebec VLT".GetHashCode() },
                { "Protocol".GetHashCode(), "Test".GetHashCode() }
            };

            _properties.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(selectedConfiguration);
            _properties.Setup(m => m.GetProperty(HardMetersEnabledKey, true))
                .Returns(true);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();

            //_target.Dispose();
        }

        [TestMethod]
        public void ReceiveEnabledEventTest()
        {
            // VerifyEnablingEvent(new EnabledEvent(EnabledReasons.Service));
        }

        private void VerifyEnablingEvent<T>(T enablingEvent) where T : IEvent
        {
            SetupForRun();

            _disableManager.Setup(m => m.Enable(It.IsAny<Guid>())).Verifiable();

            Action<T> callback = null;

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<T>>()))
                .Callback((object subscriber, Action<T> eventCallback) => { callback = eventCallback; });

            _target.Initialize();

            callback.Invoke(enablingEvent);

            _disableManager.Verify();
        }

        private void SetupForRun()
        {
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<DisabledEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<EnabledEvent>>())).Verifiable();

            _properties.Setup(m => m.GetProperty(NoHardMeterPropertyKey, It.IsAny<bool>())).Returns(false);

            SetupUnsubscribe();
        }

        private void SetupUnsubscribe()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(_target)).Verifiable();
        }
    }
}
