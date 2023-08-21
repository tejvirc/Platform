namespace Aristocrat.Sas.Client.LPParsers
{
    using LongPollDataClasses;
    using System.Collections.Generic;

    /// <summary>
    ///     This handles the Send sas Progressive win amount Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field       Bytes       Value       Description
    /// Address       1         01-7F       Gaming Machine Address
    /// Command       1           85        Send sas progressive win amount
    /// 
    /// Response
    /// Field     Bytes     Value                   Description
    /// Address     1       01-7F                   Gaming Machine Address
    /// Command     1        85                     Send sas progressive win amount
    /// Group       1       00-FF                   Group ID of the progressive
    /// Level       1       01-20                   Progressive level
    /// Amount    5 BCD     00000000000-9999999999  Win amount in units of cents
    /// CRC         2       0000-FFFF               16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Progressives)]
    public class LP85SendSasProgressiveWinAmountParser : SasLongPollParser<SendProgressiveWinAmountResponse, LongPollData>
    {
        private const int FieldLength = 1;

        /// <inheritdoc />
        public LP85SendSasProgressiveWinAmountParser()
            : base(LongPoll.SendSasProgressiveWinAmount)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll send GroupId, LevelID and WinAmount");

            var result = Handle(Data);
            Handlers = result.Handlers;

            var response = new List<byte>(command);
            response.AddRange(Utilities.ToBinary((uint)result.GroupId, FieldLength));
            response.AddRange(Utilities.ToBinary((uint)result.LevelId, FieldLength));
            response.AddRange(Utilities.ToBcd((ulong)result.WinAmount, SasConstants.Bcd10Digits));
            return response;
        }
    }
}