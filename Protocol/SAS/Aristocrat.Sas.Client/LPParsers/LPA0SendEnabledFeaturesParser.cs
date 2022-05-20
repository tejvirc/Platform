namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Enabled Features Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A0        Send Enabled Features
    /// Game Number    2 BCD        XXXX       Game number (0000 = gaming machine)
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A0        Send Enabled Features
    /// Game Number    2 BCD        XXXX       Game number
    /// Features1        1                     Feature codes - see Table 7.14c in the SAS Spec
    /// Features2        1                     Feature codes - see Table 7.14d in the SAS Spec
    /// Features3        1                     Feature codes - see Table 7.14e in the SAS Spec
    /// Features4        1                     Feature codes - see Table 7.14f in the SAS Spec
    /// Reserved       2 BCD       0000        Reserved
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)] // double check this
    public class LPA0SendEnabledFeaturesParser : SasLongPollParser<LongPollSendEnabledFeaturesResponse, LongPollSingleValueData<uint>>
    {
        private const int AddressAndCommandLength = 2;
        private const int GameNumOffset = 2;
        private const int GameNumLength = 2;

        /// <summary>
        ///     Instantiates a new instance of the LPA0SendEnabledFeaturesParser class
        /// </summary>
        public LPA0SendEnabledFeaturesParser() : base(LongPoll.SendEnabledFeatures)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            // here, capture the address, command, & game number, which are all part of the response
            var response = command.Take(AddressAndCommandLength + GameNumLength).ToList();

            var longPoll = command.ToArray();
            var (targetGame, validTargetGame) = Utilities.FromBcdWithValidation(longPoll, GameNumOffset, GameNumLength);
            if (!validTargetGame)
            {
                return NackLongPoll(command);
            }
            Data.Value = (uint)targetGame;
            var result = Handle(Data);
            
            response.AddRange(Utilities.ToBinary((uint)result.Features1Data, 1));
            response.AddRange(Utilities.ToBinary((uint)result.Features2Data, 1));
            response.AddRange(Utilities.ToBinary((uint)result.Features3Data, 1));
            response.AddRange(Utilities.ToBinary((uint)result.Features4Data, 1));
            response.AddRange(new byte[] { 0, 0 }); // add reserved 2 bytes

            return response;
        }
    }
}
