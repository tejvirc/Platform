namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the ROM Signature Verification
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1          21         ROM Signature Verification
    /// Seed             2        0000-FFFF    ROM Verification Seed
    /// CRC              2        0000-FFFF    16-bit CRC
    ///
    /// This command gets ACKed and will then send the following response once the calculation is complete:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1          21         ROM Signature Verification
    /// Seed             2        0000-FFFF    ROM Signature
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP21RomSignatureVerificationParser : SasLongPollParser<LongPollResponse, RomSignatureData>
    {
        private const int SeedIndex = 2;
        private const int SeedLength = 2;

        /// <inheritdoc />
        public LP21RomSignatureVerificationParser(SasClientConfiguration configuration)
            : base(LongPoll.RomSignatureVerification)
        {
            Data.ClientNumber = configuration.ClientNumber;
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Data.Seed = (ushort) Utilities.FromBinary(command.ToArray(), SeedIndex, SeedLength);
            Handler(Data);

            // Just ack this command as we will send the results to the host once they are calculated as per Section 6.2
            return AckLongPoll(command);
        }
    }
}