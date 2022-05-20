namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;

    /// <inheritdoc />
    public class EventHandlerDeviceOptions : BaseDeviceOptions
    {
        private const string ForcedSubscriptionParameterName = "G2S_forcedSubscription";

        private const string DeviceIdParameterName = "G2S_deviceId";

        private const string DeviceClassParameterName = "G2S_deviceClass";

        private const string EventCodeParameterName = "G2S_eventCode";

        private const string ForceDeviceStatusParameterName = "G2S_forceDeviceStatus";

        private const string ForceTransactionParameterName = "G2S_forceTransaction";

        private const string ForceClassMetersParameterName = "G2S_forceClassMeters";

        private const string ForceDeviceMetersParameterName = "G2S_forceDeviceMeters";

        private const string ForcePersistParameterName = "G2S_forcePersist";

        private const string ForceUpdatableMetersParameterName = "G2S_forceUpdatableMeters";

        private readonly IEventPersistenceManager _eventPersistenceManager;

        private readonly string[] _parameters =
        {
            DeviceIdParameterName, DeviceClassParameterName,
            EventCodeParameterName, ForceDeviceStatusParameterName,
            ForceTransactionParameterName, ForceClassMetersParameterName,
            ForceDeviceMetersParameterName, ForcePersistParameterName,
            ForceUpdatableMetersParameterName
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventHandlerDeviceOptions" /> class.
        /// </summary>
        /// <param name="eventPersistenceManager">The event persistence manager.</param>
        public EventHandlerDeviceOptions(IEventPersistenceManager eventPersistenceManager)
        {
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentNullException(nameof(eventPersistenceManager));
        }

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.EventHandler;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            ApplyForcedSubscriptionParameters(device, optionConfigValues);
        }

        private void ApplyForcedSubscriptionParameters(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasTableValue(ForcedSubscriptionParameterName))
            {
                var table = optionConfigValues.GetTableValue(ForcedSubscriptionParameterName);

                foreach (var tableRow in table)
                {
                    if (ExistAllParameters(tableRow))
                    {
                        var forcedSubscription = CreateForcedSubscription(tableRow);

                        _eventPersistenceManager.AddForcedEvent(
                            tableRow.GetDeviceOptionConfigValue(EventCodeParameterName).StringValue(),
                            forcedSubscription,
                            device.Id,
                            forcedSubscription.deviceId);
                    }
                }
            }
        }

        private forcedSubscription CreateForcedSubscription(DeviceOptionTableRow tableRow)
        {
            var forcedSubscription = new forcedSubscription
            {
                deviceId =
                    tableRow.GetDeviceOptionConfigValue(DeviceIdParameterName).Int32Value(),
                deviceClass =
                    tableRow.GetDeviceOptionConfigValue(DeviceClassParameterName).StringValue(),
                eventCode =
                    tableRow.GetDeviceOptionConfigValue(EventCodeParameterName).StringValue(),
                forceDeviceStatus =
                    tableRow.GetDeviceOptionConfigValue(ForceDeviceStatusParameterName).BooleanValue(),
                forceTransaction =
                    tableRow.GetDeviceOptionConfigValue(ForceTransactionParameterName).BooleanValue(),
                forceClassMeters =
                    tableRow.GetDeviceOptionConfigValue(ForceClassMetersParameterName).BooleanValue(),
                forceDeviceMeters =
                    tableRow.GetDeviceOptionConfigValue(ForceDeviceMetersParameterName).BooleanValue(),
                forcePersist =
                    tableRow.GetDeviceOptionConfigValue(ForcePersistParameterName).BooleanValue(),
                forceUpdatableMeters =
                    tableRow.GetDeviceOptionConfigValue(ForceUpdatableMetersParameterName).BooleanValue()
            };

            return forcedSubscription;
        }

        private bool ExistAllParameters(DeviceOptionTableRow tableRow)
        {
            foreach (var parameter in _parameters)
            {
                if (!tableRow.HasValue(parameter))
                {
                    return false;
                }
            }

            return true;
        }
    }
}