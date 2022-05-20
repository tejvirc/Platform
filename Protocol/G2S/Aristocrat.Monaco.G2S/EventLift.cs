namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of IEventLift that wraps the EventHandlerDevice.EventReport.
    /// </summary>
    public class EventLift : IEventLift
    {
        /// <inheritdoc />
        public void Report(IDevice device, string eventCode)
        {
            Report(device, eventCode, null);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList)
        {
            Report(device, eventCode, deviceList, null);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, long transactionId, transactionList transactionList)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                transactionId: transactionId,
                transactionList: transactionList);
        }

        /// <inheritdoc />
        public void Report(
            IDevice device,
            string eventCode,
            long transactionId,
            transactionList transactionList,
            meterList metersList)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                meterList: metersList,
                transactionId: transactionId,
                transactionList: transactionList);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList, meterList metersList)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                deviceList,
                meterList: metersList);
        }

        /// <inheritdoc />
        public void Report(
            IDevice device,
            string eventCode,
            deviceList1 deviceList,
            long transactionId,
            transactionList transactionList,
            meterList metersList)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                deviceList,
                meterList: metersList,
                transactionId: transactionId,
                transactionList: transactionList);
        }
    }
}