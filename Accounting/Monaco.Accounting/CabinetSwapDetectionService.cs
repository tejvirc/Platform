namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;

    public class CabinetSwapDetectionService : IService, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string CarrierBoardWasRemovedKey = @"CarrierBoardWasRemoved";
        private const string LastRecordedMacAddressKey = @"LastRecordedMacAddress";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid DisabledDueToCarrierBoardRemoval =
            AccountingConstants.DisabledDueToCarrierBoardRemovalKey;

        private readonly IPersistentStorageAccessor _accessor;
        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IIO _iio;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITransferOutHandler _transferOutHandler;

        private bool _disposed;

        public CabinetSwapDetectionService()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<ITransferOutHandler>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<IIO>())
        {
        }

        [CLSCompliant(false)]
        public CabinetSwapDetectionService(
            IPropertiesManager properties,
            IPersistentStorageManager storage,
            ISystemDisableManager systemDisableManager,
            ITransferOutHandler transferOutHandler,
            IEventBus bus,
            IBank bank,
            IIO iio)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _transferOutHandler = transferOutHandler ?? throw new ArgumentNullException(nameof(transferOutHandler));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _iio = iio ?? throw new ArgumentNullException(nameof(iio));

            if (CashoutOnBoardRemovalEnabled)
            {
                _accessor = GetAccessor();
            }
        }

        private bool CashoutOnBoardRemovalEnabled => _properties.GetValue(
            AccountingConstants.CashoutOnCarrierBoardRemovalEnabled,
            false);

        private bool CashoutInProgress { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(CabinetSwapDetectionService) };

        /// <summary>
        ///     Starts the cabinet swap detection service. Immediately it will check if the carrier board was removed and
        ///     if it wasn't then will check if the MAC address has changed. If any one of these are true, and there
        ///     is a credit balance, the game will be disabled and only re-enabled after the operator does a cash out
        ///     by toggling the jackpot key. If the two checks are true and there is no credit balance, the flags for
        ///     those checks will be reset and no lockup will happen.
        /// </summary>
        public void Initialize()
        {
            if (CashoutOnBoardRemovalEnabled)
            {
                if (WasCabinetSwapped())
                {
                    Logger.Info("Cabinet was swapped");

                    // Check the bank has a balance & force cashout if necessary
                    if (_bank.QueryBalance() > 0)
                    {
                        SetupTransferOut();
                    }
                    else
                    {
                        // Bank balance is zero, therefor no cashout needed. Reset persisted values
                        _accessor[CarrierBoardWasRemovedKey] = false;
                        _accessor[LastRecordedMacAddressKey] = NetworkInterfaceInfo.DefaultPhysicalAddress;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private IPersistentStorageAccessor GetAccessor()
        {
            var blockName = GetType().ToString();
            IPersistentStorageAccessor accessor;
            if (_storage.BlockExists(blockName))
            {
                accessor = _storage.GetBlock(blockName);
            }
            else
            {
                var blockFormat = new BlockFormat();

                blockFormat.AddFieldDescription(new FieldDescription(FieldType.Bool, 0, CarrierBoardWasRemovedKey));

                blockFormat.AddFieldDescription(new FieldDescription(FieldType.String, 18, 0, LastRecordedMacAddressKey));

                accessor = _storage.CreateDynamicBlock(Level, blockName, 1, blockFormat);
                if (accessor == null)
                {
                    var errorMessage = "Cannot create " + blockName;
                    Logger.Error(errorMessage);
                    throw new ServiceException(errorMessage);
                }

                accessor[CarrierBoardWasRemovedKey] = false;
                accessor[LastRecordedMacAddressKey] = NetworkInterfaceInfo.DefaultPhysicalAddress;
            }

            return accessor;
        }

        private bool WasCabinetSwapped()
        {
            // Check carrier board flag is set
            var boardWasRemoved = _iio.WasCarrierBoardRemoved;

            // If the flag is set, persist the value. If not, see if a value was already being persisted
            if (boardWasRemoved)
            {
                Logger.Debug("Carrier board was removed");
                _accessor[CarrierBoardWasRemovedKey] = true;
            }
            else if ((bool?)_accessor[CarrierBoardWasRemovedKey] == true)
            {
                Logger.Debug("Carrier board was previously removed");
                boardWasRemoved = true;
            }

            // Check for MAC address change if the board wasn't removed
            if (!boardWasRemoved)
            {
                if ((string)_accessor[LastRecordedMacAddressKey] != NetworkInterfaceInfo.DefaultPhysicalAddress)
                {
                    Logger.Debug("MAC address changed");
                    boardWasRemoved = true;
                }
            }

            return boardWasRemoved;
        }

        private void SetupTransferOut()
        {
            AddLockup();

            // Listen for the jackpot key to be toggled
            _bus.Subscribe<DownEvent>(
                this,
                Handle,
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            _bus.Subscribe<TransferOutCompletedEvent>(this, Handle);
            _bus.Subscribe<TransferOutFailedEvent>(this, Handle);
        }

        private void Handle(DownEvent ev)
        {
            if (!CashoutInProgress)
            {
                Logger.Debug("Begin TransferOut");

                CashoutInProgress = true;

                _transferOutHandler.TransferOut();
            }
        }

        private void Handle(TransferOutCompletedEvent ev)
        {
            if (_bank.QueryBalance(AccountType.Cashable) + _bank.QueryBalance(AccountType.Promo) > 0)
            {
                // A transfer out completed event was received, but there is still a balance of non-restricted credits. 
                // Do the transfer out again. Note that if there are non-cashable credits the operator
                // will need to remove them.
                CashoutInProgress = false;

                _bus.Publish(new DownEvent((int)ButtonLogicalId.Button30));
            }
            else
            {
                Logger.Debug("SUCCESS: TransferOut complete");

                RemoveLockup();

                _bus.UnsubscribeAll(this);

                // Transfer out complete, reset persisted flags
                _accessor[CarrierBoardWasRemovedKey] = false;
                _accessor[LastRecordedMacAddressKey] = NetworkInterfaceInfo.DefaultPhysicalAddress;
            }
        }

        private void Handle(TransferOutFailedEvent ev)
        {
            // Check to see if there is a balance of non-restricted after the transfer. If there is, 
            // transfer out again. Otherwise what remains are restricted credits, which must be played,
            // so the lockup can be removed
            if (_bank.QueryBalance(AccountType.Cashable) + _bank.QueryBalance(AccountType.Promo) > 0)
            {
                Logger.Error("TransferOut failed");

                CashoutInProgress = false;

                // Toggle the JP key again
                _bus.Publish(new DownEvent((int)ButtonLogicalId.Button30));
            }
            else
            {
                Logger.Debug("Transfer out failed due to remaining credits being restricted");

                RemoveLockup();
            }
        }

        private void AddLockup()
        {
            _systemDisableManager.Disable(
                DisabledDueToCarrierBoardRemoval,
                SystemDisablePriority.Immediate,
                DisableMessageCallback);
        }

        private string DisableMessageCallback()
        {
            var divisor = (double)_properties.GetProperty("CurrencyMultiplier", 0d);
            return string.Format(
                Localizer.ForLockup().GetString(ResourceKeys.BoardRemovalCashoutMessage)
                    .Replace("\\r\\n", Environment.NewLine),
                _bank.QueryBalance() / divisor);
        }

        private void RemoveLockup()
        {
            _systemDisableManager.Enable(DisabledDueToCarrierBoardRemoval);
        }
    }
}