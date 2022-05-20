namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using PRNGLib;

    /// <summary>
    ///     Command handler for the <see cref="Shuffle" /> command.
    /// </summary>
    public class ShuffleCommandHandler : ICommandHandler<Shuffle>
    {
        private readonly IPRNG _prng;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShuffleCommandHandler" /> class.
        /// </summary>
        /// <param name="rngFactory">An <see cref="IRandomFactory" /> implementation.</param>
        public ShuffleCommandHandler(IRandomFactory rngFactory)
        {
            if (rngFactory == null)
            {
                throw new ArgumentNullException(nameof(rngFactory));
            }

            _prng = rngFactory.Create(RandomType.Gaming);
        }

        /// <inheritdoc />
        public void Handle(Shuffle command)
        {
            var values = command.Values;
            var length = values.Count;

            for (var index = 0; index < length; index++)
            {
                var swapIndex = index + (int)_prng.GetValue((ulong)(length - index));

                var tmpValue = values[swapIndex];
                values[swapIndex] = values[index];
                values[index] = tmpValue;
            }
        }
    }
}
