namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using Application.Contracts.OperatorMenu;
    using Cabinet;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.IO;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class AttractPreviewViewModelTest
    {
        private const string TopDefaultVideo = "Top_Default_Video";
        private const string TopperDefaultVideo = "Topper_Default_Video";
        private const int waitTimeout = 5000;

        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private readonly IDictionary<string, Action> _propertyListeners = new Dictionary<string, Action>();

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ICabinetDetectionService> _cabinetService;
        private Mock<IAttractConfigurationProvider> _attractProvider;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IEventBus> _eventBus;
        private Mock<IOperatorMenuLauncher> _operatorMenu;

        private List<IAttractInfo> _attractInfo;
        private List<IGameDetail> _gameDetail;

        private LobbyConfiguration _lobbyConfiguration;

        private AttractPreviewViewModel _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Default);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _eventBus.Setup(
                e => e.Subscribe(
                    It.IsAny<AttractPreviewViewModel>(),
                    It.IsAny<Action<OperatorMenuExitingEvent>>()));

            _cabinetService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            _attractProvider =
                MoqServiceManager.CreateAndAddService<IAttractConfigurationProvider>(MockBehavior.Default);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Default);

            _operatorMenu = MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            _operatorMenu.Setup(o => o.PreventExit());

            _gameDetail = MockGameInfo.GetMockGameDetailInfo().ToList();
            _gameProvider.Setup(g => g.GetAllGames()).Returns(_gameDetail);
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(p => p.SetProperty(It.IsAny<string>(), It.IsAny<object>()));

            _attractInfo = MockGameInfo.GetMockAttractInfo().ToList();
            _attractProvider.Setup(a => a.GetAttractSequence()).Returns(_attractInfo);

            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SlotAttractSelected, It.IsAny<bool>())).Returns(_attractInfo.Any(g => g.GameType == GameType.Slot));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.KenoAttractSelected, It.IsAny<bool>())).Returns(_attractInfo.Any(g => g.GameType == GameType.Keno));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.PokerAttractSelected, It.IsAny<bool>())).Returns(_attractInfo.Any(g => g.GameType == GameType.Poker));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.BlackjackAttractSelected, It.IsAny<bool>())).Returns(_attractInfo.Any(g => g.GameType == GameType.Blackjack));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.RouletteAttractSelected, It.IsAny<bool>())).Returns(_attractInfo.Any(g => g.GameType == GameType.Roulette));

            SetupMockLobbyConfig();

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            MoqServiceManager.RemoveInstance();
            _target.PropertyChanged -= OnPropertyChanged;
        }

        [DataRow(DisplayRole.Top, true)]
        [DataRow(DisplayRole.Top, false)]
        [DataRow(DisplayRole.Topper, true)]
        [DataRow(DisplayRole.Topper, false)]
        [DataTestMethod]
        public void WhenDisplayNotAvailableHidePreviewScreenForTheDisplay(DisplayRole role, bool available)
        {
            SetupCabinetService(available && role == DisplayRole.Top, available && role == DisplayRole.Topper);

            CreateTarget();

            switch (role)
            {
                case DisplayRole.Topper:
                    Assert.AreEqual(available, _target.IsTopperAttractVisible);
                    break;

                case DisplayRole.Top:
                    Assert.AreEqual(available, _target.IsTopAttractVisible);
                    break;
            }
        }

        [DataRow(true, true, true, 480.0, 280.0)]
        [DataRow(false, true, true, 600.0, 350.0)]
        [DataRow(true, false, true, 600.0, 350.0)]
        [DataRow(true, true, false, 600.0, 350.0)]
        [DataRow(true, false, false, 600.0, 350.0)]
        [DataRow(false, true, false, 600.0, 350.0)]
        [DataRow(false, false, true, 600.0, 350.0)]
        [DataTestMethod]
        public void VideoSizeTest(
            bool topVisible,
            bool topperVisible,
            bool bottomVisible,
            double expectedWidth,
            double expectedHeight)
        {
            SetupMockLobbyConfig(false, bottomVisible);
            SetupCabinetService(topVisible, topperVisible);
            CreateTarget();
            _target.LoadedCommand.Execute(null);
            Assert.AreEqual(expectedWidth, _target.AttractVideoWidth);
            Assert.AreEqual(expectedHeight, _target.AttractVideoHeight);
        }

        [TestMethod]
        public void WhenAttractListEmptyPlayDefaultTopAndTopperAttract()
        {
            SetupCabinetService();

            _attractProvider.Setup(a => a.GetAttractSequence()).Returns(new List<IAttractInfo>());

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            Assert.IsTrue(_target.IsTopAttractFeaturePlaying);
            Assert.IsTrue(_target.IsTopperAttractFeaturePlaying);

            Assert.AreEqual(TopperDefaultVideo, _target.TopperAttractVideoPath);
            Assert.AreEqual(TopDefaultVideo, _target.TopAttractVideoPath);

        }

        [TestMethod]
        public void WhenAttractListEmptyAndBottomAttractEnabledPlayDefaultBottomAttract()
        {

            SetupCabinetService();

            _attractProvider.Setup(a => a.GetAttractSequence()).Returns(new List<IAttractInfo>());

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            Assert.AreEqual(true, _target.IsBottomAttractVisible);
            Assert.AreEqual(true, _target.IsBottomAttractFeaturePlaying);

            Assert.AreEqual(TopDefaultVideo, _target.BottomAttractVideoPath);
        }

        [TestMethod]
        public void WhenAttractListEmptyAndBottomAttractDisabledDoNotPlayDefaultBottomAttract()
        {
            SetupMockLobbyConfig(false, false);

            SetupCabinetService();

            _attractProvider.Setup(a => a.GetAttractSequence()).Returns(new List<IAttractInfo>());

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            Assert.AreEqual(false, _target.IsBottomAttractVisible);
            Assert.AreEqual(false, _target.IsBottomAttractFeaturePlaying);

            Assert.IsTrue(string.IsNullOrEmpty(_target.BottomAttractVideoPath));
        }

        [TestMethod]
        public void WhenAttractListNotEmptyEnsureTopAndTopperVideosArePlayedInSequence()
        {
            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            var expectedCurrentAttractIndex = 0;
            var attractCount = _attractInfo.Count(x => x.IsSelected);

            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);
            SetupPropertyListener(nameof(_target.TopperAttractVideoPath));

            for (var i = 0; i < attractCount; i++)
            {
                var currentGame = _gameDetail.Single(x => x.ThemeId == _attractInfo[i].ThemeId);

                Assert.IsTrue(_target.IsTopAttractFeaturePlaying);
                Assert.IsTrue(_target.IsTopperAttractFeaturePlaying);

                Assert.AreEqual(currentGame.LocaleGraphics[_target.ActiveLocaleCode].TopperAttractVideo, _target.TopperAttractVideoPath);
                Assert.AreEqual(currentGame.LocaleGraphics[_target.ActiveLocaleCode].TopAttractVideo, _target.TopAttractVideoPath);

                expectedCurrentAttractIndex = (++expectedCurrentAttractIndex) % attractCount;
                _target.OnTopGameAttractCompleteHandler(null, null);

                Assert.IsTrue(_waiter.WaitOne(waitTimeout));
                Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);
                _waiter.Reset();
            }
        }

        [TestMethod]
        public void WhenAttractListNotEmptyEnsureIfEnabledBottomVideosArePlayedInSequence()
        {
            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            var expectedCurrentAttractIndex = 0;
            var attractCount = _attractInfo.Count(x => x.IsSelected);
            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);

            Assert.AreEqual(true, _target.IsBottomAttractVisible);
            Assert.AreEqual(true, _target.IsBottomAttractFeaturePlaying);
            SetupPropertyListener(nameof(_target.BottomAttractVideoPath));

            for (var i = 0; i < attractCount; i++)
            {
                var currentGame = _gameDetail.Single(x => x.ThemeId == _attractInfo[i].ThemeId);

                Assert.IsTrue(_target.IsTopAttractFeaturePlaying);
                Assert.IsTrue(_target.IsTopperAttractFeaturePlaying);

                Assert.AreEqual(currentGame.LocaleGraphics[_target.ActiveLocaleCode].BottomAttractVideo, _target.BottomAttractVideoPath);
                _target.OnTopGameAttractCompleteHandler(null, null);

                expectedCurrentAttractIndex = (++expectedCurrentAttractIndex) % attractCount;

                Assert.IsTrue(_waiter.WaitOne(waitTimeout));
                Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);
                _waiter.Reset();
            }
        }

        [TestMethod]
        public void WhenAttractListNotEmptyEnsureIfDisabledBottomVideosAreNotPlayed()
        {
            SetupMockLobbyConfig(false, false);

            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            var expectedCurrentAttractIndex = 0;
            var attractCount = _attractInfo.Count(x => x.IsSelected);
            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);

            Assert.AreEqual(false, _target.IsBottomAttractVisible);
            Assert.AreEqual(false, _target.IsBottomAttractFeaturePlaying);
            SetupPropertyListener(nameof(_target.TopperAttractVideoPath));

            for (var i = 0; i < attractCount; i++)
            {
                Assert.IsTrue(string.IsNullOrEmpty(_target.BottomAttractVideoPath));
                _target.OnTopGameAttractCompleteHandler(null, null);

                expectedCurrentAttractIndex = (++expectedCurrentAttractIndex) % attractCount;

                Assert.IsTrue(_waiter.WaitOne(waitTimeout));
                Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);
                _waiter.Reset();
            }
        }

        [TestMethod]
        public void WhenAlternateLanguageAvailableEnsureTopAndTopperAttractVideosArePlayedAccordingly()
        {
            SetupMockLobbyConfig(true);

            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            const int expectedCurrentAttractIndex = 0;
            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);

            SetupPropertyListener(nameof(_target.TopperAttractVideoPath));
            _target.OnTopGameAttractCompleteHandler(this, null);

            Assert.IsTrue(_waiter.WaitOne(waitTimeout));
            Assert.AreEqual(TopperDefaultVideo, _target.TopperAttractVideoPath);
            Assert.AreEqual(TopDefaultVideo, _target.TopAttractVideoPath);
        }

        [TestMethod]
        public void WhenAlternateLanguageAvailableEnsureIfEnabledBottomAttractVideosArePlayedAccordingly()
        {
            SetupMockLobbyConfig(true);

            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            // Start with first attract
            var currentGame = _gameDetail.Single(x => x.ThemeId == _attractInfo.First().ThemeId);
            Assert.AreEqual(currentGame.LocaleGraphics[_target.ActiveLocaleCode].BottomAttractVideo, _target.BottomAttractVideoPath);

            SetupPropertyListener(nameof(_target.BottomAttractVideoPath));
            _target.OnTopGameAttractCompleteHandler(null, null);
            Assert.IsTrue(_waiter.WaitOne(waitTimeout));

            // Ensure alternate attract videos are selected
            Assert.AreEqual(TopDefaultVideo, _target.BottomAttractVideoPath);
        }

        [TestMethod]
        public void WhenAlternateLanguageAvailableEnsureIfDisabledBottomAttractVideosAreNotPlayed()
        {
            SetupMockLobbyConfig(true, false);

            SetupCabinetService();

            CreateTarget();

            _target.LoadedCommand.Execute(null);

            var expectedCurrentAttractIndex = 0;
            var attractCount = _attractInfo.Count(x => x.IsSelected);
            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);
            Assert.IsTrue(string.IsNullOrEmpty(_target.BottomAttractVideoPath));

            SetupPropertyListener(nameof(_target.TopperAttractVideoPath));
            _target.OnTopGameAttractCompleteHandler(null, null);
            Assert.IsTrue(_waiter.WaitOne(waitTimeout));

            expectedCurrentAttractIndex = (++expectedCurrentAttractIndex) % attractCount;

            Assert.AreEqual(expectedCurrentAttractIndex, _target.CurrentAttractIndex);

            // Ensure bottom attract is still not played
            Assert.IsTrue(string.IsNullOrEmpty(_target.BottomAttractVideoPath));
        }

        private void SetupMockLobbyConfig(bool alternateLanguage = false, bool bottomAttractEnabled = true)
        {
            _lobbyConfiguration = new LobbyConfiguration
            {
                LocaleCodes = MockGameInfo.MockLocalGraphics,
                AlternateAttractModeLanguage = alternateLanguage,
                AttractVideoWithBonusFilename = "",
                AttractVideoNoBonusFilename = "",
                DefaultTopAttractVideoFilename = TopDefaultVideo,
                DefaultTopperAttractVideoFilename = TopperDefaultVideo,
                BottomAttractVideoEnabled = bottomAttractEnabled,
            };
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.LobbyConfig, null)).Returns(_lobbyConfiguration);
        }

        private void SetupCabinetService(bool topVisible = true, bool topperVisible = true)
        {
            var expectedDevices = new List<IDisplayDevice>();

            if (topperVisible)
            {
                expectedDevices.Add(new DisplayDevice { Role = DisplayRole.Topper });
            }

            if (topVisible)
            {
                expectedDevices.Add(new DisplayDevice { Role = DisplayRole.Top });
            }

            _cabinetService.Setup(c => c.ExpectedDisplayDevices).Returns(expectedDevices);
        }

        private void CreateTarget()
        {
            _target = new AttractPreviewViewModel();
            _target.PropertyChanged += OnPropertyChanged;
        }

        private void SetupPropertyListener(string property)
        {
            _propertyListeners[property] = () => _waiter.Set();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (_propertyListeners.TryGetValue(args.PropertyName, out var action))
            {
                action();
            }
        }
    }
}
