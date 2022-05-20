namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Multiple SAS Progressive Win Amounts
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           87        Send Multiple SAS Progressive Win Amounts
    ///
    /// Response
    /// Field          Bytes           Value             Description
    /// Address          1             01-7F             Gaming Machine Address
    /// Command          1               87              Send Multiple SAS Progressive Win Amounts
    /// Length           1             02-C2             Number of bytes following,not including CRC
    /// Group            1             01-FF             Group ID of the progressive
    /// Number of Levels 1             00-20             Number of levels following(00 if queue empty)
    /// Level            1             01-20             Progressive level of first entry
    /// Amount         5 BCD  00000000000-9999999999     Win amount of first entry in units of cents
    ///               variable          ...              Additional level/amount data sets
    /// CRC              2           0000-FFFF           16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Progressives)]
    public class LP87SendMultipleSasProgressiveWinAmountsParser : SasLongPollParser<SendMultipleSasProgressiveWinAmountsResponse, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP87SendMultipleSasProgressiveWinAmountsParser class
        /// </summary>
        public LP87SendMultipleSasProgressiveWinAmountsParser() : base(LongPoll.SendMultipleSasProgressiveWinAmounts)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll(87) Send Multiple SAS Progressive Win Amounts.");
            var response = new List<byte>(command.Take(2).ToList());

            var result = Handle(Data);
            Handlers = result.Handlers;

            var levelData= GetLevelData(result);
            response.Add((byte)levelData.Count);
            response.AddRange(levelData);

            return response;
        }

        private static IReadOnlyCollection<byte> GetLevelData(SendMultipleSasProgressiveWinAmountsResponse result)
        {
            var collection = new List<byte>
            {
                (byte)result.GroupId,
                (byte)result.ProgressivesWon.Count
            };

            foreach (var data in result.ProgressivesWon)
            {
                collection.Add((byte)data.LevelId);
                collection.AddRange(Utilities.ToBcd((ulong)data.Amount, SasConstants.Bcd10Digits));
            }

            return collection;
        }
    }
}
