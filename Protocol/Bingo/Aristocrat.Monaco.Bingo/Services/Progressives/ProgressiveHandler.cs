namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Gaming.Contracts.Progressives;
    using log4net;

    public class ProgressiveHandler : IProgressiveInfoHandler, IProgressiveUpdateHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        private bool _disposed;

        private readonly Dictionary<int, long> _progressiveIdMapping = new();

        public ProgressiveHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        public Task<bool> ProcessProgressiveInfo(ProgressiveInfoMessage info, CancellationToken token)
        {
            Logger.Debug("Received a progressive information message");
            Logger.Debug($"ResponseCode={info.ResponseCode}, Accepted={info.Accepted}, GameTitle={info.GameTitleId}, AuthToken={info.AuthenticationToken}");

            _progressiveIdMapping.Clear();
            Logger.Debug("Progressive Levels:");
            foreach (var progLevel in info.ProgressiveLevels)
            {
                Logger.Debug($"SequenceNumber={progLevel.SequenceNumber}, ProgressiveLevel={progLevel.ProgressiveLevel}");
                _progressiveIdMapping.Add(progLevel.SequenceNumber - 1, progLevel.ProgressiveLevel);
            }

            Logger.Debug("Meters To Report:");
            foreach (var meter in info.MetersToReport)
            {
                Logger.Debug($"Meter={meter}");
            }

            
            return Task.FromResult(info.ResponseCode == ResponseCode.Ok);
        }

        public Task<bool> ProcessProgressiveUpdate(ProgressiveUpdateMessage update, CancellationToken token)
        {
            Logger.Debug($"Received a progressive update message, ProgLevel={update.ProgressiveLevel}, Amount={update.Amount}");

            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            token.ThrowIfCancellationRequested();

            var progressiveLevels = _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels();
            foreach (var progressiveLevel in progressiveLevels)
            {
                if (_progressiveIdMapping.ContainsKey(progressiveLevel.LevelId))
                {
                    var mappedLevelId = _progressiveIdMapping[progressiveLevel.LevelId];
                    if (mappedLevelId == update.ProgressiveLevel)
                    {
                        Logger.Debug($"Found mapping of levelId = {progressiveLevel.LevelId} to progressive level = {mappedLevelId}");

                        var linkedLevel = new LinkedProgressiveLevel()
                        {
                            ProtocolName = ProtocolNames.Bingo,
                            ProgressiveGroupId = progressiveLevel.ProgressivePackId,
                            LevelId = progressiveLevel.LevelId,
                            Amount = update.Amount
                        };

                        Logger.Debug(
                            $"UpdateLinkedProgressiveLevels ProgressiveGroupId={linkedLevel.ProgressiveGroupId}, LevelId={linkedLevel.LevelId}, Amount={linkedLevel.Amount}");

                        _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                            new[] { linkedLevel },
                            ProtocolNames.Bingo);

                        return Task.FromResult(update.ResponseCode == ResponseCode.Ok);
                    }
                }
            }

            Logger.Info($"Ignoring progressive update with unknown progressive level {update.ProgressiveLevel}");
            return Task.FromResult(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }
    }
}