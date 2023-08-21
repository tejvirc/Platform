namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Handles read multiple meters
    /// </summary>
    public class ReadMultipleMetersHandler : ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        private readonly IMeterManager _meterManager;

        /// <summary>
        ///     Create an instance of ReadMultipleMetersHandler
        /// </summary>
        /// <param name="meterManager">A reference to the meter manager service</param>
        public ReadMultipleMetersHandler(IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.SendMeters10Thru15,
            LongPoll.SendMeters11Thru15,
            LongPoll.SendGamesSincePowerUpLastDoorMeter,
            LongPoll.SendBillCountMeters,
            LongPoll.SendMeters
        };

        /// <inheritdoc/>
        public LongPollReadMultipleMetersResponse Handle(LongPollReadMultipleMetersData data)
        {
            var response = new LongPollReadMultipleMetersResponse();
            var meters = response.Meters;
            meters.Clear();
            foreach (var meter in data.Meters)
            {
                meters[meter.Meter] = new LongPollReadMeterResponse(
                    meter.Meter,
                    (ulong)_meterManager.GetMeterValue(data.AccountingDenom, meter.Meter, meter.MeterType));
            }

            return response;
        }
    }
}