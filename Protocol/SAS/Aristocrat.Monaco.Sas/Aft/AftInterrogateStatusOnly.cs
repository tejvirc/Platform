namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Reflection;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using log4net;

    /// <summary>
    ///     Handles status only interrogate
    /// </summary>
    public class AftInterrogateStatusOnly : IAftRequestProcessorTransferCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAftTransferProvider _aftProvider;
        private readonly IAftHistoryBuffer _historyBuffer;

        /// <summary>
        ///     Instantiates a new instance of the AftInterrogateStatusOnly class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="historyBuffer">reference to the AftHistoryBuffer class</param>
        public AftInterrogateStatusOnly(IAftTransferProvider aftProvider, IAftHistoryBuffer historyBuffer)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _historyBuffer = historyBuffer ?? throw new ArgumentNullException(nameof(historyBuffer));
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug("Aft Interrogate Status only");

            // asking for a transaction from the history buffer
            if (data.TransactionIndex != 0)
            {
                return _historyBuffer.GetHistoryEntry(data.TransactionIndex);
            }

            return _aftProvider.CurrentTransfer;
        }
    }
}