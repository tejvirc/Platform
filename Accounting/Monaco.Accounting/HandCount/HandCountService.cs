namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.Metering;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
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
        private bool resetTimerIsRunning;

        public event Action OnResetTimerStarted;
        public event Action OnResetTimerCancelled;

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
            IPropertiesManager propertyProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _properties = propertyProvider ?? throw new ArgumentNullException(nameof(propertyProvider));
            _handCountMeter = _meters.GetMeter(AccountingMeters.HandCount);
        }

        public string Name => typeof(HandCountService).FullName;

        public int HandCount => (int)_handCountMeter.GetValue(MeterTimeframe.Lifetime);

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountService) };

        public void Initialize()
        {
            _eventBus.Subscribe<InitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
        }

        private void HandleEvent(InitializationCompletedEvent obj)
        {
            SendHandCountChangedEvent();
        }

        private void HandleEvent(BankBalanceChangedEvent bank)
        {
            if (HandCount == 0)
            {
                return;
            }

            var balance = bank.NewBalance;
            CheckBankBalanceIsAboveMinimumRequirement(balance);
        }

        public void HandCountResetTimerElapsed()
        {
            resetTimerIsRunning = false;
            ResetHandCount();
        }

        private void CheckBankBalanceIsAboveMinimumRequirement(long balance)
        {
            var minimumRequiredCredits = (long)_properties.GetProperty(AccountingConstants.HandCountMinimumRequiredCredits,
                                                                       AccountingConstants.HandCountDefaultRequiredCredits);
            if (balance < minimumRequiredCredits)
            {
                OnResetTimerStarted.Invoke();
                resetTimerIsRunning = true;
            }
            else if (resetTimerIsRunning && balance >= minimumRequiredCredits)
            {
                OnResetTimerCancelled.Invoke();
                resetTimerIsRunning = false;
            }
        }

        public void IncrementHandCount()
        {
            _handCountMeter.Increment(1);
            SendHandCountChangedEvent();
            Logger.Info($"IncrementHandCount to {HandCount}");
        }

        public void DecreaseHandCount(int n)
        {
            _handCountMeter.Increment(-n);
            SendHandCountChangedEvent();
            Logger.Info($"DecreaseHandCount by {n} to {HandCount}");
        }

        public void ResetHandCount()
        {
            _handCountMeter.Increment(-HandCount);
            SendHandCountChangedEvent();
            Logger.Info($"ResetHandCount:{HandCount}");
        }

        public void SendHandCountChangedEvent()
        {
            _eventBus.Publish(new HandCountChangedEvent(HandCount));
        }
    }
}