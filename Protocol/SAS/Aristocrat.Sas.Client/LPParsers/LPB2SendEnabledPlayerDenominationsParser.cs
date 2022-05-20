namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send Enabled Player Denominations Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B2        Send Enabled Player Denominations
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           B2        Send Enabled Player Denominations
    /// Length           1         01-80       Length of bytes following, not including CRC
    /// # denominations  1         00-7F       Number of player denominations currently enabled
    /// Player denom     x         01-3F       Enabled denominations list. See Table C-4 in Appendix C of the SAS Specification.
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LPB2SendEnabledPlayerDenominationsParser :
        SasLongPollParser<LongPollEnabledPlayerDenominationsResponse, LongPollData>
    {
        /// <summary>
        ///     Instantiates a new instance of the LPB2SendEnabledPlayerDenominationsParser class
        /// </summary>
        public LPB2SendEnabledPlayerDenominationsParser()
            : base(LongPoll.SendEnabledPlayerDenominations)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handler(Data);
            if (response == null)
            {
                return null;
            }

            var result = new List<byte>(command)
            {
                (byte)(response.EnabledDenominations.Count + 1), (byte)response.EnabledDenominations.Count
            };

            result.AddRange(response.EnabledDenominations);
            return result;
        }
    }
}