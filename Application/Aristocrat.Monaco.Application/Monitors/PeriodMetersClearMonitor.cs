namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the PeriodMetersClearMonitor class.
    /// </summary>
    public class PeriodMetersClearMonitor : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _properties;
        private readonly ITime _time;

        private double _clearHourOffset;
        private Timer _timer;

        private bool _disposed;

        public PeriodMetersClearMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<ITime>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public PeriodMetersClearMonitor(
            IEventBus eventBus,
            IMeterManager meterManager,
            ITime time,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => typeof(PeriodMetersClearMonitor).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(PeriodMetersClearMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (!_properties.GetValue(ApplicationConstants.AutoClearPeriodMetersText, false))
            {
                return;
            }

            Logger.Debug("Initializing the PeriodMetersClearMonitor...");

            _clearHourOffset = _properties.GetValue(ApplicationConstants.ClearClearPeriodOffsetHoursText, 0D);

            _timer = new Timer(OnClear, null, GetNextDueTime(), Timeout.InfiniteTimeSpan);

            _eventBus.Subscribe<TimeUpdatedEvent>(this, HandleEvent);
        }

        /// <summary>
        ///     Disposes of this IDisposable
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
                if (_timer != null)
                {
                    _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    _timer.Dispose();
                }
            }

            _timer = null;

            _disposed = true;
        }

        private void OnClear(object state)
        {
            Logger.Debug("Clearing period meters...");

            _meterManager.ClearAllPeriodMeters();

            _timer.Change(GetNextDueTime(), Timeout.InfiniteTimeSpan);
        }

        private void HandleEvent(TimeUpdatedEvent data)
        {
            _timer.Change(GetNextDueTime(), Timeout.InfiniteTimeSpan);
        }

        private TimeSpan GetNextDueTime()
        {
            var now = _time.GetLocationTime();
            var clearOffset = now.Date.AddHours(_clearHourOffset);
            var lastClear = _time.GetLocationTime(_meterManager.LastPeriodClear);

            if (now >= clearOffset && lastClear < clearOffset || lastClear < clearOffset.AddDays(-1))
            {
                return TimeSpan.Zero;
            }

            var next = clearOffset.AddDays(1) - now;

            Logger.Debug($"Clearing period meters at {now + next}");

            return next;
        }
    }
}