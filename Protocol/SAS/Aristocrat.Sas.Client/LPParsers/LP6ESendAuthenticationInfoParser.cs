namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Authentication Info Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           6E        Send Authentication Info
    /// Length           1         01-AF       Number of bytes following, not including CRC
    /// Action           1         00-03       Requested Authentication action
    ///                                         00=Interrogate number of installed components
    ///                                         01=Read status of component (address required)
    ///                                         02=Authenticate component (address required)
    ///                                         03=Interrogate authentication status
    /// 
    ///       If the action requires an address, the following is included
    /// Addressing mode  1         00-01       00=address by index number, 01=address by name
    /// Index/name length 1        01-7F
    /// Component       x bytes                Binary component index if address mode=00
    /// Index/Name                             ASCII component name if address mode=01
    ///
    ///       If the action is authenticate, the following authentication data is included
    /// Method           4       nnnnnnnn      Authentication method requested from Table 17.1c of the SAS Protocol Spec page 17-4
    /// Seed length      1         00-14       Length of seed to be used by the authentication method
    /// Seed           x bytes                 Authentication seed value
    /// Offset length    1         00-10       Length of offset
    /// Offset         x bytes                 Authentication offset value
    /// 
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           6E        Send Authentication Info
    /// Length           1         03-B1       Number of bytes following, not including CRC
    /// component list CRC 2     0000-FFFF     16-bit CRC across all ASCII component names
    /// Status           1           xx        Status of component list, component, or error code if an error. See Table 17.1d
    ///
    ///      If the status is for a component, the following data is included
    /// Name length      1         00-7F       Length of name data following
    /// Name          x ASCII                  ASCII list name or component name
    /// size length      1         00-10       Length of size data following (if component is not byte addressable, size length will be zero)
    /// Size             x                     Number of components if action=00, or size of component
    /// Available        4        nnnnnnnn     Authentication methods supported by component(see
    /// methods
    ///
    ///      if status=authentication in progress or completed successfully, the following data is included
    /// Method           4       nnnnnnnn      Authentication method in use from Table 17.1c of the SAS Protocol Spec page 17-4
    /// Authentication len 1       00-14       00 if authentication in progress
    /// data           x bytes                 Authentication data if completed successfully
    /// 
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP6ESendAuthenticationInfoParser : SasLongPollParser<SendAuthenticationInfoResponse, SendAuthenticationInfoCommand>
    {
        private const int BytesNotIncludedInLength = 5;
        private const int LengthOffset = 2;
        private const int ActionCodeOffset = 3;
        private const int AddressingModeOffset = 4;
        private const int AddressLengthOffset = 5;
        private const int AddressDataOffset = 6;

        private const int AuthenticationSeedLengthMax = 14;
        private const int ComponentNameLengthMax = 127;

        /// <summary>
        /// Instantiates a new instance of the LP6EComponentAuthenticationParser class
        /// </summary>
        public LP6ESendAuthenticationInfoParser()
            : base(LongPoll.SendAuthenticationInformation)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var data = new SendAuthenticationInfoCommand();
            if (!GetAuthenticationInfoDataFromCommand(longPoll, ref data))
            {
                return GenerateLongPollResponse(
                    command,
                    data,
                    new SendAuthenticationInfoResponse { Status = AuthenticationStatus.InvalidCommand });
            }

            var responseData = Handler(data);
            Handlers = responseData.Handlers;

            return GenerateLongPollResponse(command, data, responseData);
        }

        private bool GetAuthenticationInfoDataFromCommand(byte[] longPoll, ref SendAuthenticationInfoCommand data)
        {
            // check if the length reported in the length byte is equal to
            // the actual length. Don't include the address + command + length + crc bytes
            var length = longPoll[LengthOffset];
            if (longPoll.Length - BytesNotIncludedInLength != length)
            {
                Logger.Debug($"SendAuthenticationInfo length is wrong: {length}");
                return false;
            }

            data.Action = (AuthenticationAction)longPoll[ActionCodeOffset];
            Logger.Debug($"Action = {data.Action}");

            if (data.Action == AuthenticationAction.InterrogateNumberOfInstalledComponents ||
                data.Action == AuthenticationAction.InterrogateAuthenticationStatus)
            {
                // no more data for an interrogate, so exit early
                return true;
            }

            if (data.Action != AuthenticationAction.ReadStatusOfComponent &&
                data.Action != AuthenticationAction.AuthenticateComponent)
            {
                Logger.Debug($"SendAuthenticationInfo.Action is invalid: {longPoll[ActionCodeOffset]}");
                return false;
            }

            // Additional addressing data
            data.AddressingMode = (AuthenticationAddressingMode)longPoll[AddressingModeOffset];
            Logger.Debug($"AddressingMode = {data.AddressingMode}");
            var addressLength = longPoll[AddressLengthOffset];
            if (data.AddressingMode == AuthenticationAddressingMode.AddressingByIndex)
            {
                if (addressLength > ComponentNameLengthMax ||
                    AddressDataOffset + addressLength - 1 > length + LengthOffset)
                {
                    Logger.Debug($"SendAuthenticationInfo.Addressing length is too large: {addressLength}");
                    return false;
                }

                if (addressLength <= 0)
                {
                    Logger.Debug("SendAuthenticationInfo.Addressing length must be greater than 0");
                    return false;
                }

                data.ComponentIndex = (int)Utilities.FromBinary(
                    longPoll,
                    AddressDataOffset,
                    Math.Min(sizeof(int), (int)addressLength));
                Logger.Debug($"ComponentIndex = {data.ComponentIndex}");
            }
            else if (data.AddressingMode == AuthenticationAddressingMode.AddressingByName)
            {
                bool valid;
                (data.ComponentName, valid) = Utilities.FromBytesToString(longPoll, AddressDataOffset, addressLength);
                if (!valid)
                {
                    Logger.Debug(
                        $"SendAuthenticationInfo.ComponentName addressing length is too large: {addressLength}");
                    return false;
                }

                Logger.Debug($"ComponentName = {data.ComponentName}");
            }
            else
            {
                Logger.Debug($"SendAuthenticationInfo.AddressMode is invalid: {longPoll[AddressingModeOffset]}");
                return false;
            }

            if (data.Action == AuthenticationAction.ReadStatusOfComponent)
            {
                // no more data for a component status, so exit early
                return true;
            }

            // Additional authenticate data, varying position because of previous variable data.
            var offset = AddressDataOffset + addressLength;
            data.Method = (AuthenticationMethods)longPoll[offset++];
            Logger.Debug($"Method = {data.Method}");

            var seedLength = (int)longPoll[offset++];
            if (seedLength > AuthenticationSeedLengthMax || offset + seedLength - 1 > length + LengthOffset)
            {
                Logger.Debug($"SendAuthenticationInfo.AuthenticationSeed length is too large: {seedLength}");
                return false;
            }

            data.AuthenticationSeed = new byte[seedLength];
            Buffer.BlockCopy(longPoll, offset, data.AuthenticationSeed, 0, seedLength);
            Logger.Debug($"AuthenticationSeed = {BitConverter.ToString(data.AuthenticationSeed)}");
            offset += seedLength;

            var offsetLength = (int)longPoll[offset++];
            if (offsetLength > sizeof(ulong))
            {
                Logger.Debug(
                    $"SendAuthenticationInfo.AuthenticationOffset: although the protocol allows offsets up to 16 bytes long, we only can handle 8 bytes.");
                return false;
            }

            if (offset + offsetLength - 1 > length + LengthOffset)
            {
                Logger.Debug($"SendAuthenticationInfo.AuthenticationOffset length is too large: {offsetLength}");
                return false;
            }

            if (offsetLength > 0)
            {
                data.AuthenticationOffset = (long)Utilities.FromBinary64Bits(longPoll, offset, offsetLength);
            }

            Logger.Debug($"AuthenticationOffset = {data.AuthenticationOffset}");

            return true;
        }

        private Collection<byte> GenerateLongPollResponse(
            IReadOnlyCollection<byte> command,
            SendAuthenticationInfoCommand commandData,
            SendAuthenticationInfoResponse responseData)
        {
            Logger.Debug($"generating response for {responseData.Status}");
            var content = new List<byte>();
            content.AddRange(Utilities.ToBinary(responseData.ComponentListCrc, sizeof(ushort)));
            content.Add((byte)responseData.Status);

            if (responseData.Status != AuthenticationStatus.InvalidCommand)
            {
                switch (commandData.Action)
                {
                    // 17.1.1
                    case AuthenticationAction.InterrogateNumberOfInstalledComponents:
                        // Component Name is empty in this case.
                        content.Add(0); // length

                        // Component Size is the number of components
                        content.Add(2); // length: 2 bytes (65535 max) is plenty
                        content.AddRange(Utilities.ToBinary((ushort)responseData.ComponentSize, sizeof(ushort)));

                        // Allowed method(s) is None, when speaking for the whole set
                        content.AddRange(Utilities.ToBinary((uint)AuthenticationMethods.None, sizeof(uint)));
                        break;

                    // 17.1.2
                    case AuthenticationAction.ReadStatusOfComponent:
                    case AuthenticationAction.InterrogateAuthenticationStatus:
                        if (responseData.Status == AuthenticationStatus.ComponentDoesNotExist ||
                            responseData.Status == AuthenticationStatus.NoAuthenticationDataAvailable)
                        {
                            break;
                        }

                        // Component Name
                        content.Add((byte)responseData.ComponentName.Length);
                        content.AddRange(Encoding.UTF8.GetBytes(responseData.ComponentName));

                        // Component Size
                        content.Add(8); // length: 8 bytes (2^64 max)
                        content.AddRange(Utilities.ToBinary((ulong)responseData.ComponentSize, sizeof(ulong)));

                        // Available methods
                        content.AddRange(Utilities.ToBinary((uint)responseData.AvailableMethods, sizeof(uint)));

                        if (responseData.Status == AuthenticationStatus.AuthenticationCurrentlyInProgress ||
                            responseData.Status == AuthenticationStatus.AuthenticationComplete)
                        {
                            // Method in use
                            content.AddRange(Utilities.ToBinary((uint)responseData.Method, sizeof(uint)));

                            // Authentication result (if any)
                            content.Add((byte)responseData.AuthenticationData.Length);
                            content.AddRange(responseData.AuthenticationData);
                        }

                        break;

                    // 17.1.3
                    case AuthenticationAction.AuthenticateComponent:
                        // Nothing more to add
                        break;
                }
            }

            var response = command.Take(2).ToList();
            response.Add((byte)content.Count);
            response.AddRange(content.ToArray());
            return new Collection<byte>(response);
        }
    }
}