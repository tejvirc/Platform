namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;

    /// <summary>
    ///     Implementation of 'getOptionSeries' command of 'OptionConfig' G2S class.
    /// </summary>
    public class GetOptionSeries : ICommandHandler<optionConfig, getOptionSeries>
    {
        private readonly IEnumerable<IDeviceOptionsBuilder> _builders;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetOptionSeries" /> class.
        /// </summary>
        /// <param name="egm">The egm.</param>
        /// <param name="builders">The builders.</param>
        public GetOptionSeries(IG2SEgm egm, IEnumerable<IDeviceOptionsBuilder> builders)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _builders = builders ?? throw new ArgumentNullException(nameof(builders));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, getOptionSeries> command)
        {
            var error = await Sanction.OwnerAndGuests<IOptionConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            // If the specified deviceClass is not supported by the EGM, the EGM MUST include
            // error code G2S_OCX001 Invalid Device Class / Device Identifier in its response.
            if (!_egm.Devices.Where(x => x.DeviceClass == command.Command.deviceClass.TrimmedDeviceClass())
                .Any(x => x.Active))
            {
                return new Error(ErrorCode.G2S_OCX001);
            }

            return await Task.FromResult<Error>(null);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, getOptionSeries> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                await Task.CompletedTask;
                return;
            }

            var response = command.GenerateResponse<optionList>();

            var parameters = new OptionListCommandBuilderParameters
            {
                IncludeAllGroups = command.Command.optionGroupId.Equals(DeviceClass.G2S_all),
                IncludeAllOptions = command.Command.optionId.Equals(DeviceClass.G2S_all),
                IncludeDetails = command.Command.optionDetail,
                DeviceClass = command.Command.deviceClass,
                OptionGroupId = command.Command.optionGroupId,
                OptionId = command.Command.optionId,
                DeviceId = command.HostId
            };

            var query =
                _egm.Devices.Where(
                        d =>
                            d.Id >= command.Command.startingDevice && d.Active
                            && d.DeviceClass ==
                            command.Command.deviceClass.TrimmedDeviceClass())
                    .OrderBy(d => d.Id)
                    .Take(command.Command.maximumDevices);

            var deviceOptions = query.Select(d => CreateDeviceOptions(d, parameters));

            response.Command.deviceOptions = deviceOptions.ToArray();
        }

        private deviceOptions CreateDeviceOptions(IDevice device, OptionListCommandBuilderParameters parameters)
        {
            var builder =
                _builders.FirstOrDefault(b => b.Matches(device.PrefixedDeviceClass().DeviceClassFromG2SString()));
            return builder?.Build(device, parameters);
        }
    }
}