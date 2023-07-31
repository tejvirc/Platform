/*namespace Aristocrat.Monaco.Application.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Application.UI.Events;
    using Cabinet;
    using Cabinet.Contracts;
    using Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.Views;

    /// <summary>
    ///     Summary description for SelectionWindowTest
    /// </summary>
    [TestClass]
    public class SelectionWindowTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPropertiesManager> _propertiesManager;
        private SelectionWindow _target;
        private Mock<ICabinetDetectionService> _cabinetDetection;
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IMultiProtocolConfigurationProvider> _protocolProvider;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);
            _cabinetDetection = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);
            _protocolProvider = MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Default);

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("en-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });

            var touchscreens = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            touchscreens.Setup(m => m.TouchscreensMapped).Returns(true);

            _pathMapper.Setup(mock => mock.GetDirectory("/Assets/Skins")).Returns(new DirectoryInfo(@".\"));

            _propertiesManager.Setup(mock => mock.GetProperty("System.Version", "0.0.0.0")).Returns("12.0.0.0");
            _propertiesManager.Setup(mock => mock.GetProperty("display", "FULLSCREEN")).Returns("FULLSCREEN");
            _propertiesManager.Setup(mock => mock.GetProperty("showMouseCursor", "FALSE")).Returns("FALSE");
            _propertiesManager.Setup(
                    mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", It.IsAny<object>()))
                .Returns(new Dictionary<int, int>());
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>())).Returns(0);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>())).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, false)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.InspectionNameAndVersion, It.IsAny<string>())).Returns("Test");

            _eventBus.Setup(mock => mock.Subscribe(It.IsAny<ButtonDeckNavigator>(), It.IsAny<Action<UpEvent>>())).Verifiable();
            _eventBus.Setup(mock => mock.Publish(It.IsAny<ButtonDeckNavigatorStartedEvent>())).Verifiable();
            _eventBus.Setup(a => a.Subscribe(It.IsAny<SelectionWindow>(), It.IsAny<Action<PageTitleEvent>>()));
            _eventBus.Setup(a => a.Subscribe(It.IsAny<SelectionWindow>(), It.IsAny<Action<CloseConfigWindowEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuPageLoadedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDownEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuPopupEvent>>()));

            _cabinetDetection.Setup(mock => mock.GetDisplayDeviceByItsRole(It.IsAny<DisplayRole>())).Returns(new DisplayDevice());

            MoqServiceManager.Instance.Setup(m => m.AddServiceAndInitialize(It.IsAny<AutoConfigurator>()));

            _target = new SelectionWindow();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            try
            {
                _target.Dispatcher.PumpUntilDry();
            }
            catch (Exception)
            {
                // just eat the exception since it is due to other window threads
                // not shutting down
            }

            _target = null;
            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void SoftwareVersionTest()
        {
            Assert.AreEqual("12.0.0.0", _target.SoftwareVersion);
        }

        [TestMethod]
        public void DemoModeTextTest()
        {
            string expected = "Hello world";
            SetPrivateProperty("DemoModeText", expected);
            Assert.AreEqual(expected, _target.DemoModeText);
        }

        [TestMethod]
        public void WindowTitleTest()
        {
            string expected = "Test Window";
            _target.WindowTitle = expected;
            Assert.AreEqual(expected, _target.WindowTitle);
        }

        /// <summary>
        ///     Sets the target object's non-public, non-static property to the supplied value
        /// </summary>
        /// <param name="propertyName">The name of the private property to set</param>
        /// <param name="value">The new value for the field</param>
        public void SetPrivateProperty(string propertyName, object value)
        {
            PropertyInfo property = typeof(SelectionWindow).GetProperty(propertyName);
            property.GetSetMethod(true).Invoke(_target, new[] { value });
        }
    }
}*/
