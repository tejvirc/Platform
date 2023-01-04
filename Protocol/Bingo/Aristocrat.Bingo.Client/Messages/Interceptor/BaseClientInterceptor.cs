namespace Aristocrat.Bingo.Client.Messages.Interceptor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Grpc.Core;
    using Grpc.Core.Interceptors;
    using log4net;

    public abstract class BaseClientInterceptor : Interceptor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAuthorizationProvider _authorizationProvider;

        protected BaseClientInterceptor(IAuthorizationProvider authorizationProvider)
        {
            _authorizationProvider = authorizationProvider ?? throw new ArgumentNullException(nameof(authorizationProvider));
        }

        public EventHandler<EventArgs> MessageReceived { get; set; }

        public int MessageTimeoutMs { get; set; } = 30000;

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
            return response;
        }

        public abstract override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation);

        public abstract override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation);

        public abstract override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation);

        public void OnMessageReceived()
        {
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }

        protected static async Task<TResponse> LogResponse<TResponse>(Task<TResponse> callingTask)
        {
            try
            {
                var response = await callingTask;
                Logger.Debug($"Response Received: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occurred trying to get a response", ex);
                throw;
            }
        }

        protected ClientInterceptorContext<TRequest, TResponse> AddAuthorization<TRequest, TResponse>(
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

        protected ClientInterceptorContext<TRequest, TResponse> AddTimeout<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            return new (
                context.Method,
                context.Host,
                context.Options.WithDeadline(DateTime.UtcNow.AddMilliseconds(MessageTimeoutMs)));
        }
    }
}