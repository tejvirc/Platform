namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Aristocrat.CryptoRng;
    using Contracts;   

    /// <summary>
    ///     Command handler for the <see cref="GetRandomNumber" /> command.
    /// </summary>
    public class GetRandomNumberCommandHandler : ICommandHandler<GetRandomNumber>
    {
        private readonly IRandom _prng;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetRandomNumberCommandHandler" /> class.
        /// </summary>
        /// <param name="rngFactory">An <see cref="IRandomFactory" /> implementation.</param>
        public GetRandomNumberCommandHandler(IRandomFactory rngFactory)
        {
            if (rngFactory == null)
            {
                throw new ArgumentNullException(nameof(rngFactory));
            }

            _prng = rngFactory.Create(RandomType.Gaming);
        }

        /// <inheritdoc />
        public void Handle(GetRandomNumber command)
        {
            command.Value = _prng.GetValue(command.Range);
        }
    }
}
