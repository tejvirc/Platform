namespace Aristocrat.Monaco.Bingo.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Application.Contracts.Localization;
    using Common;
    using Common.Events;
    using Events;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.Models;

    [TestClass]
    public class BingoDisplayConfigurationProviderTests
    {
        private BingoDisplayConfigurationProvider _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);
        private readonly Mock<IDispatcher> _dispatcher = DispatcherMock.Dispatcher;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            MoqServiceManager
                .CreateAndAddService<ILocalizerFactory>(MockBehavior.Default)
                .Setup(m => m.For(It.IsAny<string>()))
                .Returns<string>(_ =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("en-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });

            _eventBus
                .Setup(m => m.Subscribe(It.IsAny<BingoDisplayConfigurationProvider>(), It.IsAny<Action<HostConnectedEvent>>()));

            _gameProvider
                .Setup(m => m.GetAllGames())
                .Returns(new List<IGameDetail>());

            _target = new BingoDisplayConfigurationProvider(
                _dispatcher.Object,
                _eventBus.Object,
                _gameProvider.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, DisplayName = "Null IDispatcher")]
        [DataRow(false, true, false, DisplayName = "Null IEventBus")]
        [DataRow(false, false, true, DisplayName = "Null IGameProvider")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail(
            bool nullDispatcher,
            bool nullEventBus,
            bool nullGameProvider)
        {
            _target = new BingoDisplayConfigurationProvider(
                nullDispatcher ? null : _dispatcher.Object,
                nullEventBus ? null : _eventBus.Object,
                nullGameProvider ? null : _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetSettings_Fail()
        {
            _target.GetSettings(new BingoWindow());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetWindow_Fail()
        {
            _target.GetWindow(new BingoWindow());
        }

        [TestMethod]
        public void OverrideHelpAppearance_Null()
        {
            var current = _target.GetHelpAppearance();
            _target.OverrideHelpAppearance(null);

            Assert.AreEqual(current, _target.GetHelpAppearance());

            _eventBus.Verify(m => m.Publish(It.IsAny<BingoDisplayHelpAppearanceChangedEvent>()), Times.Never());
        }

        [TestMethod]
        public void OverrideHelpAppearance()
        {
            var current = _target.GetHelpAppearance();
            _target.OverrideHelpAppearance(new BingoDisplayConfigurationHelpAppearance());

            Assert.AreNotEqual(current, _target.GetHelpAppearance());

            _eventBus.Verify(m => m.Publish(It.IsAny<BingoDisplayHelpAppearanceChangedEvent>()), Times.Once());
        }

        [TestMethod]
        public void OverrideSettings_KeyNotFound()
        {
            _target.OverrideSettings(new BingoWindow(), new BingoDisplayConfigurationBingoWindowSettings());

            _eventBus.Verify(m => m.Publish(It.IsAny<BingoDisplayHelpAppearanceChangedEvent>()), Times.Never());
        }

        [TestMethod]
        public void OverrideSettings_NullSettings()
        {
            _target.OverrideSettings(new BingoWindow(), null);

            _eventBus.Verify(m => m.Publish(It.IsAny<BingoDisplayHelpAppearanceChangedEvent>()), Times.Never());
        }

        [TestMethod]
        public void OverrideSettings()
        {
            _target.OverrideSettings(BingoWindow.Main, new BingoDisplayConfigurationBingoWindowSettings());

            _eventBus.Verify(m => m.Publish(It.IsAny<BingoDisplayHelpAppearanceChangedEvent>()), Times.Once());
        }
    }
}
