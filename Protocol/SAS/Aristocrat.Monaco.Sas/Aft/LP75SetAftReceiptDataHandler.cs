namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP 75 Set Aft Receipt Data
    /// </summary>
    public class LP75SetAftReceiptDataHandler : ISasLongPollHandler<LongPollResponse, SetAftReceiptData>
    {
        private readonly IPropertiesManager _propertiesManager;

        private readonly Dictionary<TransactionReceiptDataField, string> _receiptDataProperties =
            new Dictionary<TransactionReceiptDataField, string>
            {
                { TransactionReceiptDataField.Location, SasProperties.AftTransferReceiptLocationLine },
                { TransactionReceiptDataField.Address1, SasProperties.AftTransferReceiptAddressLine1 },
                { TransactionReceiptDataField.Address2, SasProperties.AftTransferReceiptAddressLine2 },
                { TransactionReceiptDataField.InHouse1, SasProperties.AftTransferReceiptInHouseLine1 },
                { TransactionReceiptDataField.InHouse2, SasProperties.AftTransferReceiptInHouseLine2 },
                { TransactionReceiptDataField.InHouse3, SasProperties.AftTransferReceiptInHouseLine3 },
                { TransactionReceiptDataField.InHouse4, SasProperties.AftTransferReceiptInHouseLine4 },
                { TransactionReceiptDataField.Debit1, SasProperties.AftTransferReceiptDebitLine1 },
                { TransactionReceiptDataField.Debit2, SasProperties.AftTransferReceiptDebitLine2 },
                { TransactionReceiptDataField.Debit3, SasProperties.AftTransferReceiptDebitLine3 },
                { TransactionReceiptDataField.Debit4, SasProperties.AftTransferReceiptDebitLine4 }
            };

        private readonly Dictionary<string, string> _propertyDefaultValues;

        /// <summary>
        ///     Creates an instance of the LP75SetAftReceiptDataHandler class
        /// </summary>
        /// <param name="propertiesManager">IPropertiesManager interface object for updating properties.</param>
        public LP75SetAftReceiptDataHandler(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _propertyDefaultValues = new Dictionary<string, string>();

            // Since the AFT Transfer Receipt properties are not persisted,
            // store the original values now since they will not have been
            // changed at construction time.
            foreach (var property in _receiptDataProperties.Values)
            {
                _propertyDefaultValues[property] =
                    _propertiesManager.GetValue(property, string.Empty);
            }
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SetAftReceiptData
        };

        /// <inheritdoc />
        public LongPollResponse Handle(SetAftReceiptData data)
        {
            foreach (var datum in data.TransactionReceiptValues)
            {
                _propertiesManager.SetProperty(_receiptDataProperties[datum.Key],
                    datum.Value == SasConstants.UseDefault
                        ? _propertyDefaultValues[_receiptDataProperties[datum.Key]]
                        : datum.Value);
            }

            return new LongPollResponse();
        }
    }
}