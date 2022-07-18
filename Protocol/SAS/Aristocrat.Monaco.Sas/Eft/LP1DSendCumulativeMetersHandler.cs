namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.EFT;
    using Contracts.Eft;

    /// <summary>
    ///     Handler for LP 1D, send cumulative meters to host
    /// </summary>
    public class LP1DSendCumulativeMetersHandler : ISasLongPollHandler<CumulativeEftMeterData, LongPollData>
    {
        private readonly IEftTransferProvider _transferProvider;

        /// <summary>
        ///     Creates a new instance of the LP1DSendCumulativeMetersHandler class.
        /// </summary>
        public LP1DSendCumulativeMetersHandler(
            IEftTransferProvider transferProvider)
        {
            _transferProvider = transferProvider ?? throw new ArgumentNullException(nameof(transferProvider));
        }

        /// <inheritdoc cref="ISasLongPollHandler" />
        public List<LongPoll> Commands => new() { LongPoll.EftSendCumulativeMeters };

        /// <inheritdoc />
        public CumulativeEftMeterData Handle(LongPollData data)
        {
            return _transferProvider.QueryBalanceAmount();
        }
    }
}