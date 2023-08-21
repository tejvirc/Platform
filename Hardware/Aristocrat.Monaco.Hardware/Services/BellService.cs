namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Bell;
    using Contracts.IO;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using DisabledEvent = Contracts.Bell.DisabledEvent;
    using EnabledEvent = Contracts.Bell.EnabledEvent;

    public class BellService : IBell
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IReadOnlyDictionary<EnabledReasons, DisabledReasons> DisabledReasonsMap =
            new Dictionary<EnabledReasons, DisabledReasons>
            {
                { EnabledReasons.Backend, DisabledReasons.Backend },
                { EnabledReasons.System, DisabledReasons.System },
                { EnabledReasons.Service, DisabledReasons.Service },
                { EnabledReasons.Device, DisabledReasons.Device },
                { EnabledReasons.Configuration, DisabledReasons.Configuration },
                { EnabledReasons.GamePlay, DisabledReasons.GamePlay },
                {
                    EnabledReasons.Operator,
                    DisabledReasons.Operator | DisabledReasons.Error | DisabledReasons.FirmwareUpdate
                },
                { EnabledReasons.Reset, DisabledReasons.Error | DisabledReasons.FirmwareUpdate }
            };

        private readonly IIO _io;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _bus;
        private bool _bellEnabled;

        public BellService()
            : this(
                ServiceManager.GetInstance().GetService<IIO>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public BellService(IIO io, IPropertiesManager properties, IEventBus bus)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            Disable(DisabledReasons.Service);
        }

        public bool IsRinging { get; set; }

        public string Name { get; } = nameof(BellService);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IBell) };

        public bool Enabled => _bellEnabled && ReasonDisabled == 0 && Initialized;

        public bool Initialized { get; set; }

        public string LastError { get; } = string.Empty;

        public DisabledReasons ReasonDisabled { get; set; }

        public string ServiceProtocol
        {
            get => string.Empty;
            set { }
        }

        public void Initialize()
        {
            _bellEnabled = _properties.GetValue(HardwareConstants.BellEnabledKey, false);
            StopBell();

            Initialized = true;
            if (_bellEnabled)
            {
                Enable(EnabledReasons.Service);
            }
        }

        public async Task<bool> RingBell(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (!Enabled || !RingBell())
            {
                return false;
            }

            await Task.Delay(duration);
            return StopBell();
        }

        public bool RingBell()
        {
            Logger.Debug("BELL: start ringing");

            if (!Enabled)
            {
                return false;
            }

            _io.SetBellState(true);

            IsRinging = true;

            _bus.Publish(new RingStartedEvent());

            return true;
        }

        public bool StopBell()
        {
            Logger.Debug("BELL: stop ringing");

            _io.SetBellState(false);

            IsRinging = false;

            _bus.Publish(new RingStoppedEvent());

            return true;
        }

        public void Disable(DisabledReasons reason)
        {
            if (_bellEnabled)
            {
                return;
            }

            ReasonDisabled |= reason;
            var bellWasRinging = IsRinging;
            Logger.Debug($"Disabled by {reason}.  Bell was ringing={bellWasRinging}");
            if (bellWasRinging)
            {
                StopBell();
            }

            _bus.Publish(new DisabledEvent(ReasonDisabled));
        }

        public bool Enable(EnabledReasons reason)
        {
            if (!_bellEnabled)
            {
                return false;
            }

            if (!Initialized)
            {
                return false;
            }

            foreach (var flag in reason.GetFlags())
            {
                if (!DisabledReasonsMap.TryGetValue(flag, out var clearConditions))
                {
                    continue;
                }

                Logger.Debug($"Clearing disable by {clearConditions}");
                ReasonDisabled &= ~clearConditions;
            }

            if (Enabled)
            {
                _bus.Publish(new EnabledEvent(reason));
            }

            return Enabled;
        }
    }
}