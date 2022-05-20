namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Handles sending hand paid canceled credits command
    /// </summary>
    public class LP2DSendTotalHandPaidCanceledCreditsHandler :
        ISasLongPollHandler<SendTotalHandPaidCanceledCreditsDataResponse, SendTotalHandPaidCanceledCreditsData>
    {
        private const int AllGameId = 0;
        private readonly IMeterManager _meterManager;

        /// <summary>
        ///     Create an instance of LP2DLP2DSendTotalHandPaidCanceledCreditsHandler
        /// </summary>
        /// <param name="meterManager">A reference to the meter manager service</param>
        public LP2DSendTotalHandPaidCanceledCreditsHandler(IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.SendTotalHandPaidCanceledCredits
        };

        /// <inheritdoc/>
        public SendTotalHandPaidCanceledCreditsDataResponse Handle(SendTotalHandPaidCanceledCreditsData data)
        {
            if (data.GameId != AllGameId)
            {
                return new SendTotalHandPaidCanceledCreditsDataResponse
                {
                    Succeeded = false
                };
            }

            return new SendTotalHandPaidCanceledCreditsDataResponse
            {
                Succeeded = true,
                MeterValue = (ulong)_meterManager.GetMeterValue(data.AccountingDenom, SasMeters.TotalHandPaidCanceledCredits, MeterType.Lifetime)
            };
        }
    }
}
