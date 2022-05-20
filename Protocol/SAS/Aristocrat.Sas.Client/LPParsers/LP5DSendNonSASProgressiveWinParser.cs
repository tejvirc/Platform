namespace Aristocrat.Sas.Client.LPParsers
{
    using LongPollDataClasses;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     This handles the Send Non SAS Progressive win Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field       Bytes       Value       Description
    /// Address       1         01-7F       Gaming Machine Address
    /// Command       1           5D        Send Non SAS Progressive win
    /// 
    /// Response
    /// Field     Bytes     Value                Description
    /// Address     1       01-7F                   Gaming Machine Address
    /// Command     1        5D                     Send Non SAS Progressive win
    /// Length      1       01-B1                   Number of bytes following, not including CRC
    /// Number      1       00-08                   Number of levels following (00 if queue empty)
    /// of levels
    /// Controller  1       01-1F                   Progressive Controller type
    /// type
    /// Controller
    /// ID        2 BCD     0000-9999               Controller identifier  
    /// Level       1       00, 01-20               Progressive level
    /// Amount    5 BCD     00000000000-9999999999  Win amount, or non-base portion of win when 
    /// (or increment)                              base amount field is non-zero, in units of cents
    /// Base      5 BCD     00000000000-9999999999  Base portion of this progressive win, if available,
    /// Amount                                      in units of cents
    /// Escrow    5 BCD     00000000000-9999999999  Any progressive increment that was escrowed at 
    /// Amount                                      the time of the hit (not paid), if available, in units of cents
    /// ...     Variable    ...                     Additional controller/level/amount/base/escrow data sets
    /// CRC         2       0000-FFFF               16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP5DSendNonSASProgressiveWinParser : SasLongPollParser<SendNonSASProgressiveWinResponse, NonSasProgressiveWinHostData>
    {
        private const byte Length = 1;
        private const int DataStartIndex = 2;

        /// <inheritdoc />
        public LP5DSendNonSASProgressiveWinParser(SasClientConfiguration configuration)
            : base(LongPoll.SendNonSasProgressiveWinData)
        {
            Data.ClientNumber = configuration.ClientNumber;
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handle(Data);

            var result = command.Take(DataStartIndex).ToList();
            var responsedata = new List<byte>();
              
            foreach (var data in response.ProgressivesWon)
            {
                responsedata.Add((byte)data.ControllerType);
                responsedata.AddRange(Utilities.ToBcd((ulong)data.ControllerID, SasConstants.Bcd4Digits));
                responsedata.Add((byte)data.Level);
                responsedata.AddRange(Utilities.ToBcd((ulong)data.Amount, SasConstants.Bcd10Digits));
                responsedata.AddRange(Utilities.ToBcd((ulong)data.BaseAmount, SasConstants.Bcd10Digits));
                responsedata.AddRange(Utilities.ToBcd((ulong)data.EscrowAmount, SasConstants.Bcd10Digits));
            }

            Handlers = response.Handlers;

            if(response.ProgressivesWon.Count != 0)
            {
                result.Add((byte)responsedata.Count);
            }
            else
            {
                result.Add(Length);
            }
            
            result.Add((byte)response.ProgressivesWon.Count);
            result.AddRange(responsedata);

            return result;
        }
    }
}