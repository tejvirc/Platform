namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Meter Collect Status Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming machine address
    /// Command          1           B6        Meter collect status
    /// Length           1           01        Total length of the bytes following, not including the CRC
    /// Status           1         00-FF       Current host status
    ///                                        00 = Acknowledge A0
    ///                                        01 = Ready (meters collected)
    ///                                        80 = Unable to collect meters at this time 
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Address          1         01-7F       Gaming machine address
    /// Command          1           B6        Meter collect status
    /// Length           1           01        Total length of the bytes following, not including the CRC
    /// Status           1         00-FF       00 = Meter clear/ add/remove/remap activity
    ///                                        01 = Enable/disable paytable/denom only
    ///                                        80 = Not in meter change pending state
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB6MeterCollectStatusParser : SasLongPollParser<MeterCollectStatusData, LongPollSingleValueData<byte>>
    {
        private const int AddressAndCommandLength = 2;
        private const int StatusOffset = 3;
        private const int CommandLength = 6;
        private static readonly List<byte> ValidHostStatus = new List<byte> { 0x00, 0x01, 0x80 };

        /// <summary>
        ///     Instantiates a new instance of the LPB6MeterCollectStatusParser class
        /// </summary>
        public LPB6MeterCollectStatusParser() : base(LongPoll.MeterCollectStatus)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            if (command.Count != CommandLength ||
                !ValidHostStatus.Contains(Data.Value = command.ElementAt(StatusOffset)))
            {
                return NackLongPoll(command);
            }

            var result = Handle(Data);

            if (result is null)
            {
                return NackLongPoll(command);
            }

            var response = command.Take(AddressAndCommandLength).ToList();
            response.Add(0x01);
            response.Add((byte)result.Status);

            return response;
        }
    }
}
