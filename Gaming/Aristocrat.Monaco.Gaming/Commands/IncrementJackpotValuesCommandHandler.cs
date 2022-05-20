namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Progressives;

    /// <summary>
    ///     Command handler for the <see cref="IncrementJackpotValues" /> command.
    /// </summary>
    public class IncrementJackpotValuesCommandHandler : ICommandHandler<IncrementJackpotValues>
    {
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IGameRecovery _recovery;
        private readonly IPersistentStorageManager _storage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IncrementJackpotValuesCommandHandler" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        public IncrementJackpotValuesCommandHandler(
            IProgressiveGameProvider progressiveGame,
            IGameRecovery recovery,
            IPersistentStorageManager storage)
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <inheritdoc />
        public void Handle(IncrementJackpotValues command)
        {
            if (!command.Values.Any())
            {
                return;
            }

            using (var scope = _storage.ScopedTransaction())
            {
                _progressiveGame.IncrementProgressiveLevelPack(command.PoolName, CreateIncrement(command).ToArray());

                scope.Complete();
            }
        }

        private IEnumerable<ProgressiveLevelUpdate> CreateIncrement(IncrementJackpotValues increment)
        {
            foreach (var value in increment.Values)
            {
                yield return new ProgressiveLevelUpdate(
                    (int)value.Key,
                    ((long)value.Value.Cents).CentsToMillicents(),
                    ((long)value.Value.Fraction).CentsToMillicents(),
                    _recovery.IsRecovering);
            }
        }
    }
}