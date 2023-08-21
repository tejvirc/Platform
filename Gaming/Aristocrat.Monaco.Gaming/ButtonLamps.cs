namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Contracts;
    using Hardware.Contracts.IO;
    using Kernel;
    using log4net;

    public class ButtonLamps : IButtonLamps, IService, IDisposable
    {
        private const int BlinkInterval = 500;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IIO _io;
        private readonly IButtonService _buttonService;
        private readonly ConcurrentDictionary<int, Lamp> _lamps = new();
        private readonly List<int> _invalidLamps = new();
        private readonly Timer _blinkTimer = new(BlinkInterval);
        private readonly object _lockObject = new object();

        private bool _disposed;

        public ButtonLamps(IIO io, IButtonService buttonService)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _buttonService = buttonService ?? throw new ArgumentNullException(nameof(buttonService));

            _blinkTimer.Elapsed += BlinkTimerOnElapsed;
            _blinkTimer.Start();
        }

        public string Name => typeof(IButtonLamps).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IButtonLamps) };

        public LampState GetLampState(int buttonId)
        {
            return _lamps.TryGetValue(buttonId, out var lamp) ? lamp.State : LampState.Off;
        }

        public void SetLampState(int buttonId, LampState state)
        {
            var lightOn = state == LampState.On;

            var lamp = _lamps.AddOrUpdate(
                buttonId,
                _ => new Lamp(state, lightOn),
                (_, v) =>
                {
                    v.State = state;
                    v.LightOn = lightOn;
                    return v;
                });

            if (!lamp.Disabled)
            {
                SetLampLight(buttonId, lightOn);
            }
        }

        public void SetLampState(IList<ButtonLampState> buttonLampsState)
        {
            lock (_lockObject)
            {
                foreach (var buttonState in buttonLampsState.Where(buttonState => buttonState != null))
                {
                    SetLampState(buttonState.ButtonId, buttonState.State);
                }
            }
        }

        public void DisableLamps()
        {
            foreach (var lampId in GetLampIds())
            {
                _lamps.AddOrUpdate(
                    lampId,
                    _ => new Lamp { Disabled = true },
                    (_, v) =>
                    {
                        v.Disabled = true;
                        return v;
                    });

                SetLampLight(lampId, false);
            }
        }

        public void EnableLamps()
        {
            foreach (var lampId in GetLampIds())
            {
                var lamp = _lamps.AddOrUpdate(
                    lampId,
                    _ => new Lamp(),
                    (_, v) =>
                    {
                        v.Disabled = false;
                        return v;
                    });

                if (lamp.LightOn)
                {
                    SetLampLight(lampId, lamp.LightOn);
                }
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _blinkTimer.Stop();
                _blinkTimer.Dispose();
            }

            _disposed = true;
        }

        private void SetLampLight(int buttonId, bool lampOn)
        {
            var lampBit = _buttonService.GetButtonLampBit(buttonId + (int)ButtonLogicalId.ButtonBase);
            if (lampBit >= 0)
            {
                _io.SetButtonLamp((uint)lampBit, lampOn);
            }
            else
            {
                lock (_invalidLamps)
                {
                    if (!_invalidLamps.Contains(buttonId))
                    {
                        Logger.Debug($"Unable to find button ID: {buttonId}");
                        _invalidLamps.Add(buttonId);
                    }
                }
            }
        }

        private void BlinkTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var lamp in _lamps.Where(x => !x.Value.Disabled && x.Value.State == LampState.Blink))
            {
                SetLampLight(lamp.Key, lamp.Value.ToggleLamp());
            }
        }

        private IReadOnlyList<int> GetLampIds()
        {
            return _lamps.Keys.Union(Enum.GetValues(typeof(LampName)).Cast<int>()).ToList();
        }

        private class Lamp
        {
            public Lamp(LampState state = LampState.Off, bool lightOn = false, bool disabled = false)
            {
                State = state;
                LightOn = lightOn;
                Disabled = disabled;
            }

            public LampState State { get; set; }

            public bool LightOn { get; set; }

            public bool Disabled { get; set; }

            public bool ToggleLamp()
            {
                return LightOn = !LightOn;
            }
        }
    }
}
