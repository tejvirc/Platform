namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Gaming.Contracts.Central;
    using Hardware.Contracts;
    using Kernel;
    using Newtonsoft.Json;

    public class CentralHandler : ICentralHandler, IDisposable
    {
        private const string OutcomesFile = @"outcomes.json";

        private readonly ITransactionHistory _transactions;
        private readonly ICentralProvider _centralProvider;
        private readonly IPathMapper _pathMapper;

        private OutcomeInfo _outcomeInfo;
        private int _outcomeIndex;

        private bool _disposed;

        public CentralHandler()
            : this(
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<ICentralProvider>(),
                ServiceManager.GetInstance().GetService<IPathMapper>())
        {
        }

        public CentralHandler(
            ITransactionHistory transactions,
            ICentralProvider centralProvider,
            IPathMapper pathMapper)
        {
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));

            Initialize();
        }

        public async Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await GetOutcomesAsync(transaction);
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
                _centralProvider.Clear(ProtocolNames.Test);
            }

            _disposed = true;
        }

        private void Initialize()
        {
            try
            {
                if (!_centralProvider.Register(this, ProtocolNames.Test))
                {
                    return;
                }

                var transactions = _transactions.RecallTransactions<CentralTransaction>()
                    .Where(t => t.OutcomeState == OutcomeState.Committed || t.OutcomeState == OutcomeState.Failed)
                    .OrderBy(h => h.TransactionId).ToList();

                foreach (var transaction in transactions)
                {
                    _centralProvider.AcknowledgeOutcome(transaction.TransactionId);
                }
            }
            catch (TransactionHistoryException)
            {
                return;
            }

            TryGetOutcomes();
        }

        private async Task GetOutcomesAsync(CentralTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var outcomes = new List<Outcome>(transaction.OutcomesRequested);

            if (TryGetOutcomes())
            {
                for (var request = 0; request < transaction.OutcomesRequested; request++)
                {
                    if (_outcomeIndex == _outcomeInfo.Outcomes.Count() - 1)
                    {
                        _outcomeIndex = 0;
                    }

                    var value = _outcomeInfo.Outcomes.ElementAt(_outcomeIndex);

                    outcomes.Add(
                        new Outcome(
                            request,
                            1,
                            1,
                            OutcomeReference.Direct,
                            OutcomeType.Standard,
                            value.DollarsToMillicents(),
                            0,
                            string.Empty));

                    _outcomeIndex++;
                }
            }
            else
            {
                outcomes.AddRange(
                    Enumerable.Repeat(
                        new Outcome(
                            DateTime.UtcNow.Ticks,
                            1,
                            1,
                            OutcomeReference.Direct,
                            OutcomeType.Standard,
                            5m.DollarsToMillicents(),
                            0,
                            string.Empty),
                        transaction.OutcomesRequested));
            }

            // Artificial delay to simulate server interaction
            await Task.Delay(TimeSpan.FromMilliseconds(30));

            _centralProvider.OutcomeResponse(transaction.TransactionId, outcomes);
        }

        private bool TryGetOutcomes()
        {
            if (_outcomeInfo != null && _outcomeInfo.Outcomes.Any())
            {
                return true;
            }

            var dataPath = _pathMapper.GetDirectory(HardwareConstants.DataPath);
            var outcomeFile = Path.Combine(dataPath.FullName, OutcomesFile);
            if (!File.Exists(outcomeFile))
            {
                return false;
            }

            _outcomeInfo = JsonConvert.DeserializeObject<OutcomeInfo>(File.ReadAllText(outcomeFile));
            _outcomeIndex = 0;

            return true;
        }

        private class OutcomeInfo
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local - Used for JSON deserialization
            public IEnumerable<decimal> Outcomes { get; set; }
        }
    }
}