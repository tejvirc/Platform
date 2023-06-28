namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     Handles the v21.progressiveHostInfo G2S command
    /// </summary>
    public class ProgressiveHostInfoHandler : ICommandHandler<progressive, progressiveHostInfo>
    {
        private readonly IG2SEgm _egm;
        private readonly IProgressiveLevelProvider _progressives;

        public ProgressiveHostInfoHandler(
            IG2SEgm egm,
            IProgressiveLevelProvider progressives)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _progressives = progressives ?? throw new ArgumentNullException(nameof(progressives));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, progressiveHostInfo> command)
        {
            return await Sanction.OwnerAndGuests<IProgressiveDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<progressive, progressiveHostInfo> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.Class.deviceId);
            if (device == null)
            {
                return;
            }

            var egmLevels = _progressives.GetProgressiveLevels().Where(p => p.DeviceId == device.Id).ToList();

            foreach (var hostLevel in command.Command.progressiveLevel)
            {
                foreach(var p in egmLevels)
                {
                    if (p.ProgressiveId != hostLevel.progId) {continue;}

                    p.LevelType = G2STypeToPlatformType(hostLevel.levelType);
                    p.ResetValue = hostLevel.resetValue;
                    p.IncrementRate = hostLevel.incrementRate;

                    var denoms = new List<long>();
                    denoms.Add(hostLevel.rangeLow.denomId);
                    denoms.Add(hostLevel.rangeHigh.denomId);
                    p.Denomination = denoms.ToArray();

                    _progressives.UpdateProgressiveLevels(
                        p.ProgressivePackName,
                        p.GameId,
                        p.Denomination.ElementAt(0),
                        new List<ProgressiveLevel>() { p });

                }
            }

            await Task.CompletedTask;
        }

        private ProgressiveLevelType G2STypeToPlatformType(string g2SType)
        {
            var type = g2SType switch
            {
                Constants.ProgressiveTypeLinked => ProgressiveLevelType.LP,
                Constants.ProgressiveTypeSap => ProgressiveLevelType.Sap,
                Constants.ProgressiveTypeSelectable => ProgressiveLevelType.Selectable,
                _ => ProgressiveLevelType.Unknown
            };

            return type;
        }
    }
}
