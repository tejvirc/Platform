namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;

    public class BingoClientInterceptor : Interceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAuthorizationProvider _authorizationProvider;

        public BingoClientInterceptor(IAuthorizationProvider authorizationProvider)
        {
            _authorizationProvider =
                authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
        }

        public event EventHandler<EventArgs> MessageReceived;

        public event EventHandler<EventArgs> AuthorizationFailed;

        public TimeSpan MessageTimeoutMs { get; set; } = TimeSpan.FromMilliseconds(3000);

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            Logger.Debug($"Sending Request: {request}");
            context = AddTimeout(AddAuthorization(context));
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                LogResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                Logger.Debug($"Sending Request {request}");
                context = AddTimeout(AddAuthorization(context));
                var response = base.BlockingUnaryCall(request, context, continuation);
                Logger.Debug($"Response Received: {response}");
                OnMessageReceived();
                return response;
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Authorization failed", rpcException);
                OnRpcException(rpcException);
                throw;
            }
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new BingoClientClientStreamingLogger<TRequest>(Logger, call.RequestStream),
                new BingoClientServerStreamingLogger<TResponse>(
                    Logger,
                    call.ResponseStream,
                    OnMessageReceived,
                    OnRpcException),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            Logger.Debug($"Sending Request {request}");
            context = AddAuthorization(context);
            var call = continuation(request, context);
            return new AsyncServerStreamingCall<TResponse>(
                new BingoClientServerStreamingLogger<TResponse>(
                    Logger,
                    call.ResponseStream,
                    OnMessageReceived,
                    OnRpcException),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncClientStreamingCall<TRequest, TResponse>(
                new BingoClientClientStreamingLogger<TRequest>(Logger, call.RequestStream),
                LogResponse(call.ResponseAsync),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        private void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        private void OnAuthorizationFailed()
        {
            AuthorizationFailed?.Invoke(this, EventArgs.Empty);
        }

        private async Task<TResponse> LogResponse<TResponse>(Task<TResponse> callingTask)
        {
            try
            {
                var response = await callingTask.ConfigureAwait(false);
                Logger.Debug($"Response Received: {response}");
                OnMessageReceived();
                return response;
            }
            catch (RpcException rpcException)
            {
                Logger.Error("Authorization failed", rpcException);
                OnRpcException(rpcException);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occurred trying to get a response", ex);
                throw;
            }
        }

        private ClientInterceptorContext<TRequest, TResponse> AddAuthorization<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var metadata = _authorizationProvider.AuthorizationData;
            if (metadata is null)
            {
                return context;
            }

            return new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(metadata));
        }

        private ClientInterceptorContext<TRequest, TResponse> AddTimeout<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            return new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithDeadline(DateTime.UtcNow.Add(MessageTimeoutMs)));
        }

        private void OnRpcException(RpcException rpcException)
        {
            if (_authorizationProvider.AuthorizationData is null ||
                rpcException.StatusCode is not (StatusCode.Unauthenticated or StatusCode.PermissionDenied))
            {
                return;
            }
            
            OnAuthorizationFailed();
        }
    }
}