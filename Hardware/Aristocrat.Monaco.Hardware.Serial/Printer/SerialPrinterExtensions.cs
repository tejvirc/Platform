namespace Aristocrat.Monaco.Hardware.Serial
{
    using System.Text;

    public static class SerialPrinterExtensions
    {

        static SerialPrinterExtensions()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        ///     Convert a string to byte array
        /// </summary>
        /// <param name="message">The message string.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this string message)
        {
            return Encoding.GetEncoding("windows-1252").GetBytes(message);
        }
    }
}