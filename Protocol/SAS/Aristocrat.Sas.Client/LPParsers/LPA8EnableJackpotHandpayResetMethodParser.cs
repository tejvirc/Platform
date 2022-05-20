namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Enable Jackpot handpay Reset Method Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A8        Enable Jackpot Handpay Reset Method
    /// Reset Method     1         00-01       00 - Standard handpay
    ///                                        01 - Reset to the credit meter
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           A8        Enable Jackpot Handpay Reset Method
    /// ACK code         1         00-02       00 - Reset method enabled
    ///                                        01 - Unable to enable reset method
    ///                                        02 - Not currently in a handpay condition
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)] // double check this
    public class LPa8EnableJackpotHandpayResetMethodParser : SasLongPollParser<EnableJackpotHandpayResetMethodResponse, EnableJackpotHandpayResetMethodData>
    {
        private const int DataStartIndex = 2;

        /// <summary>
        ///     Instantiates a new instance of the LPA8EnableJackpotHandpayResetMethodParser class
        /// </summary>
        public LPa8EnableJackpotHandpayResetMethodParser() : base(LongPoll.EnableJackpotHandpayResetMethod)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();

            Data.Method = (ResetMethod)longPoll[DataStartIndex];

            var result = Handle(Data);

            var response = command.Take(DataStartIndex).ToList();
            response.Add((byte)result.Code);

            return response;
        }
    }
}
