namespace Aristocrat.Sas.Client.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Set AFT Receipt Data Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         00-7F       Gaming Machine Address
    /// Command          1           75        Set AFT receipt data command
    /// Length           1         02-nn       Number of bytes following, not including the CRC
    /// Data Code        1           nn        Code indicates data element type following (see Table 8.11.1b)
    /// Data          x bytes       ???        Data element (see Table 8.11.1b)
    /// ...           variable      ...        Additional data code/length/data elements
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Aft)]
    public class LP75SetAftReceiptDataParser : SasLongPollParser<LongPollResponse, SetAftReceiptData>
    {
        private const int TicketDataStartIndex = 3;
        private const int DataCodeIndex = 0;
        private const int DataLengthIndex = 1;
        private const int DataStartIndex = 2;
        private const int MinInputLength = 2;
        private const int MinDataSize = 2;
        private const int DataCodeFieldSize = 1;
        private const int DataLengthFieldSize = 1;
        private const int MaxReceiptDataLength = 22;

        /// <summary>
        ///     Instantiates a new instance of the LP7FReadDateTimeParser class
        /// </summary>
        public LP75SetAftReceiptDataParser() : base(LongPoll.SetAftReceiptData)
        {
        }

        /// <inheritdoc/>
        /// <summary>
        ///     Parse the command and build an ack or nack if successful. Places the BCD downloaded
        ///     date time into the Data structure. 
        /// </summary>
        /// <param name="command">input command array</param>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var currentIndex = TicketDataStartIndex;
            var commandData = command.ToArray();
            var inputLength = commandData[SasConstants.SasLengthIndex];
            var commandLength = inputLength + currentIndex; // Get the command length and then add currentIndex

            if (inputLength < MinInputLength)
            {
                return NackLongPoll(command);
            }

            if (commandLength != (command.Count - SasConstants.NumberOfCrcBytes))
            {
                return NackLongPoll(command);
            }

            var data = new SetAftReceiptData();

            while (commandLength >= (currentIndex + MinDataSize))
            {
                var length = ParseReceiptData(commandData, commandLength, currentIndex, ref data);
                if (length <= 0)
                {
                    return NackLongPoll(command);
                }

                currentIndex += length;
            }

            Handle(data);

            return AckLongPoll(command);
        }

        private static int ParseReceiptData(byte[] commandData, int commandLength, int dataIndex, ref SetAftReceiptData data)
        {
            var code = (TransactionReceiptDataField)commandData[dataIndex + DataCodeIndex];
            var dataLength = (int)commandData[dataIndex + DataLengthIndex];

            // Is there enough to read starting at the dataIndex and are the length and code ok
            if ((dataLength + dataIndex + DataCodeFieldSize + DataLengthFieldSize) > commandLength ||
                dataLength > MaxReceiptDataLength ||
                !Enum.IsDefined(typeof(TransactionReceiptDataField), code))
            {
                return -1;
            }

            var text = Encoding.ASCII.GetString(commandData, dataIndex + DataStartIndex, dataLength);

            data.TransactionReceiptValues[code] = dataLength == 0 ? SasConstants.UseDefault : text;

            return dataLength + MinDataSize;
        }
    }
}