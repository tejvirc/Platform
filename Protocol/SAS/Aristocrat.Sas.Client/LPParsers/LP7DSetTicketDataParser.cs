namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Set Ticket Data Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         00-7F       Gaming Machine Address
    /// Command          1          7D         Set Ticket Data
    /// Length           1        00,02-7E     Length of bytes following, not including the CRC
    /// Host ID          2        0000-FFFF    Host identification number
    /// Expiration       1         00-FF       Number of days before ticket expires (00=never expires)
    /// Location Length  1         00-28       Length of location name data (00=don't change from existing)
    /// Location data    x                     Location ASCII data (0-40 bytes)
    /// Addr 1 Length    1         00-28       Length of address 1 data (00=don't change from existing)
    /// Addr 1 data      x                     Address 1 ASCII data (0-40 bytes)
    /// Addr 2 Length    1         00-28       Length of address 2 data (00=don't change from existing)
    /// Addr 2 data      x                     Address 2 ASCII data (0-40 bytes)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1          7D         Set Ticket Data
    /// Status           1         00-01       00=Flag currently false, 01=Flag currently true
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP7DSetTicketDataParser : SasLongPollParser<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>
    {
        private const int MaxTicketDataSize = 40;
        private const int HostIdIndex = 3;
        private const int HostIdLength = 2;
        private const int ExpirationIndex = HostIdIndex + HostIdLength;
        private const int ExpirationLength = 1;
        private const int TicketDataStartIndex = ExpirationIndex + ExpirationLength;
        private const int LengthSize = 1;

        /// <inheritdoc />
        public LP7DSetTicketDataParser()
            : base(LongPoll.SetTicketData)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var commandData = command.ToArray();
            var data = new SetTicketData
            {
                BroadcastPoll = (commandData[0] == SasConstants.BroadcastAddress),
                IsExtendTicketData = false
            };

            var commandLength = commandData[SasConstants.SasLengthIndex] + SasConstants.SasLengthIndex + 1;
            if (commandLength != (command.Count - SasConstants.NumberOfCrcBytes))
            {
                data.ValidTicketData = false;
            }

            ParseHostId(data, commandData, commandLength);
            ParseExpirationDate(data, commandData, commandLength);
            if (commandLength >= TicketDataStartIndex)
            {
                // We have enough bytes to start reading at least one ticket data text
                int length;
                var index = TicketDataStartIndex;
                (length, data.Location) = ParseTicketText(data, commandData, index, commandLength);
                index += length;
                (length, data.Address1) = ParseTicketText(data, commandData, index, commandLength);
                index += length;
                (length, data.Address2) = ParseTicketText(data, commandData, index, commandLength);
                index += length;

                if (index != commandLength)
                {
                    // We have extra data which is incorrect
                    data.ValidTicketData = false;
                }
            }

            var result = Handle(data);
            Handlers = result.Handlers;
            var response = commandData.Take(SasConstants.MinimumBytesForLongPoll).ToList();
            response.Add((byte)result.Data);
            return response;
        }

        private static void ParseExpirationDate(SetTicketData data, byte[] commandData, int commandLength)
        {
            if (commandLength < (ExpirationIndex + ExpirationLength))
            {
                // We either have an invalid pack that is set or only the host ID is provided
                return;
            }

            data.ExpirationDate = (int)Utilities.FromBinary(commandData, ExpirationIndex, ExpirationLength);
            data.SetExpirationDate = true;
        }

        private static void ParseHostId(SetTicketData data, byte[] commandData, int commandLength)
        {
            if (commandLength > HostIdIndex && commandLength < (HostIdLength + HostIdIndex))
            {
                data.ValidTicketData = false;
            }
            else if (commandLength >= (HostIdLength + HostIdIndex))
            {
                data.HostId = (int)Utilities.FromBinary(commandData, HostIdIndex, HostIdLength);
            }
        }

        private static (int length, string data) ParseTicketText(
            SetTicketData data,
            byte[] commandData,
            int index,
            int commandLength)
        {
            if (commandLength < (index + LengthSize))
            {
                // We may not have any ticket data to read
                return (0, null);
            }

            var length = Utilities.FromBinary(commandData, (uint)index, LengthSize);
            if (length == 0)
            {
                // We don't want to reset any data so set this to null
                return (LengthSize, null);
            }

            if (commandLength >= (index + LengthSize + length) && length <= MaxTicketDataSize)
            {
                return (LengthSize + (int)length,
                    Encoding.ASCII.GetString(commandData, index + LengthSize, (int)length));
            }

            // We have an invalid length so just fail the data
            data.ValidTicketData = false;
            return (LengthSize + (int)length, string.Empty);
        }
    }
}