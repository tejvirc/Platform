namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Common.Events;
    using Aristocrat.Monaco.Kernel;
    using System;

    /// <summary>
    ///     An implementation of IEventLift that wraps the EventHandlerDevice.EventReport.
    /// </summary>
    public class EventLift : IEventLift
    {
        private readonly IEventBus _eventBus;

        public EventLift(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode)
        {
            Report(device, eventCode, null, associatedEvent: null);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, IEvent associatedEvent)
        {
            Report(device, eventCode, null, associatedEvent);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList)
        {
            Report(device, eventCode, deviceList, null, null);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList, IEvent associatedEvent)
        {
            Report(device, eventCode, deviceList, null, associatedEvent);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, long transactionId, transactionList transactionList)
        {
            Report(device, eventCode, transactionId, transactionList, associatedEvent: null);
        }

        public void Report(IDevice device, string eventCode, long transactionId, transactionList transactionList, IEvent associatedEvent)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                transactionId: transactionId,
                transactionList: transactionList);

            DispatchG2SEventCodeSentEvent(eventCode, associatedEvent);
        }

        /// <inheritdoc />
        public void Report(
            IDevice device,
            string eventCode,
            long transactionId,
            transactionList transactionList,
            meterList metersList)
        {
            Report(device, eventCode, transactionId, transactionList, metersList, null);
        }

        /// <inheritdoc />
        public void Report(
            IDevice device,
            string eventCode,
            long transactionId,
            transactionList transactionList,
            meterList metersList,
            IEvent associatedEvent)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                meterList: metersList,
                transactionId: transactionId,
                transactionList: transactionList);

            DispatchG2SEventCodeSentEvent(eventCode, associatedEvent);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList, meterList metersList)
        {
            Report(device, eventCode, deviceList, metersList, null);
        }

        /// <inheritdoc />
        public void Report(IDevice device, string eventCode, deviceList1 deviceList, meterList metersList, IEvent associatedEvent)
        {
            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventCode,
                deviceList,
                meterList: metersList);

            DispatchG2SEventCodeSentEvent(eventCode, associatedEvent);
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
            Report(device, eventCode, deviceList, transactionId, transactionList, metersList, null);
        }

        /// <inheritdoc />
        public void Report(
            IDevice device,
            string eventCode,
            deviceList1 deviceList,
            long transactionId,
            transactionList transactionList,
            meterList metersList,
            IEvent associatedEvent)
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

        private void DispatchG2SEventCodeSentEvent(string eventCode, IEvent associatedEvent)
        {
            if (string.IsNullOrEmpty(eventCode))
            {
                return;
            }

            G2SEvent @event = new G2SEvent(eventCode, associatedEvent);
            _eventBus.Publish<G2SEvent>(@event);
        }
    }
}