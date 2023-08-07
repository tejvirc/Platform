namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Contracts.ConfigWizard;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class SoundTestPageViewModel : INotifyPropertyChanged
    {
        private static readonly Guid AudioDisconnectedLock = HardwareConstants.AudioDisconnectedLockKey;
        private static readonly Guid AudioReconnectedLock = HardwareConstants.AudioReconnectedLockKey;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!?.DeclaringType);

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
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private ITimer _playingTimer;
        private SoundFileViewModel _sound;
        private VolumeLevel _soundLevel;
        private readonly SpeakerMix _enabledSpeakersMask;
        private bool IsAudioServiceAvailable => _audio != null;
        private IInspectionService _reporter;

        public SoundTestPageViewModel()
            : this(
                ServiceManager.GetInstance().GetService<IAudio>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public SoundTestPageViewModel(IAudio audio, IEventBus eventBus, ISystemDisableManager disableManager, IPropertiesManager propertiesManager)
        {
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _isAudioDisabled = !IsAudioServiceAvailable || !_audio.IsAvailable;

            _enabledSpeakersMask = _propertiesManager.GetValue(ApplicationConstants.EnabledSpeakersMask, SpeakerMix.All);

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

        public void SetTestReporter(IInspectionService reporter)
        {
            _reporter = reporter;
        }

        private void LoadVolumeSettings()
        {
            // Load default volume level
            _soundLevel = (VolumeLevel)_propertiesManager.GetValue(PropertyKey.DefaultVolumeLevel, ApplicationConstants.DefaultVolumeLevel);
            Logger.DebugFormat("Initializing default volume setting with value: {0}", _soundLevel);
            OnPropertyChanged(nameof(SoundLevel));
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

        public bool TestMode
        {
            set
            {
                if (value)
                {
                    Initialize();
                }
                else
                {
                    UnInitialize();
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;

            set
            {
                if (_isPlaying == value)
                {
                    return;
                }

                SetProperty(ref _isPlaying, value, nameof(IsPlaying));
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

                SetProperty(ref _isAudioDisabled, value, nameof(IsAudioDisabled));
                PlayCommand?.RaiseCanExecuteChanged();
                StopCommand?.RaiseCanExecuteChanged();
            }
        }

        public VolumeLevel SoundLevel
        {
            get => _soundLevel;

            set
            {
                if (_soundLevel == value)
                {
                    return;
                }

                SetProperty(ref _soundLevel, value, nameof(SoundLevel));
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

                SetProperty(ref _sound, value, nameof(Sound));
            }
        }

        public bool FrontLeftSpeaker
        {
            get => _frontLeftSpeaker;

            set
            {
                if (_frontLeftSpeaker != value)
                {
                    SetProperty(ref _frontLeftSpeaker, value, nameof(FrontLeftSpeaker));
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
                    SetProperty(ref _frontRightSpeaker, value, nameof(FrontRightSpeaker));
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
                    SetProperty(ref _centerSpeaker, value, nameof(CenterSpeaker));
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
                    SetProperty(ref _rearLeftSpeaker, value, nameof(RearLeftSpeaker));
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
                    SetProperty(ref _rearRightSpeaker, value, nameof(RearRightSpeaker));
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
                    SetProperty(ref _sideLeftSpeaker, value, nameof(SideLeftSpeaker));
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
                    SetProperty(ref _sideRightSpeaker, value, nameof(SideRightSpeaker));
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
                    SetProperty(ref _lowFrequencySpeaker, value, nameof(LowFrequencySpeaker));
                }
            }
        }

        protected void Initialize()
        {
            _eventBus.Subscribe<EnabledEvent>(this, OnEnabledEvent);
            _eventBus.Subscribe<DisabledEvent>(this, OnDisabledEvent);

            _playingTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromMilliseconds(100) };
            _playingTimer.Tick += OnPlayingTimerTick;

            if (IsAudioServiceAvailable)
            {
                _audio.PlayEnded += OnPlayEnded;
            }
            else
            {
                Logger.Error("Audio service is not available");
            }

            if (_disableManager.CurrentDisableKeys.Contains(AudioDisconnectedLock) || _disableManager.CurrentDisableKeys.Contains(AudioReconnectedLock))
            {
                IsPlaying = false;
                IsAudioDisabled = true;
            }
            else
            {
                LoadVolumeSettings();
                GetSoundFiles();
                TurnOnAllSpeakers();
                _previousSystemMuted = _audio.GetSystemMuted();
                _audio.SetSystemMuted(false);
            }
        }

        protected void UnInitialize()
        {
            StopSound();
            _audio.SetSystemMuted(_previousSystemMuted);

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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (field == null && value == null)
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
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

            _reporter?.SetTestName($"Play sound {Sound.Name}");
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

            _reporter?.SetTestName($"Play on {speaker} speaker");
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

        private void OnEnabledEvent(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                IsAudioDisabled = true;
            });
        }

        private void OnDisabledEvent(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                IsPlaying = false;
                IsAudioDisabled = true;
            });
        }
    }
}