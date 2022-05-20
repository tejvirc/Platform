namespace Aristocrat.Monaco.G2S
{
    using Common.PackageManager.Storage;

    /// <summary>
    ///     Defines a contract for a script manager.
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        ///     Adds script entity to be run by the EGM.
        /// </summary>
        /// <param name="scriptEntity">Script Entity.</param>
        void AddScript(Script scriptEntity);

        /// <summary>
        ///     Removes script from persistence, any pending instances for the script will not be executed.
        /// </summary>
        /// <param name="scriptEntity">Script Entity.</param>
        void CancelScript(Script scriptEntity);

        /// <summary>
        ///     Attempts to process a script after receiving and authorization command from a host.
        /// </summary>
        /// <param name="scriptEntity">Script Entity.</param>
        void AuthorizeScript(Script scriptEntity);

        /// <summary>
        ///     Updates script status and checks for terminal conditions that need to send updates to hosts.
        /// </summary>
        /// <param name="scriptEntity">Script Entity.</param>
        /// <param name="sendStatus">Flag to send script status command.</param>
        void UpdateScript(Script scriptEntity, bool sendStatus = true);

        /// <summary>
        ///     Starts processing any waiting scripts.
        /// </summary>
        void Start();
    }
}