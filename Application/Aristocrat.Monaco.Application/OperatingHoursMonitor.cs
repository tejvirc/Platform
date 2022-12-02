namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Localization;
    using Contracts.Operations;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Responsible for enabling/disabling the system based on the defined operating hours.
    /// </summary>
    public class OperatingHoursMonitor : IService, IDisposable, IOperatingHoursMonitor
    {
        private const int MinimumDelay = 1000; // It's in milliseconds
        private const int Tolerance = 100;

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITime _time;

        private bool _currentState = true;
        private bool _disposed;
        private CancellationTokenSource _monitorCancellationToken;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatingHoursMonitor" /> class.
        /// </summary>
        public OperatingHoursMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ITime>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatingHoursMonitor" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="systemDisableManager">An <see cref="ISystemDisableManager" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="time">An <see cref="ITime" /> instance.</param>
        public OperatingHoursMonitor(
            IPropertiesManager properties,
            ISystemDisableManager systemDisableManager,
            IEventBus eventBus,
            ITime time)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool OutsideOperatingHours => !_currentState;

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOperatingHoursMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Operating Hours Monitor Initialized");

            _eventBus.Subscribe<PropertyChangedEvent>(this, Handle, e => e.PropertyName == ApplicationConstants.OperatingHours);
            _eventBus.Subscribe<TimeUpdatedEvent>(this, Handle);

            MonitorOperatingHoursAsync().FireAndForget();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _monitorCancellationToken?.Cancel(false);
                _monitorCancellationToken?.Dispose();
            }

            _monitorCancellationToken = null;

            _disposed = true;
        }

        private static OperatingHours GetCurrentState(DateTime now, ICollection<OperatingHours> operatingHours)
        {
            var span = now - now.Date;

            var result = operatingHours.LastOrDefault(
                h => h.Day == now.DayOfWeek && span.TotalMilliseconds > h.Time);
            if (result != null)
            {
                return result;
            }

            result = operatingHours.LastOrDefault(h => h.Day < now.DayOfWeek);

            return result ?? operatingHours.Last();
        }

        private static OperatingHours GetNextState(DateTime now, ICollection<OperatingHours> operatingHours)
        {
            var span = now - now.Date;

            var result =
                operatingHours.FirstOrDefault(h => h.Day == now.DayOfWeek && span.TotalMilliseconds <= h.Time);
            if (result != null)
            {
                return result;
            }

            result = operatingHours.FirstOrDefault(h => h.Day > now.DayOfWeek);

            return result ?? operatingHours.First();
        }

        private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;

            return start.AddDays(daysToAdd);
        }

        private void Handle(PropertyChangedEvent evt)
        {
            MonitorOperatingHoursAsync().FireAndForget();
        }

        private void Handle(TimeUpdatedEvent evt)
        {
            MonitorOperatingHoursAsync().FireAndForget();
        }

        private async Task MonitorOperatingHoursAsync()
        {
            _monitorCancellationToken?.Cancel(false);
            using var source = _monitorCancellationToken = new CancellationTokenSource();
            try
            {
                await MonitorOperatingHoursAsync(source.Token);
            }
            finally
            {
                _monitorCancellationToken = null;
            }
        }

        private async Task MonitorOperatingHoursAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var operatingHours =
                    _properties.GetValues<OperatingHours>(ApplicationConstants.OperatingHours)
                        .OrderBy(h => h.Day)
                        .ThenBy(h => h.Time)
                        .ToList();

                if (!operatingHours.Any())
                {
                    SetState(true);
                    return;
                }

                var delay = CalculateNextDelay(operatingHours);
                await Task.Delay(delay, token);
            }
        }

        private TimeSpan CalculateNextDelay(ICollection<OperatingHours> operatingHours)
        {
            var now = _time.GetLocationTime();
            SetState(GetCurrentState(now, operatingHours));
            var nextState = GetNextState(now, operatingHours);
            var nextCheck = GetNextWeekday(now.Date.AddMilliseconds(nextState.Time), nextState.Day);
            Logger.Info($"Next operating hours change occurs @ {nextCheck} - enabled: {nextState.Enabled}");
            var delay = (nextCheck - _time.GetLocationTime()).TotalMilliseconds + Tolerance;
            return delay switch
            {
                < 0 => TimeSpan.FromMilliseconds(MinimumDelay), 
                > int.MaxValue => TimeSpan.FromMilliseconds(int.MaxValue),
                _ => TimeSpan.FromMilliseconds(delay)
            };
        }

        private void SetState(OperatingHours operatingHours)
        {
            SetState(operatingHours.Enabled);
        }

        private void SetState(bool enabled)
        {
            if (_currentState == enabled)
            {
                return;
            }

            if (enabled)
            {
                _systemDisableManager.Enable(ApplicationConstants.OperatingHoursDisableGuid);
                _eventBus.Publish(new OperatingHoursEnabledEvent());
            }
            else
            {
                _systemDisableManager.Disable(
                    ApplicationConstants.OperatingHoursDisableGuid,
                    SystemDisablePriority.Normal,
                    () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.OutsideOperatingHours),
                    false);
                _eventBus.Publish(new OperatingHoursExpiredEvent());
            }

            _currentState = enabled;

            Logger.Info($"Operating hours change - enabled: {enabled}");
        }
    }
}
