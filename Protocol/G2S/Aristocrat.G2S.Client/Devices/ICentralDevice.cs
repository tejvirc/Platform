namespace Aristocrat.G2S.Client.Devices
{
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a Central device.
    /// </summary>
    public interface ICentralDevice : IDevice, IRestartStatus, ITimeToLive, INoResponseTimer
    {
        /// <summary>
        ///     Used to report that an outcome has been received
        /// </summary>
        /// <returns></returns>
        Task<centralOutcome> GetOutcome(getCentralOutcome outcome);

        /// <summary>
        ///     Used to report that an outcome has been received
        /// </summary>
        /// <returns></returns>
        Task<bool> CommitOutcome(commitOutcome outcome);
    }
}