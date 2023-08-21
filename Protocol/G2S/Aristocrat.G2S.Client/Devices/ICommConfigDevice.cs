namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with and control a commConfig device.
    /// </summary>
    /// <typeparam name="THostList">Version specific commHostList</typeparam>
    /// <typeparam name="TCommChangeStatus">Version specific commChangeStatus</typeparam>
    public interface ICommConfigDevice<in THostList, in TCommChangeStatus> : IDevice, ISingleDevice, INoResponseTimer,
        ITransactionLogProvider
    {
        /// <summary>
        ///     Sends the list of updated hosts to the commConfig host
        /// </summary>
        /// <param name="hostList">The list of affected hosts</param>
        void UpdateHostList(THostList hostList);

        /// <summary>
        ///     Used to report the status of a set of changes.
        /// </summary>
        /// <param name="changeStatus">The list of affected hosts</param>
        /// <remarks>This should only be generated when the status of a set of changes has been updated to its terminal state</remarks>
        void CommChangeStatus(TCommChangeStatus changeStatus);
    }
}