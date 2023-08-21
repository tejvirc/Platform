namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using log4net;

    /// <summary>
    ///     Handles transferring from the host to a ticket
    /// </summary>
    public class AftTransferInHouseFromHostToTicket : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAftTransferProvider _aftProvider;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferInHouseFromHostToTicket class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        public AftTransferInHouseFromHostToTicket(IAftTransferProvider aftProvider)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _errorConditions = InitializeErrorConditions();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;
            _aftProvider.CurrentTransfer.ReceiptStatus = (byte)ReceiptStatus.ReceiptPending;

            _aftProvider.CheckForErrorConditions(_errorConditions);
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
            }
            else
            {
                Logger.Debug("Starting transfer from host to ticket as a task");
                _aftProvider.DoAftToTicket().FireAndForget();
            }

            return _aftProvider.CurrentTransfer;
        }

        /// <summary>
        ///     create a list of error conditions to check.
        /// </summary>
        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                {   // can't print ticket if no printer available
                    () => !_aftProvider.IsPrinterAvailable,
                    (AftTransferStatusCode.TransferToTicketDeviceNotAvailable, "printer not available to print ticket")
                },
                {   // remove this condition when we support host to ticket transfers
                    () => true,
                    (AftTransferStatusCode.NotAValidTransferFunction, "host to ticket not supported")
                },
            };
        }
    }
}