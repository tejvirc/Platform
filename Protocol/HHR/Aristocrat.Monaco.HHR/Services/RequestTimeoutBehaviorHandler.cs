namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper.Internal;
    using Client.Extensions;
    using Client.Messages;
    using Client.Utility;
    using Client.WorkFlow;
    using Events;
    using Hardware.Contracts.Button;
    using Kernel;
    using log4net;
    using Protocol.Common.Logging;
    using SimpleInjector;
    using Storage.Helpers;

    /// <summary>
    ///     Handles what should happen on request timeout and calls appropriate behavior of RequestTimeout defined within the
    ///     request.
    ///     This will attempt to send these messages, until success.
    ///     This will retry only Failed requests, requests which are pending will not be attempted.
    /// </summary>
    public class RequestTimeoutBehaviorHandler : IDisposable, IRequestTimeoutBehaviorService
    {
        private readonly ICentralManager _centralManager;
        private readonly Container _container;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<(Type, Type), MethodInfo> _sendMethods = new Dictionary<(Type, Type), MethodInfo>();
        private readonly Dictionary<TimeoutBehaviorType, object> _timeoutBehaviors = new Dictionary<TimeoutBehaviorType, object>();
        private readonly IPendingRequestEntityHelper _entityHelper;
        private readonly IEventBus _eventBus;
        private readonly ConcurrentDictionary<Request, Type> _requestsPending =
            new ConcurrentDictionary<Request, Type>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private bool _disposed;
        private readonly SemaphoreSlim _playAllowed = new SemaphoreSlim(1, 1);
        private readonly object _requestLock = new object();

        public RequestTimeoutBehaviorHandler(
            ICentralManager centralManager,
            IServiceProvider serviceProvider,
            IPendingRequestEntityHelper entityHelper,
            IEventBus eventBus)
        {
            _centralManager = centralManager ?? throw new ArgumentNullException(nameof(centralManager));
            _container = serviceProvider as Container ?? throw new ArgumentNullException(nameof(serviceProvider));
            _entityHelper = entityHelper ?? throw new ArgumentNullException(nameof(entityHelper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            LoadAllTimeoutBehaviors();

            foreach(var item in _entityHelper.PendingRequests)
            {
                _requestsPending[item.Key] = item.Value;
            }

            _eventBus.Subscribe<ProtocolInitializationComplete>(this, _ =>
            {
                _requestsPending.ForAll(x => x.Key.IsFailed = true);
                _entityHelper.PendingRequests = _requestsPending.ToArray();
                OnSend();
            });
            _eventBus.Subscribe<DownEvent>(this, _ => ClearFailedRequests(), evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            _eventBus.Subscribe<UnexpectedOrNoResponseEvent>(this, _ =>
                {
                    _logger.Debug("Got unexpected response. Release game play request");
                    ReleasePlayAllowed();
                });
            _disposables.Add(
                _centralManager.RequestObservable.Subscribe(
                    OnRequestSent,
                    error => { _logger.Warn("Unable to subscribe to Sent requests.", error); }));

            _disposables.Add(
                _centralManager.RequestResponseObservable.Subscribe(
                    OnResponseReceived,
                    error => { _logger.Warn("Unable to subscribe to Sent requests.", error); }));
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
                _container?.Dispose();
                _eventBus.UnsubscribeAll(this);
                _disposables.ForEach(x => x.Dispose());
                _playAllowed.Dispose();
            }

            _disposed = true;
        }

        private void OnRequestSent((Request request, Type responseType) obj)
        {
            var (request, responseType) = obj;

            if (request.RequestTimeout.TimeoutBehaviorType == TimeoutBehaviorType.Idle)
            {
                return;
            }

            lock (_requestLock)
            {
                request.IsFailed = false;

                _logger.Debug($"Marking request as pending - {request} - ResponseType - {responseType}");

                var waitRequired = !_requestsPending.Any();

                _requestsPending[obj.request] = obj.responseType;

                _entityHelper.PendingRequests = _requestsPending.ToArray();

                if (!waitRequired)
                {
                    return;
                }
            }

            _logger.Debug("Got pending request. Game play not allowed");

            _playAllowed.Wait();
        }

        private void OnResponseReceived((Request request, Response response) obj)
        {
            var (request, response) = obj;

            _logger.Debug($"[RECV] Response ({response}) received for request ({request})");

            if (request.RequestTimeout.TimeoutBehaviorType == TimeoutBehaviorType.Idle)
            {
                return;
            }

            request.IsFailed = response.MessageStatus != MessageStatus.Success;

            if (request.IsFailed)
            {
                _logger.Error($"[RECV] Marking request ({request}) as failed");
            }

            UpdatePendingRequest((request, response.GetType()), !request.IsFailed);
            _entityHelper.PendingRequests = _requestsPending.ToArray();
        }

        private void UpdatePendingRequest((Request request, Type responseType) requestResponse, bool removeRequest)
        {
            var timeoutBehavior =
                _timeoutBehaviors[requestResponse.request.RequestTimeout.TimeoutBehaviorType] as dynamic;

            if (removeRequest)
            {
                if (_requestsPending.TryRemove(requestResponse.request, out _) &&
                    IsExitAllowed(requestResponse.request))
                {
                    timeoutBehavior.OnExit(requestResponse.request.RequestTimeout as dynamic);
                }
                else
                {
                    _logger.Warn("[RECV] Unable to remove request from pending requests. This request is not pending.");
                }

                if (_requestsPending.All(x => x.Key.Command != Command.CmdTransaction))
                {
                    _logger.Debug("[RECV] No pending requests. Release game play requests.");

                    ReleasePlayAllowed();

                    _eventBus.Publish(new PendingRequestRemovedEvent());
                }
            }
            else if (_requestsPending.Any(x => ReferenceEquals(x.Key, requestResponse.request)))
            {
                // The request failed, enter timeout behavior
                timeoutBehavior.OnEntry(requestResponse.request.RequestTimeout as dynamic);
            }
        }

        private bool IsExitAllowed(Request request)
        {
            switch (request.RequestTimeout)
            {
                case LockupRequestTimeout lockupRequestTimeout:
                    // Call exit timeout behavior if there no more lockup timeout behavior with same exit lockup key.
                    return !_requestsPending.Any(
                        x => x.Key.IsFailed && x.Key.RequestTimeout is LockupRequestTimeout lrt &&
                             lrt.LockupKey == lockupRequestTimeout.LockupKey);
            }

            return true;
        }

        private void LoadAllTimeoutBehaviors()
        {
            var timeouts = AssemblyUtilities.LoadAllTypesImplementing<IRequestTimeout>();

            timeouts.ForAll(
                x =>
                {
                    var instance = _container.GetGenericInstanceOfType(typeof(IRequestTimeoutBehavior<>), x.GetType());
                    if (instance == null)
                    {
                        throw new InvalidDataException($"TimeoutBehaviorType for {x.GetType()} not found.");
                    }

                    _timeoutBehaviors.Add(x.TimeoutBehaviorType, instance);
                });
        }

        private void OnSend()
        {
            var requestsToRetry = _requestsPending.Where(tuple => tuple.Key.IsFailed).ToArray();
            requestsToRetry.ForAll(
                async x =>
                {
                    try
                    {
                        _logger.Debug($"Resending failed request ({x.Key})");
                        // Since this is a failed request, call OnEntry on all the requests.
                        UpdatePendingRequest((x.Key, x.Value), false);
                        await Send((x.Key, x.Value));
                    }
                    catch (Exception exception)
                    {
                        _logger.Error("Failed to send request to CentralServer.", exception);
                    }
                });
        }

        private async Task Send((Request request, Type responseType) request)
        {
            _logger.Debug($"Attempting to send Failed Request to server - {request.ToJson2()}");
            var requestResponsePair = (request.request.GetType(), request.responseType);

            if (!_sendMethods.TryGetValue(requestResponsePair, out var send))
            {
                send = _sendMethods[requestResponsePair] = typeof(ICentralManager).GetMethods()
                    .Single(x => x.Name == "Send" && x.GetGenericArguments().Length == 2)
                    .MakeGenericMethod(request.request.GetType(), request.responseType);
            }

            var task =
                (Task)send?.Invoke(_centralManager, new object[] { request.request, CancellationToken.None }) ??
                Task.CompletedTask;

            await task.ConfigureAwait(false);
        }

        private void ClearFailedRequests()
        {
            _logger.Debug($"Pending requests : {_requestsPending}, Failed requests : {_requestsPending.Where(tuple => tuple.Key.IsFailed).ToArray().Length}");

            _requestsPending.ForAll(
                x =>
                {
                    if (x.Key.RequestTimeout is LockupRequestTimeout || x.Key.IsFailed)
                    {
                        _logger.Debug($"Clear request timeout for ({x.Key})");
                        UpdatePendingRequest((x.Key, x.Value), true);
                    }
                });

            _entityHelper.PendingRequests = _requestsPending.ToArray();
        }

        public async Task<bool> CanPlay()
        {
            try
            {
                _logger.Debug("Wait for play");
                await _playAllowed.WaitAsync(HhrConstants.MsgTransactionTimeoutMs * HhrConstants.RetryCount);
            }
            finally
            {
                ReleasePlayAllowed();
            }

            return !_requestsPending.Any();
        }

        private void ReleasePlayAllowed()
        {
            try
            {
                _playAllowed.Release();
            }
            catch(SemaphoreFullException ex)
            {
                _logger.Debug("Semaphore max count exceeded", ex);
            }
            catch(Exception ex)
            {
                _logger.Debug("Exception : ", ex);
            }
        }
    }
}