namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Common;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Storage;
    using Storage.Repository;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using log4net;
    using Storage.Models;

    /// <summary>
    ///     Provides a history buffer for AFT transfers
    /// </summary>
    public class AftHistoryBuffer : IAftHistoryBuffer
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int AftHistoryLogSize = 128; // 0x80
        private const byte LogSizeMask = AftHistoryLogSize - 1; // 0x7F
        private readonly IAftHistoryLog[] _buffer;
        private readonly IStorageDataProvider<AftHistoryItem> _historyDataProvider;

        /// <summary>
        ///     Initializes an instance of the AftHistoryBuffer class
        /// </summary>
        /// <param name="historyDataProvider"></param>
        public AftHistoryBuffer(IStorageDataProvider<AftHistoryItem> historyDataProvider)
        {
            _historyDataProvider = historyDataProvider ?? throw new ArgumentNullException(nameof(historyDataProvider));
            _buffer = StorageHelpers.Deserialize(
                _historyDataProvider.GetData().AftHistoryLog,
                () => Enumerable.Range(0, AftHistoryLogSize)
                    .Select(
                        _ => new AftHistoryLog
                        {
                            TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable,
                            ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
                        })).Cast<IAftHistoryLog>().ToArray();
        }

        /// <inheritdoc />
        public byte CurrentBufferIndex => (byte)(_historyDataProvider.GetData().CurrentBufferIndex - 1 % LogSizeMask + 1);

        /// <inheritdoc />
        public byte AddEntry(AftResponseData response)
        {
            _historyDataProvider.Save(UpdateHistoryLog(response)).FireAndForget();
            return response.TransactionIndex;
        }

        /// <inheritdoc />
        public byte AddEntry(AftResponseData response, IUnitOfWork work)
        {
            _historyDataProvider.Save(UpdateHistoryLog(response), work);
            return response.TransactionIndex;
        }

        /// <inheritdoc />
        public AftResponseData GetHistoryEntry(byte entryIndex)
        {
            Logger.Debug($"requesting entry 0x{entryIndex:X2} current index 0x{CurrentBufferIndex:X2}");

            // per the SAS Spec section 8.6 - The host may use the interrogation form
            // of long poll 72 to retrieve transactions from the history buffer using
            // either an absolute buffer position number or a relative transaction index.
            // Relative transaction index 0xFF references the transaction most recently
            // copied to the history buffer, index 0xFE references the transaction copied
            // prior to that, etc. Transaction index 0x01 thru 0x7F reference absolute
            // buffer positions.
            var bufferPosition = entryIndex < AftHistoryLogSize
                ? entryIndex    // 0x01 - 0x7F
                : GetRelativeEntryIndex(entryIndex);  // 0x80 - 0xFF

            Logger.Debug($"bufferPosition 0x{bufferPosition:X2}");
            var response = _buffer[bufferPosition];

            // per the SAS Spec section 8.6 - If the transaction index refers
            // to a transaction older than the oldest transaction currently
            // buffered by the gaming machine, or a buffer position that is
            // empty or greater than the maximum number of buffer positions
            // on the gaming machine, the response will have a transfer status
            // and receipt status of 0xFF, and the remaining fields are
            // omitted. The transaction buffer position in the response will
            // be the requested absolute position or relative index.
            if (response.TransferStatus == AftTransferStatusCode.NoTransferInfoAvailable)
            {
                response.TransactionIndex = entryIndex;
            }

            return AftResponseData.FromIAftHistoryLog(response);
        }

        private AftHistoryItem UpdateHistoryLog(AftResponseData response)
        {
            Logger.Debug($"Current entry index is 0x{CurrentBufferIndex:X4}");
            response.TransactionIndex = CurrentBufferIndex;
            _buffer[CurrentBufferIndex] = response;
            var history = _historyDataProvider.GetData();
            history.CurrentBufferIndex = GetNextEntryIndex();
            history.AftHistoryLog = StorageHelpers.Serialize(_buffer);
            return history;
        }

        private byte GetNextEntryIndex()
        {
            var index = CurrentBufferIndex;
            index++;
            index &= LogSizeMask;
            index = index == 0 ? (byte)1 : index;
            Logger.Debug($"next entry index is {index:X2}");
            return index;
        }

        private byte GetRelativeEntryIndex(byte entryIndex)
        {
            var offset = (byte)((entryIndex ^ byte.MaxValue) + 1);
            var index = (byte)((CurrentBufferIndex + entryIndex) & LogSizeMask);
            Logger.Debug($"offset is 0x{offset:X2} index is 0x{index:X2} current index is 0x{CurrentBufferIndex:X2}");
            if (offset > CurrentBufferIndex)
            {
                index--;
                Logger.Debug($"offset > current index. new index is 0x{index:X2}");
            }

            if (index == 0)
            {
                index = LogSizeMask;
                Logger.Debug($"index is zero. new index is 0x{index:X2}");
            }

            return index;
        }
    }
}