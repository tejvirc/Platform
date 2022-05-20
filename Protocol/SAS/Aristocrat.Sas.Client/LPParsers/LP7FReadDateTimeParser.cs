namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Receive Current Date And Time Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         00-7F       Gaming Machine Address
    /// Command          1           7F        Receive Current Date And Time
    /// Date           4 BCD        XXXX       Date in MMDDYYY format
    /// Time           3 BCD        XXX        Time in HHMMSS 24 hour format
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP7FReadDateTimeParser : SasLongPollParser<LongPollReadSingleValueResponse<bool>, LongPollDateTimeData>
    {
        private const int EntryLength = 1;
        private const int YearLength = 2;
        private const int MonthIndex = 2;
        private const int DayIndex = 3;
        private const int YearIndex = 4;
        private const int HourIndex = 6;
        private const int MinIndex = 7;
        private const int SecIndex = 8;

        /// <summary>
        ///     Instantiates a new instance of the LP7FReadDateTimeParser class
        /// </summary>
        public LP7FReadDateTimeParser() : base(LongPoll.ReceiveDateTime)
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
            bool valid;
            ulong month, day, year, hour, min, sec;

            var longPoll = command.ToArray();

            (month, valid) = Utilities.FromBcdWithValidation(longPoll, MonthIndex, EntryLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }
            (day, valid) = Utilities.FromBcdWithValidation(longPoll, DayIndex, EntryLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }
            (year, valid) = Utilities.FromBcdWithValidation(longPoll, YearIndex, YearLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }
            (hour, valid) = Utilities.FromBcdWithValidation(longPoll, HourIndex, EntryLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }
            (min, valid) = Utilities.FromBcdWithValidation(longPoll, MinIndex, EntryLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }
            (sec, valid) = Utilities.FromBcdWithValidation(longPoll, SecIndex, EntryLength);
            if (!valid)
            {
                return NackLongPoll(command);
            }

            try
            {
                Data.DateAndTime = new DateTime((int)year, (int)month, (int)day, (int)hour, (int)min, (int)sec);
            }
            catch (ArgumentOutOfRangeException)
            {
                return NackLongPoll(command);
            }

            return Handle(Data).Data ? AckLongPoll(command) : NackLongPoll(command);
        }
    }
}