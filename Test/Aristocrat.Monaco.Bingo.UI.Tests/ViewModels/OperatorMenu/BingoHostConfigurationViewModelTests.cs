namespace Aristocrat.Monaco.Bingo.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.IO;
    using Application.Contracts.OperatorMenu;
    using Common.Events;
    using Common.Storage.Model;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;

    [TestClass]
    public class BingoHostConfigurationViewModelTests
    {
        private BingoHostConfigurationViewModel _target;
        private Mock<IBingoDataFactory> _bingoDataFactory;
        private Mock<IHostService> _hostService;
        private Mock<IEventBus> _eventBus;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IPropertiesManager> _propertyManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Default);
            _bingoDataFactory = new Mock<IBingoDataFactory>(MockBehavior.Default);
            _hostService = new Mock<IHostService>(MockBehavior.Default);
            _bingoDataFactory.Setup(x => x.GetHostService()).Returns(_hostService.Object);
            _propertyManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            _target = new BingoHostConfigurationViewModel(false, _pathMapper.Object, _bingoDataFactory.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(bool nullPathMapper, bool nullDataFactory, bool nullHostService)
        {
            if (nullHostService)
            {
                _bingoDataFactory.Setup(x => x.GetHostService()).Returns((IHostService)null);
            }

            _target = new BingoHostConfigurationViewModel(
                false,
                nullPathMapper ? null : _pathMapper.Object,
                nullDataFactory ? null : _bingoDataFactory.Object);
        }

        [DataRow("", 1, true, false, DisplayName = "Empty host name test")]
        [DataRow("=", 1, true, false, DisplayName = "Invalid host name test")]
        [DataRow("localhost", -1, true, false, DisplayName = "Port is below the minimum value")]
        [DataRow("localhost", 65537, true, false, DisplayName = "Port is above the maximum value")]
        [DataRow("localhost", 1, false, false, DisplayName = "Has no changes test")]
        [DataRow("localhost", 1, true, true, DisplayName = "Has no errors test")]
        [DataTestMethod]
        [Ignore("Testing access DB this shouldn't be done for a unit tests")]
        public void SaveTest(string hostName, int port, bool hasChanges, bool saved)
        {
            _pathMapper.Setup(p => p.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo("."));
            _hostService.Setup(x => x.GetHost())
                .Returns(new Host { HostName = hasChanges ? $"{hostName}1" : hostName, Port = port });
            _target.HostName = hostName;
            _target.Port = port;
            _target.Save();
            _hostService.Verify(
                x => x.SaveHost(It.Is<Host>(h => h.HostName == hostName && h.Port == port)),
                saved ? Times.Once() : Times.Never());

            _eventBus.Verify(x => x.Publish(It.IsAny<ForceReconnectionEvent>()), saved ? Times.Once() : Times.Never());
            _eventBus.Verify(
                x => x.Publish(It.IsAny<OperatorMenuSettingsChangedEvent>()),
                saved ? Times.Once() : Times.Never());
        }
    }
}