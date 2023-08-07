namespace Aristocrat.Monaco.Application.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Drm;
    using Contracts.OperatorMenu;
    using Contracts.Settings;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels;
    using UI.Views;
    using GlobalProtocol = global::Protocol;

    /// <summary>
    ///     Summary description for ConfigSelectionPageTest
    /// </summary>
    [TestClass]
    public class ConfigSelectionPageTest
    {
        private const string AcceptText = "Accept";
        private const string NextText = "Next";
        private const string FinishedText = "Finished";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IServiceManager> _serviceManager;
        private Mock<IMultiProtocolConfigurationProvider> _protocolProvider;
        private Mock<IConfigurationUtilitiesProvider> _configurationUtilitiesProvider;
        private Mock<IProtocolCapabilityAttributeProvider> _protocolCapabilityAttributeProvider;

        private ConfigSelectionPageViewModel _target;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
       
        [TestInitialize]
        public void Initialize()
        {
            Logger.InfoFormat("{0}() initialize-start", TestContext.TestName);

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            _configurationUtilitiesProvider = MoqServiceManager.CreateAndAddService<IConfigurationUtilitiesProvider>(MockBehavior.Strict);
            _protocolCapabilityAttributeProvider = MoqServiceManager.CreateAndAddService<IProtocolCapabilityAttributeProvider>(MockBehavior.Strict);

            _configurationUtilitiesProvider.Setup(s => s.GetConfigWizardConfiguration(It.IsAny<Func<ConfigWizardConfiguration>>())).Returns(() =>
                new ConfigWizardConfiguration
                {
                    ProtocolConfiguration = new ConfigWizardConfigurationProtocolConfiguration
                    {
                        ProtocolsAllowed = ProtocolNames.All.Select(s => new GlobalProtocol { Name = EnumParser.ParseOrThrow<CommsProtocol>(s) }).ToArray(),
                        ExclusiveProtocols = Array.Empty<ExclusiveProtocol>(),
                        RequiredFunctionality = Array.Empty<FunctionalityType>()
                    }
                }
            );

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<CloseConfigWindowEvent>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuPageLoadedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PropertyChangedEvent>>(), It.IsAny<Predicate<PropertyChangedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ConfigurationSettingsImportedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDownEvent>>()));
            _protocolProvider = MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Default);

            var touchscreens = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            touchscreens.Setup(m => m.TouchscreensMapped).Returns(true);

            var configurator = MoqServiceManager.CreateAndAddService<IAutoConfigurator>(MockBehavior.Default);
            configurator.Setup(m => m.AutoConfigurationExists).Returns(true);

            _serviceManager.Setup(mock => mock.AddService(It.IsAny<ConfigSelectionPageViewModel>())).Verifiable();

            _propertiesManager
                .Setup(mock => mock.GetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>()))
                .Returns(0);

            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>()))
                .Returns(false);

            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, It.IsAny<bool>()))
                .Returns(false);

            _propertiesManager
                .Setup(mock => mock.GetProperty(ApplicationConstants.SelectedConfigurationKey, null))
                .Returns(null);

            _propertiesManager.Setup(m => m.GetProperty(KernelConstants.IsInspectionOnly, false)).Returns(false);

            _target = new ConfigSelectionPageViewModel();

            _accessor = new DynamicPrivateObject(_target);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }

            Logger.InfoFormat("{0}() initialize-end", TestContext.TestName);

            _target.CanNavigateBackward = true;
            _target.CanNavigateForward = true;
        }

        private void InitializePropertySetters()
        {
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()));
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>())).Returns(string.Empty);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.ShowMode, It.IsAny<object>()));
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.GameRules, It.IsAny<object>()));

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.JurisdictionId, It.IsAny<string>()))
                .Returns(string.Empty);

            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>()));
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>()));

            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, It.IsAny<bool>()));
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            try
            {
                Application.Current.Dispatcher.PumpUntilDry();
            }
            catch
            {

            }

            _target = null;

            Logger.InfoFormat("{0}() cleanup-start", TestContext.TestName);
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            Logger.InfoFormat("{0}() cleanup-end{1}", TestContext.TestName, Environment.NewLine);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(typeof(IConfigWizardNavigator), _target.ServiceTypes.ToArray()[0]);
        }

        [TestMethod]
        public void CanNavigateForwardTest()
        {
            _target.CanNavigateForward = false;
            Assert.IsFalse(_target.CanNavigateForward);
            _target.CanNavigateForward = true;
            Assert.IsTrue(_target.CanNavigateForward);
        }

        [TestMethod]
        public void CanNavigateBackwardTest()
        {
            _target.CanNavigateBackward = false;
            Assert.IsFalse(_target.CanNavigateBackward);
            _target.CanNavigateBackward = true;
            Assert.IsTrue(_target.CanNavigateBackward);
        }

        [TestMethod]
        public void FinishedNoTicketCreatorFoundTest()
        {
            MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _serviceManager.Setup(mock => mock.TryGetService<IIdentityTicketCreator>())
                .Returns((IIdentityTicketCreator)null);

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobCompletedEvent>()));

            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true))
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.PrintIdentity", false)).Returns(true);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _eventBus.Setup(m => m.Publish(It.IsAny<ExitRequestedEvent>()));

            _accessor.Finished(true);

            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void FinishedNoPrinterFoundTest()
        {
            MoqServiceManager.CreateAndAddService<IIdentityTicketCreator>(MockBehavior.Strict);
            _serviceManager.Setup(mock => mock.TryGetService<IPrinter>()).Returns((IPrinter)null);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true))
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.PrintIdentity", false)).Returns(true);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _eventBus.Setup(m => m.Publish(It.IsAny<ExitRequestedEvent>()));

            _accessor.Finished(true);

            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void FinishedWithPrintingTest()
        {
            Ticket ticket = new Ticket();
            var ticketCreator = MoqServiceManager.CreateAndAddService<IIdentityTicketCreator>(MockBehavior.Strict);
            ticketCreator.Setup(mock => mock.CreateIdentityTicket()).Returns(ticket);

            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuPrintJobStartedEvent>()));

            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.Setup(mock => mock.Print(ticket))
                .Returns(Task.FromResult(true))
                .Verifiable();

            _serviceManager.Setup(mock => mock.IsServiceAvailable<IPrinter>()).Returns(true);
            _serviceManager.Setup(mock => mock.GetService<IPrinter>()).Returns(printer.Object);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true))
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.PrintIdentity", false)).Returns(true);
            _eventBus.Setup(m => m.Publish(It.IsAny<ExitRequestedEvent>()));

            _accessor.Finished(true);

            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void BackButtonClickOnSelectionPageTest()
        {
            InitializePropertySetters();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);

            // place page in _selectableConfigurationPages and update variable
            var configurationPages = new Collection<IOperatorMenuPageLoader>
            {
                new TestLoader()
            };

            _accessor._selectableConfigurationPages = configurationPages;

            _accessor._lastWizardSelectedIndex = 1;
            _accessor._selectablePagesDone = false;
            _accessor._onFinishedPage = false;

            _target.BackButtonClicked.Execute(null);

            Assert.AreEqual(0, (int)_accessor._lastWizardSelectedIndex);
            Assert.AreEqual(AcceptText, _target.NextButtonText);
        }

        [TestMethod]
        public void BackButtonClickOnWizardPageOnFinishPageTest()
        {
            InitializePropertySetters();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);

            // place page in _wizardPages and update variable
            var wizardPages = new Collection<IOperatorMenuPageLoader> { new TestLoader() };
            _accessor._wizardPages = wizardPages;

            _accessor._lastWizardSelectedIndex = 1;
            _accessor._selectablePagesDone = true;
            _accessor._onFinishedPage = true;

            _target.BackButtonClicked.Execute(null);

            Assert.AreEqual(0, (int)_accessor._lastWizardSelectedIndex);
            Assert.AreEqual(NextText, _target.NextButtonText);
        }

        [TestMethod]
        public void LoadLayerNoNodesTest()
        {
            _accessor.LoadLayer("Jurisdiction");

            Assert.AreEqual(0, ((Collection<IOperatorMenuPageLoader>)_accessor._wizardPages).Count);
        }

        [TestMethod]
        public void NextButtonClickTestNavigateFromSelectionPageToNextSelectionPage()
        {
            InitializePropertySetters();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);

            // test moving between selection pages
            var configurationPages = new Collection<IOperatorMenuPageLoader>
            {
                new TestLoader(),
                new TestLoader()
            };

            _accessor._selectableConfigurationPages = configurationPages;
            _accessor._lastWizardSelectedIndex = 0;
            _accessor._selectablePagesDone = false;
            _accessor._onFinishedPage = false;

            _target.NextButtonClicked.Execute(null);

            Assert.AreEqual(1, (int)_accessor._lastWizardSelectedIndex);
            Assert.AreEqual(NextText, _target.NextButtonText);
            Assert.IsFalse((bool)_accessor._onFinishedPage);
            Assert.IsFalse((bool)_accessor._selectablePagesDone);
        }

        [TestMethod]
        public void NextButtonClickTestOnLastSelectionPage()
        {
            // test a next button click on the last config selection page, which should prompt the
            // target to subscribe to the PreConfigBootCompletedEvent and post the AddinConfigurationCompleteEvent,
            // then wait for the subscribed event before navigating forward
            var configurationPages = new Collection<IOperatorMenuPageLoader>
            {
                new TestLoader()
            };

            _accessor._selectableConfigurationPages = configurationPages;
            _accessor._lastWizardSelectedIndex = 2;
            _accessor._selectablePagesDone = false;
            _accessor._onFinishedPage = false;

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<PreConfigBootCompleteEvent>>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<AddinConfigurationCompleteEvent>()));
            _eventBus.Setup(m => m.Unsubscribe<PreConfigBootCompleteEvent>(_target));

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>())).Returns(2);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>()));

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>())).Returns(false);
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>()));
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.ShowMode, It.IsAny<object>()));
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.GameRules, It.IsAny<object>()));

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>())).Returns(string.Empty);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<object>()));
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, It.IsAny<bool>()));

            _target.NextButtonClicked.Execute(null);

            Assert.IsFalse((bool)_accessor._selectablePagesDone);
            Assert.AreEqual(3, (int)_accessor._lastWizardSelectedIndex);
            Assert.IsFalse(_target.CanNavigateForward);
            Assert.IsFalse(_target.CanNavigateBackward);
            Assert.AreEqual(AcceptText, _target.NextButtonText);
            Assert.IsFalse((bool)_accessor._onFinishedPage);
        }

        [TestMethod]
        public void HandlePreConfigBootCompleteEventTest()
        {
            InitializePropertySetters();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>()))
                .Returns(true);


            // test that receiving the event causes the target to load wizard pages and navigate to the first one,
            // which will be the completion page
            _accessor._wizardsExtensionPath = "Test";
            _accessor._lastWizardSelectedIndex = 2;
            _accessor._selectablePagesDone = false;
            _accessor._wizardsAdded = false;
            _accessor._onFinishedPage = false;

            _eventBus.Setup(m => m.Unsubscribe<PreConfigBootCompleteEvent>(_target));


            _accessor.HandlePreConfigBootCompleteEvent(null);

            Application.Current.Dispatcher.PumpUntilDry();

            Assert.IsTrue((bool)_accessor._selectablePagesDone);
            Assert.AreEqual(0, (int)_accessor._lastWizardSelectedIndex);
            Assert.IsTrue((bool)_accessor._wizardsAdded);
            Assert.AreEqual(1, ((Collection<IOperatorMenuPageLoader>)_accessor._wizardPages).Count);
            Assert.IsTrue(_target.CanNavigateForward);
            Assert.IsTrue(_target.CanNavigateBackward);
            Assert.AreEqual(FinishedText, _target.NextButtonText);
            Assert.IsTrue((bool)_accessor._onFinishedPage);
        }

        [TestMethod]
        public void NextButtonClickTestNavigateFromWizardPageToWizardPage()
        {
            InitializePropertySetters();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyVerified, false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.EKeyDrive, null)).Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);

            // test that receiving the event causes the target to load wizard pages and navigate to the first one,
            // which will be the completion page
            var wizardPages = new Collection<IOperatorMenuPageLoader>
            {
                new TestLoader(),
                new TestLoader()
            };

            _accessor._wizardPages = wizardPages;
            _accessor._lastWizardSelectedIndex = 0;
            _accessor._selectablePagesDone = true;
            _accessor._wizardsAdded = true;
            _accessor._onFinishedPage = false;

            _target.NextButtonClicked.Execute(null);

            Assert.IsTrue((bool)_accessor._selectablePagesDone);
            Assert.AreEqual(1, (int)_accessor._lastWizardSelectedIndex);
            Assert.IsTrue((bool)_accessor._wizardsAdded);
            Assert.AreEqual(2, ((Collection<IOperatorMenuPageLoader>)_accessor._wizardPages).Count);
            Assert.IsFalse(_target.CanNavigateForward);
            Assert.IsFalse(_target.CanNavigateBackward);
            Assert.AreEqual(NextText, _target.NextButtonText);
            Assert.IsFalse((bool)_accessor._onFinishedPage);
        }

        [TestMethod]
        public void NextButtonTestAlreadyOnFinishPage()
        {
            // test hitting finish on the last page.
            _accessor._lastWizardSelectedIndex = 1;
            _accessor._selectablePagesDone = true;
            _accessor._onFinishedPage = true;

            _serviceManager.Setup(mock => mock.IsServiceAvailable<IPrinter>()).Returns(false);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true))
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.PrintIdentity", false)).Returns(true);
            _propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(null);
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            _propertiesManager.Setup(mock => mock.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, It.IsAny<int>()));
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, It.IsAny<bool>()));
            _propertiesManager.Setup(m => m.SetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, It.IsAny<bool>()));

            _eventBus.Setup(m => m.Publish(It.IsAny<ExitRequestedEvent>()));

            _target.NextButtonClicked.Execute(null);

            Assert.AreEqual(2, (int)_accessor._lastWizardSelectedIndex);
            Assert.IsTrue((bool)_accessor._selectablePagesDone);
            Assert.IsTrue((bool)_accessor._onFinishedPage);
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void Page_LoadedTest()
        {
            _accessor.OnLoaded();

            _eventBus.Verify();
        }
    }
}