namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using Data.Profile;
    using Handlers;
    using Options;

    /// <inheritdoc />
    public class ApplyOptionConfigToDevicesService : IApplyOptionConfigService
    {
        private readonly IEnumerable<IDeviceOptions> _deviceOptions;
        private readonly IG2SEgm _egm;
        private readonly IProfileService _profileService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplyOptionConfigToDevicesService" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="profileService">The profile service.</param>
        /// <param name="deviceOptions">Device options components.</param>
        public ApplyOptionConfigToDevicesService(
            IG2SEgm egm,
            IProfileService profileService,
            IEnumerable<IDeviceOptions> deviceOptions)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _deviceOptions = deviceOptions ?? throw new ArgumentNullException(nameof(deviceOptions));
        }

        /// <inheritdoc />
        public void UpdateDeviceProfiles(ChangeOptionConfigRequest changeOptionConfigRequest)
        {
            if (changeOptionConfigRequest == null)
            {
                throw new ArgumentNullException(nameof(changeOptionConfigRequest));
            }

            foreach (var option in changeOptionConfigRequest.Options)
            {
                var device = _egm.GetDevice(option.DeviceClass.TrimmedDeviceClass(), option.DeviceId);

                if (device == null)
                {
                    continue;
                }

                ApplyProperties(changeOptionConfigRequest.ConfigurationId, option, device);
            }
        }

        private void ApplyProperties(long configurationId, Option option, IDevice device)
        {
            var deviceOptionConfigValues = new DeviceOptionConfigValues(configurationId);
            AddKeyValuePair(deviceOptionConfigValues, option);

            var options =
                _deviceOptions.FirstOrDefault(
                    opt => opt.Matches(device.PrefixedDeviceClass().DeviceClassFromG2SString()));

            if (options == null)
            {
                throw new InvalidOperationException(
                    $"Does not have IDeviceOptions component for device class {device.DeviceClass}");
            }

            options.ApplyProperties(device, deviceOptionConfigValues);
            _profileService.Save(device);
        }

        private void AddKeyValuePair(DeviceOptionConfigValues deviceOptionConfigValues, Option option)
        {
            if (!option.OptionValues.Any())
            {
                return;
            }

            if (option.OptionValues.Length == 1 &&
                !option.OptionId.EndsWith("Table", StringComparison.InvariantCulture))
            {
                // single value
                var optionValue = option.OptionValues.First();
                AddKeyValuePair(deviceOptionConfigValues, optionValue);
            }
            else
            {
                // table value
                var valueRows = option.OptionValues.Select(
                    opt =>
                        new DeviceOptionTableRow(
                            opt.ChildValues.Select(val => new DeviceOptionConfigValue(val.ParamId, val.Value))));

                deviceOptionConfigValues.AddOption(
                    option.OptionValues.First().ParamId,
                    new DeviceOptionConfigValue(valueRows));
            }
        }

        private void AddKeyValuePair(DeviceOptionConfigValues deviceOptionConfigValues, OptionCurrentValue optionValue)
        {
            if (optionValue.ParameterType != OptionConfigParameterType.Complex)
            {
                deviceOptionConfigValues.AddOption(
                    optionValue.ParamId,
                    new DeviceOptionConfigValue(optionValue.ParamId, optionValue.Value));
            }
            else
            {
                if (optionValue.ChildValues != null)
                {
                    foreach (var childValue in optionValue.ChildValues)
                    {
                        AddKeyValuePair(deviceOptionConfigValues, childValue);
                    }
                }
            }
        }
    }
}