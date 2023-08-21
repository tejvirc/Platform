namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    /// This handles the Enable/Disable Game N Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           09        Enable/Disable Game N
    /// Game Number    2 BCD      0001-9999    Game Number
    /// Enable/Disable   1          00-01      00 - Disable, 01 - Enable
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP09DisableEnableGameNParser : SasLongPollMultiDenomAwareParser<EnableDisableResponse, EnableDisableData>
    {
        private const int GameNumberIndex = 2;
        private const int GameNumberLength = 2;
        private const int EnableIndex = 4;
        private const byte GameEnabled = 1;

        /// <summary>
        /// Instantiates a new instance of the LP09DisableEnableGameNParser class
        /// </summary>
        public LP09DisableEnableGameNParser() : base(LongPoll.EnableDisableGameN)
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
            var longPoll = command.ToArray();
            var enable = longPoll[EnableIndex];

            // check for valid values for enable/disable
            if (enable > GameEnabled)
            {
                Logger.Debug($"Enable/Disable {enable:X2} out of range. NACKing game enable/disable long poll");
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            var (id, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberIndex, GameNumberLength);

            if (!valid)
            {
                Logger.Debug("Game Id not valid BCD. NACKing game enable/disable long poll");
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            Data.Enable = enable == GameEnabled;
            Data.Id = (int)id;
            Data.TargetDenomination = denom;
            Data.MultiDenomPoll = multiDenomPoll;

            var result = Handle(Data);

            if (result.Busy)
            {
                Logger.Debug(@"Can't enable/disable the game at this time");
                return BusyResponse(command);
            }

            if (multiDenomPoll && result.ErrorCode != MultiDenomAwareErrorCode.NoError)
            {
                Logger.Debug($"Handler returned multi-denom error {result.ErrorCode}");
                return GenerateMultiDenomAwareError(command.First(), result.ErrorCode);
            }

            if (!result.Succeeded)
            {
                Logger.Debug($"Game id {Data.Id} out of range. NACKing game enable/disable long poll");
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            Logger.Debug("ACKing game enable/disable long poll");
            return AckLongPoll(command);
        }
    }
}
