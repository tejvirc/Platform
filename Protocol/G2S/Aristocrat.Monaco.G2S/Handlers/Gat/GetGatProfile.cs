namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetGatProfile : ICommandHandler<gat, getGatProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGatProfile" /> class.
        ///     Constructs a new instance using an egm.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        public GetGatProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getGatProfile> command)
        {
            return await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getGatProfile> command)
        {
            var device = _egm.GetDevice<IGatDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<gatProfile>();
            var profile = response.Command;
            profile.configurationId = device.ConfigurationId;
            profile.configComplete = device.ConfigComplete;
            profile.specialFunctions = device.SpecialFunctions;
            profile.idReaderId = device.IdReaderId;
            if (device.MinLogEntries > 0)
            {
                profile.minLogEntries = device.MinLogEntries;
            }

            if (device.MinQueuedComps > 0)
            {
                profile.minQueuedComps = device.MinQueuedComps;
            }

            profile.timeToLive = device.TimeToLive;
            profile.useDefaultConfig = device.UseDefaultConfig;
            if (device.ConfigDateTime != default(DateTime))
            {
                profile.configDateTime = device.ConfigDateTime;
                profile.configDateTimeSpecified = true;
            }

            await Task.CompletedTask;
        }
    }
}