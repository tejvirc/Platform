namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Bootstrap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for Bootstrap and is intended
    ///     to contain all Bootstrap Unit Tests
    /// </summary>
    [TestClass]
    public class BootstrapTest
    {
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;

        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPlatformDisplay> _platformDisplay;
        private Mock<IPropertiesManager> _propertiesManager;
        private Bootstrap _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // get rid of an addin file pulled in by a reference to the Test.Common project.
            File.Delete(Directory.GetCurrentDirectory() + "\\TestPropertiesManager.addin.xml");

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            // use reflection to access private constructor to create the object
            Type[] paramTypes = { };
            object[] paramValues = { };
            var ci = typeof(Bootstrap).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                paramTypes,
                null);
            _target = (Bootstrap)ci.Invoke(paramValues);
            _accessor = new DynamicPrivateObject(_target);

            // set up mock classes
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict, true);
            _platformDisplay = MoqServiceManager.CreateAndAddService<IPlatformDisplay>(MockBehavior.Strict, true);
            _platformDisplay.Setup(m => m.Shutdown(It.IsAny<bool>()));
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict, true);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict, true);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Default, true);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
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

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ParseCommandLineArgumentsHelpTest()
        {
            string[] args = { "/?" };
            var result = (bool)_accessor.ParseCommandLineArguments(args);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ParseCommandLineArgumentDisableCrashHandler()
        {
            string[] args = { "crash-handler=disabled" };
            var result = (bool)_accessor.ParseCommandLineArguments(args);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ParseCommandLineArgumentExtraPropertiesTest()
        {
            // test some user-defined parameters and some malformed entries
            string[] args = { "display=window", "testing=yes", "foo", "bar=" };

            var result = (bool)_accessor.ParseCommandLineArguments(args);
            var pendingProperties = (Dictionary<string, object>)_accessor._pendingProperties;

            Assert.IsFalse(result);
            Assert.AreEqual(3, pendingProperties.Count);
            Assert.IsTrue(
                pendingProperties.ContainsKey("display"),
                "Command-line option \"display\" should be in pendingProperties.");
            Assert.AreEqual("window", pendingProperties["display"]);
            Assert.IsTrue(
                pendingProperties.ContainsKey("testing"),
                "Command-line option \"testing\" should be in pendingProperties.");
            Assert.AreEqual("yes", pendingProperties["testing"]);
            Assert.IsTrue(
                pendingProperties.ContainsKey("foo"),
                "Command-line option \"foo\" should be in pendingProperties.");
            Assert.IsTrue((bool)pendingProperties["foo"]);

            Assert.IsFalse(pendingProperties.ContainsKey("bar"));
            Assert.IsFalse(pendingProperties.ContainsValue("foo"));
            Assert.IsFalse(pendingProperties.ContainsValue("bar"));
        }

        private void SetupMocksForStart()
        {
            // Set up the IServiceManager
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IMessageDisplay)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IPlatformDisplay)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IEventBus))).Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IPropertiesManager)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IPathMapper)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IAddinHelper)))
                .Verifiable();
            MoqServiceManager.Instance.Setup(
                m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is ISystemDisableManager)));
            MoqServiceManager.Instance
                .Setup(m => m.AddServiceAndInitialize(It.Is<IService>(svc => svc is IDummyService)))
                .Verifiable();
            MoqServiceManager.Instance.Setup(m => m.Shutdown()).Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IMessageDisplay)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IPlatformDisplay)))
                .Verifiable();
            MoqServiceManager.Instance.Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IEventBus)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IPropertiesManager))).Verifiable();
            MoqServiceManager.Instance.Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IPathMapper)))
                .Verifiable();
            MoqServiceManager.Instance
                .Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IAddinHelper)))
                .Verifiable();
            MoqServiceManager.Instance.Setup(
                m => m.RemoveService(It.Is<IService>(svc => svc is ISystemDisableManager)));
            MoqServiceManager.Instance
                .Setup(m => m.RemoveService(It.Is<IService>(svc => svc is IDummyService)))
                .Verifiable();

            // Set up the IMessageDisplay service
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Addin Helper")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating System Disable Manager")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Event Bus")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Properties Manager")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Localizer")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Message Display")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Component Registry")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Path Mapper")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Window Launcher")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Creating Platform Display")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Loading Kernel Services")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Loading Kernel Runnables")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Path Mapper")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Aristocrat.Monaco.Kernel.SystemDisableManager"))
                .Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading PlatformDisplay Test Dummy")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Service Test Dummy")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Message Display Service")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Component Registry")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Properties Manager")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Event Bus")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading AddinHelper")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Kernel Runnables")).Verifiable();
            _messageDisplay.Setup(m => m.DisplayStatus("Unloading Kernel Services")).Verifiable();

            // Set up the IEventBus service
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ExitRequestedEvent>>()))
                .Verifiable();
            _eventBus.Setup(m => m.UnsubscribeAll(_target)).Verifiable();

            // Set up the IPropertiesManager service
            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null)
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty("System.HardBoot.Time", It.IsAny<DateTime>())).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty("System.SoftBoot.Time", It.IsAny<DateTime>())).Verifiable();
            var version = Assembly.LoadFile(Path.Combine(AppContext.BaseDirectory, "bootstrap.exe"))
                .GetName()
                .Version.ToString();
            _propertiesManager.Setup(m => m.SetProperty("System.Version", version)).Verifiable();

            // Set up the IPathMapper service
            string path = _accessor.FilePath;
            _pathMapper.Setup(m => m.GetDirectory(path)).Returns(new DirectoryInfo(Directory.GetCurrentDirectory()))
                .Verifiable();
        }
    }
}