namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Last Accepted Bill Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           48        Send Last Accepted Bill Information
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           48        Send Last Accepted Bill Information
    /// Country code   1 BCD       00-38       Country code from table C-6 in Appendix C of the SAS Protocol Specification
    /// Denom code     1 BCD       00-19       Bill denomination code from Table C-6 in Appendix C
    /// Bill Meter     4 BCD 00000000-99999999 Number of accepted bills of this type
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP48SendLastAcceptedBillInformationParser : SasLongPollParser<SendLastAcceptedBillInformationResponse, LongPollData>
    {
        /// <inheritdoc />
        public LP48SendLastAcceptedBillInformationParser()
            : base(LongPoll.SendLastBillInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = new List<byte>(command);
            var result = Handle(Data);

            response.AddRange( Utilities.ToBcd((uint)result.CountryCode, SasConstants.Bcd2Digits) );
            response.AddRange( Utilities.ToBcd((uint)result.DenominationCode, SasConstants.Bcd2Digits) );
            response.AddRange( Utilities.ToBcd(result.Count, SasConstants.Bcd8Digits) );

            return response;
        }
    }
}