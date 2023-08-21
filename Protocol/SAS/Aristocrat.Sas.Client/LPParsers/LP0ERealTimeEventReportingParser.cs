namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Enable/Disable Real Time Event Reporting Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           0E        Enable/Disable Real Time Event Reporting
    /// Enable/Disable   1         00-01       00 - Disable, 01 - Enable
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP0ERealTimeEventReportingParser : SasLongPollParser<LongPollResponse, EnableDisableData>
    {
        private const int EnableIndex = 2;
        private const byte RealTimeReportingEnabled = 1;
        private readonly ISasClient _client;

        /// <summary>
        ///     Instantiates a new instance of the LP0ERealTimeEventReportingParser class
        /// </summary>
        public LP0ERealTimeEventReportingParser(ISasClient client) : base(LongPoll.EnableDisableRealTimeEventReporting)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var enable = longPoll[EnableIndex];

            // check for valid values for enable/disable
            if (enable > RealTimeReportingEnabled)
            {
                Logger.Debug($"Enable/Disable {enable:X2} out of range. Only 0 or 1 are allowed. NACKing enable/disable real time event reporting long poll");
                return NackLongPoll(command);
            }

            var isEnabled = enable == RealTimeReportingEnabled;
            _client.IsRealTimeEventReportingActive = isEnabled;
            Data.Enable = isEnabled;
            Data.Id = _client.ClientNumber;

            Handle(Data);

            Logger.Debug("ACKing enable/disable real time event reporting long poll");
            return AckLongPoll(command);
        }
    }
}
