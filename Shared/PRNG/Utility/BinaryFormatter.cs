////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="BinaryFormatter.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

//// grabbed from MSDN wholesale, removed BigInteger stuff
//// http://msdn.microsoft.com/en-us/library/system.icustomformatter.format.aspx
namespace Utility
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Implements the IFormatProvider and ICustomFormatter
    /// for formatting the
    /// </summary>
    public class BinaryFormatter : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// returns this object if we are lookign for a custom formatter otherwise null
        /// </summary>
        /// <param name="formatType">the type requested</param>
        /// <returns>the type supported</returns>
        public object GetFormat(Type formatType)
        {
            // Determine whether custom formatting object is requested.
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Format number in binary (B), octal (O), or hexadecimal (H).
        /// </summary>
        /// <param name="format">the format string for formatting</param>
        /// <param name="arg">the argument to be formatted</param>
        /// <param name="formatProvider">the format provider</param>
        /// <returns>the formatted string</returns>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            // Handle format string.
            int baseNumber;

            // Handle null or empty format string, string with precision specifier.
            string thisFmt = string.Empty;

            /*
            // Extract first character of format string (precision specifiers
            // are not supported).
             */
            if (!string.IsNullOrEmpty(format))
            {
                thisFmt = format.Length > 1 ? format.Substring(0, 1) : format;
            }

            // Get a byte array representing the numeric value.
            byte[] bytes;
            if (arg is sbyte)
            {
                string byteString = ((sbyte)arg).ToString("X2", CultureInfo.InvariantCulture);
                bytes = new byte[1] { byte.Parse(byteString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture) };
            }
            else if (arg is byte)
            {
                bytes = new byte[1] { (byte)arg };
            }
            else if (arg is short)
            {
                bytes = BitConverter.GetBytes((short)arg);
            }
            else if (arg is int)
            {
                bytes = BitConverter.GetBytes((int)arg);
            }
            else if (arg is long)
            {
                bytes = BitConverter.GetBytes((long)arg);
            }
            else if (arg is ushort)
            {
                bytes = BitConverter.GetBytes((ushort)arg);
            }
            else if (arg is uint)
            {
                bytes = BitConverter.GetBytes((uint)arg);
            }
            else if (arg is ulong)
            {
                bytes = BitConverter.GetBytes((ulong)arg);
            }
            else
            {
                try
                {
                    return HandleOtherFormats(format, arg);
                }
                catch (FormatException e)
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The format of '{0}' is invalid.", format), e);
                }
            }

            switch (thisFmt.ToUpper(CultureInfo.InvariantCulture))
            {
                // Binary formatting.
                case "B":
                    {
                        baseNumber = 2;
                    }

                    break;
                case "O":
                    {
                        baseNumber = 8;
                    }

                    break;
                case "H":
                    {
                        baseNumber = 16;
                    }

                    break;
                default:
                    {
                        try
                        {
                            return HandleOtherFormats(format, arg);
                        }
                        catch (FormatException e)
                        {
                            throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The format of '{0}' is invalid.", format), e);
                        }
                    }
            }

            // Return a formatted string.
            string numericString = string.Empty;
            for (int ctr = bytes.GetUpperBound(0); ctr >= bytes.GetLowerBound(0); ctr--)
            {
                string byteString = Convert.ToString(bytes[ctr], baseNumber);
                if (baseNumber == 2)
                {
                    byteString = new string('0', 8 - byteString.Length) + byteString;
                }
                else if (baseNumber == 8)
                {
                    byteString = new string('0', 4 - byteString.Length) + byteString;
                }
                else
                {
                    // base 16 support
                    byteString = new string('0', 2 - byteString.Length) + byteString;
                }

                numericString += byteString + " ";
            }

            return numericString.Trim();
        }

        /// <summary>
        /// Used to handle other string formats that are not recognized
        /// </summary>
        /// <param name="format">the format being requested</param>
        /// <param name="arg">the arguments that go with the format string</param>
        /// <returns>the formatted string</returns>
        private static string HandleOtherFormats(string format, object arg)
        {
            var argument = arg as IFormattable;
            if (argument != null)
            {
                return argument.ToString(format, CultureInfo.InvariantCulture);
            }
            else if (arg != null)
            {
                return arg.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
