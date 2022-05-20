namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using System.Collections;
    using System.Linq;

    /// <summary>
    ///     Provide conversion methods for packed hexadecimal strings.
    /// </summary>
    public class ConvertExtensions
    {
        /// <summary>
        ///     Convert packed hexadecimal string to a byte array.
        /// </summary>
        /// <param name="input">Input packed hexadecimal string.</param>
        /// <returns>Byte array</returns>
        public static byte[] FromPackedHexString(string input)
        {
            if (input.Length % 2 != 0)
            {
                input.Append('0'); // Try to correct the string input
            }

            var result = new byte[input.Length / 2];

            for (var index = 0; index < result.Length; index++)
            {
                result[index] = Convert.ToByte(input.Substring(2 * index, 2), 16);
            }

            return result;
        }
        /// <summary>
        ///     Convert byte array to packed hexadecimal or formatted string.
        /// </summary>
        /// <param name="input">Input byte array.</param>
        /// <param name="usePackedHexString">Flag to use packed hex string.</param>
        /// <returns>Result string.</returns>
        public static string ToGatResultString(byte[] input, bool usePackedHexString)
        {
            return usePackedHexString || !input.Any() ?
                ToPackedHexString(input) :
                $"{BitConverter.ToInt32(input, 0):X}";

        }

        /// <summary>
        ///     Convert BitArray to packed hexadecimal string.
        /// </summary>
        /// <param name="input">Input BitArray.</param>
        /// <returns>Packed hexadecimal string.</returns>
        public static string ToPackedHexString(BitArray input)
        {
            var inputAsBytes = new byte[(input.Length - 1) / 8 + 1];
            input.CopyTo(inputAsBytes, 0);
            return ToPackedHexString(inputAsBytes);
        }

        /// <summary>
        ///     Convert byte array to packed hexadecimal string.
        /// </summary>
        /// <param name="input">Input byte array.</param>
        /// <returns>Packed hexadecimal string.</returns>
        public static string ToPackedHexString(byte[] input)
        {
            return string.Join(string.Empty, input.Select(b => ((int)b).ToString(@"X2")));
        }

        /// <summary>
        ///     Convert int32 to a formatted Hex string.
        /// </summary>
        /// <param name="input">Int32 input.</param>
        /// <returns>Hex formatted string.</returns>
        public static string ToHexString(int input)
        {
            return $"0X{input:X}";
        }
    }
}