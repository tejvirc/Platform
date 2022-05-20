namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Remote Handpay Reset Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           94        Remote Handpay Reset
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           94        Remote Handpay Reset
    /// Reset code       1         00-02       0=handpay was reset
    ///                                        1=unable to reset handpay
    ///                                        2=not currently in a handpay condition
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.GeneralControl)]
    public class LP94RemoteHandPayResetParser :
        SasLongPollParser<LongPollReadSingleValueResponse<HandPayResetCode>, LongPollData>
    {
        /// <inheritdoc />
        public LP94RemoteHandPayResetParser()
            : base(LongPoll.RemoteHandpayReset)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = Handle(Data);

            return new Collection<byte>
            {
                command.Take(SasConstants.MinimumBytesForLongPoll),
                result.Data
            };
        }
    }
}