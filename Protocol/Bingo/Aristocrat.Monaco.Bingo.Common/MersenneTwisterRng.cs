namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// <c>
    ///     Implements the Mersenne Twister MT19937 PRNG based on data from the web
    ///     <para>
    ///         The coefficients for MT19937 are:
    ///         w: word size (in number of bits)
    ///         n: degree of recurrence
    ///         m: middle word, or the number of parallel sequences, 1 ≤ m ≤ n
    ///         r: separation point of one word, or the number of bits of
    ///            the lower bitmask, 0 ≤ r ≤ w - 1
    ///         (w, n, m, r) = (32, 624, 397, 31)
    ///         For details: http://en.wikipedia.org/wiki/Mersenne_twister
    ///     </para>
    /// </c>
    /// </summary>
    public class MersenneTwisterRng : IPseudoRandomNumberGenerator
    {
        /// <summary>
        ///     The degree of recurrence is the value that sets the number of samples
        ///     to be used in the generation of PRNG's
        /// </summary>
        private const uint RecurrenceDegree = 624;

        /// <summary>
        ///     the middle word is the word that lies halfway between
        ///     the recurrenceDegree in the array of
        ///     un-tempered values (samples)
        /// </summary>
        private const uint MiddleWord = 397;

        /// <summary>
        ///     A magic number that is found based on the data
        ///     described in this well known PRNG implementation
        /// </summary>
        private const uint MagicNumber = 1812433253;

        /// <summary>
        ///     A magic number that is found based on the data
        ///     described in this well known PRNG implementation
        /// </summary>
        private const uint MagicNumber2 = 0x9d2c5680;

        /// <summary>
        ///     A magic number that is found based on the data
        ///     described in this well known PRNG implementation
        /// </summary>
        private const uint MagicNumber3 = 0xefc60000;

        /// <summary>
        ///     This is what I would call a magic number that is found based on the data
        ///     described in this well known PRNG implementation
        /// </summary>
        private const uint MagicNumber4 = 0x9908b0df;

        /// <summary>
        ///     bitShift is the number of bits to be shifted when getting a PRNG sample
        /// </summary>
        private const int BitShift = 30;

        /// <summary>
        ///     wordSize is a pre-calculated value to indicate the number of bits per word of data
        /// </summary>
        private const uint WordSize = 0xFFFFFFFF;

        /// <summary>
        ///     the separationPoint is the lower 32 bits
        /// </summary>
        private const uint SeparationPoint = 0x7FFFFFFF;

        /// <summary>
        ///     bit32 is pre-calculated to define bit 32 for masking
        /// </summary>
        private const uint Bit32 = 0x80000000;

        /// <summary>
        ///     The current position into the generated number array of un-tempered values
        /// </summary>
        private int _currentPosition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MersenneTwisterRng" /> class.
        /// </summary>
        public MersenneTwisterRng()
        {
            GeneratorState = new uint[RecurrenceDegree];
        }

        /// <summary>
        ///     Gets or sets the values in the generatorState history for
        ///     use in future PRNG values being returned.
        /// </summary>
        private uint[] GeneratorState { get; }

        public string Name => "MersenneRNG";

        /// <summary>
        ///     Initializes the random number generator.
        /// </summary>
        /// <param name="seed">Sets the seed for the random number generator.</param>
        public void Seed(uint seed)
        {
            _currentPosition = 0;
            GeneratorState[_currentPosition] = seed;

            /*
             * the current position i.e. 0 is set to the seed value and then we
             * generate the next x values (recurrenceDegree - 1) to be random
             * based on the starting seed value
             * MT[i] := last 32 bits of(1812433253 * (MT[i-1] xor (right shift by 30 bits(MT[i-1]))) + i)
             */
            for (uint index = 1; index < RecurrenceDegree; index++)
            {
                GeneratorState[index] =
                    WordSize &
                    (MagicNumber * (GeneratorState[index - 1] ^
                                    (GeneratorState[index - 1] >> BitShift)) + index);
            }
        }

        /// <summary>
        ///     Calculates a random number between min and max.
        /// </summary>
        /// <param name="min">Sets the minimum value.</param>
        /// <param name="max">Sets the maximum value.</param>
        /// <returns>Returns an random integer between min and max.</returns>
        public uint Random(uint min, uint max)
        {
            var convField = new ConversionField();

            var result = ExtractNumber();
            convField.Lower = result << 20;
            convField.Upper = (result >> 12) | 0x3FF00000;
            convField.Value -= 1.0;
            var range = max - min + 1;
            result = (uint)(range * convField.Value);
            result = min + result % range;

            return result;
        }

        /// <summary>
        ///     Overrides the basic ToString to get more data regarding the RNG
        /// </summary>
        /// <returns>String representation of our RNG</returns>
        public override string ToString()
        {
            return "PRNG Name: Mersenne Twister" + Environment.NewLine;
        }

        /// <summary>
        ///     Extract a tempered pseudo random number based on the indexed value,
        ///     calling generate_numbers() every 624 numbers
        /// </summary>
        /// <returns>the next unsigned int to be used as random</returns>
        private uint ExtractNumber()
        {
            if (_currentPosition == 0)
            {
                GenerateNumbers();
            }

            var returnValue = GeneratorState[_currentPosition];

            returnValue ^= returnValue >> 11;
            returnValue ^= (returnValue << 7) & MagicNumber2;
            returnValue ^= (returnValue << 15) & MagicNumber3;
            returnValue ^= returnValue >> 18;

            _currentPosition = ++_currentPosition % (int)RecurrenceDegree;
            return returnValue;
        }

        /// <summary>
        ///     Generate an array of recurrenceDegree un-tempered numbers
        /// </summary>
        private void GenerateNumbers()
        {
            for (uint index = 0; index < RecurrenceDegree; index++)
            {
                var tempValue =
                    (Bit32 & GeneratorState[index]) +
                    (SeparationPoint & GeneratorState[(index + 1) % RecurrenceDegree]);

                GeneratorState[index] =
                    GeneratorState[(index + MiddleWord) % RecurrenceDegree] ^ (tempValue >> 1);

                // if tempValue is odd
                if (tempValue % 2 != 0)
                {
                    GeneratorState[index] = GeneratorState[index] ^ MagicNumber4;
                }
            }
        }

        /// <summary>
        ///      Defines the structure of an 8-byte layout
        ///      which is used to generate a random number
        ///      in a range.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct ConversionField
        {
            /// <summary>
            ///     Value is entire structure and is 8 bytes.
            /// </summary>
            [FieldOffset(0)]
            public double Value; // map a double to two uints

            /// <summary>
            ///     The lower 4 bytes
            /// </summary>
            [FieldOffset(0)]
            public uint Lower;

            /// <summary>
            ///     The upper 4 bytes
            /// </summary>
            [FieldOffset(4)]
            public uint Upper;
        }
    }
}