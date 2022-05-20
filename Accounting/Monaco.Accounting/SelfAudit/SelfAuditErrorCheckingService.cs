namespace Aristocrat.Monaco.Accounting.SelfAudit
{
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.SelfAudit;
    using Kernel;
    using Kernel.Contracts.Events;
    using Kernel.Contracts.LockManagement;
    using Localization.Properties;
    using log4net;
    using Mono.Addins;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;

    public class SelfAuditErrorCheckingService : ISelfAuditErrorCheckingService, IService, IDisposable
    {
        private const double SelfAuditTimerIntervalInMilliSeconds = 15000;
        private const string CreditMetersProviderPath = "/Accounting/CreditMetersProvider";
        private const string DebitMetersProviderPath = "/Accounting/DebitMetersProvider";
        private const string SelfAuditRunAdviceProviderPath = "/Accounting/SelfAuditRunAdviceProvider";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _sync = new object();

        private readonly IMeterManager _meterManager;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly ILockManager _lockManager;
        private readonly IPropertiesManager _propertiesManager;
        private bool _disposed;
        private Timer _selfAuditTimer;

        private List<ISelfAuditRunAdviceProvider> _runAdviceProviders;
        private List<IMeter> _creditMeters;
        private List<IMeter> _debitMeters;

        /// <summary>
        ///     Returns instance on SelfAuditErrorCheckingService
        /// </summary>
        public SelfAuditErrorCheckingService()
            : this(
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<ILockManager>())
        {
        }

        /// <summary>
        ///     Returns instance on SelfAuditErrorCheckingService
        /// </summary>
        /// <param name="meterManager"></param>
        /// <param name="propertiesManager"></param>
        /// <param name="eventBus"></param>
        /// <param name="disableManager"></param>
        /// <param name="lockManager"></param>
        public SelfAuditErrorCheckingService(
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            ISystemDisableManager disableManager,
            ILockManager lockManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool CheckSelfAuditPassing()
        {
            if (!(bool)_propertiesManager.GetProperty(AccountingConstants.SelfAuditErrorCheckingEnabled, false))
            {
                return true;
            }

            lock (_sync)
            {
                //Stop the timer
                _selfAuditTimer.Stop();
                Logger.Info("Running self audit error checking.");

                using (_lockManager.AcquireExclusiveLock(_creditMeters.Union(_debitMeters)))
                {
                    var currentCredit = _meterManager.GetMeter(AccountingMeters.CurrentCredits).Lifetime;
                    long sum = 0;
                    unchecked
                    {
                        foreach (var meter in _creditMeters)
                        {
                            sum += meter.Lifetime;
                        }

                        foreach (var meter in _debitMeters)
                        {
                            sum -= meter.Lifetime;
                        }

                        //When a long variable is incremented beyond its max value, it rolls over to its min value which is a negative number.
                        //Since money added and removed into EGM may exceed that long max value over time, it may produce a negative sum
                        //because of roll over of long variable, whereas current credit might still be a small number and thus never rolled over.
                        //RolledOverValue method is considering the negative value and changes it into a positive no as if
                        //the variable is rolling back to zero after reaching max value.
                        sum = RolledOverValue(sum);
                    }

                    var auditFailed = RolledOverValue(currentCredit) != sum;

                    if (auditFailed)
                    {
                        //Making a comma separated list of meter names and values
                        var meterValuesString = string.Join(
                            ", ",
                            _creditMeters.Select(m => $"{m.Name}={m.Lifetime}").Union(
                                _debitMeters.Select(m => $"{m.Name}={m.Lifetime}")));

                        //When self audit error occurs, lock the egm and publish the event
                        Logger.Error(
                            $"Self audit error occurred. Meter values are - Current Credit = {currentCredit}, {meterValuesString}");
                        _propertiesManager.SetProperty(AccountingConstants.SelfAuditErrorOccurred, true);
                        DisableEgmAndPublishEvent();
                        return false;
                    }
                }

                _ = TryResumeSelfAuditTimer();
            }

            return true;
        }

        /// <inheritdoc />
        public string Name => nameof(SelfAuditErrorCheckingService);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISelfAuditErrorCheckingService) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (!(bool)_propertiesManager.GetProperty(AccountingConstants.SelfAuditErrorCheckingEnabled, false))
            {
                return;
            }

            if (_propertiesManager.GetValue(AccountingConstants.SelfAuditErrorOccurred, false))
            {
                DisableEgmAndPublishEvent();
                return;
            }

            //Initialize timer with no due time
            _selfAuditTimer = new Timer();
            _selfAuditTimer.Interval = SelfAuditTimerIntervalInMilliSeconds;
            _selfAuditTimer.Elapsed += OnCheckSelfAudit;

            _eventBus.Subscribe<InitializationCompletedEvent>(this, InitializationCompletedEventHandler);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_selfAuditTimer != null)
                {
                    _selfAuditTimer.Stop();
                    _selfAuditTimer.Dispose();
                    _selfAuditTimer = null;
                }

                //Unassign the event handler
                if (_runAdviceProviders != null)
                {
                    _runAdviceProviders.ForEach(p => p.RunAdviceChanged -= HandleEvent);
                }
            }

            _disposed = true;
        }

        //Returns a rolled over value of long, taking care of overflow into negative nos
        private static long RolledOverValue(long value)
        {
            unchecked
            {
                if (value < 0)
                {
                    value += long.MaxValue + 1;
                }
            }

            return value;
        }

        private void InitializationCompletedEventHandler(InitializationCompletedEvent obj)
        {
            PrepareForFirstUse();
            _ = TryResumeSelfAuditTimer();
        }

        private void PrepareForFirstUse()
        {
            var addinHelper = ServiceManager.GetInstance().GetService<IAddinHelper>();
            var nodes = addinHelper.GetSelectedNodes<TypeExtensionNode>(CreditMetersProviderPath);
            var creditMeterProviders = nodes.Select(node => (ICreditMetersProvider)node.CreateInstance()).ToList();

            nodes = addinHelper.GetSelectedNodes<TypeExtensionNode>(DebitMetersProviderPath);
            var debitMeterProviders = nodes.Select(node => (IDebitMetersProvider)node.CreateInstance()).ToList();

            nodes = addinHelper.GetSelectedNodes<TypeExtensionNode>(SelfAuditRunAdviceProviderPath);
            _runAdviceProviders = new List<ISelfAuditRunAdviceProvider>();
            foreach (var node in nodes)
            {
                var statusProvider = (ISelfAuditRunAdviceProvider)node.CreateInstance();
                //Handle event to suspend/resume timer
                statusProvider.RunAdviceChanged += HandleEvent;
                _runAdviceProviders.Add(statusProvider);
            }

            _creditMeters = new List<IMeter>();
            foreach (var cmp in creditMeterProviders)
            {
                _creditMeters.AddRange(cmp.GetMeters());
            }

            _debitMeters = new List<IMeter>();
            foreach (var cmp in debitMeterProviders)
            {
                _debitMeters.AddRange(cmp.GetMeters());
            }

            if (_creditMeters is null || _debitMeters is null || _runAdviceProviders is null)
            {
                throw new Exception("Self audit error checking service is not properly initialized.");
            }
        }

        private void HandleEvent(object sender, EventArgs e)
        {
            _ = TryResumeSelfAuditTimer();
        }

        private bool TryResumeSelfAuditTimer()
        {
            if (_runAdviceProviders.Any(x => !x.SelfAuditOkToRun()))
            {
                Logger.Info(
                    "Current game play state is not idle or EGM is in lockup mode, suspending the self audit timer.");
                _selfAuditTimer.Stop();
                return false;
            }

            Logger.Info("Self audit error checking timer resumed.");
            _selfAuditTimer.Start();
            return true;
        }

        private void OnCheckSelfAudit(object sender, ElapsedEventArgs e)
        {
            CheckSelfAuditPassing();
        }

        private void DisableEgmAndPublishEvent()
        {
            Logger.Error("Disabling the EGM...");
            _disableManager.Disable(
                ApplicationConstants.SelfAuditErrorGuid,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfAuditError),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfAuditErrorHelpText));

            _eventBus.Publish(new SelfAuditErrorEvent());
        }
    }
}