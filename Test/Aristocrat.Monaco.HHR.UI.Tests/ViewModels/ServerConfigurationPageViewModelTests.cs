namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System.Globalization;
    using System.IO;
    using Hhr.UI.ViewModels;
    using Application.Contracts.Localization;
    using Kernel;
    using Test.Common;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;

    [TestClass]
    public class ServerConfigurationPageViewModelTests
    {
        private ServerConfigurationPageViewModel _target;
        private dynamic _accessor;
        private Mock<IServiceManager> _serviceManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Default);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(HHRPropertyNames.ServerTcpIp, It.IsAny<string>())).Returns(Client.Messages.HhrConstants.DefaultServerTcpIp);
            _propertiesManager.Setup(m => m.GetProperty(HHRPropertyNames.ServerTcpPort, It.IsAny<int>())).Returns(Client.Messages.HhrConstants.DefaultServerTcpPort);
            _propertiesManager.Setup(m => m.GetProperty(HHRPropertyNames.ServerUdpPort, It.IsAny<int>())).Returns(Client.Messages.HhrConstants.DefaultServerUdpPort);
            _propertiesManager.Setup(m => m.GetProperty(HHRPropertyNames.EncryptionKey, It.IsAny<string>())).Returns(Client.Messages.HhrConstants.DefaultEncryptionKey);
            _propertiesManager.Setup(m => m.GetProperty(HHRPropertyNames.ManualHandicapMode, It.IsAny<string>())).Returns(Client.Messages.HhrConstants.DetectPickMode);
            _propertiesManager.Setup(m => m.SetProperty(HHRPropertyNames.ServerTcpIp, It.IsAny<string>()));
            _propertiesManager.Setup(m => m.SetProperty(HHRPropertyNames.ServerTcpPort, It.IsAny<int>()));
            _propertiesManager.Setup(m => m.SetProperty(HHRPropertyNames.ServerUdpPort, It.IsAny<int>()));
            _propertiesManager.Setup(m => m.SetProperty(HHRPropertyNames.EncryptionKey, It.IsAny<string>()));
            _propertiesManager.Setup(m => m.SetProperty(HHRPropertyNames.ManualHandicapMode, It.IsAny<string>()));

            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                  name =>
                  {
                      var localizer = new Mock<ILocalizer>();
                      localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                      localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns(" ");
                      return localizer.Object;
                  });

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<OperatorMenuSettingsChangedEvent>()));
            _eventBus.Setup(m => m.Publish(It.IsAny<ExitRequestedEvent>()));

            _target = new ServerConfigurationPageViewModel(false);
            _accessor = new DynamicPrivateObject(_target);

            _accessor.OnLoaded();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [DataRow("10.0.3.108")]
        public void IpAddress_SetToValidAddress_HasNoErrors(string ipAddress)
        {
            _target.IpAddress = ipAddress;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.IpAddress)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerTcpIp, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("10.0.3.1080")]
        [DataRow("1.2.3.4")]
        [DataRow("1234")]
        [DataRow("a.b.c.d")]
        public void IpAddress_SetToInvalidValue_HasErrors(string ipAddress)
        {
            _target.IpAddress = ipAddress;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsNotNull(_accessor.GetErrors(nameof(_target.IpAddress)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerTcpIp, It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [DataRow("1")]
        [DataRow("2059")]
        [DataRow("32767")]
        public void TcpPortNumber_SetToValidValue_HasNoErrors(string portNumber)
        {
            _target.TcpPortNumber = portNumber;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.TcpPortNumber)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerTcpPort, It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        [DataRow("1")]
        [DataRow("1234")]
        [DataRow("65535")]
        public void UdpPortNumber_SetToValidValue_HasNoErrors(string portNumber)
        {
            _target.UdpPortNumber = portNumber;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.UdpPortNumber)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerUdpPort, It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("-2")]
        [DataRow(null)]
        [DataRow("65536")]
        [DataRow("a")]
        public void TcpPortNumber_SetToInvalidValue_HasErrors(string portNumber)
        {
            _target.TcpPortNumber = portNumber;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.TcpPortNumber)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerTcpPort, It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("0")]
        [DataRow(null)]
        [DataRow("65536")]
        [DataRow("a")]
        public void UdpPortNumber_SetToInvalidValue_HasErrors(string portNumber)
        {
            _target.UdpPortNumber = portNumber;
            Assert.IsTrue(_accessor.HasErrors);
            Assert.IsTrue(_accessor.PropertyHasErrors(nameof(_target.UdpPortNumber)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.ServerUdpPort, It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("xyz123")]
        [DataRow(null)]
        public void EncryptionKey_SetToValidValue_HasNoErrors(string encryptionKey)
        {
            _target.EncryptionKey = encryptionKey;
            Assert.IsFalse(_accessor.HasErrors);
            Assert.IsFalse(_accessor.PropertyHasErrors(nameof(_target.EncryptionKey)));

            _accessor.ApplyServerConfigurationCommand.Execute(null);
            _propertiesManager.Verify(m => m.SetProperty(HHRPropertyNames.EncryptionKey, It.IsAny<string>()), Times.Once);
        }
    }
}
