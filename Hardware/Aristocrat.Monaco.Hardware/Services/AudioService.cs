namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Audio;
    using Contracts;
    using Contracts.Audio;
    using Contracts.SharedDevice;
    using FMOD;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    /// <summary>
    ///     An audio service provider.
    /// </summary>
    [CLSCompliant(false)]
    public class AudioService : IAudio, IService, IMMNotificationClient, IDisposable
    {
        private const int UpdateLoopPollingInterval = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid AudioDisconnectedLock = HardwareConstants.AudioDisconnectedLockKey;
        private static readonly Guid AudioReconnectedLock = HardwareConstants.AudioReconnectedLockKey;

        private readonly CHANNEL_CALLBACK _callback;
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _properties;
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<SoundName, (string, Sound)> _sounds = new ConcurrentDictionary<SoundName, (string, Sound)>();
        private readonly ConcurrentQueue<Action> _callbackQueue = new ConcurrentQueue<Action>();

        private readonly Dictionary<VolumeScalar, float> _volumeScalars = new Dictionary<VolumeScalar, float>
        {
            { VolumeScalar.Scale20, 0.100f },
            { VolumeScalar.Scale40, 0.177f },
            { VolumeScalar.Scale60, 0.316f },
            { VolumeScalar.Scale80, 0.562f },
            { VolumeScalar.Scale100, 1.000f }
        };

        private readonly Dictionary<VolumeLevel, float> _volumePresets = new Dictionary<VolumeLevel, float>
        {
            { VolumeLevel.ExtraLow, 1.5f },
            { VolumeLevel.Low, 3.0f },
            { VolumeLevel.MediumLow, 6.0f },
            { VolumeLevel.Medium, 12.0f },
            { VolumeLevel.MediumHigh, 24.0f },
            { VolumeLevel.High, 48.0f },
            { VolumeLevel.ExtraHigh, 96.0f }
        };

        private Channel _channel;
        private System _system;
        private DeviceState? _lastState;
        private string _currentSoundFile;
        private Timer _pollingTimer;
        private bool _disposed;

        public AudioService()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
            _pollingTimer = new Timer(UpdateLoopPollingInterval);
            _pollingTimer.Elapsed += PollingTimerOnElapsed;
            _pollingTimer.AutoReset = true;
        }

        public AudioService(IPropertiesManager properties, IEventBus bus, ISystemDisableManager disableManager)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bus.Subscribe<PlatformBootedEvent>(this, Load);

            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _callback = ChannelCallback;
            SetSystemMuted(false);
        }

        public event EventHandler PlayEnded;

        public bool IsAvailable => AudioManager.IsSpeakerDeviceAvailable();

        private void LoadSound(SoundName soundName, string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                if (file is null)
                {
                    Logger.Error("Unable to load null audio file");
                }

                Logger.Debug("Audio file can't load; name not specified");
                return;
            }

            lock (_lock)
            {
                if (_sounds.ContainsKey(soundName))
                {
                    Logger.Debug($"Audio file is already loaded: {file}");
                    return;
                }

                if (_system == null)
                {
                    Logger.Error($"Failed to load the audio file: {file} - Sound System has not been created");
                    return;
                }

                Sound sound = null;
                var result = _system.createStream(file, MODE.ACCURATETIME, ref sound);
                if (result != RESULT.OK)
                {
                    Logger.Error($"Failed to load the audio file: {file} with error code ({result}");
                    return;
                }

                _sounds.TryAdd(soundName, (file, sound));
            }

            Logger.Debug($"Loaded audio file: {file}");
        }

        private void Load(PlatformBootedEvent evt)
        {
            var config = ConfigurationUtilities.GetConfiguration(
                HardwareConstants.SoundConfigurationExtensionPath,
                () => new SoundConfiguration());

            if (config.AudioFiles is not null)
            {
                foreach (var audio in config.AudioFiles.AudioFile)
                {
                    LoadSound(audio.Name, config.AudioFiles.Path + audio.File);
                }
            }
        }

        /// <inheritdoc />
        public void Play(SoundName soundName, float? volume, SpeakerMix speakers = SpeakerMix.All, Action callback = null)
        {
            if (!_sounds.ContainsKey(soundName))
            {
                Logger.Error($"Audio file can't play; sound file not loaded or not existed: {soundName}");
                return;
            }

            if (!volume.HasValue)
            {
                volume = GetDefaultVolume();
            }

            InternalPlay(soundName, MODE.LOOP_OFF, 0, volume.Value / 100.0f, speakers, callback);
        }

        /// <inheritdoc />
        public void Play(
            SoundName soundName,
            int loopCount,
            float? volume,
            SpeakerMix speakers = SpeakerMix.All,
            Action callback = null)
        {
            if (!_sounds.ContainsKey(soundName))
            {
                Logger.Error($"Audio file can't play; sound file not loaded or existed: {soundName}");
                return;
            }

            if (!volume.HasValue)
            {
                volume = GetDefaultVolume();
            }

            InternalPlay(soundName, MODE.LOOP_NORMAL, loopCount, volume.Value / 100.0f, speakers, callback);
        }

        /// <inheritdoc />
        public void Stop()
        {
            lock (_lock)
            {
                if (!IsPlaying())
                {
                    _callbackQueue.TryDequeue(out _);
                }

                Logger.Debug("Audio stopped");
                _channel?.stop();
            }
        }

        /// <inheritdoc />
        public void Stop(SoundName soundName)
        {
            if (!_sounds.ContainsKey(soundName))
            {
                Logger.Error($"Audio file can't stop; sound file not loaded or existed: {soundName}");
                return;
            }

            var soundFile = _sounds[soundName].Item1;
            if (string.IsNullOrEmpty(soundFile))
            {
                Logger.Debug("Audio file can't stop; name not specified");
                return;
            }

            if (!string.Equals(_currentSoundFile, soundFile))
            {
                return;
            }

            lock (_lock)
            {
                Logger.Debug("Audio stopped");
                _channel?.stop();
            }
        }

        /// <inheritdoc />
        public bool IsPlaying()
        {
            lock (_lock)
            {
                var playing = false;

                _channel?.isPlaying(ref playing);

                return playing;
            }
        }

        /// <inheritdoc />
        public bool IsPlaying(SoundName soundName)
        {
            if(!_sounds.ContainsKey(soundName)) { return false; }

            return string.Equals(_currentSoundFile, _sounds[soundName].Item1) && IsPlaying();
        }

        /// <inheritdoc />
        public void SetSystemMuted(bool mute)
        {
            AudioManager.SetMasterVolumeMute(mute);
        }

        /// <inheritdoc />
        public bool GetSystemMuted()
        {
            return AudioManager.GetMasterVolumeMute();
        }

        /// <inheritdoc />
        public float GetDefaultVolume()
        {
            var preset = (VolumeLevel)_properties.GetValue(PropertyKey.DefaultVolumeLevel, (byte)VolumeLevel.Low);
            return GetVolume(preset);
        }

        /// <inheritdoc />
        public float GetVolume(VolumeLevel preset)
        {
            return _volumePresets.TryGetValue(preset, out var volume) ? volume : 1.0f;
        }

        /// <inheritdoc />
        public float GetVolumeScalar(VolumeScalar preset)
        {
            return _volumeScalars.TryGetValue(preset, out var volume) ? volume : 1.0f;
        }

        public IVolume GetVolumeControl(int processId)
        {
            return processId <= 0 ? null : new Volume(processId);
        }

        /// <inheritdoc />
        public TimeSpan GetLength(SoundName soundName)
        {
            if (!_sounds.ContainsKey(soundName))
            {
                Logger.Error($"Audio file can't determine length; Sound file not loaded or existed: {soundName}");
                return TimeSpan.Zero;
            }

            var soundFile = _sounds[soundName].Item1;
            if (string.IsNullOrEmpty(soundFile))
            {
                Logger.Debug("Audio file can't determine length; name not specified");
                return TimeSpan.Zero;
            }
            
            var sound = _sounds[soundName].Item2;
            uint lengthMs = 0;
            if (sound.getLength(ref lengthMs, TIMEUNIT.MS) == RESULT.OK)
            {
                return TimeSpan.FromMilliseconds(lengthMs);
            }

            Logger.Error($"Failed to get length in milliseconds the audio file: {soundFile}");
            return TimeSpan.Zero;
        }

        /// <inheritdoc />
        public void SetSpeakerMix(SpeakerMix speakers)
        {
            lock (_lock)
            {
                if (!IsPlaying())
                {
                    return;
                }

                _channel?.SetSpeakerMix(speakers);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAudio) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Initializing Audio Service");

            AudioManager.RegisterEndpointNotificationCallback(this);

            if (AudioManager.IsSpeakerDeviceAvailable())
            {
                _bus.Publish(new EnabledEvent(EnabledReasons.Device));

                CreateSoundSystem();
            }
            else
            {
                Disable();
            }
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Logger.Info($"OnDeviceStateChanged {deviceId} - {newState}");

            switch (newState)
            {
                case DeviceState.Active:
                    Logger.Info("Audio device connected");

                    if (_disableManager.CurrentDisableKeys.Contains(AudioDisconnectedLock))
                    {
                        // Prevent multiple enabled event being logged
                        if (_lastState != DeviceState.Active)
                        {
                            _bus.Publish(new EnabledEvent(EnabledReasons.Reset));
                        }

                        if (_disableManager.CurrentDisableKeys.Contains(AudioReconnectedLock))
                        {
                            return;
                        }

                        // remove the disconnect lock-up and replace with reconnect
                        // We're not going to enable since we need to restart, but we're going to update the message
                        _disableManager.Disable(
                            AudioReconnectedLock,
                            SystemDisablePriority.Immediate,
                            () => Properties.Resources.AudioConnected);
                        _disableManager.Enable(AudioDisconnectedLock);
                    }

                    _lastState = DeviceState.Active;
                    break;
                case DeviceState.Disabled:
                case DeviceState.NotPresent:
                case DeviceState.Unplugged:
                case DeviceState.All:
                    Disable();
                    _lastState = null; // just assign anything other than active
                    break;
            }
        }

        public void OnDeviceAdded(string deviceId)
        {
            Logger.Info($"OnDeviceAdded {deviceId}");
        }

        public void OnDeviceRemoved(string deviceId)
        {
            Logger.Info($"OnDeviceRemoved {deviceId}");
        }

        public void OnDefaultDeviceChanged(EDataFlow dataFlow, ERole deviceRole, string defaultDeviceId)
        {
            Logger.Info($"OnDefaultDeviceChanged {dataFlow} {deviceRole} {defaultDeviceId}");
        }

        public void OnPropertyValueChanged(string deviceId, AudioProperty propertyKey)
        {
            Logger.Info($"OnDefaultDeviceChanged {deviceId} {propertyKey}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                ReleaseSoundSystem();
                _pollingTimer?.Stop();
                if (_pollingTimer != null)
                {
                    _pollingTimer.Elapsed -= PollingTimerOnElapsed;
                    _pollingTimer.Dispose();
                }

                _pollingTimer = null;
            }

            AudioManager.UnregisterEndpointNotificationCallback(this);
            _system = null;
            _disposed = true;
        }

        private void InternalPlay(
            SoundName soundName,
            MODE mode,
            int loopCount,
            float volume,
            SpeakerMix speakers,
            Action callback)
        {
            Task.Run(
                () =>
                {
                    lock (_lock)
                    {
                        var file = _sounds[soundName].Item1;
                        if (string.IsNullOrEmpty(file))
                        {
                            Logger.Debug("Audio file can't play; name not specified");
                            return;
                        }

                        Stop();

                        var sound = _sounds[soundName].Item2;
                        _currentSoundFile = file;

                        if (_system != null && _system.playSound(CHANNELINDEX.FREE, sound, true, ref _channel) ==
                            RESULT.OK)
                        {
                            _pollingTimer?.Start();
                            _callbackQueue.Enqueue(callback);
                            _channel.setCallback(_callback);
                            _channel.setMode(mode);
                            _channel.setLoopCount(loopCount);
                            _channel.setVolume(volume);
                            _channel.setPaused(false);
                            _channel.SetSpeakerMix(speakers);

                            Logger.Debug($"Playback of audio file {file} has started.");
                        }
                        else
                        {
                            Logger.Error($"Failed to playback audio file: {file}");
                        }
                    }
                });
        }

        private RESULT ChannelCallback(
            IntPtr channelRaw,
            CHANNEL_CALLBACKTYPE type,
            IntPtr commandData1,
            IntPtr commandData2)
        {
            if (type == CHANNEL_CALLBACKTYPE.END)
            {
                RaisePlayEnded();
            }

            return RESULT.OK;
        }

        private void RaisePlayEnded()
        {
            _pollingTimer?.Stop();
            PlayEnded?.Invoke(this, new EventArgs());
            if (_callbackQueue.TryDequeue(out var callback))
            {
                callback?.Invoke();
            }
        }

        private void CreateSoundSystem()
        {
            lock (_lock)
            {
                if (_system != null)
                {
                    return;
                }

                Factory.System_Create(ref _system);

                _system.setDSPBufferSize(1024, 10);

                var caps = CAPS.NONE;
                var frequency = (Min: 0, Max: 0);
                var speakerMode = SPEAKERMODE.STEREO;

                _system.getDriverCaps(0, ref caps, ref frequency.Min, ref frequency.Max, ref speakerMode);

                Logger.Debug($"Setting speaker mode: {speakerMode}");

                _system.setSpeakerMode(speakerMode);

                _system.init(32, INITFLAGS.NORMAL, (IntPtr)0);

                AudioManager.SetApplicationVolume(Process.GetCurrentProcess().Id, 100.0f);
            }
        }

        private void ReleaseSoundSystem()
        {
            lock (_lock)
            {
                if (_system == null)
                {
                    return;
                }

                Stop();
                _channel = null;

                foreach (var sound in _sounds.Values)
                {
                    sound.Item2.release();
                }

                _sounds.Clear();

                _system.release();
                _system = null;
            }
        }

        private void Disable()
        {
#if !(RETAIL)
            if (_properties.GetValue(@"audioDeviceOptional", "false") == "true")
            {
                return;
            }
#endif
            // VLT-9524 : prevent audio disconnect from firing more than once when cable unplugged
            if (_disableManager.CurrentDisableKeys.Contains(AudioDisconnectedLock))
            {
                return;
            }

            Logger.Error("Audio device disconnected - releasing Sound System");

            _bus.Publish(new DisabledEvent(DisabledReasons.Device));

            // remove the reconnect lock-up (if exists) and replace with disconnect
            _disableManager.Enable(AudioReconnectedLock);
            _disableManager.Disable(
                AudioDisconnectedLock,
                SystemDisablePriority.Immediate,
                () => Properties.Resources.AudioDisconnect);
        }

        private void PollingTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                _system?.update();
            }
        }

        private class Volume : IVolume
        {
            private readonly int _processId;

            public Volume(int processId)
            {
                _processId = processId;
            }

            /// <inheritdoc />
            public float GetVolume()
            {
                return AudioManager.GetApplicationVolume(_processId) ?? 0;
            }

            public void SetVolume(float volume)
            {
                AudioManager.SetApplicationVolume(_processId, volume);
            }

            public void SetMuted(bool muted)
            {
                AudioManager.SetApplicationMute(_processId, muted);
            }
        }
    }

    [CLSCompliant(false)]
    public static class ChannelExtensions
    {
        public static void SetSpeakerMix(this Channel @this, SpeakerMix speakers)
        {
            @this?.setSpeakerMix(
                (speakers & SpeakerMix.FrontLeft) == SpeakerMix.FrontLeft ? 1.0f : 0.0f,
                (speakers & SpeakerMix.FrontRight) == SpeakerMix.FrontRight ? 1.0f : 0.0f,
                (speakers & SpeakerMix.Center) == SpeakerMix.Center ? 1.0f : 0.0f,
                (speakers & SpeakerMix.LowFrequency) == SpeakerMix.LowFrequency ? 1.0f : 0.0f,
                (speakers & SpeakerMix.RearLeft) == SpeakerMix.RearLeft ? 1.0f : 0.0f,
                (speakers & SpeakerMix.RearRight) == SpeakerMix.RearRight ? 1.0f : 0.0f,
                (speakers & SpeakerMix.SideLeft) == SpeakerMix.SideLeft ? 1.0f : 0.0f,
                (speakers & SpeakerMix.SideRight) == SpeakerMix.SideRight ? 1.0f : 0.0f
            );
        }
    }
}