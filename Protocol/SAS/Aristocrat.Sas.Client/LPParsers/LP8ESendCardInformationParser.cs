namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Card Information Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           8E        Send Card Information
    ///
    /// Response
    /// Field          Bytes       Value         Description
    /// Address          1         01-7F         Gaming Machine Address
    /// Command          1           8E          Send Card Information
    /// Hand type        1         00-01         00 - Dealt hand, 01 - Final hand
    /// Hand             5 0000000000-5E5E5E5E5E Card data with the left most card sent first
    /// CRC              2       0000-FFFF       16-bit CRC
    ///
    /// Card Codes
    /// Upper  Definition  Lower  Definition
    /// Nibble             Nibble
    /// 0      Spades      0      Two
    /// 1      Clubs       1      Three
    /// 2      Hearts      2      Four
    /// 3      Diamonds    3      Five
    /// 4      Joker       4      Six
    /// 5      Other       5      Seven
    ///                    6      Eight
    ///                    7      Nine
    ///                    8      Ten
    ///                    9      Jack
    ///                    A      Queen
    ///                    B      King
    ///                    C      Ace
    ///                    D      Joker
    ///                    E      Other
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP8ESendCardInformationParser : SasLongPollParser<SendCardInformationResponse, LongPollData>
    {
        /// <inheritdoc />
        public LP8ESendCardInformationParser()
            : base(LongPoll.SendCardInformation)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handle(Data);

            result.Add(response.FinalHand ? (byte)1 : (byte)0);

            // On gaming machines with multiple hands or more than five card positions, only the
            // base hand or first five card positions can be reported.
            result.Add((byte)response.Card1);
            result.Add((byte)response.Card2);
            result.Add((byte)response.Card3);
            result.Add((byte)response.Card4);
            result.Add((byte)response.Card5);

            return result;
        }
    }
}