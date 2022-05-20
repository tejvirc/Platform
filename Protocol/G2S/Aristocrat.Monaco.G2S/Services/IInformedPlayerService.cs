namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Provides a mechanism to interact with the informed player service
    /// </summary>
    public interface IInformedPlayerService
    {
        /// <summary>
        ///     Control game play
        /// </summary>
        /// <param name="device">Controlling device</param>
        /// <param name="enabled">True to enable game play, false to disable it.</param>
        /// <param name="disableMessage">Display message while disabled.</param>
        /// <param name="internallyGenerated">True if this call is made internally (not from protocol handler).</param>
        void SetGamePlayState(IInformedPlayerDevice device, bool enabled, string disableMessage, bool internallyGenerated = false);

        /// <summary>
        ///     Control money in
        /// </summary>
        /// <param name="device">Controlling device</param>
        /// <param name="enabled">True to enable money in, false to disable it.</param>
        /// <param name="internallyGenerated">True if this call is made internally (not from protocol handler).</param>
        void SetMoneyInState(IInformedPlayerDevice device, bool enabled, bool internallyGenerated = false);

        /// <summary>
        ///     Handle a player session start.
        /// </summary>
        void OnPlayerSessionStarted();

        /// <summary>
        ///     Handle a player session end.
        /// </summary>
        void OnPlayerSessionEnded();
    }
}
