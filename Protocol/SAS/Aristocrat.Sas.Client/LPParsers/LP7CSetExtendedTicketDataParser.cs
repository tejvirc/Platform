namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Set Extended Ticket Data Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         00-7F       Gaming Machine Address
    /// Command          1           7C        Set Extended Ticket Data
    /// Length           1         00-nn       Length of bytes following, not including the CRC
    /// Data code        1           nn        See Table 15.3c in the SAS Protocol Spec on page 15-8
    /// Data length      1           nn        Bit=1 to enable function, 0 to disable function if corresponding Control Mask bit is 1
    /// Data             x                     Data element from Table 15.3c
    /// optional more data elements .....
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           7C        Set Extended Ticket Data
    /// Status flag      1         00-01       00 = Flag false, 01 = Flag true
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP7CSetExtendedTicketDataParser :
        SasLongPollParser<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>
    {
        private const int TicketDataStartIndex = 3;
        private const int DataCodeIndex = 0;
        private const int DataLengthIndex = 1;
        private const int DataStartIndex = 2;
        private const int MinDataSize = 2;
        private const int DataCodeLengthSize = 1;
        private const int DataLengthFieldSize = 1;
        private const int MaxTicketTitleLength = 16;
        private const int MaxTicketDataLength = 40;

        /// <inheritdoc />
        public LP7CSetExtendedTicketDataParser()
            : base(LongPoll.SetExtendedTicketData)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var index = TicketDataStartIndex;
            var commandData = command.ToArray();
            var commandLength = commandData[SasConstants.SasLengthIndex] + index; // Get the command length and then add index

            if (commandLength != (command.Count - SasConstants.NumberOfCrcBytes))
            {
                // Just set a bad length as we need to let the handler know we failed
                // This should never happen as this length is used to know how many bytes to read
                commandLength = -1;
            }

            var data = new SetTicketData
            {
                BroadcastPoll = commandData.First() == SasConstants.BroadcastAddress,
                IsExtendTicketData = true
            };

            while (commandLength >= (index + MinDataSize))
            {
                var length = ParseExtendTicketCode(commandData, commandLength, index, data);
                if (length <= 0)
                {
                    data.ValidTicketData = false;
                    break;
                }

                index += length;
            }

            // Did we have enough bytes to handle this command correctly?
            if (commandLength != index && commandLength != 0)
            {
                data.ValidTicketData = false;
            }

            var result = Handler(data);
            Handlers = result.Handlers;
            var response = command.Take(SasConstants.MinimumBytesForLongPoll).ToList();
            response.Add((byte)result.Data);
            return response;
        }

        private static int ParseExtendTicketCode(byte[] commandData, int commandLength, int index, SetTicketData data)
        {
            var code = (ExtendTicketDataCode)commandData[index + DataCodeIndex];
            var length = (int)commandData[index + DataLengthIndex];

            if ((length + index + DataCodeLengthSize + DataLengthFieldSize) > commandLength || // Is there enough to read starting at the data index
                length > MaxTicketDataLength || // Max possible length is 40
                (code > ExtendTicketDataCode.Address2 && length > MaxTicketTitleLength)) // Ticket title can't exceed 16
            {
                return -1;
            }

            var text = Encoding.ASCII.GetString(commandData, index + DataStartIndex, length);
            switch (code)
            {
                case ExtendTicketDataCode.Location:
                    data.Location = text;
                    break;
                case ExtendTicketDataCode.Address1:
                    data.Address1 = text;
                    break;
                case ExtendTicketDataCode.Address2:
                    data.Address2 = text;
                    break;
                case ExtendTicketDataCode.RestrictedTicketTitle:
                    data.RestrictedTicketTitle = text;
                    break;
                case ExtendTicketDataCode.DebitTicketTitle:
                    data.DebitTicketTitle = text;
                    break;
                default:
                    return -1;
            }

            return length + MinDataSize;
        }
    }
}