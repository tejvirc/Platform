namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handles Interrogate
    /// </summary>
    public class AftInterrogate : IAftRequestProcessorTransferCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAftTransferProvider _aftProvider;
        private readonly IAftHistoryBuffer _historyBuffer;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IHostAcknowledgementHandler _nullHandlers = new HostAcknowledgementHandler();
        private readonly IHostAcknowledgementHandler _handlers;

        /// <summary>
        ///     Instantiates a new instance of the AftInterrogate class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="historyBuffer">reference to the AftHistoryBuffer class</param>
        /// <param name="propertiesManager">reference to the PropertiesManager class</param>
        public AftInterrogate(IAftTransferProvider aftProvider, IAftHistoryBuffer historyBuffer, IPropertiesManager propertiesManager)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _historyBuffer = historyBuffer ?? throw new ArgumentNullException(nameof(historyBuffer));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            // implied ACK/NACK handlers
            _handlers = new HostAcknowledgementHandler
            {
                ImpliedAckHandler = () =>
                {
                    if (_propertiesManager.GetValue(SasProperties.AftTransferInterrogatePending, false))
                    {
                        _propertiesManager.SetProperty(SasProperties.AftTransferInterrogatePending, false);
                        Task.Run(() => _aftProvider.CreateNewTransactionHistoryEntry());
                    }
                },
                ImpliedNackHandler = () =>
                {
                    _propertiesManager.SetProperty(SasProperties.AftTransferInterrogatePending, false);
                }
            };
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug($"Aft Interrogate with index={data.TransactionIndex}");

            // asking for a transaction from the history buffer
            if (data.TransactionIndex != 0)
            {
                return _historyBuffer.GetHistoryEntry(data.TransactionIndex);
            }

            // has a transfer completed but not been added to the history buffer?
            if (_aftProvider.CurrentTransfer?.TransferStatus == AftTransferStatusCode.FullTransferSuccessful ||
                 _aftProvider.CurrentTransfer?.TransferStatus == AftTransferStatusCode.PartialTransferSuccessful)
            {
                // if the non-zero amount transfer has completed successfully then store it in the transfer history buffer
                // and update the TransactionIndex with the actual buffer position
                if (_aftProvider.TransferAmount > 0 && _aftProvider.CurrentTransfer.TransactionIndex == 0)
                {
                    _aftProvider.CurrentTransfer.TransactionIndex = _historyBuffer.CurrentBufferIndex;
                }
            }

            _propertiesManager.SetProperty(SasProperties.AftTransferInterrogatePending, true);
            if (_aftProvider.CurrentTransfer != null)
            {
                _aftProvider.CurrentTransfer.Handlers = _handlers;
            }

            return _aftProvider.CurrentTransfer ??
                   new AftResponseData
                   {
                       TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable,
                       ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
                       Handlers = _nullHandlers
                   };
        }
    }
}