namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;
    using Metering;

    /// <inheritdoc />
    public abstract class SendSelectedMetersForGameNParserBase
        : SasLongPollMultiDenomAwareParser<SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>
    {
        private const int ByteNotIncludedInLength = SizeOfGameNumberField + 3;

        // Byte offset of fields in the data 
        private const uint LengthOffset = 2;
        private const uint GameNumberOffset = 3;
        private const uint MeterOffset = 5;

        // Size of data fields in byte(s)
        private const int MaxExtendedCommandLengthValue = byte.MaxValue;
        private const int MaxCommandLengthValue = 0x0C;
        private const int SizeOfGameNumberField = 2;
        private const int SizeOfAddressAndCommandFields = 2;
        private const int SizeOfExtendMeterField = 2;
        
        private readonly bool _extendMeters;
        private readonly int _maxCommandLength;

        /// <inheritdoc />
        protected SendSelectedMetersForGameNParserBase(LongPoll command, bool extendMeters, SasClientConfiguration configuration)
            : base(command)
        {
            _maxCommandLength = extendMeters ? MaxExtendedCommandLengthValue : MaxCommandLengthValue;
            _extendMeters = extendMeters;
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command) => Parse(command, 0, false);

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom) => Parse(command, denom, true);

        /// <summary>
        ///     Handles the parsing of the long poll, being aware of multi-denom-awareness.
        /// </summary>
        /// <param name="command">Byte collection representing the long poll received</param>
        /// <param name="denom">Desired denomination represented in cents</param>
        /// <param name="multiDenomPoll">Whether or not to treat this as a multi-denom poll</param>
        /// <returns>Long poll response, or null if there is no response</returns>
        private IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom, bool multiDenomPoll)
        {
            if (!ParseCommand(command))
            {
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            Data.TargetDenomination = denom;
            Data.MultiDenomPoll = multiDenomPoll;

            var handlerResponse = Handle(Data);

            if (handlerResponse.ErrorCode != MultiDenomAwareErrorCode.NoError)
            {
                return multiDenomPoll
                    ? GenerateMultiDenomAwareError(command.First(), handlerResponse.ErrorCode)
                    : NackLongPoll(command);
            }

            var meterBlock = GetMeterData(handlerResponse.SelectedMeters);

            var response = new List<byte>();
            response.AddRange(command.Take(SizeOfAddressAndCommandFields));
            response.Add((byte)(meterBlock.Count + SizeOfGameNumberField));
            response.AddRange(Utilities.ToBcd(Data.GameNumber, SasConstants.Bcd4Digits));
            response.AddRange(meterBlock);
            
            return response;
        }

        private bool ParseCommand(IReadOnlyCollection<byte> command)
        {
            var retStatus = false;
            var longPoll = command.ToArray();
            var length = longPoll[LengthOffset];

            if (ValidLength(longPoll, length))
            {
                var meterLength = length - SizeOfGameNumberField;
                var (gameNumber, valid) = Utilities.FromBcdWithValidation(longPoll, GameNumberOffset, SizeOfGameNumberField);
                if (valid)
                {
                    Data.GameNumber = gameNumber;
                    Data.RequestedMeters.Clear();
                    Data.RequestedMeters.AddRange(GetMeters(longPoll, meterLength));

                    retStatus = true;
                }
                else
                {
                    Logger.Error($"[SAS] Invalid BCD for game number when parsing command:{longPoll}.");
                }
            }
            else
            {
                Logger.Error($"[SAS] Invalid length when parsing command:{longPoll}.");
            }

            return retStatus;
        }

        private bool ValidLength(IReadOnlyCollection<byte> longPoll, int length)
        {
            return length >= SizeOfGameNumberField && // Do we have enough to read the game number?
                   length <= _maxCommandLength && // Have we exceed our maximum command size?
                   length <= (longPoll.Count - ByteNotIncludedInLength) && // Do we have enough bytes to read?
                   (!_extendMeters || ((length - SizeOfGameNumberField) % SizeOfExtendMeterField == 0)); // Extended meters must be even
        }

        private IEnumerable<SasMeterId> GetMeters(byte[] longPoll, int meterLength)
        {
            if (!_extendMeters)
            {
                return longPoll.Skip((int)MeterOffset).Take(meterLength).Select(x => (SasMeterId)x);
            }

            var meters = new List<SasMeterId>();
            for (var i = 0; i < meterLength; i += SizeOfExtendMeterField)
            {
                meters.Add((SasMeterId)Utilities.FromBinary(
                    longPoll,
                    (uint)(MeterOffset + i),
                    SizeOfExtendMeterField));
            }

            return meters;
        }

        private IList<byte> GetMeterData(IEnumerable<SelectedMeterForGameNResponse> selectedMeters)
        {
            var meterData = new List<byte>();

            foreach (var meter in selectedMeters)
            {
                var meterSize = _extendMeters
                    ? Math.Min(meter.MeterLength, SasConstants.MaxMeterLength) / 2
                    : meter.MinMeterLength;
                if ((meterSize + meterData.Count + SizeOfGameNumberField) > byte.MaxValue)
                {
                    // If we are going to exceed the max length just truncate and only return as many meters as we can
                    break;
                }

                if (_extendMeters)
                {
                    meterData.AddRange(Utilities.ToBinary((uint)meter.MeterCode, SizeOfExtendMeterField));
                    meterData.Add((byte)meterSize);
                }
                else
                {
                    meterData.Add((byte)meter.MeterCode);
                }

                meterData.AddRange(Utilities.ToBcd(meter.MeterValue, meterSize));
            }
            
            return meterData;
        }
    }
}