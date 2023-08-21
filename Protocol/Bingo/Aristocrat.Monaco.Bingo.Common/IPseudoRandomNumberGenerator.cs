namespace Aristocrat.Monaco.Bingo.Common
{
    public interface IPseudoRandomNumberGenerator
    {
        /// <summary>
        ///    Gets the name of the PRNG for various tools to extract and receive
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Initializes the random number generator.
        /// </summary>
        /// <param name="seed">Sets the seed for the random number generator.</param>
        void Seed(uint seed);

        /// <summary>
        ///     Calculates a random number between min and max.
        /// </summary>
        /// <param name="min">Sets the minimum value.</param>
        /// <param name="max">Sets the maximum value.</param>
        /// <returns>Returns an random integer between min and max.</returns>
        uint Random(uint min, uint max);
    }
}