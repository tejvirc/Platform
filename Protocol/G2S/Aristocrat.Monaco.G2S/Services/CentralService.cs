namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Handlers.Central;
    using Kernel;
    using log4net;

    public class CentralService : ICentralHandler, ICentralService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICentralProvider _centralProvider;
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _games;
        private readonly IEventBus _bus;

        private ActionBlock<CentralTransaction> _messageProcessor;
        private CancellationTokenSource _cancelProcessing;
        private volatile bool _canSend;

        private bool _disposed;

        public CentralService(ICentralProvider centralProvider, IG2SEgm egm, IGameProvider games, IEventBus bus)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await GetOutcomesAsync(transaction);
        }

        public void Start()
        {
            var device = _egm.GetDevice<ICentralDevice>();

            if (device == null)
            {
                return;
            }

            _bus.Subscribe<CommunicationsStateChangedEvent>(
                this,
                evt =>
                {
                    if (device.IsOwner(evt.HostId))
                    {
                        _canSend = evt.Online;
                    }
                });

            _bus.Subscribe<OutcomeReceivedEvent>(this,
                evt =>
                {
                    _messageProcessor.Post(evt.Transaction);
                });

            _centralProvider.Register(this, ProtocolNames.G2S);

            _cancelProcessing = new CancellationTokenSource();

            _messageProcessor = new ActionBlock<CentralTransaction>(
                async request =>
                {
                    while (!_canSend && !_cancelProcessing.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), _cancelProcessing.Token);
                    }

                    _cancelProcessing.Token.ThrowIfCancellationRequested();

                    while (!_cancelProcessing.IsCancellationRequested && !await TryAcknowledge(request))
                    {
                        await Task.Delay(device.Queue.SessionTimeout, _cancelProcessing.Token);
                    }
                });

            try
            {
                var history = _centralProvider.Transactions
                    .Where(t => t.OutcomeState == OutcomeState.Committed)
                    .OrderBy(h => h.TransactionId).ToList();
                foreach (var log in history)
                {
                    _messageProcessor.Post(log);
                }
            }
            catch (TransactionHistoryException)
            {
                // Disable the device if there is no provider
                device.Enabled = false;

                _centralProvider.Clear(ProtocolNames.G2S);
            }
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
                _bus.UnsubscribeAll(this);
                _centralProvider.Clear(ProtocolNames.G2S);

                if (_cancelProcessing != null)
                {
                    _cancelProcessing.Cancel(false);
                    _messageProcessor.Complete();
                    _messageProcessor.Completion.Wait();

                    _cancelProcessing.Dispose();
                }
            }

            _cancelProcessing = null;
            _messageProcessor = null;

            _disposed = true;
        }

        private async Task<bool> TryAcknowledge(CentralTransaction transaction)
        {
            var device = _egm.GetDevice<ICentralDevice>(transaction.DeviceId);
            if (device == null)
            {
                return false;
            }

            var commit = new commitOutcome
            {
                transactionId = transaction.TransactionId, outcomeException = (int)transaction.Exception
            };

            if (!await device.CommitOutcome(commit))
            {
                Logger.Debug($"Failed to commit outcome [{transaction.TransactionId}]");

                return false;
            }

            _centralProvider.AcknowledgeOutcome(transaction.TransactionId);

            Logger.Debug($"Outcome committed [{transaction.TransactionId}]");

            return true;
        }

        private async Task GetOutcomesAsync(CentralTransaction transaction)
        {
            var device = _egm.GetDevice<ICentralDevice>(transaction.DeviceId);

            var game = _games.GetGame(transaction.GameId);

            var request = new getCentralOutcome
            {
                transactionId = transaction.TransactionId,
                gamePlayId = transaction.GameId,
                themeId = game.ThemeId,
                paytableId = game.PaytableId,
                denomId = transaction.Denomination,
                wagerCategory = transaction.WagerCategory,
                wagerAmt = transaction.WagerAmount,
                outcomeQty = transaction.OutcomesRequested
            };

            var response = await device.GetOutcome(request);
            if (response == null)
            {
                _centralProvider.OutcomeResponse(
                    transaction.TransactionId,
                    Enumerable.Empty<Outcome>(),
                    OutcomeException.TimedOut);
            }
            else
            {
                _centralProvider.OutcomeResponse(
                    transaction.TransactionId,
                    response.outcome.Select(outcome => outcome.ToOutcome()));
            }
        }
    }
}