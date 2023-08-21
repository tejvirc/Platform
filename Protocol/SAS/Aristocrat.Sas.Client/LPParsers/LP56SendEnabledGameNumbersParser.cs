namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Enabled Game Numbers Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           56        Send Enabled Game Numbers
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           56        Send Enabled Game Numbers
    /// Length           1         01-FF       Number of bytes following, not including CRC
    /// # of games       1         00-7F       Number of games currently enabled
    /// Game number    2 BCD                   Game number for currently enabled game
    /// additional game numbers ...
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP56SendEnabledGameNumbersParser : SasLongPollMultiDenomAwareParser<SendEnabledGameNumbersResponse, LongPollMultiDenomAwareData>
    {
        private const int SizeOfLengthField = 1;
        private const int AddressLength = 1;
        private const int CommandLength = 1;

        /// <inheritdoc />
        public LP56SendEnabledGameNumbersParser()
            : base(LongPoll.SendEnabledGameNumbers)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command) => Parse(command, 0, false);

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom) => Parse(command, denom, true);

        /// <summary>
        ///     Handles the parsing of the long poll, being aware of multi-denom-awareness.
        /// </summary>
        /// <param name="command">Byte collection representing the long poll received</param>
        /// <param name="denom">Desired denomination represented in cents</param>
        /// <param name="multiDenomPoll">Whether or not to treat this as a multi-denom poll</param>
        /// <returns>Long poll response, or null if there is no response</returns>
        private IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom, bool multiDenomPoll)
        {
            var response = new List<byte>(command).Take(AddressLength + CommandLength).ToList();
            Data.TargetDenomination = denom;
            Data.MultiDenomPoll = multiDenomPoll;
            var result = Handle(Data);

            if (result.ErrorCode != MultiDenomAwareErrorCode.NoError)
            {
                response = GenerateMultiDenomAwareError(command.First(), result.ErrorCode).ToList();
            }
            else
            {
                var length = SizeOfLengthField + (result.EnabledGameIds.Count * SasConstants.Bcd4Digits);
                response.Add((byte)length);
                response.Add((byte)result.EnabledGameIds.Count);
                response.AddRange(
                    result.EnabledGameIds.SelectMany(id => Utilities.ToBcd((ulong)id, SasConstants.Bcd4Digits)));
            }

            return response;
        }
    }
}