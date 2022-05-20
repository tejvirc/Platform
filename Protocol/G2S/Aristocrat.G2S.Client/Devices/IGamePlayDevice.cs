namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with and control a gamePlay device.
    /// </summary>
    public interface IGamePlayDevice : IDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets a value indicating whether accessible games and denominations are set via commands within the optionConfig
        ///     class or commands within the gamePlay class
        /// </summary>
        bool SetViaAccessConfig { get; }

        /// <summary>
        ///     Gets whether denomination meters for the gamePlay device are reported in total as a single
        ///     denomination(G2S_oneDenom) or are reported for each denomination that is actually wagered (G2S_eachDenom)
        /// </summary>
        string DenomMeterType { get; }
    }
}