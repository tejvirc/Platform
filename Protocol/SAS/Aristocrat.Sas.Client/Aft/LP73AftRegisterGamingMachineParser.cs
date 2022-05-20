namespace Aristocrat.Sas.Client.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the AFT Register Gaming Machine Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field           Bytes      Value      Description
    /// Address           1        01-7F      Address of gaming machine
    /// Command           1          73       AFT register gaming machine
    /// Length            1        01,1D      Number of bytes following, not including CRC
    /// Registration Code 1         nn        00 = Initialize registration
    ///                                       01 = Register gaming machine
    ///                                       40 = Request operator acknowledgement
    ///                                       80 = Unregister gaming machine
    ///                                       FF = Read current registration
    /// Asset Number      4       nnnnnnnn    Gaming machine asset number or house ID
    /// Registration Key  20       nn...      Registration key
    /// POS ID            4       nnnnnnnn    Point of Sale terminal ID (0000 = no POS ID, FFFFFFFF = no change)
    /// CRC               2       0000-FFFF   16-bit CRC
    ///
    /// ===============================================================================================================================
    /// Response
    /// Field             Bytes    Value       Description
    /// Address             1      01-7F       Address of gaming machine responding
    /// Command             1        73        AFT register gaming machine
    /// Length              1        1D        Number of bytes following, not including CRC
    /// Registration Status 1        nn        00 = Gaming machine registration ready
    ///                                        01 = Gaming machine registered
    ///                                        40 = Gaming machine registration pending
    ///                                        80 = Gaming machine not registered
    /// Asset Number        4     nnnnnnnn     Gaming machine asset number or house ID
    /// Registration Key    20     nn...       Registration key
    /// POS ID              4     nnnnnnnn     Point of Sale terminal ID (0 = no POS ID)
    /// CRC                 2    0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Aft)]
    public class LP73AftRegisterGamingMachineParser : SasLongPollParser<AftRegisterGamingMachineResponseData, AftRegisterGamingMachineData>
    {
        private const int LengthOffset = 2;
        private const int RegistrationOffset = 3;
        private const int AssetNumberOffset = 4;
        private const int AssetNumberSize = 4;
        private const int RegistrationKeyOffset = 8;
        private const int RegistrationKeySize = 20;
        private const int PosIdOffset = 28;
        private const int PosIdSize = 4;
        private const int BytesNotIncludedInLength = 5;
        private const byte ShortCommandLength = 0x01;
        private const byte LongCommandLength = 0x1D;
        private const int MinimumLongPollLength = 1;
        private const int AssetNumberLength = 4;
        private const int PosIdLength = 4;

        /// <inheritdoc />
        public LP73AftRegisterGamingMachineParser()
            : base(LongPoll.AftRegisterGamingMachine)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var data = new AftRegisterGamingMachineData();
            if (!GetAftRegisterGamingMachineDataFromCommand(longPoll, ref data))
            {
                return new Collection<byte>();
            }

            var responseData = Handler(data);
            return GenerateLongPollResponse(command, responseData);
        }

        private bool GetAftRegisterGamingMachineDataFromCommand(byte[] longPoll, ref AftRegisterGamingMachineData data)
        {
            // Total length - (address + command + length + crc bytes)
            int longPollLength = longPoll.Length - BytesNotIncludedInLength;

            // Must have at least enough bytes to fill Length and Registration Code
            if (longPollLength < MinimumLongPollLength)
            {
                return false;
            }

            data.Length = longPoll[LengthOffset];
            data.RegistrationCode = (AftRegistrationCode)longPoll[RegistrationOffset];

            // Check that actual length matches reported
            if (longPollLength != data.Length)
            {
                return false;
            }

            // Check that the length is one of the valid values
            if (data.Length != ShortCommandLength && data.Length != LongCommandLength)
            {
                return false;
            }

            // Ignore any additional fields on interrogate or unregister
            if (data.RegistrationCode == AftRegistrationCode.ReadCurrentRegistration
             || data.RegistrationCode == AftRegistrationCode.UnregisterGamingMachine)
            {
                return true;
            }

            // This is not an interrogation, so this would be the wrong length
            if (data.Length == ShortCommandLength)
            {
                return false;
            }

            data.AssetNumber = Utilities.FromBinary(longPoll, AssetNumberOffset, AssetNumberSize);
            Array.Copy(longPoll, RegistrationKeyOffset, data.RegistrationKey, 0, RegistrationKeySize);
            data.PosId = Utilities.FromBinary(longPoll, PosIdOffset, PosIdSize);

            return true;
        }

        private Collection<byte> GenerateLongPollResponse(IReadOnlyCollection<byte> command, AftRegisterGamingMachineResponseData responseData)
        {
            var response =
                new List<byte>(command.Take(2).ToList()) { responseData.Length, (byte)responseData.RegistrationStatus };

            response.AddRange(Utilities.ToBinary(responseData.AssetNumber, AssetNumberLength));
            response.AddRange(responseData.RegistrationKey);
            response.AddRange(Utilities.ToBinary(responseData.PosId, PosIdLength));

            return new Collection<byte>(response);
        }
    }
}