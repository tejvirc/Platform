namespace Aristocrat.Monaco.Common
{
    using System.Text;

    /// <summary>
    ///     Byte array extensions
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        ///     Returns a string of bytes formatted as separate words.
        /// </summary>
        /// <param name="bytes">The array of bytes to format.</param>
        /// <returns>A string of bytes formatted as separate words.</returns>
        public static string FormatBytes(this byte[] bytes)
        {
            if (bytes is null)
            {
                return string.Empty;
            }

            var format = new StringBuilder();
            var i = 0;
            foreach (var t in bytes)
            {
                format.Append(t.ToString("x2"));
                i++;
                if (i != 2)
                {
                    continue;
                }

                i = 0;
                format.Append(" ");
            }

            return format.ToString().ToUpper();
        }
    }
}