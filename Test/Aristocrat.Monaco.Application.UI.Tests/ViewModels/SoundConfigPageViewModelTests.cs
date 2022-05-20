namespace Aristocrat.Monaco.Application.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.IO;
    using Kernel;
    using Kernel.Contracts;
    using Mono.Addins;
    using Test.Common;
    using UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SoundConfigPageViewModelTests
    {
        private Mock<IAudio> _audioMock;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<ISystemDisableManager> _disableManagerMock;

        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManagerMock = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            _disableManagerMock = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);

            _audioMock = MoqServiceManager.CreateAndAddService<IAudio>(MockBehavior.Default);
            _audioMock.Setup(audio => audio.IsAvailable).Returns(true);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            var monitor = MoqServiceManager.CreateAndAddService<IOperatorMenuGamePlayMonitor>(MockBehavior.Strict);
            monitor.Setup(m => m.InGameRound).Returns(false);
            monitor.Setup(m => m.IsRecoveryNeeded).Returns(false);

            MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Loose);

            var selections = new Dictionary<int, int>
            {
                { "Jurisdiction".GetHashCode(), "Quebec VLT".GetHashCode() }
            };
            _propertiesManagerMock.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(selections);

            var showMode = false;
            _propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.ShowMode, showMode))
                .Returns(showMode);

            var alertVolume = (byte)100;
            _propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.AlertVolumeKey, alertVolume))
                .Returns(alertVolume);

            var alertVolumeMinimum = (byte)50;
            _propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.SoundConfigurationAlertVolumeMinimum, alertVolumeMinimum))
                .Returns(alertVolumeMinimum);

            var isAlertConfigurable = false;
            _propertiesManagerMock.Setup(m => m.GetProperty(ApplicationConstants.SoundConfigurationAlertVolumeConfigurable, isAlertConfigurable))
                .Returns(isAlertConfigurable);

            var _buttonService = MoqServiceManager.CreateAndAddService<IButtonService>(MockBehavior.Strict);
            _buttonService.Setup(m => m.IsTestModeActive).Returns(It.IsAny<bool>());

            var _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Strict);
            _iio.Setup(m => m.SetButtonLamp(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());
            _iio.Setup(m => m.SetButtonLampByMask(It.IsAny<uint>(), It.IsAny<bool>())).Returns(It.IsAny<uint>());

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();

            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }
        }
        [Ignore] // Ignored, needs to be rewritten. Test no longer helpful/valid.
        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var viewModel = new SoundTestPageViewModel();

            Assert.IsNotNull(viewModel.LoadedCommand);
            Assert.IsNotNull(viewModel.UnloadedCommand);
            Assert.IsNotNull(viewModel.PlayCommand);

            Assert.IsNotNull(viewModel.SoundFiles);
        }
        [Ignore] // Ignored, needs to be rewritten. Test no longer helpful/valid.
        [TestMethod]
        public void WhenPageLoadedExpectSuccess()
        {
            var page = new Page();

            _propertiesManagerMock.Setup(m => m.GetProperty(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel))
                .Returns(ApplicationConstants.DefaultVolumeLevel);

            var viewModel = new SoundTestPageViewModel();
            viewModel.LoadedCommand.Execute(page);

            Assert.IsFalse(string.IsNullOrEmpty(viewModel.Sound.Path));
        }

        [Ignore] // Ignored, needs to be rewritten. Test no longer helpful/valid.
        [TestMethod]
        public void WhenConfigurationSelectExpectSuccess()
        {
            var soundLevel = VolumeLevel.Low;
            _propertiesManagerMock.Setup(m => m.SetProperty(PropertyKey.DefaultVolumeLevel, (byte)soundLevel, true)).Verifiable();

            var page = new Page();
            var viewModel = new SoundTestPageViewModel();
            viewModel.LoadedCommand.Execute(page);
            var volume = 20.0f;

            _audioMock.Setup(m => m.GetVolume(soundLevel)).Returns(volume);

            _audioMock.Setup(m => m.Play(It.IsAny<string>(), volume, It.IsAny<SpeakerMix>(), It.IsAny<Action>())).Verifiable();

            viewModel.SoundLevel = soundLevel;

            viewModel.PlayCommand.Execute(null);

            viewModel.UnloadedCommand.Execute(page);

            Thread.Sleep(800);

            _audioMock.VerifyAll();
            _propertiesManagerMock.VerifyAll();
        }
    }
}