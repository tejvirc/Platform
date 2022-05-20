namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;

    /// <summary>
    ///     Handles the meter collect status command
    /// </summary>
    public class LPB6MeterCollectStatusHandler : ISasLongPollHandler<MeterCollectStatusData, LongPollSingleValueData<byte>>
    {
        private readonly ISasMeterChangeHandler _meterChangeHandler;

        private enum HostStatus
        {
            Acknowledge = 0x00,
            Ready = 0x01,
            UnableToCollect = 0x80
        };

        /// <summary>
        ///     Creates the LPB6MeterCollectStatusHandler instance
        /// </summary>
        public LPB6MeterCollectStatusHandler(ISasMeterChangeHandler meterChangeHandler)
        {
            _meterChangeHandler = meterChangeHandler ?? throw new ArgumentNullException(nameof(ISasMeterChangeHandler));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.MeterCollectStatus };

        /// <inheritdoc/>
        public MeterCollectStatusData Handle(LongPollSingleValueData<byte> data)
        {
            var meterChangeStatus = _meterChangeHandler.MeterChangeStatus;

            if (HandleHostResponse((HostStatus)data.Value))
            {
                return new MeterCollectStatusData(meterChangeStatus);
            }

            return null;
        }

        private bool HandleHostResponse(HostStatus status)
        {
            switch (status)
            {
                case HostStatus.Acknowledge:
                    _meterChangeHandler.AcknowledgePendingChange();
                    break;
                case HostStatus.Ready:
                    _meterChangeHandler.ReadyForPendingChange();
                    break;
                case HostStatus.UnableToCollect:
                    _meterChangeHandler.CancelPendingChange();
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
