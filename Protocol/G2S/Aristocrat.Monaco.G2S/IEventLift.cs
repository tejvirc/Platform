namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to raise a G2S event.
    /// </summary>
    public interface IEventLift
    {
        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        void Report(IDevice device, string eventCode);

        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        /// <param name="deviceList">Contains one or more statusInfo elements.</param>
        void Report(IDevice device, string eventCode, deviceList1 deviceList);

        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        /// <param name="transactionId">The associated transaction Id</param>
        /// <param name="transactionList">Contains one or more transactionInfo elements.</param>
        void Report(IDevice device, string eventCode, long transactionId, transactionList transactionList);

        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        /// <param name="transactionId">The associated transaction Id</param>
        /// <param name="transactionList">Contains one or more transactionInfo elements.</param>
        /// <param name="metersList">Contains one or more meterInfo elements.</param>
        void Report(
            IDevice device,
            string eventCode,
            long transactionId,
            transactionList transactionList,
            meterList metersList);

        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        /// <param name="deviceList">Contains one or more statusInfo elements.</param>
        /// <param name="metersList">Contains one or more meterInfo elements.</param>
        void Report(IDevice device, string eventCode, deviceList1 deviceList, meterList metersList);

        /// <summary>
        ///     Raises the specified the event.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="eventCode">The G2S event code.</param>
        /// <param name="deviceList">Contains one or more statusInfo elements.</param>
        /// <param name="transactionId">The associated transaction Id</param>
        /// <param name="transactionList">Contains one or more transactionInfo elements.</param>
        /// <param name="metersList">Contains one or more meterInfo elements.</param>
        void Report(
            IDevice device,
            string eventCode,
            deviceList1 deviceList,
            long transactionId,
            transactionList transactionList,
            meterList metersList);
    }
}