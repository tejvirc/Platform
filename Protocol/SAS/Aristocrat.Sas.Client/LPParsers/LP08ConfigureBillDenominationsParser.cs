namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Configure Bill Denominations Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1          08         Configure Bill Denominations Mode
    /// Bill             4         ????        Bill denominations send LSB first
    /// Denominations                          (0=Disable, 1=Enable)
    ///                                        Bit  LSB   2nd Byte  3rd Byte   MSB
    ///                                         0    $1     $200    $20,000    TBD
    ///                                         1    $2     $250    $25,000    TBD
    ///                                         2    $5     $500    $50,000    TBD
    ///                                         3    $10    $1,000  $100,000   TBD
    ///                                         4    $20    $2,000  $200,000   TBD
    ///                                         5    $25    $2,500  $250,000   TBD
    ///                                         6    $50    $5,000  $500,000   TBD
    ///                                         7    $100   $10,000 $1,000,000 TBD
    /// Bill Acceptor    1          00-01      Action of bill acceptor after accepting a bill
    /// Action Flag                            Bit    Description
    ///                                         0      0 = disable bill acceptor after each accepted bill
    ///                                                1 = keep bill acceptor enabled after each accepted bill
    /// 
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP08ConfigureBillDenominationsParser : SasLongPollParser<LongPollResponse, LongPollBillDenominationsData>
    {
        private const int DisableAfterAcceptOffset = 6;
        private const int Denominations1Offset = 2;
        private const int DenominationBytes = 3;

        // The values for the denominations listed in the above remarks section
        private readonly ulong[] _denominationValues =
        {
            1, 2, 5, 10, 20, 25, 50, 100,
            200, 250, 500, 1_000, 2_000, 2_500, 5_000, 10_000,
            20_000, 25_000, 50_000, 100_000, 200_000, 250_000, 500_000, 1_000_000
        };

        /// <summary>
        /// Instantiates a new instance of the LP08ConfigureBillDenominationsParser class
        /// </summary>
        public LP08ConfigureBillDenominationsParser() : base(LongPoll.ConfigureBillDenominations)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();

            // set the disable after accept flag
            Data.DisableAfterAccept = longPoll[DisableAfterAcceptOffset] == 0;

            // pull out the bytes that contain the denomination info and convert the bits to a true/false array
            var denominationBytes = new ArraySegment<byte>(longPoll, Denominations1Offset, DenominationBytes).ToArray();
            var denominationsBitArray = new BitArray(denominationBytes);

            // convert the bit mask into a list of denomination values
            Data.Denominations.Clear();
            for (var i = 0; i < denominationsBitArray.Length; i++)
            {
                if (denominationsBitArray[i])
                {
                    // SASQuatch code wants values in cents. 
                    Data.Denominations.Add(_denominationValues[i] * SasConstants.DollarsToCentsMultiplier);
                }
            }

            Handle(Data);
            return AckLongPoll(command);
        }
    }
}