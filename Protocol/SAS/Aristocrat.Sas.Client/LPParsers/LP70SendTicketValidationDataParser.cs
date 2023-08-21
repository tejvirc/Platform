namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Ticket Validation Data Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           70        Send Ticket Validation Data
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           70        Send Ticket Validation Data
    /// Length           1         04-27       number of bytes following not including the CRC
    /// Ticket Status    1         00,FF       00=Ticket in escrow, data follows, FF=no ticket in escrow
    /// Ticket Amt     5 BCD                   The ticket amount in cents. All zeros if no amount available
    /// Parsing code     1          00         The validation number is 9 BCD/18 digit decimal code. The first two digits are
    ///                                          a 2-digit system ID code indicating how to interpret the following 16 digits.
    ///                                          ID code 00 indicates that the following 16 digits represent a SAS secure enhanced
    ///                                          validation number. Other system ID codes and parsing codes will be
    ///                                          assigned by IGT as needed.
    /// Validation data  x       ???           Up to 32 bytes of validation data. (normally it will be 18 bytes)
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP70SendTicketValidationDataParser : SasLongPollParser<SendTicketValidationDataResponse, LongPollData>
    {
        private const int DataStartIndex = 2;
        private const int BarcodeSize = 9;

        /// <inheritdoc />
        public LP70SendTicketValidationDataParser()
            : base(LongPoll.SendTicketValidationData)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handler(Data);
            if (response == null)
            {
                // not supporting it; shouldn't send anything and let Host ignore it in 20 ms.
                return null;
            }

            var result = command.Take(DataStartIndex).ToList();
            var responseData = new List<byte>();
            var pending = !string.IsNullOrEmpty(response.Barcode);

            // ticket status. 0x00 - ticket in escrow; 0xFF - no ticket in escrow
            responseData.Add(pending ? (byte)0x00 : (byte)0xFF);
            if (pending)
            {
                // ticket amount. always 0.
                responseData.AddRange(new byte[] { 0, 0, 0, 0, 0});

                // parsing code.
                responseData.Add((byte)response.ParsingCode);

                // parsing validation data.
                responseData.AddRange(Utilities.AsciiStringToBcd(response.Barcode, true, BarcodeSize));
            }

            result.Add((byte)responseData.Count);
            result.AddRange(responseData);

            return result;
        }
    }
}
