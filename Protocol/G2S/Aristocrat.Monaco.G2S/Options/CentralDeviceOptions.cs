﻿namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;

    public class CentralDeviceOptions : BaseDeviceOptions
    {
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Central;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);
        }
    }
}