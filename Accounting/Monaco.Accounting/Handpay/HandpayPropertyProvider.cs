namespace Aristocrat.Monaco.Accounting.Handpay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts;
    using Contracts.Handpay;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class HandpayPropertyProvider : IPropertyProvider
    {
        private const string ConfigurationExtensionPath = "/Accounting/Configuration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        public HandpayPropertyProvider()
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
               ConfigurationExtensionPath,
               () => new AccountingConfiguration
               {
                   Handpay = new AccountingConfigurationHandpay()
               });
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();


            var storageName = GetType().ToString();

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            // The Tuple is structured as value (Item1) and storageKey (Item2)
            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                {
                    AccountingConstants.EnabledLocalHandpay,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledLocalHandpay), true)
                },
                {
                    AccountingConstants.EnabledLocalCredit,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledLocalCredit), true)
                },
                {
                    AccountingConstants.EnabledLocalVoucher,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledLocalVoucher), true)
                },
                {
                    AccountingConstants.EnabledLocalWat,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledLocalWat), true)
                },
                {
                    AccountingConstants.EnabledRemoteHandpay,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledRemoteHandpay), true)
                },
                {
                    AccountingConstants.EnabledRemoteCredit,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledRemoteCredit), true)
                },
                {
                    AccountingConstants.EnabledRemoteVoucher,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledRemoteVoucher), true)
                },
                {
                    AccountingConstants.EnabledRemoteWat,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnabledRemoteWat), true)
                },
                {
                    AccountingConstants.DisabledLocalHandpay,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledLocalHandpay), true)
                },
                {
                    AccountingConstants.DisabledLocalCredit,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledLocalCredit), true)
                },
                {
                    AccountingConstants.DisabledLocalVoucher,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledLocalVoucher), true)
                },
                {
                    AccountingConstants.DisabledLocalWat,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledLocalWat), true)
                },
                {
                    AccountingConstants.DisabledRemoteHandpay,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledRemoteHandpay), true)
                },
                {
                    AccountingConstants.DisabledRemoteCredit,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledRemoteCredit), true)
                },
                {
                    AccountingConstants.DisabledRemoteVoucher,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledRemoteVoucher), true)
                },
                {
                    AccountingConstants.DisabledRemoteWat,
                    Tuple.Create(InitFromStorage(AccountingConstants.DisabledRemoteWat), true)
                },
                {
                    AccountingConstants.MixCreditTypes,
                    Tuple.Create(InitFromStorage(AccountingConstants.MixCreditTypes), true)
                },
                {
                    AccountingConstants.RequestNonCash,
                    Tuple.Create(InitFromStorage(AccountingConstants.RequestNonCash), true)
                },
                {
                    AccountingConstants.CombineCashableOut,
                    Tuple.Create(InitFromStorage(AccountingConstants.CombineCashableOut), true)
                },
                {
                    AccountingConstants.LocalKeyOff,
                    Tuple.Create(InitFromStorage(AccountingConstants.LocalKeyOff), true)
                },
                {
                    AccountingConstants.PartialHandpays,
                    Tuple.Create(InitFromStorage(AccountingConstants.PartialHandpays), true)
                },
                {
                    AccountingConstants.EnableReceipts,
                    Tuple.Create(InitFromStorage(AccountingConstants.EnableReceipts), true)
                },
                {
                    AccountingConstants.AllowGameWinReceipts,
                    Tuple.Create(InitFromStorage(AccountingConstants.AllowGameWinReceipts), true)
                },
                {
                    AccountingConstants.TitleJackpotReceipt,
                    Tuple.Create(InitFromStorage(AccountingConstants.TitleJackpotReceipt), true)
                },
                {
                    AccountingConstants.TitleCancelReceipt,
                    Tuple.Create(InitFromStorage(AccountingConstants.TitleCancelReceipt), true)
                },
                {
                    AccountingConstants.UsePlayerIdReader,
                    Tuple.Create(InitFromStorage(AccountingConstants.UsePlayerIdReader), true)
                },
                { AccountingConstants.IdReaderId, Tuple.Create(InitFromStorage(AccountingConstants.IdReaderId), true) },
                {
                    AccountingConstants.ValidateHandpays,
                    Tuple.Create(InitFromStorage(AccountingConstants.ValidateHandpays), true)
                },
                {
                    AccountingConstants.RemoteHandpayResetAllowed,
                    Tuple.Create(InitFromStorage(AccountingConstants.RemoteHandpayResetAllowed), true)
                },
                {
                    AccountingConstants.RemoteHandpayResetConfigurable,
                    Tuple.Create((object)configuration.Handpay?.RemoteHandpayReset?.Configurable ?? false, false)
                },
                {
                    AccountingConstants.EditableReceipts,
                    Tuple.Create((object)configuration.Handpay?.PrintHandpayReceipt?.Editable ?? true, false)
                },
                {
                    AccountingConstants.HandpayPendingExitEnabled,
                    Tuple.Create((object)configuration.Handpay?.HandpayPendingExitEnabled ?? false, false)
                },
                {
                    AccountingConstants.LargeWinTransactionName,
                    Tuple.Create((object)configuration.WinLimits?.LargeWinLimit?.OverrideTransactionName ?? true, false)
                },
                {
                    AccountingConstants.CanKeyOffWhileInLockUp,
                    Tuple.Create((object)configuration.Handpay?.CanKeyOffWhileInLockUp ?? true, false)
                },
                {
                    AccountingConstants.HandpayReceiptsRequired,
                    Tuple.Create((object)configuration.Handpay?.HandpayReceiptsRequired ?? false, false)

                }
            };

            if (!blockExists)
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);

                if (machineSettingsImported == ImportMachineSettings.None)
                {
                    SetProperty(AccountingConstants.EnabledLocalHandpay, true);
                    SetProperty(AccountingConstants.EnabledLocalCredit, true);
                    SetProperty(AccountingConstants.EnabledLocalVoucher, true);
                    SetProperty(AccountingConstants.EnabledLocalWat, true);
                    var printerEnabled = propertiesManager.GetValue(ApplicationConstants.PrinterEnabled, false);
                    SetProperty(
                        AccountingConstants.EnableReceipts,
                        printerEnabled && (configuration.Handpay?.PrintHandpayReceipt?.Enabled ?? false));
                    SetProperty(AccountingConstants.AllowGameWinReceipts, configuration.Handpay?.AllowGameWinReceipt?.Enabled ?? true);
                    SetProperty(AccountingConstants.DisabledLocalHandpay, true);
                    SetProperty(AccountingConstants.ValidateHandpays, true);
                    SetProperty(AccountingConstants.LocalKeyOff, LocalKeyOff.AnyKeyOff);
                    SetProperty(AccountingConstants.RemoteHandpayResetAllowed, configuration.Handpay?.RemoteHandpayReset?.Allowed ?? true);
                    SetProperty(AccountingConstants.RemoteHandpayResetConfigurable, configuration.Handpay?.RemoteHandpayReset?.Configurable ?? true);
                    SetProperty(AccountingConstants.LargeWinTransactionName, configuration.WinLimits?.LargeWinLimit?.OverrideTransactionName ?? true);
                    SetProperty(AccountingConstants.CanKeyOffWhileInLockUp, configuration.Handpay?.CanKeyOffWhileInLockUp ?? true);
                    // These properties, if later set, will override the values defined in the current PlayerTicket culture when printing the respective ticket
                    SetProperty(AccountingConstants.TitleJackpotReceipt, string.Empty);
                    SetProperty(AccountingConstants.TitleCancelReceipt, string.Empty);
                }
                else
                {
                    // The following settings were imported and set to the default property provider since the handpay property provider was not yet loaded
                    // at the time of import, so we set each to their imported values here. 
                    SetProperty(AccountingConstants.CombineCashableOut, propertiesManager.GetValue(AccountingConstants.CombineCashableOut, false));
                    SetProperty(AccountingConstants.DisabledLocalCredit, propertiesManager.GetValue(AccountingConstants.DisabledLocalCredit, false));
                    SetProperty(AccountingConstants.DisabledLocalHandpay, propertiesManager.GetValue(AccountingConstants.DisabledLocalHandpay, false));
                    SetProperty(AccountingConstants.DisabledLocalVoucher, propertiesManager.GetValue(AccountingConstants.DisabledLocalVoucher, false));
                    SetProperty(AccountingConstants.DisabledLocalWat, propertiesManager.GetValue(AccountingConstants.DisabledLocalWat, false));
                    SetProperty(AccountingConstants.DisabledRemoteCredit, propertiesManager.GetValue(AccountingConstants.DisabledRemoteCredit, false));
                    SetProperty(AccountingConstants.DisabledRemoteHandpay, propertiesManager.GetValue(AccountingConstants.DisabledRemoteHandpay, false));
                    SetProperty(AccountingConstants.DisabledRemoteVoucher, propertiesManager.GetValue(AccountingConstants.DisabledRemoteVoucher, false));
                    SetProperty(AccountingConstants.DisabledRemoteWat, propertiesManager.GetValue(AccountingConstants.DisabledRemoteWat, false));
                    SetProperty(AccountingConstants.EnabledLocalCredit, propertiesManager.GetValue(AccountingConstants.EnabledLocalCredit, false));
                    SetProperty(AccountingConstants.EnabledLocalHandpay, propertiesManager.GetValue(AccountingConstants.EnabledLocalHandpay, false));
                    SetProperty(AccountingConstants.EnabledLocalVoucher, propertiesManager.GetValue(AccountingConstants.EnabledLocalVoucher, false));
                    SetProperty(AccountingConstants.EnabledLocalWat, propertiesManager.GetValue(AccountingConstants.EnabledLocalWat, false));
                    SetProperty(AccountingConstants.EnabledRemoteCredit, propertiesManager.GetValue(AccountingConstants.EnabledRemoteCredit, false));
                    SetProperty(AccountingConstants.EnabledRemoteHandpay, propertiesManager.GetValue(AccountingConstants.EnabledRemoteHandpay, false));
                    SetProperty(AccountingConstants.EnabledRemoteVoucher, propertiesManager.GetValue(AccountingConstants.EnabledRemoteVoucher, false));
                    SetProperty(AccountingConstants.EnabledRemoteWat, propertiesManager.GetValue(AccountingConstants.EnabledRemoteWat, false));
                    SetProperty(AccountingConstants.EnableReceipts, propertiesManager.GetValue(AccountingConstants.EnableReceipts, false));
                    SetProperty(AccountingConstants.PartialHandpays, propertiesManager.GetValue(AccountingConstants.PartialHandpays, false));
                    SetProperty(AccountingConstants.LocalKeyOff, propertiesManager.GetValue(AccountingConstants.LocalKeyOff, LocalKeyOff.AnyKeyOff));
                    SetProperty(AccountingConstants.IdReaderId, propertiesManager.GetValue(AccountingConstants.IdReaderId, 0));
                    SetProperty(AccountingConstants.MixCreditTypes, propertiesManager.GetValue(AccountingConstants.MixCreditTypes, true));
                    SetProperty(AccountingConstants.RequestNonCash, propertiesManager.GetValue(AccountingConstants.RequestNonCash, false));
                    SetProperty(AccountingConstants.TitleCancelReceipt, propertiesManager.GetValue(AccountingConstants.TitleCancelReceipt, string.Empty));
                    SetProperty(AccountingConstants.TitleJackpotReceipt, propertiesManager.GetValue(AccountingConstants.TitleJackpotReceipt, string.Empty));
                    SetProperty(AccountingConstants.UsePlayerIdReader, propertiesManager.GetValue(AccountingConstants.UsePlayerIdReader, false));
                    SetProperty(AccountingConstants.LargeWinTransactionName, propertiesManager.GetValue(AccountingConstants.LargeWinTransactionName, configuration.WinLimits?.LargeWinLimit?.OverrideTransactionName ?? true));
                    SetProperty(AccountingConstants.CanKeyOffWhileInLockUp, propertiesManager.GetValue(AccountingConstants.CanKeyOffWhileInLockUp, configuration.Handpay?.CanKeyOffWhileInLockUp ?? true));
                    SetProperty(AccountingConstants.ValidateHandpays, propertiesManager.GetValue(AccountingConstants.ValidateHandpays, false));
                    SetProperty(AccountingConstants.RemoteHandpayResetAllowed, propertiesManager.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true));
                    SetProperty(AccountingConstants.AllowGameWinReceipts, propertiesManager.GetValue(AccountingConstants.AllowGameWinReceipts, configuration.Handpay?.AllowGameWinReceipt?.Enabled ?? true));

                    machineSettingsImported |= ImportMachineSettings.HandpayPropertiesLoaded;
                    propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
                }
            }
        }

        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown game property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            // NOTE:  Not all properties are persisted
            if (value.Item2)
            {
                Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");

                _persistentStorageAccessor[propertyName] = propertyValue;
            }

            _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
        }

        private object InitFromStorage(string propertyName, Action<object> onInitialized = null)
        {
            var value = _persistentStorageAccessor[propertyName];

            onInitialized?.Invoke(value);

            return value;
        }
    }
}