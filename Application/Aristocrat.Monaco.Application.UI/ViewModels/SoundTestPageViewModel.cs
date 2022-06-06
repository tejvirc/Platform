namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Contracts;
    using Contracts.HardwareDiagnostics;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class SoundTestPageViewModel : OperatorMenuSaveViewModelBase
    {
        private static readonly Guid AudioDisconnectedLock = HardwareConstants.AudioDisconnectedLockKey;
        private static readonly Guid AudioReconnectedLock = HardwareConstants.AudioReconnectedLockKey;

        private const string SoundConfigurationExtensionPath = "/OperatorMenu/Sound/Configuration";

        private bool _isAudioDisabled;
        private bool _isPlaying;
        private bool _centerSpeaker;
        private bool _frontLeftSpeaker;
        private bool _frontRightSpeaker;
        private bool _lowFrequencySpeaker;
        private bool _rearLeftSpeaker;
        private bool _rearRightSpeaker;
        private bool _sideLeftSpeaker;
        private bool _sideRightSpeaker;
        private bool _previousSystemMuted;
        private readonly IAudio _audio;
        private readonly ISystemDisableManager _disableManager;
        private ITimer _playingTimer;
        private SoundFileViewModel _sound;
        private byte _soundLevel;
        private readonly SpeakerMix _enabledSpeakersMask;
        private bool IsAudioServiceAvailable => _audio != null;
        public SoundTestPageViewModel()
        {
            _audio = ServiceManager.GetInstance().TryGetService<IAudio>();

            _disableManager = ServiceManager.GetInstance().TryGetService<ISystemDisableManager>();

            _isAudioDisabled = !IsAudioServiceAvailable || !_audio.IsAvailable;

            _enabledSpeakersMask = PropertiesManager.GetValue(ApplicationConstants.EnabledSpeakersMask, SpeakerMix.All);

            if (IsAudioServiceAvailable)
            {
                _audio.PlayEnded += OnPlayEnded;
            }
            else
            {
                Logger.Error("Audio service is not available");
            }

            _playingTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromMilliseconds(100) };

            _playingTimer.Tick += OnPlayingTimerTick;

            bool enablePlay = IsAudioServiceAvailable && !IsPlaying && !IsAudioDisabled;

            StopCommand = new ActionCommand<object>(_ => StopSound(),
                _ => IsAudioServiceAvailable && !IsAudioDisabled && IsPlaying);

            PlayCommand = new ActionCommand<object>(_ => PlaySound(),
                _ => IsAudioServiceAvailable && !IsPlaying && !IsAudioDisabled);

            PlayCommandOnFrontLeftSpeaker = new ActionCommand<object>(PlaySoundOnFrontLeftSpeaker, _ => enablePlay);

            PlayCommandOnCenterSpeaker = new ActionCommand<object>(PlaySoundOnCenterSpeaker, _ => enablePlay);

            PlayCommandOnFrontRightSpeaker = new ActionCommand<object>(PlaySoundOnFrontRightSpeaker, _ => enablePlay);

            PlayCommandOnSideLeftSpeaker = new ActionCommand<object>(PlaySoundOnSideLeftSpeaker, _ => enablePlay);

            PlayCommandOnSideRightSpeaker = new ActionCommand<object>(PlaySoundOnSideRightSpeaker, _ => enablePlay);

            PlayCommandOnLowFrequencySpeaker = new ActionCommand<object>(PlaySoundOnLowFrequencySpeaker, _ => enablePlay);

            PlayCommandOnRearLeftSpeaker = new ActionCommand<object>(PlaySoundOnRearLeftSpeaker, _ => enablePlay);

            PlayCommandOnRearRightSpeaker = new ActionCommand<object>(PlaySoundOnRearRightSpeaker, _ => enablePlay);
        }

        private void LoadVolumeSettings()
        {
            // Load default volume level
            _soundLevel = PropertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", _soundLevel);
            RaisePropertyChanged(nameof(SoundLevel));
        }

        public IActionCommand PlayCommand { get; }

        public IActionCommand StopCommand { get; }

        public IActionCommand PlayCommandOnFrontLeftSpeaker { get; }

        public IActionCommand PlayCommandOnCenterSpeaker { get; }

        public IActionCommand PlayCommandOnFrontRightSpeaker { get; }

        public IActionCommand PlayCommandOnSideLeftSpeaker { get; }

        public IActionCommand PlayCommandOnSideRightSpeaker { get; }

        public IActionCommand PlayCommandOnLowFrequencySpeaker { get; }

        public IActionCommand PlayCommandOnRearLeftSpeaker { get; }

        public IActionCommand PlayCommandOnRearRightSpeaker { get; }

        public bool IsPlaying
        {
            get => _isPlaying;

            set
            {
                if (_isPlaying == value)
                {
                    return;
                }

                _isPlaying = value;
                RaisePropertyChanged(nameof(IsPlaying));
                PlayCommand?.RaiseCanExecuteChanged();
                StopCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsAudioDisabled
        {
            get => _isAudioDisabled;

            set
            {
                if (_isAudioDisabled == value)
                {
                    return;
                }

                _isAudioDisabled = value;
                RaisePropertyChanged(nameof(IsAudioDisabled));
                PlayCommand?.RaiseCanExecuteChanged();
                StopCommand?.RaiseCanExecuteChanged();
            }
        }

        public byte SoundLevel
        {
            get => _soundLevel;

            set
            {
                if (_soundLevel == value)
                {
                    return;
                }

                _soundLevel = value;
                RaisePropertyChanged(nameof(SoundLevel));
            }
        }

        public ObservableCollection<SoundFileViewModel> SoundFiles { get; } = new ObservableCollection<SoundFileViewModel>();

        public SoundFileViewModel Sound
        {
            get => _sound;

            set
            {
                if (_sound == value)
                {
                    return;
                }

                _sound = value;
                RaisePropertyChanged(nameof(Sound));
            }
        }

        public bool FrontLeftSpeaker
        {
            get => _frontLeftSpeaker;

            set
            {
                if (_frontLeftSpeaker != value)
                {
                    _frontLeftSpeaker = value;
                    RaisePropertyChanged(nameof(FrontLeftSpeaker));
                }
            }
        }

        public bool FrontRightSpeaker
        {
            get => _frontRightSpeaker;

            set
            {
                if (_frontRightSpeaker != value)
                {
                    _frontRightSpeaker = value;
                    RaisePropertyChanged(nameof(FrontRightSpeaker));
                }
            }
        }

        public bool CenterSpeaker
        {
            get => _centerSpeaker;

            set
            {
                if (_centerSpeaker != value)
                {
                    _centerSpeaker = value;
                    RaisePropertyChanged(nameof(CenterSpeaker));
                }
            }
        }

        public bool RearLeftSpeaker
        {
            get => _rearLeftSpeaker;

            set
            {
                if (_rearLeftSpeaker != value)
                {
                    _rearLeftSpeaker = value;
                    RaisePropertyChanged(nameof(RearLeftSpeaker));
                }
            }
        }

        public bool RearRightSpeaker
        {
            get => _rearRightSpeaker;

            set
            {
                if (_rearRightSpeaker != value)
                {
                    _rearRightSpeaker = value;
                    RaisePropertyChanged(nameof(RearRightSpeaker));
                }
            }
        }

        public bool SideLeftSpeaker
        {
            get => _sideLeftSpeaker;

            set
            {
                if (_sideLeftSpeaker != value)
                {
                    _sideLeftSpeaker = value;
                    RaisePropertyChanged(nameof(SideLeftSpeaker));
                }
            }
        }

        public bool SideRightSpeaker
        {
            get => _sideRightSpeaker;

            set
            {
                if (_sideRightSpeaker != value)
                {
                    _sideRightSpeaker = value;
                    RaisePropertyChanged(nameof(SideRightSpeaker));
                }
            }
        }

        public bool LowFrequencySpeaker
        {
            get => _lowFrequencySpeaker;

            set
            {
                if (_lowFrequencySpeaker != value)
                {
                    _lowFrequencySpeaker = value;
                    RaisePropertyChanged(nameof(LowFrequencySpeaker));
                }
            }
        }

        protected override bool IsModalDialog => true;

        protected override void OnLoaded()
        {
            EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Sound));
            EventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            EventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);

            if (_disableManager.CurrentDisableKeys.Contains(AudioDisconnectedLock) || _disableManager.CurrentDisableKeys.Contains(AudioReconnectedLock))
            {
                IsPlaying = false;
                IsAudioDisabled = true;
                TestModeEnabled = false;
            }
            else
            {
                LoadVolumeSettings();
                GetSoundFiles();
                TurnOnAllSpeakers();
                _previousSystemMuted = _audio.GetSystemMuted();
                _audio.SetSystemMuted(false);
            }

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            StopSound();
            _audio.SetSystemMuted(_previousSystemMuted);
            EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Sound));

            base.OnUnloaded();
        }

        private void StopSound()
        {
            if (Sound != null)  // VLT-12533 : Fix null reference exception in sound page when switching tabs when cable unplugged 
            {
                _audio?.Stop(Sound.Path);
            }
        }

        private void PlaySound()
        {
            if (!IsAudioServiceAvailable || IsPlaying)
            {
                return;
            }

            var path = Sound.Path;
            var volume = _audio.GetVolume(SoundLevel);

            IsPlaying = true;

            _audio.Play(path, volume);
            _playingTimer.Start();
        }

        private void PlaySoundOnSpeaker(SpeakerMix speaker, string path)
        {
            if (!IsAudioServiceAvailable || IsPlaying || path.IsEmpty())
            {
                return;
            }

            var volume = _audio.GetVolume(SoundLevel);

            IsPlaying = true;

            _audio.Play(path, volume, speaker);
            _playingTimer.Start();

        }

        private void PlaySoundOnFrontLeftSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.FrontLeft, (string)(obj));
        }

        private void PlaySoundOnCenterSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.Center, (string)(obj));
        }

        private void PlaySoundOnFrontRightSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.FrontRight, (string)(obj));
        }

        private void PlaySoundOnSideLeftSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.SideLeft, (string)(obj));
        }

        private void PlaySoundOnSideRightSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.SideRight, (string)(obj));
        }

        private void PlaySoundOnLowFrequencySpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.LowFrequency, (string)(obj));
        }

        private void PlaySoundOnRearLeftSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.RearLeft, (string)(obj));
        }

        private void PlaySoundOnRearRightSpeaker(object obj)
        {
            PlaySoundOnSpeaker(SpeakerMix.RearRight, (string)(obj));
        }

        private void OnPlayEnded(object sender, EventArgs eventArgs)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                _playingTimer.Stop();
                IsPlaying = false;
                StopCommand?.RaiseCanExecuteChanged();
            });
        }

        private void GetSoundFiles()
        {
            var filePath = SoundConfigurationExtensionPath;
            try
            {
                var files = new List<SoundFileViewModel>();

                var nodes =
                    MonoAddinsHelper.GetSelectedNodes<FilePathExtensionNode>(
                        filePath);

                foreach (var node in nodes)
                {
                    var path = node.FilePath;
                    var name = !string.IsNullOrWhiteSpace(node.Name)
                        ? node.Name
                        : Path.GetFileNameWithoutExtension(path);

                    Logger.DebugFormat(
                        CultureInfo.CurrentCulture,
                        $"Found {SoundConfigurationExtensionPath} node: {node.FilePath}");

                    files.Add(new SoundFileViewModel(name, path));
                }
                if (!SoundFiles.Any())
                {
                    SoundFiles.AddRange(files.ToArray());
                    Sound = SoundFiles.FirstOrDefault();
                }
            }
            catch (ConfigurationErrorsException)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    $"Extension path {filePath} not found");
                throw;
            }
        }

        private void OnPlayingTimerTick(object sender, EventArgs args)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                if (!IsAudioServiceAvailable)
                {
                    return;
                }
                if (!_audio.IsPlaying())
                {
                    _audio.Stop();
                }
            });
        }

        private bool GetFlag(SpeakerMix speakerMix)
        {
            return (_enabledSpeakersMask & speakerMix) > 0;
        }

        private void TurnOnAllSpeakers()
        {
            FrontLeftSpeaker = GetFlag(SpeakerMix.FrontLeft);
            FrontRightSpeaker = GetFlag(SpeakerMix.FrontRight);
            SideLeftSpeaker = GetFlag(SpeakerMix.SideLeft);
            SideRightSpeaker = GetFlag(SpeakerMix.SideRight);
            LowFrequencySpeaker = GetFlag(SpeakerMix.LowFrequency);
            CenterSpeaker = GetFlag(SpeakerMix.Center);
            RearLeftSpeaker = GetFlag(SpeakerMix.RearLeft);
            RearRightSpeaker = GetFlag(SpeakerMix.RearRight);
        }

        protected override void DisposeInternal()
        {
            if (IsAudioServiceAvailable)
            {
                _audio.PlayEnded -= OnPlayEnded;
            }

            if (_playingTimer != null)
            {
                _playingTimer.Tick -= OnPlayingTimerTick;
                _playingTimer.Stop();
                _playingTimer = null;
            }

            base.DisposeInternal();
        }

        private void OnEnabledEvent(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                IsAudioDisabled = true;
                TestModeEnabled = false;
                DialogResult = false;
            });
        }

        private void OnDisabledEvent(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                IsPlaying = false;
                IsAudioDisabled = true;
                TestModeEnabled = false;
                DialogResult = false;
            });
        }

        protected override bool CloseOnRestrictedAccess => true;
    }
}