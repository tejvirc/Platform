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

        public TimeSpan MessageTimeoutMs { get; set; } = TimeSpan.FromMilliseconds(3000);

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
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
            Logger.Debug($"Sending Request {request}");
            context = AddTimeout(AddAuthorization(context));
            var response = base.BlockingUnaryCall(request, context, continuation);
            Logger.Debug($"Response Received: {response}");
            OnMessageReceived();
            return response;
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            context = AddAuthorization(context);
            var call = continuation(context);
            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                new BingoClientClientStreamingLogger<TRequest>(Logger, call.RequestStream),
                new BingoClientServerStreamingLogger<TResponse>(Logger, call.ResponseStream, OnMessageReceived),
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
            Logger.Debug($"Sending Request {request}");
            context = AddAuthorization(context);
            var call = continuation(request, context);
            return new AsyncServerStreamingCall<TResponse>(
                new BingoClientServerStreamingLogger<TResponse>(Logger, call.ResponseStream, OnMessageReceived),
                call.ResponseHeadersAsync,
                call.GetStatus,
                call.GetTrailers,
                call.Dispose);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
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

        private async Task<TResponse> LogResponse<TResponse>(Task<TResponse> callingTask)
        {
            try
            {
                var response = await callingTask.ConfigureAwait(false);
                Logger.Debug($"Response Received: {response}");
                OnMessageReceived();
                return response;
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
    }
}