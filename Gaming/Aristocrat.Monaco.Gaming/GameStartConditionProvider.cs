namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Implements the <see cref="IGameStartConditionProvider"/> interface.
    /// </summary>
    public class GameStartConditionProvider : IGameStartConditionProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly List<IGameStartCondition> _conditions = new();

        /// <inheritdoc />
        public void AddGameStartCondition(IGameStartCondition condition)
        {
            lock (_conditions)
            {
                _conditions.Add(condition);
            }
        }

        public void RemoveGameStartCondition(IGameStartCondition condition)
        {
            lock (_conditions)
            {
                _conditions.Remove(condition);
            }
        }

        /// <inheritdoc />
        public bool CheckGameStartConditions()
        {
            var retVal = true;
            lock (_conditions)
            {
                foreach (var gameStartCondition in _conditions)
                {
                    if (!gameStartCondition.CanGameStart())
                    {
                        Logger.Debug("Preventing game start because of " + gameStartCondition.GetType().Name);
                        retVal = false;
                    }
                }
            }

            return retVal;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameStartConditionProvider) };

        /// <inheritdoc />
        public void Initialize() { }
    }
}