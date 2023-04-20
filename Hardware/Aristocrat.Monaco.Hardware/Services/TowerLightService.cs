namespace Aristocrat.Monaco.Hardware.Services
{
    using Contracts.IO;
    using Contracts.TowerLight;
    using Kernel;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using Timer = System.Timers.Timer;
    using System.Threading;

    /// <summary>Implementation of TowerLight service.</summary>
    public class TowerLightService : ITowerLight, IService, IDisposable
    {
        private readonly IIO _io;
        private readonly IEventBus _eventBus;
        private const int FlashIntervalUnitTime = 125; // in milliseconds
        private const int FlashFastInterval = 1;
        private const int FlashMediumInterval = 2;
        private const int FlashSlowInterval = 4;

        private readonly ConcurrentDictionary<LightTier, LightTierInfo> _lightTierInfo = new();
        private Timer _flashTimer;
        private uint _flashTickCounter;
        private bool _disposed;

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ITowerLight) };

        public TowerLightService()
            : this(
                ServiceManager.GetInstance().GetService<IIO>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TowerLightService" /> class.</summary>
        public TowerLightService(IIO io, IEventBus eventBus)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            foreach (LightTier lightTier in Enum.GetValues(typeof(LightTier)))
            {
                var lightInfo = new LightTierInfo();
                _lightTierInfo.AddOrUpdate(lightTier, lightInfo, (_, _) => lightInfo);
            }

            _flashTimer = new Timer(FlashIntervalUnitTime) { AutoReset = true };
            _flashTimer.Elapsed += OnFlashTick;
            _flashTickCounter = 0;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            foreach (LightTier lightTier in Enum.GetValues(typeof(LightTier)))
            {
                SetTowerLightDevice(lightTier, false);
            }

            Reset();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases allocated resources.</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; False to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_flashTimer != null)
                {
                    _flashTimer.Stop();
                    _flashTimer.Elapsed -= OnFlashTick;
                    _flashTimer.Dispose();
                }
            }

            _flashTimer = null;
            _disposed = true;
        }

        public bool IsLit => _lightTierInfo.Values.Any(a => a.FlashState != FlashState.Off);

        /// <inheritdoc />
        public void Reset()
        {
            foreach (LightTier lightTier in Enum.GetValues(typeof(LightTier)))
            {
                SetFlashState(lightTier, FlashState.Off, Timeout.InfiniteTimeSpan);
            }
        }

        /// <inheritdoc />
        public void SetFlashState(LightTier lightTier, FlashState flashState, TimeSpan duration, bool test = false)
        {
            if (_lightTierInfo[lightTier].FlashState == flashState && _lightTierInfo[lightTier].Duration == duration)
            {
                return;
            }

            if (lightTier == LightTier.Strobe && flashState != FlashState.Off)
            {
                flashState = FlashState.On;
            }

            _lightTierInfo[lightTier].FlashState = flashState;
            _lightTierInfo[lightTier].Duration = duration;
            SetTowerLightDevice(lightTier, IsLightOn(lightTier));

            // Check if the Flash timer is currently needed and enable/disable the timer accordingly
            if (IsFlashTimerNeeded())
            {
                if (duration != Timeout.InfiniteTimeSpan)
                {
                    _flashTickCounter = 0;
                }

                if (!_flashTimer.Enabled)
                {
                    _flashTickCounter = 0;
                    _flashTimer.Start();
                }
            }
            else
            {
                if (_flashTimer.Enabled)
                {
                    _flashTimer.Stop();
                    _flashTickCounter = 0;
                }
            }
        }

        public FlashState GetFlashState(LightTier lightTier)
        {
            return _lightTierInfo.TryGetValue(lightTier, out var value) ? value.FlashState : FlashState.Off;
        }

        private void SetTowerLightDevice(LightTier lightTier, bool lightOn)
        {
            _lightTierInfo[lightTier].DeviceOn = lightOn;
            var lightIndex = (int)lightTier;
            if (!_io.SetTowerLight(lightIndex, lightOn))
            {
                return;
            }

            if (lightOn)
            {
                _eventBus.Publish(new TowerLightOnEvent(lightTier, _lightTierInfo[lightTier].FlashState));
            }
            else
            {
                _eventBus.Publish(new TowerLightOffEvent(lightTier, _lightTierInfo[lightTier].FlashState));
            }
        }

        private bool IsFlashTimerNeeded()
        {
            return Enum.GetValues(typeof(LightTier)).Cast<LightTier>().Any(
                lightTier => _lightTierInfo[lightTier].Duration != Timeout.InfiniteTimeSpan ||
                             _lightTierInfo[lightTier].FlashState != FlashState.Off &&
                             _lightTierInfo[lightTier].FlashState != FlashState.On);
        }

        private bool IsLightOn(LightTier lightTier)
        {
            uint interval;
            switch (_lightTierInfo[lightTier].FlashState)
            {
                case FlashState.Off:
                    return false;
                case FlashState.On:
                    if (_lightTierInfo[lightTier].Duration == Timeout.InfiniteTimeSpan)
                    {
                        return true;
                    }

                    return _flashTickCounter <
                           _lightTierInfo[lightTier].Duration.TotalMilliseconds / FlashIntervalUnitTime;
                case FlashState.SlowFlashReversed:
                case FlashState.SlowFlash:
                    interval = FlashSlowInterval;
                    break;

                case FlashState.MediumFlash:
                case FlashState.MediumFlashReversed:
                    interval = FlashMediumInterval;
                    break;
                case FlashState.FastFlash:
                    interval = FlashFastInterval;
                    break;
                default:
                    interval = 1;
                    break;
            }

            var counter = _flashTickCounter / interval;
            var lightOn = counter % 2 == 0;
            if (_lightTierInfo[lightTier].FlashState == FlashState.MediumFlashReversed ||
                _lightTierInfo[lightTier].FlashState == FlashState.SlowFlashReversed)
            {
                lightOn = !lightOn;
            }

            return lightOn;
        }

        private void OnFlashTick(object sender, ElapsedEventArgs args)
        {
            _flashTickCounter++;

            foreach (LightTier lightTier in Enum.GetValues(typeof(LightTier)))
            {
                if (_lightTierInfo[lightTier].Duration == Timeout.InfiniteTimeSpan &&
                    (_lightTierInfo[lightTier].FlashState == FlashState.Off ||
                     _lightTierInfo[lightTier].FlashState == FlashState.On))
                {
                    continue;
                }

                var lightOn = IsLightOn(lightTier);

                if (_lightTierInfo[lightTier].DeviceOn != lightOn)
                {
                    SetTowerLightDevice(lightTier, lightOn);
                }
            }
        }
    }
}
