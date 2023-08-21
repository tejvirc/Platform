namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     Base class for all the single meter read long polls that return a 4 byte value
    /// </summary>
    public class SasSingleMeterLongPollParserBase : SasLongPollMultiDenomAwareParser<LongPollReadMeterResponse, LongPollReadMeterData>
    {
        private const int SizeOfAddressAndCommandFields = 2;

        private readonly bool _multiDenomAware;

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            return Parse(command, 0, false);
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom)
        {
            return Parse(command, denom, true);
        }

        /// <summary>
        ///     Instantiates a new instance of the SasSingleMeterLongPollParserBase class
        /// </summary>
        /// <param name="longPoll">The long poll command this parser handles</param>
        /// <param name="meter">The meter associated with this long poll</param>
        /// <param name="type">The type of meter. Period or Lifetime</param>
        /// <param name="configuration">The client configuration for the long poll</param>
        /// <param name="multiDenomAware">Whether or not this meter poll is multi-denom aware</param>
        protected SasSingleMeterLongPollParserBase(LongPoll longPoll, SasMeters meter, MeterType type, SasClientConfiguration configuration, bool multiDenomAware = false)
            : base(longPoll)
        {
            _multiDenomAware = multiDenomAware;
            Data.MeterType = type;
            Data.Meter = meter;
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <summary>
        ///     Parses the long poll and gets a response
        /// </summary>
        /// <param name="command">the command data to parse</param>
        /// <param name="denom">the denom used for this command</param>
        /// <param name="multiDenom">Whether or not this command is for a multi-denom configuration</param>
        /// <returns>The response for the long poll or null if there isn't a response</returns>
        protected virtual IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom, bool multiDenom)
        {
            Logger.Debug($"LongPoll {Command}");
            if (multiDenom && !_multiDenomAware)
            {
                return GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.NotMultiDenomAware);
            }

            Data.TargetDenomination = denom;
            Data.MultiDenomPoll = multiDenom;
            var result = Handle(Data);

            if (result.ErrorCode != MultiDenomAwareErrorCode.NoError)
            {
                return multiDenom
                    ? GenerateMultiDenomAwareError(command.First(), result.ErrorCode)
                    : NackLongPoll(command);
            }

            if (result.Meter != Data.Meter)
            {
                return multiDenom
                    ? GenerateMultiDenomAwareError(command.First(), MultiDenomAwareErrorCode.ImproperlyFormatted)
                    : NackLongPoll(command);
            }

            var response = command.Take(SizeOfAddressAndCommandFields).ToList();

            // get the meter and convert to BCD
            var meter = result.MeterValue;
            response.AddRange(Utilities.ToBcd(meter, SasConstants.Bcd8Digits));

            return response;
        }
    }
}