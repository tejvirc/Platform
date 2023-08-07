namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using log4net;
    using SimpleInjector;

    /// <summary>
    ///     Handles fund transfers
    /// </summary>
    public class LP72AftTransferFundsHandler : ISasLongPollHandler<AftResponseData, AftTransferData>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private AftResponseData _aftResponseData = new AftResponseData();
        private readonly IAftRequestProcessorTransferCode _aftTransferFullPartial;
        private readonly IAftRequestProcessorTransferCode _aftInterrogate;
        private readonly IAftRequestProcessorTransferCode _aftInterrogateStatusOnly;
        private readonly IAftTransferProvider _aftProvider;

        private Dictionary<AftTransferCode, Func<AftResponseData, AftResponseData>> _handlers;
        /// <summary>
        ///     Creates a new instance of the LP72AftTransferFundsHandler
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="aftRequestProcessors">reference to the list of request processors</param>
        public LP72AftTransferFundsHandler(
            IAftTransferProvider aftProvider,
            IEnumerable<IAftRequestProcessorTransferCode> aftRequestProcessors)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            var requestProcessors = aftRequestProcessors?.ToList() ?? throw new ArgumentNullException(nameof(aftRequestProcessors));
            _aftTransferFullPartial = GetTypeFromInjector(requestProcessors, typeof(AftTransferFullPartial));
            _aftInterrogate = GetTypeFromInjector(requestProcessors, typeof(AftInterrogate));
            _aftInterrogateStatusOnly = GetTypeFromInjector(requestProcessors, typeof(AftInterrogateStatusOnly));

            InitializeHandlers();
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.AftTransferFunds };

        /// <inheritdoc/>
        public AftResponseData Handle(AftTransferData data)
        {
            // if the backend sends the exact same aft request again, it means we took to long to
            // respond to the last command so it is resending it. Convert this into an
            // Interrogate status only command since we already started processing the
            // prior transfer funds command
            if (_aftProvider.CurrentTransfer != null && data.ToAftResponseData().IsDuplicate(_aftProvider.CurrentTransfer))
            {
                data.TransferCode = AftTransferCode.InterrogationRequestStatusOnly;
            }

            _aftResponseData = data.ToAftResponseData();

            // per SAS spec page 8-17 paragraph 4 - If the backend sends any request other
            // than Interrogate or Cancel while a request is in progress,
            // respond with C0, NotCompatibleWithCurrentTransfer 
            if (!_aftProvider.IsTransferAcknowledgedByHost && _aftProvider.TransferFundsRequest(data))
            {
                Logger.Error("Attempting to send an Aft transfer when one is already in progress");
                _aftResponseData.TransferStatus = AftTransferStatusCode.NotCompatibleWithCurrentTransfer;
                _aftResponseData.TransactionIndex = 0;
                return _aftResponseData;
            }

            if (!_handlers.TryGetValue(_aftResponseData.TransferCode, out var handler))
            {
                _aftResponseData.TransferStatus = AftTransferStatusCode.UnsupportedTransferCode;
                _aftResponseData.ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested;
                return _aftResponseData;
            }

            _aftResponseData = handler(_aftResponseData);

            if (_aftProvider.TransferFundsRequest(data))
            {
                _aftProvider.UpdateHostCashoutFlags(data);
            }

            return _aftResponseData;
        }

        private void InitializeHandlers()
        {
            _handlers = new Dictionary<AftTransferCode, Func<AftResponseData, AftResponseData>>
            {
                // we don't support cancel transfer requests, so just do an interrogate status instead
                { AftTransferCode.CancelTransferRequest, data => _aftInterrogateStatusOnly.Process(data) },
                { AftTransferCode.TransferRequestFullTransferOnly, data => _aftTransferFullPartial.Process(data) },
                { AftTransferCode.TransferRequestPartialTransferAllowed, data => _aftTransferFullPartial.Process(data) },
                { AftTransferCode.InterrogationRequest, data => _aftInterrogate.Process(data) },
                { AftTransferCode.InterrogationRequestStatusOnly, data => _aftInterrogateStatusOnly.Process(data) },
            };
        }

        /// <summary>
        ///     Gets a specific type that implements IAftRequestProcessorTransferCode from the list of
        ///     objects that the dependency injection framework gives us
        /// </summary>
        /// <param name="requestProcessors">The list of types from the DI framework</param>
        /// <param name="type">The specific type that implements the interface</param>
        /// <returns>The requested type</returns>
        /// <exception cref="ArgumentException">Thrown if the requested type is not in the list</exception>
        private IAftRequestProcessorTransferCode GetTypeFromInjector(List<IAftRequestProcessorTransferCode> requestProcessors, Type type)
        {
            return requestProcessors.SingleOrDefault(type.IsInstanceOfType) ??
                   throw new ArgumentException($"{type.ToFriendlyName()} not in aftRequestProcessors");
        }
    }
}