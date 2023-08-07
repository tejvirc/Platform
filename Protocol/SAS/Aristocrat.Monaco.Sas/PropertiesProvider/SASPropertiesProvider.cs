namespace Aristocrat.Monaco.Sas.PropertiesProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Definition of the SasPropertiesProvider class.
    /// </summary>
    public class SasPropertiesProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<string, object> _properties;

        /// <summary>
        ///     Initializes a new instance of the SasPropertiesProvider class.
        /// </summary>
        public SasPropertiesProvider()
        {
            _properties = new Dictionary<string, object>
            {
               { SasProperties.SasNoValidationSeedsReceivedKey, false },        // indicates whether no validation seeds were received
               { SasProperties.SasValidationBufferFullKey, false },             // indicates whether the validation buffer is full or not
               { SasProperties.SasShutdownCommandReceivedKey, false },          // indicates whether the shutdown command was received or not
               { SasProperties.SasCommunicationsOfflineKey, false },            // indicates whether Sas communications are offline
               { SasProperties.AftCustomTicketsSupportedKey, false },           // indicates whether Aft custom tickets are supported
               { SasProperties.AftTransactionReceiptsSupportedKey, true },      // indicates whether Aft transaction receipts are supported
               { SasProperties.AftLockAfterTransferSupportedKey, false },       // indicates whether Aft lock after transfer is supported
               { SasProperties.AftClientChoosesCashOutDeviceKey, true },        // indicates whether the client can choose the Aft cash out device
               { SasProperties.RelaxedAftLockCheckKey, false },                 // indicates whether Aft relaxed lock check is enabled
               { SasProperties.LockForFinalAftInterrogateKey, false },          // indicates whether lock for final Aft interrogate is enabled
               { SasProperties.HopperSupportedKey, false },                     // indicates whether a hopper is supported
               { SasProperties.TokenDenominationKey, 1 },                       // indicates the token denomination (cents) supported
               { SasProperties.CoinAcceptorSupportedKey, false },               // indicates whether a coin acceptor is supported
               { SasProperties.TrackHopperPercentageSupportedKey, false },      // indicates whether tracking hopper percentage is enabled
               { SasProperties.TrackHopperLevelSupportedKey, false },           // indicates whether tracking hopper level is enabled
               { SasProperties.RemoteHandPayResetSupportedKey, true },          // indicates whether remote hand pay reset is supported
               { SasProperties.MultipleGameSupportedKey, true },                // indicates whether multiple games are allowed
               { SasProperties.MultipleDenominationSupportedKey, true },        // indicates whether multiple denominations are allowed
               { SasProperties.TournamentSupportedKey, false },                 // indicates whether tournaments are supported
               { SasProperties.ProgressivesSupportedKey, false },               // indicates whether progressives are supported
               { SasProperties.ComponentAuthenticationSupportedKey, true },     // indicates whether component authentication is supported
               { SasProperties.JackpotMultiplierSupportedKey, false },          // indicates whether jackpot multiplier is supported
               { SasProperties.CompareEntirePacketForImpliedAcknowledgementKey, false }, // indicates whether packets should be compared entirely for implied acknowledgments of messages received.
               { SasProperties.LoggingSupportedKey, false },                    // indicates whether logging is enabled or not
               { SasProperties.TicketingSupportedKey, true },                   // indicates whether ticketing is enabled
               { SasProperties.TicketRedemptionSupportedKey, true },            // indicates whether ticket redemption is supported
               { SasProperties.RestrictedTicketsSupportedKey, true },           // indicates whether restricted tickets are allowed
               { SasProperties.ForeignRestrictedTicketsSupportedKey, false },    // indicates whether foreign restricted tickets are allowed
               { SasProperties.HandPayValidationSupportedKey, true },           // indicates whether hand pay validation is supported
               { SasProperties.PrinterAsHandPayDeviceSupportedKey, true },      // indicates whether the printer can be used to print hand pay receipts
               { SasProperties.PrinterAsCashOutDeviceSupportedKey, true },      // indicates whether the printer can be used as the cash out device// the number of days until a ticket can no longer be cashed
               { SasProperties.DefaultLocationKey, Localizer.For(CultureFor.Player).GetString(ResourceKeys.DataUnavailable) },     // the default location to appear on a ticket TODO make persisted and update when we can configure this from the audit menu
               { SasProperties.DefaultAddressLine1Key, Localizer.For(CultureFor.Player).GetString(ResourceKeys.DataUnavailable) }, // the default first line of an address to appear on a ticket TODO make persisted and update when we can configure this from the audit menu
               { SasProperties.DefaultAddressLine2Key, Localizer.For(CultureFor.Player).GetString(ResourceKeys.DataUnavailable) }, // the default second line of an address to appear on a ticket TODO make persisted and update when we can configure this from the audit menu
               { SasProperties.DefaultRestrictedTitleKey, AccountingConstants.DefaultNonCashTicketTitle },    // the default restricted title to appear on a ticket TODO move this to a resource item when the account layer does this
               { SasProperties.DefaultDebitTitleKey, AccountingConstants.DefaultCashoutTicketTitle },        // the default debit title to appear on a ticket TODO make this a resource item once supported
               { SasProperties.AftTransferReceiptLocationLine, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Location) }, // the string that is on the Aft transfer receipt location line
               { SasProperties.AftTransferReceiptAddressLine1, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Address1) },    // the string that is on the Aft transfer receipt address1 line
               { SasProperties.AftTransferReceiptAddressLine2, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Address2) },    // the string that is on the Aft transfer receipt address2 line
               { SasProperties.AftTransferReceiptInHouseLine1, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse1) },    // the string that is on the Aft transfer receipt in-house1 line
               { SasProperties.AftTransferReceiptInHouseLine2, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse2) },    // the string that is on the Aft transfer receipt in-house2 line
               { SasProperties.AftTransferReceiptInHouseLine3, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse3) },    // the string that is on the Aft transfer receipt in-house3 line
               { SasProperties.AftTransferReceiptInHouseLine4, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.InHouse4) },    // the string that is on the Aft transfer receipt in-house4 line
               { SasProperties.AftTransferReceiptDebitLine1, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit1) },    // the string that is on the Aft transfer receipt debit1 line
               { SasProperties.AftTransferReceiptDebitLine2, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit2) },    // the string that is on the Aft transfer receipt debit2 line
               { SasProperties.AftTransferReceiptDebitLine3, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit3) },    // the string that is on the Aft transfer receipt debit3 line
               { SasProperties.AftTransferReceiptDebitLine4, Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.Debit4) },    // the string that is on the Aft transfer receipt debit4 line
               { SasProperties.ExtendedTicketDataStatusClearPending, false },
               { SasProperties.AftTransferInterrogatePending, false },
               { SasProperties.SasVersion, 603 },
               { SasProperties.PreviousSelectedGameId, 0 },
               { SasProperties.MeterCollectStatusKey, (byte)MeterCollectStatus.NotInPendingChange }
            };
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var returnObject))
            {
                return returnObject;
            }

            var errorMessage = "Cannot get unknown property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (_properties.ContainsKey(propertyName))
            {
                Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
                _properties[propertyName] = propertyValue;
            }
            else
            {
                var errorMessage = "Cannot set unknown property: " + propertyName;
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            Logger.Info($"[CONFIG] Setting the property {propertyName} with {propertyValue}");
        }
    }
}
