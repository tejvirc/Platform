namespace Aristocrat.Sas.Client.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Send SAS Version and Gaming Machine Serial Number Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           54        Send SAS Version and Gaming Machine Serial Number
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           54        Send SAS Version and Gaming Machine Serial Number
    /// Length           1         03-2B       Number of bytes following, not including CRC
    /// SAS Version    3 ASCII                 Implemented SAS version
    /// Serial #       variable ASCII          Gaming machine serial number (0-40 bytes)
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.PerClientLoad)]
    public class LP54SendSasVersionParser : SasLongPollParser<LongPollSendSasVersionResponse, LongPollData>
    {
        private const int MaxDataLength = 40;
        private const int MaxVersionLength = 3;

        /// <summary>
        ///     Instantiates a new instance of the LP54SendSASVersionParser class
        /// </summary>
        public LP54SendSasVersionParser() : base(LongPoll.SendSasVersionAndGameSerial)
        {
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var result = command.ToList();
            var response = Handler(Data);

            var version = Encoding.ASCII.GetBytes(response.SasVersion).Take(MaxVersionLength).ToList();
            var serialNumber = Encoding.ASCII.GetBytes(response.SerialNumber).Take(MaxDataLength).ToList();

            result.Add((byte)(version.Count + serialNumber.Count));
            result.AddRange(version);
            result.AddRange(serialNumber);
            return result;
        }
    }
}
