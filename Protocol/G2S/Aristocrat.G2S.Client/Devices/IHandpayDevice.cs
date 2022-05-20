namespace Aristocrat.G2S.Client.Devices
{
    using Protocol.v21;
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides a mechanism to interact with the handpay device
    /// </summary>
    public interface IHandpayDevice : IDevice, ISingleDevice, IRestartStatus, ITimeToLive, IIdReaderId, ITransactionLogProvider
    {
        /// <summary>
        ///     Send handpay request to host
        /// </summary>
        /// <param name="command"><see cref="handpayRequest"/> command</param>
        Task<bool> Request(handpayRequest command);

        /// <summary>
        ///     send handpay keyed off notice to host
        /// </summary>
        /// <param name="command"><see cref="keyedOff"/> command</param>
        /// <returns></returns>
        Task<bool> KeyedOff(keyedOff command);
    }
}
