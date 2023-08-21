namespace Aristocrat.Monaco.Sas.ChangeRequests
{
    using Gaming.Contracts;
    using Kernel;
    using Contracts.Client;

    /// <summary>
    ///     Class that contains the information for a game enable request
    /// </summary>
    public class GameEnableRequest : ISasChangeRequest
    {
        private readonly int _gameId;
        private readonly bool _enable;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Constructs the GameEnableRequest object
        /// </summary>
        public GameEnableRequest(int id, bool enable)
        {
            _gameId = id;
            _enable = enable;
            _gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
        }

        /// <inheritdoc/>
        public ChangeType Type => ChangeType.GameOrPaytable;

        /// <inheritdoc/>
        public void Commit()
        {
            if (_enable)
            {
                _gameProvider.EnableGame(_gameId, GameStatus.DisabledByBackend);
            }
            else
            {
                _gameProvider.DisableGame(_gameId, GameStatus.DisabledByBackend);
            }
        }

        /// <inheritdoc/>
        public void Cancel()
        {
        }
    }
}
