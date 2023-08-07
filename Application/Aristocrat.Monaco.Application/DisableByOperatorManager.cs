namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     An implementation of the <see cref="IDisableByOperatorManager" /> interface that persists
    ///     the current disable state and ensures thread safety when calling Disable and Enable.
    /// </summary>
    public class DisableByOperatorManager : IService, IDisableByOperatorManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid DisableGuid = ApplicationConstants.SystemDisableGuid;

        private static string DisabledByOperatorMessage
        {
            get
            {
                var disabledMessage = ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(
                    ApplicationConstants.DisabledByOperatorText, string.Empty);
                if (disabledMessage == string.Empty)
                {
                    disabledMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfService);
                }
                return disabledMessage;
            }
        }

        private readonly object _lock = new object();

        /// <inheritdoc />
        public bool DisabledByOperator { get; private set; }

        /// <inheritdoc />
        public void Disable(Func<string> disableReason)
        {
            lock (_lock)
            {
                if (!DisabledByOperator)
                {
                    DoDisable(disableReason);
                    Persist();
                }
            }
        }

        /// <inheritdoc />
        public void Enable()
        {
            lock (_lock)
            {
                if (DisabledByOperator)
                {
                    Logger.Info("Enabling ...");
                    DisabledByOperator = false;

                    var serviceManager = ServiceManager.GetInstance();

                    serviceManager.GetService<ISystemDisableManager>().Enable(DisableGuid);

                    serviceManager.GetService<IEventBus>().Publish(new SystemEnabledByOperatorEvent());

                    Persist();
                }
            }
        }

        /// <inheritdoc />
        public string Name => "Disable-By-Operator Manager";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDisableByOperatorManager) };

        /// <inheritdoc />
        public void Initialize()
        {
            lock (_lock)
            {
                Logger.Info("Initializing...");

                var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                var blockName = GetType().ToString();
                if (storageService.BlockExists(blockName))
                {
                    var accessor = storageService.GetBlock(blockName);
                    var disabled = (bool)accessor["DisabledByOperator"];
                    if (disabled)
                    {
                        DoDisable(() => Localizer.For(CultureFor.Player).GetString(ResourceKeys.OutOfService));
                    }
                }
                else
                {
                    storageService.CreateBlock(PersistenceLevel.Critical, blockName, 1);
                }

                Logger.Info("Initialized");
            }
        }

        private void Persist()
        {
            var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var accessor = storageService.GetBlock(GetType().ToString());
            using (var transaction = accessor.StartTransaction())
            {
                transaction["DisabledByOperator"] = DisabledByOperator;
                transaction.Commit();
            }
        }

        private void DoDisable(Func<string> disableReason)
        {
            if (DisabledByOperatorMessage != string.Empty && disableReason.Invoke().Equals(
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfService)))
            {
                disableReason = () => DisabledByOperatorMessage;
            }

            Logger.DebugFormat($"Disabling for reason: {disableReason}...");
            DisabledByOperator = true;

            var serviceManager = ServiceManager.GetInstance();

            serviceManager.GetService<ISystemDisableManager>()
                .Disable(DisableGuid, SystemDisablePriority.Immediate, disableReason);

            serviceManager.GetService<IEventBus>().Publish(new SystemDisabledByOperatorEvent());
        }
    }
}