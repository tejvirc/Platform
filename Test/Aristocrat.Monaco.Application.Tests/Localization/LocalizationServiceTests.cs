namespace Aristocrat.Monaco.Application.Tests.Localization
{
    using System;
    using System.Globalization;
    using System.IO;
    using Application.Localization;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class LocalizationServiceTests
    {
        private Mock<IPropertiesManager> _properties;
        private Mock<IEventBus> _eventBus;
        private LocalizationService _target;

        [TestInitialize]
        public void TestInitialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            AddinManager.Initialize(currentDirectory, currentDirectory);
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _properties.Setup(x => x.AddPropertyProvider(It.IsAny<IPropertyProvider>()));

            _properties.Setup(x => x.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<string>()))
                .Returns("");

            _properties.Setup(x => x.SetProperty(It.IsAny<string>(), It.IsAny<object>()));

            _properties.Setup(x => x.GetProperty("Mono.SelectedAddinConfigurationHashCode", It.IsAny<string>()))
                .Returns(It.IsAny<string>());

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<LocalizationConfigurationEvent>>()))
                .Verifiable();

            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>()))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<CurrentCultureChangedEvent>()))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<OperatorCultureChangedEvent>()))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<PlayerCultureChangedEvent>()))
                .Verifiable();

            _eventBus.Setup(x => x.Publish(It.IsAny<OperatorCultureAdded>()))
                .Verifiable();

            _target = new LocalizationService();

            _target.Initialize();

            MoqServiceManager.AddService<ILocalization>(_target);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _target.Dispose();

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void SetCurrentCultureTest()
        {
            var expected = new CultureInfo("fr-CA");

            var defaultCulture = CultureInfo.GetCultureInfo("en-US");

            try
            {
                _target.CurrentCulture = expected;

                var actual = _target.CurrentCulture;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                _target.CurrentCulture = defaultCulture;
            }
        }

        [Ignore]
        [TestMethod]
        public void NoJurisdictionOverridesTest()
        {
            _properties.SetupSequence(x => x.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<string>()))
             .Returns("Test");

            _target.Initialize();
        }
    }
}
