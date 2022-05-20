namespace Aristocrat.Monaco.Bingo.Tests.Services.Configuration
{
    using System;
    using Aristocrat.Monaco.Bingo.Common.Storage.Model;
    using Aristocrat.Monaco.Bingo.Services.Configuration;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.ServerApiGateway;
    using Google.Protobuf.Collections;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MessageConfigurationTests
    {
        private MessageConfiguration _target;
        private readonly BingoServerSettingsModel model = new BingoServerSettingsModel();
        private Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<ISystemDisableManager> _disableManager = new Mock<ISystemDisableManager>();

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _target = new MessageConfiguration(_propertiesManager.Object, _disableManager.Object);
        }

        [DataRow(true, false, DisplayName = "PropertiesManager null")]
        [DataRow(false, true, DisplayName = "DisableManager null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest(
            bool propertiesManagerNull,
            bool disableManagerNull)
        {
            _target = new MessageConfiguration(
                propertiesManagerNull ? null : _propertiesManager.Object,
                disableManagerNull ? null : _disableManager.Object);
        }

        [TestMethod]
        public void ConfigureTest()
        {
            var expectedTicket1 = "Ticket 1";
            var expectedTicket2 = "Ticket 2";

            var messageConfigurationAttribute = new RepeatedField<ConfigurationResponse.Types.ClientAttribute>
            {
                new ConfigurationResponse.Types.ClientAttribute { Value = expectedTicket1, Name = MessageConfigurationConstants.Ticket1},
                new ConfigurationResponse.Types.ClientAttribute { Value = expectedTicket2, Name = MessageConfigurationConstants.Ticket2},
            };

            _target.Configure(messageConfigurationAttribute, model);

            _propertiesManager.Verify(x => x.SetProperty(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
