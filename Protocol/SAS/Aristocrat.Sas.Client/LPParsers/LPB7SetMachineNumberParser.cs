namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Set Machine Number B7
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1         B7          Set Machine Number
    /// Length           1         05-nn       Total length of the bytes following not including the crc
    /// Asset Number     4         nnnnnnnn    New game machine asset number or house ID (use 0 for interrogate only)
    /// Floor Location   1         00-28       Length of gaming machine floor location (use 0 for interrogate only)
    /// length
    /// Floor Location   x ASCII   ???         New game machine floor location
    /// CRC              2         0000-FFFF   16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1         B7          Set Machine Number
    /// Control flags    1         00-FF       Bit     Description
    ///                                        -----------------------------------------
    ///                                        0       0 = No host control of asset number
    ///                                                1 = Asset number may be changed by host
    ///                                        -----------------------------------------
    ///                                        1       0 = No host control of floor location
    ///                                                1 = Floor location may be changed by host
    ///                                        -----------------------------------------
    ///                                        7~2     TBD (leave 0)
    /// Asset Number     4         nnnnnnnn    Current asset number or house ID
    ///                                        (0 if no asset number)
    /// Floor Location   1         00-28       Length of gaming machine floor location (0 if no floor location)
    /// length
    /// Floor Location   x ASCII   ???         Current floor location
    /// CRC              2         0000-FFFF   16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB7SetMachineNumberParser : SasLongPollParser<LongPollSetMachineNumbersResponse, LongPollSetMachineNumbersData>
    {
        private const int CommandAndGameIdSize = 2; // the size of the command list to include in the response
        private const byte AssetNumberAndLengthSize = 6;
        private const uint MinCommandSize = 10;
        private const int AssetNumberIndex = 3;
        private const int FloorLocationSizeIndex = 7;
        private const int FloorLocationIndex = 8;
        private const int AssetNumberLength = 4;
        private const int MaxFloorLocationStringLength = 0x28; // from spec

        /// <summary>
        ///     Instantiates a new instance of the LPB7SetMachineNumberParser class
        /// </summary>
        public LPB7SetMachineNumberParser()
            : base(LongPoll.SetMachineNumbers)
        {
        }

        /// <summary>
        ///     This handles the Set Machine Number B7
        /// </summary>
        /// <remarks>
        ///     Returns a filled in response with an asset number and floor location or returns a NACK response in case of a bad command length
        /// </remarks>
        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.Take(CommandAndGameIdSize).ToList();
            var info = command.ToArray();

            if (info.Length < MinCommandSize || info[FloorLocationSizeIndex] > MaxFloorLocationStringLength)
            {
                return NackLongPoll(command);
            }

            Data.AssetNumber = Utilities.FromBinary(info, AssetNumberIndex, AssetNumberLength);
            Data.FloorLocation = string.Empty;
            if (info[FloorLocationSizeIndex] != 0)
            {
                Data.FloorLocation = System.Text.Encoding.ASCII.GetString(
                    info,
                    FloorLocationIndex,
                    info[FloorLocationSizeIndex]);
            }

            var response = Handler(Data);
            var floorLocation = response.FloorLocation.Length < MaxFloorLocationStringLength
                ? response.FloorLocation
                : response.FloorLocation.Substring(0, MaxFloorLocationStringLength);
            var floorLocationReturn = System.Text.Encoding.ASCII.GetBytes(floorLocation);
            result.Add((byte)(AssetNumberAndLengthSize + floorLocationReturn.Length));
            result.Add((byte)response.ControlFlags);
            result.AddRange(Utilities.ToBinary(response.AssetNumber, AssetNumberLength));
            result.Add((byte)floorLocationReturn.Length);
            if (floorLocationReturn.Length != 0)
            {
                result.AddRange(floorLocationReturn);
            }

            return result;
        }
    }
}