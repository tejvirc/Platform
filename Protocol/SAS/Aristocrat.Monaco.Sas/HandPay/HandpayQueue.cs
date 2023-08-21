namespace Aristocrat.Monaco.Sas.HandPay
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Hardware.Contracts.Persistence;
    using Protocol.Common.Storage.Entity;
    using Storage;
    using Storage.Models;

    /// <summary>
    ///     Creates the handpay queue used for secure handpay handling for a given SAS client
    /// </summary>
    public class HandpayQueue : IDisposable, IHandpayQueue
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISasHandPayCommittedHandler _handPayCommittedHandler;
        private readonly int _queueSize;

        private readonly ConcurrentQueue<HandpayDataResponse> _handpayData;
        private readonly byte _clientId;
        private readonly IHostAcknowledgementHandler _handlers;
        private bool _pendingHandpayRead;
        private bool _disposed;

        /// <summary>
        ///     Creates the handpay queue
        /// </summary>
        /// <param name="unitOfWorkFactory">An instance of <see cref="IPersistentStorageManager"/></param>
        /// <param name="handPayCommittedHandler">An instance of <see cref="ISasHandPayCommittedHandler"/></param>
        /// <param name="queueSize">The max queue size to use</param>
        /// <param name="clientId">The client number for this handpay queue</param>
        public HandpayQueue(
            IUnitOfWorkFactory unitOfWorkFactory,
            ISasHandPayCommittedHandler handPayCommittedHandler,
            int queueSize,
            byte clientId)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _handPayCommittedHandler = handPayCommittedHandler ?? throw new ArgumentNullException(nameof(handPayCommittedHandler));
            _queueSize = queueSize;
            _clientId = clientId;

            _handPayCommittedHandler.RegisterHandpayQueue(this, clientId);
            _handlers = new HostAcknowledgementHandler
            {
                ImpliedNackHandler = null, ImpliedAckHandler = () => HandpayAcknowledged().FireAndForget()
            };

            _handpayData = new ConcurrentQueue<HandpayDataResponse>(LoadResponses());
        }

        /// <inheritdoc />
        public int Count => _handpayData.Count;

        /// <inheritdoc />
        public event HandpayAcknowledged OnAcknowledged;

        /// <inheritdoc />
        public Task Enqueue(LongPollHandpayDataResponse data)
        {
            if (_handpayData.Count == _queueSize)
            {
                _handpayData.TryDequeue(out _);
            }

            _handpayData.Enqueue((HandpayDataResponse)data);
            return Persist();
        }

        /// <inheritdoc />
        public LongPollHandpayDataResponse Peek()
        {
            return _handpayData.TryPeek(out var data) ? (LongPollHandpayDataResponse)data : null;
        }

        /// <inheritdoc />
        public LongPollHandpayDataResponse GetNextHandpayData()
        {
            if (!_handpayData.TryPeek(out var data))
            {
                return null;
            }

            _pendingHandpayRead = true;
            var response = (LongPollHandpayDataResponse)data;
            response.Handlers = _handlers;
            return response;
        }

        /// <inheritdoc />
        public Task HandpayAcknowledged()
        {
            if (!_pendingHandpayRead)
            {
                return Task.CompletedTask;
            }

            _pendingHandpayRead = false;
            var longPollHandpayDataResponse = Dequeue();
            if (longPollHandpayDataResponse is null)
            {
                return Task.CompletedTask;
            }

            OnAcknowledged?.Invoke(_clientId, longPollHandpayDataResponse.TransactionId);
            return Persist();
        }

        /// <inheritdoc />
        public void ClearPendingHandpay()
        {
            _pendingHandpayRead = false;
        }

        /// <inheritdoc />
        public IEnumerator<LongPollHandpayDataResponse> GetEnumerator()
        {
            return _handpayData.Select(x => (LongPollHandpayDataResponse)x).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of resources
        /// </summary>
        /// <param name="disposing">True if disposing the first time</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _handPayCommittedHandler.UnRegisterHandpayQueue(this, _clientId);
            }

            _disposed = true;
        }

        private LongPollHandpayDataResponse Dequeue()
        {
            return !_handpayData.TryDequeue(out var data) ? null : (LongPollHandpayDataResponse)data;
        }

        private IEnumerable<HandpayDataResponse> LoadResponses()
        {
            var handpayData = _unitOfWorkFactory.Invoke(
                x => x.Repository<HandpayReportData>().Queryable().FirstOrDefault(h => h.ClientId == _clientId))?.Queue;
            return StorageHelpers.Deserialize(handpayData, () => new List<HandpayDataResponse>());
        }

        private Task Persist()
        {
            return Task.Run(
                () =>
                {
                    var data = StorageHelpers.Serialize(_handpayData.ToList());
                    using var work = _unitOfWorkFactory.Create();
                    work.BeginTransaction(IsolationLevel.Serializable);
                    var repository = work.Repository<HandpayReportData>();
                    var handpayReportData = repository.Queryable().FirstOrDefault(x => x.ClientId == _clientId) ??
                                            new HandpayReportData { ClientId = _clientId };
                    handpayReportData.Queue = data;
                    repository.AddOrUpdate(handpayReportData);
                    work.Commit();
                });
        }
    }
}