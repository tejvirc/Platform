namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Get a random number command
    /// </summary>
    public class GetRandomNumber
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetRandomNumber" /> class.
        /// </summary>
        /// <param name="range">Max value to return.</param>
        public GetRandomNumber(ulong range)
        {
            Range = range;
        }

        /// <summary>
        ///     Gets the Max Value
        /// </summary>
        public ulong Range { get; }

        /// <summary>
        ///     Gets or sets the random value.
        /// </summary>
        public ulong Value { get; set; }
    }
}
