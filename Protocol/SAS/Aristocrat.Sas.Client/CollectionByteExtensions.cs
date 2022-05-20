namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Extensions for Collection of type byte
    /// </summary>
    public static class CollectionByteExtensions
    {
        /// <summary>
        ///     Adds an array of byte to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of type byte</param>
        /// <param name="input">An array of type byte</param>
        public static void Add(this Collection<byte> collection, byte[] input)
        {
            foreach (var i in input)
            {
                collection.Add(i);
            }
        }

        /// <summary>
        ///     Adds a string to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of byte</param>
        /// <param name="input">An IEnumerable of type byte</param>
        public static void Add(this Collection<byte> collection, IEnumerable<byte> input)
        {
            collection.Add(input.ToArray());
        }

        /// <summary>
        ///     Adds a string to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of type byte</param>
        /// <param name="input">A string</param>
        public static void Add(this Collection<byte> collection, string input)
        {
            collection.Add(Encoding.ASCII.GetBytes(input));
        }

        /// <summary>
        ///     Adds a char to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of type byte</param>
        /// <param name="input">A char</param>
        public static void Add(this Collection<byte> collection, char input)
        {
            collection.Add((byte)input);
        }

        /// <summary>
        ///     Adds an int as a single byte to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of type byte</param>
        /// <param name="input">An int</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not in a byte's range</exception>
        public static void Add(this Collection<byte> collection, int input)
        {
            if (input > 0xFF)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, "Value is more than max byte");
            }

            if (input < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(input), input, "Value is less than min byte");
            }

            collection.Add((byte)input);
        }

        /// <summary>
        ///     Adds an Enum as a single byte to the end of the Collection of type byte.
        /// </summary>
        /// <param name="collection">The Collection of type byte</param>
        /// <param name="input">An Enum</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is more than one byte's maximum value</exception>
        /// <remarks>Uses int extension to verify byte value</remarks>
        public static void Add(this Collection<byte> collection, Enum input)
        {
            // Add<int> verifies byte value
            collection.Add((int)Convert.ChangeType(input, typeof(int)));
        }
    }
}