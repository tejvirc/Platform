namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.Metering;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts.HandCount;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the HandCountService class.
    /// </summary>
    public class HandCountService : IHandCountService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly IMeterManager _meters;
        private readonly IMeter _handCountMeter;

        public HandCountService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
            )
        {
        }

        public HandCountService(
            IEventBus eventBus,
            IMeterManager meters,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _handCountMeter = _meters.GetMeter(AccountingMeters.HandCount);
        }

        public string Name => typeof(HandCountService).FullName;

        public int HandCount => (int)_handCountMeter.GetValue(MeterTimeframe.Lifetime);

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountService) };

        public void Initialize()
        {
        }

        public void IncrementHandCount()
        {
            _handCountMeter.Increment(1);

            _eventBus.Publish(new HandCountChangedEvent(HandCount));

            Logger.Info($"IncrementHandCount:{HandCount}");
        }

        public void DecreaseHandCount(int n)
        {
            _handCountMeter.Increment(-n);

            //_runtime.UpdateParameter(_bank.Credits);

            _eventBus.Publish(new HandCountChangedEvent(HandCount));
            Logger.Info($"DecreaseHandCount by {n} to {HandCount}");
        }

        public void ResetHandCount()
        {
            _handCountMeter.Increment(-HandCount);
            _eventBus.Publish(new HandCountChangedEvent(HandCount));
            Logger.Info($"ResetHandCount:{HandCount}");
        }
    }
}