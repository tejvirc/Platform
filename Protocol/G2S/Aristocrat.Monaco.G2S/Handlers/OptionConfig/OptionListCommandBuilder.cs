namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;

    /// <summary>
    ///     The builder for the 'optionList' command.
    /// </summary>
    public class OptionListCommandBuilder : IOptionListCommandBuilder
    {
        private readonly IEnumerable<IDeviceOptionsBuilder> _builders;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionListCommandBuilder" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="builders">Builders.</param>
        public OptionListCommandBuilder(IG2SEgm egm, IEnumerable<IDeviceOptionsBuilder> builders)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _builders = builders ?? throw new ArgumentNullException(nameof(builders));
        }

        /// <inheritdoc />
        public Task Build(
            IOptionConfigDevice device,
            optionList command,
            OptionListCommandBuilderParameters parameters)
        {
            command.deviceOptions =
                _egm.Devices.Where(d => Filter(d, parameters))
                    .Select(d => DeviceOptions(d, parameters))
                    .ToArray();

            return Task.CompletedTask;
        }

        private static bool Filter(IDevice device, OptionListCommandBuilderParameters parameters)
        {
            if (!device.Active)
            {
                return false;
            }

            if (!parameters.IncludeAllDeviceClasses && !device.IsMatching(parameters.DeviceClass))
            {
                return false;
            }

            if (!parameters.IncludeAllDevices && !device.IsMatching(parameters.DeviceId))
            {
                return false;
            }

            return true;
        }

        private deviceOptions DeviceOptions(IDevice device, OptionListCommandBuilderParameters parameters)
        {
            var builder =
                _builders.FirstOrDefault(b => b.Matches(device.PrefixedDeviceClass().DeviceClassFromG2SString()));
            if (builder == null)
            {
                // throw new InvalidOperationException($"Missing builder for device class {device.PrefixedDeviceClass()}");
                return new deviceOptions
                {
                    deviceClass = device.PrefixedDeviceClass(),
                    deviceId = device.Id
                };
            }

            var options = builder.Build(device, parameters);

            options.optionGroup =
                options.optionGroup.Where(
                    g => parameters.IncludeAllGroups || g.optionGroupId.Equals(parameters.OptionGroupId)).ToArray();

            return options;
        }
    }
}
