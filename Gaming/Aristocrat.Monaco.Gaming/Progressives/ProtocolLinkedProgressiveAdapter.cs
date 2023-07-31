namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts.Protocol;
    using Common;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Kernel;

    public class ProtocolLinkedProgressiveAdapter : IProtocolLinkedProgressiveAdapter, IService
    {
        private readonly ILinkedProgressiveProvider _linkedProgressiveProvider;
        private readonly IProgressiveConfigurationProvider _progressiveConfigurationProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;

        private string _progressiveProtocol;

        public ProtocolLinkedProgressiveAdapter(
            ILinkedProgressiveProvider linkedProgressiveProvider,
            IProgressiveConfigurationProvider progressiveConfigurationProvider,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            IProgressiveGameProvider progressiveGameProvider)
        {
            _linkedProgressiveProvider = linkedProgressiveProvider ?? throw new ArgumentNullException(nameof(linkedProgressiveProvider));
            _progressiveConfigurationProvider = progressiveConfigurationProvider ?? throw new ArgumentNullException(nameof(progressiveConfigurationProvider));
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider ?? throw new ArgumentNullException(nameof(multiProtocolConfigurationProvider));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
        }

        public void ReportLinkDown(string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.ReportLinkDown(protocolName);
            }
        }

        public void ReportLinkUp(string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.ReportLinkUp(protocolName);
            }
        }

        public void UpdateLinkedProgressiveLevels(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(levelUpdates);
            }
        }

        public Task UpdateLinkedProgressiveLevelsAsync(IReadOnlyCollection<IViewableLinkedProgressiveLevel> levelUpdates, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                return _linkedProgressiveProvider.UpdateLinkedProgressiveLevelsAsync(levelUpdates);
            }

            return null;
        }

        public IReadOnlyCollection<IViewableLinkedProgressiveLevel> ViewLinkedProgressiveLevels()
        {
            return _linkedProgressiveProvider.ViewLinkedProgressiveLevels();
        }

        public bool ViewLinkedProgressiveLevel(string levelName, out IViewableLinkedProgressiveLevel level)
        {
            return _linkedProgressiveProvider.ViewLinkedProgressiveLevel(levelName, out level);
        }

        public bool ViewLinkedProgressiveLevels(IEnumerable<string> levelNames, out IReadOnlyCollection<IViewableLinkedProgressiveLevel> levels)
        {
            return _linkedProgressiveProvider.ViewLinkedProgressiveLevels(levelNames, out levels);
        }

        public void ClaimLinkedProgressiveLevel(string levelName, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.ClaimLinkedProgressiveLevel(levelName);
            }
        }

        public void AwardLinkedProgressiveLevel(string levelName, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.AwardLinkedProgressiveLevel(levelName);
            }
        }

        public void AwardLinkedProgressiveLevel(string levelName, long winAmount, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.AwardLinkedProgressiveLevel(levelName, winAmount);
            }
        }

        public IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels()
        {
            return _progressiveConfigurationProvider.ViewProgressiveLevels();
        }

        public IEnumerable<IViewableProgressiveLevel> ViewProgressiveLevels(int gameId, long denom, string progressivePackName)
        {
            return _progressiveConfigurationProvider.ViewProgressiveLevels(gameId, denom, progressivePackName);
        }

        public IReadOnlyCollection<IViewableProgressiveLevel> AssignLevelsToGame(IReadOnlyCollection<ProgressiveLevelAssignment> levelAssignments, string protocolName)
        {
            if (protocolName == _progressiveProtocol)
            {
                return _progressiveConfigurationProvider.AssignLevelsToGame(levelAssignments);
            }

            return null;
        }

        public IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels()
        {
            return _progressiveConfigurationProvider.ViewConfiguredProgressiveLevels();
        }

        public IEnumerable<IViewableProgressiveLevel> ViewConfiguredProgressiveLevels(int gameId, long denom)
        {
            return _progressiveConfigurationProvider.ViewConfiguredProgressiveLevels(gameId, denom);
        }

        public IEnumerable<IViewableProgressiveLevel> GetActiveProgressiveLevels()
        {
            return _progressiveGameProvider.GetActiveProgressiveLevels();
        }

        public void ClaimAndAwardLinkedProgressiveLevel(string protocolName, string levelName, long winAmount = -1)
        {
            if (protocolName == _progressiveProtocol)
            {
                _linkedProgressiveProvider.ClaimAndAwardLinkedProgressiveLevel(levelName, winAmount);
            }
        }

        public string Name => nameof(ProtocolLinkedProgressiveAdapter);

        public ICollection<Type> ServiceTypes => new[] { typeof(IProtocolLinkedProgressiveAdapter) };

        public void Initialize()
        {
            var protocol = _multiProtocolConfigurationProvider.MultiProtocolConfiguration
                .FirstOrDefault(x => x.IsProgressiveHandled)?.Protocol;

            _progressiveProtocol = protocol.HasValue ? EnumParser.ToName(protocol) : null;
        }
    }
}
