namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Set Secure Enhanced Validation ID Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4C        Set Secure Enhanced validation ID
    /// Machine ID       3      000000-FFFFFF  Gaming machine validation ID number
    /// Sequence number  3      000000-FFFFFF  Starting sequence number (incremented before being assigned to each event)
    /// CRC              2        0000-FFFF    16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           4C        Set Secure Enhanced validation ID
    /// Machine ID       3      000000-FFFFFF  Gaming machine validation ID number
    /// Sequence number  3      000000-FFFFFF  Starting sequence number (incremented before being assigned to each event)
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP4CSetValidationIdNumberParser : SasLongPollParser<LongPoll4CResponse, LongPoll4CData>
    {
        private const int MachineIdOffset = 2;
        private const int SequenceOffset = 5;

        /// <summary>
        /// Instantiates a new instance of the LP4CSetValidationIdNumberParser class
        /// </summary>
        public LP4CSetValidationIdNumberParser() : base(LongPoll.SetSecureEnhancedValidationId)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LP4C handler");
            // For a gaming machine to perform secure enhanced ticket / receipt / hand pay validation, the host must use
            // this long poll to set a gaming machine’s validation ID number and initial validation sequence number.
            // The host may also use this long poll to retrieve the current gaming machine validation ID and
            // validation sequence number by issuing the 4C command with a gaming machine validation ID of zero.
            // If a gaming machine is not configured to perform secure enhanced validation it ignores this long poll.
            //
            // If the host re-sends the exact same gaming machine validation ID and sequence number that it most
            // recently previously sent, and the gaming machine has since incremented the sequence number, the
            // gaming machine must not reset the sequence number to the value sent but continue to use the current
            // incremented value.
            var longPoll = command.ToArray();
            Data.MachineValidationId = Utilities.FromBinary(longPoll, MachineIdOffset, 3);
            Data.SequenceNumber = Utilities.FromBinary(longPoll, SequenceOffset, 3);
            var result = Handle(Data);

            // ignore this long poll if we're not doing secure enhanced validation
            if (result is null || !result.UsingSecureEnhancedValidation)
            {
                Logger.Debug("LP4C handler returning null");
                return null;
            }

            var response = new List<byte>();
            response.AddRange(new [] { longPoll[SasConstants.SasAddressIndex], longPoll[SasConstants.SasCommandIndex] });
            response.AddRange(Utilities.ToBinary(result.MachineValidationId, 3));
            response.AddRange(Utilities.ToBinary(result.SequenceNumber, 3));
            return response;
        }
   }
}
