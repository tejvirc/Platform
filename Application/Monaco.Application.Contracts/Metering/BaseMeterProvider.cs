namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A base implementation to <see cref="IMeterProvider"/>, which implements the majority of <see cref="IMeterProvider"/> work that is common
    ///     to all implementations. Derived types need only create their desired meter objects and pass them to AddMeter().
    /// </summary>
    public abstract class BaseMeterProvider : IMeterProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<string, IMeter> _meters = new ConcurrentDictionary<string, IMeter>();
        private readonly IPropertiesManager _propertiesManager;

        /// <inheritdoc />
        protected IPropertiesManager PropertiesManager => _propertiesManager;
        /// <inheritdoc />
        protected bool RolloverTest { get; set; } = false;

        private ClearPeriodMeter _clearPeriodDelegates;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseMeterProvider" /> class.
        /// </summary>
        /// <param name="name">The name of this provider</param>
        protected BaseMeterProvider(string name)
            :this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
            Name = name;
        }

        private BaseMeterProvider(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public ICollection<string> MeterNames => _meters.Keys;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IMeter GetMeter(string meterName)
        {
            _meters.TryGetValue(meterName, out var meter);
            if (meter == null)
            {
                Logger.Error($"Meter not found: {meterName}");
            }

            return meter;
        }

        /// <inheritdoc />
        public virtual DateTime LastPeriodClear
        {
            get => DateTime.MinValue;
            set => throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ClearPeriodMeters()
        {
            _clearPeriodDelegates?.Invoke();
        }

        /// <inheritdoc />
        public void RegisterMeterClearDelegate(ClearPeriodMeter del)
        {
            _clearPeriodDelegates += del ?? throw new ArgumentNullException(nameof(del));
        }

        /// <summary>
        ///     Adds a meter to the provider
        /// </summary>
        /// <param name="meter">The meter to add to the provider</param>
        protected void AddMeter(IMeter meter)
        {
            if (meter == null)
            {
                throw new ArgumentNullException(nameof(meter));
            }

            if (!_meters.TryAdd(meter.Name, meter))
            {
                Logger.Error($"Failed to add {meter.Name} meter");
            }
        }

        /// <summary>
        ///     Adds a meter to the provider
        /// </summary>
        /// <param name="meter">The meter to add to the provider</param>
        protected IMeter AddOrReplaceMeter(IMeter meter)
        {
            if (meter == null)
            {
                throw new ArgumentNullException(nameof(meter));
            }

            return _meters.AddOrUpdate(meter.Name, meter, (_, _) => meter);
        }

        /// <summary>
        ///     Invalidates the provider
        /// </summary>
        protected void Invalidate()
        {
            ServiceManager.GetInstance().TryGetService<IMeterManager>()?.InvalidateProvider(this);
        }

        /// <summary>
        ///     Checks to see if the meter exists
        /// </summary>
        /// <param name="meterName">The meter name</param>
        /// <returns>true, if the meter exists</returns>
        protected bool Contains(string meterName)
        {
            return _meters.ContainsKey(meterName);
        }

        /// <summary>
        ///     Initializes meters to maximum value when the "maxmeters" command line argument is present
        /// </summary>
        /// <param name="meter"></param>
        protected void SetupMeterRolloverTest(IMeter meter)
        {
            if (!RolloverTest)
            {
                return;
            }

            if (meter == null || meter.Lifetime != 0)
            {
                return;
            }

            var preRollover = meter.Classification.UpperBounds;
            if (meter.Classification.GetType() == typeof(CurrencyMeterClassification))
            {
                var currencyMultiplier = _propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);
                var oneCent = currencyMultiplier / (int)CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit;
                preRollover -= (long)oneCent;
            }
            else
            {
                preRollover -= 1;
            }

            Logger.Debug($"Incrementing meter: {meter.Name} to {preRollover}");
            meter.Increment(preRollover);
        }
    }
}