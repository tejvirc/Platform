﻿namespace Aristocrat.Monaco.Accounting.HandCount
{
    using Application.Contracts.Metering;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using Contracts.HandCount;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Timers;

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

        private Timer _initResetTimer;
        private const double ResetTimerIntervalInMs = 15000;

        private bool _resetTimerIsRunning;
        private bool _disposed;

        /// <summary>
        ///     Constructs the service by retrieving all necessary services from the service manager. This
        ///     constructor is necessary because this is a service in the accounting layer where DI is not used.
        /// </summary>
        public HandCountService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
            )
        {
        }

        /// <summary>
        ///     Constructs the service taking all required services as parameters. For unit testing.
        /// </summary>
        public HandCountService(
            IEventBus eventBus,
            IMeterManager meters,
            IPropertiesManager propertyProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _properties = propertyProvider ?? throw new ArgumentNullException(nameof(propertyProvider));
            _handCountMeter = _meters.GetMeter(AccountingMeters.HandCount);

            _initResetTimer = new Timer(ResetTimerIntervalInMs);
            _initResetTimer.Elapsed += InitHandCountReset;
        }

        public string Name => typeof(HandCountService).FullName;

        public int HandCount => (int)_handCountMeter.GetValue(MeterTimeframe.Lifetime);

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountService) };

        /// <inheritdoc />
        public bool HandCountServiceEnabled => (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
            .GetProperty(AccountingConstants.HandCountServiceEnabled, false);

        public void Initialize()
        {
            _eventBus.Subscribe<InitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, x => HandCountResetTimerElapsed());

            _eventBus.Subscribe<OpenEvent>(this, x => SuspendResetHandcount());
            _eventBus.Subscribe<SystemDisabledEvent>(this, x => SuspendResetHandcount());

            _eventBus.Subscribe<ClosedEvent>(this, x => CheckIfBelowResetThreshold());
            _eventBus.Subscribe<SystemEnabledEvent>(this, x => CheckIfBelowResetThreshold());

            //If bank balance is changed and new balance is above the min required credits,
            //suspend the reset hand count if underway.
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
        }

        private void HandleEvent(BankBalanceChangedEvent obj)
        {
            var minimumRequiredCredits = (long)_properties.GetProperty(AccountingConstants.HandCountMinimumRequiredCredits,
                                                                       AccountingConstants.HandCountDefaultRequiredCredits);

            if (obj.NewBalance > obj.OldBalance && obj.NewBalance >= minimumRequiredCredits)
            {
                SuspendResetHandcount();
            }
        }

        private void HandleEvent(InitializationCompletedEvent obj)
        {
            SendHandCountChangedEvent();

            //Recovery scenario when machine restarted during reset hand count check
            CheckIfBelowResetThreshold();
        }

        private void SuspendResetHandcount()
        {
            _initResetTimer.Stop();

            if (_resetTimerIsRunning)
            {
                _eventBus.Publish(new HandCountResetTimerCancelledEvent());
                _resetTimerIsRunning = false;
            }
        }

        public void CheckIfBelowResetThreshold()
        {
            if (HandCount == 0)
            {
                return;
            }

            var balance = (long)_properties.GetProperty(PropertyKey.CurrentBalance, 0L);
            var minimumRequiredCredits = (long)_properties.GetProperty(AccountingConstants.HandCountMinimumRequiredCredits,
                                                                       AccountingConstants.HandCountDefaultRequiredCredits);

            if (balance < minimumRequiredCredits)
            {
                _initResetTimer.Start();
            }
        }

        private void InitHandCountReset(object sender, ElapsedEventArgs e)
        {
            if (!_resetTimerIsRunning)
            {
                _eventBus.Publish(new HandCountResetTimerStartedEvent());
                _resetTimerIsRunning = true;
            }
        }

        private void HandCountResetTimerElapsed()
        {
            _resetTimerIsRunning = false;
            ResetHandCount();
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

            //Hands count are reset when cashout happens
            //Check if need to reset hand count
            CheckIfBelowResetThreshold();
        }

        private void ResetHandCount()
        {
            _handCountMeter.Increment(-HandCount);
            _initResetTimer.Stop();
            SendHandCountChangedEvent();
            Logger.Info($"ResetHandCount:{HandCount}");
        }

        private void SendHandCountChangedEvent()
        {
            _eventBus.Publish(new HandCountChangedEvent(HandCount));
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
                SuspendResetHandcount();
                _initResetTimer.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}