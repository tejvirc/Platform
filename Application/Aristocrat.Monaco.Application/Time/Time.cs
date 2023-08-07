namespace Aristocrat.Monaco.Application.Time
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Time service implements the ITime interface and provides the ability to convert a
    ///     UTC DateTime to a local DateTime and update the system time.  The Time service
    ///     requires kernel32.dll to make calls into Win32 APIs in order to change the system
    ///     time.
    /// </summary>
    public class Time : IPropertyProvider, IService, ITime
    {
        public const string TimeFormat24HourLong = "HH:mm:ss";
        public const string TimeFormat24HourShort = "HH:mm";
        public const string DateFormat = "yyyy-MM-dd";

        private const string BootTimePropertyKey = "System.BootTime";

        private const PersistenceLevel Level = PersistenceLevel.Critical;

        // The constant is used to determine whether to update the time. Two seconds are used, based on SAS protocol.
        private const double ThresholdForUpdate = 2.0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPersistentStorageManager _storage;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _bus;

        private readonly DateTime _minDateTime = new DateTime(2010, 01, 01);

        private TimeZoneInfo _timeZone;
        private TimeSpan _timeZoneOffset;

        public Time()
            :this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
            
        }

        public Time(IPersistentStorageManager storage, IPropertiesManager properties, IEventBus bus)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => typeof(ITime).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ITime) };

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>(ApplicationConstants.TimeZoneKey, _timeZone),
            new KeyValuePair<string, object>(ApplicationConstants.TimeZoneOffsetKey, _timeZoneOffset),
            new KeyValuePair<string, object>(ApplicationConstants.TimeZoneBias, TimeZoneBias)
        };

        public TimeZoneInfo TimeZoneInformation
        {
            get => _timeZone;
            private set
            {
                if (value != null && (_timeZone == null || _timeZone.Equals(value) == false))
                {
                    SetProperty(ApplicationConstants.TimeZoneKey, value);
                }
            }
        }

        /// <inheritdoc />
        public TimeSpan TimeZoneOffset
        {
            get => _timeZoneOffset;
            private set
            {
                if (_timeZoneOffset != value)
                {
                    SetProperty(ApplicationConstants.TimeZoneOffsetKey, value);
                }
            }
        }

        /// <inheritdoc />
        public TimeSpan TimeZoneBias => DateTime.UtcNow - DateTime.Now - TimeZoneOffset;

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case ApplicationConstants.TimeZoneKey:
                    return _timeZone;
                case ApplicationConstants.TimeZoneOffsetKey:
                    return _timeZoneOffset;
                case ApplicationConstants.TimeZoneBias:
                    return TimeZoneBias;
            }

            return null;
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            var block = _storage.GetBlock(GetType().ToString());

            var updateTime = false;
            switch (propertyName)
            {
                case ApplicationConstants.TimeZoneKey:
                    _timeZone = (TimeZoneInfo)propertyValue;

                    block["TimeZone"] = _timeZone.Id;

                    TimeZoneInteropWrapper.SetDynamicTimeZone(_timeZone);
                    updateTime = true;
                    Logger.Debug($"Time service Set Time Zone:\n Block:{_timeZone.Id}\n Local:{TimeZoneInfo.Local.Id}");
                    break;
                case ApplicationConstants.TimeZoneOffsetKey:
                    _timeZoneOffset = (TimeSpan)propertyValue;

                    block["TimeZoneOffset"] = _timeZoneOffset.Ticks;

                    updateTime = true;
                    Logger.Debug($"Time service Set Time Zone offset:\n Block:TimeZoneOffset\n Offset:{_timeZoneOffset.Ticks}");
                    break;
            }

            // only post these when Time Zone or Time Zone Offset changed
            if (updateTime)
            {
                _bus.Publish(new TimeZoneOffsetUpdatedEvent());
                _bus.Publish(new TimeZoneUpdatedEvent());
                _bus.Publish(new TimeUpdatedEvent());
            }
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _properties.SetProperty(BootTimePropertyKey, DateTime.UtcNow);

            var blockName = GetType().ToString();

            if (_storage.BlockExists(blockName))
            {
                var block = _storage.GetBlock(blockName);
                TimeZoneInformation = ConvertTimeZoneString((string)block["TimeZone"]);
                TimeZoneOffset = new TimeSpan((long)block["TimeZoneOffset"]);
            }
            else
            {
                _storage.CreateBlock(Level, blockName, 1);
                TimeZoneInformation = TimeZoneInfo.Local;
                TimeZoneOffset = TimeSpan.Zero;
            }

            _properties.AddPropertyProvider(this);

            Logger.Debug(
                $"Time service has initialized and boot time has been set as:\n{DateTime.UtcNow}\n Block:{TimeZoneInformation.Id}\n Local:{TimeZoneInfo.Local.Id}");

            SetDateTimeForCurrentCulture();
        }

        /// <inheritdoc />
        public DateTime GetLocationTime() => GetLocationTime(DateTime.UtcNow);

        /// <inheritdoc />
        public DateTime GetLocationTime(DateTime time) => GetLocationTime(time, _timeZone);

        /// <inheritdoc />
        public DateTime GetLocationTime(DateTime time, TimeZoneInfo timeZone)
        {
            if (timeZone != null)
            {
                time = TimeZoneInfo.ConvertTimeFromUtc(time.ToUniversalTime(), timeZone) + TimeZoneOffset;
            }

            return time;
        }

        /// <inheritdoc />
        public string GetFormattedLocationTime() => GetFormattedLocationTime(GetLocationTime());

        /// <inheritdoc />
        public string GetFormattedLocationTime(DateTime time, string format = ApplicationConstants.DefaultDateTimeFormat) => FormatDateTimeString(GetLocationTime(time), format);

        /// <inheritdoc />
        public bool Update(DateTimeOffset time)
        {
            if (time.UtcDateTime < _minDateTime)
            {
                Logger.Warn($"Time received ({time.UtcDateTime}) is prior to allowed time of {_minDateTime}");
            }
            else
            {
                try
                {
                    Logger.Debug($"Current time is: {DateTime.UtcNow}");

                    var update = time.UtcDateTime - DateTime.UtcNow;
                    if (!(Math.Abs(update.TotalSeconds) > ThresholdForUpdate))
                    {
                        return false;
                    }

                    Logger.Debug($"Setting system time to: {time}");
                    Logger.Debug($"The time span is: {update}");

                    SetTime(time.UtcDateTime);

                    _bus.Publish(new TimeUpdatedEvent(update));

                    return true;
                }
                catch (InvalidOperationException e)
                {
                    Logger.Error($"Invalid time update: {e}");
                }

            }

            return false;
        }

        /// <inheritdoc />
        public string FormatDateTimeString(DateTime dateTime, string format = ApplicationConstants.DefaultDateTimeFormat)
        {
            return dateTime.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <inheritdoc />
        public void SetDateTimeForCurrentCulture()
        {
            var culture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();

            culture.DateTimeFormat.FullDateTimePattern = ApplicationConstants.DefaultDateTimeFormat;
            culture.DateTimeFormat.ShortDatePattern = DateFormat;
            culture.DateTimeFormat.ShortTimePattern = TimeFormat24HourShort;
            culture.DateTimeFormat.LongTimePattern = TimeFormat24HourLong;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private static void SetTime(DateTime time)
        {
            var sysTime = default(NativeMethods.SystemTime);
            sysTime.wYear = (short)time.Year;
            sysTime.wMonth = (short)time.Month;
            sysTime.wDayOfWeek = (short)time.DayOfWeek;
            sysTime.wDay = (short)time.Day;
            sysTime.wHour = (short)time.Hour;
            sysTime.wMinute = (short)time.Minute;
            sysTime.wSecond = (short)time.Second;
            sysTime.wMilliseconds = (short)time.Millisecond;

            uint error;

            var result = NativeMethods.SetSystemTime(ref sysTime);
            if (result != 0 || (error = NativeMethods.GetLastError()) == 0)
            {
                return;
            }

            var errorMessage = $"Failed to set system time with a result of {error}";
            Logger.Error(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        private static TimeZoneInfo ConvertTimeZoneString(string timeZone)
        {
            TimeZoneInfo timeZoneInfo = null;
            try
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                Logger.Error($"TimeZone: {timeZone} not found in system time zones!");
            }

            Logger.Debug($"Converted time zone  string {timeZone} to {timeZoneInfo}");

            return timeZoneInfo;
        }
    }
}