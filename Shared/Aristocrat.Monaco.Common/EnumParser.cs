namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Provides enum related helper methods for parsing and conversion
    /// </summary>
    public static class EnumParser
    {
        /// <summary>
        /// Provides safer enum parsing than Enum.TryParse and direct casts.
        /// For instance, the following incorrectly returns true:
        /// Enum.TryParse<Aristocrat.Monaco.Asp.Client.Contracts.GameDisableReason>("27", out _)
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The value to be parsed to valid enum value</param>
        /// <returns>Named tuple that indicates whether parse succeeded and if so, the resulting enum value</returns>
        public static (bool IsValid, T? Result) Parse<T>(object value) where T : struct, Enum
        {
            try
            {
                if (value == null) return (false, null);

                if (int.TryParse(value.ToString(), out var intValue) && !Enum.IsDefined(typeof(T), intValue)) return (false, null);
                if (!Enum.TryParse<T>(value.ToString(), true, out var result)) return (false, null);

                return (true, result);
            }
            catch
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Attempts to parse input value to valid enum value and throws if unsuccessful.
        /// </summary>
        /// <typeparam name="T">The type of enum to parse input value to.</typeparam>
        /// <param name="value">The value to attempt to parse to enum of type T</param>
        /// <returns>The parsed enum value as type T</returns>
        /// <exception cref="EnumParseException">Exception thrown when input value cannot be parsed</exception>
        public static T ParseOrThrow<T>(object value) where T : struct, Enum
        {
            var (isValid, result) = Parse<T>(value);
            if (!isValid || result == null) throw new EnumParseException($"Cannot parse '{value}' to valid {typeof(T).Name} enum value");

            return result.Value;
        }

        /// <summary>
        /// Returns the enum member as a string
        /// </summary>
        /// <param name="value">The enum member to attempt to return a string version of.</param>
        /// <returns>The enum member name as a string</returns>
        public static string ToName(object value)
        {
            return Enum.GetName(value.GetType(), value);
        }

        /// <summary>
        ///     Exception thrown when EnumParser is unable to parse input value to valid enum value
        /// </summary>
        [Serializable]
        public class EnumParseException : Exception
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="EnumParseException" /> class.
            /// </summary>
            public EnumParseException() { }

            /// <summary>
            ///     Initializes a new instance of the <see cref="EnumParseException" /> class and initializes the contained message.
            /// </summary>
            /// <param name="message">Text message to associate with the exception.</param>
            public EnumParseException(string message) : base(message) { }

            /// <summary>
            ///     Initializes a new instance of the <see cref="EnumParseException" /> class and initializes
            ///     the contained message and inner exception reference.
            /// </summary>
            /// <param name="message">Text message to associate with the exception.</param>
            /// <param name="inner">Exception to set as InnerException.</param>
            public EnumParseException(string message, Exception inner) : base(message, inner) { }

            /// <summary>
            ///     Initializes a new instance of the <see cref="EnumParseException" /> class with serialized data.
            /// </summary>
            /// <param name="info">Information on how to serialize a <see cref="EnumParseException" />.</param>
            /// <param name="context">Information on the streaming context for a <see cref="EnumParseException" />.</param>
            protected EnumParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}