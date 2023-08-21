namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Shuffle a list of ulongs command
    /// </summary>
    public class Shuffle
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Shuffle" /> class.
        /// </summary>
        /// <param name="values">List of values to shuffle.</param>
        public Shuffle(List<ulong> values)
        {
            Values = values;
        }

        /// <summary>
        ///     Gets or sets the random value.
        /// </summary>
        public List<ulong> Values { get; set; }
    }
}
