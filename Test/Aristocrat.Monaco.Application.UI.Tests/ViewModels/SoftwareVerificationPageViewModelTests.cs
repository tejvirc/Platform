namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.IO;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.UI.Common.Events;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Tests for the SoftwareVerificationPageViewModelTests class
    /// </summary>
    [TestClass]
    public class SoftwareVerificationPageViewModelTests
    {
        private Mock<IComponentRegistry> _componentRego;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IAuthenticationService> _authenticationService;
        private Action<ComponentHashCompleteEvent> _componentHashCompleteEventCallback;
        private Action<AllComponentsHashCompleteEvent> _allComponentHashCompleteEventCallback;
        private CancellationToken _cancelToken;
        private SoftwareVerificationPageViewModel _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuAccess>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Default);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(e => e.Publish(It.IsAny<OperatorMenuWarningMessageEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<OperatorMenuPageLoadedEvent>()));
            _eventBus.Setup(e => e.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<ComponentHashCompleteEvent>>())).Callback<object, Action<ComponentHashCompleteEvent>>((s, c) => _componentHashCompleteEventCallback = c);
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<ComponentAddedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<AllComponentsHashCompleteEvent>>())).Callback<object, Action<AllComponentsHashCompleteEvent>>((s, c) => _allComponentHashCompleteEventCallback = c); ;
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<ComponentRemovedEvent>>()));

            _eventBus.Setup(x => x.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<ComponentAddedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<PrintButtonClickedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<PrintButtonStatusEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<DialogClosedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<SoftwareVerificationPageViewModel>(), It.IsAny<Action<OperatorCultureChangedEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<LampTestLampStateEvent>()));

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            SetupProperties(false);
            _authenticationService = MoqServiceManager.CreateAndAddService<IAuthenticationService>(MockBehavior.Strict);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            _target = new SoftwareVerificationPageViewModel();
            _componentRego = MoqServiceManager.CreateAndAddService<IComponentRegistry>(MockBehavior.Strict);
            _componentRego.SetupGet(c => c.Components).Returns(new List<Component> {
                new Component { ComponentId = "Test Runtime" },
                new Component { ComponentId = "Test Game" },
            });

            _target.LoadedCommand.Execute(null);
        }

        private void SetupProperties(bool valueForShowMasterResult)
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ShowMasterResult, false))
                .Returns(valueForShowMasterResult);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void PerformCalculationsTest()
        {
            SetupPerformCalculationThenExecute();

        }

        [TestMethod]
        public void ComponentHashCompleteEventNoMasterResultTest()
        {
            SetupPerformCalculationThenExecute();
            ComponentHashCompleteEvent evt = new ComponentHashCompleteEvent(new ComponentVerification(1) { ComponentId = "Test Runtime", Result = new byte[] { 135, 172, 236, 23, 205, 157, 205, 32, 167, 22, 204, 44, 246, 116, 23, 183, 28, 138, 112, 22 } }, _cancelToken);
            _componentHashCompleteEventCallback(evt);
            Assert.AreEqual(_target.ComponentSet.First(c => c.ComponentId == "Test Runtime").HashResult, "87ACEC17CD9DCD20A716CC2CF67417B71C8A7016");
        }

        [TestMethod]
        public void ComponentHashCompleteEventMasterResultTest()
        {
            SetupPerformCalculationThenExecute();
            _target.ShowMasterResult = true;
            ComponentHashCompleteEvent evt1 = new ComponentHashCompleteEvent(new ComponentVerification(1) { ComponentId = "Test Runtime", Result = new byte[] { 135, 172, 236, 23, 205, 157, 205, 32, 167, 22, 204, 44, 246, 116, 23, 183, 28, 138, 112, 22 } }, _cancelToken);
            _componentHashCompleteEventCallback(evt1);
            Assert.AreEqual(_target.ComponentSet.First(c => c.ComponentId == "Test Runtime").HashResult, "87ACEC17CD9DCD20A716CC2CF67417B71C8A7016");
            ComponentHashCompleteEvent evt2 = new ComponentHashCompleteEvent(new ComponentVerification(1) { ComponentId = "Test Game", Result = new byte[] { 1, 200, 74, 47, 218, 50, 69, 128, 58, 106, 151, 220, 80, 149, 140, 87, 101, 159, 131, 183 } }, _cancelToken);
            _componentHashCompleteEventCallback(evt2);
            Assert.AreEqual(_target.ComponentSet.First(c => c.ComponentId == "Test Game").HashResult, "01C84A2FDA3245803A6A97DC50958C57659F83B7");
            var expectedResult = new BitArray(new byte[] { 135, 172, 236, 23, 205, 157, 205, 32, 167, 22, 204, 44, 246, 116, 23, 183, 28, 138, 112, 22 }).Xor(new BitArray(new byte[] { 1, 200, 74, 47, 218, 50, 69, 128, 58, 106, 151, 220, 80, 149, 140, 87, 101, 159, 131, 183 }));
            AllComponentsHashCompleteEvent evt = new AllComponentsHashCompleteEvent(false, _cancelToken);
            _allComponentHashCompleteEventCallback(evt);
            Assert.AreEqual(_target.MasterResult, ConvertExtensions.ToPackedHexString(expectedResult));
        }

        [TestMethod]
        public void AllComponentsHashCompleteEventNotCancelledNoMasterResultTest()
        {
            SetupPerformCalculationThenExecute();
            _target.MasterResult = "";
            _target.ShowMasterResult = false;
            ComponentHashCompleteEvent evt1 = new ComponentHashCompleteEvent(new ComponentVerification(1) { ComponentId = "Test Runtime", Result = new byte[] { 135, 172, 236, 23, 205, 157, 205, 32, 167, 22, 204, 44, 246, 116, 23, 183, 28, 138, 112, 22 } }, _cancelToken);
            _componentHashCompleteEventCallback(evt1);
            Assert.AreEqual(_target.ComponentSet.First(c => c.ComponentId == "Test Runtime").HashResult, "87ACEC17CD9DCD20A716CC2CF67417B71C8A7016");
            ComponentHashCompleteEvent evt2 = new ComponentHashCompleteEvent(new ComponentVerification(1) { ComponentId = "Test Game", Result = new byte[] { 1, 200, 74, 47, 218, 50, 69, 128, 58, 106, 151, 220, 80, 149, 140, 87, 101, 159, 131, 183 } }, _cancelToken);
            _componentHashCompleteEventCallback(evt2);
            Assert.AreEqual(_target.ComponentSet.First(c => c.ComponentId == "Test Game").HashResult, "01C84A2FDA3245803A6A97DC50958C57659F83B7");
            AllComponentsHashCompleteEvent evt = new AllComponentsHashCompleteEvent(false, _cancelToken);
            _allComponentHashCompleteEventCallback(evt);
            Assert.AreEqual(_target.MasterResult, "");
        }

        private void SetupPerformCalculationThenExecute()
        {
            // All algorithms use the same codepath in the viewmodel, HmacSha1 has an additional check so using this
            _eventBus.Setup(e => e.Publish(It.IsAny<OperatorMenuPopupEvent>()));
            _target.FormattedHmacKey = "665112169CC0D1DF679D924038CF8DB7141047E1";
            _target.SelectedAlgorithmType =
                SoftwareVerificationPageViewModel.AlgorithmTypes.First(a => a.Type == AlgorithmType.HmacSha1);
            _authenticationService
                .Setup(
                    a => a.GetComponentHashesAsync(
                        AlgorithmType.HmacSha1,
                        It.IsAny<CancellationToken>(),
                        It.IsAny<byte[]>(),
                        It.IsAny<string>(),
                        0)).Callback<AlgorithmType, CancellationToken, byte[], string, long>(
                    (a, c, b, s, l) => _cancelToken = c).Returns(new Task(() => { }));
            _target.CalculateCommand.Execute(null);
        }
    }
}
