namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Gaming.Contracts.Progressives;
    using log4net;

    public class ProgressiveHandler : IProgressiveHandler, IProgressiveUpdateHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        private bool _disposed;

        private readonly Dictionary<int, int> _tempProgressiveIdMapping = new()
        {
            { 0, 10001 },
            { 1, 10002 },
            { 2, 10003 },
        };

        public ProgressiveHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
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
                // TODO broadcast level ids are 10001, 10002, 10003, but progressive system is using 0, 1, 2. How will this really work?
                var mappedLevelId = _tempProgressiveIdMapping[progressiveLevel.LevelId];
                if (mappedLevelId == update.ProgressiveLevel)
                {
                    var linkedLevel = new LinkedProgressiveLevel()
                    {
                        ProtocolName = ProtocolNames.Bingo,
                        ProgressiveGroupId = progressiveLevel.ProgressivePackId,
                        LevelId = progressiveLevel.LevelId,
                        Amount = update.Amount
                    };

                    Logger.Debug($"UpdateLinkedProgressiveLevels ProgressiveGroupId={linkedLevel.ProgressiveGroupId}, LevelId={linkedLevel.LevelId}, Amount={linkedLevel.Amount}");

                    _protocolLinkedProgressiveAdapter.UpdateLinkedProgressiveLevels(
                        new[] { linkedLevel },
                        ProtocolNames.Bingo);

                    break;
                }
            }

            return Task.FromResult(true);
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