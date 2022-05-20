namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Gaming.Contracts.Bonus;
    using Params = Aristocrat.G2S.Client.Devices.v21.G2SParametersNames.BonusDevice;

    public class BonusDeviceOptions : BaseDeviceOptions
    {
        private readonly IBonusHandler _bonusHandler;

        public BonusDeviceOptions(IBonusHandler bonusHandler)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
        }

        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Bonus;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            if (optionConfigValues.HasValue(Params.MaxPendingBonusParameterName))
            {
                _bonusHandler.MaxPending = optionConfigValues.Int32Value(Params.MaxPendingBonusParameterName);
            }
        }
    }
}