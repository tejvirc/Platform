namespace Aristocrat.Mgam.Client.Helpers
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using Messaging;
    using Routing;

    /// <summary>
    ///     Message helper functions and method extensions.
    /// </summary>
    internal static class MessageHelper
    {
        /// <summary>
        ///     Parses the response sent from the host.
        /// </summary>
        /// <param name="buffer">The response bytes sent from the host.</param>
        /// <returns></returns>
        public static Payload ParseResponse(byte[] buffer)
        {
            var newLine = Encoding.ASCII.GetBytes(Environment.NewLine);

            var header = string.Join("", buffer.SplitStrings(newLine, 2));

            var re = new Regex(@"MGAM Payload: (?<format>(XMLCompressedDataSet|XMLDataSet))MGAM Size: (?<size>\d+)");

            var match = re.Match(header);

            if (!match.Success)
            {
                throw new InvalidOperationException($"Invalid response header received from host, {BitConverter.ToString(buffer)}");
            }

            XmlDataSetFormat format;

            if (match.Groups["format"].Value == ProtocolConstants.XmlCompressedDataSet)
            {
                format = XmlDataSetFormat.XmlCompressedDataSet;
            }
            else if (match.Groups["format"].Value == ProtocolConstants.XmlDataSet)
            {
                format = XmlDataSetFormat.XmlDataSet;
            }
            else
            {
                throw new InvalidOperationException($"Invalid response content format received from host, {match.Groups["format"].Value}");
            }

            if (!int.TryParse(match.Groups["size"].Value, out var messageSize))
            {
                throw new InvalidOperationException($"Invalid response message size received from host, {match.Groups["size"].Value}");
            }

            var messageStart = header.Length + newLine.Length * 2;

            var content = new byte[messageSize];
            Buffer.BlockCopy(buffer, messageStart, content, 0, messageSize);

            return new Payload
            {
                Format = format,
                MessageSize = messageSize,
                Content = content
            };
        }
    }
}
