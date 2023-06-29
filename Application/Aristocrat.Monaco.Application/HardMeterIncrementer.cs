namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.HardMeter;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    /// <summary>
    ///     Definition of the HardMeterIncrementer class.
    /// </summary>
    /// <remarks>
    ///     The class is used to increment value on a hard meter. The configuration file it loads is used to determine which
    ///     hard meter is going to be advanced when the event handler is invoked.
    /// </remarks>
    public sealed class HardMeterIncrementer : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static double _currencyMultiplier;

        private bool _disposed;
        private bool _hardMeterEnabled;
        private List<MeterConfiguration> _mapOfLogicalIdAndSoftMeters;

        private string _currentMeterMapping;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_hardMeterEnabled)
            {
                _mapOfLogicalIdAndSoftMeters.ForEach(x => x.Unsubscribe());
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new List<Type> { typeof(HardMeterIncrementer) };

        private IPropertiesManager _propertiesManager;

        /// <inheritdoc />
        public void Initialize()
        {
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            _hardMeterEnabled = _propertiesManager
                .GetValue(HardwareConstants.HardMetersEnabledKey, true);

            if (!_hardMeterEnabled)
            {
                return;
            }

            
            _currentMeterMapping = _propertiesManager.GetValue(
                ApplicationConstants.HardMeterMapSelectionValue,
                "Default");

            _currencyMultiplier =
                (double)_propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1.0) / 100;
            LoadConfig();
        }

        private void LoadConfig()
        {
            var config = ConfigurationUtilities.GetConfiguration(
                "/HardMeterMapping/Configuration",
                () => new HardMeterMappingConfiguration());


            if (config.HardMeterMapping.Length <= 0)
            {
                return;
            }

            var mapping = config.HardMeterMapping.FirstOrDefault((x) => x.Name == _currentMeterMapping);

            if (mapping == null)
            {
                mapping = config.HardMeterMapping.FirstOrDefault((x )=> x.Default) ?? config.HardMeterMapping.FirstOrDefault();

                var name = mapping?.Name??"";

                _propertiesManager.SetProperty(ApplicationConstants.HardMeterMapSelectionValue, name);
            } 

            if (mapping != null)
            {
               _mapOfLogicalIdAndSoftMeters = mapping.HardMeter.Select(m => new MeterConfiguration { Configuration = m }).ToList();
            }

            _mapOfLogicalIdAndSoftMeters.ForEach(x => x.Subscribe());

            var bus = ServiceManager.GetInstance().GetService<IEventBus>();
            bus.Subscribe<MeterProviderAddedEvent>(this, HandleEvent);
            bus.Subscribe<MediaAlteredEvent>(this, HandleEvent);
        }

        private void HandleEvent(IEvent theEvent)
        {
            if (!_hardMeterEnabled)
            {
                return;
            }

            _mapOfLogicalIdAndSoftMeters.ForEach(x => x.Subscribe());
        }

        private class MeterConfiguration
        {
            private readonly List<IMeter> _subscribedMeters = new List<IMeter>();

            public HardMeterMappingConfigurationHardMeterMappingHardMeter Configuration { get; set; }
            

            public void Subscribe()
            {
                var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

                var metersToSubscribe = Configuration.SoftMeter
                    .Select(
                        s =>
                        {
                            if (_subscribedMeters.Any(m => m.Name == s.Name) || !meterManager.IsMeterProvided(s.Name))
                            {
                                return null;
                            }

                            var meter = meterManager.GetMeter(s.Name);
                            meter.MeterChangedEvent += OnMeterValueChanged;
                            return meter;
                        }).Where(x => x != null);
                _subscribedMeters.AddRange(metersToSubscribe);
            }

            public void Unsubscribe()
            {
                _subscribedMeters.ForEach(x => x.MeterChangedEvent -= OnMeterValueChanged);
                _subscribedMeters.Clear();
            }

            private void OnMeterValueChanged(object sender, MeterChangedEventArgs e)
            {
                if (!(sender is IMeter meter) || e.Amount == 0)
                {
                    return;
                }

                var hardMeter = ServiceManager.GetInstance().GetService<IHardMeter>();

                if (!(hardMeter.LogicalHardMeters.TryGetValue(Configuration.LogicalId, out var _value) && _value.IsAvailable))
                {
                    Logger.Debug($"hard meter for id: {Configuration.LogicalId} is not available");
                    return;
                }

                var value = e.Amount;
                if (meter.Classification.Name.Equals("Currency"))
                {
                    value = (long)(e.Amount / _currencyMultiplier);
                }
                
                hardMeter.AdvanceHardMeter(Configuration.LogicalId, value);
                Logger.Debug($"Incrementing hard meter {meter.Name} by {value}");
            }
        }
    }
}