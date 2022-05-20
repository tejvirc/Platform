namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Progressive;

    /// <summary>
    ///     The handler for LP 5D SendNonSASProgressiveWinHandler
    /// </summary>
    public class LP5DSendNonSASProgressiveWinHandler : ISasLongPollHandler<SendNonSASProgressiveWinResponse,
        NonSasProgressiveWinHostData>
    {
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;
        private readonly IProgressiveHitExceptionProvider _hitExceptionProvider;

        /// <summary>
        ///     Creates an Instance of the LP5DSendNonSASProgressiveWinHandler
        /// </summary>
        /// <param name="progressiveWinDetailsProvider">The game provider</param>
        /// <param name="hitExceptionProvider">The hit Exception Provider</param>
        public LP5DSendNonSASProgressiveWinHandler(
            IProgressiveWinDetailsProvider progressiveWinDetailsProvider,
            IProgressiveHitExceptionProvider hitExceptionProvider)
        {
            _progressiveWinDetailsProvider = progressiveWinDetailsProvider ??
                                             throw new ArgumentNullException(nameof(progressiveWinDetailsProvider));
            _hitExceptionProvider =
                hitExceptionProvider ?? throw new ArgumentNullException(nameof(hitExceptionProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendNonSasProgressiveWinData };

        /// <inheritdoc />
        public SendNonSASProgressiveWinResponse Handle(NonSasProgressiveWinHostData data)
        {
            var response =
                new SendNonSASProgressiveWinResponse(_progressiveWinDetailsProvider.GetNonSasProgressiveWinData(data.ClientNumber));

            if (response.NumberOfLevels > 0)
            {
                response.Handlers = new HostAcknowledgementHandler
                {
                    ImpliedAckHandler = () => Task.Run(() => NonSasProgressiveHitDataAcknowledged(data.ClientNumber))
                };
            }

            return response;
        }

        private void NonSasProgressiveHitDataAcknowledged(byte clientNumber)
        {
            _progressiveWinDetailsProvider.HandleNonSasProgressiveWinDataAcknowledged(clientNumber);

            if (_progressiveWinDetailsProvider.HasNonSasProgressiveWinData(clientNumber))
            {
                _hitExceptionProvider.ReportNonSasProgressiveHit();
            }
        }
    }
}