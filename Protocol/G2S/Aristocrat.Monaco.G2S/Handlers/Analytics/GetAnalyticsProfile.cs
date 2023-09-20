namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getAnalyticsProfile G2S message
    /// </summary>
    public class GetAnalyticsProfile : ICommandHandler<analytics, getAnalyticsProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetAnalyticsProfile" /> class.
        ///     Creates a new instance of the GetAnalyticsProfile handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetAnalyticsProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<analytics, getAnalyticsProfile> command)
        {
            return await Sanction.OwnerAndGuests<IAnalyticsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public Task Handle(ClassCommand<analytics, getAnalyticsProfile> command)
        {
            var device = _egm.GetDevice<IAnalyticsDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return Task.CompletedTask;
            }

            var response = command.GenerateResponse<analyticsProfile>();
            var profile = response.Command;

            profile.configurationId = device.ConfigurationId;
            profile.restartStatus = device.RestartStatus;
            profile.requiredForPlay = device.RequiredForPlay;
            profile.timeToLive = device.TimeToLive;
            profile.noResponseTimer = device.NoResponseTimer;
            profile.noMessageTimer = device.NoMessageTimer;
            profile.noHostText = device.NoHostText;

            return Task.CompletedTask;
        }
    }
}
