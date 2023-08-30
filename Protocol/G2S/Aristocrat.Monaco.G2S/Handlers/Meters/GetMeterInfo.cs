namespace Aristocrat.Monaco.G2S.Handlers.Meters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Meters;
    using Gaming.Contracts.Progressives;
    using Services;

    /// <summary>
    ///     Handles the v21.getMeterInfo G2S message
    /// </summary>
    public class GetMeterInfo : ICommandHandler<meters, getMeterInfo>
    {
        private readonly IG2SEgm _egm;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;
        private readonly IProgressiveLevelManager _progressiveLevelManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetMeterInfo" /> class.
        ///     Creates a new instance of the GetMeterInfo handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="metersSubscriptionManager">Meters subscription manager</param>
        public GetMeterInfo(
            IG2SEgm egm,
            IMetersSubscriptionManager metersSubscriptionManager,
            IProgressiveLevelManager progressiveLevelManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));
            _progressiveLevelManager = progressiveLevelManager ?? throw new ArgumentNullException(nameof(progressiveLevelManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<meters, getMeterInfo> command)
        {
            return await Sanction.OwnerAndGuests<IMetersDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<meters, getMeterInfo> command)
        {
            var cmd = command.Command;
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var meterInfo = new meterInfo();
                var error = _metersSubscriptionManager.GetMeters(cmd, meterInfo);
                if (error == ErrorCode.G2S_none)
                {
                    var response = command.GenerateResponse<meterInfo>();
                    response.Command.meterDateTime = DateTime.UtcNow;
                    response.Command.meterInfoType = cmd.meterInfoType;
                    response.Command.gameDenomMeters = meterInfo.gameDenomMeters;
                    response.Command.currencyMeters = meterInfo.currencyMeters;

                    if(cmd.getDeviceMeters != null)
                    { 
                        var progIndex = Array.FindIndex(cmd.getDeviceMeters, q =>
                        q.deviceClass.Equals(DeviceClass.G2S_progressive, StringComparison.Ordinal));
                        response.Command.deviceMeters = progIndex >= 0 ?
                            GetProgressiveDeviceMeters(cmd.getDeviceMeters[progIndex].deviceId) :
                            meterInfo.deviceMeters;
                    }

                    response.Command.wagerMeters = meterInfo.wagerMeters;
                }
                else
                {
                    command.Error.Code = error;
                }
            }

            await Task.CompletedTask;
        }

        // This is required because somebody created a ProgressiveMeterManager
        // It's not tied into the same mechanisms
        private deviceMeters[] GetProgressiveDeviceMeters(int progDeviceId)
        {
            if (_progressiveLevelManager == null) return null;

            return new[]
                    {
                            new deviceMeters
                            {
                                deviceClass = DeviceClass.G2S_progressive,
                                deviceId = progDeviceId,
                                simpleMeter = _progressiveLevelManager.GetProgressiveDeviceMeters(
                                    progDeviceId,
                                    ProgressiveMeters.LinkedProgressiveWageredAmount,
                                    ProgressiveMeters.LinkedProgressivePlayedCount).ToArray()
                            }
                        };
        }
    }
}