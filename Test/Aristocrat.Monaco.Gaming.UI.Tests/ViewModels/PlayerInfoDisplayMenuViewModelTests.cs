namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.PlayerInfoDisplay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.UI.Common;
    using Moq;
    using UI.ViewModels;

    [TestClass]
    public class PlayerInfoDisplayMenuViewModelTests
    {
        private PlayerInfoDisplayMenuViewModel _underTest;
        private Mock<ITimer> _timeoutTimer;
        private Mock<IPlayerInfoDisplayFeatureProvider> _playerInfoDisplayFeatureProvider;
        private const int TimeoutMilliseconds = 100;

        [TestInitialize]
        public void Setup()
        {
            _playerInfoDisplayFeatureProvider = new Mock<IPlayerInfoDisplayFeatureProvider>();
            _playerInfoDisplayFeatureProvider.Setup(x => x.TimeoutMilliseconds).Returns(TimeoutMilliseconds);
            _playerInfoDisplayFeatureProvider.Setup(x => x.IsPlayerInfoDisplaySupported).Returns(true);
            _timeoutTimer = new Mock<ITimer>();
            _underTest = new PlayerInfoDisplayMenuViewModel(_playerInfoDisplayFeatureProvider.Object, _timeoutTimer.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PlayerInfoDisplayMenuViewModel(null));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _underTest.Dispose();
        }

        [TestMethod]
        public void GivenTimeoutTimerWhenRaiseTickThenExitTriggered()
        {
            var exitTriggered = false;
            _underTest.ButtonClicked += (sender, args) =>
            {
                exitTriggered = args.CommandType == CommandType.Exit;
            };
            _timeoutTimer.Raise(e => e.Tick += null, _timeoutTimer, EventArgs.Empty);

            Assert.IsTrue(exitTriggered);
        }

        [TestMethod]
        public void GivenTimeoutTimerWhenShowThenTimerStarted()
        {
            _timeoutTimer.Setup(x => x.IsEnabled).Returns(false).Verifiable();
            _underTest.Show();

            _timeoutTimer.Verify(x => x.Start());
            _timeoutTimer.Verify(x => x.Stop(), Times.Never);
        }

        [TestMethod]
        public void GivenTimeoutTimerWhenHideThenTimerStopped()
        {
            _timeoutTimer.Setup(x => x.IsEnabled).Returns(true).Verifiable();
            _underTest.Hide();

            _timeoutTimer.Verify(x => x.Stop());
            _timeoutTimer.Verify(x => x.Start(), Times.Never);
        }

        [TestMethod]
        public void GivenModelWhenSetupResourcesThenAllResourcesMapped()
        {
            var model = new Mock<IPlayInfoDisplayResourcesModel>();
            model.Setup(
                x => x.GetScreenBackground(
                    It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.LandscapeTag, GameAssetTags.ScreenTag, GameAssetTags.PlayerInformationDisplayMenuTag }))))
                .Returns("BackgroundImageLandscapePath")
                .Verifiable();
            model.Setup(
                    x => x.GetScreenBackground(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.PortraitTag, GameAssetTags.ScreenTag, GameAssetTags.PlayerInformationDisplayMenuTag }))))
                .Returns("BackgroundImagePortraitPath")
                .Verifiable();
            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.ExitTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag }))))
                .Returns("ExitButtonPath")
                .Verifiable();
            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.ExitTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag }))))
                .Returns("ExitButtonPressedPath")
                .Verifiable();

            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.GameInfoTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag }))))
                .Returns("GameInfoButtonPath")
                .Verifiable();
            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.GameInfoTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag }))))
                .Returns("GameInfoButtonPressedPath")
                .Verifiable();

            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.GameRulesTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag }))))
                .Returns("GameRulesButtonPath")
                .Verifiable();
            model.Setup(
                    x => x.GetButton(
                        It.Is<ISet<string>>(p => p.IsSubsetOf(new HashSet<string>() { GameAssetTags.GameRulesTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag }))))
                .Returns("GameRulesButtonPressedPath")
                .Verifiable();

            _underTest.SetupResources(model.Object);

            Assert.AreEqual("BackgroundImageLandscapePath", _underTest.BackgroundImageLandscapePath);
            Assert.AreEqual("BackgroundImagePortraitPath", _underTest.BackgroundImagePortraitPath);

            Assert.AreEqual("ExitButtonPath", _underTest.ExitButtonPath);
            Assert.AreEqual("ExitButtonPressedPath", _underTest.ExitButtonPressedPath);

            Assert.AreEqual("GameInfoButtonPath", _underTest.GameInfoButtonPath);
            Assert.AreEqual("GameInfoButtonPressedPath", _underTest.GameInfoButtonPressedPath);

            Assert.AreEqual("GameRulesButtonPath", _underTest.GameRulesButtonPath);
            Assert.AreEqual("GameRulesButtonPressedPath", _underTest.GameRulesButtonPressedPath);

            model.Verify();
        }
    }
}