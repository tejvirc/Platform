namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     SetBonusKey command
    /// </summary>
    public class SetBonusKey
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetBonusKey" /> class.
        /// </summary>
        /// <param name="poolName">The pool name</param>
        /// <param name="key">The key to determine the bonus amount to be used while awarding the jackpot</param>
        public SetBonusKey(string poolName, string key)
        {
            PoolName = poolName;
            Key = key;
        }

        /// <summary>
        ///     Gets PoolName
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        ///     Gets Key
        /// </summary>
        public string Key { get; }
    }
}
