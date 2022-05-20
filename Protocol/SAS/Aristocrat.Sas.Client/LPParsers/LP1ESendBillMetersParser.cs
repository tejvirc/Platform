namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Total Bills Meter Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1E        Send Total Bills Meter
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           1E        Send Total Bills Meter
    /// $1 count      4 BCD 00000000-99999999  four byte $1 BCD meter value
    /// $5 count      4 BCD 00000000-99999999  four byte $5 BCD meter value
    /// $10 count     4 BCD 00000000-99999999  four byte $10 BCD meter value
    /// $20 count     4 BCD 00000000-99999999  four byte $20 BCD meter value
    /// $50 count     4 BCD 00000000-99999999  four byte $50 BCD meter value
    /// $100 count    4 BCD 00000000-99999999  four byte $100 BCD meter value
    /// CRC              2        0000-FFFF    16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP1ESendBillMetersParser : SasLongPollParser<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LP1ESendBillMetersParser class
        /// </summary>
        /// <param name="configuration">The configuration for the client</param>
        public LP1ESendBillMetersParser(SasClientConfiguration configuration)
            : base(LongPoll.SendBillCountMeters)
        {
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarIn1, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarsIn5, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarsIn10, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarsIn20, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarsIn50, MeterType.Lifetime));
            Data.Meters.Add(new LongPollReadMeterData(SasMeters.DollarsIn100, MeterType.Lifetime));
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug("LongPoll Send Bill Meters");
            var result = Handle(Data).Meters;

            var response = new List<byte>();
            response.AddRange(command);

            // get bill count meters and convert to BCD
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarIn1].MeterValue, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarsIn5].MeterValue, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarsIn10].MeterValue, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarsIn20].MeterValue, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarsIn50].MeterValue, SasConstants.Bcd8Digits));
            response.AddRange(Utilities.ToBcd(result[SasMeters.DollarsIn100].MeterValue, SasConstants.Bcd8Digits));

            return response;
        }
    }
}
