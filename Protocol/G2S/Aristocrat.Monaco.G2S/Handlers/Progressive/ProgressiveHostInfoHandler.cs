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
    using Aristocrat.Monaco.G2S.Services;
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

            List<ProgressiveLevel> egmLevels = _progressives.GetProgressiveLevels().Where(p => p.DeviceId == device.Id).ToList();
            var progressiveService = ServiceManager.GetInstance().TryGetService<IProgressiveService>();
            if(progressiveService == null) return;

            foreach (progressiveLevel hostLevel in command.Command.progressiveLevel)
            {
                foreach(ProgressiveLevel p in egmLevels)
                {
                    if(p.ProgressiveId == hostLevel.progId)
                    {
                        p.LevelId = progressiveService.LevelIds.GetMonacoProgressiveLevelId(p.GameId, hostLevel.progId, hostLevel.levelId);
                        p.LevelType = G2STypeToPlatformType(hostLevel.levelType);                 
                        p.ResetValue = hostLevel.resetValue;
                        p.IncrementRate = hostLevel.incrementRate;

                        List<long> denoms = new List<long>();
                        denoms.Add(hostLevel.rangeLow.denomId);
                        denoms.Add(hostLevel.rangeHigh.denomId);
                        p.Denomination = denoms.ToArray();

                        List<ProgressiveLevel> levelsToUpdate = new List<ProgressiveLevel>();
                        levelsToUpdate.Add(p);

                        _progressives.UpdateProgressiveLevels(p.ProgressivePackName, p.GameId, p.Denomination.ElementAt(0), levelsToUpdate);
                    }
                }
            }

            await Task.CompletedTask;
        }

        private ProgressiveLevelType G2STypeToPlatformType(string G2SType)
        {
            ProgressiveLevelType type = ProgressiveLevelType.Unknown;

            if (G2SType ==  Constants.ManufacturerPrefix + "_LP")
            {
                type = ProgressiveLevelType.LP;
            }
            else if (G2SType == Constants.ManufacturerPrefix + "_Sap")
            {
                type = ProgressiveLevelType.Sap;
            }
            else if (G2SType == Constants.ManufacturerPrefix + "_Selectable")
            {
                type = ProgressiveLevelType.Selectable;
            }

                return type;
        }
    }
}
