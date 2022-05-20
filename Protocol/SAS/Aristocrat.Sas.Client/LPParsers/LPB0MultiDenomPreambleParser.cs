namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;

    /// <summary>
    ///     This handles the Multi Denom Preamble Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B0        Multi Denom Preamble
    /// Length           1         02-FF       Length of bytes following not including CRC
    /// Denomination     1         00-3F       Binary number representing a specific denomination, or 00 for default response.
    ///                                        See Table C-4 in Appendix C of the SAS Specification for denominations.
    ///                                        See Table 16.1d in the SAS Specification for default responses.
    /// Base Command     1         01-FF       Command byte for multi-denom-aware long poll (see Table 16.1d)
    /// Data            varies                 Data appropriate for base long poll
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B0        Multi Denom Preamble
    /// Length           1         01-FF       Length of bytes following not including CRC
    /// Denomination     1         00-3F       Binary number representing a specific denomination, or 00 for default response.
    ///                                        See Table C-4 in Appendix C of the SAS Specification for denominations.
    ///                                        See Table 16.1d in the SAS Specification for default responses.
    /// Base Command     1         00-FF       Command byte for multi-denom-aware long poll or 00 if error.
    /// Data            varies                 Data appropriate for base long poll, or 1 byte error code from Table 16.1c
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB0MultiDenomPreambleParser : SasLongPollParser<LongPollResponse, LongPollData>
    {
        private const int AddressOffset = 0;
        private const int DataLengthOffset = 2;
        private const int DenominationOffset = 3;
        private const int BaseCommandOffset = 4;
        private const int BaseDataOffset = 5;

        private const int AddressLength = 1;
        private const int BaseCommandError = 0;
        private const byte MaxDenomValue = 0x3F;

        private const int AckNackLength = 1;
        private const int BusyResponseLength = 2;

        private readonly ISasParserFactory _factory;

        /// <summary>
        ///     Instantiates a new instance of the LPB0MultiDenomPreambleParser class
        /// </summary>
        public LPB0MultiDenomPreambleParser(ISasParserFactory factory)
            : base(LongPoll.MultiDenominationPreamble)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPollBytes = command.ToList();
            var responseBytes = longPollBytes.Take(DataLengthOffset).ToList();

            var targetAddress = longPollBytes.ElementAt(AddressOffset);
            var targetDenom = longPollBytes.ElementAt(DenominationOffset);
            var baseCommand = (LongPoll)longPollBytes.ElementAt(BaseCommandOffset);
            var baseData = longPollBytes.Skip(BaseDataOffset);

            var (handlerResponse, handlerError) = HandleSubParser(
                baseCommand,
                targetAddress,
                targetDenom,
                baseData.ToList()
            );

            if (handlerError == MultiDenomAwareErrorCode.NoError)
            {
                var postLengthBytes = new List<byte> { targetDenom };
                postLengthBytes.AddRange(handlerResponse);

                responseBytes.Add((byte)postLengthBytes.Count);
                responseBytes.AddRange(postLengthBytes);
            }
            else
            {
                var errorCodeResponse = new List<byte> { targetDenom, BaseCommandError, (byte)handlerError };
                responseBytes.Add((byte)errorCodeResponse.Count);
                responseBytes.AddRange(errorCodeResponse);
            }

            return responseBytes;
        }

        private (IReadOnlyCollection<byte> response, MultiDenomAwareErrorCode errorCode) HandleSubParser(
            LongPoll baseCommand,
            byte targetAddress,
            byte denomCode,
            IReadOnlyCollection<byte> baseData)
        {
            var subParserResponse = new List<byte>();
            var errorCode = MultiDenomAwareErrorCode.NoError;

            var subLongPoll = new List<byte> { targetAddress, (byte)baseCommand };
            subLongPoll.AddRange(baseData);

            var subParser = _factory.GetParserForLongPoll(baseCommand);

            if (denomCode > MaxDenomValue)
            {
                errorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom;
            }
            else if (subParser is UnhandledLongPollParser)
            {
                errorCode = MultiDenomAwareErrorCode.LongPollNotSupported;
            }
            else if (!(subParser is IMultiDenomAwareLongPollParser))
            {
                errorCode = MultiDenomAwareErrorCode.NotMultiDenomAware;
            }
            else
            {
                subParserResponse = ((IMultiDenomAwareLongPollParser)subParser).Parse(
                    subLongPoll,
                    DenominationCodes.GetDenominationForCode(denomCode)
                )?.ToList();
                if (subParserResponse is null || subParserResponse.Count == 0)
                {
                    errorCode = MultiDenomAwareErrorCode.LongPollNotSupported;
                }
                else
                {
                    subParserResponse = CleanSubParserResponse(subParserResponse, baseCommand).ToList();
                }
            }

            return (subParserResponse, errorCode);
        }

        private IReadOnlyCollection<byte> CleanSubParserResponse(
            IReadOnlyCollection<byte> subParserResponse,
            LongPoll baseCommand)
        {
            var subParserList = subParserResponse.ToList();
            switch (subParserList.Count)
            {
                // ACK/NACK
                // For ACK/NACK, we need to add the command to get desired format
                case AckNackLength:
                    return subParserList.Prepend((byte)baseCommand).ToList();
                // BUSY
                // BUSY has the address but not the command, so we remove the address, then add the command
                case BusyResponseLength:
                    return subParserList.Skip(AddressLength).Prepend((byte)baseCommand).ToList();
                // all others w/ data
                // The rest have address, command, and data, so we remove the address.
                default:
                    return subParserList.Skip(AddressLength).ToList();
            }
        }
    }
}